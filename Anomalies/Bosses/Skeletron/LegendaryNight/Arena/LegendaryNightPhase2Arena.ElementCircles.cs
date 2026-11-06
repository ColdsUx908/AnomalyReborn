namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class LegendaryNightPhase2Arena
{
    // 由音乐绝对时钟推导，客户端和中途加入无需同步独立角度。
    public (Vector2 Center, float Radius, float Rotation) GetMechanic7Circle(int index)
    {
        float age = SkeletronLegendHead.Mechanic7Parameters.GetIntegratedOrbitTime(MasterBehavior?.Phase2MusicElapsedTime ?? 0);
        float radius = SkeletronLegendHead.Mechanic7Parameters.CircleRadius;
        float direction = index % 2 == 0 ? 1f : -1f;
        float travel = age * SkeletronLegendHead.Mechanic7Parameters.RollingSpeeds[index] * direction;
        float angle = -MathHelper.PiOver2 + index * MathHelper.TwoPi / 8f + travel;
        // 外切无滑动滚动：中心走过的弧长等于小圆自转的弧长。
        return (Projectile.Center + angle.ToRotationVector2() * (Radius + radius),
            radius, travel * (Radius + radius) / radius);
    }
}