// Designed by ColdsUx

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(CommandCaller caller)
    {
        /// <summary>
        /// 向命令调用者回复本地化文本。
        /// </summary>
        /// <param name="key">本地化文本的键名。</param>
        /// <param name="textColor">文本颜色，默认为白色。</param>
        public void ReplyLocalizedText(string key, Color? textColor = null) => caller.Reply(Language.GetTextValue(key), textColor ?? Color.White);

        /// <summary>
        /// 向命令调用者回复格式化后的本地化文本。
        /// </summary>
        /// <param name="key">本地化文本的键名。</param>
        /// <param name="textColor">文本颜色，默认为白色。</param>
        /// <param name="args">用于格式化文本的参数。</param>
        public void ReplyLocalizedTextFormat(string key, Color? textColor = null, params object[] args) => caller.Reply(Language.GetTextValue(key, args), textColor ?? Color.White);

        /// <summary>
        /// 使用本地化前缀提供者生成完整键名后，向命令调用者回复本地化文本。
        /// </summary>
        /// <param name="localizationPrefixProvider">提供本地化前缀的对象。</param>
        /// <param name="suffix">键名的后缀部分。</param>
        /// <param name="textColor">文本颜色，默认为白色。</param>
        public void ReplyLocalizedText(ILocalizationPrefix localizationPrefixProvider, string suffix, Color? textColor = null) => caller.ReplyLocalizedText(localizationPrefixProvider.GetKey(suffix), textColor);

        /// <summary>
        /// 使用本地化前缀提供者生成完整键名后，向命令调用者回复格式化后的本地化文本。
        /// </summary>
        /// <param name="localizationPrefixProvider">提供本地化前缀的对象。</param>
        /// <param name="suffix">键名的后缀部分。</param>
        /// <param name="textColor">文本颜色，默认为白色。</param>
        /// <param name="args">用于格式化文本的参数。</param>
        public void ReplyLocalizedTextFormat(ILocalizationPrefix localizationPrefixProvider, string suffix, Color? textColor = null, params object[] args) => caller.ReplyLocalizedTextFormat(localizationPrefixProvider.GetKey(suffix), textColor, args);

        /// <summary>
        /// 向命令调用者回复 <see cref="StringBuilder"/> 中构建的文本。
        /// </summary>
        /// <param name="builder">包含回复内容的字符串构建器。</param>
        /// <param name="textColor">文本颜色，默认为白色。</param>
        public void Reply(StringBuilder builder, Color? textColor = null) => caller.Reply(builder.ToString(), textColor ?? Color.White);

        /// <summary>
        /// 向命令调用者回复调试错误信息。
        /// </summary>
        /// <param name="key">本地化错误信息的键名。</param>
        /// <param name="args">用于格式化错误信息的参数。</param>
        /// <remarks>该信息会附加调试错误颜色和固定的错误提示后缀。</remarks>
        public void ReplyDebugErrorMessage(string key, params object[] args)
        {
            caller.Reply("[Transoceanic ERROR", TOSharedData.TODebugErrorColor);
            caller.ReplyLocalizedTextFormat(key, TOSharedData.TODebugErrorColor, args);
            caller.ReplyLocalizedText(TOSharedData.DebugErrorMessageKey, TOSharedData.TODebugErrorColor);
        }
    }
}