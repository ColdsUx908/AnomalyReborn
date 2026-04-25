// Designed by ColdsUx

namespace Transoceanic.DataStructures.Geometry;

/// <summary>
/// 表示二维平面中的一条无限直线，使用一般式 Ax + By + C = 0 表示。
/// </summary>
public struct Line : IEquatable<Line>
{
    /// <summary>
    /// 直线方程中 x 的系数。
    /// </summary>
    public float A;

    /// <summary>
    /// 直线方程中 y 的系数。
    /// </summary>
    public float B;

    /// <summary>
    /// 直线方程的常数项。
    /// </summary>
    public float C;

    /// <summary>
    /// 使用一般式 Ax + By + C = 0 的系数初始化 <see cref="Line"/> 结构的新实例。
    /// </summary>
    /// <param name="a">x 的系数 A。</param>
    /// <param name="b">y 的系数 B。</param>
    /// <param name="c">常数项 C。</param>
    /// <exception cref="ArgumentException">当 A 和 B 同时为零时抛出。</exception>
    public Line(float a, float b, float c)
    {
        if (a == 0 && b == 0)
            throw new ArgumentException("A and B cannot be both zero.");
        // 规范化符号，保证 A 为正，若 A 为 0 则 B 为正
        if (a < 0)
        {
            a = -a;
            b = -b;
            c = -c;
        }
        else if (a == 0 && b < 0)
        {
            b = -b;
            c = -c;
        }
        A = a;
        B = b;
        C = c;
    }

    /// <summary>
    /// 通过两点创建一条直线。
    /// </summary>
    /// <param name="point1">直线上的第一个点。</param>
    /// <param name="point2">直线上的第二个点。</param>
    /// <returns>通过两点的直线。</returns>
    public static Line FromTwoPoints(Vector2 point1, Vector2 point2)
    {
        // 两点式转换为一般式：(y2 - y1)x - (x2 - x1)y + (x2 - x1)y1 - (y2 - y1)x1 = 0
        float a = point2.Y - point1.Y;
        float b = point1.X - point2.X;
        float c = point2.X * point1.Y - point1.X * point2.Y;

        return new Line(a, b, c);
    }

    /// <summary>
    /// 通过斜率和截距创建直线（斜截式 y = kx + b）。
    /// </summary>
    /// <param name="k">直线的斜率。</param>
    /// <param name="b">Y 轴截距。</param>
    /// <returns>对应的直线。</returns>
    public static Line FromSlopeIntercept(float k, float b) => new(k, -1, b);

    /// <summary>
    /// 通过一点和斜率创建直线（点斜式 y - y0 = k(x - x0)）。
    /// </summary>
    /// <param name="point">直线上的一个点。</param>
    /// <param name="k">直线的斜率。</param>
    /// <returns>对应的直线。</returns>
    public static Line FromPointSlope(Vector2 point, float k) => new(k, -1, point.Y - k * point.X);

    /// <summary>
    /// 通过 X 轴和 Y 轴上的截距创建直线（截距式 x/a + y/b = 1）。
    /// </summary>
    /// <param name="a">X 轴截距。</param>
    /// <param name="b">Y 轴截距。</param>
    /// <returns>对应的直线。</returns>
    public static Line FromIntercept(float a, float b) => new(b, a, -a * b);

    /// <summary>
    /// 通过法向量和直线上一点创建直线。
    /// </summary>
    /// <param name="normal">直线的法向量 (A, B)。</param>
    /// <param name="point">直线上的一个点。</param>
    /// <returns>对应的直线。</returns>
    public static Line FromNormalAndPoint(Vector2 normal, Vector2 point) => new(normal.X, normal.Y, -Vector2.Dot(normal, point));

    /// <summary>
    /// 从线段创建包含该线段的无限直线。
    /// </summary>
    /// <param name="segment">源线段。</param>
    /// <returns>包含该线段的直线。</returns>
    public static Line FromSegment(LineSegment segment) => FromTwoPoints(segment.Start, segment.End);

    /// <summary>
    /// 获取当前直线的归一化表示形式，使得法向量 (A, B) 为单位向量。
    /// </summary>
    /// <returns>归一化后的直线。</returns>
    public readonly Line Normalize()
    {
        float length = MathF.Sqrt(A * A + B * B);
        return length > 0 ? new Line(A / length, B / length, C / length) : this;
    }

    /// <summary>
    /// 获取直线的斜率。如果直线是垂直的，则返回无穷大。
    /// </summary>
    public readonly float Slope => -A / B;

    /// <summary>
    /// 获取直线在 Y 轴上的截距（当 x = 0 时的 y 值）。如果直线是垂直的，则未定义。
    /// </summary>
    public readonly float YIntercept => -C / B;

    /// <summary>
    /// 获取直线在 X 轴上的截距（当 y = 0 时的 x 值）。如果直线是水平的，则未定义。
    /// </summary>
    public readonly float XIntercept => -C / A;

