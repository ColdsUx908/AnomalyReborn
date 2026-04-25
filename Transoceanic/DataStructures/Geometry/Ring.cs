// Designed by ColdsUx

namespace Transoceanic.DataStructures.Geometry;

/// <summary>
/// 表示一个二维圆环（环形区域）。
/// </summary>
public struct Ring : IEquatable<Ring>,
    ICollidableWithRectangle,
    ICollidable<Ring, FloatRectangle>
{
    /// <summary>
    /// 圆环的中心点坐标。
    /// </summary>
    public Vector2 Center;

    /// <summary>
    /// 圆环的内半径。
    /// </summary>
    public float InnerRadius;

    /// <summary>
    /// 圆环的外半径。
    /// </summary>
    public float OuterRadius;

    /// <summary>
    /// 使用指定的中心点、内半径和外半径初始化 <see cref="Ring"/> 结构的新实例。
    /// </summary>
    /// <param name="center">圆环的中心点。</param>
    /// <param name="innerRadius">内半径，必须非负且不大于外半径。</param>
    /// <param name="outerRadius">外半径，必须大于等于内半径。</param>
    /// <exception cref="ArgumentException">当内半径小于 0 或大于外半径时抛出。</exception>
    public Ring(Vector2 center, float innerRadius, float outerRadius)
    {
        Center = center;

        if (innerRadius < 0f || innerRadius > outerRadius)
            throw new ArgumentException("Inner radius must be non-negative and less than outer radius.", $"{nameof(innerRadius)}, {nameof(outerRadius)}");

        InnerRadius = innerRadius;
        OuterRadius = outerRadius;
    }

    /// <summary>
    /// 使用指定的坐标分量、内半径和外半径初始化 <see cref="Ring"/> 结构的新实例。
    /// </summary>
    /// <param name="x">中心点的 X 坐标。</param>
    /// <param name="y">中心点的 Y 坐标。</param>
    /// <param name="innerRadius">内半径。</param>
    /// <param name="outerRadius">外半径。</param>
    public Ring(float x, float y, float innerRadius, float outerRadius) : this(new Vector2(x, y), innerRadius, outerRadius) { }

    public readonly bool Equals(Ring other) => Center == other.Center && InnerRadius == other.InnerRadius && OuterRadius == other.OuterRadius;
    public override readonly bool Equals(object obj) => obj is Ring other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(Center, InnerRadius, OuterRadius);
    public static bool operator ==(Ring left, Ring right) => left.Equals(right);
    public static bool operator !=(Ring left, Ring right) => !(left == right);

    /// <summary>
    /// 返回当前圆环的字符串表示形式。
    /// </summary>
    /// <returns>
    /// 一个格式为 <c>"Ring { Center: {X:0 Y:0}, InnerRadius:5, OuterRadius:10 }"</c> 的字符串，
    /// 其中 <c>Center</c> 使用 <see cref="Vector2"/> 的默认格式输出，
    /// <c>InnerRadius</c> 和 <c>OuterRadius</c> 输出为数值形式。
    /// </returns>
    public override readonly string ToString() => $"Ring {{ Center: {Center}, InnerRadius: {InnerRadius}, OuterRadius: {OuterRadius} }}";

    /// <summary>
    /// 判断矩形与以内半径或外半径构成的圆的包含关系。
    /// </summary>
    /// <param name="rectangle">要判断的矩形。</param>
    /// <param name="intersect">若为 <see langword="true"/>，使用矩形到圆心的最小距离判断；若为 <see langword="false"/>，使用最大距离判断。</param>
    /// <param name="useOuterRadius">若为 <see langword="true"/>，使用外半径；否则使用内半径。</param>
    /// <returns>如果矩形被圆包含（根据参数定义），则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public readonly bool CircleContains(FloatRectangle rectangle, bool intersect, bool useOuterRadius) => (intersect ? TOMathUtils.Geometry.MinDistanceFromTo(rectangle, Center) : TOMathUtils.Geometry.MaxDistanceFromTo(rectangle, Center)) < (useOuterRadius ? OuterRadius : InnerRadius);

    public readonly bool Collides(Rectangle other) => TOMathUtils.Geometry.FloatRectanglevRingCollision((FloatRectangle)other, this);
    public readonly bool Collides(FloatRectangle other) => TOMathUtils.Geometry.FloatRectanglevRingCollision(other, this);
}