namespace Transoceanic.Primitives.Geometry;

/// <summary>
/// 表示一个二维圆形。
/// </summary>
public struct Circle : IEquatable<Circle>, ICollidableWithRectangle
{
    /// <summary>
    /// 圆心的坐标。
    /// </summary>
    public Vector2 Center;

    /// <summary>
    /// 圆的半径。
    /// </summary>
    public float Radius;

    /// <summary>
    /// 使用指定的圆心和半径初始化 <see cref="Circle"/> 结构的新实例。
    /// </summary>
    /// <param name="center">圆心的坐标。</param>
    /// <param name="radius">圆的半径。</param>
    /// <exception cref="ArgumentOutOfRangeException">半径非有限值或小于零。</exception>
    public Circle(Vector2 center, float radius)
    {
        if (!float.IsFinite(radius) || radius < 0f)
            throw new ArgumentOutOfRangeException(nameof(radius));
        Center = center;
        Radius = radius;
    }

    /// <summary>
    /// 使用指定的圆心坐标分量和半径初始化 <see cref="Circle"/> 结构的新实例。
    /// </summary>
    /// <param name="x">圆心的 X 坐标。</param>
    /// <param name="y">圆心的 Y 坐标。</param>
    /// <param name="radius">圆的半径。</param>
    public Circle(float x, float y, float radius) : this(new Vector2(x, y), radius) { }

    /// <summary>
    /// 比较 Center、Radius，使用成员本身的相等语义。
    /// </summary>
    public readonly bool Equals(Circle other) => Center == other.Center && Radius == other.Radius;
    /// <inheritdoc/>
    public override readonly bool Equals(object obj) => obj is Circle other && Equals(other);
    /// <inheritdoc/>
    public override readonly int GetHashCode() => HashCode.Combine(Center, Radius);
    /// <summary>
    /// 按 Equals 比较 Center、Radius，相等时返回 true。
    /// </summary>
    public static bool operator ==(Circle left, Circle right) => left.Equals(right);
    /// <summary>
    /// 返回相等运算符结果的逻辑取反。
    /// </summary>
    public static bool operator !=(Circle left, Circle right) => !(left == right);

    /// <inheritdoc/>
    /// <remarks>
    /// 返回当前圆的字符串表示形式。
    /// </remarks>
    /// <returns>
    /// 一个格式为 <c>"Circle { Center: {X:0 Y:0}, Radius:5 }"</c> 的字符串，
    /// 其中 <c>Center</c> 使用 <see cref="Vector2"/> 的默认格式输出，
    /// <c>Radius</c> 输出为数值形式。
    /// </returns>
    public override readonly string ToString() => $"Circle {{ Center: {Center}, Radius: {Radius} }}";

    /// <summary>
    /// 判断当前形状与指定形状是否相交；双方坐标和尺寸须使用同一坐标系和单位。
    /// </summary>
    public readonly bool Collides(Rectangle other) => TOMathUtils.Collision.FloatRectangleVCircleCollision(other, this);

    /// <summary>
    /// 单位圆，中心在原点（<see cref="Vector2.Zero"/>），半径为 <c>1</c>。
    /// </summary>
    public static readonly Circle Unit = new(Vector2.Zero, 1f);
}
