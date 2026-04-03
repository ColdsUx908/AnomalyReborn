using Terraria.Chat;

namespace Transoceanic.Framework.Helpers;

public static class TOLocalizationUtils
{
    public static void ChatLiteralText(string text, Color? textColor = null)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(text, textColor ?? Color.White);
        else
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(text), textColor ?? Color.White);
    }

    public static void ChatLiteralText(string text, Color? textColor = null, params ReadOnlySpan<Player> receivers)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(text, textColor ?? Color.White);
        else
        {
            foreach (Player player in receivers)
                ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(text), textColor ?? Color.White, player.whoAmI);
        }
    }

    public static void ChatLocalizedText(string key, Color? textColor = null)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(Language.GetTextValue(key), textColor ?? Color.White);
        else
            ChatHelper.BroadcastChatMessage(NetworkText.FromKey(key), textColor ?? Color.White);
    }

    public static void ChatLocalizedText(string key, Color? textColor = null, params ReadOnlySpan<Player> receivers)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(Language.GetTextValue(key), textColor ?? Color.White);
        else
        {
            foreach (Player player in receivers)
                ChatHelper.SendChatMessageToClient(NetworkText.FromKey(key), textColor ?? Color.White, player.whoAmI);
        }
    }

    public static void ChatLocalizedTextFormat(string key, Color? textColor = null, params object[] args)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(Language.GetTextValue(key, args), textColor ?? Color.White);
        else
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(Language.GetTextValue(key, args)), textColor ?? Color.White);
    }

    public static void ChatLocalizedTextFormat(string key, Player receiver, Color? textColor = null, params object[] args)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(Language.GetTextValue(key, args), textColor ?? Color.White);
        else
            ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(Language.GetTextValue(key, args)), textColor ?? Color.White, receiver.whoAmI);
    }

    public static void ChatLocalizedText(ILocalizationPrefix localizationPrefixProvider, string suffix, Color? textColor = null) => ChatLocalizedText(localizationPrefixProvider.GetKey(suffix), textColor);

    public static void ChatLocalizedText(ILocalizationPrefix localizationPrefixProvider, string suffix, Color? textColor = null, params ReadOnlySpan<Player> receivers) => ChatLocalizedText(localizationPrefixProvider.GetKey(suffix), textColor, receivers);

    public static void ChatLocalizedText(ILocalizationPrefix localizationPrefixProvider, string suffix, Color? textColor = null, params object[] args) => ChatLocalizedTextFormat(localizationPrefixProvider.GetKey(suffix), textColor, args);

    public static void ChatLocalizedText(ILocalizationPrefix localizationPrefixProvider, string suffix, Player receiver, Color? textColor = null, params object[] args) => ChatLocalizedTextFormat(localizationPrefixProvider.GetKey(suffix), receiver, textColor, args);

    public static void ChatLocalizedText(string key, Player[] receivers, Color? textColor = null, params object[] args)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(Language.GetTextValue(key, args), textColor ?? Color.White);
        else
        {
            foreach (Player player in receivers)
                ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(Language.GetTextValue(key, args)), textColor ?? Color.White, player.whoAmI);
        }
    }

    public static StringBuilder CreateStringBuilderWithErrorHeader()
    {
        StringBuilder builder = new();
        builder.AppendLine("[Transoceanic ERROR]");
        return builder;
    }

    public static void ChatDebugErrorMessage(string key, Player receiver, params object[] args)
    {
        ChatLiteralText("[Transoceanic ERROR]", TOSharedData.TODebugErrorColor, receiver);
        ChatLocalizedTextFormat(TOSharedData.DebugPrefix + key, receiver, TOSharedData.TODebugErrorColor, args);
        ChatLocalizedText(TOSharedData.DebugErrorMessageKey, TOSharedData.TODebugErrorColor, receiver);
    }

    public static void ChatDebugErrorMessage(string key, Player[] receivers, params object[] args)
    {
        ChatLiteralText("[Transoceanic ERROR]", TOSharedData.TODebugErrorColor, receivers);
        ChatLocalizedText(TOSharedData.DebugPrefix + key, receivers, TOSharedData.TODebugErrorColor, args);
        ChatLocalizedText(TOSharedData.DebugErrorMessageKey, TOSharedData.TODebugErrorColor, receivers);
    }

    /*
    public static string Translation(string Chinese = null, string English = null, string Spanish = null, string Russian = null) => Language.ActiveCulture.LegacyId switch
    {
        7 => Chinese,
        6 => Russian,
        5 => Spanish,
        1 => English ?? "Invalid String.",
        _ => English ?? "Invalid String."
    };
    */
}