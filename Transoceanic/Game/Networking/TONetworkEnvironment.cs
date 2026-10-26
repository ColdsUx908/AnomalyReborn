namespace Transoceanic.Game.Networking;

public static class TONetworkEnvironment
{
    /// <summary>
    /// 是否启用 Transoceanic 模组内置的网络同步。
    /// <br/>由于 Transoceanic 是不同步模组，该选项必须由依赖模组手动开启。
    /// </summary>
    /// <remarks>
    /// 一旦设置为 <see langword="true"/> 后，除非模组正在卸载，否则不可重新设置为 <see langword="false"/>，否则将抛出 <see cref="InvalidOperationException"/>。
    /// </remarks>
    /// <exception cref="InvalidOperationException">在非卸载状态下尝试将已启用的同步关闭。</exception>
    public static bool SyncEnabled
    {
        get;
        set
        {
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
