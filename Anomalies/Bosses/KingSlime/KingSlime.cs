// Developed by ColdsUx

using Anomalies.DataStructures;

namespace Anomalies.Bosses.KingSlime;

public sealed partial class KingSlime : AnomalyNPCBehavior<KingSlime>, ILocalizationPrefix
{
    #region 数据
    public enum Phase : byte
    {
        Initialize = 0,
        Phase1,
        PhaseChange_1To2,
        Phase2,
        Phase2_2,
    }

    public enum Behavior : byte
    {
        Despawn = byte.MaxValue,

        None = 0,

        FirstJump,
        NormalJump,
        HighJump,
        RapidJump,
        Teleport,

        PhaseChange_1To2,
    }

    public const float DespawnDistance = 5000f;
    public static float MaxScale => Aroma ? 6f : Ultra ? 4.5f : 3f;
    public static float MinScale => Aroma ? 0.5f : 1f;
    public static float SpawnSlimeDistance => Aroma ? 0.01f : 0.05f;
    public static float SpawnSlimePow => Aroma ? 0.5f : Ultra ? 0.3f : 0.2f;

    public const float JewelRubyLifeRatio_Anomaly = 0.7f;
    public const float JewelRubyLifeRatio_Ultra = 0.8f;
    public const float JewelEmeraldLifeRatio_Anomaly = 0.5f;
    public const float JewelEmeraldLifeRatio_Ultra = 0.6f;
    public const float JewelSapphireLifeRatio_Anomaly = 0.3f;
    public const float JewelSapphireLifeRatio_Ultra = 0.4f;
    public const float Phase2LifeRatio_Anomaly = 0f;
    public const float Phase2LifeRatio_Ultra = 0.1f;
    public const float Phase2_2LifeRatio_Anomaly = 0f;
    public const float Phase2_2LifeRatio_Ultra = 0.25f;

    public static float JewelRubyLifeRatio => Ultra ? JewelRubyLifeRatio_Ultra : JewelRubyLifeRatio_Anomaly;
    public static float JewelEmeraldLifeRatio => Ultra ? JewelEmeraldLifeRatio_Ultra : JewelEmeraldLifeRatio_Anomaly;
    public static float JewelSapphireLifeRatio => Ultra ? JewelSapphireLifeRatio_Ultra : JewelSapphireLifeRatio_Anomaly;
    public static float Phase2LifeRatio => Ultra ? Phase2LifeRatio_Ultra : Phase2LifeRatio_Anomaly;
    public static float Phase2_2LifeRatio => Ultra ? Phase2_2LifeRatio_Ultra : Phase2_2LifeRatio_Anomaly;

    private static readonly ProjectileDamageContainer _GelDamage = new(40, 60, 90, 120, 120, 150);
    public static int GelDamage => _GelDamage.Value;

    public static int JumpDelay => Ultra ? 16 : 20;

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

