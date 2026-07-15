// Developed by ColdsUx

using CalamityAnomalies.Anomaly.KingSlime;
using CalamityAnomalies.DataStructures;
using CalamityMod.NPCs.NormalNPCs;
using Terraria.Graphics.Shaders;

namespace CalamityAnomalies.Anomaly.QueenSlime;

public sealed partial class QueenSlime_Anomaly : AnomalyNPCBehavior<QueenSlime_Anomaly>, ILocalizationPrefix
{
    #region 数据
    public enum Phase : byte
    {
        Initialize = 0,
        Phase1,
        Phase1_2,
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

        Phase1_FirstJump,
        Phase1_NormalJump,
        Phase1_HighJump,
        Phase1_SlamDown,
        Phase1_Teleport,

        Phase2_Gel,
        Phase2_SlamDown,

        PhaseChange_2To3,
    }

    public const float DespawnDistance = 5000f;
    public static float SpawnSlimeDistance => Aroma ? 0.01f : 0.05f;
    public static float SpawnSlimePow => Aroma ? 0.5f : Ultra ? 0.3f : 0.2f;

    public const float Phase1_2LifeRatio_Anomaly = 0.75f;
    public const float Phase1_2LifeRatio_Ultra = 0.8f;
    public const float Phase2LifeRatio_Anomaly = 0.5f;
    public const float Phase2LifeRatio_Ultra = 0.6f;
    public const float Phase2_2LifeRatio_Anomaly = 0.25f;
    public const float Phase2_2LifeRatio_Ultra = 0.35f;
    public const float Phase3LifeRatio_Anomaly = 0f;
    public const float Phase3LifeRatio_Ultra = 0.1f;

    public static float Phase1_2LifeRatio => Ultra ? Phase1_2LifeRatio_Ultra : Phase1_2LifeRatio_Anomaly;
    public static float Phase2LifeRatio => Ultra ? Phase2LifeRatio_Ultra : Phase2LifeRatio_Anomaly;
    public static float Phase2_2LifeRatio => Ultra ? Phase2_2LifeRatio_Ultra : Phase2_2LifeRatio_Anomaly;
    public static float Phase3LifeRatio => Ultra ? Phase3LifeRatio_Ultra : Phase3LifeRatio_Anomaly;

    private static readonly ProjectileDamageContainer _gelDamage = new(80, 120, 150, 210, 150, 210);
    public static int GelDamage => _gelDamage.Value;

    private static readonly ProjectileDamageContainer _slamDamage = new(100, 160, 204, 252, 204, 270);
    public static int SlamDamage => _slamDamage.Value;

    private static readonly ProjectileDamageContainer _spikeDamage = new(60, 96, 120, 150, 132, 180);
    public static int SpikeDamage => _spikeDamage.Value;

    public static int JumpDelay => 5;

