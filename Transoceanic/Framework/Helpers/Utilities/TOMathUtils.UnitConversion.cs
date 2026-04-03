namespace Transoceanic.Framework.Helpers;

public static partial class TOMathUtils
{
    public static class UnitConversion
    {
        /// <summary>
        /// 将像素每帧转换为英里每小时的转换因子。
        /// <br/>计算公式为：<c>C = 60f / 8f * 0.681818f</c>
        /// （一像素为 <c>1/8</c> 英尺）。
        /// </summary>
        public const float MphsPerPpt = 5.1136364f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Pixptick_To_Mph(float value) => value * MphsPerPpt;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Mph_To_Pixptick(float value) => value / MphsPerPpt;
    }
}