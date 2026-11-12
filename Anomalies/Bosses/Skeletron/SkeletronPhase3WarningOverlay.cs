using Anomalies.Visuals.Overlays;

namespace Anomalies.Bosses.Skeletron;

// 预警置于物块之后，避免场景绘制覆盖场内线条；不借用场外雾幕的绘制层。
public sealed class SkeletronPhase3WarningOverlay : PostTilesOverlay
{
    private SkeletronPhase3Arena _Arena;

    public override void Update()
    {
        _Arena = null;
        foreach (Projectile projectile in Projectile.ActiveProjectiles)
        {
            if (projectile.ModProjectile is SkeletronPhase3Arena { IsActivated: true } arena
                && (_Arena is null || arena.Intensity > _Arena.Intensity))
                _Arena = arena;
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!Main.gameMenu && _Arena is not null && _Arena.Projectile.active && _Arena.Intensity > 0f)
            SkeletronPhase3Warnings.Draw(spriteBatch, _Arena);
    }
}
