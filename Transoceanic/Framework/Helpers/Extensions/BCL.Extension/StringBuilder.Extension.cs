namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(StringBuilder builder)
    {
        public void AppendLocalizedLine(string key) => builder.AppendLine(Language.GetTextValue(key));

        public void AppendLocalizedLineFormat(string key, params object[] args) => builder.AppendLine(Language.GetTextValue(key, args));

        public void AppendLocalizedLine(ILocalizationPrefix localizationPrefixProvider, string suffix) => builder.AppendLocalizedLine(localizationPrefixProvider.GetKey(suffix));

        public void AppendLocalizedLineFormat(ILocalizationPrefix localizationPrefixProvider, string suffix, params object[] args) => builder.AppendLocalizedLineFormat(localizationPrefixProvider.GetKey(suffix), args);

        public void AppendTODebugErrorMessage()
        {
            builder.Append(Environment.NewLine);
            builder.AppendLocalizedLine(TOSharedData.DebugErrorMessageKey);
        }
    }
}