    /// <summary>
    /// 获取一个值，该值指示直线是否水平。
    /// </summary>
    public readonly bool IsHorizontal => Math.Abs(A) < float.Epsilon && Math.Abs(B) > float.Epsilon;

    /// <summary>
    /// 获取一个值，该值指示直线是否垂直。
    /// </summary>
    public readonly bool IsVertical => Math.Abs(B) < float.Epsilon && Math.Abs(A) > float.Epsilon;

    /// <summary>
    /// 计算给定 X 坐标对应的 Y 值。
    /// </summary>
    /// <param name="x">X 坐标。</param>
    /// <returns>对应的 Y 坐标。</returns>
    public readonly float GetY(float x) => (-A * x - C) / B;

    /// <summary>
    /// 计算给定 Y 坐标对应的 X 值。
    /// </summary>
    /// <param name="y">Y 坐标。</param>
    /// <returns>对应的 X 坐标。</returns>
    public readonly float GetX(float y) => (-B * y - C) / A;

    /// <summary>
    /// 获取直线的法向量 (A, B)。
    /// </summary>
    public readonly Vector2 Normal => new(A, B);

    /// <summary>
    /// 获取直线的方向向量。
    /// </summary>
    public readonly Vector2 Direction => IsVertical ? new Vector2(0, 1) : IsHorizontal ? new Vector2(1, 0) : new Vector2(B, -A);

    /// <summary>
    /// 计算当前直线与另一条直线的交点。
    /// </summary>
    /// <param name="other">另一条直线。</param>
    /// <returns>如果两条直线相交，返回交点坐标；如果平行或重合，返回 <c>null</c>。</returns>
    public readonly Vector2? Intersection(Line other)
    {
        float determinant = A * other.B - other.A * B;

        if (Math.Abs(determinant) == 0)
        {
            // 平行或重合
            return null;
        }

        float x = (B * other.C - other.B * C) / determinant;
        float y = (other.A * C - A * other.C) / determinant;

        return new Vector2(x, y);
    }

    /// <summary>
    /// 计算点到直线的垂直距离。
    /// </summary>
    /// <param name="point">要计算距离的点。</param>
    /// <returns>点到直线的距离。</returns>
    public readonly float DistanceToPoint(Vector2 point) => Math.Abs(A * point.X + B * point.Y + C) / (float)Math.Sqrt(A * A + B * B);

    public readonly bool Equals(Line other)
    {
        Line thisNormalized = Normalize();
        Line otherNormalized = other.Normalize();
        return Math.Abs(thisNormalized.A - otherNormalized.A) < float.Epsilon
            && Math.Abs(thisNormalized.B - otherNormalized.B) < float.Epsilon
            && Math.Abs(thisNormalized.C - otherNormalized.C) < float.Epsilon;
    }
    public override readonly bool Equals(object obj) => obj is Line other && Equals(other);
    public override readonly int GetHashCode()
    {
        Line tempThis = Normalize();
        return HashCode.Combine(tempThis.A, tempThis.B, tempThis.C);
    }
    public static bool operator ==(Line left, Line right) => left.Equals(right);
    public static bool operator !=(Line left, Line right) => !(left == right);

    /// <summary>
    /// 返回当前直线的字符串表示形式，以一般式 <c>Ax + By + C = 0</c> 的格式显示。
    /// </summary>
    /// <returns>
    /// 一个格式为 <c>"Line { Ax + By + C = 0 }"</c> 的字符串，
    /// 其中系数会进行美化处理：
    /// <list type="bullet">
    ///   <item><description>系数为 0 时省略该项；</description></item>
    ///   <item><description>系数为 1 或 -1 时省略数值部分，仅显示符号和变量；</description></item>
    ///   <item><description>常数项为正时显示 "+ C"，为负时显示 "- |C|"；</description></item>
    ///   <item><description>所有数值保留两位小数。</description></item>
    /// </list>
    /// 例如：直线 <c>2x - 3y + 5 = 0</c> 将被表示为 <c>"Line { 2x - 3y + 5 = 0 }"</c>。
    /// </returns>
    public override readonly string ToString()
    {
        string a = A switch
        {
            0 => "",
            1f => "x",
            -1f => "-x",
            _ => $"{A:0.##}x",
        };
        string b = B switch
        {
            0 => "",
            1f => a != "" ? " + y" : "y",
            -1f => a != "" ? " - y" : "-y",
            > 0 => a != "" ? $" + {B:0.##}y" : $"{B:0.##}y",
            _ => a != "" ? $"- {-B:0.##}y" : $"{B:0.##}y",
        };
        string c = C switch
        {
            0 => a != "" && b != "" ? "0" : "",
            > 0 => a != "" || b != "" ? $" + {C:0.##}" : $"{C:0.##}",
            _ => a != "" || b != "" ? $" - {-C:0.##}" : $"{C:0.##}",
        };
        return $"Line {{ {a}{b}{c} = 0 }}";
    }
}