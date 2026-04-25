// Designed by ColdsUx

namespace Transoceanic.Framework.Helpers;

public static partial class TOMathUtils
{
    /// <summary>
    /// 提供常用插值缓动函数（Easing Functions）的实现。
    /// </summary>
    /// <remarks>
    /// 所有方法均接受一个进度比率 <paramref name="ratio"/>（通常范围为 [0, 1]），并返回应用缓动曲线后的插值系数。
    /// 重载方法可直接在起始值 <paramref name="from"/> 和结束值 <paramref name="to"/> 之间进行插值。
    /// </remarks>
    public static class Interpolation
    {
        /// <summary>
        /// 二次方缓入（Quadratic Ease In）。
        /// </summary>
        /// <param name="ratio">插值进度，通常范围 [0, 1]。</param>
        /// <param name="clamped">是否将 <paramref name="ratio"/> 限制在 [0, 1] 范围内。默认为 <see langword="true"/>。</param>
        /// <returns>经过缓入处理的插值系数。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float QuadraticEaseIn(float ratio, bool clamped = true)
        {
            if (clamped)
                ratio = Math.Clamp(ratio, 0f, 1f);
            return ratio * ratio;
        }

        /// <inheritdoc cref="QuadraticEaseIn(float, bool)"/>
        /// <param name="from">起始值。</param>
        /// <param name="to">结束值。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float QuadraticEaseIn(float from, float to, float ratio, bool clamped = true) => from + (to - from) * QuadraticEaseIn(ratio, clamped);

        /// <summary>
        /// 二次方缓出（Quadratic Ease Out）。
        /// </summary>
        /// <inheritdoc cref="QuadraticEaseIn(float, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float QuadraticEaseOut(float ratio, bool clamped = true)
        {
            if (clamped)
                ratio = Math.Clamp(ratio, 0f, 1f);
            return ratio * (2f - ratio);
        }

        /// <inheritdoc cref="QuadraticEaseOut(float, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float QuadraticEaseOut(float from, float to, float ratio, bool clamped = true) => from + (to - from) * QuadraticEaseOut(ratio, clamped);

        /// <summary>
        /// 二次方缓入缓出（Quadratic Ease In Out）。
        /// </summary>
        /// <inheritdoc cref="QuadraticEaseIn(float, bool)"/>
        public static float QuadraticEaseInOut(float ratio, bool clamped = true)
        {
            if (clamped)
                ratio = Math.Clamp(ratio, 0f, 1f);
            return ratio < 0.5f ? 2f * ratio * ratio : -2f * ratio * ratio + 4f * ratio - 1f;
        }

        /// <inheritdoc cref="QuadraticEaseInOut(float, bool)"/>
        public static float QuadraticEaseInOut(float from, float to, float ratio, bool clamped = true) => from + (to - from) * QuadraticEaseInOut(ratio, clamped);

        /// <summary>
        /// 三次方缓入（Cubic Ease In）。
        /// </summary>
        /// <inheritdoc cref="QuadraticEaseIn(float, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CubicEaseIn(float ratio, bool clamped = true)
        {
            if (clamped)
                ratio = Math.Clamp(ratio, 0f, 1f);
            return ratio * ratio * ratio;
        }

        /// <inheritdoc cref="CubicEaseIn(float, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CubicEaseIn(float from, float to, float ratio, bool clamped = true) => from + (to - from) * CubicEaseIn(ratio, clamped);

        /// <summary>
        /// 三次方缓出（Cubic Ease Out）。
        /// </summary>
        /// <inheritdoc cref="QuadraticEaseIn(float, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CubicEaseOut(float ratio, bool clamped = true)
        {
            if (clamped)
                ratio = Math.Clamp(ratio, 0f, 1f);
            float inv = 1f - ratio;
            return 1f - inv * inv * inv;
        }

        /// <inheritdoc cref="CubicEaseOut(float, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CubicEaseOut(float from, float to, float ratio, bool clamped = true) => from + (to - from) * CubicEaseOut(ratio, clamped);

        /// <summary>
        /// 三次方缓入缓出（Cubic Ease In Out）。
        /// </summary>
        /// <inheritdoc cref="QuadraticEaseIn(float, bool)"/>
        public static float CubicEaseInOut(float ratio, bool clamped = true)
        {
            if (clamped)
                ratio = Math.Clamp(ratio, 0f, 1f);
            return ratio < 0.5f ? 4f * ratio * ratio * ratio : 4f * ratio * ratio * ratio - 12f * ratio * ratio + 12f * ratio - 3f;
        }

        /// <inheritdoc cref="CubicEaseInOut(float, bool)"/>
        public static float CubicEaseInOut(float from, float to, float ratio, bool clamped = true) => from + (to - from) * CubicEaseInOut(ratio, clamped);

