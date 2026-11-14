using Transoceanic.Graphics.Drawing;
using Transoceanic.Graphics.Rendering;
using Transoceanic.Primitives.Mathematics;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>星爆的共享绘制输入、动画包络和光晕校准；攻击配方留在所属视觉实现中。</summary>
internal static class LegendStarburstVisuals
{
    public const int BlurDuration = 20;
    public const int ShakeDuration = 14;

    /// <summary>Age 以伤害起始帧为零；InnerRadius 为零表示实心圆，SigilRadius 为交汇小法阵半径。</summary>
    public readonly record struct Burst(Vector2 Center, float OuterRadius, float InnerRadius, float SigilRadius, float Age);

    public static float BlurStrength(float age) => age < 0f || age >= BlurDuration ? 0f
        : 0.12f * Smooth(Clamp(age / 1.5f)) * MathF.Pow(1f - age / BlurDuration, 2f);

    /// <summary>星爆的世界像素相机偏移；相机、爆点和背景共用同一年龄。</summary>
    public static Vector2 ShakeOffset(float age)
    {
        if (age < 0f || age >= ShakeDuration)
            return Vector2.Zero;
        float amplitude = 9f * (1f - age / ShakeDuration);
        return new Vector2(MathF.Sin(age * 2.7f), MathF.Cos(age * 3.3f) * 0.7f) * amplitude;
    }

    public static float Clamp(float ratio) => Math.Clamp(ratio, 0f, 1f);
    public static float Lerp(float from, float to, float ratio) => MathHelper.Lerp(from, to, ratio);
    public static float Smooth(float ratio) => ratio * ratio * (3f - 2f * ratio);
    public static float OutCubic(float ratio) => TOMathUtils.Interpolation.ExponentialEaseOut(ratio, 3f, false);
    public static float OutQuart(float ratio) => TOMathUtils.Interpolation.ExponentialEaseOut(ratio, 4f, false);
    public static float OutExpo(float ratio) => TOMathUtils.Interpolation.BinaryExponentialEaseOut(ratio, false);
    public static float Bell(float ratio, float rise) => ratio <= 0f || ratio >= 1f ? 0f
        : ratio < rise ? Smooth(ratio / rise) : MathF.Pow(1f - (ratio - rise) / (1f - rise), 1.6f);

    public static BloomSettings GetBloomSettings(float outerRadius, in WorldView2D view, bool dayTime, bool ring)
    {
        // 保留两种设计半径和各自的运算顺序，避免提取公共逻辑时改变光晕校准。
        const float OutputScale = 1.5f;
        const float CircleCalibration = 1.1f;
        float haloScale = ring
            ? OutputScale * view.Scale * outerRadius / LegendStarburstRingVisuals.ReferenceRadius
            : OutputScale * view.Scale * (outerRadius / LegendStarburstCircleVisuals.ReferenceRadius) / CircleCalibration;
        BloomSettings defaults = BloomSettings.Default;
        return WithBody(defaults with
        {
            NearRadius = defaults.NearRadius * haloScale,
            FarRadius = defaults.FarRadius * haloScale
        }, dayTime);
    }

    public static BloomSettings WithBody(BloomSettings settings, bool dayTime) => settings with
    {
        AdditiveBody = !dayTime,
        BodyPasses = dayTime ? 2 : 1
    };
}
