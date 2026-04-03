namespace Transoceanic.DataStructures.Geometry;

public struct Line : IEquatable<Line>
{
    public float A; // x 系数
    public float B; // y 系数
    public float C; // 常数项

    /// <summary>
    /// 使用一般式 Ax + By + C = 0 创建直线。
    /// </summary>
    public Line(float a, float b, float c)
    {
        if (a == 0 && b == 0)
            throw new ArgumentException("A and B cannot be both zero.");
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
    /// 通过两点创建直线（两点式）。
    /// </summary>
    public static Line FromTwoPoints(Vector2 point1, Vector2 point2)
    {
        // 两点式转换为一般式：(y2 - y1)x - (x2 - x1)y + (x2 - x1)y1 - (y2 - y1)x1 = 0
        float a = point2.Y - point1.Y;
        float b = point1.X - point2.X;
        float c = point2.X * point1.Y - point1.X * point2.Y;

        return new Line(a, b, c);
    }

    /// <summary>
    /// 通过斜率和截距创建直线（斜截式）。
    /// </summary>
    public static Line FromSlopeIntercept(float k, float b) => new(k, -1, b);

    /// <summary>
    /// 通过点和斜率创建直线（点斜式）。
    /// </summary>
    public static Line FromPointSlope(Vector2 point, float k) => new(k, -1, point.Y - k * point.X);

    /// <summary>
    /// 通过截距创建直线（截距式）
    /// </summary>
    public static Line FromIntercept(float a, float b) => new(b, a, -a * b);

    /// <summary>
    /// 通过法向量和点创建直线。
    /// </summary>
    public static Line FromNormalAndPoint(Vector2 normal, Vector2 point) =>
        // 法向量为(A, B)，点(x0, y0)满足 Ax0 + By0 + C = 0
        // 所以 C = - (A*x0 + B*y0)
        new(normal.X, normal.Y, -Vector2.Dot(normal, point));

    /// <summary>
    /// 从线段创建直线。
    /// </summary>
    public static Line FromSegment(LineSegment segment) => FromTwoPoints(segment.Start, segment.End);

    public readonly Line Normalize()
    {
        float length = MathF.Sqrt(A * A + B * B);
        return length > 0 ? new Line(A / length, B / length, C / length) : this;
    }

    /// <summary>
    /// 获取直线的斜率（如果不是垂直线）。
    /// </summary>
    public readonly float Slope => -A / B;

    /// <summary>
    /// 获取Y轴截距（如果不是垂直线）。
    /// </summary>
    public readonly float YIntercept => -C / B;

    /// <summary>
    /// 获取X轴截距（如果不是水平线）。
    /// </summary>
    public readonly float XIntercept => -C / A;

    /// <summary>
    /// 判断是否为水平线。
    /// </summary>
    public readonly bool IsHorizontal => Math.Abs(A) < float.Epsilon && Math.Abs(B) > float.Epsilon;

    /// <summary>
    /// 判断是否为垂直线。
    /// </summary>
    public readonly bool IsVertical => Math.Abs(B) < float.Epsilon && Math.Abs(A) > float.Epsilon;

    /// <summary>
    /// 计算给定X坐标对应的Y值。
    /// </summary>
    public readonly float GetY(float x) => (-A * x - C) / B;

    /// <summary>
    /// 计算给定Y坐标对应的X值。
    /// </summary>
    public readonly float GetX(float y) => (-B * y - C) / A;

    /// <summary>
    /// 获取直线的法向量。
    /// </summary>
    public readonly Vector2 Normal => new(A, B);

    /// <summary>
    /// 获取直线的方向向量。
    /// </summary>
    public readonly Vector2 Direction => IsVertical ? new Vector2(0, 1) : IsHorizontal ? new Vector2(1, 0) : new Vector2(B, -A);

    /// <summary>
    /// 获取与另一条直线的交点。
    /// </summary>
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
    /// 计算点到直线的距离。
    /// </summary>
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
    /// 转换为字符串表示。
    /// </summary>
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