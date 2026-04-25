// Designed by ColdsUx

using System.Numerics;

namespace Transoceanic.Framework.Helpers;

/// <summary>
/// 提供丰富的数学工具集。
/// </summary>
/// <remarks>
/// 本类按功能领域划分为多个嵌套静态类：
/// <list type="bullet">
/// <item><see cref="BitOperation"/>：针对多种整数类型的位读取与修改操作。</item>
/// <item><see cref="Geometry"/>：二维几何计算，包含距离测算与碰撞检测。</item>
/// <item><see cref="Interpolation"/>：常用缓动函数（Easing Functions）的实现。</item>
/// <item><see cref="PolarEquation"/>：基于极坐标的曲线方程。</item>
/// <item><see cref="TimeWrappingFunction"/>：以游戏运行时间为自变量的周期波形生成。</item>
/// <item><see cref="UnitConversion"/>：物理单位换算。</item>
/// </list>
/// 同时，本类还直接提供常用的基础数学方法（请参见 <see cref="TOMathUtils"/> 的主成员）。
/// </remarks>
public static partial class TOMathUtils
{
    /// <summary>
    /// π / 3 的常量值。
    /// </summary>
    public const float PiOver3 = MathHelper.Pi / 3f;

    /// <summary>
    /// π / 5 的常量值。
    /// </summary>
    public const float PiOver5 = MathHelper.Pi / 5f;

    /// <summary>
    /// π / 6 的常量值。
    /// </summary>
    public const float PiOver6 = MathHelper.Pi / 6f;

    /// <summary>
    /// π / 8 的常量值。
    /// </summary>
    public const float PiOver8 = MathHelper.Pi / 8f;

    /// <summary>
    /// π / 10 的常量值。
    /// </summary>
    public const float PiOver10 = MathHelper.Pi / 10f;

    /// <summary>
    /// π / 12 的常量值。
    /// </summary>
    public const float PiOver12 = MathHelper.Pi / 12f;

    /// <summary>
    /// π / 16 的常量值。
    /// </summary>
    public const float PiOver16 = MathHelper.Pi / 16f;

    /// <summary>
    /// π / 24 的常量值。
    /// </summary>
    public const float PiOver24 = MathHelper.Pi / 24f;

    /// <summary>
    /// π / 30 的常量值。
    /// </summary>
    public const float PiOver30 = MathHelper.Pi / 30f;

    /// <summary>
    /// √2 的近似值 (1.414213562)。
    /// </summary>
    public const float Sqrt2 = 1.414213562f;

    /// <summary>
    /// √3 的近似值 (1.732050808)。
    /// </summary>
    public const float Sqrt3 = 1.732050808f;

    /// <summary>
    /// √5 的近似值 (2.236067978)。
    /// </summary>
    public const float Sqrt5 = 2.236067978f;

    /// <summary>
    /// 将输入参数规范化到 [0, <paramref name="period"/>) 范围内。
    /// </summary>
    /// <param name="value">要规范化的值。</param>
    /// <param name="period">周期长度，默认为 2π。</param>
    /// <returns>规范化后的值，范围 [0, period)。</returns>
    public static float NormalizeWithPeriod(float value, float period = MathHelper.TwoPi)
    {
        float temp = value % period;
        return temp < 0 ? temp + period : temp;
    }

    /// <summary>
    /// 计算两个角度（或任意周期量）之间的最短差值（带符号）。
    /// </summary>
    /// <param name="from">起始值。</param>
    /// <param name="to">目标值。</param>
    /// <param name="period">周期长度，默认为 2π。</param>
    /// <returns>
    /// 从 <paramref name="from"/> 到 <paramref name="to"/> 的最短差值，
    /// 结果范围在 [-<paramref name="period"/>/2, <paramref name="period"/>/2) 之间。
    /// 正数表示沿正方向最近，负数表示沿负方向最近。
    /// </returns>
    /// <remarks>
    /// 此方法先将输入规范化到同一周期内，然后计算差值并选择绝对值较小的方向。
    /// 适用于角度插值、旋转方向判断等场景。
    /// </remarks>
    public static float ShortestDifference(float from, float to, float period = MathHelper.TwoPi)
    {
        from = NormalizeWithPeriod(from, period);
        to = NormalizeWithPeriod(to, period);

        float delta = to - from;

        if (delta > period / 2f)
            delta -= period;
        else if (delta <= period / -2f)
            delta += period;

        return delta;
    }

