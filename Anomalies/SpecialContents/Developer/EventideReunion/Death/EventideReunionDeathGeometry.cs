namespace Anomalies.SpecialContents.Developer.EventideReunion;

internal static class EventideReunionDeathGeometry
{
    public const float BladeRadius = 320f;
    public const float DashSpeed = 48f;
    public const float ExitClearance = 96f;
    public const int HitDashDuration = 240;
    public const float InnerRatio = 0.48f;

    // 椭圆挥动平面加透视缩放；武器、刀光和碰撞共用同一投影。
    public static Vector2 SlashVector(float progress, Vector2 aim, float swingSign)
    {
        float angle = MathHelper.Lerp(-1.85f, 1.85f, MathHelper.Clamp(progress, 0f, 1f)) * swingSign;
        float depth = MathF.Sin(angle) * 0.09f;
        float perspective = 1f / (1f + depth);
        Vector2 local = new(MathF.Cos(angle) * 0.92f, MathF.Sin(angle));
        float rotation = MathF.Atan2(aim.Y, aim.X);
        float cos = MathF.Cos(rotation);
        float sin = MathF.Sin(rotation);
        return new Vector2(local.X * cos - local.Y * sin, local.X * sin + local.Y * cos) * (BladeRadius * perspective);
    }

    // 不只检查帧末位置，避免高速位移越过薄敌人或地形。
    public static Vector2 SweepDash(Vector2 start, Vector2 motion,
        Func<Vector2, Vector2, Vector2> collide, Action<Vector2, Vector2> visit)
    {
        int steps = Math.Max(1, (int)MathF.Ceiling(motion.Length() / 4f));
        Vector2 position = start;
        Vector2 step = motion / steps;
        visit(position, position);
        for (int i = 0; i < steps; i++)
        {
            Vector2 actual = collide(position, step);
            Vector2 previous = position;
            position += actual;
            visit(previous, position);
            if (Vector2.DistanceSquared(actual, step) > 0.001f)
                break;
        }
        return position;
    }

    public static bool ShouldFinishDash(int timer, int hitTimer, bool overlapping, int emptyDuration) =>
        hitTimer < 0 ? timer >= emptyDuration : timer - hitTimer >= 2 && !overlapping;

    public static bool HasPassedTarget(Rectangle player, Rectangle target, Vector2 direction)
    {
        Vector2 offset = new(player.Center.X - target.Center.X, player.Center.Y - target.Center.Y);
        if (Vector2.Dot(offset, direction) <= 0f)
            return false;
        float gapX = Math.Max(0, Math.Max(target.Left - player.Right, player.Left - target.Right));
        float gapY = Math.Max(0, Math.Max(target.Top - player.Bottom, player.Top - target.Bottom));
        return gapX * gapX + gapY * gapY >= ExitClearance * ExitClearance;
    }

    public static Vector2 SlashTarget(Rectangle target, Vector2 origin) =>
        new(Math.Clamp(origin.X, target.Left, target.Right), Math.Clamp(origin.Y, target.Top, target.Bottom));
}
