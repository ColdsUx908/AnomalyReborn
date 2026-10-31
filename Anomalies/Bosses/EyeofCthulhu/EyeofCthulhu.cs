using Anomalies.Assets.Effects;
using Anomalies.Visuals.BossBar;

namespace Anomalies.Bosses.EyeofCthulhu;

public sealed partial class EyeofCthulhu : AnomalyNPCBehavior<EyeofCthulhu>
{
    protected override int AI32Capacity => 8;

    #region 数据
    public enum Phase : byte
    {
        Initialize,
        Phase1,
        PhaseChange_1To2,
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

        Phase1_Hover,
        Phase1_Charge,

        PhaseChange_1To2,

        Phase2_Hover,
        Phase2_NormalCharge,
        Phase2_RapidCharge,
        Phase2_Hover2,
        Phase2_HorizontalCharge,
        Phase2_EyeSpin,

        PhaseChange_2To3,

        Phase3_Charge,
        Phase3_EyeSpin,
    }

    public const float DespawnDistance = 6000f;
    public const float ProjectileOffset = 50f;
    public const int PhaseChangeTime1To2 = 150;
    public const int PhaseChangeGateValue1To21 = PhaseChangeTime1To2 / 5 * 2;
    public const int PhaseChangeGateValue1To22 = PhaseChangeTime1To2 / 5 * 3;
    public const int PhaseChangeTime2To3 = 195;
    public const int PhaseChangeGateValue2To31 = 75;
    public const int PhaseChangeGateValue2To32 = 120;

    public const float Phase2LifeRatioAnomaly = 0.75f;
    public const float Phase2LifeRatioUltra = 0.8f;
    public const float Phase22LifeRatioAnomaly = 0.5f;
    public const float Phase22LifeRatioUltra = 0.6f;
    public const float Phase23LifeRatioAnomaly = 0.25f;
    public const float Phase23LifeRatioUltra = 0.35f;
    public const float Phase3LifeRatioAnomaly = 0f;
    public const float Phase3LifeRatioUltra = 0.1f;
    public const float Phase32LifeRatioAnomaly = 0f;
    public const float Phase32LifeRatioUltra = 0.25f;

    public static float Phase2LifeRatio => Ultra ? Phase2LifeRatioUltra : Phase2LifeRatioAnomaly;
    public static float Phase2_2LifeRatio => Ultra ? Phase22LifeRatioUltra : Phase22LifeRatioAnomaly;
    public static float Phase2_3LifeRatio => Ultra ? Phase23LifeRatioUltra : Phase23LifeRatioAnomaly;
    public static float Phase3LifeRatio => Ultra ? Phase3LifeRatioUltra : Phase3LifeRatioAnomaly;
    public static float Phase3_2LifeRatio => Ultra ? Phase32LifeRatioUltra : Phase32LifeRatioAnomaly;

    public static float TeleportOffset => 150f;

    public static readonly SoundStyle Roar = SoundID.ForceRoar with { MaxInstances = 0 };
    public static readonly SoundStyle ForceRoar = SoundID.ForceRoarPitched with { MaxInstances = 0 };

    private static readonly ProjectileDamageContainer _BloodDamage = new(30, 60, 75, 90, 120, 150);
    public static int BloodDamage => _BloodDamage.Value;

    private static readonly ProjectileDamageContainer _ArenaDamage = new(150);
    public static int ArenaDamage => _ArenaDamage.Value;

    public static readonly Color Phase3Color = Color.Lerp(Color.DarkRed, Color.Tomato, 0.4f);

    public float DamageMultiplier => Phase3 ? 1.5f : Phase2_2 ? 1.25f : 1f;

    public int SetDamage => (int)Math.Round(NPC.defDamage * DamageMultiplier);
    public int ReducedSetDamage => (int)Math.Round(NPC.defDamage * DamageMultiplier * 0.6f);

    public bool IsInPhase3Arena => Phase3 && ArenaProjectileAlive && NPC.Distance(ArenaProjectile.Center) < ArenaProjectile.GetModProjectile<EyeofCthulhuArena>().Radius + 30f;

    public float EyeRotation => TOMathUtils.NormalizeWithPeriod((Target.Center - NPC.Center).ToRotation(-MathHelper.PiOver2));
    public float ActualRotation => NPC.rotation + MathHelper.PiOver2;
    public Vector2 DrawOffset => -new PolarVector2(24f, ActualRotation);

