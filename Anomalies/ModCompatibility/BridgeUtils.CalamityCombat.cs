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
    public sealed class CalamityCombatSupport : ILoadable
    {
        /// <summary>登记延迟执行的灾厄战斗委托，不在加载时访问其内容实例。</summary>
        /// <param name="mod">由 tML 提供的所属模组。</param>
        void ILoadable.Load(Mod mod)
        {
            TOCleanup.Register(ClearReferences);
            _CalamityBossRush = () => CalamityMod.Events.BossRushEvent.BossRushActive;
            _ClearCalamityAdrenaline = player => CalamityMod.CalamityUtils.Calamity(player).adrenaline = 0;
            _CalamityProjectileDefenseDamage = projectile => CalamityMod.CalamityUtils.Calamity(projectile).DealsDefenseDamage = true;
            _CalamityNPCDefenseDamage = npc => CalamityMod.CalamityUtils.Calamity(npc).canBreakPlayerDefense = true;
            _CalamityTrueMelee = damageClass => damageClass.CountsAsClass(ModContent.GetInstance<CalamityMod.TrueMeleeDamageClass>());
        }

        // 自有委托由 TOCleanup 清理，避免两个卸载路径重复处理。
        void ILoadable.Unload() { }

        private static void ClearReferences()
        {
            _CalamityBossRush = null;
            _ClearCalamityAdrenaline = null;
            _CalamityProjectileDefenseDamage = null;
            _CalamityNPCDefenseDamage = null;
            _CalamityTrueMelee = null;
        }
    }
}
