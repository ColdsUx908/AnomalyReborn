namespace Transoceanic.Commands;

public sealed record CommandCallInfo(CommandType CommandType, string Command, CommandCaller Caller, string[] Args)
{
    public CommandCallInfo(TOCommand commandInstance, CommandCaller caller, string[] args) : this(commandInstance.Type, commandInstance.Command, caller, args) { }
}

public sealed class CommandArgumentException : Exception
{
    public readonly CommandCallInfo CallInfo;

    public CommandArgumentException(CommandCallInfo callInfo) : base() => CallInfo = callInfo;

    public CommandArgumentException(CommandCallInfo callInfo, string message = "") : base(message) => CallInfo = callInfo;

    public CommandArgumentException(CommandCallInfo callInfo, string message, Exception innerException) : base(message, innerException) => CallInfo = callInfo;

    public CommandArgumentException(TOCommand commandInstance, CommandCaller caller, string[] args) : this(new CommandCallInfo(commandInstance, caller, args)) { }

    public CommandArgumentException(TOCommand commandInstance, CommandCaller caller, string[] args, string message) : this(new CommandCallInfo(commandInstance, caller, args), message) { }

    public CommandArgumentException(TOCommand commandInstance, CommandCaller caller, string[] args, string message, Exception innerException) : this(new CommandCallInfo(commandInstance, caller, args), message, innerException) { }

    public override string ToString()
    {
        StringBuilder builder = new();
        builder.AppendLine($"Command: {CallInfo.Command}");
        builder.AppendLine($"Command Type: {CallInfo.CommandType}");
        builder.AppendLine($"Calling Player: {CallInfo.Caller.Player.name} ({CallInfo.Caller.Player.whoAmI})");
        builder.AppendLine($"Arguments: {string.Join(", ", CallInfo.Args)}");
        builder.AppendLine($"Message: {Message}");
        return builder.ToString();
    }
}

public sealed class TOGeneralChatCommand : ModCommand, ILocalizationPrefix
{
    public override CommandType Type => CommandType.Chat;

    public override string Command => "/chat"; //需要使用的指令是"//chat"（两个斜杠）

    public string LocalizationPrefix => TOSharedData.ModLocalizationPrefix + "Commands.GeneralCommand";

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

public sealed class TOCommandHelper : IContentLoader, ILocalizationPrefix
{
    internal static TOCommandHelper Instance { get; private set; }

    internal static readonly Dictionary<string, TOCommand> CommandSet = [];

    public string LocalizationPrefix => TOSharedData.ModLocalizationPrefix + "Commands.GeneralCommand";

    void IContentLoader.PostSetupContent()
    {
        Instance = this;

        foreach (TOCommand commandContainer in TOReflectionUtils.GetTypeInstancesDerivedFrom<TOCommand>())
        {
            if (!commandContainer.Command.Equals("help", StringComparison.CurrentCultureIgnoreCase)) //不加载关键字为"help"的命令
                CommandSet[commandContainer.Command.ToLower()] = commandContainer;
        }
    }

    void IContentLoader.OnModUnload() => CommandSet.Clear();

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
