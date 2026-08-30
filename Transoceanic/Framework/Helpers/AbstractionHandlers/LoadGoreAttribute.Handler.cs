namespace Transoceanic.Framework.Helpers;

/// <summary>
/// 血污加载器，负责处理标记了 <see cref="LoadGoreAttribute"/> 特性的静态 <see cref="ModGore"/> 字段的自动加载与卸载。
/// </summary>
public sealed class GoreLoader : IContentLoader
{
    /// <summary>
    /// 内容设置后阶段：扫描所有带有 <see cref="LoadGoreAttribute"/> 的静态 <see cref="ModGore"/> 字段，
    /// 并根据特性指定的名称请求加载血污资源。
    /// </summary>
    void IContentLoader.PostSetupContent()
    {
        foreach (Mod mod in TOReflectionUtils.GetAllSupportedMods())
        {
            foreach ((FieldInfo field, LoadGoreAttribute attribute) in TOReflectionUtils.GetMembersWithAttribute<FieldInfo, LoadGoreAttribute>(mod))
            {
                if (field.IsStatic && field.FieldType == typeof(ModGore) && !field.IsInitOnly && !field.IsLiteral)
                    field.SetValue(null, mod.Find<ModGore>(attribute.TextureName));
            }
        }
    }

    /// <summary>
    /// 模组卸载时：清理之前加载的血污资源引用。
    /// </summary>
    void IContentLoader.OnModUnload()
    {
        foreach ((FieldInfo field, _) in TOReflectionUtils.GetMembersWithAttribute<FieldInfo, LoadGoreAttribute>())
        {
            if (field.IsStatic && field.FieldType == typeof(ModGore) && !field.IsInitOnly && !field.IsLiteral)
                field.SetValue(null, null);
        }
    }
}
