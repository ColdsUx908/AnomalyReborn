// Developed by ColdsUx

using CalamityAnomalies.DataStructures;

namespace CalamityAnomalies.Anomaly.EyeofCthulhu;

public sealed partial class EyeofCthulhu : AnomalyNPCBehavior<EyeofCthulhu>
{
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
    public const int PhaseChangeTime_1To2 = 150;
    public const int PhaseChangeGateValue_1To2_1 = PhaseChangeTime_1To2 / 5 * 2;
    public const int PhaseChangeGateValue_1To2_2 = PhaseChangeTime_1To2 / 5 * 3;
    public const int PhaseChangeTime_2To3 = 195;
    public const int PhaseChangeGateValue_2To3_1 = 75;
    public const int PhaseChangeGateValue_2To3_2 = 120;

    public const float Phase2LifeRatio_Anomaly = 0.75f;
    public const float Phase2LifeRatio_Ultra = 0.8f;
    public const float Phase2_2LifeRatio_Anomaly = 0.5f;
    public const float Phase2_2LifeRatio_Ultra = 0.6f;
    public const float Phase2_3LifeRatio_Anomaly = 0.25f;
    public const float Phase2_3LifeRatio_Ultra = 0.35f;
    public const float Phase3LifeRatio_Anomaly = 0f;
    public const float Phase3LifeRatio_Ultra = 0.1f;
    public const float Phase3_2LifeRatio_Anomaly = 0f;
    public const float Phase3_2LifeRatio_Ultra = 0.25f;

    public static float Phase2LifeRatio => Ultra ? Phase2LifeRatio_Ultra : Phase2LifeRatio_Anomaly;
    public static float Phase2_2LifeRatio => Ultra ? Phase2_2LifeRatio_Ultra : Phase2_2LifeRatio_Anomaly;
    public static float Phase2_3LifeRatio => Ultra ? Phase2_3LifeRatio_Ultra : Phase2_3LifeRatio_Anomaly;
    public static float Phase3LifeRatio => Ultra ? Phase3LifeRatio_Ultra : Phase3LifeRatio_Anomaly;
    public static float Phase3_2LifeRatio => Ultra ? Phase3_2LifeRatio_Ultra : Phase3_2LifeRatio_Anomaly;

    public static float TeleportOffset => 150f;

    public static readonly SoundStyle Roar = SoundID.Roar with { MaxInstances = 0 };
    public static readonly SoundStyle ForceRoar = SoundID.ForceRoarPitched with { MaxInstances = 0 };

    private static readonly ProjectileDamageContainer _bloodDamage = new(30, 60, 75, 90, 120, 150);
    public static int BloodDamage => _bloodDamage.Value;

    private static readonly ProjectileDamageContainer _arenaDamage = new(30, 60, 90, 90, 90, 150);
    public static int ArenaDamage => _arenaDamage.Value;

    private static readonly ProjectileDamageContainer _bloodFlameDamage = new(50, 80, 102, 120, 180, 210);
    public static int BloodFlameDamage => _bloodFlameDamage.Value;

    public static readonly Color Phase3Color = Color.Lerp(Color.DarkRed, Color.Tomato, 0.4f);

    public float DamageMultiplier => Phase3 ? 1.5f : Phase2_2 ? 1.25f : 1f;

    public int SetDamage => (int)Math.Round(NPC.defDamage * DamageMultiplier);
    public int ReducedSetDamage => (int)Math.Round(NPC.defDamage * DamageMultiplier * 0.6f);

    public bool IsInPhase3Arena => Phase3 && ArenaProjectileAlive && NPC.Distance(ArenaProjectile.Center) < ArenaProjectile.GetModProjectile<EyeofCthulhuArena>().Radius + 30f;

    public float EyeRotation => TOMathUtils.NormalizeWithPeriod((Target.Center - NPC.Center).ToRotation(-MathHelper.PiOver2));
    public float ActualRotation => NPC.rotation + MathHelper.PiOver2;
    public Vector2 DrawOffset => -new PolarVector2(24f, ActualRotation);

    public int RapidChargeTime => Ultra ? (Phase2_3 ? 11 : 13) : (Phase2_3 ? 12 : 15);

    public int HorizontalChargeTime => Phase2_3 ? 30 : 35;

