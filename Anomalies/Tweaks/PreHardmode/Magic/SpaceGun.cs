namespace Anomalies.Tweaks.PreHardmode.Magic;

public sealed class SpaceGun : AnomalyItemBehavior<SpaceGun>
{
    public override int ApplyingType => ItemID.SpaceGun;

    public override void SetDefaultsFinal()
    {
        Item.shootSpeed = 40f;
    }
}

public sealed class GreenLaser : AnomalyProjectileBehavior<GreenLaser>
{
    public override int ApplyingType => ProjectileID.GreenLaser;

    public override void SetDefaults()
    {
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }
}