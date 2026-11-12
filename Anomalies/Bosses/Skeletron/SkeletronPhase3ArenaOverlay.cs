using Terraria.Graphics.Effects;
using Anomalies.Visuals.Overlays;

namespace Anomalies.Bosses.Skeletron;

public sealed class SkeletronPhase3ArenaOverlay : CustomOverlay
{
    private SkeletronPhase3Arena _Arena;

    public override void Update()
    {
        _Arena = null;
        foreach (Projectile projectile in Projectile.ActiveProjectiles)
        {
            if (projectile.ModProjectile is SkeletronPhase3Arena arena
                && (_Arena is null || arena.Intensity > _Arena.Intensity))
                _Arena = arena;
        }
    }

    public override void Draw(SpriteBatch spriteBatch, RenderLayers layer, bool beginSpriteBatch)
    {
        if (Main.gameMenu || Main.dedServ || layer != RenderLayers.All
            || _Arena is null || !_Arena.Projectile.active || _Arena.Intensity <= 0f)
            return;

        SkeletronArenaDrawing.DrawMist(spriteBatch, _Arena.Intensity, _Arena.VisualPolygon);
        SkeletronArenaDrawing.DrawBoundary(spriteBatch, _Arena.VisualPolygon, _Arena.Intensity);
    }
}
