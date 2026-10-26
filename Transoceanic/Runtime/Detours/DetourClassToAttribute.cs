namespace Transoceanic.Runtime.Detours;

/// <summary>
/// 标记一个类，表明该类中的所有静态方法都将尝试作为指定类型的 Detour 方法。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DetourClassToAttribute : Attribute
{
    /// <summary>
    /// 目标源类型。
    /// </summary>
    public readonly Type SourceType;

    /// <summary>
    /// 初始化 <see cref="DetourClassToAttribute"/> 的新实例。
    /// </summary>
    /// <param name="sourceType">目标源类型。</param>
    public DetourClassToAttribute(Type sourceType) => SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
}
