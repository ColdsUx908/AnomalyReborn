// Developed by ColdsUx

using Transoceanic.Framework.RuntimeEditing;

namespace Anomalies.Common.SingleBehaviors;

public sealed class AnomalyNPCDR : AnomalyGlobalNPCBehavior, IContentLoader
{
    /*
    public delegate void Orig_ApplyDR(CalamityGlobalNPC self, NPC npc, ref NPC.HitModifiers modifiers);

    /// <summary>
    /// 禁用灾厄的DR机制。
    /// </summary>
    public static void Detour_ApplyDR(Orig_ApplyDR orig, CalamityGlobalNPC self, NPC npc, ref NPC.HitModifiers modifiers) { }
    */

    public override decimal Priority => 100m;

    public override void PostAI(NPC npc) => npc.Anomaly.DynamicDRHandler?.Update(npc);

    public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
    {
        float baseDR = GetBaseDR(npc);

        StatModifier baseDRModifier = new();
        StatModifier standardDRModifier = new();
        StatModifier timedDRModifier = new();

        foreach (AnomalyGlobalItemBehavior behavior in GlobalItemBehaviorHandler.BehaviorSet.Enumerate<AnomalyGlobalItemBehavior>(nameof(AnomalyGlobalItemBehavior.ModifyHitNPC_DR)))
            behavior.ModifyHitNPC_DR(item, npc, player, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);
        if (item.TryGetBehavior(out AnomalySingleItemBehavior itemBehavior, nameof(AnomalySingleItemBehavior.ModifyHitNPC_DR)))
            itemBehavior.ModifyHitNPC_DR(npc, player, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);
        if (item.ModItem is ICAModItem caItem)
            caItem.ModifyHitNPC_DR(npc, player, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);

        baseDR = baseDRModifier.ApplyTo(baseDR);
        float standardDR = standardDRModifier.ApplyTo(baseDR);
        float timedDR = timedDRModifier.ApplyTo(GetTimedDR(npc, baseDR) + (npc.Anomaly.DynamicDRHandler?.GetCurrentDDR(npc) ?? 0f));

        modifiers.FinalDamage *= Math.Clamp(1f - standardDR - timedDR - npc.Anomaly.ExtraDR, 0f, 1f);
    }

    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
    {
        float baseDR = GetBaseDR(npc);

        StatModifier baseDRModifier = new();
        StatModifier standardDRModifier = new();
        StatModifier timedDRModifier = new();

        foreach (AnomalyGlobalProjectileBehavior behavior in GlobalProjectileBehaviorHandler.BehaviorSet.Enumerate<AnomalyGlobalProjectileBehavior>(nameof(AnomalyGlobalProjectileBehavior.ModifyHitNPC_DR)))
            behavior.ModifyHitNPC_DR(projectile, npc, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);
        if (projectile.TryGetBehavior(out AnomalySingleProjectileBehavior projectileBehavior, nameof(AnomalySingleProjectileBehavior.ModifyHitNPC_DR)))
            projectileBehavior.ModifyHitNPC_DR(npc, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);
        if (projectile.ModProjectile is ICAModProjectile caProjectile)
            caProjectile.ModifyHitNPC_DR(npc, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);

        baseDR = baseDRModifier.ApplyTo(baseDR);
        float standardDR = standardDRModifier.ApplyTo(baseDR);
        float timedDR = timedDRModifier.ApplyTo(GetTimedDR(npc, baseDR) + (npc.Anomaly.DynamicDRHandler?.GetCurrentDDR(npc) ?? 0f));

        modifiers.FinalDamage *= Math.Clamp(1f - standardDR - timedDR - npc.Anomaly.ExtraDR, 0f, 1f);
    }

    void IContentLoader.PostSetupContent()
    {
        /*
        Type type = typeof(CalamityGlobalNPC);
        TODetourHandler.Modify(type, "ApplyDR", Detour_ApplyDR);
        */
    }

    public static float GetBaseDR(NPC npc)
    {
        /*
        CalamityGlobalNPC calamityNPC = npc.CalamityNPC;
        return calamityNPC.unbreakableDR ? calamityNPC.DR : new CalamityGlobalNPC_Publicizer(calamityNPC).ApplyDRReduction(npc, calamityNPC.DR);
        */
        return npc.Anomaly.DR;
    }

    public static float GetTimedDR(NPC npc, float baseDR)
    {
        float timedDR = 0f;
        /*
        CalamityGlobalNPC calamityNPC = npc.CalamityNPC;
        int killTime = calamityNPC.KillTime;
        int aiTimer = calamityNPC.killTimeTimer;

        bool isNightProvidence = npc.ModNPC is Providence providence && providence.hasBeenGivenFullPower;
        bool isGFBDayEmpressofLight = AnomalySharedData.Anomaly && Main.zenithWorld && npc.type == NPCID.HallowBoss && npc.Anomaly.IsRunningAnomalyAI;

        if (killTime > 0 && aiTimer < killTime && !BossRushEvent_Bridge.BossRushActive && (isNightProvidence || isGFBDayEmpressofLight))
        {
            const float tdrFactor = 10f;
            float extraDRLimit = (1f - baseDR) * tdrFactor / 2f;
            float lifeCompletion = npc.LostLifeRatio;
            float timeCompletion = (float)aiTimer / killTime;
            float tdrIntensity = lifeCompletion - timeCompletion;
            if (tdrIntensity > 0f)
                timedDR = extraDRLimit * tdrIntensity / (1 + tdrIntensity);
        }
        */

        return timedDR;
    }
}
