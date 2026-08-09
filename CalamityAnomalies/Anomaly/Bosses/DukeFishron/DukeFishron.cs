// Developed by ColdsUx

namespace CalamityAnomalies.Anomaly.Bosses.DukeFishron;

public sealed partial class DukeFishron : AnomalyNPCBehavior<DukeFishron>
{
    public enum Phase : byte
    {
        Initialize = 0,
        Phase1,
        PhaseChange_1To2,
        Phase2,
        PhaseChange_2To3,
        Phase3,
        Phase3_2,
    }

    public enum Behavior : byte
    {
        Charge,
        Bubble,
        Sharknado,

        PhaseChange_1To2,
        PhaseChange_2To3,

        Phase3_Teleport,
    }

    public const float DespawnDistance = 10000f;

    public static float Phase2ContactDamageMult = 1.436f; // 201
    public static float Phase3ContactDamageMult = 1.315f; // 184

    public const float Phase2LifeRatio_Anomaly = 0.7f;
    public const float Phase2LifeRatio_Ultra = 0.75f;
    public const float Phase3LifeRatio_Anomaly = 0.4f;
    public const float Phase3LifeRatio_Ultra = 0.45f;
    public const float Phase3_2LifeRatio_Anomaly = 0.2f;
    public const float Phase3_2LifeRatio_Ultra = 0.25f;

    public static float Phase2LifeRatio => Ultra ? Phase2LifeRatio_Ultra : Phase2LifeRatio_Anomaly;
    public static float Phase3LifeRatio => Ultra ? Phase3LifeRatio_Ultra : Phase3LifeRatio_Anomaly;
    public static float Phase3_2LifeRatio => Ultra ? Phase3_2LifeRatio_Ultra : Phase3_2LifeRatio_Anomaly;

    public static int InitializeTime => Ultra ? 90 : 90;
    public static int SharknadoTime => Ultra ? 45 : 60;
    public static int PhaseChangeTime => 180;

    public static SoundStyle RoarSound = SoundID.Zombie20 with { MaxInstances = 0 };

    public Phase CurrentPhase
    {
        get
        {
            Union32 union = AI_Union_1;
            return (Phase)union.byte0;
        }
        set
        {
            Union32 union = AI_Union_1;
            union.byte0 = (byte)value;
            AI_Union_1 = union;
        }
    }

    public bool ShouldEnterPhase2 => NPC.LifeRatio < Phase2LifeRatio;
    public bool ShouldEnterPhase3 => NPC.LifeRatio < Phase3LifeRatio;
    public bool InvalidPhase1 => ShouldEnterPhase2 && Phase1;
    public bool InvalidPhase2 => ShouldEnterPhase3 && Phase2;
    public bool Phase1 => CurrentPhase == Phase.Phase1;
    public bool Phase2 => CurrentPhase == Phase.Phase2;
    public bool Phase3 => CurrentPhase is Phase.Phase3 or Phase.Phase3_2;

    public Behavior CurrentBehavior
    {
        get
        {
            Union32 union = AI_Union_1;
            return (Behavior)union.byte1;
        }
        set
        {
            Union32 union = AI_Union_1;
            union.byte1 = (byte)value;
            AI_Union_1 = union;
        }
    }

    public bool IsIdle
    {
        get
        {
            int idlePhaseTimer = Ultra ? 3 : 15;
            return Timer5 < idlePhaseTimer;
        }
    }

    public int CurrentAttackPhase
    {
        get
        {
            Union32 union = AI_Union_3;
            return union.short0;
        }
        set
        {
            Union32 union = AI_Union_3;
            union.short0 = (short)value;
            AI_Union_3 = union;
        }
    }

    public int CurrentAttackPhaseForIdle
    {
        get
        {
            Union32 union = AI_Union_3;
            return union.short1;
        }
        set
        {
            Union32 union = AI_Union_3;
            union.short1 = (short)value;
            AI_Union_3 = union;
        }
    }

    public float OffsetX
    {
        get => AnomalyNPC.AnomalyAI32[1].f;
        set
        {
            if (AnomalyNPC.AnomalyAI32[1].f != value)
            {
                AnomalyNPC.AnomalyAI32[1].f = value;
                AnomalyNPC.AIChanged32[1] = true;
            }
        }
    }

    public int ChargeCounter
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

