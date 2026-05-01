// Developed by ColdsUx

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(ArgumentException)
    {
        /// <summary>
        /// 当列表为 <see langword="null"/> 时抛出 <see cref="ArgumentNullException"/> 异常，不含任何元素时抛出 <see cref="ArgumentException"/> 异常。
        /// </summary>
        /// <typeparam name="T">列表中元素的类型。</typeparam>
        /// <param name="argument">要检查的列表。</param>
        /// <param name="paramName">
        /// 引发异常时使用的参数名称。
        /// 此参数由调用方表达式自动填充，通常无需手动提供。
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="argument"/> 为空（即 <see cref="IList{T}.Count"/> 为 0）。</exception>
        public static void ThrowIfNullOrEmpty<T>([NotNull] IList<T> argument, [CallerArgumentExpression(nameof(argument))] string paramName = null)
        {
            ArgumentNullException.ThrowIfNull(argument, paramName);
            if (argument.Count == 0)
                throw new ArgumentException($"Argument {paramName} cannot be empty.", paramName);
        }

        /// <summary>
        /// 当列表为 <see langword="null"/> 时抛出 <see cref="ArgumentNullException"/> 异常，不含任何元素或包含值为 <see langword="null"/> 的元素时抛出 <see cref="ArgumentException"/> 异常。
        /// </summary>
        /// <typeparam name="T">列表中元素的类型，必须是引用类型。</typeparam>
        /// <param name="argument">要检查的列表。</param>
        /// <param name="paramName">
        /// 引发异常时使用的参数名称。
        /// 此参数由调用方表达式自动填充，通常无需手动提供。
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="argument"/> 为空（即 <see cref="IList{T}.Count"/> 为 0），
        /// 或列表中包含值为 <see langword="null"/> 的元素。
        /// </exception>
        public static void ThrowIfNullOrEmptyOrAnyNull<T>([NotNull] IList<T> argument, [CallerArgumentExpression(nameof(argument))] string paramName = null) where T : class
        {
            ArgumentException.ThrowIfNullOrEmpty(argument, paramName);
            for (int i = 0; i < argument.Count; i++)
                _ = argument[i] ?? throw new ArgumentException($"Argument {paramName} has a null element at [{i}].", paramName);
        }
    }
}