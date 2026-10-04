using Anomalies.Assets.Effects;
using Anomalies.DataStructures;
using Anomalies.Visuals.BossBar;

namespace Anomalies.Bosses.QueenBee;

public sealed partial class QueenBee : AnomalyNPCBehavior<QueenBee>
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

        PhaseChange_1To2,

        Phase2_Stinger,
        Phase2_BeeSwarm3,
        Phase2_BeeSwarm4,
    }

    public const string AnomalyQueenBeePath = AnomalySharedData.ModPath + "Bosses/QueenBee/";

    public const float DespawnDistance = 8000f;
    public const float EnrageDistance = 1250f;

    public const float Phase1_2LifeRatio_Anomaly = 0.5f;
    public const float Phase1_2LifeRatio_Ultra = 0.65f;
    public const float Phase2LifeRatio_Anomaly = 0f;
    public const float Phase2LifeRatio_Ultra = 0.1f;
    public const float Phase2_2LifeRatio_Anomaly = 0f;
    public const float Phase2_2LifeRatio_Ultra = 0.25f;

    public static float Phase1_2LifeRatio => Ultra ? Phase1_2LifeRatio_Ultra : Phase1_2LifeRatio_Anomaly;
    public static float Phase2LifeRatio => Ultra ? Phase2LifeRatio_Ultra : Phase2LifeRatio_Anomaly;
    public static float Phase2_2LifeRatio => Ultra ? Phase2_2LifeRatio_Ultra : Phase2_2LifeRatio_Anomaly;

    public static float OwnedCombCellScaleMultiplier => 0.875f;

    public static SoundStyle ChargeSound = SoundID.Zombie125 with { MaxInstances = 0 };
    public static SoundStyle HugeStingerShootSound = new(AnomalyQueenBeePath + "HugeStingerShoot") { MaxInstances = 0 };

    public static ProjectileDamageContainer _BeeDamage = new(40, 72, 96, 132, 96, 132);
    public static int BeeDamage => _BeeDamage.Value;

    public float ChargeSpeed => MathHelper.Lerp(22.5f, 28f, NPC.LostLifeRatio);

    public float ChargeDistanceX => MathHelper.Lerp(Ultra ? 500f : 600f, Ultra ? 400f : 480f, NPC.LostLifeRatio);
    public static float ChargeDistanceY => 25f;

    public static int StingerProjectileType => Aroma ? ProjectileID.FlamingWood : ProjectileID.QueenBeeStinger;

    public static ProjectileDamageContainer _StingerDamage = new(40, 72, 96, 132, 96, 132);
    public static int StingerDamage => _StingerDamage.Value;

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

    public float LifeRatioForPhase2 => Math.Min(NPC.LifeRatio * 2f, 1f);
    public float LostLifeRatioForPhase2 => 1f - LifeRatioForPhase2;

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

    public int CurrentLocalPhase
    {
        get => (int)NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    public int FinishedBehaviorCounter
    {
        get => (int)NPC.ai[2];
        set => NPC.ai[2] = value;
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

    public bool HasBeenEnraged
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

    public bool AttackRandomVariation_Charge
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

    public bool AttackRandomVariation_Stinger
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

    public bool AttackRandomVariation_BeeSwarm
    {
        get => AnomalyNPC.AnomalyAI32[0].bits[5];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].bits[5] != value)
            {
                AnomalyNPC.AnomalyAI32[0].bits[5] = value;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }

    public bool LastPhase1BeeSwarmAttackIsType2
    {
        get => AnomalyNPC.AnomalyAI32[0].bits[6];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].bits[6] != value)
            {
                AnomalyNPC.AnomalyAI32[0].bits[6] = value;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }

    public bool AttackRandomVariation_BeeSwarmPhase2
    {
        get => AnomalyNPC.AnomalyAI32[0].bits[7];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].bits[7] != value)
            {
                AnomalyNPC.AnomalyAI32[0].bits[7] = value;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }

    public Projectile OwnedCombCell
    {
        get => Projectile.GetProjectileFromIndex(AnomalyNPC.AnomalyAI32[1].i);
        set
        {
            int temp = value?.whoAmI ?? -1;
            if (AnomalyNPC.AnomalyAI32[1].i != temp)
            {
                AnomalyNPC.AnomalyAI32[1].i = temp;
                AnomalyNPC.AIChanged32[1] = true;
            }
        }
    }
    public bool OwnCombCell => OwnedCombCell.active && OwnedCombCell.ModProjectile is CombCell;
    public CombCell ModOwnedCombCell => OwnedCombCell.GetModProjectile<CombCell>();

    public Projectile SpecialCombCell
    {
        get => Projectile.GetProjectileFromIndex(AnomalyNPC.AnomalyAI32[2].i);
        set
        {
            int temp = value?.whoAmI ?? -1;
            if (AnomalyNPC.AnomalyAI32[2].i != temp)
            {
                AnomalyNPC.AnomalyAI32[2].i = temp;
                AnomalyNPC.AIChanged32[2] = true;
            }
        }
    }
    public bool HasSpecialCombCell => SpecialCombCell.active && SpecialCombCell.ModProjectile is CombCell;

    public int CurrentAttackCounter
    {
        get => AnomalyNPC.AnomalyAI32[3].i;
        set
        {
            if (AnomalyNPC.AnomalyAI32[3].i != value)
            {
                AnomalyNPC.AnomalyAI32[3].i = value;
                AnomalyNPC.AIChanged32[3] = true;
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

    /* 数组使用说明
     * 
     * NPC.ai
     *   [0]. (Union)
     *       byte0 CurrentPhase
     *       byte1 CurrentBehavior
     *   [1] CurrentLocalPhase
     *   [2] FinishedBehaviorCounter
     * 
     * AnomalyAI32
     *   [0].
     *       bits[0] IsCharging
     *       bits[1] ShouldDecelerate
     *       bits[2] HasBeenEnraged
     *       bits[3] AttackRandomVariation_Charge
     *       bits[4] AttackRandomVariation_Stinger
     *       bits[5] AttackRandomVariation_BeeSwarm
     *       bits[6] LastPhase1BeeSwarmAttackIsType2
     *       bits[7] AttackRandomVariation_BeeSwarmPhase2
     *   [1].i OwnedCombCell
     *   [2].i SpecialCombCell
     *   [3].i CurrentAttackCounter
     * 
     * AnomalyAI64
     *   [0] (Vector2) ChargeStartDistance
     */
    #endregion 数据

    public override int ApplyingType => NPCID.QueenBee;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        CalamityLogicType_NPCBehavior.GetAlpha => false,
        _ => true,
    };

    public override void SetDefaults()
    {
        AttackRandomVariation_BeeSwarm = Main.rand.NextBool();
        AttackRandomVariation_BeeSwarmPhase2 = Main.rand.NextBool();

        AnomalyNPC.DynamicDRHandler = new TimedDDRHandler(
            new TimedDDRHandler.SingleDDRHandler(1f, Phase2LifeRatio, null, n => GetInstance(n).CurrentPhase >= Phase.PhaseChange_1To2, 60),
            new TimedDDRHandler.SingleDDRHandler(0.5f, 0f, n => GetInstance(n).Phase2, null, 45)
        );

        NPC.AddAnomalyHPIndicator(Phase1_2LifeRatio_Anomaly, Phase1_2LifeRatio_Ultra, true);
        NPC.AddAnomalyHPIndicator(Phase2LifeRatio_Anomaly, Phase2LifeRatio_Ultra);
        NPC.AddAnomalyHPIndicator(Phase2_2LifeRatio_Anomaly, Phase2_2LifeRatio_Ultra, true, n => GetInstance(n).Phase2);
    }

    public override void SetDefaultsFinal()
    {
        NPC.lifeMax = CalamityEnabled ? 9000 : 6000;
        NPC.damage = 40;
        BridgeUtils.ApplyCalamityHealthBoost(NPC);
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
    {
        const float VanillaExpertLifeMultiplier = 1.4f;
        NPC.lifeMax = (int)MathF.Round(NPC.lifeMax / (VanillaExpertLifeMultiplier * 1.5f) / bossAdjustment / 10f) * 10;
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

    public override Color? GetAlpha(Color drawColor)
    {
        if (Aroma)
            return Color.Red with { A = NPC.GraphicAlpha };

        return null;
    }

    public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
    {
        if (Ultra && !Phase2)
            modifiers.SetMaxDamage((int)(NPC.life - NPC.lifeMax * Phase2LifeRatio + 1));
    }

    public override bool CheckDead()
    {
        if (Ultra && !Phase2 && !NPC.downedQueenBee)
        {
            NPC.life = 1;
            NPC.active = true;
            NPC.netUpdate = true;
            return false;
        }

        return true;
    }

    public override void ApplyCustomMainBossBarShader(BossHealthBar newBar, SpriteBatch spriteBatch, Rectangle destinationRentangle)
    {
        AnomalyEffects.BossBars.QueenBee.Data
            .UseImage1(AnomalyTextures.Noise._Milky)
            .SetCustomParameter("uScreenResolution", Main.ScreenSize.ToVector2() * Math.Max(Main.UIScale, 1f) / 2f)
            .SetCustomParameter("uScreenRatio", TODrawUtils.ScreenRatio)
            .SetCustomParameter("uPosition", destinationRentangle.TopLeft())
            .Apply();
    }
}

