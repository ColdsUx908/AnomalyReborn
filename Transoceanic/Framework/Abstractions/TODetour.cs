using System.ComponentModel;
using MonoMod.RuntimeDetour;

namespace Transoceanic.Framework.Abstractions;

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
    /// <param name="parameterTypes">参数类型数组，若为 <c>null</c> 则忽略参数类型进行匹配。</param>
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

/// <summary>
/// 为 Detour 方法指定自定义名称前缀，用于自动解析源方法名称。
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomDetourPrefixAttribute : Attribute
{
    /// <summary>
    /// 自定义的前缀字符串。
    /// </summary>
    public readonly string Prefix;

    /// <summary>
    /// 初始化 <see cref="CustomDetourPrefixAttribute"/> 的新实例。
    /// </summary>
    /// <param name="prefix">要使用的前缀。</param>
    public CustomDetourPrefixAttribute(string prefix) => Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
}

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

/// <summary>
/// 标记一个类，表明该类中的所有静态方法都将尝试作为指定泛型类型的 Detour 方法。
/// </summary>
/// <typeparam name="T">目标源类型。</typeparam>
public class DetourClassToAttribute<T> : DetourClassToAttribute where T : class
{
    /// <summary>
    /// 初始化 <see cref="DetourClassToAttribute{T}"/> 的新实例。
    /// </summary>
    public DetourClassToAttribute() : base(typeof(T)) { }
}

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

/// <summary>
/// 标记一个不在 Detour 类中的方法，指明其要 Detour 的目标泛型源类型。
/// </summary>
/// <typeparam name="T">目标源类型。</typeparam>
public class DetourMethodToAttribute<T> : DetourMethodToAttribute
{
    /// <summary>
    /// 初始化 <see cref="DetourMethodToAttribute{T}"/> 的新实例。
    /// </summary>
    public DetourMethodToAttribute() : base(typeof(T)) { }
}

/// <summary>
/// 用于标记一个方法，使其在自动 Detour 应用逻辑中被忽略。
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class NotDetourMethodAttribute : Attribute;

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