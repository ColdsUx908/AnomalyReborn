namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(ILocalizationPrefix localizationPrefixProvider)
    {
        /// <summary>
        /// 获取此实例提供的本地化前缀。
        /// </summary>
        public string LocalizationPrefix => localizationPrefixProvider.LocalizationPrefix;

        /// <summary>
        /// 根据给定的后缀生成完整的本地化键。
        /// <para>若前缀不以 "." 结尾，则自动补全分隔符。</para>
        /// </summary>
        /// <param name="suffix">本地化键的后缀部分。</param>
        /// <returns>组合后的完整本地化键。</returns>
        public string GetKey(string suffix)
        {
            string localizationPrefix = localizationPrefixProvider.LocalizationPrefix;
            return localizationPrefix + (localizationPrefix.EndsWith('.') ? "" : ".") + suffix;
        }

        /// <summary>
        /// 获取与指定后缀对应的本地化文本对象 <see cref="LocalizedText"/>。
        /// </summary>
        /// <param name="suffix">本地化键的后缀部分。</param>
        /// <returns>表示本地化文本的对象。</returns>
        public LocalizedText GetText(string suffix) => Language.GetText(localizationPrefixProvider.GetKey(suffix));

        /// <summary>
        /// 获取与指定后缀对应的本地化文本字符串。
        /// </summary>
        /// <param name="suffix">本地化键的后缀部分。</param>
        /// <returns>本地化后的字符串。</returns>
        public string GetTextValue(string suffix) => Language.GetTextValue(localizationPrefixProvider.GetKey(suffix));

        /// <summary>
        /// 获取格式化后的本地化文本字符串，支持一个参数。
        /// </summary>
        /// <param name="suffix">本地化键的后缀部分。</param>
        /// <param name="arg0">用于格式化的参数。</param>
        /// <returns>格式化后的本地化字符串。</returns>
        public string GetTextValue(string suffix, object arg0) => Language.GetTextValue(localizationPrefixProvider.GetKey(suffix), arg0);

        /// <summary>
        /// 获取格式化后的本地化文本字符串，支持两个参数。
        /// </summary>
        /// <param name="suffix">本地化键的后缀部分。</param>
        /// <param name="arg0">用于格式化的第一个参数。</param>
        /// <param name="arg1">用于格式化的第二个参数。</param>
        /// <returns>格式化后的本地化字符串。</returns>
        public string GetTextValue(string suffix, object arg0, object arg1) => Language.GetTextValue(localizationPrefixProvider.GetKey(suffix), arg0, arg1);

        /// <summary>
        /// 获取格式化后的本地化文本字符串，支持三个参数。
        /// </summary>
        /// <param name="suffix">本地化键的后缀部分。</param>
        /// <param name="arg0">用于格式化的第一个参数。</param>
        /// <param name="arg1">用于格式化的第二个参数。</param>
        /// <param name="arg2">用于格式化的第三个参数。</param>
        /// <returns>格式化后的本地化字符串。</returns>
        public string GetTextValue(string suffix, object arg0, object arg1, object arg2) => Language.GetTextValue(localizationPrefixProvider.GetKey(suffix), arg0, arg1, arg2);

        /// <summary>
        /// 获取格式化后的本地化文本字符串，支持多个参数。
        /// </summary>
        /// <param name="suffix">本地化键的后缀部分。</param>
        /// <param name="args">用于格式化的参数数组。</param>
        /// <returns>格式化后的本地化字符串。</returns>
        public string GetTextValue(string suffix, params object[] args) => Language.GetTextValue(localizationPrefixProvider.GetKey(suffix), args);
    }
}