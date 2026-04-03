namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Enum)
    {
        /// <summary>
        /// 检查枚举值是否定义。
        /// <br/>十分高效。
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsDefinedBetter<TEnum>(TEnum value) where TEnum : Enum =>
            value.ToString()[0] is '+' or '-' or '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9';
    }
}