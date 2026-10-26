namespace Transoceanic.Runtime.Detours;

/// <summary>
/// 标记一个类，表明该类中的所有静态方法都可能作为多个源类型之一的 Detour 方法，通过方法名前缀中的类型名区分。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DetourClassTo_MultiSourceAttribute : Attribute
{
    /// <summary>
    /// 所有可能的源类型数组。
    /// </summary>
    public readonly Type[] SourceTypes;

    /// <summary>
    /// 初始化 <see cref="DetourClassTo_MultiSourceAttribute"/> 的新实例。
    /// </summary>
    /// <param name="sourceTypes">源类型数组，不能为空且不能包含空元素。</param>
    public DetourClassTo_MultiSourceAttribute(params Type[] sourceTypes)
    {
        ArgumentException.ThrowIfNullOrEmptyOrAnyNull(sourceTypes);
        SourceTypes = sourceTypes;
    }
}
