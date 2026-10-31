using Anomalies.Assets.Effects;
using Anomalies.Visuals.BossBar;

namespace Anomalies.Bosses.EaterofWorlds;

public sealed partial class EaterofWorldsHead : EaterofWorldsSegment<EaterofWorldsHead>
{
    protected override int AI32Capacity => 5;

    protected override int AI64Capacity => 4;

    #region 数据
    public enum Phase : byte
    {
        Initialize,
        Phase1,
        Phase1_2,
    }

    public enum Behavior : byte
    {
        Despawn = byte.MaxValue,

        None = 0,

        Phase1_Hunt,
        Phase1_BurrowCharge,
        Phase1_Split,
        Phase1_TripleHunt,
        Phase1_CrossSweep,
        Phase1_Combine,
    }

    public const string AnomalyEaterofWorldsPath = AnomalySharedData.ModPath + "Bosses/EaterofWorlds/";

    public const float DespawnDistance = 8000f;

    public static int BodyCount => Ultra ? 88 : 58;
    public static int TotalSegmentCount => BodyCount + 2;

    public const float Phase12LifeRatioAnomaly = 0.5f;
    public const float Phase12LifeRatioUltra = Phase12LifeRatioAnomaly;
    public static float Phase1_2LifeRatio => Phase12LifeRatioAnomaly;

    private static readonly ProjectileDamageContainer _CursedFireBallDamage = new(50, 80, 120, 150, 108, 180);
    public static int CursedFireballDamage => _CursedFireBallDamage.Value;

    public List<EaterofWorldsSegmentContainer> AllSegments => AnomalyNPC.EaterOfWorlds_Segments;

    public Phase CurrentPhase
    {
        get => (Phase)AI_Union_2.Byte0;
        set
        {
            if (CurrentPhase != value && Main.netMode != NetmodeID.MultiplayerClient)
                NPC.netUpdate = true;
            Union32 union = AI_Union_2;
            union.Byte0 = (byte)value;
            AI_Union_2 = union;
        }
    }

    public bool Phase1_2 => CurrentPhase == Phase.Phase1_2;

    public Behavior CurrentBehavior
    {
        get => (Behavior)AI_Union_2.Byte1;
        set
        {
            if (CurrentBehavior != value && Main.netMode != NetmodeID.MultiplayerClient)
                NPC.netUpdate = true;
            Union32 union = AI_Union_2;
            union.Byte1 = (byte)value;
            AI_Union_2 = union;
        }
    }

    public int CurrentLocalPhase
    {
        get => (int)NPC.ai[3];
        set
        {
            if (NPC.ai[3] != value)
            {
                NPC.ai[3] = value;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NPC.netUpdate = true;
            }
        }
    }

