using CalamityMod.Events;
using CalamityMod.NPCs;
using CalamityMod.NPCs.Providence;
using Transoceanic.Framework.Helpers.AbstractionHandlers;
using Transoceanic.Framework.RuntimeEditing;

namespace CalamityAnomalies.Common.SingleBehaviors;

public sealed class CANPCDR : CAGlobalNPCBehavior, IContentLoader
{
    public delegate void Orig_ApplyDR(CalamityGlobalNPC self, NPC npc, ref NPC.HitModifiers modifiers);

    /// <summary>
    /// 禁用灾厄的DR机制。
    /// </summary>
    public static void Detour_ApplyDR(Orig_ApplyDR orig, CalamityGlobalNPC self, NPC npc, ref NPC.HitModifiers modifiers) { }

    public static float GetBaseDR(NPC npc)
    {
        CalamityGlobalNPC calamityNPC = npc.Calamity;
        return calamityNPC.unbreakableDR ? calamityNPC.DR : new CalamityGlobalNPC_Publicizer(calamityNPC).ApplyDRReduction(npc, calamityNPC.DR);
    }

    public static float GetTimedDR(NPC npc, float baseDR)
    {
        float timedDR = 0f;
        CalamityGlobalNPC calamityNPC = npc.Calamity;
        int killTime = calamityNPC.KillTime;
        int aiTimer = calamityNPC.killTimeTimer;

        bool isNightProvidence = npc.ModNPC is Providence providence && providence.hasBeenGivenFullPower;
        bool isGFBDayEmpressofLight = CASharedData.Anomaly && Main.zenithWorld && npc.type == NPCID.HallowBoss && npc.Anomaly.IsRunningAnomalyAI;

        if (killTime > 0 && aiTimer < killTime && !BossRushEvent.BossRushActive && (isNightProvidence || isGFBDayEmpressofLight))
        {
            const float tdrFactor = 10f;
            float extraDRLimit = (1f - baseDR) * tdrFactor / 2f;
            float lifeCompletion = npc.LostLifeRatio;
            float timeCompletion = (float)aiTimer / killTime;
            float timedDRStrength = lifeCompletion - timeCompletion;
            if (timedDRStrength > 0f)
                timedDR = extraDRLimit * timedDRStrength / (1 + timedDRStrength);
        }

        return timedDR;
    }

    public override decimal Priority => 100m;

    public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
    {
        float baseDR = GetBaseDR(npc);

        StatModifier baseDRModifier = new();
        StatModifier standardDRModifier = new();
        StatModifier timedDRModifier = new();

        foreach (CAGlobalItemBehavior behavior in GlobalItemBehaviorHandler.BehaviorSet.Enumerate<CAGlobalItemBehavior>(nameof(CAGlobalItemBehavior.ModifyHitNPC_DR)))
            behavior.ModifyHitNPC_DR(item, npc, player, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);
        if (item.TryGetBehavior(out CASingleItemBehavior itemBehavior, nameof(CASingleItemBehavior.ModifyHitNPC_DR)))
            itemBehavior.ModifyHitNPC_DR(npc, player, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);
        if (item.ModItem is ICAModItem caItem)
            caItem.ModifyHitNPC_DR(npc, player, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);

        baseDR = baseDRModifier.ApplyTo(baseDR);
        float standardDR = standardDRModifier.ApplyTo(baseDR);
        float timedDR = timedDRModifier.ApplyTo(GetTimedDR(npc, baseDR));

        modifiers.FinalDamage *= Math.Clamp(1f - standardDR - timedDR - npc.Anomaly.ExtraDR, 0f, 1f);
    }

    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
    {
        float baseDR = GetBaseDR(npc);

        StatModifier baseDRModifier = new();
        StatModifier standardDRModifier = new();
        StatModifier timedDRModifier = new();

        foreach (CAGlobalProjectileBehavior behavior in GlobalProjectileBehaviorHandler.BehaviorSet.Enumerate<CAGlobalProjectileBehavior>(nameof(CAGlobalProjectileBehavior.ModifyHitNPC_DR)))
            behavior.ModifyHitNPC_DR(projectile, npc, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);
        if (projectile.TryGetBehavior(out CASingleProjectileBehavior projectileBehavior, nameof(CASingleProjectileBehavior.ModifyHitNPC_DR)))
            projectileBehavior.ModifyHitNPC_DR(npc, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);
        if (projectile.ModProjectile is ICAModProjectile caProjectile)
            caProjectile.ModifyHitNPC_DR(npc, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);

        baseDR = baseDRModifier.ApplyTo(baseDR);
        float standardDR = standardDRModifier.ApplyTo(baseDR);
        float timedDR = timedDRModifier.ApplyTo(GetTimedDR(npc, baseDR));

        modifiers.FinalDamage *= Math.Clamp(1f - standardDR - timedDR - npc.Anomaly.ExtraDR, 0f, 1f);
    }

    void IContentLoader.PostSetupContent()
    {
        Type type = typeof(CalamityGlobalNPC);
        TODetourHandler.Modify(type, "ApplyDR", Detour_ApplyDR);
    }
}
