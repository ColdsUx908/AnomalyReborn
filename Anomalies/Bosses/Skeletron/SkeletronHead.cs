namespace Anomalies.Bosses.Skeletron;

public sealed partial class SkeletronHead : AnomalyNPCBehavior<SkeletronHead>
{
    protected override int AI32Capacity => 10;
    protected override int AI64Capacity => 3;

    public override int ApplyingType => NPCID.SkeletronHead;

    public override bool ShouldProcess => base.ShouldProcess && AnomalyBossChangeToggler.Skeletron && AnomalyNPC.CustomAIType != 1;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        _ => true,
    };

    public override void SetDefaults()
    {
        NPC.AddAnomalyHPIndicator(FinalLifeRatio, FinalLifeRatio,
            condition: npc => npc.Anomaly.CustomAIType != 1);
        NPC.AddAnomalyHPIndicator(Phase3LifeRatioAnomaly, Phase3LifeRatioUltra,
            condition: npc => npc.Anomaly.CustomAIType != 1);
        NPC.AddAnomalyHPIndicator(Phase2_2LifeRatioAnomaly, Phase2_2LifeRatioUltra, true,
            npc => npc.Anomaly.CustomAIType != 1);
    }

    public override void SetDefaultsFinal()
    {
        NPC.lifeMax = CalamityEnabled ? 15000 : 11250;
        BridgeUtils.ApplyCalamityHealthBoost(NPC);
    }

    public override void Load() => BossDeathEffectRegistry.Register(NPCID.SkeletronHead,
        npc => GetInstance(npc) is { ShouldProcess: true, DeathLocked: true });

    public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
    {
        if (DeathLocked)
            modifiers.SetMaxDamage(Math.Max(1, NPC.life - DeathLockLifeFloor));
        else if (Ultra && CurrentBehavior != Behavior.Despawn)
        {
            if (Phase3)
                modifiers.SetMaxDamage(Math.Max(1, NPC.life - FinalLifeFloor));
        }
    }

    public override bool CheckDead()
    {
        if (!DeathLocked)
            return true;

        NPC.life = DeathLockLifeFloor;
        NPC.active = true;
        NPC.netUpdate = true;
        return false;
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) =>
        AnomalyUtils.ApplyAnomalyBossDifficultyAndPlayerScaling(NPC, bossAdjustment);

    internal static bool IsLivingHand(NPC head, NPC hand) =>
        hand.active && hand.life > 0 && hand.type == NPCID.SkeletronHand && hand.ai[1] == head.whoAmI;

    internal int CountLivingHands()
    {
        int count = 0;
        foreach (NPC hand in NPC.ActiveNPCs)
        {
            if (IsLivingHand(NPC, hand))
                count++;
        }
        return count;
    }

    // 命中后立即转入无敌，避免同一帧的后续命中跳过过渡阶段。
    public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) => TryEnterFinal();

    public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) => TryEnterFinal();

    public override void UpdateLifeRegen(ref int damage)
    {
        TryEnterFinal();
        if (IsFinalStage)
        {
            ApplyFinalDebuffImmunity();
            if (NPC.lifeRegen < 0)
                NPC.lifeRegen = 0;
            damage = 0;
        }
    }

}
