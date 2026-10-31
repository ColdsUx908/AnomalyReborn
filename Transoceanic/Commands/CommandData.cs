namespace Transoceanic.Commands;

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
