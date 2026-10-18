namespace Transoceanic.DataStructures.Geometry;

/// <summary>二维正十六边形；旋转为零时首个顶点朝右。</summary>
public readonly struct Hexadecagon(Vector2 center, float circumRadius, float rotation = 0f) : ICollidableWithRectangle
{
    public const int VertexCount = 16;
    public const float VertexAngle = MathHelper.TwoPi / VertexCount;

    public Vector2 Center { get; } = center;
    public float CircumRadius { get; } = circumRadius;
    public float Rotation { get; } = rotation;
    public float Apothem => CircumRadius * MathF.Cos(VertexAngle * 0.5f);

    public Vector2 GetVertex(int index) => Center + (Rotation + index * VertexAngle).ToRotationVector2() * CircumRadius;

    private Vector2 GetNormal(int index) => (Rotation + (index + 0.5f) * VertexAngle).ToRotationVector2();

    public bool Contains(Vector2 point)
    {
        for (int i = 0; i < VertexCount; i++)
            if (Vector2.Dot(point - Center, GetNormal(i)) > Apothem)
                return false;
        return true;
    }

    /// <summary>获取区域内距离指定点最近的位置；区域内的点保持不变。</summary>
    public Vector2 ClosestPoint(Vector2 point)
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

    /// <summary>判断矩形是否严格位于内部；接触边界时返回 false。</summary>
    public bool Contains(Rectangle rectangle)
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

    public bool Collides(Rectangle rectangle)
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

    /// <summary>沿边法线向外偏移指定距离，负数表示向内收缩。</summary>
    public Hexadecagon Offset(float distance) => new(Center,
        Math.Max(0f, Apothem + distance) / MathF.Cos(VertexAngle * 0.5f), Rotation);

    /// <summary>检测矩形是否接触以多边形边界为中线、指定厚度的边框。</summary>
    public bool BoundaryCollides(Rectangle rectangle, float thickness) =>
        Offset(thickness * 0.5f).Collides(rectangle) && !Offset(-thickness * 0.5f).Contains(rectangle);
}
