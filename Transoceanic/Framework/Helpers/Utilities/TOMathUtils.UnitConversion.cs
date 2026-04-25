// Designed by ColdsUx

namespace Transoceanic.Framework.Helpers;

public static partial class TOMathUtils
{
    /// <summary>
    /// 提供物理单位之间的转换方法（像素/帧 ↔ 英里/小时）。
    /// </summary>
    public static class UnitConversion
    {
        /// <summary>
        /// 像素每帧转换为英里每小时的转换因子。
        /// </summary>
        /// <remarks>
        /// 计算公式为：<c>C = 60f / 8f * 0.681818f</c>（假设一像素为 1/8 英尺，60 帧/秒）。
        /// </remarks>
        public const float MphsPerPpt = 5.1136364f;

        /// <summary>
        /// 将速度从像素/帧转换为英里/小时。
        /// </summary>
        /// <param name="value">像素/帧为单位的速度值。</param>
        /// <returns>英里/小时为单位的速度值。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Pixptick_To_Mph(float value) => value * MphsPerPpt;

        /// <summary>
        /// 将速度从英里/小时转换为像素/帧。
        /// </summary>
        /// <param name="value">英里/小时为单位的速度值。</param>
        /// <returns>像素/帧为单位的速度值。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Mph_To_Pixptick(float value) => value / MphsPerPpt;
    }
}