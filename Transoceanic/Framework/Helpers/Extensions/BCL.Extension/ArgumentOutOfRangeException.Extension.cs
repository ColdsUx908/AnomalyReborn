namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(ArgumentOutOfRangeException)
    {
        /// <summary>
        /// 当传递的枚举值在枚举类型中未定义时抛出 <see cref="ArgumentOutOfRangeException"/>。
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void ThrowIfNotDefined<TEnum>(TEnum argument, [CallerArgumentExpression(nameof(argument))] string paramName = null!) where TEnum : Enum
        {
            if (!Enum.IsDefinedBetter(argument))
                throw new ArgumentOutOfRangeException(paramName, argument, $"Value {argument} is not defined in enum {typeof(TEnum).Name}.");
        }
    }
}