namespace Anomalies.SpecialContents.Developer.EventideReunion;

// 世界像素坐标下的几何；渲染、命中检测及不同长宽比共用同一尺度。
public static class EventideReunionSunGeometry
{
    public const float SweepOuterRadius = 400f;
    public const float SlashProjectionRadius = 450f;

    public static Vector2 SlashVector(float progress, Vector2 aim, int facing, float gravity)
    {
        // ThunderSpear.Vector2Elipse2：圆周绕 X 倾斜、绕 Z 瞄准，再做透视投影。
        float t = Math.Clamp(progress, 0f, 1f);
        float eased = t * t * (3f - 2f * t);
        float angle = (-1.4f + 2.8f * eased) * facing * gravity;
        Vector3 local = new(MathF.Cos(angle) * SlashProjectionRadius, MathF.Sin(angle) * SlashProjectionRadius, 0f);
        local = Vector3.Transform(local, Matrix.CreateRotationX(-1.2f));
        local = Vector3.Transform(local, Matrix.CreateRotationZ(MathF.Atan2(aim.Y, aim.X)));
        return new Vector2(local.X, local.Y) * (1000f / (1000f - local.Z)) / 240f;
    }

    public static float SlashArmOffset(float progress)
    {
        float t = Math.Clamp(progress / 0.25f, 0f, 1f);
        return -22f + 70f * t * t * (3f - 2f * t);
    }

    public static Vector2 SweepVector(float progress, int facing, float gravity)
    {
        float angle = progress >= 1f ? 0f : Math.Clamp(progress, 0f, 1f) * MathF.Tau;
        float depth = MathF.Sin(angle) * 0.95f;
        float perspective = 900f / (900f - depth * 250f);
        return new Vector2(MathF.Cos(angle) * facing, MathF.Sin(angle) * 0.32f * gravity) * perspective;
    }

    public static Vector2 DashMotion(Vector2 direction, float speed, Vector2 fallback)
    {
        if (!float.IsFinite(direction.X) || !float.IsFinite(direction.Y) || direction.LengthSquared() < 0.000001f)
            direction = fallback;
        if (!float.IsFinite(direction.X) || !float.IsFinite(direction.Y) || direction.LengthSquared() < 0.000001f)
            direction = Vector2.UnitY;
        direction /= Math.Max(Math.Abs(direction.X), Math.Abs(direction.Y));
        return Vector2.Normalize(direction) * speed;
    }

    public static float CollisionTravelFraction(Vector2 motion, Vector2 safeMotion)
    {
        // 碰到任意一轴的障碍就沿原方向停止，不能把另一轴剩余位移变成贴墙滑动。
        float fraction = 1f;
        if (Math.Abs(motion.X) > 0.00001f)
            fraction = Math.Min(fraction, safeMotion.X / motion.X);
        if (Math.Abs(motion.Y) > 0.00001f)
            fraction = Math.Min(fraction, safeMotion.Y / motion.Y);
        return Math.Clamp(fraction, 0f, 1f);
    }

    public static Vector2 SweepDash(Vector2 position, Vector2 motion,
        Func<Vector2, Vector2, Vector2> collide, out bool hitTerrain)
    {
        // 每次最多推进半格，确保单格墙体也会落在碰撞检测覆盖的路径内。
        const float MaxStep = 8f;
        int steps = Math.Max(1, (int)MathF.Ceiling(motion.Length() / MaxStep));
        Vector2 step = motion / steps;
        hitTerrain = false;
        for (int i = 0; i < steps; i++)
        {
            Vector2 safeStep = collide(position, step);
            if (Vector2.DistanceSquared(step, safeStep) > 0.000001f)
            {
                position += step * CollisionTravelFraction(step, safeStep);
                hitTerrain = true;
                break;
            }
            position += step;
        }
        return position;
    }

}
