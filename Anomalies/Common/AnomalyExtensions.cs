// Developed by ColdsUx

using System.Diagnostics.CodeAnalysis;

namespace Anomalies.Common;

public static class AnomalyExtensions
{
    extension(Item item)
    {
        public AnomalyGlobalItem Anomaly { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => item?.GetGlobalItem<AnomalyGlobalItem>(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetBehavior(out AnomalySingleItemBehavior itemBehavior, [CallerMemberName] string methodName = null) => AnomalyEntityChangeHelper.ItemBehaviors.TryGetBehavior(item, methodName, out itemBehavior);
    }

    extension(AnomalyItemTooltipModifier modifier)
    {
        public void ApplyAnomalyTweakColorToDamage() => modifier.Modify(null, "Damage", l => l.OverrideColor = AnomalySharedData.GetGradientColor(0.25f));
    }

    extension(NPC npc)
    {
        public AnomalyGlobalNPC Anomaly { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => npc?.GetGlobalNPC<AnomalyGlobalNPC>(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetBehavior(out AnomalySingleNPCBehavior npcBehavior, [CallerMemberName] string methodName = null) => AnomalyEntityChangeHelper.NPCBehaviors.TryGetBehavior(npc, methodName, out npcBehavior);

        public bool TryGetBossBar([NotNullWhen(true)] out BossHealthBar bar)
        {
            if (BossHealthBarStyle.CurrentBars.TryGetValue(npc.Identifier, out BossHealthBar foundBar) && foundBar.Valid)
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
                ValueFunction = (indicator, npc, bar) => npc.Anomaly.IsRunningAnomalyAI && condition(npc) ? AnomalySharedData.AnomalyUltramundane ? anomalyUltraLifeRatio : anomalyLifeRatio : 0f,
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
        public AnomalyPlayer Anomaly { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => player?.GetModPlayer<AnomalyPlayer>(); }
    }

    extension(Projectile projectile)
    {
        public AnomalyGlobalProjectile Anomaly { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => projectile?.GetGlobalProjectile<AnomalyGlobalProjectile>(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetBehavior(out AnomalySingleProjectileBehavior projectileBehavior, [CallerMemberName] string methodName = null) => AnomalyEntityChangeHelper.ProjectileBehaviors.TryGetBehavior(projectile, methodName, out projectileBehavior);
    }
}