    public override int ApplyingType => NPCID.DukeFishron;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        _ => true,
    };

    public override void SetDefaults()
    {
        NPC.AddAnomalyHPIndicator(Phase2LifeRatio_Anomaly, Phase2LifeRatio_Ultra);
        NPC.AddAnomalyHPIndicator(Phase3LifeRatio_Anomaly, Phase3LifeRatio_Ultra);
        NPC.AddAnomalyHPIndicator(Phase3_2LifeRatio_Anomaly, Phase3_2LifeRatio_Ultra, true);
    }

    public override void FindFrame(int frameHeight)
    {
        const int loopFrameCount = 6;        // 主循环动画的帧数
        const int specialFrame1 = 6;         // 特殊姿态帧索引（如张嘴）
        const int specialFrame2 = 7;         // 另一个特殊姿态帧索引
        const double normalFrameDelay = 5.0; // 常规动画每帧间隔（tick）
        const double idlePhase2FrameDelay = 4.0; // Phase2 下 Idle 动画的帧间隔

        ref double frameCounter = ref OceanNPC.FrameCounter;
        ref Rectangle frame = ref NPC.frame;

        bool isPhase1Or2 = CurrentPhase is Phase.Phase1 or Phase.Phase2;

        if (isPhase1Or2)
        {
            // Idle 状态下的循环动画
            if (IsIdle)
            {
                double frameDelay = Phase2 ? idlePhase2FrameDelay : normalFrameDelay;

                frameCounter += 1.0;
                if (frameCounter > frameDelay)
                {
                    frameCounter = 0.0;
                    frame.Y += frameHeight;
                }

                if (frame.Y >= frameHeight * loopFrameCount)
                    frame.Y = 0;
            }

            // 冲锋或泡泡行为时直接设置特定帧
            if (CurrentBehavior is Behavior.Charge or Behavior.Bubble)
            {
                if (Timer1 < 10f)
                    frame.Y = frameHeight * specialFrame1;
                else
                    frame.Y = frameHeight * specialFrame2;
            }
        }

        // 鲨卷风行为（仅 Phase1/2）或初始化阶段
        if ((isPhase1Or2 && CurrentBehavior == Behavior.Sharknado) || CurrentPhase == Phase.Initialize)
        {
            int phaseDuration = CurrentPhase == Phase.Initialize ? InitializeTime : SharknadoTime;

            if (Timer1 < phaseDuration - 30 || Timer1 > phaseDuration - 10)
            {
                frameCounter += 1.0;
                if (frameCounter > normalFrameDelay)
                {
                    frameCounter = 0.0;
                    frame.Y += frameHeight;
                }

                if (frame.Y >= frameHeight * loopFrameCount)
                    frame.Y = 0;
            }
            else
            {
                frame.Y = frameHeight * specialFrame1;
                if (Timer1 > phaseDuration - 20 && Timer1 < phaseDuration - 15)
                    frame.Y = frameHeight * specialFrame2;
            }
        }

        // 阶段切换过渡动画
        if (CurrentPhase is Phase.PhaseChange_1To2 or Phase.PhaseChange_2To3)
        {
            int phaseChangeDuration = PhaseChangeTime;

            if (Timer1 < phaseChangeDuration - 60 || Timer1 > phaseChangeDuration - 20)
            {
                frameCounter += 1.0;
                if (frameCounter > normalFrameDelay)
                {
                    frameCounter = 0.0;
                    frame.Y += frameHeight;
                }

                if (frame.Y >= frameHeight * loopFrameCount)
                    frame.Y = 0;
            }
            else
            {
                frame.Y = frameHeight * specialFrame1;
                if (Timer1 > phaseChangeDuration - 50 && Timer1 < phaseChangeDuration - 25)
                    frame.Y = frameHeight * specialFrame2;
            }
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        int type = NPC.type;

        // 基础偏移与纹理中心点
        float baseVerticalOffset = 0f;
        float addedHeight = Main.NPCAddHeight(NPC);
        Vector2 origin = new(TextureAssets.Npc[type].Width() / 2, TextureAssets.Npc[type].Height() / Main.npcFrameCount[type] / 2);

        // 主纹理、混合目标色与混合量
        Texture2D npcTexture = TextureAssets.Npc[type].Value;
        Color blendTargetColor = Color.White;
        float colorBlendAmount = 0f;

        // AI 阶段判断
        bool isPhase2 = CurrentPhase >= Phase.Phase2;  // 第二阶段（变绿）
        bool isPhase3 = CurrentPhase >= Phase.Phase3;  // 第三阶段（深绿）

        // 狂暴叠加计时相关阈值
        float enrageTimerThreshold = 120f;   // 计时器超过此值开始特定效果
        int enrageTransitionDuration = 60; // 过渡持续时间

        // 残影基础颜色
        Color ghostBaseColor = drawColor;

        // 翻转效果
        SpriteEffects spriteEffects = SpriteEffects.None;
        if (NPC.spriteDirection == 1)
            spriteEffects = SpriteEffects.FlipHorizontally;

        // ------ 根据 AI 阶段调整主颜色 ------
        if (isPhase3)
        {
            drawColor = NPC.buffColor(drawColor, 0.4f, 0.8f, 0.4f, 1f);
        }
        else if (isPhase2)
        {
            drawColor = NPC.buffColor(drawColor, 0.5f, 0.7f, 0.5f, 1f);
        }
        else if (CurrentPhase == Phase.PhaseChange_1To2 && Timer1 > enrageTimerThreshold)
        {
            float transitionProgress = (Timer1 - enrageTimerThreshold) / enrageTransitionDuration;
            drawColor = NPC.buffColor(drawColor, 1f - 0.5f * transitionProgress, 1f - 0.3f * transitionProgress, 1f - 0.5f * transitionProgress, 1f);
        }

        // 残影数量与步长
        int ghostCount = 10;
        int ghostStep = 2;

        // 根据 AI 状态调整残影数量和颜色混合参数
        if (CurrentPhase == Phase.Initialize)
            ghostCount = 0;
        if (IsIdle)
            ghostCount = 7;

        if (Phase1 && CurrentBehavior == Behavior.Charge)
        {
            blendTargetColor = Color.Blue;
            colorBlendAmount = 0.5f;
        }
        else
        {
            ghostBaseColor = drawColor;
        }

        // ------ 绘制残影拖尾（幽灵轨迹） ------
        for (int ghostIndex = 1; ghostIndex < ghostCount; ghostIndex += ghostStep)
        {
            Color ghostColor = ghostBaseColor;
            ghostColor = Color.Lerp(ghostColor, blendTargetColor, colorBlendAmount);
            ghostColor = NPC.GetAlpha(ghostColor);
            ghostColor *= (ghostCount - ghostIndex) / 15f;

            Vector2 drawPos = NPC.oldPos[ghostIndex] + new Vector2(NPC.width, NPC.height) / 2f - screenPos;
            drawPos -= new Vector2(npcTexture.Width, npcTexture.Height / Main.npcFrameCount[type]) * NPC.scale / 2f;
            drawPos += origin * NPC.scale + new Vector2(0f, baseVerticalOffset + addedHeight + NPC.gfxOffY);

            spriteBatch.Draw(npcTexture, drawPos, NPC.frame, ghostColor, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
        }

        // ------ 计算漩涡特效参数 ------
        int vortexParticleCount = 0;
        float vortexIntensity = 0f;   // 控制半径缩放的强度值
        float vortexMaxRadius = 0f;   // 最大旋转半径

        // 不同 AI 状态下的漩涡配置
        if (CurrentBehavior == Behavior.Sharknado)
        {
            float vortexStartTime = 60;
            int vortexTransitionTime = 30;
            if (Timer1 > vortexStartTime)
            {
                vortexParticleCount = 6;
                vortexIntensity = 1f - (float)Math.Cos((Timer1 - vortexStartTime) / vortexTransitionTime * MathHelper.TwoPi);
                vortexIntensity /= 3f;
                vortexMaxRadius = 40f;
            }
        }
        else if (CurrentPhase == Phase.PhaseChange_1To2 && Timer1 > enrageTimerThreshold)
        {
            vortexParticleCount = 6;
            vortexIntensity = 1f - (float)Math.Cos((Timer1 - enrageTimerThreshold) / enrageTransitionDuration * MathHelper.TwoPi);
            vortexIntensity /= 3f;
            vortexMaxRadius = 60f;
        }
        else if (CurrentPhase == Phase.PhaseChange_2To3 && Timer1 > enrageTimerThreshold)
        {
            vortexParticleCount = 6;
            vortexIntensity = 1f - (float)Math.Cos((Timer1 - enrageTimerThreshold) / enrageTransitionDuration * MathHelper.TwoPi);
            vortexIntensity /= 3f;
            vortexMaxRadius = 60f;
        }
        else if (CurrentBehavior == Behavior.Phase3_Teleport)
        {
            vortexParticleCount = 6;
            vortexIntensity = 1f - (float)Math.Cos(NPC.ai[2] / 30f * MathHelper.TwoPi);
            vortexIntensity /= 3f;
            vortexMaxRadius = 20f;
        }

        // 绘制漩涡粒子
        for (int vortexIndex = 0; vortexIndex < vortexParticleCount; vortexIndex++)
        {
            Color vortexColor = drawColor;
            vortexColor = Color.Lerp(vortexColor, blendTargetColor, colorBlendAmount);
            vortexColor = NPC.GetAlpha(vortexColor);
            vortexColor *= 1f - vortexIntensity;

            float angle = (float)vortexIndex / vortexParticleCount * MathHelper.TwoPi + NPC.rotation;
            Vector2 offset = angle.ToRotationVector2() * vortexMaxRadius * vortexIntensity;
            Vector2 drawPos = NPC.Center + offset - screenPos;
            drawPos -= new Vector2(npcTexture.Width, npcTexture.Height / Main.npcFrameCount[type]) * NPC.scale / 2f;
            drawPos += origin * NPC.scale + new Vector2(0f, baseVerticalOffset + addedHeight + NPC.gfxOffY);

            spriteBatch.Draw(npcTexture, drawPos, NPC.frame, vortexColor, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
        }

        // ------ 绘制 NPC 本体 ------
        Vector2 mainDrawPos = NPC.Center - screenPos;
        mainDrawPos -= new Vector2(npcTexture.Width, npcTexture.Height / Main.npcFrameCount[type]) * NPC.scale / 2f;
        mainDrawPos += origin * NPC.scale + new Vector2(0f, baseVerticalOffset + addedHeight + NPC.gfxOffY);
        spriteBatch.Draw(npcTexture, mainDrawPos, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, spriteEffects, 0f);

        // 如果还未进入第二阶段，到此结束
        if (CurrentPhase < Phase.PhaseChange_1To2)
            return false;

        // ====== 狂暴阶段（ai[0] >= 4）额外鱼龙叠加绘制 ======
        npcTexture = TextureAssets.DukeFishron.Value;
        Color enrageOverlayColor = Color.Lerp(Color.White, Color.Yellow, 0.5f);
        blendTargetColor = Color.Yellow;
        colorBlendAmount = 1f;
        vortexIntensity = 0.5f;
        vortexMaxRadius = 10f;
        ghostStep = 1;

        // 根据特定 AI 状态调整叠加透明度
        if (CurrentPhase == Phase.PhaseChange_1To2)
        {
            float enrageFactor = (Timer1 - enrageTimerThreshold) / enrageTransitionDuration;
            blendTargetColor *= enrageFactor;
            enrageOverlayColor *= enrageFactor;
        }
        else if (CurrentBehavior == Behavior.Phase3_Teleport)
        {
            float pulse = Timer1 / 30f;
            if (pulse > 0.5f)
                pulse = 1f - pulse;
            pulse *= 2f;
            pulse = 1f - pulse;
            blendTargetColor *= pulse;
            enrageOverlayColor *= pulse;
        }

        // 狂暴阶段残影拖尾
        for (int ghostIndex = 1; ghostIndex < ghostCount; ghostIndex += ghostStep)
        {
            Color ghostColor = enrageOverlayColor;
            ghostColor = Color.Lerp(ghostColor, blendTargetColor, colorBlendAmount);
            ghostColor *= (ghostCount - ghostIndex) / 15f;

            Vector2 drawPos = NPC.oldPos[ghostIndex] + new Vector2(NPC.width, NPC.height) / 2f - screenPos;
            drawPos -= new Vector2(npcTexture.Width, npcTexture.Height / Main.npcFrameCount[type]) * NPC.scale / 2f;
            drawPos += origin * NPC.scale + new Vector2(0f, baseVerticalOffset + addedHeight + NPC.gfxOffY);

            spriteBatch.Draw(npcTexture, drawPos, NPC.frame, ghostColor, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
        }

        // 狂暴阶段漩涡特效
        for (int vortexIndex = 1; vortexIndex < vortexParticleCount; vortexIndex++)
        {
            Color vortexColor = enrageOverlayColor;
            vortexColor = Color.Lerp(vortexColor, blendTargetColor, colorBlendAmount);
            vortexColor = NPC.GetAlpha(vortexColor);
            vortexColor *= 1f - vortexIntensity;

            float angle = (float)vortexIndex / vortexParticleCount * MathHelper.TwoPi + NPC.rotation;
            Vector2 offset = angle.ToRotationVector2() * vortexMaxRadius * vortexIntensity;
            Vector2 drawPos = NPC.Center + offset - screenPos;
            drawPos -= new Vector2(npcTexture.Width, npcTexture.Height / Main.npcFrameCount[type]) * NPC.scale / 2f;
            drawPos += origin * NPC.scale + new Vector2(0f, baseVerticalOffset + addedHeight + NPC.gfxOffY);

            spriteBatch.Draw(npcTexture, drawPos, NPC.frame, vortexColor, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
        }

        // 狂暴叠加层本体
        spriteBatch.Draw(npcTexture, mainDrawPos, NPC.frame, enrageOverlayColor, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);

        return false;
    }

    public override void BossHeadSlot(ref int index)
    {
        if (Phase3)
            index = -1;
    }
}
