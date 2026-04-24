namespace Transoceanic.DataStructures;

/// <summary>
/// 二维极坐标向量 (ρ, θ)。
/// </summary>
public struct PolarVector2 : IEquatable<PolarVector2>
{
    /// <summary>
    /// 模长。
    /// <br/>非负。
    /// </summary>
    public float Radius
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            field = value;
        }
    }

    /// <summary>
    /// 角度（弧度制）。
    /// <br/>范围为 [0, 2π)。
    /// <para>对于零向量（<see cref="Radius"/> 为 0），该属性始终返回 0。</para>
    /// </summary>
    public float Angle
    {
        readonly get => Radius == 0f ? 0f : field;
        set => field = Radius == 0f ? 0f : TOMathUtils.NormalizeWithPeriod(value); //零向量的角度强制为0
    }

    /// <summary>
    /// 角度（角度制）值。
    /// <br/>设置时将自动转换为弧度并规范化到 [0, 2π) 区间。
    /// </summary>
    public float AngleInDegree
    {
        readonly get => MathHelper.ToDegrees(Angle);
        set => Angle = MathHelper.ToRadians(value);
    }

    /// <summary>
    /// 角度除以 π 的值。
    /// <br/>例如，180° 对应 1.0。
    /// </summary>
    public float AngleOverPi
    {
        readonly get => Angle / MathHelper.Pi;
        set => Angle = value * MathHelper.Pi;
    }

    /// <summary>
    /// 角度除以 2π 的值。
    /// <br/>例如，360° 对应 1.0。
    /// </summary>
    public float AngleOverPeriod
    {
        readonly get => Angle / MathHelper.TwoPi;
        set => Angle = value * MathHelper.TwoPi;
    }

    /// <summary>
    /// 构造一个极坐标向量 (ρ, θ)。
    /// </summary>
    /// <param name="radius">模长 ρ，必须非负。</param>
    /// <param name="angle">角度 θ（弧度制）。</param>
    public PolarVector2(float radius, float angle)
    {
        Radius = radius;
        Angle = angle;
    }

    /// <summary>
    /// 构造一个极坐标单位向量 (1, θ)。
    /// </summary>
    /// <param name="angle">角度 θ（弧度制）。</param>
    public PolarVector2(float angle) : this(1f, angle) { }

    /// <summary>
    /// 使用直角向量构造一个极坐标向量。
    /// </summary>
    /// <param name="value">直角坐标向量。</param>
    public PolarVector2(Vector2 value) : this(value.Modulus, value.ToRotation()) { }

    /// <summary>
    /// 拷贝构造函数。
    /// </summary>
    /// <param name="original">要复制的原极坐标向量。</param>
    public PolarVector2(PolarVector2 original)
    {
        Radius = original.Radius;
        Angle = original.Angle;
    }

    /// <summary>
    /// 将极坐标向量解构为 (ρ, θ) 元组。
    /// </summary>
    /// <param name="radius">输出的模长 ρ。</param>
    /// <param name="angle">输出的角度 θ（弧度制）。</param>
    public readonly void Deconstruct(out float radius, out float angle) => (radius, angle) = (Radius, Angle);

    /// <summary>
    /// 将直角向量显式转换为极坐标向量。
    /// </summary>
    /// <param name="value">直角坐标向量。</param>
    public static explicit operator PolarVector2(Vector2 value) => new(value);

    /// <summary>
    /// 将极坐标向量隐式转换为直角向量。
    /// </summary>
    /// <param name="value">极坐标向量。</param>
    public static implicit operator Vector2(PolarVector2 value)
    {
        (float sin, float cos) = MathF.SinCos(value.Angle);
        return new(value.Radius * cos, value.Radius * sin);
    }

    /// <summary>
    /// 一元正运算符，返回原向量。
    /// </summary>
    public static PolarVector2 operator +(PolarVector2 a) => a;

    /// <summary>
    /// 极坐标向量取反。
    /// <br/>结果向量方向增加 π，模长不变。
    /// </summary>
    /// <param name="a">原向量。</param>
    /// <returns>取反后的向量。</returns>
    public static PolarVector2 operator -(PolarVector2 a) => new(a.Radius, a.Angle + MathHelper.Pi);

    /// <summary>
    /// 极坐标向量加法。
    /// </summary>
    /// <param name="a">第一个向量。</param>
    /// <param name="b">第二个向量。</param>
    /// <returns>两向量之和。</returns>
    public static PolarVector2 operator +(PolarVector2 a, PolarVector2 b)
    {
        //极坐标加法公式：
        //新极径 ρ = sqrt(ρ1² + ρ2² + 2ρ1ρ2cos(θ2 - θ1))
        //新角度 θ = θ1 + arctan(r2sin(θ2 - θ1) / (r1 + r2cos(θ2 - θ1)))

        (float sinDelta, float cosDelta) = MathF.SinCos(b.Angle - a.Angle);
        float radius = MathF.Sqrt(a.Radius * a.Radius + b.Radius * b.Radius + 2 * a.Radius * b.Radius * cosDelta);

        float numerator = b.Radius * sinDelta;
        float denominator = a.Radius + b.Radius * cosDelta;
        float angleOffset = denominator != 0 ? MathF.Atan2(numerator, denominator) : 0f;
        float angle = a.Angle + angleOffset;

        return new(radius, angle);
    }

    /// <summary>
    /// 极坐标向量减法。
    /// </summary>
    /// <param name="a">被减向量。</param>
    /// <param name="b">减数向量。</param>
    /// <returns>两向量之差（a - b）。</returns>
    public static PolarVector2 operator -(PolarVector2 a, PolarVector2 b) => a + -b;

    /// <summary>
    /// 极坐标向量数乘。
    /// <br/>若乘数为正，仅缩放模长；若乘数为负，同时翻转方向。
    /// </summary>
    /// <param name="a">极坐标向量。</param>
    /// <param name="b">标量乘数。</param>
    /// <returns>数乘结果。</returns>
    /// <exception cref="NotFiniteNumberException">乘数为非有限值时抛出。</exception>
    public static PolarVector2 operator *(PolarVector2 a, float b) => b switch
    {
        > 0f => new(a.Radius * b, a.Angle),
        0f => Zero,
        < 0f => new(a.Radius * -b, a.Angle + MathHelper.Pi),
        _ => throw new NotFiniteNumberException(b),
    };

    /// <summary>
    /// 极坐标向量数除。
    /// </summary>
    /// <param name="a">被除数向量。</param>
    /// <param name="b">除数（标量）。</param>
    /// <returns>数除结果。</returns>
    /// <exception cref="DivideByZeroException">除数为 0 时抛出。</exception>
    /// <exception cref="NotFiniteNumberException">除数为非有限值时抛出。</exception>
    public static PolarVector2 operator /(PolarVector2 a, float b) => b switch
    {
        0f => throw new DivideByZeroException(),
        _ => a * (1 / b)
    };

    /// <summary>
    /// 返回将此向量旋转指定角度后的新向量。
    /// </summary>
    /// <param name="offset">旋转的角度偏移量（弧度制，逆时针为正）。</param>
    /// <returns>旋转后的新极坐标向量。</returns>
    public readonly PolarVector2 RotatedBy(float offset) => new(Radius, Angle + offset);

    /// <summary>
    /// 获取两个极坐标向量之间的夹角（绝对值，范围 [0, π]）。
    /// <br/>若任一向量为零向量，返回 0。
    /// </summary>
    /// <param name="a">第一个向量。</param>
    /// <param name="b">第二个向量。</param>
    /// <returns>两向量夹角（弧度制）。</returns>
    public static float IncludedAngle(PolarVector2 a, PolarVector2 b)
    {
        //两个向量中有任意一个为零向量时，返回0。
        if (a == Zero || b == Zero)
            return 0f;

        float angle = Math.Abs(a.Angle - b.Angle);
        return angle switch
        {
            MathHelper.Pi => 0f,
            > MathHelper.Pi => MathHelper.TwoPi - angle,
            _ => angle
        };
    }

    public readonly bool Equals(PolarVector2 other) => Radius == other.Radius && Angle == other.Angle;
    public override readonly bool Equals(object obj) => obj is PolarVector2 other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(Radius, Angle);
    public static bool operator ==(PolarVector2 left, PolarVector2 right) => left.Equals(right);
    public static bool operator !=(PolarVector2 left, PolarVector2 right) => !(left == right);

    /// <summary>
    /// 返回向量的字符串表示形式。
    /// </summary>
    /// <returns>形如 "PolarVector2 { Radius: x, Angle: y }" 的字符串。</returns>
    public override readonly string ToString() => $"PolarVector2 {{ Radius: {Radius}, Angle: {Angle} }}";

    /// <summary>
    /// 极坐标向量点乘（内积）。
    /// </summary>
    /// <param name="a">第一个向量。</param>
    /// <param name="b">第二个向量。</param>
    /// <returns>点乘结果。</returns>
    public static float Dot(PolarVector2 a, PolarVector2 b) => a.Radius * b.Radius * MathF.Cos(IncludedAngle(a, b));

    #region 预定义量

    /// <summary>
    /// 零向量 (0, 0)。
    /// </summary>
    public static readonly PolarVector2 Zero = new(0f, 0f);

    /// <summary>
    /// X 轴正方向单位向量 (1, 0)。
    /// </summary>
    public static readonly PolarVector2 UnitX = new(0f);

    /// <summary>
    /// Y 轴正方向单位向量 (1, π/2)。
    /// </summary>
    public static readonly PolarVector2 UnitY = new(MathHelper.PiOver2);

    /// <summary>
    /// 钟点方向单位向量。
    /// <br/>索引直接对应钟点数，不要使用 <c>Index - 1</c>。
    /// <para/>示例：
    /// <code>
    /// PolarVector2 ClockOne = PolarVector2.UnitClocks[1] * 5f; //一点钟方向，长度为5
    /// 
    /// //遍历十二个方向（注意第一个元素是0点钟方向，索引为0）
    /// foreach (PolarVector2 vector in PolarVector.UnitClocks)
    /// {
    ///     //代码
    /// }
    /// </code>
    /// </summary>
    public static readonly PolarVector2[] UnitClocks =
    [
        //0点钟方向（同12点钟方向）
        -UnitY,
        //1点钟方向 (5π/3)
        new(TOMathUtils.PiOver3 * 5f), 
        //2点钟方向 (11π/6)
        new(TOMathUtils.PiOver6 * 11f),
        //3点钟方向 (0)
        UnitX,
        //4点钟方向 (π/6)
        new(TOMathUtils.PiOver6),
        //5点钟方向 (π/3)
        new(TOMathUtils.PiOver3),
        //6点钟方向 (π/2)
        UnitY,
        //7点钟方向 (2π/3)
        new(TOMathUtils.PiOver3 * 2f),
        //8点钟方向 (5π/6)
        new(TOMathUtils.PiOver6 * 5f),
        //9点钟方向 (π)
        -UnitX,
        //10点钟方向 (7π/6)
        new(TOMathUtils.PiOver6 * 7f),
        //11点钟方向 (4π/3)
        new(TOMathUtils.PiOver3 * 4f),
    ];

    #endregion 预定义量
}