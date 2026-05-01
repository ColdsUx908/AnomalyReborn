// Developed by ColdsUx

namespace Transoceanic.Framework.Abstractions;

/// <summary>
/// 指定加载方法的优先级。优先级数值越大，执行顺序越靠前。
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class LoadPriorityAttribute : Attribute
{
    /// <summary>
    /// 获取加载优先级值。
    /// </summary>
    public readonly double Priority;

    /// <summary>
    /// 初始化 <see cref="LoadPriorityAttribute"/> 的新实例。
    /// </summary>
    /// <param name="priority">优先级数值。</param>
    public LoadPriorityAttribute(double priority) => Priority = priority;
}

/// <summary>
/// 定义内容加载生命周期的接口，用于在 Mod 加载流程中的特定节点执行自定义操作。
/// </summary>
public interface IContentLoader
{
    /// <summary>
    /// 在所有 Mod 内容加载完成后调用。
    /// </summary>
    public virtual void PostSetupContent() { }

    /// <summary>
    /// 在所有配方添加完成后调用。
    /// </summary>
    public virtual void PostAddRecipes() { }

    /// <summary>
    /// 在 Mod 卸载前调用，用于清理资源。
    /// </summary>
    public virtual void OnModUnload() { }

    /// <summary>
    /// 在世界加载完成时调用。
    /// </summary>
    public virtual void OnWorldLoad() { }

    /// <summary>
    /// 在世界卸载前调用。
    /// </summary>
    public virtual void OnWorldUnload() { }
}

/// <summary>
/// 内部 Mod 加载器接口，提供比 <see cref="IContentLoader"/> 更早或更晚的加载时机。
/// </summary>
internal interface ITOLoader
{
    /// <summary>
    /// 在 Mod 加载时调用。
    /// </summary>
    internal virtual void Load() { }

    /// <summary>
    /// 在 Mod 卸载时调用。
    /// </summary>
    internal virtual void Unload() { }
}