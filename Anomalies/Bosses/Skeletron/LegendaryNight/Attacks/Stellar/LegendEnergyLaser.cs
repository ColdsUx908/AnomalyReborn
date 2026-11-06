using Anomalies.Assets.Effects;
using Terraria.Graphics.Shaders;
using static Anomalies.Bosses.Skeletron.LegendaryNight.SkeletronLegendHead.Mechanic6Parameters;
using static Anomalies.Bosses.Skeletron.LegendaryNight.SkeletronLegendHead.Mechanic6Timeline;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 音乐驱动的恒星激光及其旋转预警。
/// </summary>
/// <remarks>
/// <para>ai[0]：主体 NPC 索引 + 1；ai[1]：锁定方向弧度；ai[2]：预警开始音乐帧。</para>
/// <para>localAI：未使用；框架 Timer1..5：未使用。</para>
/// <para>ExtraAI：TurnAngle（Single，有符号预警转角）、Punishment（Boolean）；BeamColor 为本地默认视觉色，最终颜色按音乐阶段派生。</para>
/// <para>_PunishmentHit 仅记录本机玩家在本束惩罚激光中是否已结算。</para>
/// </remarks>
public sealed class LegendEnergyLaser : AnomalyModProjectile
{
    // Radiance 的直线光束形状：半宽700，实际绘制宽1400；中心伤害宽度独立为32。
    /// <summary>光束半径，单位为世界像素。</summary>
    private const float BeamRadius = 700f;

    /// <summary>发射音效。</summary>
    public static readonly SoundStyle FiringSound = new(LegendaryNightHandler.LegendaryNightPath + "Attacks/Stellar/LegendEnergyLaserFire") { MaxInstances = 0 };

    /// <summary>本束激光及预警的视觉底色；默认混合光球金色与40%星光色，不影响伤害判定。</summary>
    public Color BeamColor { get; set; } = new(Vector3.Lerp(LegendEnergySphere.GoldColor, LegendEnergySphere.StarlightColor, 0.4f));

    /// <summary>服务器确定的有符号预警旋转角（弧度），正负分别对应顺、逆时针，范围[-π,π]。</summary>
    public float TurnAngle { get; set; }
    public bool Punishment { get; set; }
    // 蓝色恒星的冷白核心与偏蓝外沿；保留着色器已有的白核过渡。
    private Color EffectiveBeamColor => Punishment ? new Color(125, 155, 255)
        : Projectile.ai[2] >= SkeletronLegendHead.Mechanic7Timeline.FirstLaserWarningMusicTime
            ? SkeletronLegendHead.Mechanic7Parameters.LaserColor : BeamColor;
    private bool _PunishmentHit;
    private readonly bool[] _HitPlayers = new bool[Main.maxPlayers];

    public override string Texture => TOTextures.InvisibleTexturePath;
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;
    private SkeletronLegendHead Head => LegendaryNightHandler.TryGetLegendaryNightHead((int)Projectile.ai[0], out SkeletronLegendHead head, oneBased: true)
        ? head : null;
    private int Age => Head is SkeletronLegendHead head ? head.Phase2MusicElapsedTime - (int)Projectile.ai[2] : -1;
    private bool Firing => Age is >= LaserWarningDuration and < (LaserWarningDuration + LaserDuration);
    private float Angle => GetLaserRotation(Projectile.ai[1], TurnAngle, Age);
    private float Strength => Firing ? Math.Min(1f,
        MathF.Pow((LaserWarningDuration + LaserDuration - Age) / (LaserDuration * 0.5f), 2f)) : 0f;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 16;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.netImportant = true;
        Projectile.timeLeft = LaserWarningDuration + LaserDuration + 60;
        Projectile.hide = true;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override bool ShouldUpdatePosition() => false;

    protected override void SendAnomalyExtraAI(BinaryWriter writer)
    {
        base.SendAnomalyExtraAI(writer);
        writer.Write(TurnAngle);
        writer.Write(Punishment);
    }

    protected override void ReceiveAnomalyExtraAI(BinaryReader reader)
    {
        base.ReceiveAnomalyExtraAI(reader);
        TurnAngle = reader.ReadSingle();
        Punishment = reader.ReadBoolean();
    }

