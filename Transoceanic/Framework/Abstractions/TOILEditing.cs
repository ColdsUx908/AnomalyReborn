// Developed by ColdsUx

namespace Transoceanic.Framework.Abstractions;

/// <summary>
/// 为 IL 编辑方法指定自定义名称前缀，用于自动解析目标方法名称。
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomManipulatorPrefixAttribute : Attribute
{
    /// <summary>
    /// 自定义的前缀字符串。
    /// </summary>
    public readonly string Prefix;

    /// <summary>
    /// 初始化 <see cref="CustomManipulatorPrefixAttribute"/> 的新实例。
    /// </summary>
    /// <param name="prefix">要使用的前缀。</param>
    public CustomManipulatorPrefixAttribute(string prefix) => Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
}

/// <summary>
/// 标记一个不在 IL 编辑类中的方法，指明其要编辑 IL 的目标源类型。
/// </summary>
/// <remarks>
/// 注意：在已由类似 <c>ILEditingClassToAttribute</c> 特性标记的类中使用此特性可能导致 IL 编辑方法重复应用。
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ILEditingMethodToAttribute : Attribute
{
    /// <summary>
    /// 目标源类型。
    /// </summary>
    public readonly Type SourceType;

    /// <summary>
    /// 初始化 <see cref="ILEditingMethodToAttribute"/> 的新实例。
    /// </summary>
    /// <param name="targetType">目标源类型。</param>
    public ILEditingMethodToAttribute(Type targetType) => SourceType = targetType ?? throw new ArgumentNullException(nameof(targetType));
}

/// <summary>
/// 标记一个不在 IL 编辑类中的方法，指明其要编辑 IL 的目标泛型源类型。
/// </summary>
/// <typeparam name="T">目标源类型。</typeparam>
public class ILEditingMethodToAttribute<T> : ILEditingMethodToAttribute
{
    /// <summary>
    /// 初始化 <see cref="ILEditingMethodToAttribute{T}"/> 的新实例。
    /// </summary>
    public ILEditingMethodToAttribute() : base(typeof(T)) { }
}