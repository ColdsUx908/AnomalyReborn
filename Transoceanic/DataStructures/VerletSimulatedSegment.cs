namespace Transoceanic.DataStructures;

/// <summary>具有可锁定端点的 Verlet 质点，可用于绳索、触须和装饰链。</summary>
public class VerletSimulatedSegment
{
    public Vector2 position, oldPosition;
    public bool locked;

    public VerletSimulatedSegment(Vector2 _position, bool _locked = false)
    {
        position = _position;
        oldPosition = _position;
        locked = _locked;
    }

    /// <summary>积分质点并迭代约束相邻距离。inertia 为速度保留比例，默认保持原有运动。</summary>
    public static List<VerletSimulatedSegment> SimpleSimulation(List<VerletSimulatedSegment> segments, float segmentDistance, int loops = 10, float gravity = 0.3f, float inertia = 1f)
    {
        ArgumentNullException.ThrowIfNull(segments);
        if (!float.IsFinite(segmentDistance) || segmentDistance < 0f) throw new ArgumentOutOfRangeException(nameof(segmentDistance));
        if (loops < 0) throw new ArgumentOutOfRangeException(nameof(loops));
        if (!float.IsFinite(gravity)) throw new ArgumentOutOfRangeException(nameof(gravity));
        if (!float.IsFinite(inertia) || inertia < 0f || inertia > 1f) throw new ArgumentOutOfRangeException(nameof(inertia));
        foreach (VerletSimulatedSegment segment in segments)
        {
            if (!segment.locked)
            {
                Vector2 positionBeforeUpdate = segment.position;

                segment.position += (segment.position - segment.oldPosition) * inertia;
                segment.position += Vector2.UnitY * gravity;

                segment.oldPosition = positionBeforeUpdate;
            }
        }

        int segmentCount = segments.Count;

        for (int k = 0; k < loops; k++)
        {
            for (int j = 0; j < segmentCount - 1; j++)
            {
                VerletSimulatedSegment pointA = segments[j];
                VerletSimulatedSegment pointB = segments[j + 1];
                Vector2 segmentCenter = (pointA.position + pointB.position) / 2f;
                Vector2 segmentDirection = Terraria.Utils.SafeNormalize(pointA.position - pointB.position, Vector2.UnitY);

                if (!pointA.locked)
                    pointA.position = segmentCenter + segmentDirection * segmentDistance / 2f;

                if (!pointB.locked)
                    pointB.position = segmentCenter - segmentDirection * segmentDistance / 2f;

                segments[j] = pointA;
                segments[j + 1] = pointB;
            }
        }

        return segments;
    }
}
