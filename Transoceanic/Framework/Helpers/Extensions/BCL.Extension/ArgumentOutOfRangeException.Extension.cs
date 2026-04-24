namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(ArgumentOutOfRangeException)
    {
        /// <summary>
        /// 当传递的枚举值在枚举类型中未定义时抛出 <see cref="ArgumentOutOfRangeException"/>。
        /// </summary>
        /// <typeparam name="TEnum">枚举类型。</typeparam>
        /// <param name="argument">要检查的枚举值。</param>
        /// <param name="paramName">
        /// 引发异常时使用的参数名称。
        /// 此参数由调用方表达式自动填充，通常无需手动提供。
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="argument"/> 不是 <typeparamref name="TEnum"/> 中定义的有效枚举值。
        /// </exception>
        public static void ThrowIfNotDefined<TEnum>(TEnum argument, [CallerArgumentExpression(nameof(argument))] string paramName = null!) where TEnum : Enum
        {
            if (!Enum.IsDefinedBetter(argument))
                throw new ArgumentOutOfRangeException(paramName, argument, $"Value {argument} is not defined in enum {typeof(TEnum).Name}.");
        }
    }
}