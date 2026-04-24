namespace Transoceanic.Framework.Helpers.AbstractionHandlers;

/// <summary>
/// 资产加载器，负责处理标记了 <see cref="LoadTextureAttribute"/> 特性的静态 <see cref="Asset{T}"/> 字段的自动加载与卸载。
/// </summary>
public sealed class AssetLoader : IContentLoader
{
    /// <summary>
    /// 内容设置后阶段：扫描所有带有 <see cref="LoadTextureAttribute"/> 的静态 <see cref="Asset{Texture2D}"/> 字段，
    /// 并根据特性指定的路径请求加载纹理。
    /// </summary>
    void IContentLoader.PostSetupContent()
    {
        foreach ((FieldInfo field, LoadTextureAttribute attribute) in TOReflectionUtils.GetMembersWithAttribute<FieldInfo, LoadTextureAttribute>())
        {
            if (field.IsStatic && field.FieldType == typeof(Asset<Texture2D>) && !field.IsInitOnly && !field.IsLiteral)
                field.SetValue(null, ModContent.Request<Texture2D>(attribute.TexturePath));
        }
    }

    /// <summary>
    /// 模组卸载时：清理之前加载的纹理资产引用。
    /// </summary>
    void IContentLoader.OnModUnload()
    {
        foreach ((FieldInfo field, _) in TOReflectionUtils.GetMembersWithAttribute<FieldInfo, LoadTextureAttribute>())
        {
            if (field.IsStatic && field.FieldType == typeof(Asset<Texture2D>) && !field.IsInitOnly && !field.IsLiteral)
                field.SetValue(null, null);
        }
    }
}