namespace Anomalies.Bosses.Skeletron.SkeletronLegend;

public sealed class FallenStar : AnomalyModProjectile
{
    public override string LocalizationCategory => "Bosses.Skeletron.Legend";

    public override void SetDefaults()
    {
        Projectile.width = 250;
        Projectile.height = 250;
        Projectile.friendly = true;
        Projectile.hostile = true;
        Projectile.timeLeft = 600;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
    }

    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, LegendShockwave.FallenStarColor.ToVector3());
        Projectile.rotation += 0.2f * Projectile.direction;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = Projectile.Texture;

        // 计算五角星材质的几何中心位置。
        // 注意：SpriteBatch.Draw 中的 origin 参数应以未缩放的纹理像素为单位传入，
        // 因此这里使用 texture.Width/Height（未乘 scale）。
        float cosPiOver5 = MathF.Cos(TOMathUtils.PiOver5);
        float originX = texture.Width / 2f;
        float originY = texture.Height / (1f + cosPiOver5);
        Vector2 origin = new(originX, originY);

        Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        Vector2 center = Projectile.Center;
        float scale = Projectile.scale;
        float rotation = Projectile.rotation;

        //第一步：圆形包围盒检测
        float outerRadius = 190f * scale; //落星的外接圆半径为 190 * scale
        if (!new Circle(center, outerRadius).Collides(targetHitbox))
            return false;

        rotation += MathHelper.PiOver2; //标准落星对应的正五边形是一个顶点朝下的，所以需在旋转角度上加上 π / 2，表示顺时针旋转 90 度。

        //第二步：正五边形检测
        float sideLength = 114f * scale;
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
