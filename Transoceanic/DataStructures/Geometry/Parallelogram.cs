// Developed by ColdsUx

namespace Transoceanic.DataStructures.Geometry;

/// <summary>
/// 表示一个二维平行四边形，由中心点和两个从中心指向相邻顶点的向量定义。
/// </summary>
/// <remarks>向量 <c>VectorA</c> 和 <c>VectorB</c> 不能共线，否则将无法构成有效的平行四边形。</remarks>
public struct Parallelogram : IEquatable<Parallelogram>, ICoordinateTransformable<Parallelogram>, ICollidableWithRectangle
{
    /// <summary>
    /// 平行四边形的中心点坐标。
    /// </summary>
    public Vector2 Center;

    /// <summary>
    /// 从中心指向一个顶点的向量。
    /// </summary>
    public Vector2 VectorA;

    /// <summary>
    /// 从中心指向另一个相邻顶点的向量。
    /// </summary>
    public Vector2 VectorB;

    /// <summary>
    /// 使用指定的中心点和两个向量初始化 <see cref="Parallelogram"/> 结构的新实例。
    /// </summary>
    /// <param name="center">平行四边形的中心点。</param>
    /// <param name="vectorA">从中心指向一个顶点的向量。</param>
    /// <param name="vectorB">从中心指向另一个相邻顶点的向量。</param>
    /// <remarks>注意：该构造函数不会检查 <paramref name="vectorA"/> 与 <paramref name="vectorB"/> 是否共线，可能会构造出退化的平行四边形。</remarks>
    public Parallelogram(Vector2 center, Vector2 vectorA, Vector2 vectorB)
    {
        Center = center;
        VectorA = vectorA;
        VectorB = vectorB;
    }

    /// <summary>
    /// 使用指定的中心坐标分量和两个向量的分量初始化 <see cref="Parallelogram"/> 结构的新实例。
    /// </summary>
    /// <param name="centerX">中心点的 X 坐标。</param>
    /// <param name="centerY">中心点的 Y 坐标。</param>
    /// <param name="vectorAX">向量 A 的 X 分量。</param>
    /// <param name="vectorAY">向量 A 的 Y 分量。</param>
    /// <param name="vectorBX">向量 B 的 X 分量。</param>
    /// <param name="vectorBY">向量 B 的 Y 分量。</param>
    public Parallelogram(float centerX, float centerY, float vectorAX, float vectorAY, float vectorBX, float vectorBY)
        : this(new Vector2(centerX, centerY), new Vector2(vectorAX, vectorAY), new Vector2(vectorBX, vectorBY)) { }

    /// <summary>
    /// 获取平行四边形的四个顶点。
    /// </summary>
    /// <remarks>四个顶点依次为 <c>Center + VectorA</c>、<c>Center + VectorB</c>、<c>Center - VectorA</c>、<c>Center - VectorB</c>。</remarks>
    public readonly (Vector2, Vector2, Vector2, Vector2) Vertices => (Center + VectorA, Center + VectorB, Center - VectorA, Center - VectorB);

    /// <summary>
    /// 获取平行四边形的四条边。
    /// </summary>
    /// <remarks>第 <c>n</c> 条边为 <see cref="Vertices"/> 的第 <c>n</c>，<c>n + 1</c> 个元素（按模4理解）的连线。</remarks>
    public readonly (LineSegment, LineSegment, LineSegment, LineSegment) Sides
    {
        get
        {
            (Vector2 a, Vector2 b, Vector2 c, Vector2 d) = Vertices;
            return (new LineSegment(a, b), new LineSegment(b, c), new LineSegment(c, d), new LineSegment(d, a));
        }
    }

    /// <summary>
    /// 获取平行四边形的面积。
    /// </summary>
    /// <remarks>面积计算公式为 <c>2 * |VectorA × VectorB|</c>。</remarks>
    public readonly float Area => 2f * Math.Abs(Vector2.Cross(VectorA, VectorB));

    /// <summary>
    /// 获取一个值，指示平行四边形是否退化（即两个向量共线）。
    /// </summary>
    public readonly bool IsDegenerate => Vector2.Cross(VectorA, VectorB) == 0f;

    public readonly bool Equals(Parallelogram other) => Center == other.Center && VectorA == other.VectorA && VectorB == other.VectorB;
    public override readonly bool Equals(object obj) => obj is Parallelogram other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(Center, VectorA, VectorB);
    public static bool operator ==(Parallelogram left, Parallelogram right) => left.Equals(right);
    public static bool operator !=(Parallelogram left, Parallelogram right) => !(left == right);

    /// <summary>
    /// 返回当前平行四边形的字符串表示形式。
    /// </summary>
    /// <returns>
    /// 一个格式为 <c>"Parallelogram { Center: {X:0 Y:0}, VectorA: {X:0 Y:0}, VectorB: {X:0 Y:0} }"</c> 的字符串，
    /// 其中每个 <see cref="Vector2"/> 使用其默认格式输出。
    /// </returns>
    public override readonly string ToString() => $"Parallelogram {{ Center: {Center}, VectorA: {VectorA}, VectorB: {VectorB} }}";

    /// <summary>
    /// 检查平行四边形是否包含一个点。
    /// </summary>
    /// <param name="point">待检测点</param>
    /// <returns>平行四边形是否包含一个点。</returns>
    /// <remarks>调用前必须确保平行四边形未退化。</remarks>
    /// <exception cref="InvalidOperationException">当坐标系退化时抛出。</exception>
    public readonly bool ContainsPoint(Vector2 point)
    {
        Vector2 local = new PlaneCoordinateSystem(Center, VectorA, VectorB).WorldToLocal(point);
        return local.ManhattanLength <= 1f;
    }

    public readonly Parallelogram WorldToLocal(PlaneCoordinateSystem localSystem) => new(
        localSystem.WorldToLocal(Center),
        localSystem.WorldToLocal(VectorA, false),
        localSystem.WorldToLocal(VectorB, false));

    public readonly Parallelogram LocalToWorld(PlaneCoordinateSystem localSystem) => new(
        localSystem.LocalToWorld(Center),
        localSystem.LocalToWorld(VectorA, false),
        localSystem.LocalToWorld(VectorB, false));

    public readonly bool Collides(Rectangle other) => TOMathUtils.Geometry.ParallelogramVFloatRectangleCollision(this, other);
}
