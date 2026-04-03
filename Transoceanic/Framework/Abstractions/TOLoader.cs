namespace Transoceanic.Framework.Abstractions;

[AttributeUsage(AttributeTargets.Method)]
public class LoadPriorityAttribute : Attribute
{
    /// <summary>
    /// 加载优先级。
    /// <remarks>值越大，加载越早。</remarks>
    /// </summary>
    public readonly double Priority;

    public LoadPriorityAttribute(double priority) => Priority = priority;
}

/// <summary>
/// 资源加载器接口。
/// </summary>
public interface IContentLoader
{
    /// <summary>
    /// 内容加载完成后调用。
    /// </summary>
    public virtual void PostSetupContent() { }

    /// <summary>
    /// 添加配方完成后调用。
    /// </summary>
    public virtual void PostAddRecipes() { }

    /// <summary>
    /// 在Mod卸载前调用。
    /// </summary>
    public virtual void OnModUnload() { }

    /// <summary>
    /// 在世界加载时调用。
    /// </summary>
    public virtual void OnWorldLoad() { }

    /// <summary>
    /// 在世界卸载时调用。
    /// </summary>
    public virtual void OnWorldUnload() { }
}

internal interface ITOLoader
{
    /// <summary>
    /// 在本Mod加载时调用。
    /// </summary>
    internal virtual void Load() { }

    /// <summary>
    /// 在Mod卸载时调用。
    /// </summary>
    internal virtual void Unload() { }
}
