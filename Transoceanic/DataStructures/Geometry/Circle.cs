// Developed by ColdsUx

namespace Transoceanic.DataStructures.Geometry;

/// <summary>
/// 表示一个二维圆形。
/// </summary>
public struct Circle : IEquatable<Circle>,
    ICollidableWithRectangle,
    ICollidable<Circle, Circle>,
    ICollidable<Circle, FloatRectangle>,
    ICollidable<Circle, RotatedRectangle>
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
    public Circle(Vector2 center, float radius)
    {
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

    public readonly bool Equals(Circle other) => Center == other.Center && Radius == other.Radius;
    public override readonly bool Equals(object obj) => obj is Circle other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(Center, Radius);
    public static bool operator ==(Circle left, Circle right) => left.Equals(right);
    public static bool operator !=(Circle left, Circle right) => !(left == right);

    /// <summary>
    /// 返回当前圆的字符串表示形式。
    /// </summary>
    /// <returns>
    /// 一个格式为 <c>"Circle { Center: {X:0 Y:0}, Radius:5 }"</c> 的字符串，
    /// 其中 <c>Center</c> 使用 <see cref="Vector2"/> 的默认格式输出，
    /// <c>Radius</c> 输出为数值形式。
    /// </returns>
    public override readonly string ToString() => $"Circle {{ Center: {Center}, Radius: {Radius} }}";

    public readonly bool Collides(Rectangle other) => Collides((FloatRectangle)other);
    public readonly bool Collides(Circle other) => Vector2.Distance(Center, other.Center) <= Radius + other.Radius;
    public readonly bool Collides(FloatRectangle other) => TOMathUtils.Geometry.FloatRectanglevCircleCollision(other, this);
    public readonly bool Collides(RotatedRectangle other) => TOMathUtils.Geometry.RotatedRectanglevCircleCollision(other, this);
}