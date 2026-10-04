namespace Transoceanic.DataStructures.Geometry;

/// <summary>
/// 表示一个由三个点确定的二维三角形。
/// </summary>
public struct Triangle : IEquatable<Triangle>, ICollidableWithRectangle
{
    /// <summary>
    /// 三角形的第一个点。
    /// </summary>
    public Vector2 PointA;

    /// <summary>
    /// 三角形的第二个点。
    /// </summary>
    public Vector2 PointB;

    /// <summary>
    /// 三角形的第三个点。
    /// </summary>
    public Vector2 PointC;

    /// <summary>
    /// 使用三个点初始化 <see cref="Triangle"/>。
    /// </summary>
    public Triangle(Vector2 a, Vector2 b, Vector2 c)
    {
        PointA = a;
        PointB = b;
        PointC = c;
    }

    /// <summary>
    /// 从中心点和指向顶点 A 的极坐标向量构造一个正三角形（等边三角形）。
    /// <para>参数 centerToA 表示从中心指向顶点 A 的 PolarVector2。</para>
    /// </summary>
    public static Triangle CreateEquilateral(Vector2 center, PolarVector2 centerToA)
    {
        // 另两个顶点相对于顶点 A 旋转 ±120°（±2π/3）
        float twoThirdsPi = TOMathUtils.PiOver3 * 2f;
        Vector2 a = center + centerToA;
        Vector2 b = center + centerToA.RotatedBy(twoThirdsPi);
        Vector2 c = center + centerToA.RotatedBy(-twoThirdsPi);
        return new Triangle(a, b, c);
    }

    public readonly bool Equals(Triangle other) => PointA == other.PointA && PointB == other.PointB && PointC == other.PointC;
    public override readonly bool Equals(object obj) => obj is Triangle other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(PointA, PointB, PointC);
    public static bool operator ==(Triangle left, Triangle right) => left.Equals(right);
    public static bool operator !=(Triangle left, Triangle right) => !(left == right);

    public override readonly string ToString() => $"Triangle {{ A: {PointA}, B: {PointB}, C: {PointC} }}";

    /// <summary>
    /// 判断当前三角形是否与指定矩形相交。
    /// 使用分离轴定理：测试矩形的两个对齐轴（UnitX, UnitY）以及三角形三条边的外法线方向。
    /// 注意：不会检测三角形是否退化（共线或重复点）。
    /// </summary>
    public readonly bool Collides(Rectangle other)
    {
        Span<Vector2> tri = [PointA, PointB, PointC];
        ReadOnlySpan<Vector2> rect = [other.TopLeft(), other.TopRight(), other.BottomLeft(), other.BottomRight()];

        // 测试矩形对齐轴
        if (!TOMathUtils.Geometry.OverlapOnAxis(Vector2.UnitX, tri, rect))
            return false;
        if (!TOMathUtils.Geometry.OverlapOnAxis(Vector2.UnitY, tri, rect))
            return false;

        // 测试三角形三条边的分离轴（边的外法线）
        for (int i = 0; i < 3; i++)
        {
            Vector2 p0 = tri[i];
            Vector2 p1 = tri[(i + 1) % 3];
            Vector2 edge = p1 - p0;

            // 外法线
            Vector2 axis = new(edge.Y, -edge.X);

            // 如果边退化为零向量，则跳过该轴（不增加额外测试）
            if (axis == Vector2.Zero)
                continue;

            axis = Vector2.Normalize(axis);

            if (!TOMathUtils.Geometry.OverlapOnAxis(axis, tri, rect))
                return false;
        }

        // 所有轴均重叠，则发生碰撞
        return true;
    }
}
