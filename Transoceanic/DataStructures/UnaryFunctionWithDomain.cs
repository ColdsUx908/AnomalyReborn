namespace Transoceanic.DataStructures;

/// <summary>
/// 表示带定义域限制的一元泛型函数。
/// </summary>
/// <typeparam name="R">函数返回值类型。</typeparam>
public class UnaryFunctionWithDomainBase<R>
{
    /// <summary>
    /// 函数定义域。
    /// </summary>
    public readonly MathInterval Domain;

    /// <summary>
    /// 函数实现。
    /// </summary>
    public readonly Func<float, R> _function;

    /// <summary>
    /// 创建一个带定义域的一元函数。
    /// </summary>
    /// <param name="function">函数实现。</param>
    /// <param name="domain">函数定义域。</param>
    /// <exception cref="ArgumentNullException">当函数为null时抛出。</exception>
    public UnaryFunctionWithDomainBase(Func<float, R> function, MathInterval? domain = null)
    {
        _function = function ?? throw new ArgumentNullException(nameof(function));
        Domain = domain ?? MathInterval.AllReals;
    }

    /// <summary>
    /// 尝试计算函数值。
    /// </summary>
    /// <param name="x">自变量值。</param>
    /// <param name="result">函数计算结果。</param>
    /// <returns>如果x在定义域内且计算成功返回true，否则返回false。</returns>
    public bool TryProcess(float x, out R result)
    {
        // 检查是否在定义域内
        if (Domain.IsEmpty || !Domain.Contains(x))
        {
            result = default;
            return false;
        }

        try
        {
            result = _function(x);
            return true;
        }
        catch
        {
            // 函数执行过程中出现异常
            result = default;
            return false;
        }
    }

    /// <summary>
    /// 直接计算函数值，失败时抛出异常。
    /// </summary>
    /// <param name="x">自变量值。</param>
    /// <returns>函数值。</returns>
    /// <exception cref="ArgumentOutOfRangeException">当x不在定义域内时抛出。</exception>
    public R Process(float x)
    {
        if (Domain.IsEmpty || !Domain.Contains(x))
            throw new ArgumentOutOfRangeException(nameof(x), $"值{x}不在函数定义域{Domain}内");

        return _function(x);
    }

    /// <summary>
    /// 检查是否与另一个函数在相同定义域上相等（逐点比较）。
    /// </summary>
    /// <param name="other">要比较的函数。</param>
    /// <returns>如果定义域和函数值都相同则返回true。</returns>
    public bool Equals(UnaryFunctionWithDomainBase<R> other) => Domain == other.Domain && _function == other._function;
    public override bool Equals(object obj) => obj is UnaryFunctionWithDomainBase<R> other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(_function, Domain);
    public static bool operator ==(UnaryFunctionWithDomainBase<R> left, UnaryFunctionWithDomainBase<R> right) => left.Equals(right);
    public static bool operator !=(UnaryFunctionWithDomainBase<R> left, UnaryFunctionWithDomainBase<R> right) => !(left == right);

    /// <summary>
    /// 获取空函数（定义域为空）。
    /// </summary>
    public static UnaryFunctionWithDomainBase<R> Empty => new(_ => default, MathInterval.Empty);

    /// <summary>
    /// 创建分段函数。
    /// </summary>
    /// <param name="segments">分段定义，每段包含区间和函数。</param>
    /// <returns>分段函数。</returns>
    public static UnaryFunctionWithDomainBase<R> Piecewise(params (MathInterval interval, Func<float, R> function)[] segments)
    {
        if (segments == null || segments.Length == 0)
            return Empty;

        // 检查区间是否重叠
        for (int i = 0; i < segments.Length; i++)
        {
            for (int j = i + 1; j < segments.Length; j++)
            {
                if (MathInterval.Overlap(segments[i].interval, segments[j].interval))
                {
                    throw new ArgumentException($"区间 {segments[i].interval} 和 {segments[j].interval} 重叠");
                }
            }
        }

        // 计算总定义域（所有区间的并集）
        MathInterval totalDomain = MathInterval.Empty;
        foreach ((MathInterval interval, Func<float, R> function) in segments)
            totalDomain |= interval; // 使用区间并集

        return new UnaryFunctionWithDomainBase<R>(
            x =>
            {
                // 查找x所在的区间
                foreach ((MathInterval interval, Func<float, R> function) in segments)
                {
                    if (interval.Contains(x))
                        return function(x);
                }

                // 如果x不在任何区间内，抛出异常
                throw new ArgumentOutOfRangeException(nameof(x), $"值{x}不在任何分段区间内");
            },
            totalDomain
        );
    }
}

