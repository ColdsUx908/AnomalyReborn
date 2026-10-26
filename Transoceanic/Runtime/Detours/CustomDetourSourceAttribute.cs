namespace Transoceanic.Runtime.Detours;

/// <summary>
/// 标记一个方法为 Detour 的目标源方法，并指定其所在的类型、名称和绑定标志。
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomDetourSourceAttribute : Attribute
{
    /// <summary>
    /// 源方法所在的目标类型。
    /// </summary>
    public readonly Type SourceType;

    /// <summary>
    /// 源方法的名称。
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// 用于反射查找源方法的绑定标志。
    /// </summary>
    public readonly BindingFlags BindingAttr;

    /// <summary>
    /// 源方法的参数类型数组（可选，用于区分重载）。
    /// </summary>
    public readonly Type[] ParameterTypes;

    /// <summary>
    /// 初始化 <see cref="CustomDetourSourceAttribute"/> 的新实例。
    /// </summary>
    /// <param name="sourceType">源方法所在的目标类型。</param>
    /// <param name="name">源方法的名称。</param>
    /// <param name="bindingAttr">反射绑定时使用的标志。</param>
    /// <param name="parameterTypes">参数类型数组，若为 <see langword="null"/> 则忽略参数类型进行匹配。</param>
    public CustomDetourSourceAttribute(Type sourceType, string name, BindingFlags bindingAttr, Type[] parameterTypes = null)
    {
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentException.ThrowIfNullOrEmpty(name);
        SourceType = sourceType;
        Name = name;
        BindingAttr = bindingAttr;
        ParameterTypes = parameterTypes;
    }

    /// <summary>
    /// 通过反射获取该特性指定的源方法信息。
    /// </summary>
    public MethodInfo Source =>
        ParameterTypes is not null ? SourceType.GetMethod(Name, BindingAttr, ParameterTypes) : SourceType.GetMethod(Name, BindingAttr);
}
