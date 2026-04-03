namespace Transoceanic.Framework.Helpers.AbstractionHandlers;

public sealed class TOLoaderHandler : ModSystem
{
    public override void PostAddRecipes()
    {
        foreach (IContentLoader loader in
            from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<IContentLoader>().AsValueEnumerable()
            orderby pair.type.GetMethod(nameof(IContentLoader.PostAddRecipes), TOReflectionUtils.UniversalBindingFlags)?.Attribute<LoadPriorityAttribute>()?.Priority ?? 0
            select pair.instance)
        {
            loader.PostAddRecipes();
        }
    }

    public override void OnModUnload()
    {
        foreach (IContentLoader loader in (
            from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<IContentLoader>().AsValueEnumerable()
            orderby pair.type.GetMethod(nameof(IContentLoader.PostSetupContent), TOReflectionUtils.UniversalBindingFlags)?.Attribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
            select pair.instance).Reverse())
        {
            loader.OnModUnload();
        }
    }

    public override void OnWorldLoad()
    {
        foreach (IContentLoader loader in
            from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<IContentLoader>().AsValueEnumerable()
            orderby pair.type.GetMethod(nameof(IContentLoader.OnWorldLoad), TOReflectionUtils.UniversalBindingFlags)?.Attribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
            select pair.instance)
        {
            loader.OnWorldLoad();
        }
    }

    public override void OnWorldUnload()
    {
        if (TOMain.Loaded)
        {
            foreach (IContentLoader loader in (
                from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<IContentLoader>().AsValueEnumerable()
                orderby pair.type.GetMethod(nameof(IContentLoader.OnWorldLoad), TOReflectionUtils.UniversalBindingFlags)?.Attribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
                select pair.instance).Reverse())
            {
                loader.OnWorldUnload();
            }
        }
    }
}