namespace Anomalies.Bosses.Skeletron;

public sealed partial class SkeletronHead
{
    public enum Phase : byte
    {
        Initialize,
        Phase1,
        Phase2,
        Phase2_2,
        PhaseChange_2To3,
        Phase3,
        ToFinal,
        Final,
    }

    public enum Behavior : byte
    {
        Hover,
        Spin,
        Enraged,
        SkullWarningSpin,
        TripleSpinCharge,
        ForwardSkullSpin,
        PhaseChange_2To3,
        Phase3_GuardianPatrol = 7,
        Phase3_GuardianRain = 8,
        Phase3_BoneTide = 9,
        ToFinal,
        FinalGuardian,
        Despawn = byte.MaxValue,
    }

    // NPC.ai: [0] phase (Byte0), behavior (Byte1), next enhanced spin (Byte2), completed charges (Byte3),
    // [1] local phase, [2] timer, [3] reserved legacy teleport timer.
    // AI32: [0] life ratio at enhanced hover start, [1] attack age (negative during post-spin quiet), [2..3] reserved legacy teleport destination,
    // [4] charge readiness, [5] charge telegraph, [6] rapid skull duration, [7] life at phase-three transition start, [8] resync timer, [9] phase-three cycle (0..31).
    // AI64: [0..2] phase-three approach brake origin, destination and initial velocity.

    public Phase CurrentPhase
    {
        get => (Phase)AI_Union_0.Byte0;
        set
        {
            if (CurrentPhase != value && Main.netMode != NetmodeID.MultiplayerClient)
                NPC.netUpdate = true;
            Union32 union = AI_Union_0;
            union.Byte0 = (byte)value;
            AI_Union_0 = union;
        }
    }

    public Behavior CurrentBehavior
    {
        get => (Behavior)AI_Union_0.Byte1;
        set
        {
            if (CurrentBehavior != value && Main.netMode != NetmodeID.MultiplayerClient)
                NPC.netUpdate = true;
            Union32 union = AI_Union_0;
            union.Byte1 = (byte)value;
            AI_Union_0 = union;
        }
    }

    public bool Phase2 => CurrentPhase is Phase.Phase2 or Phase.Phase2_2;
    public bool Phase2_2 => CurrentPhase == Phase.Phase2_2;
    private bool ShouldEnterPhase2_2 => CurrentPhase == Phase.Phase2 && NPC.LifeRatio <= Phase2_2LifeRatio;

    private bool Phase3 => CurrentPhase == Phase.Phase3;
    internal bool IsEnteringPhase3 => CurrentPhase == Phase.PhaseChange_2To3;
    private bool ShouldEnterPhase3 => Ultra && (Phase2 || CurrentPhase == Phase.Phase1)
        && NPC.life <= Phase3LifeFloor;
    private int Phase3LifeFloor => Math.Max(1, (int)(NPC.lifeMax * Phase3LifeRatio));
    internal bool IsFinalStage => CurrentPhase is Phase.ToFinal or Phase.Final;
    private bool CanEnterFinal => Ultra ? Phase3 : CurrentPhase is Phase.Phase1 or Phase.Phase2 or Phase.Phase2_2;
    private bool ShouldEnterFinal => CanEnterFinal && CurrentBehavior != Behavior.Despawn && NPC.life <= FinalLifeFloor;
    private int FinalLifeFloor => Math.Max(1, (int)(NPC.lifeMax * FinalLifeRatio));
    private bool DeathLocked => !IsFinalStage && CurrentBehavior != Behavior.Despawn
        && (Ultra && !Phase3 || !Ultra && CanEnterFinal);
    private int DeathLockLifeFloor => Ultra ? Phase3LifeFloor : FinalLifeFloor;

    public bool IsSpinning => CurrentBehavior is Behavior.Spin or Behavior.SkullWarningSpin
        or Behavior.TripleSpinCharge or Behavior.ForwardSkullSpin or Behavior.Enraged or Behavior.FinalGuardian;

    // 各行为的局部步骤编号见 AI 中对应的 switch。
    public int CurrentLocalPhase
    {
        get => (int)NPC.ai[1];
        set
        {
            if (CurrentLocalPhase != value && Main.netMode != NetmodeID.MultiplayerClient)
                NPC.netUpdate = true;
            NPC.ai[1] = value;
        }
    }

    private float HoverLifeRatio
    {
        get => AnomalyNPC.AnomalyAI32[0].F;
        set
        {
            AnomalyNPC.AnomalyAI32[0].F = value;
            AnomalyNPC.AIChanged32[0] = true;
        }
    }

    private int RapidSkullDuration
    {
        get => AnomalyNPC.AnomalyAI32[6].I;
        set
        {
            AnomalyNPC.AnomalyAI32[6].I = value;
            AnomalyNPC.AIChanged32[6] = true;
        }
    }

    private ref float AttackTimer => ref AnomalyNPC.AnomalyAI32[1].F;
    private ref float TeleportX => ref AnomalyNPC.AnomalyAI32[2].F;
    private ref float TeleportY => ref AnomalyNPC.AnomalyAI32[3].F;
    private ref float ChargeReadiness => ref AnomalyNPC.AnomalyAI32[4].F;
    private ref float ChargeTelegraphTimer => ref AnomalyNPC.AnomalyAI32[5].F;
    private int Phase3HealStartLife
    {
        get => AnomalyNPC.AnomalyAI32[7].I;
        set
        {
            AnomalyNPC.AnomalyAI32[7].I = value;
            AnomalyNPC.AIChanged32[7] = true;
        }
    }

    private int Phase3Cycle
    {
        get => AnomalyNPC.AnomalyAI32[9].I;
        set
        {
            AnomalyNPC.AnomalyAI32[9].I = value;
            AnomalyNPC.AIChanged32[9] = true;
        }
    }

    private ref float ResyncTimer => ref AnomalyNPC.AnomalyAI32[8].F;

    public ref float Timer => ref NPC.ai[2];
    public ref float TeleportTimer => ref NPC.ai[3];

    // 首轮常态攻击结束后才设置下一次强化旋转，之后不再清零。
    private bool HasCompletedNormalHover => AI_Union_0.Byte2 != 0;

    private Behavior NextEnhancedSpin
    {
        get => AI_Union_0.Byte2 == 0 ? Behavior.SkullWarningSpin : (Behavior)AI_Union_0.Byte2;
        set
        {
            Union32 state = AI_Union_0;
            state.Byte2 = (byte)value;
            AI_Union_0 = state;
        }
    }

    private byte CompletedCharges
    {
        get => AI_Union_0.Byte3;
        set
        {
            Union32 state = AI_Union_0;
            state.Byte3 = value;
            AI_Union_0 = state;
        }
    }

    // Reset head attack state without changing the enhanced-spin sequence or the arena.
    private void BeginPhaseTransition(Phase phase, Behavior behavior)
    {
        CurrentPhase = phase;
        CurrentBehavior = behavior;
        CurrentLocalPhase = 0;
        Timer = AttackTimer = ChargeReadiness = ChargeTelegraphTimer = 0f;
        TeleportTimer = TeleportX = TeleportY = 0f;
        CompletedCharges = 0;
    }
}
