namespace Transoceanic.Primitives.Geometry;

/// <summary>
/// 表示一个二维圆环（环形区域）。
/// </summary>
public struct Annulus : IEquatable<Annulus>, ICollidableWithRectangle
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
    /// 使用指定的中心点、内半径和外半径初始化 <see cref="Annulus"/> 结构的新实例。
    /// </summary>
    /// <param name="center">圆环的中心点。</param>
    /// <param name="innerRadius">内半径，必须非负且不大于外半径。</param>
    /// <param name="outerRadius">外半径，必须大于等于内半径。</param>
    /// <exception cref="ArgumentException">当半径不是有限值，或内半径小于 0、大于外半径时抛出。</exception>
    public Annulus(Vector2 center, float innerRadius, float outerRadius)
    {
        Center = center;

        if (!float.IsFinite(innerRadius) || !float.IsFinite(outerRadius) || innerRadius < 0f || innerRadius > outerRadius)
            throw new ArgumentException("Inner radius must be non-negative and less than outer radius.", $"{nameof(innerRadius)}, {nameof(outerRadius)}");

        InnerRadius = innerRadius;
        OuterRadius = outerRadius;
    }

    /// <summary>
    /// 使用指定的坐标分量、内半径和外半径初始化 <see cref="Annulus"/> 结构的新实例。
    /// </summary>
    /// <param name="x">中心点的 X 坐标。</param>
    /// <param name="y">中心点的 Y 坐标。</param>
    /// <param name="innerRadius">内半径。</param>
    /// <param name="outerRadius">外半径。</param>
    public Annulus(float x, float y, float innerRadius, float outerRadius) : this(new Vector2(x, y), innerRadius, outerRadius) { }

    /// <summary>
    /// 圆环的厚度，为外半径与内半径的差。
    /// </summary>
    public readonly float Thickness => OuterRadius - InnerRadius;

    /// <summary>
    /// 比较 Center、InnerRadius、OuterRadius，使用成员本身的相等语义。
    /// </summary>
    public readonly bool Equals(Annulus other) => Center == other.Center && InnerRadius == other.InnerRadius && OuterRadius == other.OuterRadius;
    /// <inheritdoc/>
    public override readonly bool Equals(object obj) => obj is Annulus other && Equals(other);
    /// <inheritdoc/>
    public override readonly int GetHashCode() => HashCode.Combine(Center, InnerRadius, OuterRadius);
    /// <summary>
    /// 按 Equals 比较 Center、InnerRadius、OuterRadius，相等时返回 true。
    /// </summary>
    public static bool operator ==(Annulus left, Annulus right) => left.Equals(right);
    /// <summary>
    /// 返回相等运算符结果的逻辑取反。
    /// </summary>
    public static bool operator !=(Annulus left, Annulus right) => !(left == right);

    /// <inheritdoc/>
    /// <remarks>
    /// 返回当前圆环的字符串表示形式。
    /// </remarks>
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

    /// <summary>
    /// 判断当前形状与指定形状是否相交；双方坐标和尺寸须使用同一坐标系和单位。
    /// </summary>
    public readonly bool Collides(Rectangle other) => TOMathUtils.Collision.FloatRectangleVAnnulusCollision(other, this);
}
