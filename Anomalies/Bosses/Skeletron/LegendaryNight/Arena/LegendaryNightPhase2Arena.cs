using Anomalies.BaseContents;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 二阶段竞技场，提供边界、法阵几何、公转和实体化碰撞。
/// </summary>
/// <remarks>
/// <para>ai[0]：主体 NPC 索引 + 1；ai[1]：展开起始音乐帧；ai[2]：首次交汇方位弧度 + 1，0 表示服务器尚未初始化。</para>
/// <para>localAI：未使用；框架 Timer1..5：未使用。</para>
/// <para>ExtraAI：继承竞技场基类的 Radius、OutofArenaTimer；其余几何由音乐时钟推导。</para>
/// </remarks>
public sealed partial class LegendaryNightPhase2Arena : BaseRingArenaProjectile
{
    /// <summary>显现持续时间，单位为游戏帧。</summary>
    public const int AppearanceDuration = 90;
    /// <summary>主圆完整半径，单位为世界像素。</summary>
    public const float MainCircleFullRadius = 600f;
    /// <summary>小圆半径比例。</summary>
    public const float SmallCircleRadiusRatio = 1f / 6f;
    /// <summary>外圆半径比例。</summary>
    public const float OuterCircleRadiusRatio = 1f + 2f * SmallCircleRadiusRatio; //800f
    /// <summary>自转或公转角速度，单位为弧度/游戏帧。</summary>
    public const float RotationSpeed = 0.006f;
    /// <summary>首次小圆公转速度，以世界像素和游戏帧计量。</summary>
    public static float FirstSmallCircleOrbitSpeed => SlowOrbitSpeed;
    public static float GetFastOrbitSpeed(int musicTime) => GetOrbitSpeed(2, musicTime);
    /// <summary>自转或公转角速度，单位为弧度/游戏帧。</summary>
    public const float FirstSmallCircleSpinSpeed = RotationSpeed * 2f;
    /// <summary>自转或公转角速度，单位为弧度/游戏帧。</summary>
    public const float SecondSmallCircleSpinSpeed = -RotationSpeed * 3f;

    /// <summary>召唤音效。</summary>
    public static readonly SoundStyle SummonSound = new(LegendaryNightHandler.LegendaryNightPath + "Arena/Phase2ArenaSummon");

    // ai[0]：所属骷髅王索引（1-based）；ai[1]：法阵出现时的音乐帧数。
    // ai[2]：首次星爆方位角 + 1，由服务器随机选定并同步；亦为首次拍击时慢法阵的方位。
    public NPC Master
    {
        get => NPC.GetNPCFromIndex((int)Projectile.ai[0] - 1);
        set => Projectile.ai[0] = (value?.whoAmI ?? -1) + 1;
    }
    public int AppearanceStartMusicTime
    {
        get => (int)Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }
    public SkeletronLegendHead MasterBehavior => LegendaryNightHandler.TryGetLegendaryNightHead((int)Projectile.ai[0], out SkeletronLegendHead head, oneBased: true) && head.NPC.life > 0 ? head : null;
    public int MusicAge => Math.Max(0, (MasterBehavior?.Phase2MusicElapsedTime ?? AppearanceStartMusicTime) - AppearanceStartMusicTime);
    public float AppearanceProgress => MathHelper.Clamp(MusicAge / (float)AppearanceDuration, 0f, 1f);
    public float Intensity => MathHelper.SmoothStep(0f, 1f, AppearanceProgress);
    public float AppearanceScale => MathHelper.Lerp(0.2f, 1f, 1f - MathF.Pow(1f - AppearanceProgress, 3f));
    public float MagicCircleRotation => MusicAge * RotationSpeed;
    public float MainCircleRadius => Radius / OuterCircleRadiusRatio;
    public float SmallCircleRadius => MainCircleRadius * SmallCircleRadiusRatio;
    // 目标有效范围以完整法阵为准，不随入场缩放收缩，避免展开时误判脱战。
    /// <summary>目标选择半径，单位为世界像素。</summary>
    public static float TargetingRadius => MainCircleFullRadius * OuterCircleRadiusRatio * 2f;
    /// <summary>激光宽度，单位为世界像素。</summary>
    public const float LaserWidth = 24f;

    public override string Texture => TOTextures.InvisibleTexturePath;
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;
    public override Player Target => MasterBehavior?.Target;
    public override float Radius { get; set; } = MainCircleFullRadius * OuterCircleRadiusRatio * 0.2f;
    public override float Thickness => 0f;
    public override int DustType => DustID.GoldFlame;
    public override bool EnableMovementRestriction => MasterBehavior is { Phase2ArenaRequested: true } && AppearanceProgress >= 1f;

    private const int OutsideDamageInterval = 30;
    private int _OutsideTime;
    private int _OutsideDamageCooldown;

    public override void SetStaticDefaults() =>
        ProjectileID.Sets.PlayerHurtDamageIgnoresDifficultyScaling[Type] = true;

    public override void SetDefaults()
    {
        Projectile.netImportant = true;
        Projectile.width = Projectile.height = 16;
        Projectile.penetrate = -1;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.timeLeft = 100;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        SkeletronLegendHead head = MasterBehavior;
        if (head is null || !head.Phase2ArenaRequested)
        {
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }
        if (Projectile.ai[2] == 0f && TONetworkEnvironment.NotClient)
        {
            Projectile.ai[2] = 1f + Main.rand.NextFloat(MathHelper.TwoPi);
            Projectile.netUpdate = true;
        }
        Projectile.timeLeft = 100;
        Projectile.velocity = Vector2.Zero;
        // 圆心在生成时固定；半径与所有绘制共用同一份入场缩放。
        Radius = MainCircleFullRadius * OuterCircleRadiusRatio * AppearanceScale
            * (head.StellarMechanicsActive ? SkeletronLegendHead.Mechanic6Parameters.ArenaScale(head.Phase2MusicElapsedTime) : 1f);
        bool restrictMovement = EnableMovementRestriction;
        if (!Main.dedServ)
        {
            Player player = Main.LocalPlayer;
            bool outside = restrictMovement && player == Target && player.Alive && !player.ghost
                && Vector2.DistanceSquared(player.Center, Projectile.Center) > Radius * Radius;

            if (_OutsideDamageCooldown > 0)
                _OutsideDamageCooldown--;
            if (outside && _OutsideDamageCooldown == 0)
            {
                LegendaryNightDamage.HurtLocalPlayer(Projectile, SkeletronLegendHead.GetOutsideArenaDamage(_OutsideTime),
                    false, ignoreAllImmunity: true);
                _OutsideDamageCooldown = OutsideDamageInterval;
            }
            _OutsideTime = SkeletronLegendHead.GetNextOutsideArenaTime(_OutsideTime, outside);
        }

        if (restrictMovement)
            MovementRestriction();
        else
            OutofArenaTimer = 0;
    }

    public override bool CanHitPlayer(Player target) => Materialized;
    // 平时圆周安全；每次拍击时将内外圆之间的圆环、五角星线段和整个中央实心五角星一起实体化。
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) =>
        Materialized && MaterializedGeometryCollides(targetHitbox);

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        if (Materialized && MaterializedGeometryCollides(target.Hitbox))
            target.AddBuff(ModContent.BuffType<AstralSuppression>(), 15 * 60);
    }
    public override bool PreDraw(ref Color lightColor) => false;
}