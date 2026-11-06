namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class LegendBlueStar
{
    /// <summary>物质喷发间隔，单位为游戏帧。</summary>
    private const int MatterEmissionInterval = 3;
    /// <summary>物质流。</summary>
    private const int MatterStreams = 3;
    /// <summary>物质最大存活时间，单位为游戏帧。</summary>
    private const int MatterMaximumLifetime = 300;
    /// <summary>物质重力，以世界像素和游戏帧计量。</summary>
    private const float MatterGravity = 650000f;
    /// <summary>物质角动量损失。</summary>
    private const float MatterAngularMomentumLoss = 0.012f;
    private List<StellarMatterTrack> _CompanionMatter;

    private sealed record StellarMatterTrack(int BirthAge, Vector3[] Positions);

    /// <summary>计算物质轨迹。</summary>
    internal static Vector3[] CreateMatterTrajectory(float birthAge, int lane)
    {
        const float SurfaceRadius = SkeletronLegendHead.Mechanic6Parameters.SphereRadius;
        const float BlueRadius = SurfaceRadius * CompanionRadiusRatio;
        Vector3 donor = CompanionOrbitPosition(birthAge);
        Vector3 outward = Vector3.Normalize(donor);
        Vector3 position = donor - outward * BlueRadius;
        position.Y = MathF.Sin(birthAge * 1.71f + lane * 2.1f) * BlueRadius * 0.22f;
        // 从朝向主星的球面逸出，继承公转速度和自转切向速度，不使用直线追踪。
        Vector3 velocity = (CompanionOrbitPosition(birthAge + 0.5f) - CompanionOrbitPosition(birthAge - 0.5f)) * 0.82f;
        velocity += new Vector3(-outward.Z, 0f, outward.X) * (lane - 1) * 0.7f - outward * 1.2f;
        List<Vector3> positions = new(MatterMaximumLifetime + 1) { position };
        for (int frame = 0; frame < MatterMaximumLifetime; frame++)
        {
            // 中心引力+耗散近似：气体保留切向角动量，在流体阻力/碰撞下逐圈内落。
            // 两个半帧的半隐式Euler步，避免近星表面加速度过大导致穿越。
            for (int step = 0; step < 2; step++)
            {
                float radius = Math.Max(position.Length(), SurfaceRadius);
                Vector3 acceleration = -position * (MatterGravity / (radius * radius * radius));
                velocity += (acceleration - velocity * MatterAngularMomentumLoss) * 0.5f;
                Vector3 next = position + velocity * 0.5f;
                if (next.LengthSquared() <= SurfaceRadius * SurfaceRadius)
                {
                    // 求首次球面交点，吸积流准确结束在光球层，不能穿进被遮盖的星核。
                    Vector3 delta = next - position;
                    float a = delta.LengthSquared();
                    float b = 2f * Vector3.Dot(position, delta);
                    float c = position.LengthSquared() - SurfaceRadius * SurfaceRadius;
                    float fraction = a > 0f ? MathHelper.Clamp((-b - MathF.Sqrt(Math.Max(0f, b * b - 4f * a * c))) / (2f * a), 0f, 1f) : 0f;
                    positions.Add(position + delta * fraction);
                    return positions.ToArray();
                }
                position = next;
            }
            positions.Add(position);
        }
        return positions.ToArray();
    }

    private void PrepareCompanionMatter(int time)
    {
        if (_CompanionMatter is not null || time < SkeletronLegendHead.Mechanic6Timeline.CompanionStartMusicTime || time > SkeletronLegendHead.Mechanic6Timeline.CompanionImpactMusicTime + MatterMaximumLifetime)
            return;
        _CompanionMatter = new List<StellarMatterTrack>();
        const int Duration = SkeletronLegendHead.Mechanic6Timeline.CompanionImpactMusicTime - SkeletronLegendHead.Mechanic6Timeline.CompanionStartMusicTime;
        for (int birth = 1; birth < Duration; birth += MatterEmissionInterval)
        {
            for (int lane = 0; lane < MatterStreams; lane++)
                _CompanionMatter.Add(new(birth, CreateMatterTrajectory(birth, lane)));
        }
    }
}