    public int RapidChargeTime => Ultra ? (Phase2_3 ? 10 : 13) : (Phase2_3 ? 12 : 15);

    public int HorizontalChargeTime => Phase2_3 ? 30 : 35;

    public static readonly UnaryFunctionWithDomain PhaseChange1To2RotationSpeedFunction = UnaryFunctionWithDomain.Piecewise(
        (new MathInterval(float.NegativeInfinity, PhaseChangeGateValue1To22, false, false), x => 0.5f * TOMathUtils.Interpolation.QuadraticEaseInOut(x / PhaseChangeGateValue1To21)),
        (new MathInterval(PhaseChangeGateValue1To22, float.PositiveInfinity, true, false), x => 0.5f * TOMathUtils.Interpolation.QuadraticEaseInOut((PhaseChangeTime1To2 - x) / PhaseChangeGateValue1To21))
    );
    public static readonly UnaryFunctionWithDomain PhaseChange2To3RotationSpeedFunction = UnaryFunctionWithDomain.Piecewise(
        (new MathInterval(float.NegativeInfinity, PhaseChangeGateValue2To32, false, false), x => 0.5f * TOMathUtils.Interpolation.QuadraticEaseInOut(x / PhaseChangeGateValue2To31)),
        (new MathInterval(PhaseChangeGateValue2To32, float.PositiveInfinity, true, false), x => 0.5f * TOMathUtils.Interpolation.QuadraticEaseInOut((PhaseChangeTime2To3 - x) / PhaseChangeGateValue2To31))
        );

    public Phase CurrentPhase
    {
        get
        {
            Union32 union = AI_Union_0;
            return (Phase)union.Byte0;
        }
        set
        {
            if (CurrentPhase != value && Main.netMode != NetmodeID.MultiplayerClient)
                NPC.netUpdate = true;
            Union32 union = AI_Union_0;
            union.Byte0 = (byte)value;
            AI_Union_0 = union;
        }
    }

    public bool ShouldEnterPhase3 => Ultra && NPC.LifeRatio <= Phase3LifeRatio;
    public bool InvalidPhase2 => ShouldEnterPhase3 && !Phase3;
    public bool Phase2_2 => CurrentPhase is Phase.Phase2_2 or Phase.Phase2_3;
    public bool Phase2_3 => CurrentPhase == Phase.Phase2_3;
    public bool Phase3 => CurrentPhase is Phase.Phase3 or Phase.Phase3_2;
    public bool Phase3_2 => CurrentPhase == Phase.Phase3_2;

    public Behavior CurrentBehavior
    {
        get
        {
            Union32 union = AI_Union_0;
            return (Behavior)union.Byte1;
        }
        set
        {
            if (CurrentBehavior != value && Main.netMode != NetmodeID.MultiplayerClient)
                NPC.netUpdate = true;
            Union32 union = AI_Union_0;
            union.Byte1 = (byte)value;
            AI_Union_0 = union;
        }
    }

    public int CurrentLocalPhase
    {
        get => (int)NPC.ai[1];
        set
        {
            if (NPC.ai[1] != value)
            {
                NPC.ai[1] = value;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NPC.netUpdate = true;
            }
        }
    }

