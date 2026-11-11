namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 按金币种类产生不同形状的审判攻击。
/// </summary>
/// <remarks>
/// <para>ai[0]：金币种类，0铜、1银、2金、3铂金；ai[1]：锁定时的主体自转帧，机制五存放负的锁定音乐帧；ai[2]：主体 NPC 索引 + 1。</para>
/// <para>localAI：未使用；框架 Timer1..5：未使用；无实体专属 ExtraAI。</para>
/// </remarks>
public sealed class CoinJudgment : AnomalyModProjectile
{
    /// <summary>预警持续时间，单位为游戏帧。</summary>
    public const int WarningDuration = 150;
    /// <summary>危险预警持续时间，单位为游戏帧。</summary>
    public const int DangerWarningDuration = 20;
    private const int BoundaryWarningDuration = 30;
    /// <summary>危险预警淡入持续时间，单位为游戏帧。</summary>
    private const int DangerWarningFadeInDuration = 6;
    /// <summary>危险预警最大透明度。</summary>
    private const float DangerWarningMaxOpacity = 0.22f;
    private const int OverlayFadeInDuration = 10;
    /// <summary>遮罩保持持续时间，单位为游戏帧。</summary>
    public const int OverlayHoldDuration = LegendaryNightStarburstParameters.StarburstDamageDuration;
    /// <summary>遮罩淡出持续时间，单位为游戏帧。</summary>
    public const int OverlayFadeOutDuration = LegendaryNightStarburstParameters.StarburstFadeDuration;
    /// <summary>遮罩最大透明度。</summary>
    public const float OverlayMaxOpacity = 0.9f;
    private const int BoundaryFadeInDuration = 12;
    /// <summary>环形最大半径，单位为世界像素。</summary>
    private const float RingMaxRadius = 320f;
    /// <summary>圆半径，单位为世界像素。</summary>
    public const float NormalCircleRadius = 960f;
    /// <summary>十字半宽，单位为世界像素。</summary>
    public const float NormalCrossHalfWidth = 320f;
    private const float Phase2CrossHalfWidth = 240f;
    /// <summary>金币外层半径，单位为世界像素。</summary>
    public const float GoldOuterRadius = 3200f;
    internal static float CrossRotation(int coinType) => coinType == 3 ? MathHelper.PiOver4 : 0f;

    /// <summary>爆炸音效。</summary>
    public static readonly SoundStyle ExplosionSound = new(LegendaryNightHandler.LegendaryNightPath + "Attacks/Coins/CoinJudgmentExplosion") { Volume = 1.5f };
    /// <summary>预警音效。</summary>
    public static readonly SoundStyle WarningSound = SoundID.Item4 with { Volume = 0.65f, Pitch = -0.35f };

    private bool _Judged;
    private bool _WarningSoundPlayed;
    private int _Age = -1;
    private bool IsMechanic5 => Projectile.ai[1] < 0f;
    private int ImpactAge => IsMechanic5
        ? SkeletronLegendHead.Mechanic5Parameters.JudgmentInterval - CoinRoulette.FirstLockFrame
        : WarningDuration;

    private float? ArenaInnerRadius => IsMechanic5
        && LegendaryNightHandler.TryGetLegendaryNightHead((int)Projectile.ai[2], out SkeletronLegendHead head, oneBased: true)
        ? head.Phase2Arena?.MainCircleRadius : null;
    internal float CrossHalfWidth => IsMechanic5 ? Phase2CrossHalfWidth : NormalCrossHalfWidth;
    internal float EffectiveCopperRadius => ArenaInnerRadius ?? NormalCircleRadius;
    internal float EffectiveGoldInnerRadius => ArenaInnerRadius ?? NormalCircleRadius;
    public float OverlayOpacity => GetOverlayOpacity(_Age - ImpactAge + WarningDuration);
    public float WarningOverlayOpacity => IsMechanic5 || _Age >= ImpactAge - OverlayFadeInDuration
        ? 0f : GetWarningOverlayOpacity(_Age - (WarningDuration - DangerWarningDuration), DangerWarningDuration);
    public float BoundaryOpacity => IsMechanic5 ? 0f : _Age < WarningDuration
        ? MathHelper.Clamp((_Age - (WarningDuration - BoundaryWarningDuration)) / (float)BoundaryFadeInDuration, 0f, 1f)
        : OverlayOpacity / OverlayMaxOpacity;
    public static float GetWarningOverlayOpacity(float age, int duration)
    {
        if (age < 0f || age >= duration)
            return 0f;
        return DangerWarningMaxOpacity * MathHelper.Clamp(age / DangerWarningFadeInDuration, 0f, 1f);
    }
    public static float GetOverlayOpacity(float timer)
    {
        float age = timer - WarningDuration;
        if (age < -OverlayFadeInDuration)
            return 0f;
        float fadeIn = MathHelper.Clamp((age + OverlayFadeInDuration + 1f) / (OverlayFadeInDuration + 1f), 0f, 1f);
        float fadeOut = MathHelper.Clamp((OverlayHoldDuration + OverlayFadeOutDuration - age) / OverlayFadeOutDuration, 0f, 1f);
        return OverlayMaxOpacity * fadeIn * fadeOut;
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
        Projectile.timeLeft = WarningDuration + OverlayHoldDuration + OverlayFadeOutDuration + 2;
    }

