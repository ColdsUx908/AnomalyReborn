namespace Anomalies.Bosses.Skeletron.LegendaryNight.Entities;

public sealed class FallingStar : AnomalyModProjectile
{
    /// <summary>拖尾类型：0 无拖尾，1 蓝白光尾，2 金色轨迹拖尾。</summary>
    public int TrailType;

    /// <summary>Timer1 超过此值后开始加速。</summary>
    public int AccelerationStartTime = 420;

    [LoadTexture(LegendaryNightHandler.LegendaryNightEntityPath + "FallingStarLight")]
    private static Asset<Texture2D> _LightTexture;
    public static Texture2D LightTexture => _LightTexture.Value;

    [LoadTexture(LegendaryNightHandler.LegendaryNightEntityPath + "FallingStar")]
    private static Asset<Texture2D> _StarTexture;
    public static Texture2D StarTexture => _StarTexture.Value;

    [LoadTexture(LegendaryNightHandler.LegendaryNightEntityPath + "FallingStar_Medium")]
    private static Asset<Texture2D> _MediumTexture;
    public static Texture2D MediumTexture => _MediumTexture.Value;

    [LoadTexture(LegendaryNightHandler.LegendaryNightEntityPath + "FallingStar_Glow")]
    private static Asset<Texture2D> _GlowTexture;
    public static new Texture2D GlowTexture => _GlowTexture.Value;

    [LoadTexture(LegendaryNightHandler.LegendaryNightEntityPath + "FallingStar_MediumGlow")]
    private static Asset<Texture2D> _MediumGlowTexture;
    public static Texture2D MediumGlowTexture => _MediumGlowTexture.Value;

