using Anomalies.Assets.Effects;
using Anomalies.Visuals.BossBar;

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

    #region 数据
    public enum Phase : byte
    {
        Initialize,
        Phase1,
        Phase2,
        Phase2_2,
        PhaseChange_2To3,
        Phase3,
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
        Phase3_NodeVolley,
        Phase3_OppositeGuardians,
        Phase3_ChordCharge,
        Phase3_ClosingRing,
        Despawn = byte.MaxValue,
    }

    // 阶段与退场
    public const float Phase22LifeRatioAnomaly = 0.5f;
    public const float Phase22LifeRatioUltra = 0.6f;
    public static float Phase2_2LifeRatio => Ultra ? Phase22LifeRatioUltra : Phase22LifeRatioAnomaly;
    public const float Phase3LifeRatioAnomaly = 0f;
    public const float Phase3LifeRatioUltra = 0.1f;
    public static float Phase3LifeRatio => Ultra ? Phase3LifeRatioUltra : Phase3LifeRatioAnomaly;
    private const float Phase3HealLifeRatio = 0.3f;
    private const int Phase3StillDuration = 60;
    private const int Phase3HealDuration = 60;
    private const int Phase3HealInterval = 10;
    private const int Phase3ApproachDuration = 63;
    private const int Phase3BrakeDuration = 30;
    private const float Phase3ApproachHeight = 320f;
    private const float Phase3ApproachSpeed = 40f;

    public const float DespawnDistance = 8000f;

    // 手部
    public static float UpperHandNegativeSideInitialTimer => Ultra ? -75f : 0f;
    public static float UpperHandPositiveSideInitialTimer => Ultra ? 75f : 150f;
    public const float HandSlapGateValue = 300f;
    public const float HandSlapTelegraphTime = 120f;
    public const float HandSwipeDistanceMaster = 1280f;

    // 悬停与头骨
    public const int WithHandsDuration = 480;
    public const int MinNormalSkullDuration = 180;
    public const int MaxNormalSkullDuration = 300;
    public const int MinRapidSkullDuration = 240;
    public const int MaxRapidSkullDuration = 300;
    public const int QuietDuration = 60;
    public const int PostSpinHoverQuietDuration = 30;
    public static float HoverBaseAccelerationX => Ultra ? 0.21f : 0.16f;
    public static float HoverBaseAccelerationY => Ultra ? 0.07f : 0.06f;
    public static float HoverLowHealthAccelerationX => Ultra ? 0f : 0.08f;
    public static float HoverLowHealthAccelerationY => Ultra ? 0f : 0.04f;
    public const int SkullInterval = 30;
    public const int EnhancedSkullInterval = 10;
    public static int HoverDoubleSkullChanceDenominator => Ultra ? 3 : 0;
    public const float HeadSkullSpeed = 20f;

    private static ProjectileDamageContainer _SkullDamage = new(30, 60, 90, 120, 90, 120);
    public static int SkullDamage => _SkullDamage.Value;
    public static float HoverCrossboneSpeed = Ultra ? 16.5f : 15f;
    public static float CrossboneMaxSpeed => Skull.MaxSpeed;

    // 旋转与预警
    public const float ChargeGateValue = 600f;
    public const float ChargeTelegraphTime = 120f;
    public const float SpinDamageMult = 1.3f;
    public const float SpinSpeed = 0.3f;
    public const float SpinBaseMoveSpeed = 10f;
    public const float SpinMinMoveSpeed = 7.5f;
    public const float SpinMaxMoveSpeed = 40f;
    public const float SpinSlowdownMinDistance = 80f;
    public const float SpinSlowdownEndDistance = 160f;
    public const float SpinAccelerationStartDistance = 320f;
    public const float SpinAccelerationMaxDistance = 1280f;
    private const int HandsClearedHoverQuietDuration = 120;
    private const int HandsClearedMinSpinRemaining = 120;
    public const int HandsClearedSpinDuration = 240;
    public const int EnhancedSpinDuration = 420;
    public const int WarningSpinQuietDuration = 60;
    public static int SkullWarningInterval => Ultra ? 60 : 75;
    public static int SkullWarningCount => Ultra ? 4 : 3;
    public static int Phase22SkullWarningCount => Ultra ? 6 : 5;
    public const int ForwardSkullSpinDuration = 450;
    public const int ForwardSkullInterval = 75;
    public static int ForwardSkullCount => Ultra ? 12 : 8;
    public static float ForwardSkullArc => Ultra ? 60f : 40f;
    public const float ForwardSkullRadius = 1200f;

    // 三连旋冲与环形散射
    public const float SpinChargeDuration = 210f;
    public const int ChargesPerRound = 3;
    public const int ChargeRetreatDuration = 30;
    public const int MaxChargeRetreatDuration = 120;
    public static float RetreatSpeedMultiplier => Ultra ? 1.2f : 1f;
    public const float ChargeDistance = 320f;
    public static float ChargeTrackingSpeedMultiplier => Ultra ? 1.3f : 1f;
    public static int ChargeEndSkullCount => Ultra ? 8 : 4;
    public const float ChargeEndSkullSpacing = MathHelper.PiOver4;
    public const float ChargeEndSkullSpeed = 6f;
    public const float RadialSkullMaxSpeed = 12f;
    public const float RadialSkullAcceleration = 1.008f;
    public const int RadialSkullHomingStartFrame = 16;
    public const int RadialSkullHomingDuration = 84;
    public static int FinalChargeFanCount => Ultra ? 5 : 3;
    public static float FinalChargeFanHalfAngle => Ultra ? 80f : 60f;
    public static float FinalChargeSkullSpeed => Ultra ? 8f : ChargeEndSkullSpeed;
    public static float FinalChargeSkullMaxSpeed => Skull.MaxSpeed;
    public const float FinalChargeSpawnOffset = 5f;

    public static int TeleportGateValue => Ultra ? 180 : 300;
    public const int MaximumSpreadCount = 36;
    public const float MaximumSpreadDegrees = 180f;
    // Indexed by living hand count from zero to four, then the fallback for any larger count.
    public const int DefaultHandCountIndex = 5;
    public static ReadOnlySpan<int> TeleportSpreadCounts =>
        Ultra ? [MaximumSpreadCount, 27, 18, 15, 12, 22] : [9, 7, 6, 5, 4, 5];
    public static ReadOnlySpan<int> TeleportSpreadAngles =>
        Ultra ? [(int)MaximumSpreadDegrees, 150, 140, 130, 120, 180] : [90, 76, 70, 64, 56, 60];

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
    private bool ShouldEnterPhase3 => Ultra && (Phase2 || CurrentPhase == Phase.Phase1 && CurrentBehavior == Behavior.Enraged)
        && NPC.life <= Phase3LifeFloor;
    private int Phase3LifeFloor => Math.Max(1, (int)(NPC.lifeMax * Phase3LifeRatio));
    private bool DeathLocked => Ultra && !Phase3 && CurrentBehavior != Behavior.Despawn;

    public bool IsSpinning => CurrentBehavior is Behavior.Spin or Behavior.SkullWarningSpin
        or Behavior.TripleSpinCharge or Behavior.ForwardSkullSpin or Behavior.Enraged or Behavior.Phase3_ChordCharge;

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
    #endregion 数据

    public override void SetDefaults()
    {
        NPC.AddAnomalyHPIndicator(Phase3LifeRatioAnomaly, Phase3LifeRatioUltra,
            condition: npc => npc.Anomaly.CustomAIType != 1);
        NPC.AddAnomalyExtraHPIndicator(0f, 0f, condition: npc => npc.Anomaly.CustomAIType != 1);
        NPC.AddAnomalyHPIndicator(Phase22LifeRatioAnomaly, Phase22LifeRatioUltra, true,
            npc => npc.Anomaly.CustomAIType != 1 && GetInstance(npc).Phase2);
    }

    public override void Load() => BossDeathEffectRegistry.Register(NPCID.SkeletronHead,
        npc => GetInstance(npc) is { ShouldProcess: true, DeathLocked: true });

    public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
    {
        if (DeathLocked)
            modifiers.SetMaxDamage(Math.Max(1, NPC.life - Phase3LifeFloor));
    }

    public override bool CheckDead()
    {
        if (!DeathLocked)
            return true;

        NPC.life = Phase3LifeFloor;
        NPC.active = true;
        NPC.netUpdate = true;
        return false;
    }

    public override void SetDefaultsFinal()
    {
        NPC.lifeMax = CalamityEnabled ? 9000 : 7500;
        BridgeUtils.ApplyCalamityHealthBoost(NPC);
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

    // NPC.ai: [0] phase (Byte0), behavior (Byte1), next enhanced spin (Byte2), completed charges (Byte3),
    // [1] local phase, [2] timer, [3] teleport timer.
    // AI32: [0] life ratio at enhanced hover start, [1] attack age (negative during post-spin quiet), [2..3] teleport destination,
    // [4] charge readiness, [5] charge telegraph, [6] rapid skull duration, [7] life at phase-three transition start, [8] resync timer, [9] phase-three cycle (0..31).
    // AI64: [0..2] phase-three approach brake origin, destination and initial velocity; in phase three [0..1] hold chord endpoints.
    public override void BossHeadRotation(ref float rotation) =>
        rotation = IsSpinning ? NPC.rotation : 0f;

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        DrawPhase3ChordWarning(spriteBatch);
        // Telegraph for charges
        float beginTelegraphGateValue = ChargeGateValue - ChargeTelegraphTime;
        if (ChargeTelegraphTimer > beginTelegraphGateValue)
        {
            float colorScale = MathHelper.Clamp((ChargeTelegraphTimer - beginTelegraphGateValue) / ChargeTelegraphTime, 0f, 1f);
            Color drawColor2 = new Color(150, 150, 150, 0) * colorScale;
            Vector2 halfSize = NPC.frame.Size() / 2;
            SpriteEffects spriteEffects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            for (int i = 0; i < 2; i++)
            {
                spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, NPC.Center - screenPos + new Vector2(0, NPC.gfxOffY), NPC.frame,
                    drawColor2, NPC.rotation, halfSize, NPC.scale, spriteEffects, 0f);
            }
        }
    }

    public override void ApplyCustomMainBossBarShader(BossHealthBar newBar, SpriteBatch spriteBatch, Rectangle destinationRectangle)
    {
        AnomalyEffects.BossBars.Skeletron.Data
            .UseImage1(AnomalyTextures.Noise._Vein)
            .SetCustomParameter("uScreenResolution", Main.ScreenSize.ToVector2() * Math.Max(Main.UIScale, 1f) / 2f)
            .SetCustomParameter("uPosition", destinationRectangle.TopLeft())
            .SetCustomParameter("uStarIntensity", 0f)
            .Apply();
    }
}
