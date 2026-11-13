namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 星爆绘制的共享输入类型；只依赖 XNA 类型，离线预览可直接编译本文件。
/// </summary>
internal static class LegendStarburstVisuals
{
    /// <summary>屏幕变换：屏幕左上角世界坐标、世界偏移到屏幕像素的变换和屏幕尺寸。</summary>
    public readonly record struct View(Vector2 ScreenPosition, Matrix Transform, Vector2 Resolution);

    /// <summary>
    /// 一次星爆的绘制输入。InnerRadius 为 0 时为实心圆；SigilRadius 为交汇小法阵半径；
    /// 两个旋转分别取慢、快法阵自转角；Age 以伤害起始帧为 0；Seed 区分各次星爆。
    /// </summary>
    public readonly record struct Burst(Vector2 Center, float OuterRadius, float InnerRadius, float SigilRadius,
        float SlowRotation, float FastRotation, float Age, int Seed);
}
