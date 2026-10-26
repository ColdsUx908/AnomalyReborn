using static Anomalies.Bosses.Skeletron.SkeletronHead;

namespace Anomalies.Bosses.Skeletron;

public sealed class SkeletronHand : AnomalyNPCBehavior<SkeletronHand>
{
    public override int ApplyingType => NPCID.SkeletronHand;

    public override bool ShouldProcess => base.ShouldProcess && AnomalyNPC.CustomAIType != 1;

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

    // NPC.ai[0..1]: side/owner; AI32[0..3]: vertical side, age, telegraph, readiness.
    // AI32[7]: packed phase/behavior; AI32[8]: periodic resync timer.
    public override bool PreAI()
    {
        using Anomalies.DataStructures.BossNetUpdateScope netUpdate = new(NPC);
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

        // This bool exists for fairness so the hands don't slap when Skeletron is in phase 3 and getting ready to do the new charge
        SkeletronHead head = SkeletronHead.GetInstance(Main.npc[headIndex]);
        bool cancelSlap = head.Timer >= ChargeGateValue;

        // Fire skulls from hands at the end of each slap phase (master mode only)
        CurrentPhase = skeletronLifeRatio < 0.3f ? Phase.Phase3
            : skeletronLifeRatio < 0.5f ? Phase.Phase2 : Phase.Phase1;


        // Attack far more often if still alive


        float velocityMultiplier = MathHelper.Lerp(0.6f, 1f, skeletronLifeRatio);
        float velocityIncrement = MathHelper.Lerp(0.2f, 0.4f, 1f - skeletronLifeRatio);
        float handSwipeVelocity = MathHelper.Lerp(16f, 24f, 1f - skeletronLifeRatio);
        float deceleration = Main.getGoodWorld ? 0.78f : 0.82f;

        velocityMultiplier *= 0.75f;
        velocityIncrement *= 1.5f;
        handSwipeVelocity *= 1.35f;
        deceleration *= 0.75f;
        float handSwipeDistance = HandSwipeDistanceMaster;
        float handSwipeDuration = handSwipeDistance / handSwipeVelocity;
        float slapGateValue = HandSlapGateValue;

        float slapTimerIncrement = MathHelper.Lerp(1.5f, 3f, 1f - skeletronLifeRatio);
        if (Phase3)
            slapTimerIncrement *= (2.5f);
        else if (Phase2)
            slapTimerIncrement *= (2f);

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

            if (head.CurrentBehavior != SkeletronHead.Behavior.Hover || cancelSlap)
            {
                deceleration *= 0.75f;
                velocityIncrement *= 1.5f;

                float maxX = velocityIncrement * 100f * velocityMultiplier;
                float maxY = velocityIncrement * 100f * velocityMultiplier;

                if (NPC.Top.Y > Main.npc[(int)NPC.ai[1]].Top.Y - 100f * yMultiplier)
                {
                    if (NPC.velocity.Y > 0f)
                        NPC.velocity.Y *= deceleration;
                    NPC.velocity.Y -= velocityIncrement;
                    if (NPC.velocity.Y > maxY)
                        NPC.velocity.Y = maxY;
                }
                else if (NPC.Top.Y < Main.npc[(int)NPC.ai[1]].Top.Y - 100f * yMultiplier)
                {
                    if (NPC.velocity.Y < 0f)
                        NPC.velocity.Y *= deceleration;
                    NPC.velocity.Y += velocityIncrement;
                    if (NPC.velocity.Y < -maxY)
                        NPC.velocity.Y = -maxY;
                }

                if (NPC.Center.X > Main.npc[(int)NPC.ai[1]].Center.X - 120f * NPC.ai[0])
                {
                    if (NPC.velocity.X > 0f)
                        NPC.velocity.X *= deceleration;
                    NPC.velocity.X -= velocityIncrement;
                    if (NPC.velocity.X > maxX)
                        NPC.velocity.X = maxX;
                }

                if (NPC.Center.X < Main.npc[(int)NPC.ai[1]].Center.X - 120f * NPC.ai[0])
                {
                    if (NPC.velocity.X < 0f)
                        NPC.velocity.X *= deceleration;
                    NPC.velocity.X += velocityIncrement;
                    if (NPC.velocity.X < -maxX)
                        NPC.velocity.X = -maxX;
                }
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
                    SlapTelegraphTimer -= slapTimerIncrement * 2f;
                    if (SlapTelegraphTimer <= 0f)
                    {
                        SlapTelegraphTimer = 0f;
                        SlapReadiness = 1f;
                        NPC.netUpdate = true;
                    }
                }

                float maxX = velocityIncrement * 100f * velocityMultiplier;
                float maxY = velocityIncrement * 100f * velocityMultiplier;

                if (NPC.Top.Y > Main.npc[(int)NPC.ai[1]].Top.Y + 230f * yMultiplier)
                {
                    if (NPC.velocity.Y > 0f)
                        NPC.velocity.Y *= deceleration;
                    NPC.velocity.Y -= velocityIncrement;
                    if (NPC.velocity.Y > maxY)
                        NPC.velocity.Y = maxY;
                }
                else if (NPC.Top.Y < Main.npc[(int)NPC.ai[1]].Top.Y + 230f * yMultiplier)
                {
                    if (NPC.velocity.Y < 0f)
                        NPC.velocity.Y *= deceleration;
                    NPC.velocity.Y += velocityIncrement;
                    if (NPC.velocity.Y < -maxY)
                        NPC.velocity.Y = -maxY;
                }

                if (NPC.Center.X > Main.npc[(int)NPC.ai[1]].Center.X - 200f * NPC.ai[0])
                {
                    if (NPC.velocity.X > 0f)
                        NPC.velocity.X *= deceleration;
                    NPC.velocity.X -= velocityIncrement;
                    if (NPC.velocity.X > maxX)
                        NPC.velocity.X = maxX;
                }

                if (NPC.Center.X < Main.npc[(int)NPC.ai[1]].Center.X - 200f * NPC.ai[0])
                {
                    if (NPC.velocity.X < 0f)
                        NPC.velocity.X *= deceleration;
                    NPC.velocity.X += velocityIncrement;
                    if (NPC.velocity.X < -maxX)
                        NPC.velocity.X = -maxX;
                }
            }