    public override void AI()
    {
        SkeletronLegendHead head = Head;
        if (head is not { StellarMechanicsActive: true }
            || head.CurrentBehavior == SkeletronLegendHead.Behavior.Despawn
            || Age >= LaserWarningDuration + LaserDuration)
        {
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }
        if (head.Phase2Arena is LegendaryNightPhase2Arena arena)
            Projectile.Center = arena.Projectile.Center;
        Projectile.rotation = Angle;
        // 蓝激光独立检测本机玩家，绕过原版免疫路径；同一束在伤害窗口内只结算一次。
        if (Punishment && Firing && !Main.dedServ && !_PunishmentHit)
        {
            Player player = Main.LocalPlayer;
            if (player.active && !player.dead && !player.ghost && Colliding(Projectile.Hitbox, player.Hitbox) == true)
            {
                _PunishmentHit = true;
                LegendaryNightDamage.HurtLocalPlayer(Projectile, Projectile.damage, true,
                    ignoreAllImmunity: true);
            }
        }
        if (!Main.dedServ && Age == LaserWarningDuration)
            PlayFiringSound();

        void PlayFiringSound()
        {
            // 同帧只播放距本机玩家最近的一束，避免多束激光叠加音效。
            Vector2 listener = Main.LocalPlayer.Center;
            Vector2 position = ClosestPoint(this);
            float distanceSquared = Vector2.DistanceSquared(listener, position);
            foreach (Projectile projectile in Projectile.ActiveProjectiles)
            {
                if (projectile.ModProjectile is not LegendEnergyLaser other || other == this
                    || other.Age != LaserWarningDuration || other.Head is not SkeletronLegendHead { StellarMechanicsActive: true } otherHead
                    || otherHead.CurrentBehavior == SkeletronLegendHead.Behavior.Despawn)
                    continue;
                float otherDistanceSquared = Vector2.DistanceSquared(listener, ClosestPoint(other));
                if (otherDistanceSquared < distanceSquared
                    || otherDistanceSquared == distanceSquared && projectile.whoAmI < Projectile.whoAmI)
                    return;
            }

            Vector2 screenCenter = Main.screenPosition + new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
            float attenuation = MathF.Sqrt(Math.Max(1f - Vector2.Distance(screenCenter, position) / 2500f, 0f));
            if (attenuation <= 0f)
                return;
            float pan = MathHelper.Clamp((position.X - screenCenter.X) / (Main.screenWidth * 0.5f), -1f, 1f);
            SoundEngine.PlaySound(FiringSound, updateCallback: sound =>
            {
                sound.Volume = attenuation;
                sound.Sound.Pan = pan;
                return true;
            });

            Vector2 ClosestPoint(LegendEnergyLaser laser)
            {
                Vector2 direction = laser.Angle.ToRotationVector2();
                float distance = MathHelper.Clamp(Vector2.Dot(listener - laser.Projectile.Center, direction), 0f, LaserLength);
                return laser.Projectile.Center + direction * distance;
            }
        }
    }

    public override bool CanHitPlayer(Player target) => !Punishment && !_HitPlayers[target.whoAmI] && Firing && Head is SkeletronLegendHead { StellarMechanicsActive: true } head
        && head.CurrentBehavior != SkeletronLegendHead.Behavior.Despawn;

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Firing
        && RotatedRectangle.FromLineSegment(Projectile.Center,
            Projectile.Center + Angle.ToRotationVector2() * LaserLength, LaserWidth).Collides(targetHitbox);

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        _HitPlayers[target.whoAmI] = true;
        target.AddBuff(ModContent.BuffType<AstralSuppression>(), 15 * 60);
    }

    // 激光中心可能在屏外；明确加入绘制列表，不依赖小尺寸实体的屏幕裁剪。
    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs,
        List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => overPlayers.Add(index);

    public override bool PreDraw(ref Color lightColor)
    {
        int age = Age;
        if (Main.dedServ || age < 0 || age >= LaserWarningDuration + LaserDuration
            || Head is not SkeletronLegendHead { StellarMechanicsActive: true } head
            || head.CurrentBehavior == SkeletronLegendHead.Behavior.Despawn)
            return false;
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D pixel = TOTextures.FillerTexture;
        bool firing = Firing;
        float progress = MathHelper.Clamp(age / (float)LaserWarningDuration, 0f, 1f);
        float appear = MathHelper.Clamp((age + 1f) / LaserAppearDuration, 0f, 1f);
        // Radiance 的三次方蓄力、末段集中辉光及20帧光束衰减。
        float opacity = Math.Max(0.15f, progress * progress * progress + 0.25f * progress * (1f - progress)) * appear;
        float glow = progress > 0.33f ? MathF.Pow((progress - 0.33f) / 0.67f, 10f) * 2f : 0f;
        AnomalyEffects.MiscShaderContainer shader = !AnomalyClientConfig.Instance.EnableShaders ? null
            : firing ? AnomalyEffects.Primitives.LegendStellarLaser : AnomalyEffects.Primitives.LegendStellarLaserWarning;
        Color beamColor = EffectiveBeamColor;
        Color color = beamColor;
        color.A = (byte)(beamColor.A * 190 / 255);
        Vector2 position = Projectile.Center - Main.screenPosition;
        if (shader is null)
        {
            float width = firing ? LaserWidth * Strength : 3f;
            Color edgeColor = beamColor with { A = 0 };
            float opacityScale = (firing ? Strength : appear) * (beamColor.A / 255f);
            spriteBatch.Draw(pixel, position, null, edgeColor * opacityScale,
                Angle, new Vector2(0f, pixel.Height * 0.5f),
                new Vector2(LaserLength / pixel.Width, width / pixel.Height), SpriteEffects.None, 0f);
            if (firing)
            {
                Color coreColor = Color.Lerp(beamColor, Color.White, 0.8f) with { A = 0 };
                spriteBatch.Draw(pixel, position, null, coreColor * opacityScale,
                    Angle, new Vector2(0f, pixel.Height * 0.5f),
                    new Vector2(LaserLength / pixel.Width, width * 0.45f / pixel.Height), SpriteEffects.None, 0f);
            }
            return false;
        }
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        MiscShaderData data = shader.Data;
        data.SetCustomParameter("uBeamSize", new Vector2(BeamRadius, LaserLength));
        if (firing)
        {
            data.SetCustomParameter("uBeamStrength", Strength)
                .SetCustomParameter("uGlobalTime", (float)Main.timeForVisualEffects / 60f);
        }
        else
        {
            data.SetCustomParameter("uMarkerOpacity", opacity)
                .SetCustomParameter("uGlow", glow);
        }
        data.Apply();
        spriteBatch.Draw(pixel, position, null, color, Angle, new Vector2(0f, pixel.Height * 0.5f),
            new Vector2(LaserLength / pixel.Width, BeamRadius * 2f / pixel.Height), SpriteEffects.None, 0f);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        return false;
    }
}