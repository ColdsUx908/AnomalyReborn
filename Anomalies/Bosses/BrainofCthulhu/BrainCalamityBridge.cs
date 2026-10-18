namespace Anomalies.Bosses.BrainofCthulhu;

internal static class BrainCalamityBridge
{
    internal static Func<bool> BossRush;
    internal static Func<bool> BrainSpriteChanged;
    internal static Action<Player> ClearAdrenaline;
    internal static Action<Projectile> ProjectileDefenseDamage;
    internal static Action<NPC> NPCDefenseDamage;
    internal static Func<DamageClass, bool> TrueMelee;
    public static bool UseChangedBrainSprite => BrainSpriteChanged?.Invoke() ?? false;
    public static bool BossRushActive => BossRush?.Invoke() ?? false;

    public static void ResetAdrenaline(Player player) => ClearAdrenaline?.Invoke(player);
    public static void EnableDefenseDamage(Projectile projectile) => ProjectileDefenseDamage?.Invoke(projectile);
    public static void EnableDefenseDamage(NPC npc) => NPCDefenseDamage?.Invoke(npc);
    public static bool IsTrueMelee(DamageClass damageClass) => TrueMelee?.Invoke(damageClass) ?? false;
    [ExtendsFromMod("CalamityMod")]
    public sealed class Support : IContentLoader
    {
        public void PostSetupContent()
        {
            CalamityMod.NPCs.CalamityVanillaAIOverrideNPC.ModifyAIOverride += SuppressOriginalAI;
            BrainSpriteChanged = () => CalamityMod.CalamityClientConfig.Instance.BoCSpriteChange;
            BossRush = () => CalamityMod.Events.BossRushEvent.BossRushActive;
            ClearAdrenaline = player => CalamityMod.CalamityUtils.Calamity(player).adrenaline = 0;
            ProjectileDefenseDamage = projectile => CalamityMod.CalamityUtils.Calamity(projectile).DealsDefenseDamage = true;
            NPCDefenseDamage = npc => CalamityMod.CalamityUtils.Calamity(npc).canBreakPlayerDefense = true;
            TrueMelee = damageClass => damageClass.CountsAsClass(ModContent.GetInstance<CalamityMod.TrueMeleeDamageClass>());
        }

        public void OnModUnload()
        {
            CalamityMod.NPCs.CalamityVanillaAIOverrideNPC.ModifyAIOverride -= SuppressOriginalAI;
            BrainSpriteChanged = null;
            BossRush = null;
            ClearAdrenaline = null;
            ProjectileDefenseDamage = null;
            NPCDefenseDamage = null;
            TrueMelee = null;
        }

        private static void SuppressOriginalAI(CalamityMod.NPCs.VanillaNPCAIOverrides.VanillaAIOverrideContext context)
        {
            if (AnomalySharedData.Anomaly && context.NPCType is NPCID.BrainofCthulhu or NPCID.Creeper)
                context.OverrideToApply = null;
        }
    }
}
