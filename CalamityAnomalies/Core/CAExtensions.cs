// Developed by ColdsUx

using System.Diagnostics.CodeAnalysis;

namespace CalamityAnomalies.Core;

public static class CAExtensions
{
    extension(Item item)
    {
        public CAGlobalItem Anomaly { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => item?.GetGlobalItem<CAGlobalItem>(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetBehavior(out CASingleItemBehavior itemBehavior, [CallerMemberName] string methodName = null) => CAEntityChangeHelper.ItemBehaviors.TryGetBehavior(item, methodName, out itemBehavior);
    }

    extension(CAItemTooltipModifier modifier)
    {
        public void ApplyCATweakColorToDamage() => modifier.Modify(null, "Damage", l => l.OverrideColor = CASharedData.GetGradientColor(0.25f));
    }

    extension(NPC npc)
    {
        public CAGlobalNPC Anomaly { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => npc?.GetGlobalNPC<CAGlobalNPC>(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetBehavior(out CASingleNPCBehavior npcBehavior, [CallerMemberName] string methodName = null) => CAEntityChangeHelper.NPCBehaviors.TryGetBehavior(npc, methodName, out npcBehavior);

        public bool TryGetBossBar([NotNullWhen(true)] out BetterBossHPUI bar)
        {
            if (BetterBossHealthBar.CurrentBars.TryGetValue(npc.Identifier, out BetterBossHPUI foundBar) && foundBar.Valid)
            {
                bar = foundBar;
                return true;
            }
            bar = null;
            return false;
        }

        public void AddAnomalyHPIndicator(float anomalyLifeRatio, float anomalyUltraLifeRatio, bool isSubPhaseIndicator = false, Func<NPC, bool> condition = null)
        {
            condition ??= _ => true;

            npc.Anomaly.HPThresholdIndicators.Add(new HPThresholdIndicator()
            {
                ValueFunction = (indicator, npc, bar) => npc.Anomaly.IsRunningAnomalyAI && condition(npc) ? CASharedData.AnomalyUltramundane ? anomalyUltraLifeRatio : anomalyLifeRatio : 0f,
                CustomUpdateFunction = (indicator, npc, bar) =>
                {
                    if (!npc.Anomaly.IsRunningAnomalyAI || !condition(npc))
                        return false;

                    if (indicator.GetValue(npc, bar) == 0f)
                        return false;

                    return true;
                },
                IsSubPhaseIndicator = isSubPhaseIndicator
            });
        }
    }

    extension(Player player)
    {
        public CAPlayer Anomaly { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => player?.GetModPlayer<CAPlayer>(); }
    }

    extension(Projectile projectile)
    {
        public CAGlobalProjectile Anomaly { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => projectile?.GetGlobalProjectile<CAGlobalProjectile>(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetBehavior(out CASingleProjectileBehavior projectileBehavior, [CallerMemberName] string methodName = null) => CAEntityChangeHelper.ProjectileBehaviors.TryGetBehavior(projectile, methodName, out projectileBehavior);
    }
}
