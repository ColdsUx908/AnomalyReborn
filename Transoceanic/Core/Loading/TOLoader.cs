namespace Transoceanic.Core.Loading;

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
/// 定义需要后置内容、世界或配方阶段的有序回调；无内容依赖的初始化应使用 tML 原生 Load。
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
/// 保留给需要集中排序的框架内部加载任务；无顺序依赖的类型使用 tML 原生 Load 和 TOCleanup。
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
