using Anomalies.Bosses.Skeletron.LegendaryNight.Entities;
using Anomalies.GameContents.Base;
using Anomalies.GameContents.Buffs;

namespace Anomalies.Bosses.Skeletron.LegendaryNight.Arena;

public sealed class LegendaryNightPhase2Arena : BaseRingArenaProjectile
{
    public const int AppearanceDuration = 90;
    public const float MainCircleFullRadius = 800f;
    public const float SmallCircleRadiusRatio = 0.15f;
    public const float OuterCircleRadiusRatio = 1f + 2f * SmallCircleRadiusRatio;
    public const float RotationSpeed = 0.007f;
    public const float FirstSmallCircleOrbitSpeed = RotationSpeed * 0.8f;
    public const float SecondSmallCircleOrbitSpeed = -RotationSpeed * 1.1f;
    public const float FirstSmallCircleSpinSpeed = RotationSpeed * 2f;
    public const float SecondSmallCircleSpinSpeed = -RotationSpeed * 3f;

    public static readonly SoundStyle SummonSound = new(LegendaryNightHandler.LegendaryNightPath + "Arena/Phase2ArenaSummon");

    // ai[0]：所属骷髅王索引（1-based）；ai[1]：法阵出现时的音乐帧数。
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
    public SkeletronLegendHead MasterBehavior => Master is { active: true, type: NPCID.SkeletronHead, life: > 0 } master
        && master.Anomaly.CustomAIType == 1 ? SkeletronLegendHead.GetInstance(master) : null;
    public int MusicAge => Math.Max(0, (MasterBehavior?.Phase2MusicElapsedTime ?? AppearanceStartMusicTime) - AppearanceStartMusicTime);
    public float AppearanceProgress => MathHelper.Clamp(MusicAge / (float)AppearanceDuration, 0f, 1f);
    public float Intensity => MathHelper.SmoothStep(0f, 1f, AppearanceProgress);
    public float AppearanceScale => MathHelper.Lerp(0.2f, 1f, 1f - MathF.Pow(1f - AppearanceProgress, 3f));
    public float MagicCircleRotation => MusicAge * RotationSpeed;
    public float MainCircleRadius => Radius / OuterCircleRadiusRatio;
    public float SmallCircleRadius => MainCircleRadius * SmallCircleRadiusRatio;
    public const float LaserWidth = 24f;
    public bool Materialized => MasterBehavior is { Mechanic2Active: true } head && SkeletronLegendHead.Mechanic2Data.LasersActive(head.Mechanic2Age);
    public float MaterializationWarning => MasterBehavior is { Mechanic2Active: true } head ? SkeletronLegendHead.Mechanic2Data.LaserWarning(head.Mechanic2Age) : 0f;
    public float MaterializationIntensity => MasterBehavior is { Mechanic2Active: true } head ? SkeletronLegendHead.Mechanic2Data.LaserIntensity(head.Mechanic2Age) : 0f;
    public Pentagon CentralPentagon => GetCentralPentagon(Projectile.Center, MainCircleRadius, MagicCircleRotation);
    public Annulus MaterializedRing => new(Projectile.Center, MainCircleRadius, Radius);

    public static Pentagon GetCentralPentagon(Vector2 center, float starRadius, float rotation) => new(center,
        starRadius * MathF.Cos(MathHelper.TwoPi / 5f) / MathF.Cos(MathHelper.Pi / 5f), rotation + MathHelper.Pi / 5f);

    // 统一提供视觉与后续交互使用的几何数据；0 主法阵，1/2 两个附属法阵。
    public (Vector2 Center, float Radius, float Rotation) GetMagicCircle(int index)
    {
        if (index == 0)
            return (Projectile.Center, MainCircleRadius, MagicCircleRotation);
        // 两个小法阵以不同角速度相向公转，并分别自转；均由音乐时钟确定。
        float orbitSpeed = index == 1 ? FirstSmallCircleOrbitSpeed : SecondSmallCircleOrbitSpeed;
        float spinSpeed = index == 1 ? FirstSmallCircleSpinSpeed : SecondSmallCircleSpinSpeed;
        float angle = MusicAge * orbitSpeed + (index == 1 ? -MathHelper.Pi * 0.8f : MathHelper.Pi * 0.8f);
        Vector2 center = Projectile.Center + angle.ToRotationVector2() * (MainCircleRadius + SmallCircleRadius);
        return (center, SmallCircleRadius, MusicAge * spinSpeed);
    }

