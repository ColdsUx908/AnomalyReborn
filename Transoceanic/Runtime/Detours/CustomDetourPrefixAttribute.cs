namespace Transoceanic.Runtime.Detours;

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
