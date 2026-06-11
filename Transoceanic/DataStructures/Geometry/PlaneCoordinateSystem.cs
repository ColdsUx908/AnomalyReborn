// Developed by ColdsUx

namespace Transoceanic.DataStructures.Geometry;

/// <summary>
/// 表示一个二维平面坐标系，由原点（坐标中心）和两个线性无关的基底向量定义。
/// </summary>
/// <remarks>
/// 基底向量 <c>BasisU</c> 和 <c>BasisV</c> 不能共线（即叉积不为零），否则无法构成有效的坐标系。
/// </remarks>
public readonly struct PlaneCoordinateSystem : IEquatable<PlaneCoordinateSystem>
{
    /// <summary>
    /// 坐标系的原点。
    /// </summary>
    public readonly Vector2 Origin;

    /// <summary>
    /// 第一个基底向量（U 轴方向）。
    /// </summary>
    public readonly Vector2 BasisU;

    /// <summary>
    /// 第二个基底向量（V 轴方向）。
    /// </summary>
    public readonly Vector2 BasisV;

    /// <summary>
    /// 使用指定的原点和基底向量初始化 <see cref="PlaneCoordinateSystem"/> 结构的新实例。
    /// </summary>
    /// <param name="origin">坐标系的原点。</param>
    /// <param name="basisU">第一个基底向量（U 轴）。</param>
    /// <param name="basisV">第二个基底向量（V 轴）。</param>
    /// <exception cref="ArgumentException">当 <paramref name="basisU"/> 与 <paramref name="basisV"/> 共线时抛出。</exception>
    public PlaneCoordinateSystem(Vector2 origin, Vector2 basisU, Vector2 basisV)
    {
        // 检测基底是否共线（叉积为零）
        float cross = Vector2.Cross(basisU, basisV);
        if (cross == 0f)
            throw new ArgumentException("基底向量不能共线，否则无法构成有效的平面坐标系。");

        Origin = origin;
        BasisU = basisU;
        BasisV = basisV;
    }

    /// <summary>
    /// 获取一个值，指示坐标系是否退化（即基底向量共线）。
    /// </summary>
    public bool IsDegenerate => Vector2.Cross(BasisU, BasisV) == 0f;

    /// <summary>
    /// 将局部坐标（相对于当前坐标系）转换为世界坐标。
    /// </summary>
    /// <param name="localCoordinate">局部坐标 <c>(u, v)</c>，可表示点或向量。</param>
    /// <param name="asPoint">
    /// 是否将 <paramref name="localCoordinate"/> 视为点坐标。
    /// <br/>- <see langword="true"/>：视为点，转换时同时应用平移（加上原点）。
    /// <br/>- <see langword="false"/>：视为向量，仅进行基底变换（不加原点）。
    /// </param>
    /// <returns>世界坐标 <c>(x, y)</c>。</returns>
    public Vector2 LocalToWorld(Vector2 localCoordinate, bool asPoint = true)
    {
        Vector2 result = localCoordinate.X * BasisU + localCoordinate.Y * BasisV;
        if (asPoint)
            result += Origin;
        return result;
    }

    /// <summary>
    /// 将世界坐标转换为当前坐标系的局部坐标。
    /// </summary>
    /// <param name="worldCoordinate">世界坐标 <c>(x, y)</c>，可表示点或向量。</param>
    /// <param name="asPoint">
    /// 是否将 <paramref name="worldCoordinate"/> 视为点坐标。
    /// <br/>- <see langword="true"/>：视为点，转换时会先减去原点再求基底坐标。
    /// <br/>- <see langword="false"/>：视为向量，直接求解向量在基底下的表示。
    /// </param>
    /// <returns>局部坐标 <c>(u, v)</c>。</returns>
    /// <exception cref="InvalidOperationException">当坐标系退化（基底共线）时抛出。</exception>
    public Vector2 WorldToLocal(Vector2 worldCoordinate, bool asPoint = true)
    {
        float cross = Vector2.Cross(BasisU, BasisV);
        if (cross == 0f)
            throw new InvalidOperationException("坐标系已退化，无法进行转换。");

        Vector2 delta = worldCoordinate;
        if (asPoint)
            delta -= Origin;

        float u = Vector2.Cross(delta, BasisV) / cross;
        float v = Vector2.Cross(BasisU, delta) / cross;
        return new Vector2(u, v);
    }

    public bool Equals(PlaneCoordinateSystem other) => Origin == other.Origin && BasisU == other.BasisU && BasisV == other.BasisV;
    public override bool Equals(object obj) => obj is PlaneCoordinateSystem other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Origin, BasisU, BasisV);
    public static bool operator ==(PlaneCoordinateSystem left, PlaneCoordinateSystem right) => left.Equals(right);
    public static bool operator !=(PlaneCoordinateSystem left, PlaneCoordinateSystem right) => !(left == right);

    /// <summary>
    /// 返回当前平面坐标系的字符串表示形式。
    /// </summary>
    /// <returns>
    /// 格式为 <c>"PlaneCoordinateSystem { Origin: {X:0 Y:0}, BasisU: {X:0 Y:0}, BasisV: {X:0 Y:0} }"</c> 的字符串。
    /// </returns>
    public override string ToString() => $"PlaneCoordinateSystem {{ Origin: {Origin}, BasisU: {BasisU}, BasisV: {BasisV} }}";
}