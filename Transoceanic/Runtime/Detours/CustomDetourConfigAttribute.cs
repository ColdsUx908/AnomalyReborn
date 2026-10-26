using System.ComponentModel;
using MonoMod.RuntimeDetour;

namespace Transoceanic.Runtime.Detours;

/// <summary>
/// 为 Detour 方法提供配置信息，例如 ID、优先级、前置/后置依赖。
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomDetourConfigAttribute : Attribute
{
    /// <summary>
    /// Detour 的唯一标识符。
    /// </summary>
    public readonly string Id;

    /// <summary>
    /// Detour 的优先级，数值越大越先应用。
    /// </summary>
    public readonly int? Priority;

    /// <summary>
    /// 必须在此 Detour 之前应用的其他 Detour 的 ID 列表。
    /// </summary>
    public readonly string[] Before;

    /// <summary>
    /// 必须在此 Detour 之后应用的其他 Detour 的 ID 列表。
    /// </summary>
    public readonly string[] After;

    /// <summary>
    /// 内部使用的子优先级。
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly int SubPriority;

    /// <summary>
    /// 根据当前特性生成对应的 <see cref="MonoMod.RuntimeDetour.DetourConfig"/> 实例。
    /// </summary>
    public DetourConfig DetourConfig => new(Id, Priority, Before, After, SubPriority);

    /// <summary>
    /// 初始化 <see cref="CustomDetourConfigAttribute"/> 的新实例。
    /// </summary>
    /// <param name="id">Detour 的唯一标识符。</param>
    public CustomDetourConfigAttribute(string id) => Id = id ?? throw new ArgumentNullException(nameof(id));
}
