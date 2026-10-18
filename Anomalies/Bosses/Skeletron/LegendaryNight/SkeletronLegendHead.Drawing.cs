using Anomalies.Bosses.Skeletron.LegendaryNight.Entities;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class SkeletronLegendHead
{
    private const int StarAvatarTrailLength = 50;

    [LoadTexture(LegendaryNightHandler.LegendaryNightEntityPath + "SlimeAvatar")]
    private static Asset<Texture2D> _SlimeAvatarTexture;
    public static Texture2D SlimeAvatarTexture => _SlimeAvatarTexture.Value;

    public enum Avatar : byte
    {
        None,
        FlyingSlime,
        Star,
        PlatinumCoin,
    }

    // AnomalyAI32[1].byte0：当前化身。AnomalyAI32[2].i：当前化身持续帧数。
    public Avatar CurrentAvatar
    {
        get => (Avatar)AnomalyNPC.AnomalyAI32[1].byte0;
        private set
        {
            if (AnomalyNPC.AnomalyAI32[1].byte0 != (byte)value)
            {
                AnomalyNPC.AnomalyAI32[1].byte0 = (byte)value;
                AnomalyNPC.AIChanged32[1] = true;
            }
        }
    }

    public int AvatarTimer
    {
        get => AnomalyNPC.AnomalyAI32[2].i;
        private set
        {
            if (AnomalyNPC.AnomalyAI32[2].i != value)
            {
                AnomalyNPC.AnomalyAI32[2].i = value;
                AnomalyNPC.AIChanged32[2] = true;
            }
        }
    }

    // AnomalyAI32[3].f：化身独立旋转角，与 NPC.rotation 无关。
    public float AvatarRotation
    {
        get => AnomalyNPC.AnomalyAI32[3].f;
        private set
        {
            if (AnomalyNPC.AnomalyAI32[3].f != value)
            {
                AnomalyNPC.AnomalyAI32[3].f = value;
                AnomalyNPC.AIChanged32[3] = true;
            }
        }
    }

    public bool StarAvatarActive => CurrentAvatar == Avatar.Star;

    private Avatar RequestedAvatar => CurrentPhase == Phase.Phase1 && CurrentBaseBehavior == BaseBehavior.Rotate && CurrentLocalPhase == 1
        ? CurrentSpell switch
        {
            Spell.Slime => Avatar.FlyingSlime,
            Spell.Star => Avatar.Star,
            Spell.PlatinumCoin => Avatar.PlatinumCoin,
            _ => Avatar.None
        }
        : Avatar.None;

    private float AvatarFadeIn => MathHelper.Clamp(AvatarTimer / 10f, 0f, 1f);
    private float AvatarOpacity => CurrentPhase == Phase.PhaseChange_1To2
        ? 1f - MathHelper.Clamp(Timer1 / (float)PhaseChangeAvatarFadeDuration, 0f, 1f)
        : 1f;

    public override void PostSetupContent()
    {
        NPCID.Sets.MustAlwaysDraw[NPCID.SkeletronHead] = true;
        NPCID.Sets.MustAlwaysDraw[NPCID.SkeletronHand] = true;
        NPCID.Sets.TrailCacheLength[NPCID.SkeletronHead] = StarAvatarTrailLength;
        // 模式 1 每帧记录位置，模式 0 每两帧记录，无法对应 50 帧的时间窗口。
        NPCID.Sets.TrailingMode[NPCID.SkeletronHead] = 1;
    }

    private void UpdateAvatar(Avatar avatar)
    {
        if (CurrentAvatar != avatar)
        {
            if (CurrentPhase != Phase.PhaseChange_1To2)
                SpawnAvatarDisappearanceParticles(CurrentAvatar);
            CurrentAvatar = avatar;
            AvatarTimer = 0;
            NPC.netUpdate = true;
        }

        if (CurrentAvatar != Avatar.None)
        {
            AvatarTimer++;
            if (CurrentAvatar is Avatar.Star or Avatar.PlatinumCoin)
            {
                // 两张贴图的上方均为 -Y，补偿 π/2，使上方持续指向目标。
                Vector2 direction = NPC.GetVelocityTowards(Target);
                AvatarRotation = direction == Vector2.Zero ? 0f : direction.ToRotation() + MathHelper.PiOver2;
            }
            else if (CurrentAvatar == Avatar.FlyingSlime)
                AvatarRotation = NPC.velocity.X * 0.03f;
        }
        else
            AvatarRotation = 0f;
    }

    private void SpawnAvatarDisappearanceParticles(Avatar avatar)
    {
        if (avatar == Avatar.None || Main.dedServ)
            return;

        Color avatarColor = avatar switch
        {
            Avatar.FlyingSlime => LegendaryNightHandler.SlimeColor,
            Avatar.Star => LegendaryNightHandler.StarColor,
            Avatar.PlatinumCoin => LegendaryNightHandler.PlatinumColor,
            _ => LegendaryNightHandler.BoneColor
        };
        for (int i = 0; i < 120; i++)
        {
            Vector2 offset = Main.rand.NextPolarVector2(20f, 100f * NPC.scale);
            Color color = Color.Lerp(avatarColor, LegendaryNightHandler.BoneColor, Main.rand.NextFloat());
            ParticleHandler.SpawnParticle(new OrbParticle(NPC.Center + offset, offset.ToCustomLength(Main.rand.NextFloat(3f, 14f)),
                Main.rand.Next(25, 50), Main.rand.NextFloat(0.4f, 1.2f), color, lifeEndRatio: 0.2f));
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (CurrentPhase == Phase.PhaseChange_1To2 && Timer1 >= PhaseChangeStarSpawnTime
            && Timer1 < PhaseChangeStarSpawnTime + PhaseChangeStarFallDuration)
            DrawPhaseChangeStar(spriteBatch, screenPos);
        if (Phase2GlowOpacity > 0f)
            DrawPhase2Glow(spriteBatch, screenPos);
        if (StarAvatarActive && AvatarTimer > 0)
            DrawStarAvatarTrail(spriteBatch, screenPos);
        if (AvatarTimer <= 0)
            return true;

        switch (CurrentAvatar)
        {
            case Avatar.FlyingSlime:
                DrawFlyingSlimeAvatar(spriteBatch, screenPos);
                break;
            case Avatar.Star:
                DrawStarAvatar(spriteBatch, screenPos);
                break;
            case Avatar.PlatinumCoin:
                DrawPlatinumCoinAvatar(spriteBatch, screenPos);
                break;
        }
        return true;
    }

    private float Phase2GlowOpacity => Phase2 ? 1f : CurrentPhase == Phase.PhaseChange_1To2
        ? MathHelper.Clamp((Timer1 - PhaseChangeStarSpawnTime - PhaseChangeStarFallDuration) / (float)PhaseChangeGlowFadeDuration, 0f, 1f)
        : 0f;

    private Vector2 GetPhaseChangeStarPosition(float timer)
    {
        float completion = MathHelper.Clamp((timer - PhaseChangeStarSpawnTime) / PhaseChangeStarFallDuration, 0f, 1f);
        // 二次加速下落，120 帧后准确抵达头顶；与 Boss 共用计时，无需额外同步弹幕。
        return NPC.Top + new Vector2(0f, 8f - 1600f * (1f - completion * completion));
    }

    private void DrawPhaseChangeStar(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        Texture2D bloom = ParticleHandler.GetTexture<BloomParticle>();
        int samples = Math.Min(Timer1 - PhaseChangeStarSpawnTime, StarAvatarTrailLength);
        for (int i = samples; i >= 1; i--)
        {
            float strength = 1f - i / (float)(samples + 1);
            float diameter = MathHelper.Lerp(12f, 240f, strength) * NPC.scale * 1.6f;
            float intensity = strength * strength;
            Vector2 position = GetPhaseChangeStarPosition(Timer1 - i) + NPC.netOffset - screenPos;
            Color outer = (LegendaryNightHandler.StarColor with { A = 0 }) * (intensity * 0.12f);
            Color inner = (LegendaryNightHandler.BoneColor with { A = 0 }) * (intensity * 0.18f);
            spriteBatch.DrawFromCenter(bloom, position, null, outer, 0f, diameter / bloom.Width);
            spriteBatch.DrawFromCenter(bloom, position, null, inner, 0f, diameter * 0.45f / bloom.Width);
        }

        Texture2D star = FallingStar.HardTexture;
        Vector2 origin = new(star.Width / 2f, star.Height / (1f + MathF.Cos(TOMathUtils.PiOver5)));
        float fadeIn = MathHelper.Clamp((Timer1 - PhaseChangeStarSpawnTime + 1f) / 10f, 0f, 1f);
        spriteBatch.Draw(star, GetPhaseChangeStarPosition(Timer1) + NPC.netOffset - screenPos, null,
            Color.White * fadeIn, MathHelper.Pi, origin, NPC.scale * 1.2f, SpriteEffects.None, 0f);
    }

    private void DrawPhase2Glow(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        Texture2D bloom = ParticleHandler.GetTexture<BloomParticle>();
        Vector2 position = NPC.Center + NPC.netOffset - screenPos;
        float pulse = 1f + 0.08f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f);
        float scale = NPC.scale * pulse;
        Color outer = (LegendaryNightHandler.StarColor with { A = 0 }) * (0.35f * Phase2GlowOpacity);
        Color inner = (LegendaryNightHandler.BoneColor with { A = 0 }) * (0.3f * Phase2GlowOpacity);
        spriteBatch.DrawFromCenter(bloom, position, null, outer * NPC.Opacity, 0f, 300f * scale / bloom.Width);
        spriteBatch.DrawFromCenter(bloom, position, null, inner * NPC.Opacity, 0f, 150f * scale / bloom.Width);
    }

    private void DrawStarAvatarTrail(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        Texture2D bloom = ParticleHandler.GetTexture<BloomParticle>();
        int samples = Math.Min(Math.Min(AvatarTimer, StarAvatarTrailLength), NPC.oldPos.Length);
        float sizeFactor = NPC.scale * 1.6f;
        for (int i = samples - 1; i >= 0; i--)
        {
            if (NPC.oldPos[i] == Vector2.Zero)
                continue;

            float strength = 1f - (float)i / samples;
            float diameter = MathHelper.Lerp(12f, 240f, strength) * sizeFactor;
            float intensity = strength * strength * AvatarOpacity;
            Vector2 position = NPC.oldPos[i] + NPC.Size * 0.5f - screenPos;
            Color outer = new Color(255, 195, 15, 0) * (intensity * 0.12f);
            Color inner = (LegendaryNightHandler.BoneColor with { A = 0 }) * (intensity * 0.18f);
            spriteBatch.DrawFromCenter(bloom, position, null, outer, 0f, diameter / bloom.Width);
            spriteBatch.DrawFromCenter(bloom, position, null, inner, 0f, diameter * 0.45f / bloom.Width);
        }
    }

    private void DrawStarAvatar(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        Texture2D texture = FallingStar.HardTexture;
        Vector2 origin = new(texture.Width / 2f, texture.Height / (1f + MathF.Cos(TOMathUtils.PiOver5)));
        spriteBatch.Draw(texture, NPC.Center + NPC.netOffset - screenPos, null,
            Color.White * AvatarOpacity, AvatarRotation, origin, NPC.scale * 1.2f * AvatarFadeIn, SpriteEffects.None, 0f);
    }

    private void DrawFlyingSlimeAvatar(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        Texture2D texture = SlimeAvatarTexture;
        float pulse = 1f + 0.04f * MathF.Sin(AvatarTimer * 0.15f);
        float avatarScale = NPC.scale * 1.2f * AvatarFadeIn * pulse;
        Vector2 center = NPC.Center + NPC.netOffset - screenPos;

        // 沿用史莱姆皇后的四帧振翅动画，先画翅膀再画化身，遮住翅根。
        Texture2D wingTexture = TextureAssets.Extra[ExtrasID.QueenSlimeWing].Value;
        Rectangle wingFrame = wingTexture.Frame(1, 4, 0, AvatarTimer / 6 % 4);
        for (int side = 0; side < 2; side++)
        {
            bool isRightWing = side == 1;
            Vector2 wingOrigin = wingFrame.Size() * new Vector2(isRightWing ? 0f : 1f, 0.5f);
            // 安插点位于化身上部，并随化身缩放及旋转保持相对位置。
            Vector2 attachment = new Vector2(isRightWing ? 2f : 0f, -texture.Height * 0.3f) * avatarScale;
            Vector2 wingPosition = center + attachment.RotatedBy(AvatarRotation);
            float flapRotation = MathHelper.Clamp(NPC.velocity.Y, -6f, 6f) * -0.03f;
            if (!isRightWing)
                flapRotation *= -1f;
            SpriteEffects effects = isRightWing ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(wingTexture, wingPosition, wingFrame, Color.White * AvatarOpacity, AvatarRotation + flapRotation,
                wingOrigin, avatarScale * 0.8f, effects, 0f);
        }

        spriteBatch.DrawFromCenter(texture, center, null, Color.White * AvatarOpacity, AvatarRotation, avatarScale);
    }

    private void DrawPlatinumCoinAvatar(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        Texture2D texture = TextureAssets.Coin[3].Value;
        Rectangle frame = texture.Frame(1, 8, 0, 0);
        frame.Height -= 2;
        spriteBatch.DrawFromCenter(texture, NPC.Center + NPC.netOffset - screenPos, frame,
            Color.White * AvatarOpacity, AvatarRotation, NPC.scale * 10f * AvatarFadeIn);
    }

    public override void OnKill() => UpdateAvatar(Avatar.None);
}
