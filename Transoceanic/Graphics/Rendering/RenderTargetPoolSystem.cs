namespace Transoceanic.Graphics.Rendering;

/// <summary>
/// 将延迟归还与 GPU 资源释放连接到客户端绘制生命周期。
/// </summary>
internal sealed class RenderTargetPoolSystem : ModSystem
{
    /// <inheritdoc/>
    /// <remarks>
    /// 客户端注册绘制钩子，在每帧原版绘制前归还延迟租约。
    /// </remarks>
    public override void Load()
    {
        if (Main.dedServ)
            return;
        On_Main.Draw += (orig, self, gameTime) =>
        {
            RenderTargetPool.ClearPendingLeases();
            orig(self, gameTime);
        };
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 在图形主线程清理延迟租约并释放共享池；专用服务器跳过。
    /// </remarks>
    public override void Unload()
    {
        if (Main.dedServ)
            return;
        if (global::Terraria.Program.IsMainThread)
            RenderTargetPool.UnloadShared();
        else
            Main.RunOnMainThread(RenderTargetPool.UnloadShared).GetAwaiter().GetResult();
    }
}
