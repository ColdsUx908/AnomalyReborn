namespace Anomalies.Bosses.BrainofCthulhu;

// Behavior instances are shared. All encounter state belongs to the entity's synchronized arrays.
public sealed partial class BrainofCthulhu
{
    public enum Phase : byte
    {
        Initialize,
        Introduction,
        Phase1,
        Stunned,
        Transition,
        Phase2,
        Death
    }

    public Phase CurrentPhase
    {
        get => (Phase)AI_Union_0.byte0;
        set
        {
            Union32 data = AI_Union_0;
            data.byte0 = (byte)value;
            AI_Union_0 = data;
        }
    }

    public Behavior CurrentBehavior
    {
        get => (Behavior)AI_Union_0.byte1;
        set
        {
            if (CurrentBehavior == value && CurrentPhase != Phase.Initialize)
                return;
            Union32 data = AI_Union_0;
            data.byte1 = (byte)value;
            AI_Union_0 = data;
            CurrentPhase = value switch
            {
                <= Behavior.SurfaceSpawnAnimation => Phase.Introduction,
                Behavior.Stunned => Phase.Stunned,
                < Behavior.Phase2TransitionClosed => Phase.Phase1,
                <= Behavior.Phase2TransitionOpen => Phase.Transition,
                Behavior.DeathAnimation => Phase.Death,
                _ => Phase.Phase2
            };
            NPC.netUpdate = true;
        }
    }

    // NPC.ai: [0] packed Phase/Behavior; [1] timer; [2] despawn timer; [3] cached health ratio.
    // AI32: [0..17] scalar attack data; [18..19] list counts; [20..63] packed lists/target bits.
    // AI64: [0] attack position; [1..15] remaining packed list bytes; [16] draw offset.
    public ref float Time => ref NPC.ai[1];
    public ref float DespawnTime => ref NPC.ai[2];
    public ref float CachedRatio => ref NPC.ai[3];

    private float Read(int slot) => AnomalyNPC.AnomalyAI32[slot].f;
    private void Write(int slot, float value)
    {
        if (Read(slot) == value)
            return;
        AnomalyNPC.AnomalyAI32[slot].f = value;
        AnomalyNPC.AIChanged32[slot] = true;
    }

    public float ShieldOpacity { get => (float)Read(0); set => Write(0, (float)value); }
    public float ShieldScale { get => (float)Read(1); set => Write(1, (float)value); }
    public float TeleportTime { get => (float)Read(2); set => Write(2, (float)value); }
    public float TeleportDuration { get => (float)Read(3); set => Write(3, (float)value); }
    public float SpawnTime { get => (float)Read(4); set => Write(4, (float)value); }
    public int SpawnDelay { get => (int)Read(5); set => Write(5, value); }
    public Behavior PreviousAttack { get => (Behavior)Read(6); set => Write(6, (float)value); }
    public bool OnSecondCreeperPhase { get => Read(7) != 0; set => Write(7, value ? 1 : 0); }
    public bool ForcedIllusionAttack { get => Read(8) != 0; set => Write(8, value ? 1 : 0); }
    public bool IsNegative { get => Read(9) != 0; set => Write(9, value ? 1 : 0); }
    public float AttackRotation { get => (float)Read(10); set => Write(10, (float)value); }
    public float AttackTime { get => (float)Read(11); set => Write(11, (float)value); }
    public int CurrentLocalPhase { get => (int)Read(12); set => Write(12, value); }
    public bool AttackFlag { get => Read(13) != 0; set => Write(13, value ? 1 : 0); }
    public int AttackSign { get => IsNegative ? -1 : 1; set => IsNegative = value < 0; }

    private Vector2 ReadVector(int slot) => AnomalyNPC.AnomalyAI64[slot].GetValue<Vector2>();
    private void WriteVector(int slot, Vector2 value)
    {
        if (ReadVector(slot) == value)
            return;
        AnomalyNPC.AnomalyAI64[slot].SetValue(value);
        AnomalyNPC.AIChanged64[slot] = true;
    }

    public Vector2 AttackPosition { get => ReadVector(0); set => WriteVector(0, value); }
    public Vector2 BoCDrawOffset { get => ReadVector(16); set => WriteVector(16, value); }
    public ref Rectangle BoCFrame => ref AnomalyNPC.BrainOfCthulhu_Frame;
    public BrainByteList<Behavior> AvailableAttacks => new(NPC, 0, 8, 18);
    public BrainByteList<byte> AttackList => new(NPC, 8, 256, 19);
    public BrainTargetSet TargetsSet => new(NPC);
}

public sealed partial class CreeperAI
{
    public BrainofCthulhu.Phase CurrentPhase => BoCAI.CurrentPhase;

    private float Read(int slot) => AnomalyNPC.AnomalyAI32[slot].f;
    private void Write(int slot, float value)
    {
        if (Read(slot) == value)
            return;
        AnomalyNPC.AnomalyAI32[slot].f = value;
        AnomalyNPC.AIChanged32[slot] = true;
    }

    public bool IsPhantom { get => Read(6) != 0; set => Write(6, value ? 1 : 0); }

    public float FlowTime { get => (float)Read(0); set => Write(0, value); }
    public int Time { get => (int)Read(1); set => Write(1, value); }
    public int CachedValue2 { get => (int)Read(2); set => Write(2, value); }
    public int PartnerIndex { get => (int)Read(3); set => Write(3, value); }
    public float ConnectionOpacity { get => (float)Read(4); set => Write(4, value); }
    public int CurrentLocalPhase { get => Time; set => Time = value; }

    public Vector2 AttackPosition
    {
        get => AnomalyNPC.AnomalyAI64[0].GetValue<Vector2>();
        set
        {
            AnomalyNPC.AnomalyAI64[0].SetValue(value);
            AnomalyNPC.AIChanged64[0] = true;
        }
    }
}