    public bool CurrentlySplit
    {
        get => AnomalyNPC.AnomalyAI32[0].Bits[0];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].Bits[0] != value)
            {
                AnomalyNPC.AnomalyAI32[0].Bits[0] = value;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }

    public float CoilingStartAngle
    {
        get => AnomalyNPC.AnomalyAI32[1].F;
        set
        {
            float temp = TOMathUtils.NormalizeWithPeriod(value);
            if (AnomalyNPC.AnomalyAI32[1].F != temp)
            {
                AnomalyNPC.AnomalyAI32[1].F = temp;
                AnomalyNPC.AIChanged32[1] = true;
            }
        }
    }

    public Vector2 CoilingCenter
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
    #endregion 数据

    public override EaterofWorldsSegmentType SegmentType => EaterofWorldsSegmentType.Head;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        _ => true,
    };

    public override void SetDefaultsFinal()
    {
        NPC.lifeMax = CalamityEnabled ? 12000 : 9000;
        BridgeUtils.ApplyCalamityHealthBoost(NPC);
        NPC.defense = 8;
        NPC.AddAnomalyHPIndicator(Phase12LifeRatioAnomaly, Phase12LifeRatioUltra, true);
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) =>
        AnomalyUtils.ApplyAnomalyBossDifficultyAndPlayerScaling(NPC, bossAdjustment);

    public override bool CheckActive() => false;

    public override bool CheckDead()
    {
        if (!IsFirstSegment || !TONetworkEnvironment.NotClient)
            return true;
        // Remove children before vanilla loot/progression examines the remaining Eater segments.
        foreach (EaterofWorldsSegmentContainer segment in AllSegments)
        {
            if (segment.NPC == NPC || !segment.NPC.active)
                continue;
            segment.NPC.life = 0;
            segment.NPC.active = false;
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.SyncNPC, number: segment.NPC.whoAmI);
        }
        return true;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        spriteBatch.DrawFromCenter(NPC.Texture, NPC.Center - screenPos, NPC.frame,
            drawColor, NPC.rotation, NPC.scale);
        return false;
    }

    public override void ApplyCustomMainBossBarShader(BossHealthBar newBar, SpriteBatch spriteBatch, Rectangle destinationRentangle)
    {
        AnomalyEffects.BossBars.EaterofWorlds.Data
            .UseImage1(AnomalyTextures.Noise._Smear)
            .UseImage2(AnomalyTextures.Noise._Vein)
            .SetCustomParameter("uScreenResolution", Main.ScreenSize.ToVector2() * Math.Max(Main.UIScale, 1f) / 2f)
            .SetCustomParameter("uPosition", destinationRentangle.TopLeft())
            .Apply();
    }

    /// <summary>
    /// 将 Boss 转换为指定数量的独立蠕虫。
    /// </summary>
    /// <param name="amount">
    /// 要转换的独立蠕虫数量。
    /// <br/>若 amount 为 1，则该方法将 Boss 转换为一个完整蠕虫。
    /// </param>
    /// <returns>由所有转换后的独立蠕虫的头部组成的列表，按顺序排列。</returns>
    public List<NPC> TransformToWorms(int amount)
    {
        if (!TONetworkEnvironment.NotClient || !IsFirstSegment) //只有服务端的真正头部能执行转换操作
            return [];

        NPC owner = NPC;
        List<EaterofWorldsSegmentContainer> segments = AllSegments;
        int count = segments.Count;
        // Validate the entire graph before mutating any entity; a partial conversion cannot be recovered safely.
        if (amount <= 0 || count != TotalSegmentCount || count % amount != 0 || count / amount < 2
            || segments.Any(s => !s.NPC.active || !s.NPC.EoW))
            return [];

        for (int i = 0; i < count; i++)
        {
            IEaterofWorldsSegment segment = segments[i].SegmentBehavior;
            if (segment.Head != owner || segment.SegmentIndex != i + 1)
                return [];
        }

        EaterofWorldsTopology.Link[] links = EaterofWorldsTopology.Create(count, amount);
        int length = count / amount;
        List<NPC> heads = [];
        for (int i = 0; i < count; i++)
        {
            NPC segment = segments[i].NPC;
            int offset = i % length;
            EaterofWorldsTopology.Link link = links[i];
            NPC localHead = segments[link.Head].NPC;
            int type = offset == 0 ? NPCID.EaterofWorldsHead
                : offset == length - 1 ? NPCID.EaterofWorldsTail : NPCID.EaterofWorldsBody;
            // Do not SetDefaults: it would reset health, global data and the shared-life relationship.
            segment.type = segment.netID = type;
            IEaterofWorldsSegment behavior = EaterofWorldsHandler.TryGetSegment(segment);
            behavior.Head = owner;
            behavior.LocalHead = localHead;
            behavior.SegmentIndex = i + 1;
            behavior.Previous = link.Previous < 0 ? null : segments[link.Previous].NPC;
            behavior.Next = link.Next < 0 ? null : segments[link.Next].NPC;
            segment.realLife = i == 0 ? -1 : owner.whoAmI;
            segment.target = owner.target;
            if (i != 0)
            {
                segment.lifeMax = owner.lifeMax;
                segment.life = owner.life;
                segment.boss = false;
            }
            segment.ai[2] = owner.ai[2];
            segment.ai[3] = owner.ai[3];
            segment.Timer1 = owner.Timer1;
            segment.Timer2 = owner.Timer2;
            segment.Timer3 = owner.Timer3;
            segment.Anomaly.AnomalyAI32[0].Bits[0] = amount > 1;
            segment.Anomaly.AnomalyAI32[0].Bits[1] = true; // initialized trajectory, independent of form
            segment.Anomaly.AIChanged32[0] = true;
            segment.damage = 0;
            segment.defense = 8;
            Array.Fill(segment.oldPos, segment.position);
            Array.Fill(segment.oldRot, segment.rotation);
            segment.netUpdate = true;
            if (offset == 0)
                heads.Add(segment);
        }
        CurrentlySplit = amount > 1;
        return heads;
    }
}
