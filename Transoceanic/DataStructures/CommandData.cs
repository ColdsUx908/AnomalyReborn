// Developed by ColdsUx

namespace Transoceanic.DataStructures;

/// <summary>
/// 记录一次命令调用的完整信息，包括命令类型、命令文本、调用者和参数列表。
/// 该类型为不可变记录（record），通常用于异常报告或上下文传递。
/// </summary>
/// <param name="CommandType">触发命令的通道类型（如聊天、控制台等）。</param>
/// <param name="Command">实际执行或尝试执行的命令字符串（如 "/chat" 或子命令名）。</param>
/// <param name="Caller">调用该命令的 <see cref="CommandCaller"/> 实例。</param>
/// <param name="Args">传递给命令的参数数组。</param>
public sealed record CommandCallInfo(CommandType CommandType, string Command, CommandCaller Caller, string[] Args)
{
    /// <summary>
    /// 通过 <see cref="TOCommand"/> 实例构造 <see cref="CommandCallInfo"/>，
    /// 自动提取其 <see cref="TOCommand.Type"/> 和 <see cref="TOCommand.Command"/>。
    /// </summary>
    /// <param name="commandInstance">包含命令元数据的 <see cref="TOCommand"/> 实例。</param>
    /// <param name="caller">命令调用者。</param>
    /// <param name="args">命令参数数组。</param>
    public CommandCallInfo(TOCommand commandInstance, CommandCaller caller, string[] args)
        : this(commandInstance.Type, commandInstance.Command, caller, args) { }
}

/// <summary>
/// 表示在命令参数解析或验证过程中发生的异常。
/// 包含触发异常的完整命令调用信息，便于生成详细的错误提示。
/// </summary>
public sealed class CommandArgumentException : Exception
{
    /// <summary>
    /// 导致异常的 <see cref="CommandCallInfo"/> 只读记录。
    /// </summary>
    public readonly CommandCallInfo CallInfo;

    /// <summary>
    /// 使用指定的 <see cref="CommandCallInfo"/> 初始化 <see cref="CommandArgumentException"/>。
    /// </summary>
    /// <param name="callInfo">触发异常的命令调用信息。</param>
    public CommandArgumentException(CommandCallInfo callInfo) : base() => CallInfo = callInfo;

    /// <summary>
    /// 使用指定的 <see cref="CommandCallInfo"/> 和错误消息初始化异常。
    /// </summary>
    /// <param name="callInfo">触发异常的命令调用信息。</param>
    /// <param name="message">描述错误的消息。</param>
    public CommandArgumentException(CommandCallInfo callInfo, string message = "") : base(message) => CallInfo = callInfo;

    /// <summary>
    /// 使用指定的 <see cref="CommandCallInfo"/>、错误消息和内部异常初始化异常。
    /// </summary>
    /// <param name="callInfo">触发异常的命令调用信息。</param>
    /// <param name="message">描述错误的消息。</param>
    /// <param name="innerException">导致当前异常的原始异常。</param>
    public CommandArgumentException(CommandCallInfo callInfo, string message, Exception innerException)
        : base(message, innerException) => CallInfo = callInfo;

    /// <summary>
    /// 通过 <see cref="TOCommand"/> 实例和相关调用信息构造异常。
    /// 内部会创建新的 <see cref="CommandCallInfo"/> 记录。
    /// </summary>
    /// <param name="commandInstance">引发异常的命令实例。</param>
    /// <param name="caller">命令调用者。</param>
    /// <param name="args">命令参数数组。</param>
    public CommandArgumentException(TOCommand commandInstance, CommandCaller caller, string[] args)
        : this(new CommandCallInfo(commandInstance, caller, args)) { }

    /// <summary>
    /// 通过 <see cref="TOCommand"/> 实例、调用信息和错误消息构造异常。
    /// </summary>
    /// <param name="commandInstance">引发异常的命令实例。</param>
    /// <param name="caller">命令调用者。</param>
    /// <param name="args">命令参数数组。</param>
    /// <param name="message">描述错误的消息。</param>
    public CommandArgumentException(TOCommand commandInstance, CommandCaller caller, string[] args, string message)
        : this(new CommandCallInfo(commandInstance, caller, args), message) { }

    /// <summary>
    /// 通过 <see cref="TOCommand"/> 实例、调用信息、错误消息和内部异常构造异常。
    /// </summary>
    /// <param name="commandInstance">引发异常的命令实例。</param>
    /// <param name="caller">命令调用者。</param>
    /// <param name="args">命令参数数组。</param>
    /// <param name="message">描述错误的消息。</param>
    /// <param name="innerException">导致当前异常的原始异常。</param>
    public CommandArgumentException(TOCommand commandInstance, CommandCaller caller, string[] args, string message, Exception innerException)
        : this(new CommandCallInfo(commandInstance, caller, args), message, innerException) { }

    /// <summary>
    /// 返回包含命令名称、命令类型、调用玩家、参数列表以及错误消息的格式化字符串，
    /// 便于调试和日志记录。
    /// </summary>
    /// <returns>异常详情格式化字符串。</returns>
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