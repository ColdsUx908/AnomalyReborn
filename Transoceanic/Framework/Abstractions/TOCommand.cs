namespace Transoceanic.Framework.Abstractions;

/// <summary>
/// 表示一个自定义游戏命令的抽象基类。
/// </summary>
public abstract class TOCommand
{
    /// <summary>
    /// 获取命令的执行环境类型（聊天命令或控制台命令）。
    /// </summary>
    public abstract CommandType Type { get; }

    /// <summary>
    /// 获取命令的字符串标识符，例如 "mycommand"。
    /// </summary>
    public abstract string Command { get; }

    /// <summary>
    /// 执行命令时调用的核心逻辑。
    /// </summary>
    /// <param name="caller">命令调用者信息，包含玩家、命令来源等。</param>
    /// <param name="args">命令后跟随的参数数组。</param>
    public abstract void Action(CommandCaller caller, string[] args);

    /// <summary>
    /// 显示命令的帮助信息（可选重写）。
    /// </summary>
    /// <param name="caller">命令调用者信息。</param>
    /// <param name="args">命令后跟随的参数数组。</param>
    public virtual void Help(CommandCaller caller, string[] args) { }
}