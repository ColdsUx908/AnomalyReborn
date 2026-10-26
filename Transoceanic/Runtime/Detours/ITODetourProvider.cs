namespace Transoceanic.Runtime.Detours;

/// <summary>
/// 实现自定义 Detour 逻辑的提供程序接口。
/// </summary>
public interface ITODetourProvider
{
    /// <summary>
    /// 在此方法中应用 Detour 逻辑。
    /// 应使用 <see cref="TODetourHandler.Modify{TDelegate}(MethodBase, TDelegate)"/> 或类似方法注册 Detour，
    /// 以便框架自动管理 Detour 的加载与卸载。
    /// </summary>
    public abstract void ApplyDetour();

    /// <summary>
    /// 获取 Detour 的加载优先级。数值越大，越早加载。
    /// </summary>
    public virtual decimal LoadPriority => 0m;
}
