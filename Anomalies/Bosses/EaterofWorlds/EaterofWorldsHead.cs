using Anomalies.Assets.Effects;
using Anomalies.DataStructures;
using Anomalies.Visuals.BossBar;

namespace Anomalies.Bosses.EaterofWorlds;

public sealed partial class EaterofWorldsHead : EaterofWorldsSegment<EaterofWorldsHead>
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

        Phase1_Normal,
        Phase1_Split,
        Phase1_Combine,
    }

    public const string AnomalyEaterofWorldsPath = AnomalySharedData.ModPath + "Bosses/EaterofWorlds/";

    public const float DespawnDistance = 8000f;

    public static int BodyCount => Ultra ? 88 : 58;
    public static int TotalSegmentCount => BodyCount + 2;

    public const float Phase1_2LifeRatio_Anomaly = 0.5f;
    public const float Phase1_2LifeRatio_Ultra = 0.55f;
    public const float Phase2LifeRatio_Anomaly = 0f;
    public const float Phase2LifeRatio_Ultra = 0.1f;

    public static float Phase1_2LifeRatio => Ultra ? Phase1_2LifeRatio_Ultra : Phase1_2LifeRatio_Anomaly;
    public static float Phase2LifeRatio => Ultra ? Phase2LifeRatio_Ultra : Phase2LifeRatio_Anomaly;

    private static readonly ProjectileDamageContainer _CursedFireBallDamage = new(50, 80, 120, 150, 144, 180);
    public static int CursedFireballDamage => _CursedFireBallDamage.Value;

    public static Dictionary<int, List<EaterofWorldsSegmentContainer>> AllSegmentsDictionary = [];
    public List<EaterofWorldsSegmentContainer> AllSegments => AllSegmentsDictionary[NPC.whoAmI];

    public Phase CurrentPhase
    {
        get => (Phase)AI_Union_2.byte0;
        set
        {
            Union32 union = AI_Union_2;
            union.byte0 = (byte)value;
            AI_Union_2 = union;
        }
    }

    public bool ShouldEnterPhase2 => Ultra && NPC.LifeRatio < Phase2LifeRatio;
    public bool InvalidPhase1 => ShouldEnterPhase2 && !Phase2;
    public bool Phase1_2 => CurrentPhase == Phase.Phase1_2;
    public bool Phase2 => CurrentPhase is Phase.Phase2 or Phase.Phase2_2;
    public bool Phase2_2 => CurrentPhase == Phase.Phase2_2;

    public Behavior CurrentBehavior
    {
        get => (Behavior)AI_Union_2.byte1;
        set
        {
            Union32 union = AI_Union_2;
            union.byte1 = (byte)value;
            AI_Union_2 = union;
        }
    }

    public int CurrentAttackPhase
    {
        get => (int)NPC.ai[3];
        set => NPC.ai[3] = value;
    }

    public bool CurrentlySplit
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

    public float CoilingStartAngle
    {
        get => AnomalyNPC.AnomalyAI32[1].f;
        set
        {
            float temp = TOMathUtils.NormalizeWithPeriod(value);
            if (AnomalyNPC.AnomalyAI32[1].f != temp)
            {
                AnomalyNPC.AnomalyAI32[1].f = temp;
                AnomalyNPC.AIChanged32[1] = true;
            }
        }
    }

    public int TransformAmount
    {
        get => AnomalyNPC.AnomalyAI32[2].i;
        set
        {
            if (AnomalyNPC.AnomalyAI32[2].i != value)
            {
                AnomalyNPC.AnomalyAI32[2].i = value;
                AnomalyNPC.AIChanged32[2] = true;
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
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
    {
        const float VanillaExpertLifeMultiplier = 1.4f;
        NPC.lifeMax = (int)MathF.Round(NPC.lifeMax / (VanillaExpertLifeMultiplier * 1.5f) / bossAdjustment / 10f) * 10;
    }

    public override bool CheckActive() => false;

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        spriteBatch.DrawFromCenter(NPC.Texture, NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, NPC.scale);
        return false;
    }

    public override void ApplyCustomMainBossBarShader(BossHealthBar newBar, SpriteBatch spriteBatch, Rectangle destinationRentangle)
    {
        AnomalyEffects.CustomBossBars.EaterofWorlds.Data
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
        if (!IsFirstSegment) //只有真正的头部能执行转换操作
            return [];

        int segmentCount = TotalSegmentCount;

        if (amount <= 0 || amount > segmentCount || segmentCount % amount != 0)
            return [];

        int segmentPerSplit = segmentCount / amount;

        if (segmentCount < 2)
            return [];

        List<NPC> heads = [];

        for (int i = 0; i < amount; i++)
        {
            int offset = i * segmentPerSplit;

            //转换头部

            (NPC head, IEaterofWorldsSegment segmentHead) = AllSegments[offset];
            segmentHead.LocalHead = head;
            if (head.type != NPCID.EaterofWorldsHead)
            {
                head.type = NPCID.EaterofWorldsHead;
                SoundEngine.PlaySound(SoundID.NPCDeath1, head.Center);
            }
            AllSegments[offset] = new EaterofWorldsSegmentContainer(head);
            heads.Add(head);

            //转换身体

            for (int j = 1; j <= segmentPerSplit - 2; j++)
            {
                int index = offset + j;
                (NPC body, IEaterofWorldsSegment segmentBody) = AllSegments[index];
                segmentBody.LocalHead = head;
                body.type = NPCID.EaterofWorldsBody;
                AllSegments[index] = new EaterofWorldsSegmentContainer(body);
            }

            //转换尾部

            int indexTail = offset + segmentPerSplit - 1;
            (NPC tail, IEaterofWorldsSegment segmentTail) = AllSegments[indexTail];
            segmentTail.LocalHead = head;
            tail.type = NPCID.EaterofWorldsTail;
            AllSegments[indexTail] = new EaterofWorldsSegmentContainer(tail);
        }

        return heads;
    }
}
