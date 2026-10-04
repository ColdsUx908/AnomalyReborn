namespace Transoceanic.DataStructures.Geometry;

/// <summary>
/// 表示一个二维正五边形。
/// </summary>
public struct Pentagon : IEquatable<Pentagon>, ICollidableWithRectangle
{
    /// <summary>
    /// 五边形的中心坐标。
    /// </summary>
    public Vector2 Center;

    /// <summary>
    /// 五边形的半径。
    /// </summary>
    public float CircumRadius;

    /// <summary>
    /// 五边形的旋转。
    /// <br/>表示五边形绕其中心点的旋转角度。范围为 [0, 2π/5) 弧度，其中 0 度表示一个顶点朝右。
    /// </summary>
    public float Rotation
    {
        get;
        set => field = TOMathUtils.NormalizeWithPeriod(value, TOMathUtils.PiOver5 * 2f);
    }

    /// <summary>
    /// 使用指定的中心坐标和半径初始化 <see cref="Pentagon"/> 结构的新实例。
    /// </summary>
    /// <param name="center">五边形的中心坐标。</param>
    /// <param name="radius">五边形的半径。</param>
    /// <param name="rotation">五边形的旋转。</param>
    public Pentagon(Vector2 center, float radius, float rotation)
    {
        Center = center;
        CircumRadius = radius;
        Rotation = rotation;
    }

    public readonly bool Equals(Pentagon other) => Center == other.Center && CircumRadius == other.CircumRadius && Rotation == other.Rotation;
    public override readonly bool Equals(object obj) => obj is Pentagon other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(Center, CircumRadius, Rotation);
    public static bool operator ==(Pentagon left, Pentagon right) => left.Equals(right);
    public static bool operator !=(Pentagon left, Pentagon right) => !(left == right);

    /// <summary>
    /// 返回当前五边形的字符串表示形式。
    /// </summary>
    /// <returns>
    /// 一个格式为 <c>"Pentagon { Center: {X:0 Y:0}, Radius:5, Rotation:0 }"</c> 的字符串。
    /// </returns>
    public override readonly string ToString() => $"Pentagon {{ Center: {Center}, Radius: {CircumRadius}, Rotation: {Rotation} }}";

    /// <summary>
    /// 获取五边形的五个顶点坐标，按顺时针（或逆时针，取决于 Rotation 方向）顺序返回。
    /// </summary>
    public readonly (Vector2 V0, Vector2 V1, Vector2 V2, Vector2 V3, Vector2 V4) Vertices
    {
        get
        {
            float step = TOMathUtils.PiOver5 * 2f; // 2π/5
            Vector2 v0 = Center + new PolarVector2(CircumRadius, Rotation);
            Vector2 v1 = Center + new PolarVector2(CircumRadius, Rotation + step);
            Vector2 v2 = Center + new PolarVector2(CircumRadius, Rotation + 2 * step);
            Vector2 v3 = Center + new PolarVector2(CircumRadius, Rotation + 3 * step);
            Vector2 v4 = Center + new PolarVector2(CircumRadius, Rotation + 4 * step);
            return (v0, v1, v2, v3, v4);
        }
    }

    public readonly bool Collides(Rectangle other)
    {
        const int count = 5;
        Span<Vector2> poly = stackalloc Vector2[count];
        float step = TOMathUtils.PiOver5 * 2f; // 2π/5
        float half = TOMathUtils.PiOver5; // π/5

        for (int i = 0; i < count; i++)
        {
            float angle = Rotation + i * step;
            poly[i] = Center + new PolarVector2(CircumRadius, angle);
        }

        ReadOnlySpan<Vector2> rect = [other.TopLeft(), other.TopRight(), other.BottomLeft(), other.BottomRight()];

        // 测试矩形对齐轴
        if (!TOMathUtils.Geometry.OverlapOnAxis(Vector2.UnitX, poly, rect))
            return false;
        if (!TOMathUtils.Geometry.OverlapOnAxis(Vector2.UnitY, poly, rect))
            return false;

        // 测试五条边的分离轴（边法线方向）
        for (int i = 0; i < count; i++)
        {
            float axisAngle = Rotation + half + i * step;
            Vector2 axis = new PolarVector2(axisAngle);
            if (!TOMathUtils.Geometry.OverlapOnAxis(axis, poly, rect))
                return false;
        }

        // 所有轴均重叠，则发生碰撞
        return true;
    }

    public readonly bool Collides(Pentagon other)
    {
        const int count = 5;
        Span<Vector2> vertsA = stackalloc Vector2[count];
        Span<Vector2> vertsB = stackalloc Vector2[count];
        float step = TOMathUtils.PiOver5 * 2f;
        float half = TOMathUtils.PiOver5;

        for (int i = 0; i < count; i++)
        {
            float angleA = Rotation + i * step;
            float angleB = other.Rotation + i * step;
            vertsA[i] = Center + new PolarVector2(CircumRadius, angleA);
            vertsB[i] = other.Center + new PolarVector2(other.CircumRadius, angleB);
        }

        // 测试当前五边形的五条分离轴（边法线方向）
        for (int i = 0; i < count; i++)
        {
            float axisAngle = Rotation + half + i * step;
            Vector2 axis = new PolarVector2(axisAngle);
            if (!TOMathUtils.Geometry.OverlapOnAxis(axis, vertsA, vertsB))
                return false;
        }

        // 测试另一个五边形的五条分离轴
        for (int i = 0; i < count; i++)
        {
            float axisAngle = other.Rotation + half + i * step;
            Vector2 axis = new PolarVector2(axisAngle);
            if (!TOMathUtils.Geometry.OverlapOnAxis(axis, vertsA, vertsB))
                return false;
        }

        // 所有轴均重叠，则发生碰撞
        return true;
    }

    public readonly bool Contains(Rectangle other)
    {
        float apothem = CircumRadius * MathF.Cos(TOMathUtils.PiOver5);
        ReadOnlySpan<Vector2> adjustedCorners =
        [
            other.TopLeft() - Center,
            other.TopRight() - Center,
            other.BottomLeft() - Center,
            other.BottomRight() - Center
        ];

        const int count = 5;
        float step = TOMathUtils.PiOver5 * 2f;
        float half = TOMathUtils.PiOver5;

        // 五条边的外法线方向
        for (int i = 0; i < count; i++)
        {
            float normalAngle = Rotation + half + i * step;
            Vector2 outwardNormal = new PolarVector2(normalAngle);

            foreach (Vector2 corner in adjustedCorners)
            {
                float projection = Vector2.Dot(corner, outwardNormal);

                // 超出五边形边界
                if (projection > apothem)
                    return false;
            }
        }

        return true;
    }
}
