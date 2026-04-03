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

        Phase1_SpawnBee,
        Phase1_Charge,
        Phase1_Stinger,
        Phase2_Charge,
        Phase2_RapidCharge,
        Phase2_Stinger_Rain,

        PhaseChange_2To3,
    }

    public const float DespawnDistance = 8000f;

    public static float Phase1_2LifeRatio => Ultra ? 0.9f : 0.85f;
    public static float Phase1_3LifeRatio => Ultra ? 0.75f : 0.7f;
    public static float Phase2LifeRatio => Ultra ? 0.55f : 0.5f;
    public static float Phase2_2LifeRatio => Ultra ? 0.35f : 0.3f;
    public static float Phase2_3LifeRatio => Ultra ? 0.2f : 0.1f;
    public static float Phase3LifeRatio => Ultra ? 0.1f : 0f;
    public static float Phase3_2LifeRatio => Ultra ? 0.25f : 0f;

    public static int beeLimit => 9;
    public static int hornetLimit => 2;

    public float ChargeSpeed => Phase2_3 ? 27f : Phase2_2 ? 16f : Phase2 ? 27f : Phase1_2 ? 22f : 17f;
    public float chargeDistanceX => Phase2_3 ? 700f : Phase2_2 ? 300f : Phase2 ? 600f : Phase1_2 ? 500f : 400f;
    public float chargeDistanceY
    {
        get
        {
            float value = Phase2_3 ? 150f : Phase2 ? 100f : 75f;
            value += MathHelper.Lerp(0f, 100f, 1f - (NPC.LifeRatio / 2));
            value *= 2f;
            return value;
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
    public bool Phase1_2 => CurrentPhase is >= Phase.Phase1_2 and <= Phase.Phase2_3;    //双倍刺针（死亡模式三倍）
    public bool Phase1_3 => CurrentPhase is >= Phase.Phase1_3 and <= Phase.Phase2_3;    //刺针速度略微提高
    public bool Phase2 => CurrentPhase is >= Phase.Phase2 and <= Phase.Phase2_3;        //停止生成蜜蜂，开始刺针弧线 + 对角冲刺，冲刺时生成蜜蜂
    public bool Phase2_2 => CurrentPhase is Phase.Phase2_2 or Phase.Phase2_3;          //多次短距离冲刺 + 双向刺针弧线
    public bool Phase2_3 => CurrentPhase == Phase.Phase2_3;                              //三倍刺针（死亡模式五倍）轰炸
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

    public bool ShouldDecelerate
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

    public int AttackCounter
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

    public override int ApplyingType => NPCID.QueenBee;

    public override bool ShouldProcess => false; //暂时禁用

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior method) => method switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        _ => true,
    };
}
