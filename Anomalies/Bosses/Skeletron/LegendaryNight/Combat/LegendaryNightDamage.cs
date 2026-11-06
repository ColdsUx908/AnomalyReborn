namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>传说之夜本地玩家受击结算入口及其共享参数。</summary>
public static class LegendaryNightDamage
{
    // 只结算本机玩家，Hurt 自行发送伤害包；保留已有免疫时间，避免修改其他攻击的免疫行为。
    public static void HurtLocalPlayer(Projectile source, int damage, bool suppressDamage, float knockback = 0f,
        bool ignoreAllImmunity = false)
    {
        if (Main.dedServ)
            return;
        Player player = Main.LocalPlayer;
        if (!player.active || player.dead || player.ghost)
            return;
        bool previousImmune = player.immune;
        int previousImmuneTime = player.immuneTime;
        int[] previousCooldowns = ignoreAllImmunity ? (int[])player.hurtCooldowns.Clone() : null;
        try
        {
            player.immune = false;
            if (ignoreAllImmunity)
            {
                player.immuneTime = 0;
                Array.Clear(player.hurtCooldowns);
            }
            if (player.Hurt(PlayerDeathReason.ByProjectile(source.owner, source.whoAmI), damage, 0,
                cooldownCounter: ImmunityCooldownID.General, dodgeable: false, knockback: knockback) > 0 && suppressDamage)
                player.AddBuff(ModContent.BuffType<AstralSuppression>(), 15 * 60);
        }
        finally
        {
            player.immune |= previousImmune;
            player.immuneTime = Math.Max(player.immuneTime, previousImmuneTime);
            if (previousCooldowns is not null)
            {
                for (int i = 0; i < previousCooldowns.Length; i++)
                    player.hurtCooldowns[i] = Math.Max(player.hurtCooldowns[i], previousCooldowns[i]);
            }
        }
    }
}