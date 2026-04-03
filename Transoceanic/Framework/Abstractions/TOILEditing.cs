namespace Transoceanic.Framework.RuntimeEditing;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomManipulatorPrefixAttribute : Attribute
{
    public readonly string Prefix;

    public CustomManipulatorPrefixAttribute(string prefix) => Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
}

/// <summary>
/// 用于标记不在IL编辑类中的方法是IL编辑方法。
/// <br/>若目标类不是静态类，则建议使用 <see cref="ILEditingMethodToAttribute{T}"/>。
/// <remarks>注意：在IL编辑类中应用该特性会使IL编辑方法重复应用。</remarks>
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ILEditingMethodToAttribute : Attribute
{
    public readonly Type SourceType;

    public ILEditingMethodToAttribute(Type targetType) => SourceType = targetType ?? throw new ArgumentNullException(nameof(targetType));
}

/// <summary>
/// 用于标记不在IL编辑中的方法是IL方法。
/// <remarks>注意：在IL编辑类中应用该特性会使IL编辑方法重复应用，很可能会导致问题。</remarks>
/// </summary>
public class ILEditingMethodToAttribute<T> : ILEditingMethodToAttribute
{
    public ILEditingMethodToAttribute() : base(typeof(T)) { }
}