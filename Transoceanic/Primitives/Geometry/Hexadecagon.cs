namespace Transoceanic.Primitives.Geometry;

/// <summary>
/// 二维正十六边形；旋转为零时首个顶点朝右。
/// </summary>
public struct Hexadecagon : IEquatable<Hexadecagon>, ICollidableWithRectangle
{
    /// <summary>
    /// 正十六边形的顶点数量，固定为 16。
    /// </summary>
    public const int VertexCount = 16;
    /// <summary>
    /// 正十六边形相邻顶点的中心夹角，单位为弧度。
    /// </summary>
    public const float VertexAngle = TOMathUtils.PiOver8;

    /// <summary>
    /// 形状中心坐标；须与碰撞目标使用相同坐标系和单位。
    /// </summary>
    public Vector2 Center;
    /// <summary>
    /// 正多边形的外接圆半径，单位与中心坐标一致。
    /// </summary>
    public float CircumRadius;
    /// <summary>
    /// 多边形绕中心旋转的弧度角。
    /// </summary>
    public float Rotation
    {
        get;
        set => field = TOMathUtils.NormalizeWithPeriod(value, VertexAngle);
    }

    /// <summary>
    /// 使用指定的中心坐标、外接圆半径和旋转初始化 <see cref="Hexadecagon"/> 结构的新实例。
    /// </summary>
    /// <param name="center">十六边形的中心坐标。</param>
    /// <param name="circumRadius">十六边形的外接圆半径。</param>
    /// <param name="rotation">十六边形的旋转，按 π/8 弧度归一化。</param>
    public Hexadecagon(Vector2 center, float circumRadius, float rotation = 0f)
    {
        Center = center;
        CircumRadius = circumRadius;
        Rotation = rotation;
    }

    /// <summary>
    /// 比较 Center、CircumRadius、Rotation，使用成员本身的相等语义。
    /// </summary>
    public readonly bool Equals(Hexadecagon other) => Center == other.Center && CircumRadius == other.CircumRadius && Rotation == other.Rotation;
    /// <inheritdoc/>
    public override readonly bool Equals(object obj) => obj is Hexadecagon other && Equals(other);
    /// <inheritdoc/>
    public override readonly int GetHashCode() => HashCode.Combine(Center, CircumRadius, Rotation);
    /// <summary>
    /// 按 Equals 比较 Center、CircumRadius、Rotation，相等时返回 true。
    /// </summary>
    public static bool operator ==(Hexadecagon left, Hexadecagon right) => left.Equals(right);
    /// <summary>
    /// 返回相等运算符结果的逻辑取反。
    /// </summary>
    public static bool operator !=(Hexadecagon left, Hexadecagon right) => !(left == right);

    /// <summary>
    /// 正十六边形的内切圆半径，等于 CircumRadius × cos(π/16)，单位与外接圆半径相同。
    /// </summary>
    public readonly float Apothem => CircumRadius * MathF.Cos(VertexAngle * 0.5f);

    /// <summary>
    /// 按索引计算正十六边形顶点，相邻顶点间隔 π/8 弧度；索引不作范围检查，按角度周期延拓。
    /// </summary>
    public readonly Vector2 GetVertex(int index) => Center + (Rotation + index * VertexAngle).ToRotationVector2() * CircumRadius;

    private readonly Vector2 GetNormal(int index) => (Rotation + (index + 0.5f) * VertexAngle).ToRotationVector2();

    /// <summary>
    /// 判断指定矩形的所有角点是否在当前凸多边形内部或边界上。
    /// </summary>
    public readonly bool Contains(Vector2 point)
    {
        for (int i = 0; i < VertexCount; i++)
            if (Vector2.Dot(point - Center, GetNormal(i)) > Apothem)
                return false;
        return true;
    }

    /// <summary>
    /// 获取区域内距离指定点最近的位置；区域内的点保持不变。
    /// </summary>
    public readonly Vector2 ClosestPoint(Vector2 point)
    {
        if (Contains(point))
            return point;

        Vector2 closest = Center;
        float minDistanceSquared = float.PositiveInfinity;
        for (int i = 0; i < VertexCount; i++)
        {
            Vector2 start = GetVertex(i);
            Vector2 edge = GetVertex((i + 1) % VertexCount) - start;
            float lengthSquared = edge.LengthSquared();
            float t = lengthSquared > 0f ? MathHelper.Clamp(Vector2.Dot(point - start, edge) / lengthSquared, 0f, 1f) : 0f;
            Vector2 candidate = start + edge * t;
            float distanceSquared = Vector2.DistanceSquared(point, candidate);
            if (distanceSquared < minDistanceSquared)
            {
                minDistanceSquared = distanceSquared;
                closest = candidate;
            }
        }
        return closest;
    }

    /// <summary>
    /// 判断矩形是否严格位于内部；接触边界时返回 false。
    /// </summary>
    public readonly bool Contains(Rectangle rectangle)
    {
        // 使用浮点中心，避免奇数宽高的矩形中心被截断。
        Vector2 offset = new Vector2(rectangle.X + rectangle.Width * 0.5f, rectangle.Y + rectangle.Height * 0.5f) - Center;
        for (int i = 0; i < VertexCount; i++)
        {
            Vector2 normal = GetNormal(i);
            float extent = MathF.Abs(normal.X) * rectangle.Width * 0.5f + MathF.Abs(normal.Y) * rectangle.Height * 0.5f;
            if (Vector2.Dot(offset, normal) + extent >= Apothem)
                return false;
        }
        return true;
    }

    /// <summary>
    /// 判断当前形状与指定形状是否相交；双方坐标和尺寸须使用同一坐标系和单位。
    /// </summary>
    public readonly bool Collides(Rectangle rectangle)
    {
        Span<Vector2> vertices = stackalloc Vector2[VertexCount];
        for (int i = 0; i < VertexCount; i++)
            vertices[i] = GetVertex(i);
        ReadOnlySpan<Vector2> corners = [rectangle.TopLeft(), rectangle.TopRight(), rectangle.BottomLeft(), rectangle.BottomRight()];
        if (!TOMathUtils.Geometry.OverlapOnAxis(Vector2.UnitX, vertices, corners)
            || !TOMathUtils.Geometry.OverlapOnAxis(Vector2.UnitY, vertices, corners))
            return false;
        for (int i = 0; i < VertexCount / 2; i++)
            if (!TOMathUtils.Geometry.OverlapOnAxis(GetNormal(i), vertices, corners))
                return false;
        return true;
    }

    /// <summary>
    /// 沿边法线向外偏移指定距离，负数表示向内收缩。
    /// </summary>
    public readonly Hexadecagon Offset(float distance) => new(Center,
        Math.Max(0f, Apothem + distance) / MathF.Cos(VertexAngle * 0.5f), Rotation);

    /// <summary>
    /// 检测矩形是否接触以多边形边界为中线、指定厚度的边框。
    /// </summary>
    public readonly bool BoundaryCollides(Rectangle rectangle, float thickness) =>
        Offset(thickness * 0.5f).Collides(rectangle) && !Offset(-thickness * 0.5f).Contains(rectangle);
}
