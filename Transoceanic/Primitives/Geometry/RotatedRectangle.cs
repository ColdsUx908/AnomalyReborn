namespace Transoceanic.Primitives.Geometry;

/// <summary>
/// 表示一个可以旋转的矩形。
/// </summary>
public struct RotatedRectangle : IEquatable<RotatedRectangle>, ICollidableWithRectangle
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
    /// 以线段为中轴、指定宽度构造旋转矩形，端点对应两条短边的中点。
    /// </summary>
    /// <param name="start">中轴起点。</param>
    /// <param name="end">中轴终点。</param>
    /// <param name="width">垂直于中轴的完整宽度。</param>
    public static RotatedRectangle FromLineSegment(Vector2 start, Vector2 end, float width)
        => new((start + end) * 0.5f, Vector2.Distance(start, end), width, (end - start).ToRotation());

    /// <summary>
    /// 根据内部参考点、四向距离及弧度角构造矩形；宽高为 left + right、top + bottom，中心沿旋转横轴偏移 (right - left)/2，沿垂轴偏移 (top - bottom)/2。
    /// </summary>
    public static RotatedRectangle FromInnerPoint(Vector2 point, float left, float right, float top, float bottom, float rotation)
    {
        Vector2 realCenter = point + new PolarVector2(rotation) * ((right - left) / 2f) + new PolarVector2(rotation + MathHelper.PiOver2) * ((top - bottom) / 2f);
        return new RotatedRectangle(FloatRectangle.FromCenter(realCenter, right + left, top + bottom), rotation);
    }

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
    /// 获取旋转后矩形的轴对齐包围盒 (Axis-Aligned Bounding Box)。
    /// </summary>
    /// <remarks>
    /// 包围盒是能够完全包含旋转后矩形的最小轴对齐矩形，其边平行于坐标轴。
    /// </remarks>
    public readonly FloatRectangle AABB
    {
        get
        {
            float width = Source.Width;
            float height = Source.Height;
            (float sin, float cos) = MathF.SinCos(Rotation);

            // 计算半宽度和半高度在全局 X、Y 轴上的投影绝对值之和，得到包围盒的半尺寸
            float extentX = MathF.Abs(cos * width) + MathF.Abs(-sin * height);
            float extentY = MathF.Abs(sin * width) + MathF.Abs(cos * height);

            Vector2 center = Center;
            return FloatRectangle.FromCenter(center, extentX, extentY);
        }
    }

    /// <summary>
    /// 判断指定的点是否位于当前旋转矩形内部（包含边界）。
    /// </summary>
    /// <param name="point">要测试的点坐标。</param>
    /// <returns>如果点在矩形内，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public readonly bool Contains(Vector2 point) => Source.Contains(Center + (point - Center).RotatedBy(-Rotation));

    /// <summary>
    /// 比较 Source、Rotation，使用成员本身的相等语义。
    /// </summary>
    public readonly bool Equals(RotatedRectangle other) => Source == other.Source && Rotation == other.Rotation;
    /// <inheritdoc/>
    public override readonly bool Equals(object obj) => obj is RotatedRectangle other && Equals(other);
    /// <inheritdoc/>
    public override readonly int GetHashCode() => HashCode.Combine(Source, Rotation);
    /// <summary>
    /// 按 Equals 比较 Source、Rotation，相等时返回 true。
    /// </summary>
    public static bool operator ==(RotatedRectangle left, RotatedRectangle right) => left.Equals(right);
    /// <summary>
    /// 返回相等运算符结果的逻辑取反。
    /// </summary>
    public static bool operator !=(RotatedRectangle left, RotatedRectangle right) => !(left == right);

    /// <inheritdoc/>
    /// <remarks>
    /// 返回当前旋转矩形的字符串表示形式。
    /// </remarks>
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

    /// <summary>
    /// 判断当前形状与指定形状是否相交；双方坐标和尺寸须使用同一坐标系和单位。
    /// </summary>
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

    /// <summary>
    /// 判断当前形状与指定形状是否相交；双方坐标和尺寸须使用同一坐标系和单位。
    /// </summary>
    public readonly bool Collides(Rectangle other) => Collides((FloatRectangle)other);
    /// <summary>
    /// 判断当前形状与指定形状是否相交；双方坐标和尺寸须使用同一坐标系和单位。
    /// </summary>
    public readonly bool Collides(FloatRectangle other) => TOMathUtils.Collision.RotatedRectangleVFloatRectangleCollision(this, other);
    /// <summary>
    /// 判断当前形状与指定形状是否相交；双方坐标和尺寸须使用同一坐标系和单位。
    /// </summary>
    public readonly bool Collides(Circle other) => TOMathUtils.Collision.RotatedRectangleVCircleCollision(this, other);
}
