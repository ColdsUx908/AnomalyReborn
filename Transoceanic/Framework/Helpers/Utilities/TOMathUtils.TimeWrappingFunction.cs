namespace Transoceanic.Framework.Helpers;

public static partial class TOMathUtils
{
    /// <summary>
    /// 提供一些以 <see cref="TOSharedData.TotalSeconds"/> 为自变量的函数。
    /// </summary>
    public static class TimeWrappingFunction
    {
        /// <summary>
        /// 生成一个形如 <c>y = A * sin(</c>ω<c>t + </c>φ<c>)</c> 的正弦波。其中 <c>t</c> 随游戏内时间变化，单位是秒。
        /// </summary>
        /// <param name="amplitude">振幅 <c>A</c>。默认为 <c>1</c>。</param>
        /// <param name="angularFrequency">角频率 ω。默认为 <c>1</c>，表示周期为 <c>2π</c>s。</param>
        /// <param name="initialPhase">初相 φ。默认为 <c>0</c>。</param>
        /// <param name="unsigned">是否将结果转换为非负值（将结果整体加上 <c>A / 2</c>）。默认为 <see langword="false"/>。</param>
        public static float GetTimeSin(float amplitude = 1f, float angularFrequency = 1f, float initialPhase = 0f, bool unsigned = false) => (MathF.Sin(TOSharedData.TotalSeconds * angularFrequency + initialPhase) + unsigned.ToInt()) * amplitude;

        /// <summary>
        /// 生成形如 <c>(Sin, Cos) = (A * sin(</c>ω<c>t + </c>φ<c>), A * cos(</c>ω<c>t + </c>φ<c>))</c> 的正余弦波。其中 <c>t</c> 随游戏内时间变化，单位是秒。
        /// </summary>
        /// <param name="amplitude">振幅 <c>A</c>。默认为 <c>1</c>。</param>
        /// <param name="angularFrequency">角频率 ω。默认为 <c>1</c>，表示周期为 <c>2π</c>s。</param>
        /// <param name="initialPhase">初相 φ。默认为 <c>0</c>。</param>
        /// <param name="unsigned">是否将结果转换为非负值（将结果整体加上 <c>A / 2</c>）。默认为 <see langword="false"/>。</param>
        public static (float Sin, float Cos) GetTimeSinCos(float amplitude = 1f, float angularFrequency = 1f, float initialPhase = 0f, bool unsigned = false)
        {
            (float sin, float cos) = MathF.SinCos(TOSharedData.TotalSeconds * angularFrequency + initialPhase);
            return ((sin + unsigned.ToInt()) * amplitude, (cos + unsigned.ToInt()) * amplitude);
        }
    }
}