            Vector2 handCurrentPos = NPC.Center;
            float handIdleXPos = Main.npc[(int)NPC.ai[1]].Center.X - 200f * NPC.ai[0] - handCurrentPos.X;
            float handIdleYPos = Main.npc[(int)NPC.ai[1]].Top.Y + 230f - handCurrentPos.Y;
            float handIdleDist = (float)Math.Sqrt(handIdleXPos * handIdleXPos + handIdleYPos * handIdleYPos);
            NPC.rotation = (float)Math.Atan2(handIdleYPos, handIdleXPos) + MathHelper.PiOver2;

            return;
        }

        void VerticalDrawback()
        {
            Vector2 handCurrentPosition = NPC.Center;
            float handDrawbackXPos = Main.npc[(int)NPC.ai[1]].Center.X - 200f * NPC.ai[0] - handCurrentPosition.X;
            float handDrawbackYPos = Main.npc[(int)NPC.ai[1]].Top.Y + 230f - handCurrentPosition.Y;
            float handDrawbackDist = (float)Math.Sqrt(handDrawbackXPos * handDrawbackXPos + handDrawbackYPos * handDrawbackYPos);
            NPC.rotation = (float)Math.Atan2(handDrawbackYPos, handDrawbackXPos) + MathHelper.PiOver2;
            NPC.velocity.X *= 0.95f;
            NPC.velocity.Y -= velocityIncrement;

            if (NPC.velocity.Y < -14f)
                NPC.velocity.Y = -14f;
            else if (NPC.velocity.Y > 10f)
                NPC.velocity.Y = 10f;

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

                // Spawn projectiles
                if (Collision.CanHit(NPC.Center, 1, 1, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height) && !cancelSlap)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        if (Phase2 && Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) > 160f)
                        {
                            float skullProjSpeed = handSwipeVelocity * (Phase3 ? 0.6f : 0.2f);
                            Vector2 initialProjectileVelocity = NPC.Center.DirectionTo(Main.player[NPC.target].Center) * skullProjSpeed;
                            int type = ProjectileID.Skull;
                            Projectile.NewProjectileAction(NPC.GetSource_FromAI(), NPC.Center, initialProjectileVelocity, type, SkullDamage, 0f, Main.myPlayer, action: p =>
                            {
                                p.ai[0] = Phase3 ? Skull.AcceleratingCrossbone : Skull.Homing;
                                p.timeLeft = 600;
                            });
                        }
                    }
                }
            }
        }

        void HorizontalDrawback()
        {
            Vector2 handStrikeCurrentPos = NPC.Center;
            float handStrikeXPos = Main.npc[(int)NPC.ai[1]].Center.X - 200f * NPC.ai[0] - handStrikeCurrentPos.X;
            float handStrikeYPos = Main.npc[(int)NPC.ai[1]].Top.Y + 230f - handStrikeCurrentPos.Y;
            float handStrikeDist = (float)Math.Sqrt(handStrikeXPos * handStrikeXPos + handStrikeYPos * handStrikeYPos);
            NPC.rotation = (float)Math.Atan2(handStrikeYPos, handStrikeXPos) + MathHelper.PiOver2;
            NPC.velocity.Y *= 0.95f;
            NPC.velocity.X += velocityIncrement * -NPC.ai[0];

            if (NPC.velocity.X < -10f)
                NPC.velocity.X = -10f;
            else if (NPC.velocity.X > 14f)
                NPC.velocity.X = 14f;

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

                // Spawn projectiles
                if (Collision.CanHit(NPC.Center, 1, 1, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height) && !cancelSlap)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        if (Phase2 && Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) > 160f)
                        {
                            float skullProjSpeed = handSwipeVelocity * (Phase3 ? 0.6f : 0.2f);
                            Vector2 initialProjectileVelocity = NPC.Center.DirectionTo(Main.player[NPC.target].Center) * skullProjSpeed;
                            int type = ProjectileID.Skull;
                            Projectile.NewProjectileAction(NPC.GetSource_FromAI(), NPC.Center, initialProjectileVelocity, type, SkullDamage, 0f, Main.myPlayer, action: p =>
                            {
                                p.ai[0] = Phase3 ? Skull.AcceleratingCrossbone : Skull.Homing;
                                p.timeLeft = 600;
                            });
                        }
                    }
                }
            }
        }
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