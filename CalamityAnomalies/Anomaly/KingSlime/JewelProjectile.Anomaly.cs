// Developed by ColdsUx

using CalamityMod.Projectiles.Boss;

namespace CalamityAnomalies.Anomaly.KingSlime;

public sealed class JewelProjectile_Anomaly : AnomalyProjecileBehavior<JewelProjectile>
{
    public override void SetDefaults()
    {
        Projectile.timeLeft = 450;
    }
}
