namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class LegendaryNightPhase2Arena
{
    public bool CentralStarCollides(Rectangle hitbox)
    {
        Pentagon core = GetCentralPentagon(Projectile.Center, MainCircleRadius, MagicCircleRotation);
        if (core.Collides(hitbox))
            return true;
        for (int i = 0; i < 5; i++)
        {
            float angle = MagicCircleRotation + MathHelper.TwoPi * i / 5f;
            Vector2 firstInner = Projectile.Center + (angle + MathHelper.Pi / 5f).ToRotationVector2() * core.CircumRadius;
            Vector2 secondInner = Projectile.Center + (angle - MathHelper.Pi / 5f).ToRotationVector2() * core.CircumRadius;
            Triangle triangle = new(GetStarVertex(0, i), firstInner, secondInner);
            if (triangle.Collides(hitbox))
                return true;
        }
        return false;
    }

    public (Vector2 Center, float Radius, float Rotation) GetMagicCircle(int index)
    {
        float absorb = MasterBehavior is SkeletronLegendHead head && head.StellarMechanicsActive
            ? SkeletronLegendHead.Mechanic6Parameters.AbsorbProgress(head.Phase2MusicElapsedTime) : 0f;
        Vector2 destination = MasterBehavior is SkeletronLegendHead owner
            ? owner.NPC.Center + SkeletronLegendHead.Mechanic6Parameters.SphereOffset(owner.Phase2MusicElapsedTime) : Projectile.Center;
        float originalRadius = MainCircleFullRadius * AppearanceScale;
        if (index == 0)
            return (Vector2.Lerp(Projectile.Center, destination, absorb), originalRadius * (1f - absorb), MagicCircleRotation);
        // 同向公转；交汇位置由音乐时钟和同步初始角确定。
        float spinSpeed = index == 1 ? FirstSmallCircleSpinSpeed : SecondSmallCircleSpinSpeed;
        float angle = GetOrbitAngle(index, MasterBehavior?.Phase2MusicElapsedTime ?? AppearanceStartMusicTime);
        Vector2 center = Projectile.Center + angle.ToRotationVector2() * (originalRadius * (1f + SmallCircleRadiusRatio));
        return (Vector2.Lerp(center, destination, absorb), originalRadius * SmallCircleRadiusRatio * (1f - absorb), MusicAge * spinSpeed);
    }

    public Vector2 GetStarVertex(int circleIndex, int vertexIndex)
    {
        (Vector2 Center, float Radius, float Rotation) circle = GetMagicCircle(circleIndex);
        return circle.Center + (circle.Rotation + MathHelper.TwoPi * vertexIndex / 5f).ToRotationVector2() * circle.Radius;
    }
    public static bool MagicCircleCollides(Vector2 center, float radius, float rotation, Rectangle hitbox, bool includeCircle = true)
    {
        if (includeCircle && new Annulus(center, radius - LaserWidth * 0.5f, radius + LaserWidth * 0.5f).Collides(hitbox))
            return true;
        for (int i = 0; i < 5; i++)
        {
            Vector2 start = center + (rotation + MathHelper.TwoPi * i / 5f).ToRotationVector2() * radius;
            Vector2 end = center + (rotation + MathHelper.TwoPi * (i + 2) / 5f).ToRotationVector2() * radius;
            RotatedRectangle laser = RotatedRectangle.FromLineSegment(start, end, LaserWidth);
            if (laser.Collides(hitbox))
                return true;
        }
        return false;
    }
    public static Pentagon GetCentralPentagon(Vector2 center, float radius, float rotation) => new(center,
        radius * MathF.Cos(MathHelper.TwoPi / 5f) / MathF.Cos(MathHelper.Pi / 5f), rotation + MathHelper.Pi / 5f);
}