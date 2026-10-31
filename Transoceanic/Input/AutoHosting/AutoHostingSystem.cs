using Terraria.GameInput;
using Terraria.UI;

namespace Transoceanic.Input.AutoHosting;

/// <summary>
/// 应用当前托管输入并绘制共享托管状态。
/// </summary>
internal sealed class AutoHostingSystem : ModSystem
{
    /// <inheritdoc/>
    /// <remarks>
    /// 客户端注册输入钩子，在原版输入更新周围校验中断并应用托管鼠标。
    /// </remarks>
    public override void Load()
    {
        if (Main.dedServ)
            return;
        On_PlayerInput.UpdateInput += UpdateInput;
        On_PlayerInput.UpdateMainMouse += UpdateMainMouse;
    }

    private static void UpdateInput(On_PlayerInput.orig_UpdateInput original)
    {
        AutoHosting.CheckCurrentInput();
        original();
        AutoHosting.ApplyCurrentMouse();
    }

    private static void UpdateMainMouse(On_PlayerInput.orig_UpdateMainMouse original)
    {
        AutoHosting.CheckCurrentInput();
        AutoHosting.ApplyCurrentMouse();
        original();
        AutoHosting.ApplyCurrentMouse();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 每次 UI 更新时检查托管是否被用户操作或游戏状态中断。
    /// </remarks>
    public override void UpdateUI(GameTime gameTime) => AutoHosting.CheckCurrentInput();

    /// <inheritdoc/>
    /// <remarks>
    /// 托管有效且不在主菜单时，在光标层前插入托管状态提示。
    /// </remarks>
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        if (!AutoHosting.IsHosting || Main.gameMenu)
            return;
        int index = layers.FindIndex(layer => layer.Name == "Vanilla: Cursor");
        if (index < 0)
            index = layers.Count;
        layers.Insert(index, new LegacyGameInterfaceLayer("Transoceanic: Auto Hosting", DrawStatus, InterfaceScaleType.UI));
    }

    private static bool DrawStatus()
    {
        AutoHosting.CheckCurrentInput();
        if (AutoHosting.IsHosting && !Main.gameMenu)
            Utils.DrawBorderString(Main.spriteBatch, Language.GetTextValue("Mods.Transoceanic.Core.AutoHosting.Active"),
                new Vector2(Main.screenWidth * 0.5f, Math.Max(30f, Main.screenHeight * 0.03f)), Color.Gold, 2f, 0.5f);
        return true;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 保存退出前中断托管并释放输入控制。
    /// </remarks>
    public override void PreSaveAndQuit() => AutoHosting.InterruptCurrent();
    /// <inheritdoc/>
    /// <remarks>
    /// 退出世界时中断托管并释放输入控制。
    /// </remarks>
    public override void OnWorldUnload() => AutoHosting.InterruptCurrent();

    /// <inheritdoc/>
    /// <remarks>
    /// 在主线程中断托管；专用服务器跳过。
    /// </remarks>
    public override void Unload()
    {
        if (Main.dedServ)
            return;
        if (global::Terraria.Program.IsMainThread)
            AutoHosting.InterruptCurrent();
        else
            Main.RunOnMainThread(AutoHosting.InterruptCurrent).GetAwaiter().GetResult();
        // tML removes this ModSystem's hooks on unload.
    }
}
