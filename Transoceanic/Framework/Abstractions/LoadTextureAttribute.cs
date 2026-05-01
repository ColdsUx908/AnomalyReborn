// Developed by ColdsUx

namespace Transoceanic.Framework.Abstractions;

/// <summary>
/// 标记一个静态 <see cref="Asset{T}"/> 字段（T 为 <see cref="Texture2D"/>），使其在模组加载时自动从指定路径加载纹理资源，
/// 并在模组卸载时自动清空引用。
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class LoadTextureAttribute : Attribute
{
    /// <summary>
    /// 要加载的纹理资源的路径。
    /// </summary>
    public string TexturePath;

    /// <summary>
    /// 初始化 <see cref="LoadTextureAttribute"/> 类的新实例，并指定纹理资源的路径。
    /// </summary>
    /// <param name="texturePath">纹理资源的路径，通常相对于模组根目录。</param>
    public LoadTextureAttribute(string texturePath) => TexturePath = texturePath;
}