    public bool IsAttacking
    {
        get
        {
            int attackDelay = CurrentPhase switch
            {
                Phase.Phase1 or Phase.Phase1_2 => (int)MathHelper.Lerp(40, Ultra ? 20 : 25, LostLifeRatioForPhase1),
                Phase.Phase2 or Phase.Phase2_2 => (int)MathHelper.Lerp(35, Ultra ? 20 : 25, LostLifeRatioForPhase2),
                _ => 0,
            };
            return Timer5 >= attackDelay;
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

    public bool Phase1_2 => CurrentPhase == Phase.Phase1_2;
    public bool Phase2 => CurrentPhase is Phase.Phase2 or Phase.Phase2_2;
    public bool Phase2_2 => CurrentPhase == Phase.Phase2_2;

    public float LifeRatioForPhase1 => Utils.Remap(NPC.LifeRatio, Phase2LifeRatio, 1f, 0f, 1f);
    public float LostLifeRatioForPhase1 => 1f - LifeRatioForPhase1;

    public float LifeRatioForPhase2 => Utils.Remap(NPC.LifeRatio, Phase3LifeRatio, Phase2LifeRatio, 0f, 1f);
    public float LostLifeRatioForPhase2 => 1f - LifeRatioForPhase2;

    public float LifeRatioForPhase3 => Math.Min(NPC.LifeRatio * 2f, 1f);
    public float LostLifeRatioForPhase3 => 1f - LifeRatioForPhase3;

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
    public bool JewelAmethystSpawned
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
    /// 王冠红玉实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <c>DummyNPC</c>。
    /// </summary>
    public NPC JewelAmethyst
    {
        get => NPC.TryGetNPC(AnomalyNPC.AnomalyAI32[1].byte0);
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
    public bool JewelAmethystAlive => JewelAmethyst.active && JewelAmethyst.ModNPC is KingSlimeJewelRuby && JewelAmethyst.Master == NPC;
    public bool JewelAmethystDead => JewelAmethystSpawned && !JewelAmethystAlive;

    public bool JewelRainbowSpawned
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
    /// 王冠彩虹宝石实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <c>DummyNPC</c>。
    /// </summary>
    public NPC JewelRainbow
    {
        get => NPC.TryGetNPC(AnomalyNPC.AnomalyAI32[1].byte1);
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
    #endregion 数据

    public string LocalizationPrefix => CASharedData.AnomalyLocalizationPrefix + "QueenSlime";

    public override int ApplyingType => NPCID.QueenSlimeBoss;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        _ => true,
    };

    public override void SetDefaults()
    {
        DespawnScaleMultiplier = 1f;
        TeleportScaleMultiplier = 1f;

        NPC.AddAnomalyHPIndicator(Phase1_2LifeRatio_Anomaly, Phase1_2LifeRatio_Ultra, true);
        NPC.AddAnomalyHPIndicator(Phase2LifeRatio_Anomaly, Phase2LifeRatio_Ultra);
    }

    public override void FindFrame(int frameHeight)
    {
        bool phase2 = Phase2;
        NPC.frame.Width = 180;
        ref int frameIndex = ref OceanNPC.FrameCounter3;
        ref double frameCounter = ref OceanNPC.FrameCounter;

        // 上升或滞空（且半血时特殊处理）
        if ((phase2 && NPC.noGravity) || NPC.velocity.Y < 0f)
        {
            if (frameIndex is < 20 or > 23)
            {
                // 不在特殊区间（20~23）时，确保在 4~7 区间起始
                if (frameIndex is < 4 or > 7)
                {
                    frameIndex = 4;
                    frameCounter = -1.0;
                }
                if ((frameCounter += 1.0) >= 4.0)
                {
                    frameCounter = 0.0;
                    frameIndex++;
                    if (frameIndex >= 7)
                        frameIndex = phase2 ? 22 : 7;
                }
            }
            else
            {
                // 在 20~23 区间循环
                if ((frameCounter += 1.0) >= 5.0)
                {
                    frameCounter = 0.0;
                    frameIndex++;
                    if (frameIndex >= 24)
                        frameIndex = 20;
                }
            }
        }
        else if (NPC.velocity.Y > 0f) // 下落
        {
            if (frameIndex is < 8 or > 10)
            {
                frameIndex = 8;
                frameCounter = -1.0;
            }
            if ((frameCounter += 1.0) >= 8.0)
            {
                frameCounter = 0.0;
                frameIndex++;
                if (frameIndex >= 10)
                    frameIndex = 10;
            }
        }
        else
        {
            if (CurrentBehavior == Behavior.Phase1_SlamDown)
            {
                frameCounter = 0.0;
                int phase = Timer1 / 8;
                frameIndex = phase switch
                {
                    1 => 11,
                    2 or 3 => 10,
                    _ => 12,
                };
            }
            else // 普通行走/待机动画
            {
                bool isSpecialFrameRange = frameIndex is >= 10 and <= 12;
                int frameDelay = isSpecialFrameRange ? 6 : 10;

                if (!isSpecialFrameRange && frameIndex >= 4)
                {
                    frameIndex = 0;
                    frameCounter = -1.0;
                }

                if ((frameCounter += 1.0) >= frameDelay)
                {
                    frameCounter = 0.0;
                    frameIndex++;
                    if ((!isSpecialFrameRange || frameIndex == 13) && frameIndex >= 4)
                        frameIndex = 0;
                }
            }
        }

        NPC.frame.Y = frameIndex * frameHeight;

        if (phase2) //处理翅膀的帧
        {
            ref float wingFrameCounter = ref NPC.localAI[3];

            wingFrameCounter++;
            if (wingFrameCounter >= 24f)
                wingFrameCounter = 0f;

            if (IsAttacking)
            {
                if (CurrentBehavior == Behavior.Phase2_SlamDown)
                    wingFrameCounter = 6f;
                else if (CurrentBehavior == Behavior.Phase2_Gel)
                    wingFrameCounter = 7f;
            }
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        // --- 通用前置处理（原 switch 之前对 657 无特殊偏移，只应用颜色与方向）---
        Color npcColor = NPC.GetNPCColorTintedByBuffs(drawColor);
        SpriteEffects spriteEffects = (NPC.spriteDirection == 1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        // --- 原 case 657 绘制逻辑 ---
        int type = NPC.type;
        Texture2D bodyTexture = TextureAssets.Npc[type].Value;
        Vector2 bodyPosition = NPC.Bottom - screenPos;
        bodyPosition.Y += 2f;

        int frameCount = Main.npcFrameCount[type];
        int frameY = NPC.frame.Y / NPC.frame.Height;
        Rectangle bodyFrame = bodyTexture.Frame(2, 16, frameY / frameCount, frameY % frameCount);
        bodyFrame.Inflate(0, -2);
        Vector2 bodyOrigin = bodyFrame.Size() * new Vector2(0.5f, 1f);

        Color queenColor = Color.Lerp(Color.White, npcColor, 0.5f);

        // 翅膀
        if (Phase2)
        {
            Texture2D wingTexture = TextureAssets.Extra[ExtrasID.QueenSlimeWing].Value;
            int wingFrameIndex = (int)NPC.localAI[3] / 6;
            Rectangle wingFrame = wingTexture.Frame(1, 4, 0, wingFrameIndex);
            float wingScale = 0.8f;

            for (int side = 0; side < 2; side++)
            {
                // side 0：左翅（不翻转），side 1：右翅（水平翻转）
                bool isRightWing = side == 1;
                float originXFraction = isRightWing ? 0f : 1f;   // 控制锚点在纹理左/右边缘
                float xOffset = isRightWing ? 2f : 0f;           // 水平偏移量
                SpriteEffects flipEffect = isRightWing ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                Vector2 wingOrigin = wingFrame.Size() * new Vector2(originXFraction, 0.5f);
                Vector2 wingPosition = NPC.Center + new Vector2(xOffset, 0f);

                if (NPC.rotation != 0f)
                    wingPosition = wingPosition.RotatedBy(NPC.rotation, NPC.Bottom);

                wingPosition -= screenPos;

                // 根据垂直速度计算翅膀扇动角度，左右翅膀摆动方向相反
                float flapRotation = MathHelper.Clamp(NPC.velocity.Y, -6f, 6f) * -0.1f;
                if (!isRightWing)
                    flapRotation *= -1f;

                spriteBatch.Draw(wingTexture, wingPosition, wingFrame, queenColor, NPC.rotation + flapRotation, wingOrigin, wingScale, flipEffect, 0f);
            }
        }

        // 水晶核心
        Texture2D coreTexture = TextureAssets.Extra[ExtrasID.QueenSlimeCrystalCore].Value;
        Rectangle coreFrame = coreTexture.Frame();
        Vector2 coreOrigin = coreFrame.Size() * new Vector2(0.5f, 0.5f);
        Vector2 corePosition = new(NPC.Center.X, NPC.Center.Y);
        float coreOffsetY = frameY switch
        {
            1 or 6 => -10f,
            3 or 5 => 10f,
            4 or 12 or 13 or 14 or 15 => 18f,
            7 or 8 => -14f,
            9 => -16f,
            10 => -18f,
            11 => 20f,
            20 => -14f,
            21 or 23 => -18f,
            22 => -22f,
            _ => 0f
        };
        corePosition.Y += coreOffsetY;
        if (NPC.rotation != 0f)
            corePosition = corePosition.RotatedBy(NPC.rotation, NPC.Bottom);
        corePosition -= screenPos;

        // 冲刺残影（需要切到 Immediate 模式）
        if (!NPC.IsABestiaryIconDummy)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.Transform);
        }

        GameShaders.Misc["QueenSlime"].Apply();
        if (CurrentBehavior is Behavior.Phase1_SlamDown or Behavior.Phase1_SlamDown && NPC.velocity.Y != 0f)
        {
            float streakScale = (CurrentAttackPhase == 2) ? 6f : 1f;
            for (int i = 7; i >= 0; i--)
            {
                float alphaFactor = 1f - i / 8f;
                Vector2 oldPos = NPC.oldPos[i] + new Vector2(NPC.width * 0.5f, NPC.height);
                oldPos -= (NPC.Bottom - Vector2.Lerp(oldPos, NPC.Bottom, 0.75f)) * streakScale;
                oldPos -= screenPos;
                spriteBatch.Draw(bodyTexture, oldPos, bodyFrame, queenColor * alphaFactor, NPC.rotation, bodyOrigin, NPC.scale, spriteEffects ^ SpriteEffects.FlipHorizontally, 0f);
            }
        }

        if (!NPC.IsABestiaryIconDummy)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        }

        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        spriteBatch.Draw(coreTexture, corePosition, coreFrame, queenColor, NPC.rotation, coreOrigin, 1f, spriteEffects ^ SpriteEffects.FlipHorizontally, 0f);

        // 主体（使用 QueenSlime shader）
        GameShaders.Misc["QueenSlime"].Apply();
        if (!NPC.IsABestiaryIconDummy)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.Transform);
        }

        DrawData bodyDrawData = new(bodyTexture, bodyPosition, bodyFrame, NPC.GetAlpha(queenColor), NPC.rotation, bodyOrigin, NPC.scale, spriteEffects ^ SpriteEffects.FlipHorizontally);
        GameShaders.Misc["QueenSlime"].Apply(bodyDrawData);
        bodyDrawData.Draw(spriteBatch);
        Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        if (!NPC.IsABestiaryIconDummy)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        }

        // 头顶皇冠
        Texture2D crownTexture = TextureAssets.Extra[ExtrasID.QueenSlimeCrown].Value;
        Rectangle crownFrame = crownTexture.Frame();
        Vector2 crownOrigin = crownFrame.Size() * new Vector2(0.5f, 0.5f);
        Vector2 crownPosition = new(NPC.Center.X, NPC.Top.Y - crownFrame.Bottom + 44f);
        float crownOffsetY = frameY switch
        {
            1 => -10f,
            3 or 5 or 6 => 10f,
            4 or 12 or 13 or 14 or 15 => 18f,
            7 or 8 => -14f,
            9 => -16f,
            10 => -18f,
            11 => 20f,
            20 => -14f,
            21 or 23 => -18f,
            22 => -22f,
            _ => 0f
        };
        crownPosition.Y += crownOffsetY;
        if (NPC.rotation != 0f)
            crownPosition = crownPosition.RotatedBy(NPC.rotation, NPC.Bottom);
        crownPosition -= screenPos;

        spriteBatch.Draw(crownTexture, crownPosition, crownFrame, queenColor, NPC.rotation, crownOrigin, 1f, spriteEffects ^ SpriteEffects.FlipHorizontally, 0f);

        return false;
    }
}