        /// <summary>
        /// 指数缓入（Exponential Ease In）。
        /// </summary>
        /// <param name="ratio">插值进度，通常范围 [0, 1]。</param>
        /// <param name="exponent">指数幂次，例如 2.0 表示平方缓入。</param>
        /// <param name="clamped">是否将 <paramref name="ratio"/> 限制在 [0, 1] 范围内。默认为 <see langword="true"/>。</param>
        /// <returns>经过缓入处理的插值系数。</returns>
        public static float ExponentialEaseIn(float ratio, float exponent, bool clamped = true)
        {
            if (clamped)
                ratio = Math.Clamp(ratio, 0f, 1f);
            return MathF.Pow(ratio, exponent);
        }

        /// <inheritdoc cref="ExponentialEaseIn(float, float, bool)"/>
        public static float ExponentialEaseIn(float from, float to, float ratio, float exponent, bool clamped = true) => from + (to - from) * ExponentialEaseIn(ratio, exponent, clamped);

        /// <summary>
        /// 指数缓出（Exponential Ease Out）。
        /// </summary>
        /// <inheritdoc cref="ExponentialEaseIn(float, float, bool)"/>
        public static float ExponentialEaseOut(float ratio, float exponent, bool clamped = true)
        {
            if (clamped)
                ratio = Math.Clamp(ratio, 0f, 1f);
            return 1f - MathF.Pow(1f - ratio, exponent);
        }

        /// <inheritdoc cref="ExponentialEaseOut(float, float, bool)"/>
        public static float ExponentialEaseOut(float from, float to, float ratio, float exponent, bool clamped = true) => from + (to - from) * ExponentialEaseOut(ratio, exponent, clamped);

        /// <summary>
        /// 指数缓入缓出（Exponential Ease In Out）。
        /// </summary>
        /// <inheritdoc cref="ExponentialEaseIn(float, float, bool)"/>
        public static float ExponentialEaseInOut(float ratio, float exponent, bool clamped = true)
        {
            if (clamped)
                ratio = Math.Clamp(ratio, 0f, 1f);
            return ratio < 0.5f ? 0.5f * MathF.Pow(2f * ratio, exponent) : 1f - 0.5f * MathF.Pow(2f * (1f - ratio), exponent);
        }

        /// <inheritdoc cref="ExponentialEaseInOut(float, float, bool)"/>
        public static float ExponentialEaseInOut(float from, float to, float ratio, float exponent, bool clamped = true) => from + (to - from) * ExponentialEaseInOut(ratio, exponent, clamped);

        /// <summary>
        /// 正弦缓入（Sine Ease In）。
        /// </summary>
        /// <param name="ratio">插值进度，通常范围 [0, 1]。</param>
        /// <param name="clamped">是否将 <paramref name="ratio"/> 限制在 [0, 1] 范围内。默认为 <see langword="true"/>。</param>
        /// <returns>经过缓入处理的插值系数，计算公式为 <c>1 - cos(π/2 * x)</c>。</returns>
        public static float SineEaseIn(float ratio, bool clamped = true)
        {
            if (clamped)
                ratio = Math.Clamp(ratio, 0f, 1f);
            return 1f - MathF.Cos(ratio * MathHelper.PiOver2);
        }

        /// <inheritdoc cref="SineEaseIn(float, bool)"/>
        public static float SineEaseIn(float from, float to, float ratio, bool clamped = true) => from + (to - from) * SineEaseIn(ratio, clamped);

        /// <summary>
        /// 正弦缓出（Sine Ease Out）。
        /// </summary>
        /// <param name="ratio">插值进度，通常范围 [0, 1]。</param>
        /// <param name="clamped">是否将 <paramref name="ratio"/> 限制在 [0, 1] 范围内。默认为 <see langword="true"/>。</param>
        /// <returns>经过缓出处理的插值系数，计算公式为 <c>sin(π/2 * x)</c>。</returns>
        public static float SineEaseOut(float ratio, bool clamped = true)
        {
            if (clamped)
                ratio = Math.Clamp(ratio, 0f, 1f);
            return MathF.Sin(ratio * MathHelper.PiOver2);
        }

        /// <inheritdoc cref="SineEaseOut(float, bool)"/>
        public static float SineEaseOut(float from, float to, float ratio, bool clamped = true) => from + (to - from) * SineEaseOut(ratio, clamped);

