using Terraria.Graphics.Effects;

namespace Anomalies.Visuals.Overlays;

public abstract class CustomOverlay
{
    public virtual void Update() { }
    public abstract void Draw(SpriteBatch spriteBatch, RenderLayers layer, bool beginSpriteBatch);
}

public abstract class PostTilesOverlay
{
    public virtual void Update() { }
    public abstract void Draw(SpriteBatch spriteBatch);
}

public sealed class CustomOverlayManager : ModSystem, IContentLoader
{
    private static List<CustomOverlay> _Overlays = [];
    private static List<PostTilesOverlay> _PostTilesOverlays = [];

    public override void PreUpdateEntities()
    {
        foreach (CustomOverlay overlay in _Overlays)
            overlay.Update();
        foreach (PostTilesOverlay overlay in _PostTilesOverlays)
            overlay.Update();
    }

    private static void On_OverlayManager_Draw(On_OverlayManager.orig_Draw orig, OverlayManager self, SpriteBatch spriteBatch, RenderLayers layer, bool beginSpriteBatch)
    {
        orig(self, spriteBatch, layer, beginSpriteBatch);
        foreach (CustomOverlay overlay in _Overlays)
            overlay.Draw(spriteBatch, layer, beginSpriteBatch);
    }

    public override void PostDrawTiles()
    {
        foreach (PostTilesOverlay overlay in _PostTilesOverlays)
            overlay.Draw(Main.spriteBatch);
    }

    void IContentLoader.PostSetupContent()
    {
        _Overlays.AddRange(TOReflectionUtils.GetTypeInstancesDerivedFrom<CustomOverlay>(true));
        _PostTilesOverlays.AddRange(TOReflectionUtils.GetTypeInstancesDerivedFrom<PostTilesOverlay>(true));
        On_OverlayManager.Draw += On_OverlayManager_Draw;
    }

    void IContentLoader.OnModUnload()
    {
        _Overlays.Clear();
        _PostTilesOverlays.Clear();
    }
}
