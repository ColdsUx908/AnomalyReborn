using static Anomalies.Bosses.Skeletron.SkeletronHead;

namespace Anomalies.Bosses.Skeletron;

public sealed class SkeletronHand : AnomalyNPCBehavior<SkeletronHand>
{
    protected override int AI32Capacity => 9;

    public override int ApplyingType => NPCID.SkeletronHand;

    public override bool ShouldProcess => base.ShouldProcess && AnomalyBossChangeToggler.Skeletron && AnomalyNPC.CustomAIType != 1;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        _ => true,
    };

    public enum Phase : byte
    {
        Phase1,
        Phase2,
        Phase3,
    }

    public enum Behavior : byte
    {
        Idle,
        VerticalSlap,
        HorizontalSlap,
    }


    public Phase CurrentPhase
    {
        get => (Phase)AnomalyNPC.AnomalyAI32[7].Byte0;
        set
        {
            if (CurrentPhase == value)
                return;
            AnomalyNPC.AnomalyAI32[7].Byte0 = (byte)value;
            AnomalyNPC.AIChanged32[7] = true;
            if (Main.netMode != NetmodeID.MultiplayerClient)
                NPC.netUpdate = true;
        }
    }

    public Behavior CurrentBehavior
    {
        get => (Behavior)AnomalyNPC.AnomalyAI32[7].Byte1;
        set
        {
            if (CurrentBehavior == value)
                return;
            AnomalyNPC.AnomalyAI32[7].Byte1 = (byte)value;
            AnomalyNPC.AIChanged32[7] = true;
            if (Main.netMode != NetmodeID.MultiplayerClient)
                NPC.netUpdate = true;
        }
    }

    // Idle: 0 before vertical slap, 1 before horizontal slap. Slap: 0 drawback, 1 swipe.
    public int CurrentLocalPhase
    {
        get => (int)NPC.ai[2];
        set
        {
            if (CurrentLocalPhase != value && Main.netMode != NetmodeID.MultiplayerClient)
                NPC.netUpdate = true;
            NPC.ai[2] = value;
        }
    }

    private ref float VerticalSide => ref AnomalyNPC.AnomalyAI32[0].F;
    private ref float SpawnTimer => ref AnomalyNPC.AnomalyAI32[1].F;
    private ref float SlapTelegraphTimer => ref AnomalyNPC.AnomalyAI32[2].F;
    private ref float SlapReadiness => ref AnomalyNPC.AnomalyAI32[3].F;
    private ref float ResyncTimer => ref AnomalyNPC.AnomalyAI32[8].F;

    public bool Phase2 => CurrentPhase >= Phase.Phase2;
    public bool Phase3 => CurrentPhase == Phase.Phase3;

    public ref float Timer => ref NPC.ai[3];

    public override void SetDefaultsFinal()
    {
        NPC.lifeMax = CalamityEnabled ? 1350 : 1125;
        BridgeUtils.ApplyCalamityHealthBoost(NPC);
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) =>
        AnomalyUtils.ApplyAnomalyBossDifficultyAndPlayerScaling(NPC, bossAdjustment);

    // NPC.ai[0..1]: side/owner; AI32[0..3]: vertical side, age, telegraph, readiness.
    // AI32[7]: packed phase/behavior; AI32[8]: periodic resync timer.
    public override bool PreAI()
    {
        using BossNetUpdateScope netUpdate = new(NPC);
        // Late joiners and predicted timers receive the same complete per-NPC snapshot.
        if (Main.netMode != NetmodeID.MultiplayerClient && ++ResyncTimer >= 60f)
        {
            ResyncTimer = 0f;
            NPC.netUpdate = true;
        }

        // Get a target
        if (NPC.target < 0 || NPC.target >= Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            NPC.TargetClosest();
        if (NPC.target < 0 || NPC.target >= Main.maxPlayers || !Main.player[NPC.target].active || Main.player[NPC.target].dead)
        {
            NPC.velocity.Y += 0.1f;
            NPC.EncourageDespawn(10);
            return false;
        }

        float yMultiplier = 1f;
        if (VerticalSide != 0f)
            yMultiplier = VerticalSide;
        yMultiplier *= 1.3f;

        // Inflict 0 damage for 3 seconds after spawning
        if (SpawnTimer < 180f)
        {
            SpawnTimer += 1f;
            if (SpawnTimer % 15f == 0f)
                NPC.netUpdate = true;

            NPC.damage = 0;
        }
        else
            NPC.damage = NPC.defDamage;

        NPC.spriteDirection = -Math.Sign(NPC.ai[0]);

        // Validate the owner before any array access; clients wait for the server's despawn.
        int headIndex = (int)NPC.ai[1];
        if (headIndex < 0 || headIndex >= Main.maxNPCs || !Main.npc[headIndex].active
            || Main.npc[headIndex].type != NPCID.SkeletronHead
            || Main.npc[headIndex].Anomaly.CustomAIType == 1)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.active = false;
                NPC.netUpdate = true;
            }
            return false;
        }
        float skeletronLifeRatio = Main.npc[headIndex].life / (float)Main.npc[headIndex].lifeMax;

        SkeletronHead head = SkeletronHead.GetInstance(Main.npc[headIndex]);
        bool cancelSlap = head.CurrentBehavior != SkeletronHead.Behavior.Hover || head.Timer >= ChargeGateValue;

        // Ultra starts with the former second wave's faster slaps and escalates as hands fall.
        const int CrossboneHandCount = 2;
        CurrentPhase = Ultra ? (head.CountLivingHands() <= CrossboneHandCount ? Phase.Phase3 : Phase.Phase2) : Phase.Phase1;

        float velocityMultiplier = MathHelper.Lerp(0.6f, 1f, skeletronLifeRatio);
        float velocityIncrement = MathHelper.Lerp(0.2f, 0.4f, 1f - skeletronLifeRatio);
        float handSwipeVelocity = MathHelper.Lerp(16f, 24f, 1f - skeletronLifeRatio);
        float deceleration = Ultra ? 0.78f : 0.82f;

        velocityMultiplier *= 0.75f;
        velocityIncrement *= 1.5f;
        handSwipeVelocity *= 1.35f;
        deceleration *= 0.75f;
        float handSwipeDistance = HandSwipeDistanceMaster;
        float handSwipeDuration = handSwipeDistance / handSwipeVelocity;
        float slapGateValue = HandSlapGateValue;

        float slapTimerIncrement = MathHelper.Lerp(1.5f, 3f, 1f - skeletronLifeRatio);
        if (Phase3)
            slapTimerIncrement *= 2.5f;
        else if (Phase2)
            slapTimerIncrement *= 2f;

        if (cancelSlap && CurrentBehavior != Behavior.Idle && CurrentLocalPhase == 0
            && Main.netMode != NetmodeID.MultiplayerClient)
        {
            CurrentLocalPhase = CurrentBehavior == Behavior.HorizontalSlap ? 1 : 0;
            CurrentBehavior = Behavior.Idle;
        }

        switch (CurrentBehavior)
        {
            case Behavior.Idle:
                Idle();
                break;
            case Behavior.VerticalSlap:
                if (CurrentLocalPhase == 0)
                    VerticalDrawback();
                else
                    VerticalSwipe();
                break;
            case Behavior.HorizontalSlap:
                if (CurrentLocalPhase == 0)
                    HorizontalDrawback();
                else
                    HorizontalSwipe();
                break;
        }

        return false;

        void Idle()
        {
            if (head.CurrentBehavior == SkeletronHead.Behavior.Despawn && NPC.timeLeft > 10)
                NPC.timeLeft = 10;

            if (cancelSlap)
            {
                if (Timer != 0f)
                {
                    Timer = 0f;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        NPC.netUpdate = true;
                }
                FadeSlapTelegraph();

                deceleration *= 0.75f;
                velocityIncrement *= 1.5f;

                NPC master = Main.npc[headIndex];
                Vector2 destination = master.Center + GetIdleOffset(-Math.Sign(NPC.ai[0]), NPC.height, master.height, yMultiplier, raised: true);
                UpdateIdleMovement(NPC, destination, velocityIncrement, velocityMultiplier, deceleration,
                    followVelocity: head.IsSpinning ? master.velocity : null);
            }
            else
            {
                if (SlapReadiness == 1f)
                {
                    SlapTelegraphTimer += slapTimerIncrement;
                    Timer += slapTimerIncrement;
                    if (Main.netMode != NetmodeID.MultiplayerClient && (Timer >= slapGateValue))
                    {
                        NPC.target = Main.npc[(int)NPC.ai[1]].target;
                        CurrentBehavior = CurrentLocalPhase == 0 ? Behavior.VerticalSlap : Behavior.HorizontalSlap;
                        CurrentLocalPhase = 0;
                        Timer = SlapTelegraphTimer = slapGateValue;
                        SlapReadiness = 0f;
                        NPC.netUpdate = true;
                    }
                }
                else
                {
                    FadeSlapTelegraph();
                }

                Vector2 destination = Main.npc[headIndex].Center + GetIdleOffset(-Math.Sign(NPC.ai[0]), NPC.height, Main.npc[headIndex].height, yMultiplier, raised: false);
                UpdateIdleMovement(NPC, destination, velocityIncrement, velocityMultiplier, deceleration);
            }

            UpdateIdleRotation(NPC, Main.npc[headIndex]);

            return;
        }

        void FadeSlapTelegraph()
        {
            if (SlapTelegraphTimer == 0f && SlapReadiness == 1f)
                return;

            SlapTelegraphTimer = Math.Max(0f, SlapTelegraphTimer - slapTimerIncrement * 2f);
            SlapReadiness = SlapTelegraphTimer == 0f ? 1f : 0f;
            if (SlapReadiness == 1f && Main.netMode != NetmodeID.MultiplayerClient)
                NPC.netUpdate = true;
        }

        void VerticalDrawback()
        {
            Vector2 handCurrentPosition = NPC.Center;
            float handDrawbackXPos = Main.npc[(int)NPC.ai[1]].Center.X - 200f * NPC.ai[0] - handCurrentPosition.X;
            float handDrawbackYPos = Main.npc[(int)NPC.ai[1]].Top.Y + 230f - handCurrentPosition.Y;
            float handDrawbackDist = (float)Math.Sqrt(handDrawbackXPos * handDrawbackXPos + handDrawbackYPos * handDrawbackYPos);
            NPC.rotation = (float)Math.Atan2(handDrawbackYPos, handDrawbackXPos) + MathHelper.PiOver2;
            UpdateVerticalDrawbackMovement(NPC, velocityIncrement);

            if (Main.netMode != NetmodeID.MultiplayerClient && (NPC.Top.Y < Main.npc[(int)NPC.ai[1]].Top.Y - 200f))
            {
                CurrentBehavior = Behavior.VerticalSlap;
                CurrentLocalPhase = 1;
                Timer = 0f;
                NPC.velocity = (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.UnitY) * handSwipeVelocity;
                NPC.netUpdate = true;
            }
        }

        void VerticalSwipe()
        {
            Timer += 1f;
            if (Main.netMode != NetmodeID.MultiplayerClient && (Timer >= handSwipeDuration || Vector2.Distance(Main.npc[(int)NPC.ai[1]].Center, NPC.Center) > handSwipeDistance || cancelSlap))
            {
                CurrentBehavior = Behavior.Idle;
                CurrentLocalPhase = 1;
                Timer = 0f;
                NPC.netUpdate = true;

                FireSwipeProjectile();
            }
        }

        void HorizontalDrawback()
        {
            Vector2 handStrikeCurrentPos = NPC.Center;
            float handStrikeXPos = Main.npc[(int)NPC.ai[1]].Center.X - 200f * NPC.ai[0] - handStrikeCurrentPos.X;
            float handStrikeYPos = Main.npc[(int)NPC.ai[1]].Top.Y + 230f - handStrikeCurrentPos.Y;
            float handStrikeDist = (float)Math.Sqrt(handStrikeXPos * handStrikeXPos + handStrikeYPos * handStrikeYPos);
            NPC.rotation = (float)Math.Atan2(handStrikeYPos, handStrikeXPos) + MathHelper.PiOver2;
            UpdateHorizontalDrawbackMovement(NPC, velocityIncrement);

            if (Main.netMode != NetmodeID.MultiplayerClient && (NPC.Center.X < Main.npc[(int)NPC.ai[1]].Center.X - 500f || NPC.Center.X > Main.npc[(int)NPC.ai[1]].Center.X + 500f))
            {
                CurrentBehavior = Behavior.HorizontalSlap;
                CurrentLocalPhase = 1;
                Timer = 0f;
                NPC.velocity = (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.UnitY) * handSwipeVelocity;
                NPC.netUpdate = true;
            }
        }

        void HorizontalSwipe()
        {
            Timer += 1f;
            if (Main.netMode != NetmodeID.MultiplayerClient && (Timer >= handSwipeDuration || Vector2.Distance(Main.npc[(int)NPC.ai[1]].Center, NPC.Center) > handSwipeDistance || cancelSlap))
            {
                CurrentBehavior = Behavior.Idle;
                CurrentLocalPhase = 0;
                Timer = 0f;
                NPC.netUpdate = true;

                FireSwipeProjectile();
            }
        }

        void FireSwipeProjectile()
        {
            Player target = Main.player[NPC.target];
            if (!Phase2 || cancelSlap || Vector2.Distance(target.Center, NPC.Center) <= 160f
                || !Collision.CanHit(NPC.Center, 1, 1, target.position, target.width, target.height))
                return;

            float speed = handSwipeVelocity * (Phase3 ? 0.6f : 0.2f);
            Vector2 velocity = NPC.Center.DirectionTo(target.Center) * speed;
            if (Phase3)
                Crossbone.Fire(NPC, NPC.Center, velocity, SkullDamage, Skull.UltraMaxSpeed);
            else
                Projectile.NewProjectileAction(SourceAI, NPC.Center, velocity, ProjectileID.Skull, SkullDamage, 0f, Main.myPlayer, action: p =>
                {
                    Skull.InitializeFlight(p, Skull.Homing, Skull.MaxSpeed, Skull.Acceleration);
                    Skull.SetSource(p, NPC);
                    p.timeLeft = 600;
                });
        }
    }

    /// <summary>
    /// 普通手部的待机位置；保留原版以实体顶部计算高度的语义。
    /// </summary>
    internal static Vector2 GetIdleOffset(int side, int handHeight, int headHeight, float yMultiplier = 1.3f, bool raised = false) =>
        new(side * (raised ? 120f : 200f), (raised ? -100f : 230f) * yMultiplier + (handHeight - headHeight) * 0.5f);

    /// <summary>
    /// 普通与传说之夜手部共用的逐轴待机加速、反向制动和速度限制。
    /// </summary>
    internal static void UpdateIdleMovement(NPC hand, Vector2 destination, float velocityIncrement, float velocityMultiplier, float deceleration,
        Vector2? followVelocity = null)
    {
        // 启用跟随时，接近待机点后改为相对头部的阻尼收敛，避免固定加速度反复跨越目标。
        // 此分支直接替代逐轴加速，不在同一帧叠加两套归位。
        if (followVelocity is Vector2 ownerVelocity)
        {
            const float SettlingRadius = 48f;
            const float SettlingSpeed = 6f;
            const float VelocityDamping = 0.65f;
            const float PositionResponse = 0.035f;
            Vector2 relativeOffset = hand.Center - destination + ownerVelocity;
            Vector2 relativeVelocity = hand.velocity - ownerVelocity;
            if (relativeOffset.LengthSquared() <= SettlingRadius * SettlingRadius
                && relativeVelocity.LengthSquared() <= SettlingSpeed * SettlingSpeed)
            {
                Vector2 nextVelocity = relativeVelocity * VelocityDamping - relativeOffset * PositionResponse;
                hand.velocity = ownerVelocity + nextVelocity;
                return;
            }
        }

        float maxX = velocityIncrement * 100f * velocityMultiplier;
        float maxY = velocityIncrement * 100f * velocityMultiplier;

        if (hand.Center.Y > destination.Y)
        {
            if (hand.velocity.Y > 0f)
                hand.velocity.Y *= deceleration;
            hand.velocity.Y -= velocityIncrement;
            if (hand.velocity.Y > maxY)
                hand.velocity.Y = maxY;
        }
        else if (hand.Center.Y < destination.Y)
        {
            if (hand.velocity.Y < 0f)
                hand.velocity.Y *= deceleration;
            hand.velocity.Y += velocityIncrement;
            if (hand.velocity.Y < -maxY)
                hand.velocity.Y = -maxY;
        }

        if (hand.Center.X > destination.X)
        {
            if (hand.velocity.X > 0f)
                hand.velocity.X *= deceleration;
            hand.velocity.X -= velocityIncrement;
            if (hand.velocity.X > maxX)
                hand.velocity.X = maxX;
        }

        if (hand.Center.X < destination.X)
        {
            if (hand.velocity.X < 0f)
                hand.velocity.X *= deceleration;
            hand.velocity.X += velocityIncrement;
            if (hand.velocity.X < -maxX)
                hand.velocity.X = -maxX;
        }
    }

    /// <summary>普通与传说之夜待机手部共用的朝向，使用当前位置和原有头部参考点。</summary>
    internal static void UpdateIdleRotation(NPC hand, NPC master)
    {
        Vector2 handCenter = hand.Center;
        float offsetX = master.Center.X - 200f * hand.ai[0] - handCenter.X;
        float offsetY = master.Top.Y + 230f - handCenter.Y;
        hand.rotation = (float)Math.Atan2(offsetY, offsetX) + MathHelper.PiOver2;
    }

    /// <summary>普通与传说之夜下拍前的抬手运动。</summary>
    internal static void UpdateVerticalDrawbackMovement(NPC hand, float velocityIncrement)
    {
        hand.velocity.X *= 0.95f;
        hand.velocity.Y -= velocityIncrement;
        if (hand.velocity.Y < -14f)
            hand.velocity.Y = -14f;
        else if (hand.velocity.Y > 10f)
            hand.velocity.Y = 10f;
    }

    /// <summary>普通与传说之夜横扫前的侧向蓄势运动。</summary>
    internal static void UpdateHorizontalDrawbackMovement(NPC hand, float velocityIncrement)
    {
        hand.velocity.Y *= 0.95f;
        hand.velocity.X += velocityIncrement * -hand.ai[0];
        if (hand.velocity.X < -10f)
            hand.velocity.X = -10f;
        else if (hand.velocity.X > 14f)
            hand.velocity.X = 14f;
    }

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        float beginTelegraphGateValue = HandSlapGateValue - HandSlapTelegraphTime;
        if (SlapTelegraphTimer > beginTelegraphGateValue)
        {
            float colorScale = MathHelper.Clamp((SlapTelegraphTimer - beginTelegraphGateValue) / HandSlapTelegraphTime, 0f, 1f);
            Color drawColor2 = new Color(150, 150, 150, 0) * colorScale;
            Vector2 halfSize = NPC.frame.Size() / 2;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Vector2 glowOffset = Vector2.UnitY * 8f;
            for (int i = 0; i < 2; i++)
            {
                spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, NPC.Center - screenPos + new Vector2(0, NPC.gfxOffY) - glowOffset, NPC.frame,
                    drawColor2, NPC.rotation, halfSize, NPC.scale, spriteEffects, 0f);
            }
        }
    }

}
