namespace Transoceanic.Primitives.Physics;

/// <summary>
/// 具有可锁定端点的 Verlet 质点，可用于绳索、触须和装饰链。
/// </summary>
public class VerletSimulatedSegment
{
    public Vector2 Position;
    public Vector2 OldPosition;
    public bool Locked;

    public VerletSimulatedSegment(Vector2 position, bool locked = false)
    {
        Position = position;
        OldPosition = position;
        Locked = locked;
    }

    /// <summary>
    /// 积分质点并迭代约束相邻距离。inertia 为速度保留比例，默认保持原有运动。
    /// </summary>
    public static List<VerletSimulatedSegment> SimpleSimulation(List<VerletSimulatedSegment> segments, float segmentDistance, int loops = 10, float gravity = 0.3f, float inertia = 1f)
    {
        ArgumentNullException.ThrowIfNull(segments);
        if (!float.IsFinite(segmentDistance) || segmentDistance < 0f)
            throw new ArgumentOutOfRangeException(nameof(segmentDistance));
        ArgumentOutOfRangeException.ThrowIfNegative(loops);
        if (!float.IsFinite(gravity))
            throw new ArgumentOutOfRangeException(nameof(gravity));
        if (!float.IsFinite(inertia) || inertia < 0f || inertia > 1f)
            throw new ArgumentOutOfRangeException(nameof(inertia));
        foreach (VerletSimulatedSegment segment in segments)
        {
            if (!segment.Locked)
            {
                Vector2 positionBeforeUpdate = segment.Position;

                segment.Position += (segment.Position - segment.OldPosition) * inertia;
                segment.Position += Vector2.UnitY * gravity;

                segment.OldPosition = positionBeforeUpdate;
            }
        }

        int segmentCount = segments.Count;

        for (int k = 0; k < loops; k++)
        {
            for (int j = 0; j < segmentCount - 1; j++)
            {
                VerletSimulatedSegment pointA = segments[j];
                VerletSimulatedSegment pointB = segments[j + 1];
                Vector2 segmentCenter = (pointA.Position + pointB.Position) / 2f;
                Vector2 difference = pointA.Position - pointB.Position;
                Vector2 segmentDirection = difference == Vector2.Zero || float.IsNaN(difference.X) || float.IsNaN(difference.Y)
                    ? Vector2.UnitY
                    : Vector2.Normalize(difference);

                if (!pointA.Locked)
                    pointA.Position = segmentCenter + segmentDirection * segmentDistance / 2f;

                if (!pointB.Locked)
                    pointB.Position = segmentCenter - segmentDirection * segmentDistance / 2f;

                segments[j] = pointA;
                segments[j + 1] = pointB;
            }
        }

        return segments;
    }
}
