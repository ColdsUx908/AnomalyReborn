namespace CalamityAnomalies.GlobalInstances.Single;

public sealed class CAProjectileMisc : CAGlobalProjectileBehavior
{
    public override decimal Priority => 500m;

    public override void SetDefaults(Projectile projectile)
    {
        CAGlobalProjectile anomalyProjectile = projectile.Anomaly;

        anomalyProjectile.ShouldRunAnomalyAI = true;
    }
}
