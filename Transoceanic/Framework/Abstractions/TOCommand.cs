namespace Transoceanic.Framework.Abstractions;

public abstract class TOCommand
{
    public abstract CommandType Type { get; }

    public abstract string Command { get; }

    /// <summary>
    /// 执行命令的函数。
    /// </summary>
    /// <param name="caller"></param>
    /// <param name="args">参数数组。</param>
    public abstract void Action(CommandCaller caller, string[] args);

    public virtual void Help(CommandCaller caller, string[] args) { }
}
