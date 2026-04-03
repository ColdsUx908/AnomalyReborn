using System.ComponentModel;
using MonoMod.RuntimeDetour;

namespace Transoceanic.Framework.Abstractions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomDetourSourceAttribute : Attribute
{
    public readonly Type SourceType;

    public readonly string Name;

    public readonly BindingFlags BindingAttr;

    public readonly Type[] ParameterTypes;

    public CustomDetourSourceAttribute(Type sourceType, string name, BindingFlags bindingAttr, Type[] parameterTypes = null)
    {
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentException.ThrowIfNullOrEmpty(name);
        SourceType = sourceType;
        Name = name;
        BindingAttr = bindingAttr;
        ParameterTypes = parameterTypes;
    }

    public MethodInfo Source =>
        ParameterTypes is not null ? SourceType.GetMethod(Name, BindingAttr, ParameterTypes) : SourceType.GetMethod(Name, BindingAttr);
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomDetourPrefixAttribute : Attribute
{
    public readonly string Prefix;

    public CustomDetourPrefixAttribute(string prefix) => Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomDetourConfigAttribute : Attribute
{
    public readonly string Id;

    public readonly int? Priority;

    public readonly string[] Before;

    public readonly string[] After;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly int SubPriority;

    public DetourConfig DetourConfig => new(Id, Priority, Before, After, SubPriority);

    public CustomDetourConfigAttribute(string id) => Id = id ?? throw new ArgumentNullException(nameof(id));
}

/// <summary>
/// 用于标记包含Detour方法的类。
/// <br/>若目标类不是静态类，则建议使用 <see cref="DetourClassToAttribute{T}"/>。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DetourClassToAttribute : Attribute
{
    public readonly Type SourceType;

    public DetourClassToAttribute(Type sourceType) => SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
}

/// <summary>
/// 用于标记包含Detour方法的类。
/// </summary>
/// <typeparam name="T"></typeparam>
public class DetourClassToAttribute<T> : DetourClassToAttribute where T : class
{
    public DetourClassToAttribute() : base(typeof(T)) { }
}

/// <summary>
/// 用于标记包含Detour方法的类。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DetourClassTo_MultiSourceAttribute : Attribute
{
    public readonly Type[] SourceTypes;

    public DetourClassTo_MultiSourceAttribute(params Type[] sourceTypes)
    {
        ArgumentException.ThrowIfNullOrEmptyOrAnyNull(sourceTypes);
        SourceTypes = sourceTypes;
    }
}

/// <summary>
/// 用于标记不在Detour类中的方法是Detour方法。
/// <br/>若目标类不是静态类，则建议使用 <see cref="DetourMethodToAttribute{T}"/>。
/// <remarks>注意：在Detour类特性修饰的类中应用该特性会使Detour重复应用。</remarks>
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class DetourMethodToAttribute : Attribute
{
    public readonly Type SourceType;

    /// <summary>
    /// 参数偏移量，设为负数以禁用参数偏移机制。
    /// </summary>
    public int ParamOffset { get; init; } = -1;

    public DetourMethodToAttribute(Type targetType) => SourceType = targetType ?? throw new ArgumentNullException(nameof(targetType));
}

/// <summary>
/// 用于标记不在Detour类中的方法是Detour方法。
/// <remarks>注意：在Detour类特性修饰的类中应用该特性会使Detour重复应用。</remarks>
/// </summary>
public class DetourMethodToAttribute<T> : DetourMethodToAttribute
{
    public DetourMethodToAttribute() : base(typeof(T)) { }
}

/// <summary>
/// 用于标记由Detour类特性修饰的类中的某个方法，使自动应用逻辑不涉及该方法。
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class NotDetourMethodAttribute : Attribute;

/// <summary>
/// 用于实现自定义的Detour逻辑。
/// </summary>
public interface ITODetourProvider
{
    /// <summary>
    /// 应用Detour逻辑。
    /// <br/>使用 <see cref="TODetourHandler.Modify{TDelegate}(MethodBase, TDelegate)"/> 或 <see cref="TODetourHandler.Modify(MethodBase, MethodInfo)"/> 来实现Detour逻辑，
    /// <br/>以便在 <see cref="TODetourHandler.Detours"/> 中注册Detour，并自动加载和卸载。
    /// </summary>
    public abstract void ApplyDetour();

    /// <summary>
    /// 加载优先级，越大越早加载。
    /// </summary>
    public virtual decimal LoadPriority => 0m;
}
