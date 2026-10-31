namespace Anomalies.Common.SingleBehaviors;

public sealed class AnomalyNPCDR : AnomalyGlobalNPCBehavior
{
    public override decimal Priority => 100m;

    public override void PostAI(NPC npc) => npc.Anomaly.DamageReduction_DynamicHandler?.Update(npc);

    public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
    {
        float baseDR = GetBaseDR(npc);

        StatModifier baseDRModifier = new();
        StatModifier standardDRModifier = new();
        StatModifier timedDRModifier = new();

        foreach (AnomalyGlobalItemBehavior behavior in global::Transoceanic.Behaviors.BehaviorQueries.EnumerateItem<AnomalyGlobalItemBehavior>(nameof(AnomalyGlobalItemBehavior.ModifyHitNPC_DR)))
            behavior.ModifyHitNPC_DR(item, npc, player, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);
        if (item.TryGetBehavior(out AnomalyItemBehavior itemBehavior, nameof(AnomalyItemBehavior.ModifyHitNPC_DR)))
        {
            using EntityBehavior<Item>.EntityLock entityLock = itemBehavior.LockEntity();
            itemBehavior.ModifyHitNPC_DR(npc, player, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);
        }
        if (item.ModItem is IAnomalyModItem caItem)
            caItem.ModifyHitNPC_DR(npc, player, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);

        baseDR = baseDRModifier.ApplyTo(baseDR);
        float standardDR = standardDRModifier.ApplyTo(baseDR);
        float timedDR = timedDRModifier.ApplyTo(GetTimedDR(npc, baseDR) + (npc.Anomaly.DamageReduction_DynamicHandler?.GetCurrentDDR(npc) ?? 0f));

        modifiers.FinalDamage *= Math.Clamp(1f - standardDR - timedDR - npc.Anomaly.ExtraDR, 0f, 1f);
    }

    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
    {
        float baseDR = GetBaseDR(npc);

        StatModifier baseDRModifier = new();
        StatModifier standardDRModifier = new();
        StatModifier timedDRModifier = new();

        foreach (AnomalyGlobalProjectileBehavior behavior in global::Transoceanic.Behaviors.BehaviorQueries.EnumerateProjectile<AnomalyGlobalProjectileBehavior>(nameof(AnomalyGlobalProjectileBehavior.ModifyHitNPC_DR)))
            behavior.ModifyHitNPC_DR(projectile, npc, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);
        if (projectile.TryGetBehavior(out AnomalyProjectileBehavior projectileBehavior, nameof(AnomalyProjectileBehavior.ModifyHitNPC_DR)))
        {
            using EntityBehavior<Projectile>.EntityLock entityLock = projectileBehavior.LockEntity();
            projectileBehavior.ModifyHitNPC_DR(npc, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);
        }
        if (projectile.ModProjectile is IAnomalyModProjectile anomalyProjectile)
            anomalyProjectile.ModifyHitNPC_DR(npc, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);

        baseDR = baseDRModifier.ApplyTo(baseDR);
        float standardDR = standardDRModifier.ApplyTo(baseDR);
        float timedDR = timedDRModifier.ApplyTo(GetTimedDR(npc, baseDR) + (npc.Anomaly.DamageReduction_DynamicHandler?.GetCurrentDDR(npc) ?? 0f));

        modifiers.FinalDamage *= Math.Clamp(1f - standardDR - timedDR - npc.Anomaly.ExtraDR, 0f, 1f);
    }

    public static float GetBaseDR(NPC npc) => npc.Anomaly.DR;

    public static float GetTimedDR(NPC npc, float baseDR)
    {
        float timedDR = 0f;
        return timedDR;
    }
}

