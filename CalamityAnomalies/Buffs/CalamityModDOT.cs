// Developed by ColdsUx

using CalamityMod.World;

namespace CalamityAnomalies.Buffs;

public abstract class CalamityModDOT : ModDOT
{
    public virtual float GetDamageCalamity(Player player) => 0f;

    public sealed override float GetDamage(Player player) => GetDamageCalamity(player) * (CalamityWorld.death ? 1.25f : 1f) * (player.CalamityPlayer.reaverDefense ? 0.8f : 1f);
}
