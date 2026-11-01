namespace Anomalies.Bosses.EaterofWorlds;

public sealed partial class EaterofWorldsHead
{
    private void SpawnPortal(Vector2 center, bool entrance)
    {
        if (!TONetworkEnvironment.NotClient)
            return;
        Projectile.NewProjectileAction(SourceAI, center, Vector2.Zero,
            entrance ? ModContent.ProjectileType<EaterEntryPortal>() : ModContent.ProjectileType<EaterTeleportRift>(), 0, 0f,
            action: portal =>
            {
                portal.ai[0] = NPC.whoAmI + 1;
                if (portal.ModProjectile is EaterTeleportRift rift)
                    rift.RiftLifetime = PortalChargeDuration;
                portal.timeLeft = PortalChargeDuration + 45;
            });
    }
}
