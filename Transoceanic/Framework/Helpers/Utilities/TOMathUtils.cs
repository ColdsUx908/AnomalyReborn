using System.Numerics;

namespace Transoceanic.Framework.Helpers;

public static partial class TOMathUtils
{
    public const float PiOver3 = MathHelper.Pi / 3f;
    public const float PiOver5 = MathHelper.Pi / 5f;
    public const float PiOver6 = MathHelper.Pi / 6f;
    public const float PiOver8 = MathHelper.Pi / 8f;
    public const float PiOver10 = MathHelper.Pi / 10f;
    public const float PiOver12 = MathHelper.Pi / 12f;
    public const float PiOver16 = MathHelper.Pi / 16f;
    public const float PiOver24 = MathHelper.Pi / 24f;
    public const float PiOver30 = MathHelper.Pi / 30f;

    public const float Sqrt2 = 1.414213562f;
    public const float Sqrt3 = 1.732050808f;
    public const float Sqrt5 = 2.236067978f;

    /// <summary>
    /// 将输入参数规范化到 [0, <paramref name="period"/>) 范围内。
    /// </summary>
    public static float NormalizeWithPeriod(float value, float period = MathHelper.TwoPi)
    {
        float temp = value % period;
        return temp < 0 ? temp + period : temp;
    }

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

    public static T Min<T>(T value, params ReadOnlySpan<T> values) where T : INumber<T>
    {
        foreach (T temp in values)
        {
            if (temp < value)
                value = temp;
        }
        return value;
    }

    public static T Max<T>(T value, params ReadOnlySpan<T> values) where T : INumber<T>
    {
        foreach (T temp in values)
        {
            if (temp > value)
                value = temp;
        }
        return value;
    }

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
    public static void NormalizeMinMax<T>(ref T left, ref T right) where T : INumber<T>
    {
        if (left > right)
            Utils.Swap(ref left, ref right);
    }

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

    public static (int integer, float fractional) SplitFloat(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be a finite number.");
        int integerPart = (int)value;
        return (integerPart, value - integerPart);
    }
}