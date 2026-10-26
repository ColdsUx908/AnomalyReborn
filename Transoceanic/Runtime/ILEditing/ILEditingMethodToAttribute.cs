namespace Transoceanic.Runtime.ILEditing;

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
