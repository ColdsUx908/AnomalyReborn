namespace Transoceanic.Localization;

public static class TOLocalizationKeys
{
    /// <summary>
    /// 模组本地化键的前缀，用于拼接完整的本地化键路径。
    /// </summary>
    public const string ModLocalizationPrefix = "Mods.Transoceanic.";

    /// <summary>
    /// 调试相关本地化键的前缀，以 <see cref="ModLocalizationPrefix"/> 开头。
    /// </summary>
    public const string DebugPrefix = ModLocalizationPrefix + "DEBUG.";

    /// <summary>
    /// 调试用错误信息的完整本地化键。
    /// </summary>
    public const string DebugErrorMessageKey = ModLocalizationPrefix + "DEBUG.ErrorMessage";
}
