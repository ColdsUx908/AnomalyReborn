namespace Transoceanic.Framework.Abstractions;

/// <summary>
/// 内部使用的更新提醒接口，用于在游戏更新循环中注册一个提示动作。
/// </summary>
internal interface IUpdateReminder
{
    /// <summary>
    /// 注册一个更新提醒委托，该委托将在合适的时机被调用以提醒开发者需要更新的内容。
    /// </summary>
    /// <returns>一个 <see cref="Action"/> 委托，其中包含提醒逻辑。</returns>
    internal abstract Action RegisterUpdateReminder();
}

/// <summary>
/// 公开的更新提醒接口，允许外部程序集注册更新提醒。
/// </summary>
public interface IExternalUpdateReminder
{
    /// <summary>
    /// 注册一个更新提醒委托，该委托将在合适的时机被调用以提醒外部使用者。
    /// </summary>
    /// <returns>一个 <see cref="Action"/> 委托，其中包含提醒逻辑。</returns>
    public abstract Action RegisterUpdateReminder();
}