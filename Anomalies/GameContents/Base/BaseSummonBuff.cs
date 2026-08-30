namespace Anomalies.GameContents.Base;

/// <summary>
/// An abstract class that gives the baseline code needed to make a buff dedicated to a minion.
/// </summary>
public abstract class BaseSummonBuff<TModProjectile> : ModBuff where TModProjectile : ModProjectile
{
    /// <summary>
    /// The correspondent minion's ID of this buff.
    /// </summary>
    public int MinionProjectileType => ModContent.ProjectileType<TModProjectile>();

    public abstract ref bool MinionBool { get; }

    /// <summary>
    /// The <see cref="Player"/> that possesses this buff (Read-only).
    /// </summary>
    public Player BuffOwner { get; private set; }

    public AnomalyPlayer AnomalyPlayer => BuffOwner.Anomaly;

    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
        Main.buffNoSave[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        BuffOwner = player;

        if (player.ownedProjectileCounts[MinionProjectileType] > 0)
            MinionBool = true;

        if (!MinionBool)
        {
            player.DelBuff(buffIndex);
            buffIndex--;
        }

        else
            player.buffTime[buffIndex] = 18000;
    }
}
