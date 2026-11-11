using Anomalies.Visuals.Overlays;
using Terraria.Graphics.Effects;

namespace Anomalies.Bosses.Skeletron;

public sealed class SkeletronPhase3Arena : SkeletronBoneArena
{
    public override string LocalizationCategory => "Bosses.Skeletron";

    private SkeletronHead MasterBehavior => Master is NPC { active: true, life: > 0, type: NPCID.SkeletronHead } head
        && SkeletronHead.GetInstance(head) is SkeletronHead { ShouldProcess: true } behavior
        && behavior.CurrentPhase is SkeletronHead.Phase.PhaseChange_2To3 or SkeletronHead.Phase.Phase3
        && behavior.CurrentBehavior != SkeletronHead.Behavior.Despawn ? behavior : null;

    public override Player Target => MasterBehavior?.Target;

    public override void AI()
    {
        if (MasterBehavior is null)
        {
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }

        UpdateBoundary();
    }
}

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