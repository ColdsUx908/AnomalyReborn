namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class SkeletronLegendHead
{
    private const int StarAvatarTrailLength = 50;

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

    public override void PostSetupContent()
    {
        NPCID.Sets.TrailCacheLength[NPCID.SkeletronHead] = StarAvatarTrailLength;
        // 模式 1 每帧记录位置，模式 0 每两帧记录，无法对应 50 帧的时间窗口。
        NPCID.Sets.TrailingMode[NPCID.SkeletronHead] = 1;
    }

    private void UpdateAvatar(Avatar avatar)
    {
        if (CurrentAvatar != avatar)
        {
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
            float intensity = strength * strength;
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
            Color.White, AvatarRotation, origin, NPC.scale * 1.2f * AvatarFadeIn, SpriteEffects.None, 0f);
    }

    private void DrawFlyingSlimeAvatar(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        // TODO：飞翔史莱姆贴图完成后替换蓝色光晕占位，保留共用的渐入和消散流程。
        //Texture2D bloom = ParticleHandler.GetTexture<BloomParticle>();
        //Color color = LegendaryNightHandler.SlimeColor;
        //spriteBatch.DrawFromCenter(bloom, NPC.Center + NPC.netOffset - screenPos, null, color, 0f, NPC.scale * 0.5f * AvatarFadeIn);
    }

    private void DrawPlatinumCoinAvatar(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        Texture2D texture = TextureAssets.Coin[3].Value;
        Rectangle frame = texture.Frame(1, 8, 0, 0);
        frame.Height -= 2;
        spriteBatch.DrawFromCenter(texture, NPC.Center + NPC.netOffset - screenPos, frame,
            Color.White, AvatarRotation, NPC.scale * 10f * AvatarFadeIn);
    }

    public override void OnKill() => UpdateAvatar(Avatar.None);
}
