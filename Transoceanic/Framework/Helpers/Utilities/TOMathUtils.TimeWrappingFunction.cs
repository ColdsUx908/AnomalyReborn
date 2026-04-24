namespace Transoceanic.Framework.Helpers;

public static partial class TOMathUtils
{
    /// <summary>
    /// 提供以游戏运行总时间 <see cref="TOSharedData.TotalSeconds"/> 为自变量的波形生成函数。
    /// </summary>
    public static class TimeWrappingFunction
    {
        /// <summary>
        /// 生成形如 <c>y = A * sin(ωt + φ)</c> 的正弦波，其中 <c>t</c> 为游戏运行总秒数。
        /// </summary>
        /// <param name="amplitude">振幅 A，默认为 1。</param>
        /// <param name="angularFrequency">角频率 ω，默认为 1（周期为 2π 秒）。</param>
        /// <param name="initialPhase">初相 φ，默认为 0。</param>
        /// <param name="unsigned">若为 <see langword="true"/>，则将输出偏移至非负范围（即加上 A/2）。默认为 <see langword="false"/>。</param>
        /// <returns>当前时刻的正弦波值。</returns>
        public static float GetTimeSin(float amplitude = 1f, float angularFrequency = 1f, float initialPhase = 0f, bool unsigned = false)
            => (MathF.Sin(TOSharedData.TotalSeconds * angularFrequency + initialPhase) + unsigned.ToInt()) * amplitude;

        /// <summary>
        /// 生成形如 <c>(Sin, Cos) = (A * sin(ωt + φ), A * cos(ωt + φ))</c> 的正余弦波，其中 <c>t</c> 为游戏运行总秒数。
        /// </summary>
        /// <param name="amplitude">振幅 A，默认为 1。</param>
        /// <param name="angularFrequency">角频率 ω，默认为 1（周期为 2π 秒）。</param>
        /// <param name="initialPhase">初相 φ，默认为 0。</param>
        /// <param name="unsigned">若为 <see langword="true"/>，则将输出偏移至非负范围（即加上 A/2）。默认为 <see langword="false"/>。</param>
        /// <returns>包含正弦和余弦分量的元组。</returns>
        public static (float Sin, float Cos) GetTimeSinCos(float amplitude = 1f, float angularFrequency = 1f, float initialPhase = 0f, bool unsigned = false)
        {
            (float sin, float cos) = MathF.SinCos(TOSharedData.TotalSeconds * angularFrequency + initialPhase);
            return ((sin + unsigned.ToInt()) * amplitude, (cos + unsigned.ToInt()) * amplitude);
        }
    }
}