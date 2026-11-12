namespace Anomalies.Bosses.Skeletron;

public sealed partial class SkeletronHead
{
    public override bool PreAI()
    {
        using BossNetUpdateScope netUpdate = new(NPC);

        // Late joiners and predicted timers receive the same complete per-NPC snapshot.
        if (TONetworkEnvironment.NotClient && ++ResyncTimer >= 60f)
        {
            ResyncTimer = 0f;
            NPC.netUpdate = true;
        }

        NPC.defense = NPC.defDefense;
        NPC.damage = NPC.defDamage;
        NPC.reflectsProjectiles = false; //不反弹弹幕

        if (CurrentBehavior == Behavior.Despawn || !NPC.TargetClosestIfInvalid(true, DespawnDistance))
        {
            if (TONetworkEnvironment.NotClient)
                CurrentBehavior = Behavior.Despawn;
            if (CurrentBehavior == Behavior.Despawn)
                Despawn();
            return false;
        }

        if (NPC.timeLeft < 1800)
            NPC.timeLeft = 1800;

        if (CurrentPhase == Phase.Initialize)
        {
            if (TONetworkEnvironment.NotClient)
            {
                SpawnHand(-1.3f, 0f, 0f);
                SpawnHand(1.3f, 0f, 150f);
                SpawnHand(-1.3f, -1f, UpperHandNegativeSideInitialTimer);
                SpawnHand(1.3f, -1f, UpperHandPositiveSideInitialTimer);
                NPC.netUpdate = true;

                void SpawnHand(float side, float verticalSide, float timer)
                {
                    NPC.NewNPCAction(SourceAI, NPC.Center, NPCID.SkeletronHand, NPC.whoAmI, hand =>
                    {
                        hand.ai[0] = side;
                        hand.ai[1] = NPC.whoAmI;
                        hand.ai[3] = timer;
                        hand.target = NPC.target;
                        hand.Anomaly.EnsureAICapacity(9, 1);
                        hand.Anomaly.AnomalyAI32[0].F = verticalSide;
                        hand.Anomaly.AnomalyAI32[2].F = timer;
                        hand.netUpdate = true;
                    });
                }
            }

            CurrentPhase = Phase.Phase1;
        }

        int numHandsAlive = UpdateHandProtection();
        bool handsDead = numHandsAlive == 0;

        AnomalyNPC.DR = 0f;
        AnomalyNPC.ExtraDR = 0f;
        AnomalyNPC.BossBar_IncreasingDefenseOrDR = false;

        if (TONetworkEnvironment.NotClient && handsDead && ShouldEnterPhase3)
        {
            Phase3HealStartLife = NPC.life;
            BeginPhaseTransition(Phase.PhaseChange_2To3, Behavior.PhaseChange_2To3);
        }

        TryEnterFinal();
        if (IsFinalStage)
            ApplyFinalDebuffImmunity();
        if (CurrentPhase == Phase.ToFinal)
            NPC.dontTakeDamage = true;

        if (!IsEnteringPhase3 && !Phase3 && !IsFinalStage && Main.IsItDay() && CurrentBehavior != Behavior.Enraged)
        {
            CurrentBehavior = Behavior.Enraged;
            SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
        }

        AnomalyNPC.BossBar_Enraged = CurrentBehavior == Behavior.Enraged;
        switch (CurrentPhase)
        {
            case Phase.PhaseChange_2To3:
                PhaseChange_2To3AI();
                break;
            case Phase.ToFinal:
                ToFinalAI();
                break;
            case Phase.Final:
                FinalAI();
                break;
            case Phase.Phase3:
                Phase3AI();
                break;
            case Phase.Phase1 or Phase.Phase2 or Phase.Phase2_2 when CurrentBehavior == Behavior.Enraged:
                Enraged();
                break;
            case Phase.Phase1:
                Phase1AI(handsDead);
                break;
            case Phase.Phase2 or Phase.Phase2_2:
                Phase2AI(handsDead);
                break;
        }

        // 旋转会重设防御；有手保护在行为执行后保持固定防御值。
        if (numHandsAlive > 0)
            NPC.defense = WithHandsDefense;

        if (CurrentBehavior != Behavior.Enraged && numHandsAlive != 0)
            EmitHandProtectionDust();

        return false;
    }

    private int UpdateHandProtection()
    {
        int count = CountLivingHands();
        NPC.dontTakeDamage = false;
        AnomalyNPC.BossBar_Immune = count > 0;
        if (count > 0)
            NPC.defense = WithHandsDefense;
        NPC.chaseable = count == 0;
        return count;
    }

    private void Enraged()
    {
        NPC.damage = 1000;
        AnomalyNPC.ExtraDR = 0.9999f;
        AnomalyNPC.DR = 0f;

        AnomalyNPC.BossBar_IncreasingDefenseOrDR = true;
        SkeletronKinematics.SpinMovement(NPC, Target, SpinSpeed, 8f);
    }

    private void Despawn()
    {
        UpdateHandProtection();
        AnomalyNPC.DR = 0f;
        AnomalyNPC.ExtraDR = 0f;
        AnomalyNPC.BossBar_IncreasingDefenseOrDR = false;

        // Disable teleports
        if (TeleportTimer != 0f || TeleportX != 0f || TeleportY != 0f)
        {
            TeleportTimer = 0f;
            TeleportX = 0f;
            TeleportY = 0f;
            NPC.netUpdate = true;
        }

        NPC.velocity.Y += 0.1f;
        if (NPC.velocity.Y < 0f)
            NPC.velocity.Y *= 0.95f;
        NPC.velocity.X *= 0.95f;
        if (NPC.timeLeft > 50)
            NPC.timeLeft = 50;
    }
}