    public static readonly UnaryFunctionWithDomain PhaseChange_1To2_RotationSpeedFunction = UnaryFunctionWithDomain.Piecewise(
        (new MathInterval(float.NegativeInfinity, PhaseChangeGateValue_1To2_2, false, false), x => 0.5f * TOMathUtils.Interpolation.QuadraticEaseInOut(x / PhaseChangeGateValue_1To2_1)),
        (new MathInterval(PhaseChangeGateValue_1To2_2, float.PositiveInfinity, true, false), x => 0.5f * TOMathUtils.Interpolation.QuadraticEaseInOut((PhaseChangeTime_1To2 - x) / PhaseChangeGateValue_1To2_1))
    );
    public static readonly UnaryFunctionWithDomain PhaseChange_2To3_RotationSpeedFunction = UnaryFunctionWithDomain.Piecewise(
        (new MathInterval(float.NegativeInfinity, PhaseChangeGateValue_2To3_2, false, false), x => 0.5f * TOMathUtils.Interpolation.QuadraticEaseInOut(x / PhaseChangeGateValue_2To3_1)),
        (new MathInterval(PhaseChangeGateValue_2To3_2, float.PositiveInfinity, true, false), x => 0.5f * TOMathUtils.Interpolation.QuadraticEaseInOut((PhaseChangeTime_2To3 - x) / PhaseChangeGateValue_2To3_1))
        );

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
            return (Behavior)union.byte1;
        }
        set
        {
            Union32 union = AI_Union_0;
            union.byte1 = (byte)value;
            AI_Union_0 = union;
        }
    }

    public int CurrentAttackPhase
    {
        get => (int)NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    public bool Hover2DirectionIsNegative
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

    public int Hover2Direction
    {
        get => Hover2DirectionIsNegative ? -1 : 1;
        set => Hover2DirectionIsNegative = value == -1;
    }

    public bool NextChargeTypeIsHorizontal
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

    public int ServantSpawnCounter
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

    public int AttackCounter2
    {
        get => AnomalyNPC.AnomalyAI32[5].i;
        set
        {
            if (AnomalyNPC.AnomalyAI32[5].i != value)
            {
                AnomalyNPC.AnomalyAI32[5].i = value;
                AnomalyNPC.AIChanged32[5] = true;
            }
        }
    }

    public float Phase3ChangeRatio
    {
        get => AnomalyNPC.AnomalyAI32[6].f;
        set
        {
            float temp = Math.Clamp(value, 0f, 1f);
            if (AnomalyNPC.AnomalyAI32[6].f != temp)
            {
                AnomalyNPC.AnomalyAI32[6].f = temp;
                AnomalyNPC.AIChanged32[6] = true;
            }
        }
    }

    public int UsedEyeIndex1
    {
        get => AnomalyNPC.AnomalyAI32[7].byte0;
        set
        {
            byte temp = (byte)TOMathUtils.NormalizeWithPeriod(value, 32);
            if (AnomalyNPC.AnomalyAI32[7].byte0 != temp)
            {
                AnomalyNPC.AnomalyAI32[7].byte0 = temp;
                AnomalyNPC.AIChanged32[7] = true;
            }
        }
    }

    public int UsedEyeIndex2
    {
        get => AnomalyNPC.AnomalyAI32[7].byte1;
        set
        {
            byte temp = (byte)(value % 32);
            if (AnomalyNPC.AnomalyAI32[7].byte1 != temp)
            {
                AnomalyNPC.AnomalyAI32[7].byte1 = temp;
                AnomalyNPC.AIChanged32[7] = true;
            }
        }
    }

    public int UsedEyeIndex3
    {
        get => AnomalyNPC.AnomalyAI32[7].byte2;
        set
        {
            byte temp = (byte)(value % 32);
            if (AnomalyNPC.AnomalyAI32[7].byte2 != temp)
            {
                AnomalyNPC.AnomalyAI32[7].byte2 = temp;
                AnomalyNPC.AIChanged32[7] = true;
            }
        }
    }

    public int UsedEyeIndex4
    {
        get => AnomalyNPC.AnomalyAI32[7].byte3;
        set
        {
            byte temp = (byte)(value % 32);
            if (AnomalyNPC.AnomalyAI32[7].byte3 != temp)
            {
                AnomalyNPC.AnomalyAI32[7].byte3 = temp;
                AnomalyNPC.AIChanged32[7] = true;
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
            }
        }
    }

    #region 仆从
    public bool ServantLeftSpawned
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
    /// <summary>
    /// 左仆从实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <c>DummyNPC</c>。
    /// </summary>
    public NPC ServantLeft
    {
        get => NPC.GetNPCFromIndex(AnomalyNPC.AnomalyAI32[1].byte0);
        set
        {
            byte temp = (byte)(value?.whoAmI ?? Main.maxNPCs);
            if (AnomalyNPC.AnomalyAI32[1].byte0 != temp)
            {
                AnomalyNPC.AnomalyAI32[1].byte0 = temp;
                AnomalyNPC.AIChanged32[1] = true;
            }
        }
    }
    public bool ServantLeftAlive => ServantLeft.active && ServantLeft.ModNPC is BloodlettingServant && ServantLeft.Master == NPC;

    public bool ServantRightSpawned
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
    /// <summary>
    /// 右仆从实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <c>DummyNPC</c>。
    /// </summary>
    public NPC ServantRight
    {
        get => NPC.GetNPCFromIndex(AnomalyNPC.AnomalyAI32[1].byte1);
        set
        {
            byte temp = (byte)(value?.whoAmI ?? Main.maxNPCs);
            if (AnomalyNPC.AnomalyAI32[1].byte1 != temp)
            {
                AnomalyNPC.AnomalyAI32[1].byte1 = temp;
                AnomalyNPC.AIChanged32[1] = true;
            }
        }
    }
    public bool ServantRightAlive => ServantRight.active && ServantRight.ModNPC is BloodlettingServant && ServantRight.Master == NPC;

    public bool ArenaProjectileSpawned
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
    /// <summary>
    /// 竞技场弹幕实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <c>DummyProjectile</c>。
    /// </summary>
    public Projectile ArenaProjectile
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
    public bool ArenaProjectileAlive => ArenaProjectile.active && ArenaProjectile.ModProjectile is EyeofCthulhuArena arena && arena.Master == NPC;
    public EyeofCthulhuArena ArenaModProjectile => ArenaProjectile.GetModProjectile<EyeofCthulhuArena>();
    #endregion 仆从

    /* 数组使用说明
     * 
     * NPC.ai
     *   [0]. (Union)
     *       byte0 CurrentPhase
     *       byte1 CurrentBehavior
     *   [1] CurrentAttackPhase
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
     *   [2].i ArenaProjectile
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
     */
    #endregion 数据

    public override int ApplyingType => NPCID.EyeofCthulhu;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        _ => true,
    };

    public override void SetStaticDefaults()
    {
        NPCID.Sets.TrailingMode[ApplyingType] = 3;
        NPCID.Sets.TrailCacheLength[ApplyingType] = 5;
    }

    public override void SetDefaults()
    {
        NPC.lifeMax = 3620;
        NPC.ApplyCalamityBossHealthBoost();

        ServantLeft = NPC.DummyNPC;
        ServantRight = NPC.DummyNPC;
        ArenaProjectile = Projectile.DummyProjectile;

        AnomalyNPC.DynamicDRHandler = new TimedDDRHandler(
            new TimedDDRHandler.SingleDDRHandler(1f, Phase2LifeRatio, null, n => GetInstance(n).CurrentPhase >= Phase.PhaseChange_1To2, 15),
            new TimedDDRHandler.SingleDDRHandler(Phase2LifeRatio, Phase3LifeRatio, null, n => GetInstance(n).CurrentPhase >= Phase.PhaseChange_2To3, 55),
            new TimedDDRHandler.SingleDDRHandler(0.5f, 0f, n => GetInstance(n).Phase3, null, 30)
        );

        NPC.AddAnomalyHPIndicator(Phase2LifeRatio_Anomaly, Phase2LifeRatio_Ultra);
        NPC.AddAnomalyHPIndicator(Phase2_2LifeRatio_Anomaly, Phase2_2LifeRatio_Ultra, true);
        NPC.AddAnomalyHPIndicator(Phase2_3LifeRatio_Anomaly, Phase2_3LifeRatio_Ultra, true);
        NPC.AddAnomalyHPIndicator(Phase3LifeRatio_Anomaly, Phase3LifeRatio_Ultra);
        NPC.AddAnomalyHPIndicator(Phase3_2LifeRatio_Anomaly, Phase3_2LifeRatio_Ultra, true, n => GetInstance(n).Phase3);
    }

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
            Phase.PhaseChange_1To2 => Timer1 >= PhaseChangeGateValue_1To2_2,
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
        else if (InvalidPhase2)
            modifiers.SetMaxDamage((int)(NPC.life - NPC.lifeMax * Phase3LifeRatio));
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
}
