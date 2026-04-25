// Designed by ColdsUx

namespace Transoceanic.Framework.Helpers.AbstractionHandlers;

/// <summary>
/// 调度所有实现了 <see cref="IContentLoader"/> 接口的类型，在对应的生命周期阶段按优先级执行加载与卸载逻辑。
/// </summary>
public sealed class TOLoaderHandler : ModSystem
{
    /// <summary>
    /// 在所有配方添加后调用，按 <see cref="LoadPriorityAttribute"/> 排序执行加载器的 <see cref="IContentLoader.PostAddRecipes"/>。
    /// </summary>
    public override void PostAddRecipes()
    {
        foreach (IContentLoader loader in
            from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<IContentLoader>()
            orderby pair.type.GetMethod(nameof(IContentLoader.PostAddRecipes), TOReflectionUtils.UniversalBindingFlags)?.Attribute<LoadPriorityAttribute>()?.Priority ?? 0
            select pair.instance)
        {
            loader.PostAddRecipes();
        }
    }

    /// <summary>
    /// 模组卸载时调用，按优先级逆序执行加载器的 <see cref="IContentLoader.OnModUnload"/>。
    /// </summary>
    public override void OnModUnload()
    {
        foreach (IContentLoader loader in (
            from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<IContentLoader>()
            orderby pair.type.GetMethod(nameof(IContentLoader.PostSetupContent), TOReflectionUtils.UniversalBindingFlags)?.Attribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
            select pair.instance).Reverse())
        {
            loader.OnModUnload();
        }
    }

    /// <summary>
    /// 世界加载时调用，按优先级顺序执行加载器的 <see cref="IContentLoader.OnWorldLoad"/>。
    /// </summary>
    public override void OnWorldLoad()
    {
        foreach (IContentLoader loader in
            from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<IContentLoader>()
            orderby pair.type.GetMethod(nameof(IContentLoader.OnWorldLoad), TOReflectionUtils.UniversalBindingFlags)?.Attribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
            select pair.instance)
        {
            loader.OnWorldLoad();
        }
    }

    /// <summary>
    /// 世界卸载时调用，按优先级逆序执行加载器的 <see cref="IContentLoader.OnWorldUnload"/>。
    /// </summary>
    public override void OnWorldUnload()
    {
        if (TOMain.Loaded)
        {
            foreach (IContentLoader loader in (
                from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<IContentLoader>()
                orderby pair.type.GetMethod(nameof(IContentLoader.OnWorldLoad), TOReflectionUtils.UniversalBindingFlags)?.Attribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
                select pair.instance).Reverse())
            {
                loader.OnWorldUnload();
            }
        }
    }
}