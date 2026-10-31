using Transoceanic.Runtime.Reflection;
namespace Transoceanic.Core.Loading;

/// <summary>
/// 在一次模组加载期间复用加载器实例，并按优先级调度生命周期回调。
/// </summary>
internal sealed class TOLoaderHandler : ModSystem
{
    private static readonly Dictionary<Type, object> _Instances = [];
    private static readonly List<ITOLoader> _InternalLoaders = [];
    private static readonly List<IContentLoader> _ContentLoaders = [];
    private static readonly List<IContentLoader> _WorldLoaders = [];

    /// <inheritdoc/>
    /// <remarks>
    /// 按配方回调的加载优先级依次通知已登记的内容加载器。
    /// </remarks>
    public override void PostAddRecipes()
    {
        foreach (IContentLoader loader in OrderLoaders(_ContentLoaders, nameof(IContentLoader.PostAddRecipes)))
            loader.PostAddRecipes();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 逆序清理世界和内容加载器；单个清理失败不阻止其余清理，最终聚合抛出异常。
    /// </remarks>
    public override void OnModUnload() => TOCleanup.Execute([
        () => UnloadReverse(_WorldLoaders, loader => loader.OnWorldUnload()),
        () => UnloadReverse(_ContentLoaders, loader => loader.OnModUnload()),
        TOCleanup.Run
    ]);

    /// <inheritdoc/>
    /// <remarks>
    /// 按优先级执行世界加载，调用前登记清理资格，包含初始化失败的加载器。
    /// </remarks>
    public override void OnWorldLoad()
    {
        foreach (IContentLoader loader in OrderLoaders(_ContentLoaders, nameof(IContentLoader.OnWorldLoad)))
        {
            // 失败的回调也可能已经分配了资源，保留它以供清理。
            _WorldLoaders.Add(loader);
            loader.OnWorldLoad();
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 按实际世界初始化的逆序清理加载器，并聚合清理异常。
    /// </remarks>
    public override void OnWorldUnload() => UnloadReverse(_WorldLoaders, loader => loader.OnWorldUnload());

    /// <summary>
    /// 发现并复用框架程序集中的 ITOLoader 实例，按优先级初始化并登记卸载资格。
    /// </summary>
    internal static void Loader_Load()
    {
        foreach (ITOLoader loader in OrderLoaders(GetLoaderInstances<ITOLoader>(TOReflectionUtils.Assembly), nameof(ITOLoader.Load)))
        {
            _InternalLoaders.Add(loader);
            loader.Load();
        }
    }

    /// <summary>
    /// 发现并复用各模组的内容加载器，按优先级初始化并登记卸载资格。
    /// </summary>
    internal static void Loader_PostSetupContent()
    {
        foreach (IContentLoader loader in OrderLoaders(GetLoaderInstances<IContentLoader>(), nameof(IContentLoader.PostSetupContent)))
        {
            _ContentLoaders.Add(loader);
            loader.PostSetupContent();
        }
    }

    /// <summary>
    /// 逆序卸载框架加载器，并在 finally 中清空按类型保存的实例。
    /// </summary>
    internal static void Loader_Unload()
    {
        try
        {
            UnloadReverse(_InternalLoaders, loader => loader.Unload());
        }
        finally
        {
            _Instances.Clear();
        }
    }

    /// <summary>
    /// 行为分发与生命周期使用同一实例；显式跳过构造的粒子模板不走此入口。
    /// </summary>
    internal static IEnumerable<T> GetLoaderInstances<T>(Assembly assembly = null) where T : class
    {
        // 从实际内容取得所有权，支持手动添加的闭合泛型或由其他程序集提供的内容。
        Dictionary<Type, T> registered = TOReflectionUtils.GetAllSupportedMods()
            .SelectMany(mod => mod.GetContent()).OfType<T>()
            .Where(content => assembly is null || content.GetType().Assembly == assembly)
            .GroupBy(content => content.GetType())
            .ToDictionary(group => group.Key, group => group.Single());
        IEnumerable<Type> discovered = assembly is null
            ? TOReflectionUtils.GetTypesDerivedFrom<T>()
            : TOReflectionUtils.GetTypesDerivedFrom<T>(assembly);
        // 保留已有反射类型顺序，只将反射列表没有的手动内容追加到末尾。
        foreach (Type type in discovered.Concat(registered.Keys).Distinct())
        {
            if (registered.TryGetValue(type, out T content))
            {
                _Instances[type] = content;
                yield return content;
            }
            else if (!typeof(ILoadable).IsAssignableFrom(type))
            {
                if (!_Instances.TryGetValue(type, out object instance))
                {
                    instance = Activator.CreateInstance(type, true);
                    _Instances.Add(type, instance);
                }
                yield return (T)instance;
            }
        }
    }

    private static IEnumerable<T> OrderLoaders<T>(IEnumerable<T> loaders, string methodName) where T : class =>
        loaders.OrderByDescending(loader => GetPriority(loader.GetType(), typeof(T), methodName))
            .ThenBy(loader => loader.GetType().Assembly.GetName().Name, StringComparer.Ordinal)
            .ThenBy(loader => loader.GetType().FullName, StringComparer.Ordinal)
            .ToArray();

    private static double GetPriority(Type type, Type interfaceType, string methodName)
    {
        InterfaceMapping mapping = type.GetInterfaceMap(interfaceType);
        int index = Array.FindIndex(mapping.InterfaceMethods, method => method.Name == methodName);
        return index < 0 ? 0 : mapping.TargetMethods[index].GetCustomAttribute<LoadPriorityAttribute>(true)?.Priority ?? 0;
    }

    private static void UnloadReverse<T>(List<T> loaders, Action<T> unload)
    {
        List<Exception> exceptions = [];
        for (int i = loaders.Count - 1; i >= 0; i--)
        {
            try
            {
                unload(loaders[i]);
            }
            catch (Exception exception)
            {
                exceptions.Add(exception);
            }
        }
        loaders.Clear();
        if (exceptions.Count > 0)
            throw new AggregateException("Loader cleanup failed.", exceptions);
    }
}
