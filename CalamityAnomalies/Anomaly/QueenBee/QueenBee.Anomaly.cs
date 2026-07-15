// Developed by ColdsUx

using CalamityAnomalies.DataStructures;

namespace CalamityAnomalies.Anomaly.QueenBee;

public sealed partial class QueenBee_Anomaly : AnomalyNPCBehavior<QueenBee_Anomaly>
{
    #region 数据
    public enum Phase : byte
    {
        Initialize,
        Phase1,
        Phase1_2,
        Phase1_3,
        Phase1_4,
        Phase1_5,
        PhaseChange_1To2,
        Phase2,
        Phase2_2,
    }

    public enum Behavior : byte
    {
        Despawn = byte.MaxValue,

        None = 0,

        Phase1_NormalCharge,
        Phase1_DirectCharge,
        Phase1_Stinger,
        Phase1_BeeSwarm,
        Phase1_BeeSwarm2,

        PhaseChange_2To3,
    }

    public const float DespawnDistance = 8000f;

    public const float Phase1_2LifeRatio_Anomaly = 0.5f;
    public const float Phase1_2LifeRatio_Ultra = 0.55f;
    public const float Phase2LifeRatio_Anomaly = 0f;
    public const float Phase2LifeRatio_Ultra = 0.1f;
    public const float Phase2_2LifeRatio_Anomaly = 0f;
    public const float Phase2_2LifeRatio_Ultra = 0.25f;

    public static float Phase1_2LifeRatio => Ultra ? Phase1_2LifeRatio_Ultra : Phase1_2LifeRatio_Anomaly;
    public static float Phase2LifeRatio => Ultra ? Phase2LifeRatio_Ultra : Phase2LifeRatio_Anomaly;
    public static float Phase2_2LifeRatio => Ultra ? Phase2_2LifeRatio_Ultra : Phase2_2LifeRatio_Anomaly;

    public static SoundStyle ChargeSound = SoundID.Zombie125 with { MaxInstances = 0 };

    public static ProjectileDamageContainer _beeDamage = new(40, 72, 96, 132, 96, 132);
    public static int BeeDamage => _beeDamage.Value;

    public float ChargeSpeed => MathHelper.Lerp(22.5f, 28f, NPC.LostLifeRatio);

    public static float ChargeDistanceX => 800f;
    public static float ChargeDistanceY => 25f;

    public static ProjectileDamageContainer _stingerDamage = new(40, 72, 96, 132, 96, 132);
    public static int StingerDamage => _stingerDamage.Value;

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

    public bool ShouldEnterPhase2 => Ultra && NPC.LifeRatio < Phase2LifeRatio;
    public bool InvalidPhase1 => ShouldEnterPhase2 && !Phase2;
    public bool Phase1_2 => CurrentPhase == Phase.Phase1_2;
    public bool Phase2 => CurrentPhase is Phase.Phase2 or Phase.Phase2_2;
    public bool Phase2_2 => CurrentPhase == Phase.Phase2_2;

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

    public bool AttackRandomVariation_Charge
    {
        get => AnomalyNPC.AnomalyAI32[0].bits[2];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].bits[2] != value)
            {
                AnomalyNPC.AnomalyAI32[0].bits[2] = value;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }

    public bool AttackRandomVariation_Stinger
    {
        get => AnomalyNPC.AnomalyAI32[0].bits[3];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].bits[3] != value)
            {
                AnomalyNPC.AnomalyAI32[0].bits[3] = value;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }

    public bool AttackRandomVariation_BeeSwarm
    {
        get => AnomalyNPC.AnomalyAI32[0].bits[4];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].bits[4] != value)
            {
                AnomalyNPC.AnomalyAI32[0].bits[4] = value;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }

    public int SafeCombCellNumber
    {
        get => AnomalyNPC.AnomalyAI32[1].i;
        set
        {
            if (AnomalyNPC.AnomalyAI32[1].i != value)
            {
                AnomalyNPC.AnomalyAI32[1].i = value;
                AnomalyNPC.AIChanged32[1] = true;
            }
        }
    }

    public Projectile SafeCombCell
    {
        get => Projectile.TryGetProjectile(SafeCombCellNumber);
        set => SafeCombCellNumber = value?.whoAmI ?? -1;
    }
    public bool HasSafeCombCell => SafeCombCellNumber >= 0 && SafeCombCell.active && SafeCombCell.ModProjectile is CombCell combCell && combCell.BehaviorType == CombCell.Behavior.BeeSwarm2_Safe;
    public CombCell ModCombCell => SafeCombCell.GetModProjectile<CombCell>();

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

    public Vector2 ChargeStartDistance
    {
        get => AnomalyNPC.AnomalyAI64[0].GetValue<Vector2>();
        set
        {
            if (AnomalyNPC.AnomalyAI64[0].GetValue<Vector2>() != value)
            {
                AnomalyNPC.AnomalyAI64[0].SetValue(value);
                AnomalyNPC.AIChanged64[0] = true;
            }
        }
    }

    public Vector2 SafeCombCellOffset
    {
        get => AnomalyNPC.AnomalyAI64[1].GetValue<Vector2>();
        set
        {
            if (AnomalyNPC.AnomalyAI64[1].GetValue<Vector2>() != value)
            {
                AnomalyNPC.AnomalyAI64[1].SetValue(value);
                AnomalyNPC.AIChanged64[1] = true;
            }
        }
    }
    #endregion 数据

    public override int ApplyingType => NPCID.QueenBee;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        _ => true,
    };

    public override void SetDefaults()
    {
        AttackRandomVariation_BeeSwarm = Main.rand.NextBool();

        NPC.AddAnomalyHPIndicator(Phase1_2LifeRatio_Anomaly, Phase1_2LifeRatio_Ultra, true);
        NPC.AddAnomalyHPIndicator(Phase2LifeRatio_Anomaly, Phase2LifeRatio_Ultra);
    }

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