        /// <summary>
        /// 正弦缓入缓出（Sine Ease In Out）。
        /// </summary>
        /// <param name="ratio">插值进度，通常范围 [0, 1]。</param>
        /// <param name="clamped">是否将 <paramref name="ratio"/> 限制在 [0, 1] 范围内。默认为 <see langword="true"/>。</param>
        /// <returns>经过缓入缓出处理的插值系数，计算公式为 <c>0.5 - 0.5 * cos(π * x)</c>。</returns>
        public static float SineEaseInOut(float ratio, bool clamped = true)
        {
            if (clamped)
                ratio = Math.Clamp(ratio, 0f, 1f);
            return 0.5f - 0.5f * MathF.Cos(ratio * MathHelper.Pi);
        }

        /// <inheritdoc cref="SineEaseInOut(float, bool)"/>
        public static float SineEaseInOut(float from, float to, float ratio, bool clamped = true) => from + (to - from) * SineEaseInOut(ratio, clamped);

        /// <summary>
        /// 对数缓入（Logarithmic Ease In）。
        /// </summary>
        /// <param name="ratio">插值进度，通常范围 [0, 1]。</param>
        /// <param name="clamped">是否将 <paramref name="ratio"/> 限制在 [0, 1] 范围内。默认为 <see langword="true"/>。</param>
        /// <returns>经过缓入处理的插值系数，计算公式为 <c>1 - ln((e - 1) * (1 - x) + 1)</c>。</returns>
        public static float LogarithmicEaseIn(float ratio, bool clamped = true)
        {
            if (clamped)
                ratio = Math.Clamp(ratio, 0f, 1f);
            return 1f - MathF.Log((MathF.E - 1f) * (1f - ratio) + 1f);
        }

        /// <inheritdoc cref="LogarithmicEaseIn(float, bool)"/>
        public static float LogarithmicEaseIn(float from, float to, float ratio, bool clamped = true) => from + (to - from) * LogarithmicEaseIn(ratio, clamped);

        /// <summary>
        /// 对数缓出（Logarithmic Ease Out）。
        /// </summary>
        /// <param name="ratio">插值进度，通常范围 [0, 1]。</param>
        /// <param name="clamped">是否将 <paramref name="ratio"/> 限制在 [0, 1] 范围内。默认为 <see langword="true"/>。</param>
        /// <returns>经过缓出处理的插值系数，计算公式为 <c>ln((e - 1) * x + 1)</c>。</returns>
        public static float LogarithmicEaseOut(float ratio, bool clamped = true)
        {
            if (clamped)
                ratio = Math.Clamp(ratio, 0f, 1f);
            return MathF.Log((MathF.E - 1f) * ratio + 1f);
        }

        /// <inheritdoc cref="LogarithmicEaseOut(float, bool)"/>
        public static float LogarithmicEaseOut(float from, float to, float ratio, bool clamped = true) => from + (to - from) * LogarithmicEaseOut(ratio, clamped);

        /// <summary>
        /// 对数缓入缓出（Logarithmic Ease In Out）。
        /// </summary>
        /// <param name="ratio">插值进度，通常范围 [0, 1]。</param>
        /// <param name="clamped">是否将 <paramref name="ratio"/> 限制在 [0, 1] 范围内。默认为 <see langword="true"/>。</param>
        /// <returns>经过缓入缓出处理的插值系数，前半段为对数缓入，后半段为对数缓出。</returns>
        public static float LogarithmicEaseInOut(float ratio, bool clamped = true)
        {
            if (clamped)
                ratio = Math.Clamp(ratio, 0f, 1f);
            return ratio < 0.5f ? 0.5f * (1f - MathF.Log((MathF.E - 1f) * (1f - 2f * ratio) + 1f)) : 0.5f * MathF.Log((MathF.E - 1f) * (2f * ratio - 1f) + 1f) + 0.5f;
        }

        /// <inheritdoc cref="LogarithmicEaseInOut(float, bool)"/>
        public static float LogarithmicEaseInOut(float from, float to, float ratio, bool clamped = true) => from + (to - from) * LogarithmicEaseInOut(ratio, clamped);

        /// <summary>
        /// 更平滑的插值（Smoother Step），基于五次多项式。
        /// </summary>
        /// <param name="ratio">插值进度，通常范围 [0, 1]。</param>
        /// <param name="clamped">是否将 <paramref name="ratio"/> 限制在 [0, 1] 范围内。默认为 <see langword="true"/>。</param>
        /// <returns>应用五次多项式平滑后的插值系数。</returns>
        public static float SmootherStep(float ratio, bool clamped = true)
        {
            if (clamped)
                ratio = Math.Clamp(ratio, 0f, 1f);
            return ratio * ratio * ratio * (ratio * (ratio * 6f - 15f) + 10f);
        }

        /// <inheritdoc cref="SmootherStep(float, bool)"/>
        public static float SmootherStep(float from, float to, float ratio, bool clamped = true) => from + (to - from) * SmootherStep(ratio, clamped);
    }
}