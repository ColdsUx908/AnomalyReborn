// Developed by ColdsUx

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Regex regex)
    {
        /// <summary>
        /// 尝试使用正则表达式在输入字符串中查找匹配项。
        /// </summary>
        /// <param name="input">要搜索匹配项的字符串。</param>
        /// <param name="match">
        /// 当此方法返回时，如果匹配成功，则包含匹配结果；否则包含 <see langword="null"/>。
        /// 此参数以未初始化状态传递。
        /// </param>
        /// <returns>如果找到匹配项，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input"/> 为 <see langword="null"/>。</exception>
        public bool TryMatch(string input, [NotNullWhen(true)] out Match match)
        {
            ArgumentNullException.ThrowIfNull(input);
            return (match = regex.Match(input)).Success;
        }
    }

    extension(Regex)
    {
        /// <summary>
        /// 使用指定的正则表达式模式在输入字符串中查找匹配项。
        /// </summary>
        /// <param name="input">要搜索匹配项的字符串。</param>
        /// <param name="pattern">要匹配的正则表达式模式。</param>
        /// <param name="match">
        /// 当此方法返回时，如果匹配成功，则包含匹配结果；否则包含 <see langword="null"/>。
        /// 此参数以未初始化状态传递。
        /// </param>
        /// <returns>如果找到匹配项，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input"/> 或 <paramref name="pattern"/> 为 <see langword="null"/>。</exception>
        public static bool TryMatch(string input, string pattern, [NotNullWhen(true)] out Match match)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(pattern);
            return (match = Regex.Match(input, pattern)).Success;
        }
    }
}