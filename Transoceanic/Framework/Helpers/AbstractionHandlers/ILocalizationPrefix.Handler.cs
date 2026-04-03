namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(ILocalizationPrefix localizationPrefixProvider)
    {
        public string LocalizationPrefix => localizationPrefixProvider.LocalizationPrefix;

        public string GetKey(string suffix)
        {
            string localizationPrefix = localizationPrefixProvider.LocalizationPrefix;
            return localizationPrefix + (localizationPrefix.EndsWith('.') ? "" : ".") + suffix;
        }

        public LocalizedText GetText(string suffix) => Language.GetText(localizationPrefixProvider.GetKey(suffix));

        public string GetTextValue(string suffix) => Language.GetTextValue(localizationPrefixProvider.GetKey(suffix));

        public string GetTextValue(string suffix, object arg0) => Language.GetTextValue(localizationPrefixProvider.GetKey(suffix), arg0);

        public string GetTextValue(string suffix, object arg0, object arg1) => Language.GetTextValue(localizationPrefixProvider.GetKey(suffix), arg0, arg1);

        public string GetTextValue(string suffix, object arg0, object arg1, object arg2) => Language.GetTextValue(localizationPrefixProvider.GetKey(suffix), arg0, arg1, arg2);

        public string GetTextValue(string suffix, params object[] args) => Language.GetTextValue(localizationPrefixProvider.GetKey(suffix), args);
    }
}