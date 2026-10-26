namespace Transoceanic.Runtime.ILEditing;

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