    // 伤害由本机玩家的 Hurt 路径结算，避免常规敌对弹幕检测提前被无敌帧拦截或重复命中。
    public override bool? CanDamage() => false;
    public override bool ShouldUpdatePosition() => false;
    public override bool PreDraw(ref Color lightColor)
    {
        // 蓝圈与复制币固定在锁定位置，共用审判计时。
        if (_Age >= 0 && _Age < ImpactAge)
        {
            float radiusFactor = 1f - _Age / (float)ImpactAge;
            Vector2 center = Projectile.Center - Main.screenPosition;
            Texture2D ring = ParticleHandler.GetTexture<PulseRing>();
            Main.spriteBatch.DrawFromCenter(ring, center, null,
                new Color(70, 150, 255, 0), scale: RingMaxRadius * radiusFactor / PulseRing.TextureRadius);
            CoinRoulette.DrawCoin(center, (int)Projectile.ai[0], 0, 1f, 0.8f);
        }
        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        Vector2 center = Projectile.Center;
        int coinType = (int)Projectile.ai[0];
        return coinType switch
        {
            0 => new Circle(center, EffectiveCopperRadius).Collides(targetHitbox),
            1 or 3 => CrossCollides(),
            2 => new Annulus(center, EffectiveGoldInnerRadius, GoldOuterRadius).Collides(targetHitbox),
            _ => false
        };

        bool CrossCollides()
        {
            // 将条带延伸至目标碰撞箱之外，检测时等价于无限延伸的十字。
            Vector2 offset = new Vector2(targetHitbox.Center.X, targetHitbox.Center.Y) - center;
            float reach = offset.Length() + targetHitbox.Width + targetHitbox.Height + 1f;
            Vector2 axis = Vector2.UnitX.RotatedBy(CrossRotation(coinType)) * reach;
            Vector2 perpendicular = new(-axis.Y, axis.X);
            return RotatedRectangle.FromLineSegment(center - axis, center + axis, CrossHalfWidth * 2f).Collides(targetHitbox)
                || RotatedRectangle.FromLineSegment(center - perpendicular, center + perpendicular, CrossHalfWidth * 2f).Collides(targetHitbox);
        }
    }

    public override void AI()
    {
        if (!LegendaryNightHandler.TryGetLegendaryNightHead((int)Projectile.ai[2], out SkeletronLegendHead head, oneBased: true)
            || (IsMechanic5 ? !head.Mechanic5Active : head.CurrentPhase != SkeletronLegendHead.Phase.Phase1
                || head.CurrentSpell != SkeletronLegendHead.Spell.PlatinumCoin))
        {
            _Age = -1;
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }

        // 第二次结算所在帧已切回常态；从常态计时接续冲击余光。
        int spinTime = head.CurrentBehavior == SkeletronLegendHead.Behavior.Spin
            ? head.Timer1
            : CoinRoulette.RotationDuration + head.Timer1;
        int timer = IsMechanic5 ? head.Phase2MusicElapsedTime + (int)Projectile.ai[1] : spinTime - (int)Projectile.ai[1];
        _Age = timer;
        if (!IsMechanic5 && !_WarningSoundPlayed && timer >= WarningDuration - BoundaryWarningDuration && timer < WarningDuration && !Main.dedServ)
        {
            _WarningSoundPlayed = true;
            SoundEngine.PlaySound(WarningSound);
        }

        if (timer != ImpactAge || _Judged)
            return;

        _Judged = true;
        if (!IsMechanic5 && TONetworkEnvironment.NotClient)
            head.RecordPhase1CoinJudgment((int)Projectile.ai[0]);
        if (Main.dedServ)
            return;

        SoundEngine.PlaySound(ExplosionSound);

        Player player = Main.LocalPlayer;
        if (!player.active || player.dead || player.ghost || Colliding(Projectile.Hitbox, player.Hitbox) != true)
            return;

        LegendaryNightDamage.HurtLocalPlayer(Projectile, Projectile.damage, true, knockback: 4.5f);
    }
}
