namespace CalamityAnomalies.DataStructures;

public sealed class TextureAssetWithCalamityStyle
{
    public Asset<Texture2D> OriginalAsset;
    public Asset<Texture2D> CalamityStyleAsset;

    public TextureAssetWithCalamityStyle(string pathWithoutSuffix)
    {
        OriginalAsset = ModContent.Request<Texture2D>(pathWithoutSuffix);
        CalamityStyleAsset = ModContent.Request<Texture2D>(pathWithoutSuffix + "_CalamityStyle");
    }

    public Texture2D Value => CAClientConfig.Instance.UseCalamityStyleTextures ? CalamityStyleAsset.Value : OriginalAsset.Value;
}

/// <summary>
/// 标记一个静态 <see cref="TextureAssetWithCalamityStyle"/> 字段，使其在模组加载时自动从指定路径加载纹理资源，
/// 并在模组卸载时自动清空引用。
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class LoadTextureWithCalamityStyleAttribute : Attribute
{
    /// <summary>
    /// 要加载的纹理资源的路径，不包含后缀（"_CalamityStyle"）。
    /// </summary>
    public string TexturePathWithoutSuffix;

    /// <summary>
    /// 初始化 <see cref="LoadTextureWithCalamityStyleAttribute"/> 类的新实例，并指定纹理资源的路径。
    /// </summary>
    /// <param name="texturePathWithoutSuffix">纹理资源的路径，不包含后缀（"_CalamityStyle"），通常相对于模组根目录。</param>
    public LoadTextureWithCalamityStyleAttribute(string texturePathWithoutSuffix) => TexturePathWithoutSuffix = texturePathWithoutSuffix;
}

public sealed class TextureAssetWithCalamityStyleLoader : IContentLoader
{
    void IContentLoader.PostSetupContent()
    {
        foreach ((FieldInfo field, LoadTextureWithCalamityStyleAttribute attribute) in TOReflectionUtils.GetMembersWithAttribute<FieldInfo, LoadTextureWithCalamityStyleAttribute>())
        {
            if (field.IsStatic && field.FieldType == typeof(TextureAssetWithCalamityStyle) && !field.IsInitOnly && !field.IsLiteral)
                field.SetValue(null, new TextureAssetWithCalamityStyle(attribute.TexturePathWithoutSuffix));
        }
    }

    void IContentLoader.OnModUnload()
    {
        foreach ((FieldInfo field, _) in TOReflectionUtils.GetMembersWithAttribute<FieldInfo, LoadTextureWithCalamityStyleAttribute>())
        {
            if (field.IsStatic && field.FieldType == typeof(TextureAssetWithCalamityStyle) && !field.IsInitOnly && !field.IsLiteral)
                field.SetValue(null, null);
        }
    }
}