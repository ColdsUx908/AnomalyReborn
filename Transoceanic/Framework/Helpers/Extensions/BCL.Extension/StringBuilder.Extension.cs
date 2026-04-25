// Designed by ColdsUx

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(StringBuilder builder)
    {
        /// <summary>
        /// 将本地化文本追加到 <see cref="StringBuilder"/> 末尾，并自动添加换行符。
        /// </summary>
        /// <param name="key">本地化文本的键。</param>
        public void AppendLocalizedLine(string key) => builder.AppendLine(Language.GetTextValue(key));

        /// <summary>
        /// 将格式化后的本地化文本追加到 <see cref="StringBuilder"/> 末尾，并自动添加换行符。
        /// </summary>
        /// <param name="key">本地化文本的键。</param>
        /// <param name="args">用于格式化字符串的参数数组。</param>
        public void AppendLocalizedLineFormat(string key, params object[] args) => builder.AppendLine(Language.GetTextValue(key, args));

        /// <summary>
        /// 将带前缀的本地化文本追加到 <see cref="StringBuilder"/> 末尾，并自动添加换行符。
        /// </summary>
        /// <param name="localizationPrefixProvider">提供本地化前缀的实例。</param>
        /// <param name="suffix">本地化键的后缀部分。</param>
        public void AppendLocalizedLine(ILocalizationPrefix localizationPrefixProvider, string suffix) => builder.AppendLocalizedLine(localizationPrefixProvider.GetKey(suffix));

        /// <summary>
        /// 将带前缀且格式化后的本地化文本追加到 <see cref="StringBuilder"/> 末尾，并自动添加换行符。
        /// </summary>
        /// <param name="localizationPrefixProvider">提供本地化前缀的实例。</param>
        /// <param name="suffix">本地化键的后缀部分。</param>
        /// <param name="args">用于格式化字符串的参数数组。</param>
        public void AppendLocalizedLineFormat(ILocalizationPrefix localizationPrefixProvider, string suffix, params object[] args) => builder.AppendLocalizedLineFormat(localizationPrefixProvider.GetKey(suffix), args);

        /// <summary>
        /// 将预定义的调试错误信息追加到 <see cref="StringBuilder"/> 末尾。
        /// <br/>该方法会先添加一个换行符，再追加错误信息。
        /// </summary>
        public void AppendTODebugErrorMessage()
        {
            builder.Append(Environment.NewLine);
            builder.AppendLocalizedLine(TOSharedData.DebugErrorMessageKey);
        }
    }
}