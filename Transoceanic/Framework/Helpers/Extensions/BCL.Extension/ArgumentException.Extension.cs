namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(ArgumentException)
    {
        /// <summary>
        /// 当列表为 <see langword="null"/> 时抛出 <see cref="ArgumentNullException"/> 异常，不含任何元素时抛出 <see cref="ArgumentException"/> 异常。
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static void ThrowIfNullOrEmpty<T>([NotNull] IList<T> argument, [CallerArgumentExpression(nameof(argument))] string paramName = null!)
        {
            ArgumentNullException.ThrowIfNull(argument, paramName);
            if (argument.Count == 0)
                throw new ArgumentException($"Argument {paramName} cannot be empty.", paramName);
        }

        /// <summary>
        /// 当列表为 <see langword="null"/> 时抛出 <see cref="ArgumentNullException"/> 异常，不含任何元素或包含值为 <see langword="null"/> 的元素时抛出 <see cref="ArgumentException"/> 异常。
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static void ThrowIfNullOrEmptyOrAnyNull<T>([NotNull] IList<T> argument, [CallerArgumentExpression(nameof(argument))] string paramName = null!) where T : class
        {
            ArgumentException.ThrowIfNullOrEmpty(argument, paramName);
            for (int i = 0; i < argument.Count; i++)
                _ = argument[i] ?? throw new ArgumentException($"Argument {paramName} has a null element at [{i}].", paramName);
        }
    }
}