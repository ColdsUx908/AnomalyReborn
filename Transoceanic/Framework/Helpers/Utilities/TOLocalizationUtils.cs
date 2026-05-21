// Developed by ColdsUx

using Terraria.Chat;

namespace Transoceanic.Framework.Helpers;

/// <summary>
/// 提供本地化文本获取、聊天消息发送等辅助功能。
/// 所有聊天方法均会根据当前网络模式（单人/多人）自动选择合适的发送方式，
/// 并支持指定接收者、格式化文本以及基于 <see cref="ILocalizationPrefix"/> 的键名构造。
/// </summary>
public static class TOLocalizationUtils
{
    /// <summary>
    /// 获取指定 <typeparamref name="T"/> 类型 Mod 物品的显示名称本地化文本。
    /// </summary>
    /// <typeparam name="T">继承自 <see cref="ModItem"/> 的物品类型。</typeparam>
    /// <returns>表示该物品 "DisplayName" 键的 <see cref="LocalizedText"/> 实例。</returns>
    public static LocalizedText GetItemName<T>() where T : ModItem => ItemLoader.GetItem(ModContent.ItemType<T>()).GetLocalization("DisplayName");

    /// <summary>
    /// 向所有玩家发送一段纯文本聊天消息（不经过本地化）。
    /// 单人模式下直接显示在聊天栏；多人模式下广播给所有客户端。
    /// </summary>
    /// <param name="text">要发送的文本内容。</param>
    /// <param name="textColor">文本颜色，默认为白色。</param>
    public static void ChatLiteralText(string text, Color? textColor = null)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(text, textColor ?? Color.White);
        else
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(text), textColor ?? Color.White);
    }

    /// <summary>
    /// 向指定的玩家集合发送纯文本聊天消息。
    /// 单人模式下直接显示，忽略接收者参数；多人模式下逐个发送给 <paramref name="receivers"/> 中的玩家。
    /// </summary>
    /// <param name="text">要发送的文本内容。</param>
    /// <param name="textColor">文本颜色，默认为白色。</param>
    /// <param name="receivers">接收该消息的玩家列表。在单人模式下无效。</param>
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

    /// <summary>
    /// 根据本地化键名发送一条聊天消息，允许客户端使用自己的语言显示对应文本。
    /// 单人模式下直接显示本地化后的文本；多人模式下广播键名，由各客户端自行本地化。
    /// </summary>
    /// <param name="key">本地化文本的键名。</param>
    /// <param name="textColor">文本颜色，默认为白色。</param>
    public static void ChatLocalizedText(string key, Color? textColor = null)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(Language.GetTextValue(key), textColor ?? Color.White);
        else
            ChatHelper.BroadcastChatMessage(NetworkText.FromKey(key), textColor ?? Color.White);
    }

    /// <summary>
    /// 根据本地化键名向指定玩家发送聊天消息，客户端各自本地化。
    /// 单人模式下直接显示，忽略接收者参数。
    /// </summary>
    /// <param name="key">本地化文本的键名。</param>
    /// <param name="textColor">文本颜色，默认为白色。</param>
    /// <param name="receivers">接收该消息的玩家列表。</param>
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

    /// <summary>
    /// 发送一条带有格式化参数的本地化消息。
    /// <br/><b>注意：</b>此方法会先在服务器端将本地化文本与参数结合，生成最终字符串后以<b>字面文本</b>形式广播，
    /// 因此所有客户端将看到相同的格式化结果，而无法根据各自语言再次本地化。
    /// </summary>
    /// <param name="key">本地化文本的键名。</param>
    /// <param name="textColor">文本颜色，默认为白色。</param>
    /// <param name="args">用于格式化本地化文本的参数。</param>
    public static void ChatLocalizedTextFormat(string key, Color? textColor = null, params object[] args)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(Language.GetTextValue(key, args), textColor ?? Color.White);
        else
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(Language.GetTextValue(key, args)), textColor ?? Color.White);
    }

    /// <summary>
    /// 向指定单个玩家发送带有格式化参数的本地化消息。
    /// <br/><b>注意：</b>此方法会先在服务器端将本地化文本与参数结合，生成最终字符串后以<b>字面文本</b>形式广播，
    /// 因此所有客户端将看到相同的格式化结果，而无法根据各自语言再次本地化。
    /// </summary>
    /// <param name="key">本地化文本的键名。</param>
    /// <param name="receiver">接收消息的玩家。</param>
    /// <param name="textColor">文本颜色，默认为白色。</param>
    /// <param name="args">格式化参数。</param>
    public static void ChatLocalizedTextFormat(string key, Player receiver, Color? textColor = null, params object[] args)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(Language.GetTextValue(key, args), textColor ?? Color.White);
        else
            ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(Language.GetTextValue(key, args)), textColor ?? Color.White, receiver.whoAmI);
    }

    /// <summary>
    /// 使用 <see cref="ILocalizationPrefix"/> 和指定后缀构造本地化键名，并发送一条普通本地化消息。
    /// </summary>
    /// <param name="localizationPrefixProvider">提供本地化键前缀的对象。</param>
    /// <param name="suffix">本地化键的后缀部分。</param>
    /// <param name="textColor">文本颜色，默认为白色。</param>
    public static void ChatLocalizedText(ILocalizationPrefix localizationPrefixProvider, string suffix, Color? textColor = null) => ChatLocalizedText(localizationPrefixProvider.GetKey(suffix), textColor);

    /// <summary>
    /// 使用 <see cref="ILocalizationPrefix"/> 和指定后缀构造键名，并向指定玩家发送普通本地化消息。
    /// </summary>
    /// <param name="localizationPrefixProvider">提供本地化键前缀的对象。</param>
    /// <param name="suffix">本地化键的后缀部分。</param>
    /// <param name="textColor">文本颜色，默认为白色。</param>
    /// <param name="receivers">接收消息的玩家列表。</param>
    public static void ChatLocalizedText(ILocalizationPrefix localizationPrefixProvider, string suffix, Color? textColor = null, params ReadOnlySpan<Player> receivers) => ChatLocalizedText(localizationPrefixProvider.GetKey(suffix), textColor, receivers);

    /// <summary>
    /// 使用 <see cref="ILocalizationPrefix"/> 和指定后缀构造键名，并发送带格式化参数的本地化消息（服务器端格式化后以字面文本广播）。
    /// </summary>
    /// <param name="localizationPrefixProvider">提供本地化键前缀的对象。</param>
    /// <param name="suffix">本地化键的后缀部分。</param>
    /// <param name="textColor">文本颜色，默认为白色。</param>
    /// <param name="args">格式化参数。</param>
    public static void ChatLocalizedText(ILocalizationPrefix localizationPrefixProvider, string suffix, Color? textColor = null, params object[] args) => ChatLocalizedTextFormat(localizationPrefixProvider.GetKey(suffix), textColor, args);

    /// <summary>
    /// 使用 <see cref="ILocalizationPrefix"/> 和指定后缀构造键名，并向指定玩家发送带格式化参数的本地化消息。
    /// </summary>
    /// <param name="localizationPrefixProvider">提供本地化键前缀的对象。</param>
    /// <param name="suffix">本地化键的后缀部分。</param>
    /// <param name="receiver">接收消息的玩家。</param>
    /// <param name="textColor">文本颜色，默认为白色。</param>
    /// <param name="args">格式化参数。</param>
    public static void ChatLocalizedText(ILocalizationPrefix localizationPrefixProvider, string suffix, Player receiver, Color? textColor = null, params object[] args) => ChatLocalizedTextFormat(localizationPrefixProvider.GetKey(suffix), receiver, textColor, args);

    /// <summary>
    /// 向玩家数组发送带有格式化参数的本地化消息。
    /// <br/><b>注意：</b>此方法会先在服务器端将本地化文本与参数结合，生成最终字符串后以<b>字面文本</b>形式广播，
    /// 因此所有客户端将看到相同的格式化结果，而无法根据各自语言再次本地化。
    /// </summary>
    /// <param name="key">本地化文本的键名。</param>
    /// <param name="receivers">接收消息的玩家数组。</param>
    /// <param name="textColor">文本颜色，默认为白色。</param>
    /// <param name="args">格式化参数。</param>
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

    /*
    /// <summary>
    /// 创建一个带有 "[Transoceanic ERROR]" 标题头部的 <see cref="StringBuilder"/> 实例。
    /// 通常用于构建调试错误信息。
    /// </summary>
    /// <returns>已追加错误头的新 <see cref="StringBuilder"/>。</returns>
    public static StringBuilder CreateStringBuilderWithErrorHeader()
    {
        StringBuilder builder = new();
        builder.AppendLine("[Transoceanic ERROR]");
        return builder;
    }

    /// <summary>
    /// 向指定玩家发送一条调试错误消息，包含错误头、具体错误信息以及通用错误提示。
    /// </summary>
    /// <param name="key">错误信息的本地化键（不含调试前缀，方法内部会拼接 <see cref="TOSharedData.DebugPrefix"/>）。</param>
    /// <param name="receiver">接收消息的玩家。</param>
    /// <param name="args">格式化参数。</param>
    public static void ChatDebugErrorMessage(string key, Player receiver, params object[] args)
    {
        ChatLiteralText("[Transoceanic ERROR]", TOSharedData.TODebugErrorColor, receiver);
        ChatLocalizedTextFormat(TOSharedData.DebugPrefix + key, receiver, TOSharedData.TODebugErrorColor, args);
        ChatLocalizedText(TOSharedData.DebugErrorMessageKey, TOSharedData.TODebugErrorColor, receiver);
    }
    */

    /// <summary>
    /// 向玩家数组发送调试错误消息，依次发送错误头、具体错误以及通用错误提示。
    /// </summary>
    /// <param name="key">错误信息的本地化键（不含前缀）。</param>
    /// <param name="receivers">接收消息的玩家数组。</param>
    /// <param name="args">格式化参数。</param>
    public static void ChatDebugErrorMessage(string key, Player[] receivers, params object[] args)
    {
        ChatLiteralText("[Transoceanic ERROR]", TOSharedData.TODebugErrorColor, receivers);
        ChatLocalizedText(TOSharedData.DebugPrefix + key, receivers, TOSharedData.TODebugErrorColor, args);
        ChatLocalizedText(TOSharedData.DebugErrorMessageKey, TOSharedData.TODebugErrorColor, receivers);
    }
}