    public Vector2 GetStarVertex(int circleIndex, int vertexIndex)
    {
        (Vector2 Center, float Radius, float Rotation) circle = GetMagicCircle(circleIndex);
        return circle.Center + (circle.Rotation + MathHelper.TwoPi * vertexIndex / 5f).ToRotationVector2() * circle.Radius;
    }

    public override string Texture => TOTextures.InvisibleTexturePath;
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;
    public override Player Target => MasterBehavior?.Target;
    public override float Radius { get; set; } = MainCircleFullRadius * OuterCircleRadiusRatio * 0.2f;
    public override float Thickness => 0f;
    public override int DustType => DustID.GoldFlame;
    public override bool EnableMovementRestriction => MasterBehavior is { Phase2ArenaRequested: true } && AppearanceProgress >= 1f;

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
            if (TOSharedData.NotClient)
                Projectile.Kill();
            return;
        }
        Projectile.timeLeft = 100;
        Projectile.velocity = Vector2.Zero;
        // 圆心在生成时固定；半径与所有绘制共用同一份入场缩放。
        Radius = MainCircleFullRadius * OuterCircleRadiusRatio * AppearanceScale;
        if (EnableMovementRestriction)
            MovementRestriction();
        else
            OutofArenaTimer = 0;

        if (!Main.dedServ && head.Mechanic2Active)
        {
            int age = head.Mechanic2Age;
            for (int strike = 0; strike < SkeletronLegendHead.Mechanic2Data.StrikeCount; strike++)
            {
                if (age != SkeletronLegendHead.Mechanic2Data.GetStrikeTime(strike))
                    continue;
                Player player = Main.LocalPlayer;
                if (new Circle(Projectile.Center, Radius).Collides(player.Hitbox)
                    && (head.Mechanic2StrikeSide(strike) < 0 ? player.Hitbox.Left < Projectile.Center.X : player.Hitbox.Right > Projectile.Center.X))
                    LegendaryNightDamage.HurtLocalPlayer(Projectile, SkeletronLegendHead.SlapDamage, true);
            }
        }
    }

    public override bool CanHitPlayer(Player target) => Materialized || EnableMovementRestriction && base.CanHitPlayer(target);
    // 平时圆周安全；每次拍击时将内外圆之间的圆环、五角星线段和中央五边形一起实体化。
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) =>
        Materialized && MaterializedGeometryCollides(targetHitbox)
        || EnableMovementRestriction && Vector2.DistanceSquared(targetHitbox.Center.ToVector2(), Projectile.Center) > Radius * Radius;

    public bool MaterializedGeometryCollides(Rectangle hitbox)
    {
        if (CentralPentagon.Collides(hitbox)
            || MaterializedRing.Collides(hitbox)
            || new Annulus(Projectile.Center, Radius - LaserWidth * 0.5f, Radius + LaserWidth * 0.5f).Collides(hitbox))
            return true;
        for (int circle = 0; circle < 3; circle++)
        {
            (Vector2 center, float radius, float rotation) = GetMagicCircle(circle);
            if (MagicCircleCollides(center, radius, rotation, hitbox))
                return true;
        }
        return false;
    }

    public static bool MagicCircleCollides(Vector2 center, float radius, float rotation, Rectangle hitbox)
    {
        if (new Annulus(center, radius - LaserWidth * 0.5f, radius + LaserWidth * 0.5f).Collides(hitbox))
            return true;
        for (int i = 0; i < 5; i++)
        {
            Vector2 start = center + (rotation + MathHelper.TwoPi * i / 5f).ToRotationVector2() * radius;
            Vector2 end = center + (rotation + MathHelper.TwoPi * (i + 2) / 5f).ToRotationVector2() * radius;
            Vector2 edge = end - start;
            RotatedRectangle laser = new((start + end) * 0.5f, edge.Length(), LaserWidth, edge.ToRotation());
            if (laser.Collides(hitbox))
                return true;
        }
        return false;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        if (Materialized && MaterializedGeometryCollides(target.Hitbox))
            target.AddBuff(ModContent.BuffType<StarPowerSuppression>(), 15 * 60);
    }
    public override bool PreDraw(ref Color lightColor) => false;
}