    /// <summary>
    /// 返回给定值和只读跨度中的最小值。
    /// </summary>
    /// <typeparam name="T">实现了 <see cref="INumber{T}"/> 接口的数值类型。</typeparam>
    /// <param name="value">要比较的第一个值。</param>
    /// <param name="values">要参与比较的其他值的只读跨度。</param>
    /// <returns>所有输入值中的最小值。</returns>
    public static T Min<T>(T value, params ReadOnlySpan<T> values) where T : INumber<T>
    {
        foreach (T temp in values)
        {
            if (temp < value)
                value = temp;
        }
        return value;
    }

    /// <summary>
    /// 返回给定值和只读跨度中的最大值。
    /// </summary>
    /// <typeparam name="T">实现了 <see cref="INumber{T}"/> 接口的数值类型。</typeparam>
    /// <param name="value">要比较的第一个值。</param>
    /// <param name="values">要参与比较的其他值的只读跨度。</param>
    /// <returns>所有输入值中的最大值。</returns>
    public static T Max<T>(T value, params ReadOnlySpan<T> values) where T : INumber<T>
    {
        foreach (T temp in values)
        {
            if (temp > value)
                value = temp;
        }
        return value;
    }

    /// <summary>
    /// 同时返回给定值和只读跨度中的最小值和最大值。
    /// </summary>
    /// <typeparam name="T">实现了 <see cref="INumber{T}"/> 接口的数值类型。</typeparam>
    /// <param name="value">要比较的第一个值。</param>
    /// <param name="values">要参与比较的其他值的只读跨度。</param>
    /// <returns>一个元组，包含所有输入值中的最小值和最大值。</returns>
    public static (T Min, T Max) MinMax<T>(T value, params ReadOnlySpan<T> values) where T : INumber<T>
    {
        T min = value, max = value;
        foreach (T temp in values)
        {
            if (temp < min)
                min = temp;
            if (temp > max)
                max = temp;
        }
        return (min, max);
    }

    /// <summary>
    /// 必要时交换 <paramref name="left"/> 和 <paramref name="right"/> 的值，确保 <paramref name="left"/> 不大于 <paramref name="right"/>。
    /// </summary>
    /// <typeparam name="T">实现了 <see cref="INumber{T}"/> 接口的数值类型。</typeparam>
    /// <param name="left">第一个值，方法返回时该值将小于或等于 <paramref name="right"/>。</param>
    /// <param name="right">第二个值，方法返回时该值将大于或等于 <paramref name="left"/>。</param>
    public static void NormalizeMinMax<T>(ref T left, ref T right) where T : INumber<T>
    {
        if (left > right)
            Utils.Swap(ref left, ref right);
    }

    /// <summary>
    /// 检查给定的布尔跨度中是否至少有 <paramref name="x"/> 个值为 <see langword="true"/>。
    /// </summary>
    /// <param name="x">所需的最小 true 数量。</param>
    /// <param name="span">要检查的布尔值只读跨度。</param>
    /// <returns>
    /// 如果 <paramref name="span"/> 中值为 <see langword="true"/> 的元素数量大于或等于 <paramref name="x"/>，则返回 <see langword="true"/>；
    /// 如果 <paramref name="x"/> 小于等于 0，则总是返回 <see langword="true"/>；
    /// 如果 <paramref name="span"/> 的长度小于 <paramref name="x"/>，则返回 <see langword="false"/>。
    /// </returns>
    /// <remarks>
    /// 此方法采用短路求值：一旦找到足够数量的 true 值即返回，不会遍历整个跨度。
    /// </remarks>
    public static bool AtLeastXTrue(int x, params ReadOnlySpan<bool> span)
    {
        if (x <= 0)
            return true;
        if (span.Length < x)
            return false;

        int count = 0;
        foreach (bool value in span)
        {
            if (value && ++count >= x)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 将浮点数拆分为整数部分和小数部分。
    /// </summary>
    /// <param name="value">要拆分的浮点数，必须为有限值。</param>
    /// <returns>一个元组，包含整数部分和正的小数部分（范围 [0, 1)）。</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// 当 <paramref name="value"/> 为 <see cref="float.NaN"/> 或 <see cref="float.PositiveInfinity"/> 或 <see cref="float.NegativeInfinity"/> 时抛出。
    /// </exception>
    /// <remarks>
    /// 对于负数，例如 -3.14，将返回整数部分 -3 和小数部分 -0.14（即 value - integerPart），
    /// 因此小数部分仍然携带原始符号。
    /// </remarks>
    public static (int integer, float fractional) SplitFloat(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be a finite number.");
        int integerPart = (int)value;
        return (integerPart, value - integerPart);
    }
}