    public int CurrentAttackPhase
    {
        get => (int)NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    public int LastSpawnSlimeLife
    {
        get => (int)NPC.ai[2];
        set => NPC.ai[2] = value;
    }

    #region 宝石
    public bool JewelRubySpawned
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
    /// <summary>
    /// 王冠红宝石实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <c>DummyNPC</c>。
    /// </summary>
    public NPC JewelRuby
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
    public bool JewelRubyAlive => JewelRuby.active && JewelRuby.ModNPC is RubyJewel && JewelRuby.Master == NPC;
    public bool JewelRubyDead => JewelRubySpawned && !JewelRubyAlive;

    public bool JewelEmeraldSpawned
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
    /// <summary>
    /// 王冠绿宝石实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <c>DummyNPC</c>。
    /// </summary>
    public NPC JewelEmerald
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
    public bool JewelEmeraldAlive => JewelEmerald.active && JewelEmerald.ModNPC is EmeraldJewel && JewelEmerald.Master == NPC;
    public bool JewelEmeraldDead => JewelEmeraldSpawned && !JewelEmeraldAlive;

    public bool JewelSapphireSpawned
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
    /// 王冠蓝宝石实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <c>DummyNPC</c>。
    /// </summary>
    public NPC JewelSapphire
    {
        get => NPC.GetNPCFromIndex(AnomalyNPC.AnomalyAI32[1].byte2);
        set
        {
            byte temp = (byte)(value?.whoAmI ?? Main.maxNPCs);
            if (AnomalyNPC.AnomalyAI32[1].byte2 != temp)
            {
                AnomalyNPC.AnomalyAI32[1].byte2 = temp;
                AnomalyNPC.AIChanged32[1] = true;
            }
        }
    }
    public bool JewelSapphireAlive => JewelSapphire.active && JewelSapphire.ModNPC is SapphireJewel && JewelSapphire.Master == NPC;
    public bool JewelSapphireDead => JewelSapphireSpawned && !JewelSapphireAlive;
    public bool HasSapphireBuff => JewelSapphireAlive && !JewelHandler.CheckIfPhase2(JewelSapphire);

    public bool JewelRainbowSpawned
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
    /// 王冠蓝宝石实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <c>DummyNPC</c>。
    /// </summary>
    public NPC JewelRainbow
    {
        get => NPC.GetNPCFromIndex(AnomalyNPC.AnomalyAI32[1].byte3);
        set
        {
            byte temp = (byte)(value?.whoAmI ?? Main.maxNPCs);
            if (AnomalyNPC.AnomalyAI32[1].byte3 != temp)
            {
                AnomalyNPC.AnomalyAI32[1].byte3 = temp;
                AnomalyNPC.AIChanged32[1] = true;
            }
        }
    }
    public bool JewelRainbowAlive => JewelRainbow.active && JewelRainbow.ModNPC is RainbowJewel && JewelRainbow.Master == NPC;
    public bool JewelRainbowDead => JewelRainbowSpawned && !JewelRainbowAlive;
    #endregion 宝石

    public float TeleportTimer
    {
        get => AnomalyNPC.AnomalyAI32[2].f;
        set
        {
            if (AnomalyNPC.AnomalyAI32[2].f != value)
            {
                AnomalyNPC.AnomalyAI32[2].f = value;
                AnomalyNPC.AIChanged32[2] = true;
            }
        }
    }

    public int SmallJumpCounter
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

    public int DirectionChangeCounter
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

    public float DespawnScaleMultiplier
    {
        get => AnomalyNPC.AnomalyAI32[5].f;
        set
        {
            float temp = Math.Clamp(value, 0f, 1f);
            if (AnomalyNPC.AnomalyAI32[5].f != temp)
            {
                AnomalyNPC.AnomalyAI32[5].f = temp;
                AnomalyNPC.AIChanged32[5] = true;
            }
        }
    }

    public float TeleportScaleMultiplier
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

    public float SapphireBuffRatio
    {
        get => AnomalyNPC.AnomalyAI32[7].f;
        set
        {
            float temp = Math.Clamp(value, 0f, 1f);
            if (AnomalyNPC.AnomalyAI32[7].f != temp)
            {
                AnomalyNPC.AnomalyAI32[7].f = temp;
                AnomalyNPC.AIChanged32[7] = true;
            }
        }
    }

    public float RainbowRatio
    {
        get => AnomalyNPC.AnomalyAI32[8].f;
        set
        {
            float temp = Math.Clamp(value, 0f, 1f);
            if (AnomalyNPC.AnomalyAI32[8].f != temp)
            {
                AnomalyNPC.AnomalyAI32[8].f = temp;
                AnomalyNPC.AIChanged32[8] = true;
            }
        }
    }

    public float PhaseChangeLifeRatio
    {
        get => AnomalyNPC.AnomalyAI32[9].f;
        set
        {
            if (AnomalyNPC.AnomalyAI32[9].f != value)
            {
                AnomalyNPC.AnomalyAI32[9].f = value;
                AnomalyNPC.AIChanged32[9] = true;
            }
        }
    }

    public Vector2 TeleportDestination
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
     *   [1] CurrentAttackPhase
     *   [1] LastSpawnSlimeLife
     * 
     * AnomalyAI32
     *   [0].
     *       bits[0] JewelRubySpawned
     *       bits[1] JewelEmeraldSpawned
     *       bits[2] JewelSapphireSpawned
     *       bits[3] JewelRainbowSpawned
     *   [1].
     *       byte0 JewelRuby
     *       byte1 JewelEmerald
     *       byte2 JewelSapphire
     *       byte3 JewelRainbow
     *   [2].f TeleportTimer
     *   [3].i SmallJumpCounter
     *   [4].i DirectionChangeCounter
     *   [5].f DespawnScaleMultiplier
     *   [6].f TeleportScaleMultiplier
     *   [7].f SapphireBuffRatio
     *   [8].f RainbowRatio
     *   [9].f PhaseChangeLifeRatio
     * 
     * AnomalyAI64
     *   [0] (Vector2) TeleportDestination
     */
    #endregion 数据

    public string LocalizationPrefix => AnomalySharedData.BossLocalizationPrefix + "KingSlime";

    public override int ApplyingType => NPCID.KingSlime;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        CalamityLogicType_NPCBehavior.GetAlpha => false,
        _ => true,
    };

