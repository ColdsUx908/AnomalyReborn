using Transoceanic.Game.Networking;
namespace Transoceanic;

/// <summary>
/// Transoceanic 模组的主入口类。
/// </summary>
public sealed class TOMain : Mod
{
    /// <summary>
    /// 获取当前 Transoceanic 模组的唯一实例。
    /// </summary>
    internal static TOMain Instance { get; private set; }

    /// <summary>
    /// 获取一个值，指示模组是否正在执行加载过程。
    /// </summary>
    internal static bool Loading { get; private set; }

    /// <summary>
    /// 获取一个值，指示模组是否已完成加载。
    /// </summary>
    internal static bool Loaded { get; private set; }

    /// <summary>
    /// 获取一个值，指示模组是否正在执行卸载过程。
    /// </summary>
    internal static bool Unloading { get; private set; }

    /// <summary>
    /// 获取一个值，指示模组是否已完成卸载。
    /// </summary>
    internal static bool Unloaded { get; private set; }

    /// <inheritdoc/>
    /// <remarks>
    /// 模组加载入口点。
    /// 加载过程的状态由 <see cref="Loading"/> 和 <see cref="Loaded"/> 标志跟踪。
    /// </remarks>
    public override void Load()
    {
        Loading = true;
        Loaded = false;
        Unloaded = false;
        List<Exception> failures = [];
        try
        {
            Instance = this;
            TOLoaderHandler.Loader_Load();
            Loaded = true;
        }
        catch (Exception exception)
        {
            failures.Add(exception);
        }
        finally
        {
            Loading = false;
        }

        if (failures.Count > 0)
            throw new AggregateException("Transoceanic loading failed.", failures).Flatten();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 在所有模组的内容加载完成后调用。
    /// </remarks>
    public override void PostSetupContent()
    {
        TOLoaderHandler.Loader_PostSetupContent();
        TONetworkEnvironment.CompleteSetup();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 模组卸载入口点。
    /// 卸载过程的状态由 <see cref="Unloading"/> 和 <see cref="Unloaded"/> 标志跟踪。
    /// </remarks>
    public override void Unload()
    {
        Unloading = true;
        List<Exception> failures = [];
        try
        {
            try { TOLoaderHandler.Loader_Unload(); }
            catch (Exception exception) { failures.Add(exception); }
            try { TOCleanup.Run(); }
            catch (Exception exception) { failures.Add(exception); }
            if (failures.Count > 0)
                throw new AggregateException("Transoceanic unloading failed.", failures).Flatten();
        }
        finally
        {
            TONetworkEnvironment.Reset();
            Instance = null;
            Loaded = false;
            Unloaded = true;
            Unloading = false;
        }
    }
}
