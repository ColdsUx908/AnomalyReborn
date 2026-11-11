using Anomalies.Visuals.Overlays;
using Terraria.Graphics.Effects;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class LegendaryNightOverlay : CustomOverlay
{
    /// <summary>主贴图资源路径。</summary>
    private const string TexturePath = "Anomalies/Bosses/Skeletron/LegendaryNight/Arena/";

    /// <summary>实体化星体。</summary>
    [LoadTexture(TexturePath + "MaterializedStar")]
    private static Asset<Texture2D> _MaterializedStar;

    public LegendaryNightArena Arena;
    public LegendaryNightPhase2Arena Phase2Arena;

    public override void Update()
    {
        Arena = null;
        Phase2Arena = null;
        foreach (Projectile projectile in Projectile.ActiveProjectiles)
        {
            if (projectile.ModProjectile is LegendaryNightArena arena && (Arena is null || arena.Intensity > Arena.Intensity))
                Arena = arena;
            if (projectile.ModProjectile is LegendaryNightPhase2Arena phase2Arena
                && (Phase2Arena is null || phase2Arena.Intensity > Phase2Arena.Intensity))
                Phase2Arena = phase2Arena;
        }
    }

    public override void Draw(SpriteBatch spriteBatch, RenderLayers layer, bool beginSpriteBatch)
    {
        if (Main.gameMenu || Main.dedServ || layer != RenderLayers.All)
            return;

        bool phase2 = Phase2Arena is not null && Phase2Arena.Projectile.active;
        float intensity = phase2 ? Phase2Arena.Intensity : Arena?.Intensity ?? 0f;
        if (intensity <= 0f || !phase2 && (Arena is null || !Arena.Projectile.active))
        {
            DrawImpactFlame(spriteBatch, phase2 ? Phase2Arena : null);
            return;
        }

        if (phase2)
            SkeletronArenaDrawing.DrawMist(spriteBatch, intensity, null, Phase2Arena.Projectile.Center, Phase2Arena.OuterRadius);
        else
            SkeletronArenaDrawing.DrawMist(spriteBatch, intensity, Arena.VisualPolygon);

        if (phase2)
        {
            // Composite the gold edge above the original outside mist, below warnings and boundary lines.
            DrawArenaEdge(spriteBatch);
            // 先铺发光底色，让攻击火焰的流纹和边缘在交叠时保持清晰。
            DrawMaterializedFill(spriteBatch);
            DrawImpactFlame(spriteBatch, Phase2Arena);
            DrawMagicCircle(spriteBatch);
            LegendaryNightAttackDrawing.Draw(spriteBatch, Phase2Arena.MasterBehavior);
        }
        else
        {
            DrawImpactFlame(spriteBatch, null);
            SkeletronArenaDrawing.DrawBoundary(spriteBatch, Arena.VisualPolygon, intensity);
        }
    }

}