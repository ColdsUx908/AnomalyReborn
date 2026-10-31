namespace Transoceanic.Game.Networking;

/// <summary>
/// 管理 NoSync 框架的同步启用契约、消费方登记和本地网络环境查询。
/// </summary>
public static class TONetworkEnvironment
{
    private static readonly HashSet<string> _Consumers = new(StringComparer.Ordinal);
    private static bool _SetupComplete;

    /// <summary>
    /// 获取通过 EnableSync 注册的消费模组名称快照。该列表仅用于本地诊断，不是远端握手结果。
    /// </summary>
    public static IReadOnlyList<string> SyncConsumers => _Consumers.OrderBy(name => name, StringComparer.Ordinal).ToArray();

    /// <summary>
    /// 在消费模组的 tML 加载阶段无条件启用 Ocean AI 同步，并记录启用方；可从 Mod.Load 或其原生内容 Load 调用。
    /// 消费模组必须为 Both、正式强依赖 Transoceanic，各端使用相同构建；本方法不协商远端协议。
    /// </summary>
    /// <param name="consumer">由 tML 提供的消费模组实例；内容自动加载时不依赖其 Mod.Load 初始化的静态单例。</param>
    /// <exception cref="ArgumentNullException">消费模组为空。</exception>
    /// <exception cref="InvalidOperationException">不在加载阶段、消费模组未注册或不是 Both。</exception>
    public static void EnableSync(Mod consumer)
    {
        ArgumentNullException.ThrowIfNull(consumer);
        if (TOMain.Instance is null || TOMain.Unloading || _SetupComplete)
            throw new InvalidOperationException("EnableSync must be called unconditionally during native loading of the consumer or its contents, before PostSetupContent completes.");
        if (!ModLoader.Mods.Contains(consumer) || consumer.Side != ModSide.Both)
            throw new InvalidOperationException("The sync consumer must be a registered Both-side mod with a strong Transoceanic dependency.");
        SyncEnabled = true;
        if (_Consumers.Add(consumer.Name))
            TOMain.Instance.Logger.Info($"Ocean AI sync enabled by {consumer.Name} {consumer.Version}; Transoceanic {TOMain.Instance.Version}. All peers must use identical builds and enable sync during native Load. A strong modReferences dependency is required and is not verified by this API.");
    }

    internal static void CompleteSetup()
    {
        _SetupComplete = true;
        if (SyncEnabled && _Consumers.Count == 0)
            TOMain.Instance.Logger.Warn("Ocean AI sync was enabled through the legacy SyncEnabled setter; no consumer was registered. Use EnableSync(this) in the Both consumer Mod.Load and declare a strong modReferences dependency.");
        TOMain.Instance.Logger.Info($"Ocean AI sync: {SyncEnabled}; consumers: {string.Join(", ", SyncConsumers)}. NoSync provides no runtime handshake or remote version verification.");
    }

    internal static void Reset()
    {
        SyncEnabled = false;
        _Consumers.Clear();
        _SetupComplete = false;
    }

    /// <summary>
    /// 是否启用 Transoceanic 模组内置的网络同步。新消费方应使用 EnableSync 注册启用方。
    /// <br/>由于 Transoceanic 是不同步模组，该选项必须由依赖模组手动开启。
    /// </summary>
    /// <remarks>
    /// 启用方必须保证客户端与服务端安装相同构建，并在各端 Mod.Load 中无条件启用。
    /// 禁止按本地配置、netMode 或世界状态切换；该值会改变全局 ExtraAI 的读写布局。
    /// NoSync 允许单端安装，不负责协商本开关。详细接入约定见同目录 README.md。
    /// 一旦设置为 <see langword="true"/> 后，除非模组正在卸载，否则不可重新设置为 <see langword="false"/>，否则将抛出 <see cref="InvalidOperationException"/>。
    /// </remarks>
    /// <exception cref="InvalidOperationException">内容设置完成后首次启用同步，或在非卸载状态下关闭已启用的同步。</exception>
    public static bool SyncEnabled
    {
        get;
        set
        {
            if (!field && value && _SetupComplete)
                throw new InvalidOperationException("Ocean AI sync cannot be enabled after content setup; call EnableSync during Mod.Load.");
            if (field && !value && !TOMain.Unloading)
                throw new InvalidOperationException("SyncEnabled cannot be set to false after it has been set to true, unless unloading.");
            field = value;
        }
    }

    /// <summary>
    /// 当前是否处于多人模式（既非单人客户端也非单人服务器）。
    /// </summary>
    public static bool Multiplayer => Main.netMode != NetmodeID.SinglePlayer;

    /// <summary>
    /// 当前是否不是客户端模式（服务器或单人，而非多人客户端）。
    /// </summary>
    /// <remarks>
    /// 即 <see cref="Main.netMode"/> 不等于 <see cref="NetmodeID.MultiplayerClient"/>，用于判断代码是否运行在本地完整权限环境下。
    /// </remarks>
    public static bool NotClient => Main.netMode != NetmodeID.MultiplayerClient;
}
