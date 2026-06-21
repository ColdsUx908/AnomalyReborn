// Developed by ColdsUx

namespace CalamityAnomalies.Anomaly.QueenBee;

public sealed partial class QueenBee_Anomaly : AnomalyNPCBehavior
{
    #region 数据
    public enum Phase : byte
    {
        Initialize,
        Phase1,
        Phase1_2,
        Phase1_3,
        Phase2,
        Phase2_2,
        Phase2_3,
        PhaseChange_2To3,
        Phase3,
        Phase3_2,
    }

    public enum Behavior : byte
    {
        Despawn = byte.MaxValue,

        None = 0,

        Phase1_Charge,
        Phase1_Stinger,
        Phase1_Beehell,
        Phase2_Charge,
        Phase2_RapidCharge,
        Phase2_Stinger_Rain,

        PhaseChange_2To3,
    }

    public const float DespawnDistance = 8000f;

    public const float Phase1_2LifeRatio_Anomaly = 0.85f;
    public const float Phase1_2LifeRatio_Ultra = 0.9f;
    public const float Phase1_3LifeRatio_Anomaly = 0.7f;
    public const float Phase1_3LifeRatio_Ultra = 0.75f;
    public const float Phase2LifeRatio_Anomaly = 0.5f;
    public const float Phase2LifeRatio_Ultra = 0.55f;
    public const float Phase2_2LifeRatio_Anomaly = 0.3f;
    public const float Phase2_2LifeRatio_Ultra = 0.35f;
    public const float Phase2_3LifeRatio_Anomaly = 0.1f;
    public const float Phase2_3LifeRatio_Ultra = 0.2f;
    public const float Phase3LifeRatio_Anomaly = 0f;
    public const float Phase3LifeRatio_Ultra = 0.1f;
    public const float Phase3_2LifeRatio_Anomaly = 0f;
    public const float Phase3_2LifeRatio_Ultra = 0.25f;

    public static float Phase1_2LifeRatio => Ultra ? Phase1_2LifeRatio_Ultra : Phase1_2LifeRatio_Anomaly;
    public static float Phase1_3LifeRatio => Ultra ? Phase1_3LifeRatio_Ultra : Phase1_3LifeRatio_Anomaly;
    public static float Phase2LifeRatio => Ultra ? Phase2LifeRatio_Ultra : Phase2LifeRatio_Anomaly;
    public static float Phase2_2LifeRatio => Ultra ? Phase2_2LifeRatio_Ultra : Phase2_2LifeRatio_Anomaly;
    public static float Phase2_3LifeRatio => Ultra ? Phase2_3LifeRatio_Ultra : Phase2_3LifeRatio_Anomaly;
    public static float Phase3LifeRatio => Ultra ? Phase3LifeRatio_Ultra : Phase3LifeRatio_Anomaly;
    public static float Phase3_2LifeRatio => Ultra ? Phase3_2LifeRatio_Ultra : Phase3_2LifeRatio_Anomaly;

    public float chargeDistanceX => Phase2_3 ? 700f : Phase2_2 ? 300f : Phase2 ? 600f : Phase1_2 ? 500f : 400f;
    public float chargeDistanceY
    {
        get
        {
            /*
            float value = Phase2_3 ? 150f : Phase2 ? 100f : 75f;
            value += MathHelper.Lerp(0f, 100f, 1f - (NPC.LifeRatio / 2));
            value *= 2f;
            return value;
            */
            return 20f;
        }
    }

    public Phase CurrentPhase
    {
        get
        {
            Union32 union = AI_Union_0;
            return (Phase)union.byte0;
        }
        set
        {
            Union32 union = AI_Union_0;
            union.byte0 = (byte)value;
            AI_Union_0 = union;
        }
    }

    public bool ShouldEnterPhase3 => Ultra && NPC.LifeRatio < Phase3LifeRatio;
    public bool InvalidPhase2 => ShouldEnterPhase3 && !Phase3;
    public bool Phase1_2 => CurrentPhase is >= Phase.Phase1_2 and <= Phase.Phase2_3;
    public bool Phase1_3 => CurrentPhase is >= Phase.Phase1_3 and <= Phase.Phase2_3;
    public bool Phase2 => CurrentPhase is >= Phase.Phase2 and <= Phase.Phase2_3;
    public bool Phase2_2 => CurrentPhase is Phase.Phase2_2 or Phase.Phase2_3;
    public bool Phase2_3 => CurrentPhase == Phase.Phase2_3;
    public bool Phase3 => CurrentPhase is Phase.Phase3 or Phase.Phase3_2;
    public bool Phase3_2 => CurrentPhase == Phase.Phase3_2;

    public Behavior CurrentBehavior
    {
        get
        {
            Union32 union = AI_Union_0;
            return (Behavior)union.byte1;
        }
        set
        {
            Union32 union = AI_Union_0;
            union.byte1 = (byte)value;
            AI_Union_0 = union;
        }
    }

    public Behavior LastBehavior
    {
        get
        {
            Union32 union = AI_Union_0;
            return (Behavior)union.byte2;
        }
        set
        {
            Union32 union = AI_Union_0;
            union.byte2 = (byte)value;
            AI_Union_0 = union;
        }
    }

    public Behavior LastBehavior2
    {
        get
        {
            Union32 union = AI_Union_0;
            return (Behavior)union.byte3;
        }
        set
        {
            Union32 union = AI_Union_0;
            union.byte3 = (byte)value;
            AI_Union_0 = union;
        }
    }

    public int CurrentAttackPhase
    {
        get => (int)NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    public int FinishedBehaviorCounter
    {
        get => (int)NPC.ai[3];
        set => NPC.ai[3] = value;
    }

    public bool IsCharging
    {
        get => AnomalyNPC.AnomalyAI32[0].bits[0];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].bits[0] != value)
            {
                AnomalyNPC.AnomalyAI32[0].bits[0] = value;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }

    public bool ShouldDecelerate
    {
        get => AnomalyNPC.AnomalyAI32[0].bits[1];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].bits[1] != value)
            {
                AnomalyNPC.AnomalyAI32[0].bits[1] = value;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }

    public int CurrentAttackCounter
    {
        get => AnomalyNPC.AnomalyAI32[4].i;
        set
        {
            if (AnomalyNPC.AnomalyAI32[4].i != value)
            {
                AnomalyNPC.AnomalyAI32[4].i = value;
                AnomalyNPC.AIChanged32[4] = true;
            }
        }
    }
    #endregion 数据

    public static QueenBee_Anomaly GetNewInstance(NPC npc) => new() { _entity = npc };

    public override int ApplyingType => NPCID.QueenBee;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior method) => method switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        _ => true,
    };

    public override void FindFrame(int frameHeight)
    {
        int frameNum;
        ref double frameCounter = ref OceanNPC.FrameCounter;

        frameCounter += 1.0;

        bool isCharging = IsCharging;
        int frames = isCharging ? 4 : 8; //冲刺为0~3，非冲刺为4~11

        frameNum = (int)(frameCounter / 4.0);
        if (frameNum >= frames)
        {
            frameCounter = 0.0;
            frameNum = 0;
        }

        if (!isCharging)
            frameNum += 4;

        NPC.frame.Y = frameNum * frameHeight;
    }
}
