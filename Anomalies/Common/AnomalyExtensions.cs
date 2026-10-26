using System.Diagnostics.CodeAnalysis;
using Anomalies.Visuals.BossBar;
using Terraria.Graphics.Shaders;

namespace Anomalies.Common;

public static class AnomalyExtensions
{
    extension(Item item)
    {
        public AnomalyGlobalItem Anomaly { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => item?.GetGlobalItem<AnomalyGlobalItem>(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetBehavior(out AnomalyItemBehavior itemBehavior, [CallerMemberName] string methodName = null) => AnomalyEntityChangeHelper.ItemBehaviors.TryGetBehavior(item, methodName, out itemBehavior);
    }

    extension(AnomalyItemTooltipModifier modifier)
    {
        public void ApplyAnomalyTweakColorToDamage() => modifier.Modify(null, "Damage", l => l.OverrideColor = AnomalySharedData.GetGradientColor(0.25f));
    }

    extension(MiscShaderData data)
    {
        public void ApplyIfUsingShader(DrawData? drawData = null)
        {
            if (AnomalySharedData.ShouldUseShaders)
                data.Apply(drawData);
        }
    }

    extension(NPC npc)
    {
        public AnomalyGlobalNPC Anomaly { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => npc?.GetGlobalNPC<AnomalyGlobalNPC>(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetBehavior(out AnomalyNPCBehavior npcBehavior, [CallerMemberName] string methodName = null) => AnomalyEntityChangeHelper.NPCBehaviors.TryGetBehavior(npc, methodName, out npcBehavior);

        public bool TryGetBossBar([NotNullWhen(true)] out BossHealthBar bar)
        {
            if (BossBarHandler.CurrentBars.TryGetValue(npc.Identifier, out BossHealthBar foundBar) && foundBar.Valid)
            {
                bar = foundBar;
                return true;
            }
            bar = null;
            return false;
        }

        /// <summary>添加本体血量阈值，在额外血量清零后的本体区段标记对应百分比。</summary>
        public void AddAnomalyHPIndicator(float anomalyLifeRatio, float anomalyUltraLifeRatio, bool isSubPhaseIndicator = false, Func<NPC, bool> condition = null, HPThresholdLifeSource lifeSource = HPThresholdLifeSource.NPC)
        {
            condition ??= _ => true;

            npc.Anomaly.BossBar_HPThresholdIndicators.Add(new HPThresholdIndicator()
            {
                ValueFunction = (indicator, npc, bar) => npc.Anomaly.AnomalyAI_IsRunning && condition(npc)
                    ? HPThresholdMath.GetPosition(lifeSource, AnomalySharedData.AnomalyUltramundane ? anomalyUltraLifeRatio : anomalyLifeRatio, npc.lifeMax, bar.InitialMaxLife)
                    : 0f,
                ProgressFunction = (indicator, npc, bar) => HPThresholdMath.GetProgress(lifeSource, npc.life, npc.lifeMax, bar.ExtraNPCLife, bar.InitialMaxLife),
                CustomUpdateFunction = (indicator, npc, bar) =>
                {
                    if (!npc.Anomaly.AnomalyAI_IsRunning || !condition(npc))
                        return false;

                    if (indicator.GetValue(npc, bar) == 0f)
                        return false;

                    return true;
                },
                IsSubPhaseIndicator = isSubPhaseIndicator,

                ParticleSet = new EnchantedParticleSet(-1, 0f,
                    () => isSubPhaseIndicator ? TOColors.CelestialColor : AnomalySharedData.AnomalyUltramundaneColor,
                    () => Color.White, 0.1f, 0f)
            });
        }

        /// <summary>添加额外血量阈值；0% 标记额外血量全部清零、本体仍满血的位置。</summary>
        public void AddAnomalyExtraHPIndicator(float anomalyLifeRatio, float anomalyUltraLifeRatio, bool isSubPhaseIndicator = false, Func<NPC, bool> condition = null)
            => npc.AddAnomalyHPIndicator(anomalyLifeRatio, anomalyUltraLifeRatio, isSubPhaseIndicator, condition, HPThresholdLifeSource.Extra);
    }

    extension(Player player)
    {
        public AnomalyPlayer Anomaly { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => player?.GetModPlayer<AnomalyPlayer>(); }
    }

    extension(Projectile projectile)
    {
        public AnomalyGlobalProjectile Anomaly { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => projectile?.GetGlobalProjectile<AnomalyGlobalProjectile>(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetBehavior(out AnomalyProjectileBehavior projectileBehavior, [CallerMemberName] string methodName = null) => AnomalyEntityChangeHelper.ProjectileBehaviors.TryGetBehavior(projectile, methodName, out projectileBehavior);
    }
}

