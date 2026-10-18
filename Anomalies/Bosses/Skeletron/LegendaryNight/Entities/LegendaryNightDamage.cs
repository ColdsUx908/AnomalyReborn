using Anomalies.GameContents.Buffs;

namespace Anomalies.Bosses.Skeletron.LegendaryNight.Entities;

public static class LegendaryNightDamage
{
    // 只结算本机玩家，Hurt 自行发送伤害包；保留已有免疫时间，避免修改其他攻击的免疫行为。
    public static void HurtLocalPlayer(Projectile source, int damage, bool suppressDamage, float knockback = 0f)
    {
        if (Main.dedServ)
            return;
        Player player = Main.LocalPlayer;
        if (!player.active || player.dead || player.ghost)
            return;
        bool previousImmune = player.immune;
        int previousImmuneTime = player.immuneTime;
        try
        {
            player.immune = false;
            if (player.Hurt(PlayerDeathReason.ByProjectile(source.owner, source.whoAmI), damage, 0,
                cooldownCounter: ImmunityCooldownID.General, dodgeable: false, knockback: knockback) > 0 && suppressDamage)
                player.AddBuff(ModContent.BuffType<StarPowerSuppression>(), 15 * 60);
        }
        finally
        {
            player.immune |= previousImmune;
            player.immuneTime = Math.Max(player.immuneTime, previousImmuneTime);
        }
    }
}
