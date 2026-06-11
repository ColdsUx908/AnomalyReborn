// Developed by ColdsUx

namespace Transoceanic.DataStructures.Geometry;

/// <summary>
/// 表示一个二维正六边形。
/// </summary>
public struct Hexagon : IEquatable<Hexagon>, ICollidableWithRectangle
{
    /// <summary>
    /// 六边形的中心坐标。
    /// </summary>
    public Vector2 Center;

    /// <summary>
    /// 六边形的半径。
    /// </summary>
    public float CircumRadius;

    /// <summary>
    /// 六边形的旋转。
    /// <br/>表示六边形绕其中心点的旋转角度。范围为 [0, π/3) 弧度，其中 0 度表示一个顶点朝右。
    /// </summary>
    public float Rotation
    {
        get;
        set => field = TOMathUtils.NormalizeWithPeriod(value, TOMathUtils.PiOver3);
    }

    /// <summary>
    /// 使用指定的中心坐标和半径初始化 <see cref="Hexagon"/> 结构的新实例。
    /// </summary>
    /// <param name="center">六边形的中心坐标。</param>
    /// <param name="radius">六边形的半径。</param>
    /// <param name="rotation">六边形的旋转。</param>
    public Hexagon(Vector2 center, float radius, float rotation)
    {
        Center = center;
        CircumRadius = radius;
        Rotation = rotation;
    }

    public readonly bool Equals(Hexagon other) => Center == other.Center && CircumRadius == other.CircumRadius && Rotation == other.Rotation;
    public override readonly bool Equals(object obj) => obj is Hexagon other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(Center, CircumRadius, Rotation);
    public static bool operator ==(Hexagon left, Hexagon right) => left.Equals(right);
    public static bool operator !=(Hexagon left, Hexagon right) => !(left == right);

    /// <summary>
    /// 返回当前六边形的字符串表示形式。
    /// </summary>
    /// <returns>
    /// 一个格式为 <c>"Hexagon { Center: {X:0 Y:0}, Radius:5, Rotation:0 }"</c> 的字符串，
    /// 其中 <c>Center</c> 使用 <see cref="Vector2"/> 的默认格式输出，
    /// <c>Radius</c> 输出为数值形式，
    /// <c>Rotation</c> 输出为数值形式。
    /// </returns>
    public override readonly string ToString() => $"Hexagon {{ Center: {Center}, Radius: {CircumRadius}, Rotation: {Rotation} }}";

    public readonly bool Collides(Rectangle other)
    {
        Span<Vector2> hex = stackalloc Vector2[6];
        for (int i = 0; i < 6; i++)
        {
            float angle = Rotation + i * TOMathUtils.PiOver3;
            hex[i] = Center + new PolarVector2(CircumRadius, angle);
        }

        ReadOnlySpan<Vector2> rect = [ other.TopLeft(), other.TopRight(), other.BottomLeft(), other.BottomRight() ];

        //测试矩形对齐轴
        if (!TOMathUtils.Geometry.OverlapOnAxis(Vector2.UnitX, hex, rect))
            return false;
        if (!TOMathUtils.Geometry.OverlapOnAxis(Vector2.UnitY, hex, rect))
            return false;

        //测试六边形的三条分离轴（边法线方向）
        for (int i = 0; i < 3; i++)
        {
            float axisAngle = Rotation + TOMathUtils.PiOver6 + i * TOMathUtils.PiOver3;
            Vector2 axis = new PolarVector2(axisAngle);
            if (!TOMathUtils.Geometry.OverlapOnAxis(axis, hex, rect))
                return false;
        }

        //所有轴均重叠，则发生碰撞
        return true;
    }

    public readonly bool Collides(Hexagon other)
    {
        //构造两个六边形的世界坐标顶点（各6个）
        Span<Vector2> vertsA = stackalloc Vector2[6];
        Span<Vector2> vertsB = stackalloc Vector2[6];

        for (int i = 0; i < 6; i++)
        {
            float angleA = Rotation + i * TOMathUtils.PiOver3;
            float angleB = other.Rotation + i * TOMathUtils.PiOver3;
            vertsA[i] = Center + new PolarVector2(CircumRadius, angleA);
            vertsB[i] = other.Center + new PolarVector2(other.CircumRadius, angleB);
        }

        //测试当前六边形的三条分离轴（边法线方向）
        for (int i = 0; i < 3; i++)
        {
            float axisAngle = Rotation + TOMathUtils.PiOver6 + i * TOMathUtils.PiOver3;
            Vector2 axis = new PolarVector2(axisAngle);
            if (!TOMathUtils.Geometry.OverlapOnAxis(axis, vertsA, vertsB))
                return false;
        }

        //测试另一个六边形的三条分离轴
        for (int i = 0; i < 3; i++)
        {
            float axisAngle = other.Rotation + TOMathUtils.PiOver6 + i * TOMathUtils.PiOver3;
            Vector2 axis = new PolarVector2(axisAngle);
            if (!TOMathUtils.Geometry.OverlapOnAxis(axis, vertsA, vertsB))
                return false;
        }

        //所有轴均重叠，则发生碰撞
        return true;
    }
}