    [LoadTexture(LegendaryNightHandler.LegendaryNightEntityPath + "FallingStar_Hard")]
    private static Asset<Texture2D> _HardTexture;
    public static Texture2D HardTexture => _HardTexture.Value;

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
        if (Timer1 > AccelerationStartTime)
            Projectile.velocity *= 1.02f;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        DrawFallingStar(Projectile.Center, Projectile.rotation, Projectile.scale, Projectile.velocity, trailType: TrailType, useMediumTexture: true, oldPositions: Projectile.oldPos, trailOffset: Projectile.Size * 0.5f);
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
    }

    protected override void ReceiveAnomalyExtraAI(BinaryReader reader)
    {
        base.ReceiveAnomalyExtraAI(reader);
        TrailType = reader.ReadInt32();
        AccelerationStartTime = reader.ReadInt32();
    }

    public static void DrawFallingStar(Vector2 position, float rotation, float scale, Vector2 velocity, int trailType = 0, bool useMediumTexture = false, Vector2[] oldPositions = null, Vector2 trailOffset = default)
    {
        const float MediumToLargeScaleFactor = 2.55f; // 大型落星与中型落星的缩放比例

        position -= Main.screenPosition;

        float timeForVisualEffects = (float)Main.timeForVisualEffects / 60f;

        switch (trailType)
        {
            case 1:
                Texture2D lightTexture = LightTexture;
                Vector2 lightOrigin = new(lightTexture.Width / 2f, lightTexture.Height * 0.2f);
                Color lightColor = (Color.Blue * 0.25f) with { A = 0 };
                float trailRotation = velocity.ToRotation(MathHelper.PiOver2);
                float lightScale = scale * 7.5f;
                if (useMediumTexture)
                    lightScale /= MediumToLargeScaleFactor;
                PolarVector2 offset = new(12f * lightScale, MathHelper.TwoPi * timeForVisualEffects - MathHelper.PiOver2);
                Main.spriteBatch.Draw(lightTexture, position + offset, null, lightColor, trailRotation, lightOrigin, lightScale * 1.5f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(lightTexture, position + offset.RotatedBy(TOMathUtils.PiOver3 * 2), null, lightColor, trailRotation, lightOrigin, lightScale * 1.1f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(lightTexture, position + offset.RotatedBy(TOMathUtils.PiOver3 * 4), null, lightColor, trailRotation, lightOrigin, lightScale * 1.3f, SpriteEffects.None, 0f);

                for (float phaseOffset = 0f; phaseOffset < 1f; phaseOffset += 0.5f)
                {
                    float pulsePhase = timeForVisualEffects % 0.5f / 0.5f;
                    pulsePhase = (pulsePhase + phaseOffset) % 1f;

                    float pulseOpacity = pulsePhase * 2f;
                    if (pulseOpacity > 1f)
                        pulseOpacity = 2f - pulseOpacity;

                    Main.spriteBatch.Draw(lightTexture, position - velocity * 0.7f, null, (Color.White * 0.2f) with { A = 0 } * pulseOpacity, trailRotation, lightOrigin, lightScale * (0.3f + pulsePhase * 0.5f), SpriteEffects.None, 0f);
                }
                break;
            case 2:
                Texture2D bloom = ParticleHandler.GetTexture<BloomParticle>();
                float sizeFactor = 4f * (useMediumTexture ? scale / MediumToLargeScaleFactor : scale);
                float speed = velocity.Length();
                if (oldPositions is null)
                    break;

                int trailSamples = oldPositions.Length;

                for (int i = trailSamples - 1; i >= 0; i--)
                {
                    if (oldPositions[i] == Vector2.Zero)
                        continue;
                    float progress = (float)i / trailSamples;
                    float strength = 1f - progress;
                    Vector2 trailPosition = oldPositions[i] + trailOffset - Main.screenPosition;
                    Vector2 nextPosition = i == 0 ? position
                        : oldPositions[i - 1] != Vector2.Zero ? oldPositions[i - 1] + trailOffset - Main.screenPosition : trailPosition;
                    Vector2 segment = nextPosition - trailPosition;
                    float segmentLength = segment.Length();
                    float segmentRotation = segment.ToRotation();
                    Vector2 segmentCenter = (trailPosition + nextPosition) * 0.5f;
                    float diameter = MathHelper.Lerp(12f, 240f, strength) * sizeFactor;
                    float intensity = strength * strength * MathHelper.Clamp(speed / 2f, 0f, 1f);
                    Color outerColor = new Color(255, 90, 10, 0) * (intensity * 0.12f);
                    Color innerColor = new Color(255, 225, 100, 0) * (intensity * 0.18f);
                    // 将每个光斑置于相邻采样点中间，沿轨迹拉长；额外重叠抵消光斑边缘的淡出。
                    // 内芯也独立补足线段长度，避免随外层一起缩小后再次出现断点。
                    Vector2 outerScale = new((diameter + segmentLength * 2f) / bloom.Width, diameter / bloom.Height);
                    float innerDiameter = diameter * 0.45f;
                    Vector2 innerScale = new((innerDiameter + segmentLength * 2f) / bloom.Width, innerDiameter / bloom.Height);
                    Main.spriteBatch.DrawFromCenter_VectorScale(bloom, segmentCenter, null, outerColor, segmentRotation, outerScale);
                    Main.spriteBatch.DrawFromCenter_VectorScale(bloom, segmentCenter, null, innerColor, segmentRotation, innerScale);
                }
                break;
        }

        Texture2D starTexture = useMediumTexture ? MediumTexture : StarTexture;

        // 计算五角星材质的几何中心位置。
        // 注意：SpriteBatch.Draw 中的 origin 参数应以未缩放的纹理像素为单位传入，
        // 因此这里使用 texture.Width/Height（未乘 scale）。
        float cosPiOver5 = MathF.Cos(TOMathUtils.PiOver5);
        Vector2 starOrigin = new(starTexture.Width / 2f, starTexture.Height / (1f + cosPiOver5));

        //TODrawUtils.DrawBorderTexture(Main.spriteBatch, starTexture, position, null, LegendaryNightHandler.StarColor, projectile.rotation, starOrigin, projectile.scale, way: 15, borderWidth: 3f);

        Texture2D glowTexture = useMediumTexture ? MediumGlowTexture : GlowTexture;
        for (int i = 0; i < 10; i++)
        {
            Color color = (Color.Yellow * 0.008f * MathF.Pow(i, 1.3f)) with { A = 0 };
            Main.spriteBatch.Draw(glowTexture, position, null, color, rotation, starOrigin, scale * (1.6f - i * 0.06f), SpriteEffects.None, 0f);
        }

        Main.spriteBatch.Draw(starTexture, position, null, Color.White, rotation, starOrigin, scale, SpriteEffects.None, 0f);
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