    public override void SetDefaults()
    {
        NPC.lifeMax = CalamityEnabled ? 3000 : 2400;
        BridgeUtils.ApplyCalamityHealthBoost(NPC);

        TeleportScaleMultiplier = 1f;
        DespawnScaleMultiplier = 1f;

        JewelRuby = NPC.DummyNPC;
        JewelEmerald = NPC.DummyNPC;
        JewelSapphire = NPC.DummyNPC;

        AnomalyNPC.DynamicDRHandler = new TimedDDRHandler(
            new TimedDDRHandler.SingleDDRHandler(1f, Phase2LifeRatio, null, n => GetInstance(n).CurrentPhase >= Phase.PhaseChange_1To2, 60),
            new TimedDDRHandler.SingleDDRHandler(0.5f, 0f, n => GetInstance(n).Phase2, null, 30)
        );

        NPC.AddAnomalyHPIndicator(JewelRubyLifeRatio_Anomaly, JewelRubyLifeRatio_Ultra, true);
        NPC.AddAnomalyHPIndicator(JewelEmeraldLifeRatio_Anomaly, JewelEmeraldLifeRatio_Ultra, true);
        NPC.AddAnomalyHPIndicator(JewelSapphireLifeRatio_Anomaly, JewelSapphireLifeRatio_Ultra, true);
        NPC.AddAnomalyHPIndicator(Phase2LifeRatio_Anomaly, Phase2LifeRatio_Ultra);
        NPC.AddAnomalyHPIndicator(Phase2_2LifeRatio_Anomaly, Phase2_2LifeRatio_Ultra, true, n => GetInstance(n).Phase2);
    }

    public override bool CheckActive() => false;

    public override Color? GetAlpha(Color drawColor)
    {
        Color newColor = Color.Lerp(new Color(0, 0, 150, NPC.alpha), new Color(125, 125, 255, NPC.alpha), TOMathUtils.TimeWrappingFunction.GetTimeSin(0.35f, 1.5f, unsigned: true) + 0.3f);
        Color preRainbow = Color.Lerp(Aroma ? new Color(125, 125, 255, NPC.alpha) : drawColor, newColor, SapphireBuffRatio);

        if (Main.remixWorld || Aroma)
        {
            byte r = preRainbow.R;
            byte g = preRainbow.G;
            byte b = preRainbow.B;
            byte a = preRainbow.A;
            preRainbow = new Color(b, b, (r + g) / 2, a);
        }

        return Color.Lerp(preRainbow, Main.DiscoColor, RainbowRatio) with { A = NPC.GraphicAlpha };
    }

    public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
    {
        if (Ultra && !Phase2)
            modifiers.SetMaxDamage((int)(NPC.life - NPC.lifeMax * Phase2LifeRatio));
    }

    public override bool CheckDead()
    {
        if (Ultra && !Phase2 && !NPC.downedSlimeKing)
        {
            NPC.life = 1;
            NPC.active = true;
            NPC.netUpdate = true;
            return false;
        }

        return true;
    }

    public override void OnKill()
    {
        if (JewelRubyAlive)
            JewelHandler.GetKingSlimeJewel(JewelRuby)?.MasterDead = true;
        if (JewelEmeraldAlive)
            JewelHandler.GetKingSlimeJewel(JewelEmerald)?.MasterDead = true;
        if (JewelSapphireAlive)
            JewelHandler.GetKingSlimeJewel(JewelSapphire)?.MasterDead = true;
        if (JewelRainbowAlive)
            JewelHandler.GetKingSlimeJewel(JewelRainbow)?.MasterDead = true;
    }
}
