namespace Anomalies.Bosses.Skeletron.LegendaryNight;

// ai[0]：铜/银/金/铂；ai[1]：预警计时；ai[2]：所属骷髅王（1-based）。
public sealed class CoinJudgment : AnomalyModProjectile
{
    public const int WarningDuration = 180;
    public const int DangerWarningDuration = 60;
    private const int DangerWarningFadeInDuration = 6;
    private const float DangerWarningMaxOpacity = 0.22f;
    public const int OverlayFadeInDuration = 8;
    public const int OverlayHoldDuration = 8;
    public const int OverlayFadeOutDuration = 24;
    public const float OverlayMaxOpacity = 0.9f;
    private const int RingFadeInDuration = 12;
    private const float RingMaxRadius = 320f;
    public const float CopperRadius = 960f;
    public const float SilverHalfWidth = 320f;
    public const float GoldInnerRadius = 960f;
    public const float GoldOuterRadius = 3200f;
    public const float PlatinumSafeHalfWidth = 160f;

    public static readonly SoundStyle ExplosionSound = new(LegendaryNightHandler.LegendaryNightEntityPath + "CoinJudgmentExplosion") { Volume = 1.5f };
    public static readonly SoundStyle WarningSound = SoundID.Item4 with { Volume = 0.65f, Pitch = -0.35f };

    private bool _Judged;
    private bool _WarningSoundPlayed;
    public float OverlayOpacity => GetOverlayOpacity(Projectile.ai[1]);
    public float WarningOverlayOpacity => GetWarningOverlayOpacity(Projectile.ai[1]);
    public float BoundaryOpacity => Projectile.ai[1] < WarningDuration
        ? MathHelper.Clamp(Projectile.ai[1] / RingFadeInDuration, 0f, 1f)
        : OverlayOpacity / OverlayMaxOpacity;

    public static float GetWarningOverlayOpacity(float timer)
    {
        float age = timer - (WarningDuration - DangerWarningDuration);
        if (age < 0f || timer >= WarningDuration)
            return 0f;
        return DangerWarningMaxOpacity * MathHelper.Clamp(age / DangerWarningFadeInDuration, 0f, 1f);
    }

    public static float GetOverlayOpacity(float timer)
    {
        float age = timer - WarningDuration;
        if (age < 0f)
            return OverlayMaxOpacity * MathHelper.Clamp((age + OverlayFadeInDuration) / OverlayFadeInDuration, 0f, 1f);
        if (age <= OverlayHoldDuration)
            return OverlayMaxOpacity;
        return OverlayMaxOpacity * MathHelper.Clamp(1f - (age - OverlayHoldDuration) / OverlayFadeOutDuration, 0f, 1f);
    }

    public override string Texture => TOTextures.InvisibleTexturePath;
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.netImportant = true;
        Projectile.timeLeft = WarningDuration + OverlayHoldDuration + OverlayFadeOutDuration + 1;
    }

    // 伤害由本机玩家的 Hurt 路径结算，避免常规敌对弹幕检测提前被无敌帧拦截或重复命中。
    public override bool? CanDamage() => false;
    public override bool ShouldUpdatePosition() => false;
    public override bool PreDraw(ref Color lightColor)
    {
        // 蓝圈和白色遮罩共用判定弹幕的计时，避免独立弹幕更新顺序或同步造成错位。
        float timer = Projectile.ai[1];
        float ringDuration = WarningDuration - OverlayFadeInDuration;
        float progress = MathHelper.Clamp(timer / ringDuration, 0f, 1f);
        if (timer < ringDuration)
        {
            float radiusFactor = 1f - progress;
            float opacity = MathHelper.Clamp(timer / RingFadeInDuration, 0f, 1f);
            Texture2D ring = ParticleHandler.GetTexture<PulseRing>();
            Main.spriteBatch.DrawFromCenter(ring, Projectile.Center - Main.screenPosition, null,
                new Color(70, 150, 255, 0) * opacity, 0f, RingMaxRadius * radiusFactor / PulseRing.TextureRadius);
        }
        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        Vector2 center = Projectile.Center;
        // 在待测碰撞箱的纵/横跨度上截取无限条带，避免人为添加远处的外边界。
        FloatRectangle vertical = new(center.X - SilverHalfWidth, targetHitbox.Top - 1f, SilverHalfWidth * 2f, targetHitbox.Height + 2f);
        FloatRectangle horizontal = new(targetHitbox.Left - 1f, center.Y - SilverHalfWidth, targetHitbox.Width + 2f, SilverHalfWidth * 2f);
        FloatRectangle platinumSafe = new(center.X - PlatinumSafeHalfWidth, targetHitbox.Top - 1f, PlatinumSafeHalfWidth * 2f, targetHitbox.Height + 2f);
        return (int)Projectile.ai[0] switch
        {
            0 => new Circle(center, CopperRadius).Collides(targetHitbox),
            1 => vertical.Collides(targetHitbox) || horizontal.Collides(targetHitbox),
            2 => new Annulus(center, GoldInnerRadius, GoldOuterRadius).Collides(targetHitbox),
            // 铂金币没有外侧安全区：整个玩家碰撞箱必须位于无限延伸的竖直安全带内。
            3 => !platinumSafe.Contains(targetHitbox.TopLeft()) || !platinumSafe.Contains(targetHitbox.BottomRight()),
            _ => false
        };
    }

    public override void AI()
    {
        NPC master = NPC.GetNPCFromIndex((int)Projectile.ai[2] - 1);
        if (!master.active || master.type != NPCID.SkeletronHead || master.Anomaly.CustomAIType != 1
            || SkeletronLegendHead.GetInstance(master).CurrentAvatar != SkeletronLegendHead.Avatar.PlatinumCoin)
        {
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }

        int timer = (int)Projectile.ai[1] + 1;
        Projectile.ai[1] = timer;
        if (!_WarningSoundPlayed && timer >= WarningDuration - DangerWarningDuration && timer < WarningDuration && !Main.dedServ)
        {
            _WarningSoundPlayed = true;
            SoundEngine.PlaySound(WarningSound, Projectile.Center);
        }

        if (timer == WarningDuration - 5)
            SoundEngine.PlaySound(ExplosionSound);

        if (timer < WarningDuration)
            return;

        if (_Judged)
            return;
        _Judged = true;
        if (Main.dedServ)
            return;

        Player player = Main.LocalPlayer;
        if (!player.active || player.dead || player.ghost || Colliding(Projectile.Hitbox, player.Hitbox) != true)
            return;

        LegendaryNightDamage.HurtLocalPlayer(Projectile, Projectile.damage, false, knockback: 4.5f);
    }
}
