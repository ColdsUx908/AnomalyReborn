// Developed by ColdsUx

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Enum)
    {
        /// <summary>
        /// 检查枚举值是否已定义。
        /// <br/>此方法通过判断枚举值字符串表示的首字符是否包含符号或数字来实现快速判定，十分高效。
        /// </summary>
        /// <typeparam name="TEnum">枚举类型。</typeparam>
        /// <param name="value">要检查的枚举值。</param>
        /// <returns>
        /// 如果 <paramref name="value"/> 是 <typeparamref name="TEnum"/> 中定义的有效枚举值，则为 <see langword="true"/>；
        /// 否则为 <see langword="false"/>。
        /// </returns>
        public static bool IsDefinedBetter<TEnum>(TEnum value) where TEnum : Enum =>
            value.ToString()[0] is '+' or '-' or '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9';
    }
}