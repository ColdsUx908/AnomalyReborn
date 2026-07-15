// Developed by ColdsUx


namespace CalamityAnomalies.Anomaly.QueenBee;

public sealed class HugeStinger : CAModProjectile
{
    public override string LocalizationCategory => "Anomaly.QueenBee";

    public override void SetDefaults()
    {
        Projectile.width = 20;
        Projectile.height = 50;
        Projectile.hostile = true;
        Projectile.timeLeft = 150;
    }

    public override void OnKill(int timeLeft)
    {
    }
}
