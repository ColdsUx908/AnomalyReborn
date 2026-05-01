// Developed by ColdsUx

namespace Transoceanic.DataStructures.Geometry;

/// <summary>
/// 表示一个二维椭圆，支持旋转。
/// </summary>
public struct Ellipse : IEquatable<Ellipse>
{
    /// <summary>
    /// 椭圆的中心点坐标。
    /// </summary>
    public Vector2 Center;

    /// <summary>
    /// 椭圆的长半轴长度（旋转前的 X 轴方向半径）。
    /// </summary>
    public float A;

    /// <summary>
    /// 椭圆的短半轴长度（旋转前的 Y 轴方向半径）。
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
}