    public bool Hover2DirectionIsNegative
    {
        get => AnomalyNPC.AnomalyAI32[0].Bits[0];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].Bits[0] != value)
            {
                AnomalyNPC.AnomalyAI32[0].Bits[0] = value;
                AnomalyNPC.AIChanged32[0] = true;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NPC.netUpdate = true;
            }
        }
    }

    public int Hover2Direction
    {
        get => Hover2DirectionIsNegative ? -1 : 1;
        set => Hover2DirectionIsNegative = value == -1;
    }

    public bool NextChargeTypeIsHorizontal
    {
        get => AnomalyNPC.AnomalyAI32[0].Bits[1];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].Bits[1] != value)
            {
                AnomalyNPC.AnomalyAI32[0].Bits[1] = value;
                AnomalyNPC.AIChanged32[0] = true;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NPC.netUpdate = true;
            }
        }
    }

    public int ServantSpawnCounter
    {
        get => AnomalyNPC.AnomalyAI32[3].I;
        set
        {
            if (AnomalyNPC.AnomalyAI32[3].I != value)
            {
                AnomalyNPC.AnomalyAI32[3].I = value;
                AnomalyNPC.AIChanged32[3] = true;
            }
        }
    }

    public int AttackCounter
    {
        get => AnomalyNPC.AnomalyAI32[4].I;
        set
        {
            if (AnomalyNPC.AnomalyAI32[4].I != value)
            {
                AnomalyNPC.AnomalyAI32[4].I = value;
                AnomalyNPC.AIChanged32[4] = true;
            }
        }
    }

    public int AttackCounter2
    {
        get => AnomalyNPC.AnomalyAI32[5].I;
        set
        {
            if (AnomalyNPC.AnomalyAI32[5].I != value)
            {
                AnomalyNPC.AnomalyAI32[5].I = value;
                AnomalyNPC.AIChanged32[5] = true;
            }
        }
    }

    public float Phase3ChangeRatio
    {
        get => AnomalyNPC.AnomalyAI32[6].F;
        set
        {
            float temp = Math.Clamp(value, 0f, 1f);
            if (AnomalyNPC.AnomalyAI32[6].F != temp)
            {
                AnomalyNPC.AnomalyAI32[6].F = temp;
                AnomalyNPC.AIChanged32[6] = true;
            }
        }
    }

    public int UsedEyeIndex1
    {
        get => AnomalyNPC.AnomalyAI32[7].Byte0;
        set
        {
            byte temp = (byte)TOMathUtils.NormalizeWithPeriod(value, 32);
            if (AnomalyNPC.AnomalyAI32[7].Byte0 != temp)
            {
                AnomalyNPC.AnomalyAI32[7].Byte0 = temp;
                AnomalyNPC.AIChanged32[7] = true;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NPC.netUpdate = true;
            }
        }
    }

    public int UsedEyeIndex2
    {
        get => AnomalyNPC.AnomalyAI32[7].Byte1;
        set
        {
            byte temp = (byte)(value % 32);
            if (AnomalyNPC.AnomalyAI32[7].Byte1 != temp)
            {
                AnomalyNPC.AnomalyAI32[7].Byte1 = temp;
                AnomalyNPC.AIChanged32[7] = true;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NPC.netUpdate = true;
            }
        }
    }

    public int UsedEyeIndex3
    {
        get => AnomalyNPC.AnomalyAI32[7].Byte2;
        set
        {
            byte temp = (byte)(value % 32);
            if (AnomalyNPC.AnomalyAI32[7].Byte2 != temp)
            {
                AnomalyNPC.AnomalyAI32[7].Byte2 = temp;
                AnomalyNPC.AIChanged32[7] = true;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NPC.netUpdate = true;
            }
        }
    }

    public int UsedEyeIndex4
    {
        get => AnomalyNPC.AnomalyAI32[7].Byte3;
        set
        {
            byte temp = (byte)(value % 32);
            if (AnomalyNPC.AnomalyAI32[7].Byte3 != temp)
            {
                AnomalyNPC.AnomalyAI32[7].Byte3 = temp;
                AnomalyNPC.AIChanged32[7] = true;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NPC.netUpdate = true;
            }
        }
    }

    public Vector2 Phase3ArenaCenter
    {
        get => AnomalyNPC.AnomalyAI64[0].GetValue<Vector2>();
        set
        {
            if (AnomalyNPC.AnomalyAI64[0].GetValue<Vector2>() != value)
            {
                AnomalyNPC.AnomalyAI64[0].SetValue(value);
                AnomalyNPC.AIChanged64[0] = true;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NPC.netUpdate = true;
            }
        }
    }

    #region 仆从
    public bool ServantLeftSpawned
    {
        get => AnomalyNPC.AnomalyAI32[0].Bits[2];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].Bits[2] != value)
            {
                AnomalyNPC.AnomalyAI32[0].Bits[2] = value;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }
    /// <summary>
    /// 左仆从实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <c>DummyNPC</c>。
    /// </summary>
    public NPC ServantLeft
    {
        get => NPC.GetNPCFromIndex(AnomalyNPC.AnomalyAI32[1].Byte0);
        set
        {
            byte temp = (byte)(value?.whoAmI ?? Main.maxNPCs);
            if (AnomalyNPC.AnomalyAI32[1].Byte0 != temp)
            {
                AnomalyNPC.AnomalyAI32[1].Byte0 = temp;
                AnomalyNPC.AIChanged32[1] = true;
            }
        }
    }
    public bool ServantLeftAlive => ServantLeft.active && ServantLeft.ModNPC is BloodlettingServant && ServantLeft.Master == NPC;

    public bool ServantRightSpawned
    {
        get => AnomalyNPC.AnomalyAI32[0].Bits[3];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].Bits[3] != value)
            {
                AnomalyNPC.AnomalyAI32[0].Bits[3] = value;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }
    /// <summary>
    /// 右仆从实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <c>DummyNPC</c>。
    /// </summary>
    public NPC ServantRight
    {
        get => NPC.GetNPCFromIndex(AnomalyNPC.AnomalyAI32[1].Byte1);
        set
        {
            byte temp = (byte)(value?.whoAmI ?? Main.maxNPCs);
            if (AnomalyNPC.AnomalyAI32[1].Byte1 != temp)
            {
                AnomalyNPC.AnomalyAI32[1].Byte1 = temp;
                AnomalyNPC.AIChanged32[1] = true;
            }
        }
    }
    public bool ServantRightAlive => ServantRight.active && ServantRight.ModNPC is BloodlettingServant && ServantRight.Master == NPC;

    public bool ArenaProjectileSpawned
    {
        get => AnomalyNPC.AnomalyAI32[0].Bits[4];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].Bits[4] != value)
            {
                AnomalyNPC.AnomalyAI32[0].Bits[4] = value;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }
    /// <summary>
    /// 竞技场弹幕实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <c>DummyProjectile</c>。
    /// </summary>
    public Projectile ArenaProjectile
    {
        get => AnomalyNPC.GetProjectileReference(1);
        set => AnomalyNPC.SetProjectileReference(NPC, 1, value);
    }
    public bool ArenaProjectileAlive => ArenaProjectile.active && ArenaProjectile.ModProjectile is EyeofCthulhuArena arena && arena.Master == NPC;
    public EyeofCthulhuArena ArenaModProjectile => ArenaProjectile.GetModProjectile<EyeofCthulhuArena>();
    #endregion 仆从

    /* 数组使用说明
     * 
     * NPC.ai
     *   [0]. (Union)
     *       byte0 CurrentPhase
     *       byte1 CurrentBehavior
     *   [1] CurrentLocalPhase
     * 
     * AnomalyAI32
     *   [0].
     *       bits[0] Hover2DirectionIsNegative
     *       bits[1] NextChargeTypeIsHorizontal
     *       bits[2] ServantLeftSpawned
     *       bits[3] ServantRightSpawned
     *       bits[4] ArenaProjectileSpawned
     *   [1].
     *       byte0 ServantLeft
     *       byte1 ServantRight
     *   [2] 保留（ArenaProjectile 改用 AI64 网络引用）
     *   [3].i ServantSpawnCounter
     *   [4].i AttackCounter
     *   [5].i AttackCounter2
     *   [6].f Phase3ChangeRatio
     *   [7].
     *       byte0 UsedEyeIndex1
     *       byte1 UsedEyeIndex2
     *       byte2 UsedEyeIndex3
     *       byte3 UsedEyeIndex4
     * 
     * AnomalyAI64
     *   [0] (Vector2) Phase3ArenaCenter
     *   [1] (owner + identity) ArenaProjectile
     */
    #endregion 数据

    public override int ApplyingType => NPCID.EyeofCthulhu;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        _ => true,
    };

    public override void SetDefaults()
    {
        BridgeUtils.ApplyCalamityHealthBoost(NPC);

        ServantLeft = NPC.DummyNPC;
        ServantRight = NPC.DummyNPC;
        ArenaProjectile = Projectile.DummyProjectile;

        AnomalyNPC.DamageReduction_DynamicHandler = new TimedDDRHandler(
            new TimedDDRHandler.SingleDDRHandler(1f, Phase2LifeRatio, null, n => GetInstance(n).CurrentPhase >= Phase.PhaseChange_1To2, 15),
            new TimedDDRHandler.SingleDDRHandler(Phase2LifeRatio, Phase3LifeRatio, null, n => GetInstance(n).CurrentPhase >= Phase.PhaseChange_2To3, 55),
            new TimedDDRHandler.SingleDDRHandler(0.5f, 0f, n => GetInstance(n).Phase3, null, 30)
        );

        NPC.AddAnomalyHPIndicator(Phase2LifeRatioAnomaly, Phase2LifeRatioUltra);
        NPC.AddAnomalyHPIndicator(Phase22LifeRatioAnomaly, Phase22LifeRatioUltra, true);
        NPC.AddAnomalyHPIndicator(Phase23LifeRatioAnomaly, Phase23LifeRatioUltra, true);
        NPC.AddAnomalyHPIndicator(Phase3LifeRatioAnomaly, Phase3LifeRatioUltra);
        NPC.AddAnomalyHPIndicator(Phase32LifeRatioAnomaly, Phase32LifeRatioUltra, true, n => GetInstance(n).Phase3);
    }

    public override void SetDefaultsFinal()
    {
        NPC.lifeMax = CalamityEnabled ? 6000 : 4500;
        BridgeUtils.ApplyCalamityHealthBoost(NPC);
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) =>
        AnomalyUtils.ApplyAnomalyBossDifficultyAndPlayerScaling(NPC, bossAdjustment);

    public override void FindFrame(int frameHeight)
    {
        int frameNum;
        ref double frameCounter = ref OceanNPC.FrameCounter;

        frameCounter += 1.0;

        frameNum = (int)(frameCounter / 7.0);
        if (frameNum >= 3)
        {
            frameCounter = 0.0;
            frameNum = 0;
        }

        bool shouldUsePhase2Frame = CurrentPhase switch
        {
            Phase.Phase1 => false,
            Phase.PhaseChange_1To2 => Timer1 >= PhaseChangeGateValue1To22,
            _ => true
        };

        if (shouldUsePhase2Frame)
            frameNum += 3;

        NPC.frame.Y = frameNum * frameHeight;
    }

    public override Color? GetAlpha(Color drawColor)
    {
        if (Phase3ChangeRatio > 0f)
            return Color.Lerp(drawColor, Phase3Color, Phase3ChangeRatio * 0.6f) with { A = NPC.GraphicAlpha };

        return null;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D npcTexture = NPC.Texture;
        Color originalColor = NPC.GetAlpha(drawColor);
        Rectangle frame = NPC.frame;
        spriteBatch.DrawFromCenter(npcTexture, NPC.Center + DrawOffset - screenPos, frame, originalColor * NPC.Opacity, NPC.rotation, NPC.scale);
        return false;
    }

    public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
    {
        if (!Ultra)
            return;

        if (Phase3) //在竞技场内获得10%易伤，否则获得85%减伤
        {
            float damageMultiplier = IsInPhase3Arena ? 1.1f : 0.15f;
            modifiers.SourceDamage *= damageMultiplier;
        }
        else if (Ultra)
            modifiers.SetMaxDamage((int)(NPC.life - NPC.lifeMax * Phase3LifeRatio + 1));
    }

    public override bool CheckDead()
    {
        if (Ultra && !Phase3 && !NPC.downedBoss1)
        {
            NPC.life = 1;
            NPC.active = true;
            NPC.netUpdate = true;
            return false;
        }

        return true;
    }

    public override void BossHeadSlot(ref int index)
    {
        if (Phase3 && (!IsInPhase3Arena || NPC.Opacity < 0.2f))
            index = -1;
    }

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => Phase3 && (!IsInPhase3Arena || NPC.Opacity < 0.2f) ? false : null;

    public override bool PreHoverInteract(bool mouseIntersects)
    {
        if (Phase3 && (!IsInPhase3Arena || NPC.Opacity < 0.2f))
            return false;

        return true;
    }

    public override void ApplyCustomMainBossBarShader(BossHealthBar newBar, SpriteBatch spriteBatch, Rectangle destinationRentangle)
    {
        AnomalyEffects.BossBars.EyeofCthulhu.Data
            .UseImage1(AnomalyTextures.Noise._Vein)
            .SetCustomParameter("uScreenResolution", Main.ScreenSize.ToVector2() * Math.Max(Main.UIScale, 1f) / 2f)
            .SetCustomParameter("uPosition", destinationRentangle.TopLeft())
            .Apply();
    }
}
