namespace Transoceanic.Framework.Abstractions;

/// <summary>
/// 标记一个静态 <see cref="Asset{T}"/> 字段（T 为 <see cref="Effect"/>），使其在模组加载时自动从指定路径加载特效资源，
/// 并在模组卸载时自动清空引用。
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class LoadEffectAttribute : Attribute
{
    /// <summary>
    /// 要加载的特效资源的路径。
    /// </summary>
    public string EffectPath;

    /// <summary>
    /// 初始化 <see cref="LoadEffectAttribute"/> 类的新实例，并指定特效资源的路径。
    /// </summary>
    /// <param name="effectPath">特效资源的路径，通常相对于模组根目录。</param>
    public LoadEffectAttribute(string effectPath) => EffectPath = effectPath;
}

