// Developed by ColdsUx

using CalamityMod.NPCs.NormalNPCs;

namespace CalamityAnomalies.Anomaly.KingSlime;

public class KingSlime_Anomaly : AnomalyNPCBehavior, ILocalizationPrefix
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

        NormalJump,
        HighJump,
        RapidJump,
        Teleport,

        PhaseChange_1To2,
    }

    public const float DespawnDistance = 5000f;
    public static float MaxScale => Main.zenithWorld ? (TOSharedData.LegendaryMode ? 6f : 5f) : Ultra ? 4.5f : 3f;
    public static float MinScale => Main.zenithWorld ? 0.5f : 1f;
    public static float SpawnSlimeDistance => TOSharedData.LegendaryMode && Main.zenithWorld ? 0.01f : 0.05f;
    public static float SpawnSlimePow => Main.zenithWorld ? 0.5f : Ultra ? 0.3f : 0.2f;

    public const float JewelRubyLifeRatio_Anomaly = 0.7f;
    public const float JewelRubyLifeRatio_Ultra = 0.75f;
    public const float JewelEmeraldLifeRatio_Anomaly = 0.5f;
    public const float JewelEmeraldLifeRatio_Ultra = 0.75f;
    public const float JewelSapphireLifeRatio_Anomaly = 0.3f;
    public const float JewelSapphireLifeRatio_Ultra = 0.5f;
    public const float Phase2LifeRatio_Anomaly = 0f;
    public const float Phase2LifeRatio_Ultra = 0.1f;
    public const float Phase2_2LifeRatio_Anomaly = 0f;
    public const float Phase2_2LifeRatio_Ultra = 0.25f;

    public static float JewelRubyLifeRatio => Ultra ? JewelRubyLifeRatio_Ultra : JewelRubyLifeRatio_Anomaly;
    public static float JewelEmeraldLifeRatio => Ultra ? JewelEmeraldLifeRatio_Ultra : JewelEmeraldLifeRatio_Anomaly;
    public static float JewelSapphireLifeRatio => Ultra ? JewelSapphireLifeRatio_Ultra : JewelSapphireLifeRatio_Anomaly;
    public static float Phase2LifeRatio => Ultra ? Phase2LifeRatio_Ultra : Phase2LifeRatio_Anomaly;
    public static float Phase2_2LifeRatio => Ultra ? Phase2_2LifeRatio_Ultra : Phase2_2LifeRatio_Anomaly;

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

    public float LifeRatio2 => Math.Min(NPC.LifeRatio * 2f, 1f);
    public float LostLifeRatio2 => 1f - LifeRatio2;

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
        get => Main.npc[AnomalyNPC.AnomalyAI32[1].byte0];
        set
        {
            byte temp = (byte)value.whoAmI;
            if (AnomalyNPC.AnomalyAI32[1].byte0 != temp)
            {
                AnomalyNPC.AnomalyAI32[1].byte0 = temp;
                AnomalyNPC.AIChanged32[1] = true;
            }
        }
    }
    public bool JewelRubyAlive => JewelRuby.active && JewelRuby.ModNPC is KingSlimeJewelRuby && JewelRuby.Master == NPC;
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
        get => Main.npc[AnomalyNPC.AnomalyAI32[1].byte1];
        set
        {
            byte temp = (byte)value.whoAmI;
            if (AnomalyNPC.AnomalyAI32[1].byte1 != temp)
            {
                AnomalyNPC.AnomalyAI32[1].byte1 = temp;
                AnomalyNPC.AIChanged32[1] = true;
            }
        }
    }
    public bool JewelEmeraldAlive => JewelEmerald.active && JewelEmerald.ModNPC is KingSlimeJewelEmerald && JewelEmerald.Master == NPC;
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
        get => Main.npc[AnomalyNPC.AnomalyAI32[1].byte2];
        set
        {
            byte temp = (byte)value.whoAmI;
            if (AnomalyNPC.AnomalyAI32[1].byte2 != temp)
            {
                AnomalyNPC.AnomalyAI32[1].byte2 = temp;
                AnomalyNPC.AIChanged32[1] = true;
            }
        }
    }
    public bool JewelSapphireAlive => JewelSapphire.active && JewelSapphire.ModNPC is KingSlimeJewelSapphire && JewelSapphire.Master == NPC;
    public bool JewelSapphireDead => JewelSapphireSpawned && !JewelSapphireAlive;
    public bool HasSapphireBuff => JewelSapphireAlive && !KingSlime_Handler.CheckIfPhase2(JewelSapphire);

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
        get => Main.npc[AnomalyNPC.AnomalyAI32[1].byte3];
        set
        {
            byte temp = (byte)value.whoAmI;
            if (AnomalyNPC.AnomalyAI32[1].byte3 != temp)
            {
                AnomalyNPC.AnomalyAI32[1].byte3 = temp;
                AnomalyNPC.AIChanged32[1] = true;
            }
        }
    }
    public bool JewelRainbowAlive => JewelRainbow.active && JewelRainbow.ModNPC is KingSlimeJewelRainbow && JewelRainbow.Master == NPC;
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

    public string LocalizationPrefix => CASharedData.AnomalyLocalizationPrefix + "KingSlime";

    public override int ApplyingType => NPCID.KingSlime;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior method) => method switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        CalamityLogicType_NPCBehavior.GetAlpha => false,
        _ => true,
    };

    public override void SetDefaults()
    {
        TeleportScaleMultiplier = 1f;
        DespawnScaleMultiplier = 1f;

        JewelRuby = NPC.DummyNPC;
        JewelEmerald = NPC.DummyNPC;
        JewelSapphire = NPC.DummyNPC;
    }

    public override bool CheckActive() => false;

    public override bool PreAI()
    {
        if (CurrentBehavior == Behavior.Despawn || !NPC.TargetClosestIfInvalid(true, DespawnDistance))
        {
            Despawn();
            ChangeScale();
            return false;
        }
        else
            NPC.FaceTarget(Target);

        NPC.noTileCollide = false;
        bool hasSapphireBuff = HasSapphireBuff;
        if (hasSapphireBuff)
            SapphireBuffRatio += 0.02f;
        else
            SapphireBuffRatio -= 0.025f;
        CalamityNPC.CurrentlyIncreasingDefenseOrDR = hasSapphireBuff;
        NPC.defense = JewelSapphireDead ? (int)(NPC.defDefense * 0.75f) : hasSapphireBuff ? (int)(NPC.defDefense * 1.25f) : NPC.defDefense;
        CalamityNPC.DR = Ultra && hasSapphireBuff ? 0.2f : 0f;

        switch (CurrentPhase)
        {
            case Phase.Initialize: //初始化
                if (Timer1 == 0) //注册血量阈值
                {
                    NPC.AddAnomalyHPIndicator(JewelRubyLifeRatio_Anomaly, JewelRubyLifeRatio_Ultra, true);
                    NPC.AddAnomalyHPIndicator(JewelEmeraldLifeRatio_Anomaly, JewelEmeraldLifeRatio_Ultra, true, _ => !CASharedData.AnomalyUltramundane);
                    NPC.AddAnomalyHPIndicator(JewelSapphireLifeRatio_Anomaly, JewelSapphireLifeRatio_Ultra, true);
                    NPC.AddAnomalyHPIndicator(Phase2LifeRatio_Anomaly, Phase2LifeRatio_Ultra);
                    NPC.AddAnomalyHPIndicator(Phase2_2LifeRatio_Anomaly, Phase2_2LifeRatio_Ultra, true, n => new KingSlime_Anomaly { _entity = n }.Phase2);
                }

                Timer1++;
                if (NPC.velocity.Y == 0f)
                    Timer2++;
                if (Timer2 > 20)
                {
                    NPC.velocity = Vector2.Zero;
                    LastSpawnSlimeLife = NPC.life;
                    CurrentPhase = Phase.Phase1;
                    SelectNextAttack();
                }
                else if (Timer1 > 600)
                {
                    CurrentBehavior = Behavior.Despawn;
                    Despawn();
                }
                break;
            case Phase.Phase1 or Phase.Phase2 or Phase.Phase2_2:
                switch (CurrentBehavior)
                {
                    case Behavior.Despawn:
                        Despawn();
                        ChangeScale();
                        return false;
                    case Behavior.NormalJump or Behavior.HighJump or Behavior.RapidJump:
                        Jump();
                        break;
                    case Behavior.Teleport:
                        Teleport();
                        break;
                    default:
                        SelectNextAttack();
                        break;
                }
                break;
            case Phase.PhaseChange_1To2:
                PhaseChange();
                break;
        }

        TeleportTimer += Utils.Remap((Target.Center - NPC.Center).Y, 0f, 800f, 1.5f, 5f);

        ChangeScale();
        TrySpawnMinions();

        NPC.GraphicAlpha = (byte)(255 * TOMathUtils.Interpolation.ExponentialEaseOut(TeleportScaleMultiplier * 3f, 4f));

        if (Main.dedServ)
            NPC.netUpdate = true;

        return false;

        #region 行为函数
        void SelectNextAttack()
        {
            if (InvalidPhase1)
            {
                CurrentPhase = Phase.PhaseChange_1To2;
                CurrentBehavior = Behavior.PhaseChange_1To2;
            }
            else
            {
                if (Phase2 && NPC.LifeRatio < Phase2_2LifeRatio)
                    CurrentPhase = Phase.Phase2_2;

                if (TeleportTimer > MathHelper.Lerp(1250f, 1000f, NPC.LostLifeRatio) || !NPC.WithinRange(Target.Center, 2400f))
                {
                    CurrentBehavior = Behavior.Teleport;
                    TeleportTimer = 0f;
                }
                else
                {
                    switch (SmallJumpCounter)
                    {
                        case 3:
                            CurrentBehavior = Behavior.HighJump;
                            SmallJumpCounter = 0;
                            break;
                        case 1 or 2 when NPC.LifeRatio < JewelSapphireLifeRatio:
                            CurrentBehavior = Behavior.RapidJump;
                            break;
                        default:
                            CurrentBehavior = Behavior.NormalJump;
                            break;
                    }
                }
            }
            CurrentAttackPhase = 0;
            DirectionChangeCounter = 0;
            Timer1 = 0;
            Timer2 = 0;
        }

        bool StopHorizontalMovement()
        {
            NPC.velocity.X *= 0.85f;
            float velocityXLength = Math.Abs(NPC.velocity.X);
            if (velocityXLength < 0.1f)
            {
                NPC.velocity.X = 0f;
                return true;
            }
            else if (velocityXLength > 0.2f)
                NPC.velocity.X -= 0.15f * Math.Sign(NPC.velocity.X);
            return false;
        }

        void MakeSlimeDust(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                bool useRainbowDust = Main.rand.NextProbability(RainbowRatio);
                Dust.NewDustAction(NPC.Center, NPC.width + 25, NPC.height, useRainbowDust ? KingSlime_Handler.GetRandomDustID() : JewelSapphireAlive ? (Main.remixWorld || Main.zenithWorld ? DustID.GemTopaz : DustID.GemSapphire) : DustID.TintableDust, Vector2.Zero, d =>
                {
                    d.alpha = 150;
                    if (!useRainbowDust)
                    {
                        Color color = new(78, 136, 255, 80);
                        if (Main.remixWorld || Main.zenithWorld)
                        {
                            byte r = color.R;
                            byte g = color.G;
                            byte b = color.B;
                            byte a = color.A;
                            color = new Color(b, b, (r + g) / 2, a);
                        }
                        d.color = color;
                    }
                    d.noGravity = true;
                    d.scale = Utils.Remap(NPC.scale, MinScale, MaxScale, 1.5f, 4.5f);
                    d.velocity *= 0.5f;
                });
            }
        }

        void Despawn()
        {
            StopHorizontalMovement(); //停止水平移动，避免奇怪的滑行现象
            NPC.dontTakeDamage = true;
            NPC.damage = 0;
            DespawnScaleMultiplier *= 0.97f;
            MakeSlimeDust((int)Utils.Remap(NPC.scale, MinScale, MaxScale, 4f, 10f));

            if (NPC.scale < 0.2f) //体积足够小时执行脱战逻辑
            {
                NPC.active = false;
                NPC.netUpdate = true;
            }
        }

        void SpawnJewelParticle(NPC jewel, int amount)
        {
            for (int i = 0; i < amount; i++)
                KingSlime_Handler.SpawnOrbParticle(jewel, Main.rand.NextFloat(5f, 10f), Main.rand.Next(40, 60), Main.rand.NextFloat(0.4f, 0.7f));
        }

        void ChangeScale() => NPC.ChangeScaleFixBottom(98, 92, MathHelper.Lerp(
            Main.zenithWorld ? MinScale : MaxScale,
            Main.zenithWorld ? MaxScale : MinScale,
            CurrentPhase == Phase.PhaseChange_1To2 && PhaseChangeLifeRatio > 0f ? 1f - PhaseChangeLifeRatio : NPC.LostLifeRatio) * TeleportScaleMultiplier * DespawnScaleMultiplier);

        void SpawnSlime(int type)
        {
            float spawnZoneWidth = NPC.width / 2f - 16f;
            float spawnZoneHeight = NPC.height - 32f;
            Vector2 spawnPosition = new(NPC.Center.X + Main.rand.NextFloat(-spawnZoneWidth, spawnZoneWidth), NPC.Bottom.Y - Main.rand.NextFloat(spawnZoneHeight));
            NPC.NewNPCAction(SourceAI, spawnPosition, type, action: n =>
            {
                n.velocity = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-3f, 3f));
                n.ai[0] = -1000 * Main.rand.Next(3);
                n.Master = NPC;
            });
        }

        Vector2 GetJewelSpawnPosition() => NPC.Top - new Vector2(0, NPC.height);

        void SpawnJewelAction(NPC n)
        {
            n.Master = NPC;
            n.netUpdate = true;
            SoundEngine.PlaySound(KingSlime_Handler.SpawnSound, GetJewelSpawnPosition());
            SpawnJewelParticle(n, 50);
        }

        void TrySpawnMinions()
        {
            if (!TOSharedData.GeneralClient || NPC.Master is not null) //史莱姆王召唤的史莱姆王不再召唤仆从
                return;

            if (NPC.LifeRatio < JewelRubyLifeRatio && !JewelRubySpawned)
            {
                NPC.NewNPCAction<KingSlimeJewelRuby>(SourceAI, GetJewelSpawnPosition(), NPC.whoAmI, action: n =>
                {
                    SpawnJewelAction(n);
                    JewelRuby = n;
                    JewelRubySpawned = true;
                });
            }
            if (NPC.LifeRatio < JewelEmeraldLifeRatio && !JewelEmeraldSpawned)
            {
                NPC.NewNPCAction<KingSlimeJewelEmerald>(SourceAI, GetJewelSpawnPosition(), NPC.whoAmI, action: n =>
                {
                    SpawnJewelAction(n);
                    JewelEmerald = n;
                    JewelEmeraldSpawned = true;
                });
            }
            if (NPC.LifeRatio < JewelSapphireLifeRatio && !JewelSapphireSpawned)
            {
                NPC.NewNPCAction<KingSlimeJewelSapphire>(SourceAI, GetJewelSpawnPosition(), NPC.whoAmI, action: n =>
                {
                    SpawnJewelAction(n);
                    JewelSapphire = n;
                    JewelSapphireSpawned = true;
                });
            }

            float distance = (float)(LastSpawnSlimeLife - NPC.life) / NPC.lifeMax;
            float distanceNeeded = SpawnSlimeDistance;
            if (distance >= distanceNeeded && RainbowRatio == 0f)
            {
                LastSpawnSlimeLife = NPC.life;
                int spawnAmount = Main.rand.Next(1, Main.zenithWorld ? (int)MathHelper.Lerp(3f, 6f, NPC.LostLifeRatio) : 2) + Math.Clamp((int)Math.Pow(distance / distanceNeeded, SpawnSlimePow), 0, 5);

                for (int i = 0; i < spawnAmount; i++)
                {
                    int minTypeChoice = (int)MathHelper.Lerp(i < 2 ? 0 : 4, 9f, NPC.LostLifeRatio);
                    int maxTypeChoice = 15;

                    int type = -1;
                    do
                    {
                        switch (Main.rand.Next(minTypeChoice, maxTypeChoice + 1))
                        {
                            //基础史莱姆
                            case 0: //绿史莱姆
                                type = NPCID.GreenSlime;
                                break;
                            case 1: //蓝史莱姆
                                type = NPCID.BlueSlime;
                                break;
                            case 2: //红史莱姆
                                type = NPCID.RedSlime;
                                break;
                            case 3: //紫史莱姆
                                type = NPCID.PurpleSlime;
                                break;
                            case 4: //黄史莱姆
                                type = NPCID.YellowSlime;
                                break;
                            case 5: //冰雪史莱姆
                                type = NPCID.IceSlime;
                                break;
                            case 6: //丛林史莱姆
                                type = NPCID.JungleSlime;
                                break;
                            //特殊史莱姆
                            case 7: //尖刺史莱姆
                                type = NPCID.SlimeSpiked;
                                break;
                            case 8: //尖刺冰雪史莱姆
                                type = NPCID.SpikedIceSlime;
                                break;
                            case 9: //尖刺丛林史莱姆
                                type = NPCID.SpikedJungleSlime;
                                break;
                            case 10 when Main.raining: //雨伞史莱姆
                                type = NPCID.UmbrellaSlime;
                                break;
                            case 11 when Main.zenithWorld || Main.bloodMoon: //腐化史莱姆、猩红史莱姆、黑檀枯萎史莱姆、血腥枯萎史莱姆
                                int corrupt = (Main.zenithWorld || NPC.downedBoss2) && Main.rand.NextBool(4) ? ModContent.NPCType<EbonianBlightSlime>() : NPCID.CorruptSlime;
                                int crimson = (Main.zenithWorld || NPC.downedBoss2) && Main.rand.NextBool(4) ? ModContent.NPCType<CrimulanBlightSlime>() : NPCID.Crimslime;
                                type = Main.drunkWorld ? (Main.rand.NextBool() ? crimson : corrupt) : WorldGen.crimson ? crimson : corrupt;
                                break;
                            case 12 when Main.zenithWorld || NPC.downedBoss3: //微光史莱姆
                                type = NPCID.ShimmerSlime;
                                break;
                            case 13 when Main.zenithWorld || Main.hardMode: //夜明史莱姆、彩虹史莱姆
                                type = (Main.raining || Ultra) && Main.rand.NextBool(5) ? NPCID.RainbowSlime : NPCID.IlluminantSlime;
                                break;
                            case 14 when Main.rand.NextBool(15): //粉史莱姆
                                type = NPCID.Pinky;
                                break;
                            case 15 when Main.zenithWorld && Main.rand.NextBool(200): //金史莱姆
                                type = NPCID.GoldenSlime;
                                break;
                        }
                    } while (type == -1); //随机选择史莱姆类型直到成功

                    SpawnSlime(type);
                }

                if (Main.zenithWorld && Main.rand.NextBool(1000)) //GFB世界中0.1%概率生成史莱姆王！
                {
                    NPC.NewNPCAction(NPC.GetBossSpawnSource(Target.whoAmI), NPC.Center, NPCID.KingSlime, action: n =>
                    {
                        n.Master = NPC;
                        SoundEngine.PlaySound(SoundID.Roar, n.Center);
                        TOLocalizationUtils.ChatLocalizedText(this, "GFBSummon", Color.LightSeaGreen);
                    });
                }
            }
        }

        void Jump()
        {
            switch (CurrentAttackPhase)
            {
                case 0: //静止一段时间后起跳
                    Timer2++;

                    NPC.damage = 0;
                    NPC.GravityMultiplier *= Ultra && NPC.velocity.Y > 0f ? Utils.Remap(Timer2, 15, 60, 1f, 1.5f) : 1f;
                    NPC.MaxFallSpeedMultiplier *= Ultra && NPC.velocity.Y > 0f ? Utils.Remap(Timer2, 15, 60, 1f, 1.75f) : 1f;
                    StopHorizontalMovement();
                    if (NPC.velocity.Y == 0f && TeleportScaleMultiplier > 0.6f)
                        Timer1++;

                    float jumpDelay = MathHelper.Lerp(Ultra ? 20f : 27.5f, Ultra ? 15f : 20f, NPC.LostLifeRatio);
                    if (Timer1 > jumpDelay)
                    {
                        if (CurrentBehavior != Behavior.HighJump)
                            SmallJumpCounter++;
                        NPC.damage = NPC.defDamage;
                        NPC.netUpdate = true;
                        NPC.velocity = GetInitialVelocity();
                        CurrentAttackPhase = 1;
                    }

                    break;
                case 1: //上升
                case 2: //下降
                    NPC.damage = NPC.defDamage;
                    bool rapidJump = CurrentBehavior == Behavior.RapidJump;
                    bool highJump = CurrentBehavior == Behavior.HighJump;
                    bool farAway = Math.Abs(NPC.Center.X - Target.Center.X) > (rapidJump ? 2000f : 1000f);
                    if (NPC.velocity.X * NPC.direction <= 0.1f && farAway) //跳跃过度时调整水平速度
                    {
                        NPC.velocity.X *= rapidJump ? 0.99f : highJump ? 0.945f : 0.965f;
                        switch (Math.Abs(NPC.velocity.X))
                        {
                            case < 0.1f:
                                DirectionChangeCounter++;
                                NPC.velocity.X += GetDeltaVelocityX() * NPC.direction;
                                break;
                            case > 0.25f:
                                NPC.velocity.X -= (rapidJump ? 0.005f : highJump ? 0.0125f : 0.0075f) * Math.Sign(NPC.velocity.X);
                                break;
                        }
                    }
                    else
                        NPC.velocity.X = Math.Min(Math.Abs(NPC.velocity.X) + GetDeltaVelocityX(), GetMaxVelocityX()) * Math.Sign(NPC.velocity.X);
                    switch (CurrentAttackPhase)
                    {
                        case 1:
                            NPC.noTileCollide = true; //上升时无视物块
                            if (NPC.velocity.Y >= 0) //检测是否已过最高点
                                CurrentAttackPhase = 2;
                            break;
                        case 2:
                            if (NPC.velocity.Y == 0f)
                            {
                                TeleportTimer += CurrentBehavior switch
                                {
                                    Behavior.HighJump => 400f,
                                    Behavior.NormalJump => 100f,
                                    Behavior.RapidJump => 75f,
                                    _ => 0f
                                };
                                SelectNextAttack();
                                if (CurrentBehavior == Behavior.RapidJump)
                                    Timer1 += Phase2 ? 15 : 10;
                            }
                            else
                            {
                                NPC.GravityMultiplier *= CurrentBehavior switch
                                {
                                    Behavior.HighJump => Ultra ? (Phase2 ? Utils.Remap(Timer2, 10, 55, 1.5f, 2f) : Utils.Remap(Timer2, 15, 60, 1.35f, 1.75f)) : 1.25f,
                                    Behavior.NormalJump => Ultra && Phase2 ? Utils.Remap(Timer2, 15, 55, 1.15f, 1.5f) : 1f,
                                    _ => 1f
                                };
                                NPC.MaxFallSpeedMultiplier *= CurrentBehavior switch
                                {
                                    Behavior.HighJump => Ultra ? (Phase2 ? Utils.Remap(Timer2, 10, 55, 2f, 2.75f) : Utils.Remap(Timer2, 15, 60, 1.85f, 2.5f)) : 1.75f,
                                    Behavior.NormalJump => Ultra && Phase2 ? Utils.Remap(Timer2, 15, 55, 1.15f, 1.5f) : 1f,
                                    _ => 1f
                                };
                            }
                            break;
                    }
                    break;
            }

            Vector2 GetInitialVelocity() => new(
                CurrentBehavior switch
                {
                    Behavior.NormalJump => Phase2 ? MathHelper.Lerp(7f, 10f, LostLifeRatio2) : MathHelper.Lerp(4f, 7f, NPC.LostLifeRatio),
                    Behavior.HighJump => Phase2 ? MathHelper.Lerp(9f, 12f, LostLifeRatio2) : MathHelper.Lerp(6f, 9f, NPC.LostLifeRatio),
                    Behavior.RapidJump => Phase2 ? MathHelper.Lerp(12.5f, 15f, LostLifeRatio2) : MathHelper.Lerp(10f, 12.5f, NPC.LostLifeRatio),
                    _ => 0f
                } * NPC.direction,
                CurrentBehavior switch
                {
                    Behavior.NormalJump => (Phase2 ? 8.5f : 7f) * (1f + Math.Clamp(Math.Max(NPC.Center.Y - Target.Center.Y, 0f) / 1000f, 0f, 0.5f)),
                    Behavior.HighJump => (Phase2 ? MathHelper.Lerp(12.5f, 15f, LostLifeRatio2) : MathHelper.Lerp(10f, 12.5f, NPC.LostLifeRatio)) * (1f + Math.Clamp(Math.Max(NPC.Center.Y - Target.Center.Y, 0f) / 1000f, 0f, 1f)),
                    Behavior.RapidJump => Phase2 ? 5f : 4f,
                    _ => 0f
                } * -1f);

            float GetMaxVelocityX() => CurrentBehavior switch
            {
                Behavior.RapidJump => (Phase2 ? MathHelper.Lerp(15f, 17f, LostLifeRatio2) : MathHelper.Lerp(13.5f, 15f, NPC.LostLifeRatio)) + TOMathUtils.Interpolation.QuadraticEaseOut(Math.Max(Math.Abs(NPC.Center.X - Target.Center.X) - 300f, 0f) / 1000f) * 5f,
                Behavior.HighJump => DirectionChangeCounter switch
                {
                    0 => Phase2 ? MathHelper.Lerp(11.5f, 13f, LostLifeRatio2) : MathHelper.Lerp(9f, 11.5f, NPC.LostLifeRatio),
                    1 => Phase2 ? 3f : 2.5f,
                    _ => 1.5f
                },
                _ => DirectionChangeCounter switch
                {
                    0 => Phase2 ? MathHelper.Lerp(10f, 12.5f, LostLifeRatio2) : MathHelper.Lerp(7f, 9f, NPC.LostLifeRatio),
                    1 => 2.5f,
                    _ => 1.5f
                }
            };

            float GetDeltaVelocityX() => CurrentBehavior switch
            {
                Behavior.RapidJump => DirectionChangeCounter switch
                {
                    0 => 0.4f,
                    1 => 0.2f,
                    _ => 0.1f
                },
                _ => DirectionChangeCounter switch
                {
                    0 => 0.25f,
                    1 => 0.15f,
                    _ => 0.075f
                }
            };
        }

        void Teleport()
        {
            NPC.damage = 0;
            switch (CurrentAttackPhase)
            {
                case 0: //寻的
                    Vector2? destination = null;
                    Vector2 vectorAimedAheadOfTarget = Target.Center + new Vector2(MathF.Round(Target.velocity.X / 2f), 0f).ToCustomLength(800f);
                    //目标点
                    Point predictiveTeleportPoint = vectorAimedAheadOfTarget.ToTileCoordinates();
                    predictiveTeleportPoint.X = Math.Clamp(predictiveTeleportPoint.X, 10, Main.maxTilesX - 10);
                    predictiveTeleportPoint.Y = Math.Clamp(predictiveTeleportPoint.Y, 10, Main.maxTilesY - 10);
                    int randomPredictiveTeleportOffset = 5;

                    for (int i = 0; i < 100; i++)
                    {
                        int teleportTileX = Main.rand.Next(predictiveTeleportPoint.X - randomPredictiveTeleportOffset, predictiveTeleportPoint.X + randomPredictiveTeleportOffset + 1);
                        int teleportTileY = Main.rand.Next(predictiveTeleportPoint.Y - randomPredictiveTeleportOffset, predictiveTeleportPoint.Y);
                        Tile potentialTile = Main.tile[teleportTileX, teleportTileY];
                        if (!potentialTile.HasUnactuatedTile)
                        {
                            if (potentialTile.LiquidType != LiquidID.Lava && Collision.CanHitLine(NPC.Center, 0, 0, predictiveTeleportPoint.ToVector2() * 16, 0, 0))
                            {
                                destination = new Vector2((teleportTileX + 0.5f) * 16f, (teleportTileY + 1f) * 16f);
                                break; //在此处退出循环
                            }
                            else
                            {
                                predictiveTeleportPoint.X += predictiveTeleportPoint.X < 0f ? 1 : -1;
                                predictiveTeleportPoint.X = Math.Clamp(predictiveTeleportPoint.X, 10, Main.maxTilesX - 10);
                            }
                        }
                        else
                        {
                            predictiveTeleportPoint.X += predictiveTeleportPoint.X < 0f ? 1 : -1;
                            predictiveTeleportPoint.X = Math.Clamp(predictiveTeleportPoint.X, 10, Main.maxTilesX - 10);
                        }
                    }

                    TeleportDestination = destination ?? Target.Bottom;
                    CurrentAttackPhase = 1;
                    break;
                case 1: //停止水平移动并缩小体型，满足条件时传送
                    MakeSlimeDust((int)Utils.Remap(NPC.scale, MinScale, MaxScale, 5f, 12.5f));
                    TeleportScaleMultiplier -= MathHelper.Lerp(Ultra ? 0.016f : 0.013f, Ultra ? 0.02f : 0.015f, NPC.LostLifeRatio);
                    if (StopHorizontalMovement() && TeleportScaleMultiplier <= 0.2f)
                    {
                        Gore.NewGoreAction(SourceAI, NPC.Center + new Vector2(-40f, -NPC.height / 2f), NPC.velocity, GoreID.KingSlimeCrown, g => g.timeLeft += 180);
                        NPC.Bottom = TeleportDestination;
                        CurrentAttackPhase = 2;
                    }
                    break;
                case 2: //恢复体型，恢复完成后开始下一次攻击
                    TeleportScaleMultiplier += MathHelper.Lerp(0.03f, 0.05f, NPC.LostLifeRatio);
                    if (TeleportScaleMultiplier >= 1f)
                    {
                        TeleportScaleMultiplier = 1f;
                        if (Timer2++ >= 1f)
                            SelectNextAttack();
                    }
                    break;
            }
        }

        void PhaseChange()
        {
            RainbowRatio += 0.01f;
            StopHorizontalMovement();

            switch (Timer1)
            {
                case 0:
                    if (JewelRubyAlive)
                        KingSlime_Handler.DisableAttack(JewelRuby);
                    if (JewelEmeraldAlive)
                        KingSlime_Handler.DisableAttack(JewelEmerald);
                    if (JewelSapphireAlive)
                        KingSlime_Handler.DisableAttack(JewelSapphire);
                    break;
                case 90:
                    SoundEngine.PlaySound(SoundID.Item38, NPC.Center);
                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                    for (int i = 0; i < 6; i++)
                        SpawnSlime(NPCID.RainbowSlime);
                    Vector2 position = GetJewelSpawnPosition();
                    if (JewelRubyAlive)
                    {
                        KingSlime_Handler.CreateDustFromJewelTo(JewelRuby, position, Main.zenithWorld ? DustID.IceTorch : DustID.GemRuby);
                        KingSlime_Handler.Kill(JewelRuby);
                    }
                    if (JewelEmeraldAlive)
                    {
                        KingSlime_Handler.CreateDustFromJewelTo(JewelEmerald, position, Main.zenithWorld ? DustID.GemAmethyst : DustID.GemEmerald);
                        KingSlime_Handler.Kill(JewelEmerald);
                    }
                    if (JewelSapphireAlive)
                    {
                        KingSlime_Handler.CreateDustFromJewelTo(JewelSapphire, position, Main.zenithWorld ? DustID.GemTopaz : DustID.GemSapphire);
                        KingSlime_Handler.Kill(JewelSapphire);
                    }
                    NPC.NewNPCAction<KingSlimeJewelRainbow>(SourceAI, GetJewelSpawnPosition(), NPC.whoAmI, action: n =>
                    {
                        SpawnJewelAction(n);
                        JewelRainbow = n;
                        JewelRainbowSpawned = true;
                    });
                    break;
                case 100:
                    PhaseChangeLifeRatio = NPC.LifeRatio;
                    break;
                case 160:
                    CurrentPhase = Phase.Phase2;
                    TeleportTimer = 2000f;
                    SelectNextAttack();
                    break;
            }

            Timer1++;

            if (Timer1 is >= 100 and <= 160) //回复血量
            {
                float ratio = (Timer1 - 100f) / 60f;
                int newLife = (int)MathHelper.Lerp(NPC.life, NPC.lifeMax * MathHelper.Lerp(0.1f, 0.5f, ratio), TOMathUtils.Interpolation.LogarithmicEaseOut(ratio));
                int increasedLife = Math.Clamp(newLife - NPC.life, 0, NPC.lifeMax / 2 - NPC.life);

                if (increasedLife > 0)
                {
                    NPC.life += increasedLife;
                    NPC.HealEffect(increasedLife, true);
                }

                if (NPC.life > NPC.lifeMax)
                    NPC.life = NPC.lifeMax;
            }

            if (Timer1 > 80)
                TeleportScaleMultiplier -= 0.01f;
        }
        #endregion 行为函数
    }

    public override void FindFrame(int frameHeight)
    {
        /*
        if (NPC.velocity.Y != 0f)
        {
            if (NPC.frame.Y < frameHeight * 4)
            {
                NPC.frame.Y = frameHeight * 4;
                NPC.frameCounter = 0.0;
            }

            if ((NPC.frameCounter += 1.0) >= 4.0)
                NPC.frame.Y = frameHeight * 5;

        }
        else
        {
            if (NPC.frame.Y >= frameHeight * 5)
            {
                NPC.frame.Y = frameHeight * 4;
                NPC.frameCounter = 0.0;
            }
            NPC.frameCounter += 1.0;


            if (num2 > 0)
                NPC.frameCounter += 1.0;
            if (num2 == 4)
                NPC.frameCounter += 1.0;
            if (frameCounter >= 8.0)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0.0;
                if (NPC.frame.Y >= frameHeight * 4)
                    NPC.frame.Y = 0;
            }
        }
        */
    }

    public override Color? GetAlpha(Color drawColor)
    {
        Color newColor = Color.Lerp(new Color(0, 0, 150, NPC.alpha), new Color(125, 125, 255, NPC.alpha), TOMathUtils.TimeWrappingFunction.GetTimeSin(0.35f, 1.5f, unsigned: true) + 0.3f);
        Color preRainbow = Color.Lerp(Main.zenithWorld ? new Color(125, 125, 255, NPC.alpha) : drawColor, newColor, SapphireBuffRatio);

        if (Main.remixWorld || Main.zenithWorld)
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
            KingSlime_Handler.GetKingSlimeJewel(JewelRuby)?.KingSlimeDead = true;
        if (JewelEmeraldAlive)
            KingSlime_Handler.GetKingSlimeJewel(JewelEmerald)?.KingSlimeDead = true;
        if (JewelSapphireAlive)
            KingSlime_Handler.GetKingSlimeJewel(JewelSapphire)?.KingSlimeDead = true;
        if (JewelRainbowAlive)
            KingSlime_Handler.GetKingSlimeJewel(JewelRainbow)?.KingSlimeDead = true;
    }
}
