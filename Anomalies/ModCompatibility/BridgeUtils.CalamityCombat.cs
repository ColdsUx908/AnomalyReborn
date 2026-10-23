namespace Anomalies.ModCompatibility;

/// <summary>可选灾厄战斗接口；未加载灾厄时使用空操作或 false。</summary>
public static partial class BridgeUtils
{
    private static Func<bool> _CalamityBossRush;
    private static Action<Player> _ClearCalamityAdrenaline;
    private static Action<Projectile> _CalamityProjectileDefenseDamage;
    private static Action<NPC> _CalamityNPCDefenseDamage;
    private static Func<DamageClass, bool> _CalamityTrueMelee;

    public static bool CalamityBossRushActive => _CalamityBossRush?.Invoke() ?? false;
    public static void ResetCalamityAdrenaline(Player player) => _ClearCalamityAdrenaline?.Invoke(player);
    public static void EnableCalamityDefenseDamage(Projectile projectile) => _CalamityProjectileDefenseDamage?.Invoke(projectile);
    public static void EnableCalamityDefenseDamage(NPC npc) => _CalamityNPCDefenseDamage?.Invoke(npc);
    public static bool IsCalamityTrueMelee(DamageClass damageClass) => _CalamityTrueMelee?.Invoke(damageClass) ?? false;

    [ExtendsFromMod(CalamityModName)]
    public sealed class CalamityCombatSupport : IContentLoader
    {
        public void PostSetupContent()
        {
            _CalamityBossRush = () => CalamityMod.Events.BossRushEvent.BossRushActive;
            _ClearCalamityAdrenaline = player => CalamityMod.CalamityUtils.Calamity(player).adrenaline = 0;
            _CalamityProjectileDefenseDamage = projectile => CalamityMod.CalamityUtils.Calamity(projectile).DealsDefenseDamage = true;
            _CalamityNPCDefenseDamage = npc => CalamityMod.CalamityUtils.Calamity(npc).canBreakPlayerDefense = true;
            _CalamityTrueMelee = damageClass => damageClass.CountsAsClass(ModContent.GetInstance<CalamityMod.TrueMeleeDamageClass>());
        }

        public void OnModUnload()
        {
            _CalamityBossRush = null;
            _ClearCalamityAdrenaline = null;
            _CalamityProjectileDefenseDamage = null;
            _CalamityNPCDefenseDamage = null;
            _CalamityTrueMelee = null;
        }
    }
}
