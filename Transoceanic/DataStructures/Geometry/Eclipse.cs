// Developed by ColdsUx

namespace Transoceanic.DataStructures.Geometry;

/// <summary>
/// 表示一个二维椭圆，支持旋转。
/// </summary>
public struct Ellipse : IEquatable<Ellipse>, ICollidableWithRectangle
{
    /// <summary>
    /// 椭圆的中心点坐标。
    /// </summary>
    public Vector2 Center;

    /// <summary>
    /// 椭圆的 X 半轴长度（旋转前的 X 轴方向半径）。
    /// </summary>
    public float A;

    /// <summary>
    /// 椭圆的 Y 半轴长度（旋转前的 Y 轴方向半径）。
    /// </summary>
    public float B;

    /// <summary>
    /// 椭圆的旋转角度（弧度），已规范化到标准周期内。
    /// </summary>
    public float Rotation;

    /// <summary>
    /// 使用指定的中心点、半轴长度和旋转角度初始化 <see cref="Ellipse"/> 结构的新实例。
    /// </summary>
    /// <param name="center">椭圆的中心点。</param>
    /// <param name="a">长半轴长度。</param>
    /// <param name="b">短半轴长度。</param>
    /// <param name="rotation">旋转角度（弧度）。</param>
    public Ellipse(Vector2 center, float a, float b, float rotation)
    {
        Center = center;
        A = a;
        B = b;
        Rotation = TOMathUtils.NormalizeWithPeriod(rotation);
    }

    public readonly bool Equals(Ellipse other) => Center == other.Center && A == other.A && B == other.B && Rotation == other.Rotation;
    public override readonly bool Equals(object obj) => obj is Ellipse other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(Center, A, B, Rotation);
    public static bool operator ==(Ellipse left, Ellipse right) => left.Equals(right);
    public static bool operator !=(Ellipse left, Ellipse right) => !left.Equals(right);

    /// <summary>
    /// 返回当前椭圆的字符串表示形式。
    /// </summary>
    /// <returns>
    /// 一个格式为 <c>"Ellipse { Center: {X:0 Y:0}, A:10, B:5, Rotation:0.785 }"</c> 的字符串，
    /// 其中 <c>Center</c> 使用 <see cref="Vector2"/> 的默认格式输出，
    /// <c>A</c>、<c>B</c> 和 <c>Rotation</c> 输出为数值形式。
    /// </returns>
    public override readonly string ToString() => $"Ellipse {{ Center: {Center}, A: {A}, B: {B}, Rotation: {Rotation} }}";

    /// <summary>
    /// 获取椭圆在世界坐标系中的轴对齐包围盒（AABB）。
    /// </summary>
    /// <remarks>
    /// 包围盒的计算基于椭圆旋转后的外接矩形，用于快速碰撞预检。
    /// </remarks>
    public readonly FloatRectangle BoundingBox
    {
        get
        {
            (float sin, float cos) = MathF.SinCos(Rotation);
            float aSquared = A * A;
            float bSquared = B * B;
            sin *= sin;
            cos *= cos;
            float halfWidth = MathF.Sqrt(aSquared * cos + bSquared * sin);
            float halfHeight = MathF.Sqrt(aSquared * sin + bSquared * cos);
            return FloatRectangle.FromCenter(Center, halfWidth * 2f, halfHeight * 2f);
        }
    }

    /// <summary>
    /// 获取将椭圆变换为单位圆的局部坐标系。
    /// </summary>
    /// <remarks>
    /// 该坐标系的原点为椭圆中心，基底向量分别为长轴和短轴方向（带半轴长度）。
    /// 在局部坐标系中，椭圆边界对应方程为 <c>u² + v² = 1</c>。
    /// </remarks>
    public readonly PlaneCoordinateSystem LocalCoordinateSystem
    {
        get
        {
            (float sin, float cos) = MathF.SinCos(Rotation);
            return new PlaneCoordinateSystem(Center, new Vector2(cos * A, sin * A), new Vector2(-sin * B, cos * B));
        }
    }

    public readonly bool Collides(Rectangle other)
    {
        Parallelogram localParallelogram = ((Parallelogram)(FloatRectangle)other).WorldToLocal(LocalCoordinateSystem);
        return TOMathUtils.Geometry.ParallelogramVCircleCollision(localParallelogram, Circle.Unit);
    }
}