// Developed by ColdsUx

namespace Transoceanic.DataStructures.Geometry;

/// <summary>
/// 表示一个可以旋转的矩形。
/// </summary>
public struct RotatedRectangle : IEquatable<RotatedRectangle>,
    ICollidableWithRectangle,
    ICollidable<RotatedRectangle, RotatedRectangle>,
    ICollidable<RotatedRectangle, FloatRectangle>,
    ICollidable<RotatedRectangle, Circle>
{
    /// <summary>
    /// 旋转前的基础轴对齐矩形。
    /// </summary>
    public FloatRectangle Source;

    /// <summary>
    /// 旋转角度（弧度），已规范化到标准周期内。
    /// </summary>
    public float Rotation;

    /// <summary>
    /// 使用指定的源矩形和旋转角度初始化 <see cref="RotatedRectangle"/> 结构的新实例。
    /// </summary>
    /// <param name="source">旋转前的基础轴对齐矩形。</param>
    /// <param name="rotation">旋转角度（弧度）。</param>
    public RotatedRectangle(FloatRectangle source, float rotation)
    {
        Source = source;
        Rotation = TOMathUtils.NormalizeWithPeriod(rotation);
    }

    /// <summary>
    /// 使用指定的中心点、宽度、高度和旋转角度初始化 <see cref="RotatedRectangle"/> 结构的新实例。
    /// </summary>
    /// <param name="center">旋转后矩形的中心点。</param>
    /// <param name="width">矩形的宽度。</param>
    /// <param name="height">矩形的高度。</param>
    /// <param name="rotation">旋转角度（弧度）。</param>
    public RotatedRectangle(Vector2 center, float width, float height, float rotation) : this(FloatRectangle.FromCenter(center, width, height), rotation) { }

    /// <summary>
    /// 获取旋转后矩形的中心点。
    /// </summary>
    public readonly Vector2 Center => Source.Center;

    /// <summary>
    /// 获取旋转后矩形左上角的坐标。
    /// </summary>
    public readonly Vector2 TopLeft => Source.TopLeft.RotatedBy(Rotation, Center);
    /// <summary>
    /// 获取旋转后矩形右上角的坐标。
    /// </summary>
    public readonly Vector2 TopRight => Source.TopRight.RotatedBy(Rotation, Center);
    /// <summary>
    /// 获取旋转后矩形左下角的坐标。
    /// </summary>
    public readonly Vector2 BottomLeft => Source.BottomLeft.RotatedBy(Rotation, Center);
    /// <summary>
    /// 获取旋转后矩形右下角的坐标。
    /// </summary>
    public readonly Vector2 BottomRight => Source.BottomRight.RotatedBy(Rotation, Center);

    /// <summary>
    /// 获取旋转后矩形的上边线段。
    /// </summary>
    public readonly LineSegment TopSide
    {
        get
        {
            Vector2 center = Center;
            (float sin, float cos) = MathF.SinCos(Rotation);
            Vector2 widthRotated = new Vector2(cos, sin) * (Source.Width / 2f);
            Vector2 heightRotated = new Vector2(-sin, cos) * (Source.Height / 2f);
            return new(center - widthRotated - heightRotated, center + widthRotated - heightRotated);
        }
    }

    /// <summary>
    /// 获取旋转后矩形的下边线段。
    /// </summary>
    public readonly LineSegment BottomSide
    {
        get
        {
            Vector2 center = Center;
            (float sin, float cos) = MathF.SinCos(Rotation);
            Vector2 widthRotated = new Vector2(cos, sin) * (Source.Width / 2f);
            Vector2 heightRotated = new Vector2(-sin, cos) * (Source.Height / 2f);
            return new(center - widthRotated + heightRotated, center + widthRotated + heightRotated);
        }
    }

    /// <summary>
    /// 获取旋转后矩形的左边线段。
    /// </summary>
    public readonly LineSegment LeftSide
    {
        get
        {
            Vector2 center = Center;
            (float sin, float cos) = MathF.SinCos(Rotation);
            Vector2 widthRotated = new Vector2(cos, sin) * (Source.Width / 2f);
            Vector2 heightRotated = new Vector2(-sin, cos) * (Source.Height / 2f);
            return new(center - widthRotated - heightRotated, center - widthRotated + heightRotated);
        }
    }

    /// <summary>
    /// 获取旋转后矩形的右边线段。
    /// </summary>
    public readonly LineSegment RightSide
    {
        get
        {
            Vector2 center = Center;
            (float sin, float cos) = MathF.SinCos(Rotation);
            Vector2 widthRotated = new Vector2(cos, sin) * (Source.Width / 2f);
            Vector2 heightRotated = new Vector2(-sin, cos) * (Source.Height / 2f);
            return new(center + widthRotated - heightRotated, center + widthRotated + heightRotated);
        }
    }

    /// <summary>
    /// 获取旋转后矩形的四个顶点坐标。
    /// </summary>
    public readonly (Vector2 TopLeft, Vector2 TopRight, Vector2 BottomLeft, Vector2 BottomRight) Vertices
    {
        get
        {
            Vector2 center = Center;
            (float sin, float cos) = MathF.SinCos(Rotation);
            Vector2 widthRotated = new Vector2(cos, sin) * (Source.Width / 2f);
            Vector2 heightRotated = new Vector2(-sin, cos) * (Source.Height / 2f);
            return (center - widthRotated - heightRotated, center + widthRotated - heightRotated, center - widthRotated + heightRotated, center + widthRotated + heightRotated);
        }
    }

    /// <summary>
    /// 获取旋转后矩形的四条边线段。
    /// </summary>
    public readonly (LineSegment Top, LineSegment Bottom, LineSegment Left, LineSegment Right) Sides
    {
        get
        {
            (Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight) = Vertices;
            return (new LineSegment(topLeft, topRight), new LineSegment(bottomLeft, bottomRight), new LineSegment(topLeft, bottomLeft), new LineSegment(topRight, bottomRight));
        }
    }

    /// <summary>
    /// 判断指定的点是否位于当前旋转矩形内部（包含边界）。
    /// </summary>
    /// <param name="point">要测试的点坐标。</param>
    /// <returns>如果点在矩形内，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public readonly bool Contains(Vector2 point) => Source.Contains(Center + (point - Center).RotatedBy(-Rotation));

    public readonly bool Equals(RotatedRectangle other) => Source == other.Source && Rotation == other.Rotation;
    public override readonly bool Equals(object obj) => obj is RotatedRectangle other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(Source, Rotation);
    public static bool operator ==(RotatedRectangle left, RotatedRectangle right) => left.Equals(right);
    public static bool operator !=(RotatedRectangle left, RotatedRectangle right) => !(left == right);

    /// <summary>
    /// 返回当前旋转矩形的字符串表示形式。
    /// </summary>
    /// <returns>
    /// 一个格式为 <c>"RotatedRectangle { Source: FloatRectangle { Position: {X:0 Y:0}, Width:100, Height:50 }, Rotation:1.57 }"</c> 的字符串，
    /// 其中 <c>Source</c> 使用 <see cref="FloatRectangle.ToString"/> 的格式输出，
    /// <c>Rotation</c> 以弧度数值形式输出。
    /// </returns>
    public override readonly string ToString() => $"RotatedRectangle {{ Source: {Source}, Rotation: {Rotation} }}";

    /// <summary>
    /// 定义从 <see cref="FloatRectangle"/> 到 <see cref="RotatedRectangle"/> 的隐式转换，旋转角度为 0。
    /// </summary>
    /// <param name="rect">要转换的 <see cref="FloatRectangle"/>。</param>
    public static implicit operator RotatedRectangle(FloatRectangle rect) => new(rect, 0f);

    public readonly bool Collides(RotatedRectangle other)
    {
        //SAT检测
        (Vector2 aPoint1, Vector2 aPoint2, Vector2 aPoint3, Vector2 aPoint4) = Vertices;
        (Vector2 bPoint1, Vector2 bPoint2, Vector2 bPoint3, Vector2 bPoint4) = other.Vertices;
        ReadOnlySpan<Vector2> aPoints = [aPoint1, aPoint2, aPoint3, aPoint4];
        ReadOnlySpan<Vector2> bPoints = [bPoint1, bPoint2, bPoint3, bPoint4];

        (float sinA, float cosA) = MathF.SinCos(Rotation);
        if (!TOMathUtils.Geometry.OverlapOnAxis(new Vector2(cosA, sinA), aPoints, bPoints))
            return false;
        if (!TOMathUtils.Geometry.OverlapOnAxis(new Vector2(-sinA, cosA), aPoints, bPoints))
            return false;

        (float sinB, float cosB) = MathF.SinCos(other.Rotation);
        if (!TOMathUtils.Geometry.OverlapOnAxis(new Vector2(cosB, sinB), aPoints, bPoints))
            return false;
        if (!TOMathUtils.Geometry.OverlapOnAxis(new Vector2(-sinB, cosB), aPoints, bPoints))
            return false;

        return true;
    }

    public readonly bool Collides(Rectangle other) => Collides((FloatRectangle)other);
    public readonly bool Collides(FloatRectangle other) => TOMathUtils.Geometry.RotatedRectanglevFloatRectangleCollision(this, other);
    public readonly bool Collides(Circle other) => TOMathUtils.Geometry.RotatedRectanglevCircleCollision(this, other);
}