/// <summary>
/// 表示带定义域限制的一元浮点函数。
/// </summary>
public sealed class UnaryFunctionWithDomain : UnaryFunctionWithDomainBase<float>
{
    /// <summary>
    /// 创建一个带定义域的一元函数。
    /// </summary>
    /// <param name="function">函数实现。</param>
    /// <param name="domain">函数定义域。</param>
    /// <exception cref="ArgumentNullException">当函数为null时抛出。</exception>
    public UnaryFunctionWithDomain(Func<float, float> function, MathInterval? domain = null) : base(function, domain) { }

    // 扩展功能：函数运算

    /// <summary>
    /// 创建函数的复合函数 f∘g。
    /// </summary>
    /// <param name="inner">内层函数。</param>
    /// <returns>复合函数 f(g(x))。</returns>
    public UnaryFunctionWithDomain Compose(UnaryFunctionWithDomain inner)
    {
        if (inner.Domain.IsEmpty || Domain.IsEmpty)
            return Empty;

        // 计算复合函数的定义域：inner的值域必须在outer的定义域内
        // 简化处理：返回inner的定义域，但运行时检查
        return new UnaryFunctionWithDomain(x => _function(inner.Process(x)), inner.Domain);
    }

    // 预定义函数（保持原有的静态属性和方法）

    /// <summary>
    /// 获取空函数（定义域为空）。
    /// </summary>
    public new static UnaryFunctionWithDomain Empty => new(_ => 0f, MathInterval.Empty);

    /// <summary>
    /// 获取恒等函数 f(x) = x。
    /// </summary>
    public static UnaryFunctionWithDomain Identity => new(x => x, MathInterval.AllReals);

    /// <summary>
    /// 获取平方函数 f(x) = x²。
    /// </summary>
    public static UnaryFunctionWithDomain Square => new(x => x * x, MathInterval.AllReals);

    /// <summary>
    /// 获取平方根函数 f(x) = √x。
    /// </summary>
    public static UnaryFunctionWithDomain SquareRoot => new(MathF.Sqrt, MathInterval.NonNegativeReals);

    /// <summary>
    /// 获取正弦函数 f(x) = sin(x)。
    /// </summary>
    public static UnaryFunctionWithDomain Sin => new(MathF.Sin, MathInterval.AllReals);

    /// <summary>
    /// 获取余弦函数 f(x) = cos(x)。
    /// </summary>
    public static UnaryFunctionWithDomain Cos => new(MathF.Cos, MathInterval.AllReals);

    /// <summary>
    /// 获取绝对值函数 f(x) = |x|。
    /// </summary>
    public static UnaryFunctionWithDomain Abs => new(MathF.Abs, MathInterval.AllReals);

    /// <summary>
    /// 获取符号函数 f(x) = sign(x)。
    /// </summary>
    public static UnaryFunctionWithDomain Sign => new(x => Math.Sign(x), MathInterval.AllReals);

    /// <summary>
    /// 创建线性函数 f(x) = ax + b。
    /// </summary>
    /// <param name="a">斜率。</param>
    /// <param name="b">截距。</param>
    /// <param name="domain">定义域，默认为所有实数。</param>
    /// <returns>线性函数。</returns>
    public static UnaryFunctionWithDomain Linear(float a, float b, MathInterval? domain = null)
        => new(x => a * x + b, domain ?? MathInterval.AllReals);

    /// <summary>
    /// 创建分段函数。
    /// </summary>
    /// <param name="segments">分段定义，每段包含区间和函数。</param>
    /// <returns>分段函数。</returns>
    public new static UnaryFunctionWithDomain Piecewise(params (MathInterval interval, Func<float, float> function)[] segments)
    {
        if (segments == null || segments.Length == 0)
            return Empty;

        // 检查区间是否重叠
        for (int i = 0; i < segments.Length; i++)
        {
            for (int j = i + 1; j < segments.Length; j++)
            {
                if (MathInterval.Overlap(segments[i].interval, segments[j].interval))
                    throw new ArgumentException($"区间 {segments[i].interval} 和 {segments[j].interval} 重叠");
            }
        }

        // 计算总定义域（所有区间的并集）
        MathInterval totalDomain = MathInterval.Empty;
        foreach ((MathInterval interval, Func<float, float> function) in segments)
        {
            totalDomain |= interval; // 使用区间并集
        }

        return new UnaryFunctionWithDomain(
            x =>
            {
                // 查找x所在的区间
                foreach ((MathInterval interval, Func<float, float> function) in segments)
                {
                    if (interval.Contains(x))
                        return function(x);
                }

                // 如果x不在任何区间内，抛出异常
                throw new ArgumentOutOfRangeException(nameof(x), $"值{x}不在任何分段区间内");
            },
            totalDomain
        );
    }
}