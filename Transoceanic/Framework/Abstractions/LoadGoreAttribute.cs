namespace Transoceanic.Framework.Abstractions;

/// <summary>
/// 标记一个静态 <see cref="ModGore"/> 字段，使其在模组加载时自动根据指定名称加载血污资源，
/// 并在模组卸载时自动清空引用。
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class LoadGoreAttribute : Attribute
{
    /// <summary>
    /// 要加载的血污资源的名称。
    /// </summary>
    public string TextureName;

    /// <summary>
    /// 初始化 <see cref="LoadGoreAttribute"/> 类的新实例，并指定血污资源的名称。
    /// </summary>
    /// <param name="textureName">血污资源的名称。</param>
    public LoadGoreAttribute(string textureName) => TextureName = textureName;
}

