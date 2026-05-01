// Developed by ColdsUx

namespace Transoceanic.Framework.Helpers.AbstractionHandlers;

/// <summary>
/// 处理聊天命令 "/chat"（需要输入 "//chat"）的入口命令类。
/// 若无参数则显示帮助信息；根据第一个参数决定是显示帮助、重做上次命令还是委派给子命令。
/// </summary>
/// <remarks>
/// 本地化键使用 <c>TOSharedData.ModLocalizationPrefix + "Commands.GeneralCommand"</c> 作为前缀。
/// </remarks>
public sealed class TOGeneralChatCommand : ModCommand, ILocalizationPrefix
{
    /// <summary>
    /// 命令类型为聊天（Chat），表示该命令仅在游戏聊天窗口中被识别。
    /// </summary>
    public override CommandType Type => CommandType.Chat;

    /// <summary>
    /// 该命令在游戏中的调用文本。客户端必须输入 "//chat"（两个斜杠）才能触发。
    /// </summary>
    public override string Command => "/chat";

    /// <summary>
    /// 本地化键前缀，用于查找该命令相关的翻译文本。
    /// </summary>
    public string LocalizationPrefix => TOSharedData.ModLocalizationPrefix + "Commands.GeneralCommand";

    /// <summary>
    /// 执行 "/chat" 命令的具体逻辑。
    /// <list type="bullet">
    /// <item>参数为空时输出帮助信息。</item>
    /// <item>第一个参数为 "help"、"h" 或 "?" 时输出帮助。</item>
    /// <item>第一个参数为 "redo" 时重做上一次聊天命令。</item>
    /// <item>其他情况则将第一个参数视为子命令名称，剩余参数作为子命令参数进行尝试执行。</item>
    /// </list>
    /// </summary>
    /// <param name="caller">调用此命令的玩家或控制台。</param>
    /// <param name="input">原始的完整输入文本。</param>
    /// <param name="args">以空格分割的参数数组。</param>
    public override void Action(CommandCaller caller, string input, string[] args)
    {
        if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
        {
            caller.ReplyLocalizedText(this, "Helper");
            return;
        }

        switch (args[0].ToLower())
        {
            case "help":
            case "h":
            case "?":
                caller.ReplyLocalizedText(this, "Helper");
                break;
            case "redo":
                CommandCallInfo commandCallInfo = caller.Player.Ocean.CommandCallInfo;
                if (commandCallInfo is not null && commandCallInfo.CommandType == CommandType.Chat)
                    TOCommandHelper.Instance.TryExecute(caller, commandCallInfo.Command, CommandType.Chat, commandCallInfo.Args);
                break;
            default:
                TOCommandHelper.Instance.TryExecute(caller, args[0], CommandType.Chat, args[1..]);
                break;
        }
    }
}

/// <summary>
/// 命令注册与调度辅助类。在模组内容加载完毕后收集所有 <see cref="TOCommand"/> 派生实例，
/// 并依据命令类型和名称将实际执行分派给对应的命令对象。
/// 实现了 <see cref="IContentLoader"/> 以便自动注册和卸载。
/// </summary>
public sealed class TOCommandHelper : IContentLoader, ILocalizationPrefix
{
    /// <summary>
    /// <see cref="TOCommandHelper"/> 的单例实例。。
    /// </summary>
    internal static TOCommandHelper Instance { get; private set; }

    /// <summary>
    /// 已注册的命令字典。键为小写命令名称，值为对应的 <see cref="TOCommand"/> 实例。
    /// 注册时会忽略关键字为 "help" 的命令。
    /// </summary>
    internal static readonly Dictionary<string, TOCommand> CommandSet = [];

    /// <summary>
    /// 用于本地化文本的键前缀。
    /// </summary>
    public string LocalizationPrefix => TOSharedData.ModLocalizationPrefix + "Commands.GeneralCommand";

    /// <summary>
    /// 在模组所有内容加载完毕后调用。初始化单例并扫描所有 <see cref="TOCommand"/> 派生类，
    /// 将非 "help" 命令注册到 <see cref="CommandSet"/> 中。
    /// </summary>
    void IContentLoader.PostSetupContent()
    {
        Instance = this;

        foreach (TOCommand commandContainer in TOReflectionUtils.GetTypeInstancesDerivedFrom<TOCommand>())
        {
            if (!commandContainer.Command.Equals("help", StringComparison.CurrentCultureIgnoreCase))
                CommandSet[commandContainer.Command.ToLower()] = commandContainer;
        }
    }

    /// <summary>
    /// 在模组卸载时清空命令字典，避免残留引用。
    /// </summary>
    void IContentLoader.OnModUnload() => CommandSet.Clear();

    /// <summary>
    /// 尝试查找并执行指定名称的命令。
    /// <para>流程：</para>
    /// <list type="number">
    /// <item>在 <see cref="CommandSet"/> 中查找命令名（忽略大小写）。</item>
    /// <item>检查命令实例的 <see cref="TOCommand.Type"/> 是否与 <paramref name="commandType"/> 匹配。</item>
    /// <item>设置玩家的最后命令调用记录（<c>caller.Player.Ocean.CommandCallInfo</c>）。</item>
    /// <item>执行 <see cref="TOCommand.Action"/>。若抛出 <see cref="CommandArgumentException"/>，
    /// 则回复本地化错误消息并显示该命令的帮助信息。</item>
    /// <item>若未找到命令或类型不匹配，回复通用帮助文本。</item>
    /// </list>
    /// </summary>
    /// <param name="caller">命令调用者。</param>
    /// <param name="command">要查找的命令名（不区分大小写）。</param>
    /// <param name="commandType">触发该命令的通道类型，必须与目标命令的 <see cref="TOCommand.Type"/> 标志位匹配。</param>
    /// <param name="args">传递给命令的参数数组。</param>
    public void TryExecute(CommandCaller caller, string command, CommandType commandType, string[] args)
    {
        if (CommandSet.TryGetValue(command, out TOCommand value) && value.Type.HasFlag(commandType))
        {
            try
            {
                caller.Player.Ocean.CommandCallInfo = new(commandType, command, caller, args);
                value.Action(caller, args);
            }
            catch (CommandArgumentException e)
            {
                caller.ReplyLocalizedTextFormat(this, "InvalidArguments", Color.Red, e);
                value.Help(caller, args);
            }
        }
        else
            caller.ReplyLocalizedText(this, "Helper2");
    }
}

/// <summary>
/// 开启或关闭调试模式的命令。
/// </summary>
public sealed class DebugModeCommand : TOCommand, ILocalizationPrefix
{
    public override CommandType Type => CommandType.Chat;

    public override string Command => "debug";

    public string LocalizationPrefix => TOSharedData.ModLocalizationPrefix + "Commands.DebugMode";

    public override void Action(CommandCaller caller, string[] args)
    {
        if (args.Length == 0)
        {
            caller.ReplyLocalizedTextFormat(this, "Status", Color.White, TOSharedData.DEBUG);
            return;
        }

        switch (args[0])
        {
            case "on":
            case "true":
            case "1":
                TOSharedData.DEBUG = true;
                caller.ReplyLocalizedText(this, "On");
                break;
            case "off":
            case "false":
            case "0":
                TOSharedData.DEBUG = false;
                caller.ReplyLocalizedText(this, "Off");
                break;
            default:
                throw new CommandArgumentException(this, caller, args);
        }
    }
}