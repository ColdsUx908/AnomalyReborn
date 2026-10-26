using Terraria.GameInput;
using Terraria.UI;

namespace Transoceanic.Input.AutoHosting;

/// <summary>Applies owned input and draws the shared hosting status.</summary>
public sealed class AutoHostingSystem : ModSystem
{
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

    public override void UpdateUI(GameTime gameTime) => AutoHosting.CheckCurrentInput();

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

    public override void PreSaveAndQuit() => AutoHosting.InterruptCurrent();
    public override void OnWorldUnload() => AutoHosting.InterruptCurrent();

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
