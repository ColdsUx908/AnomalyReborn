// Developed by ColdsUx

namespace Anomalies.Bosses.QueenBee;

public sealed class QueenBeeStinger : AnomalyProjectileBehavior<QueenBeeStinger>
{
    public override int ApplyingType => ProjectileID.QueenBeeStinger;

    public override void SetDefaults()
    {
        Projectile.ignoreWater = true;
    }
}
