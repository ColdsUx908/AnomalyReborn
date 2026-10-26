namespace Transoceanic.Runtime.Detours;

/// <summary>
/// 标记一个不在 Detour 类中的方法，指明其要 Detour 的目标源类型。
/// </summary>
/// <remarks>
/// 注意：在已由 <see cref="DetourClassToAttribute"/> 标记的类中使用此特性可能导致 Detour 重复应用。
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class DetourMethodToAttribute : Attribute
{
    /// <summary>
    /// 目标源类型。
    /// </summary>
    public readonly Type SourceType;

    /// <summary>
    /// 获取或设置参数偏移量。若为负数则禁用参数偏移机制。
    /// 用于指示目标方法的第一个参数在 Detour 方法中的索引位置。
    /// </summary>
    public int ParamOffset { get; init; } = -1;

    /// <summary>
    /// 初始化 <see cref="DetourMethodToAttribute"/> 的新实例。
    /// </summary>
    /// <param name="targetType">目标源类型。</param>
    public DetourMethodToAttribute(Type targetType) => SourceType = targetType ?? throw new ArgumentNullException(nameof(targetType));
}
