namespace Anomalies.Bosses.QueenBee;

public sealed class QueenBeeStinger : AnomalyProjectileBehavior<QueenBeeStinger>
{
    public override int ApplyingType => ProjectileID.QueenBeeStinger;

    public override bool AllowCalamityLogic(CalamityLogicType_ProjectileBehavior type) => type switch
    {
        CalamityLogicType_ProjectileBehavior.PreAI => false,
        _ => true,
    };

    public override void SetDefaults()
    {
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        Projectile.ai[0]--; //避免原版的重力机制生效
    }
}

