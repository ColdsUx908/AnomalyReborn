using CalamityMod.Buffs.Summon;

namespace CalamityAnomalies.GameContents.Base;

public abstract class CABaseSummonBuff<TModProjectile> : BaseSummonBuff where TModProjectile : ModProjectile
{
    protected sealed override int MinionProjectileType => ModContent.ProjectileType<TModProjectile>();

    public CAPlayer AnomalyPlayer => BuffOwner.Anomaly;
}