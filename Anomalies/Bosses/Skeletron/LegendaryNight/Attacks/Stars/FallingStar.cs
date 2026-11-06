namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 支持延迟加速与拍击扇形速度曲线的落星。
/// </summary>
/// <remarks>
/// <para>ai[0]：MotionMode，0延迟加速、1拍击扇形；ai[1]：扇形生成时的竞技场半径，普通模式未使用；ai[2]：主体索引 + 1，供所属攻击清理识别，0可独立存在。</para>
/// <para>localAI：未使用；Timer1（OceanAI32[2].I）：飞行累计帧；其余 Timer 未使用。</para>
/// <para>ExtraAI：TrailType、AccelerationStartTime、颜色覆盖存在标志与可选 PackedValue；oldPos 仅作本地拖尾。</para>
/// </remarks>
public sealed class FallingStar : AnomalyModProjectile
{
    // ai[0]：运动模式；扇形模式的 ai[1] 为发射时场地半径，ai[2] 为主体索引（1-based）。
    public enum MotionMode
    {
        Accelerating,
        StrikeFan
    }

    public MotionMode CurrentMotionMode
    {
        get => (MotionMode)(int)Projectile.ai[0];
        set => Projectile.ai[0] = (int)value;
    }

    public override string Texture => LegendaryNightHandler.LegendaryNightPath + "Attacks/Stars/FallingStar";

    /// <summary>拖尾类型：0 无拖尾，1 蓝白光尾，2 金色轨迹拖尾。</summary>
    public int TrailType;

    /// <summary>可选拖尾颜色覆盖；null 时保留对应拖尾的默认颜色。</summary>
    public Color? TrailColorOverride;

    /// <summary>Timer1 超过此值后开始加速。</summary>
    public int AccelerationStartTime = 420;

    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 40;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = 100;
        Projectile.height = 100;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.timeLeft = 840;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
    }

    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, LegendaryNightHandler.StarColor.ToVector3());
        Projectile.rotation += 0.1f * Projectile.direction;

        Timer1++;
        if (CurrentMotionMode == MotionMode.StrikeFan)
        {
            float speed = SkeletronLegendHead.Mechanic2Parameters.GetFanStarSpeed(Projectile.ai[1], (int)Timer1 - 1);
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * speed;
        }
        else if (Timer1 > AccelerationStartTime)
            Projectile.velocity *= 1.02f;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        LegendaryNightStarDrawing.DrawFallingStar(Projectile.Center, Projectile.rotation, Projectile.scale, Projectile.velocity, trailType: TrailType, useMediumTexture: true, oldPositions: Projectile.oldPos, trailOffset: Projectile.Size * 0.5f, trailColorOverride: TrailColorOverride);
        return false;
    }

    public override void OnKill(int timeLeft)
    {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 18; i++)
        {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(24f, 24f) * Projectile.scale,
                DustID.GoldFlame, Main.rand.NextPolarVector2(1f, 5f), newColor: LegendaryNightHandler.StarColor,
                Scale: Main.rand.NextFloat(0.7f, 1.2f));
            dust.noGravity = true;
        }
    }

    protected override void SendAnomalyExtraAI(BinaryWriter writer)
    {
        base.SendAnomalyExtraAI(writer);
        writer.Write(TrailType);
        writer.Write(AccelerationStartTime);
        writer.Write(TrailColorOverride.HasValue);
        if (TrailColorOverride.HasValue)
            writer.Write(TrailColorOverride.Value.PackedValue);
    }

    protected override void ReceiveAnomalyExtraAI(BinaryReader reader)
    {
        base.ReceiveAnomalyExtraAI(reader);
        TrailType = reader.ReadInt32();
        AccelerationStartTime = reader.ReadInt32();
        TrailColorOverride = reader.ReadBoolean() ? new Color { PackedValue = reader.ReadUInt32() } : null;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        Vector2 center = Projectile.Center;
        float scale = Projectile.scale;
        float rotation = Projectile.rotation;

        //第一步：圆形包围盒检测
        float outerRadius = 74.5f * scale; //落星的外接圆半径为 74.5 * scale
        if (!new Circle(center, outerRadius).Collides(targetHitbox))
            return false;

        rotation += MathHelper.PiOver2; //标准落星对应的正五边形是一个顶点朝下的，所以需在旋转角度上加上 π / 2，表示顺时针旋转 90 度。

        //第二步：正五边形检测
        float sideLength = 44.7f * scale;
        float circumradius = sideLength / (MathF.Sin(TOMathUtils.PiOver5) * 2f);
        Pentagon pentagon = new(center, circumradius, rotation);
        if (pentagon.Collides(targetHitbox))
            return true;

        //第三步：外部的五个等腰三角形检测
        (Vector2 V0, Vector2 V1, Vector2 V2, Vector2 V3, Vector2 V4) = pentagon.Vertices;
        float step = TOMathUtils.PiOver5 * 2f; // 2π/5

        //外部顶点，V0与V1的中点，中心三点共线
        // 使用 pentagon 的 Rotation 确保外部顶点角度与五边形顶点索引对齐
        PolarVector2 toOuterV0 = new(outerRadius, pentagon.Rotation + TOMathUtils.PiOver5);
        Vector2 outerV0 = center + toOuterV0;
        if (new Triangle(V0, V1, outerV0).Collides(targetHitbox))
            return true;
        Vector2 outerV1 = center + toOuterV0.RotatedBy(step);
        if (new Triangle(V1, V2, outerV1).Collides(targetHitbox))
            return true;
        Vector2 outerV2 = center + toOuterV0.RotatedBy(step * 2f);
        if (new Triangle(V2, V3, outerV2).Collides(targetHitbox))
            return true;
        Vector2 outerV3 = center + toOuterV0.RotatedBy(step * 3f);
        if (new Triangle(V3, V4, outerV3).Collides(targetHitbox))
            return true;
        Vector2 outerV4 = center + toOuterV0.RotatedBy(step * 4f);
        if (new Triangle(V4, V0, outerV4).Collides(targetHitbox))
            return true;

        //如果第二步与第三步都没有发生碰撞，则说明落星与矩形没有碰撞
        return false;
    }
}