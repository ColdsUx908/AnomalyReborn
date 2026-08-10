// Developed by ColdsUx

namespace Anomalies.Common.SingleBehaviors;

public sealed class AnomalyProjectileMisc : AnomalyGlobalProjectileBehavior
{
    public override decimal Priority => 500m;

    public override void SetDefaults(Projectile projectile)
    {
        AnomalyGlobalProjectile anomalyProjectile = projectile.Anomaly;

        anomalyProjectile.ShouldRunAnomalyAI = true;
    }
}
