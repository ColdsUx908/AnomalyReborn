using Anomalies.Visuals.BossBar;
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

    internal const float SlapGateValue = 300f;
    internal const float SlapTelegraphTime = 120f;
    internal const float SwipeDistance = 1280f;

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

    private float HorizontalSide => NPC.ai[0];
    private int OwnerIndex => (int)NPC.ai[1];

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
        NPC.lifeMax = CalamityEnabled ? 900 : 600;
        BridgeUtils.ApplyCalamityHealthBoost(NPC);
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) =>
        AnomalyUtils.ApplyAnomalyBossDifficultyAndPlayerScaling(NPC, bossAdjustment);

    // NPC.ai[0..1]: side/owner; AI32[0..3]: vertical side, age, telegraph, readiness.
    // AI32[7]: packed phase/behavior; AI32[8]: periodic resync timer.
    public override bool PreAI()
    {
        NPC.damage = 0;
        NPC.GivenName = string.Empty;
        AnomalyNPC.BossBar_Enabled = true;
        AnomalyNPC.BossBar_IsSmall = true;
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

        // Inflict 0 damage for 3 seconds after spawning.
        bool spawnProtectionEnded = SpawnTimer >= 180f;
        if (SpawnTimer < 180f)
        {
            SpawnTimer += 1f;
            if (SpawnTimer % 15f == 0f)
                NPC.netUpdate = true;
        }

        NPC.spriteDirection = -Math.Sign(HorizontalSide);

        // Validate the owner before any array access; clients wait for the server's despawn.
        int headIndex = OwnerIndex;
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
        NPC master = Main.npc[headIndex];
        float skeletronLifeRatio = master.life / (float)master.lifeMax;

        SkeletronHead head = SkeletronHead.GetInstance(master);
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
        float handSwipeDistance = SwipeDistance;
        float handSwipeDuration = handSwipeDistance / handSwipeVelocity;
        float slapGateValue = SlapGateValue;

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
                    Swipe(nextIdleStep: 1);
                break;
            case Behavior.HorizontalSlap:
                if (CurrentLocalPhase == 0)
                    HorizontalDrawback();
                else
                    Swipe(nextIdleStep: 0);
                break;
        }

        // Only the forward slap can deal contact damage; windup and idle are harmless.
        if (spawnProtectionEnded && CurrentBehavior is Behavior.VerticalSlap or Behavior.HorizontalSlap && CurrentLocalPhase == 1)
            NPC.damage = NPC.defDamage;

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

                Vector2 destination = master.Center + SkeletronKinematics.GetIdleOffset(-Math.Sign(HorizontalSide), NPC.height, master.height, yMultiplier, raised: true);
                SkeletronKinematics.UpdateIdleMovement(NPC, destination, velocityIncrement, velocityMultiplier, deceleration,
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
                        NPC.target = master.target;
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

                Vector2 destination = master.Center + SkeletronKinematics.GetIdleOffset(-Math.Sign(HorizontalSide), NPC.height, master.height, yMultiplier, raised: false);
                SkeletronKinematics.UpdateIdleMovement(NPC, destination, velocityIncrement, velocityMultiplier, deceleration);
            }

            SkeletronKinematics.UpdateIdleRotation(NPC, Main.npc[headIndex]);

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
            SkeletronKinematics.UpdateIdleRotation(NPC, master);
            SkeletronKinematics.UpdateVerticalDrawbackMovement(NPC, velocityIncrement);

            if (Main.netMode != NetmodeID.MultiplayerClient && (NPC.Top.Y < master.Top.Y - 200f))
            {
                CurrentBehavior = Behavior.VerticalSlap;
                CurrentLocalPhase = 1;
                Timer = 0f;
                NPC.velocity = (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.UnitY) * handSwipeVelocity;
                NPC.netUpdate = true;
            }
        }

        void Swipe(int nextIdleStep)
        {
            Timer += 1f;
            if (Main.netMode != NetmodeID.MultiplayerClient && (Timer >= handSwipeDuration || Vector2.Distance(master.Center, NPC.Center) > handSwipeDistance || cancelSlap))
            {
                CurrentBehavior = Behavior.Idle;
                CurrentLocalPhase = nextIdleStep;
                Timer = 0f;
                NPC.netUpdate = true;

                FireSwipeProjectile();
            }
        }

        void HorizontalDrawback()
        {
            SkeletronKinematics.UpdateIdleRotation(NPC, master);
            SkeletronKinematics.UpdateHorizontalDrawbackMovement(NPC, velocityIncrement);

            if (Main.netMode != NetmodeID.MultiplayerClient && (NPC.Center.X < master.Center.X - 500f || NPC.Center.X > master.Center.X + 500f))
            {
                CurrentBehavior = Behavior.HorizontalSlap;
                CurrentLocalPhase = 1;
                Timer = 0f;
                NPC.velocity = (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.UnitY) * handSwipeVelocity;
                NPC.netUpdate = true;
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
                Crossbone.Fire(NPC, NPC.Center, velocity, SkullDamage, Skull.MaxSpeed);
            Projectile.NewProjectileAction(SourceAI, NPC.Center, velocity, ProjectileID.Skull, SkullDamage, 0f, Main.myPlayer, action: p =>
            {
                Skull.InitializeFlight(p, Skull.Homing, Skull.MaxSpeed, Skull.Acceleration);
                Skull.SetSource(p, NPC);
                p.timeLeft = 600;
            });
        }
    }

    public override void ApplyCustomMainBossBarShader(BossHealthBar newBar, SpriteBatch spriteBatch, Rectangle destinationRectangle) =>
        ApplyBossBarShader(destinationRectangle);

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        float beginTelegraphGateValue = SlapGateValue - SlapTelegraphTime;
        if (SlapTelegraphTimer > beginTelegraphGateValue)
        {
            float colorScale = MathHelper.Clamp((SlapTelegraphTimer - beginTelegraphGateValue) / SlapTelegraphTime, 0f, 1f);
            Color telegraphColor = new Color(150, 150, 150, 0) * colorScale;
            Vector2 halfSize = NPC.frame.Size() / 2;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Vector2 glowOffset = Vector2.UnitY * 8f;
            for (int i = 0; i < 2; i++)
            {
                spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, NPC.Center - screenPos + new Vector2(0, NPC.gfxOffY) - glowOffset, NPC.frame,
                    telegraphColor, NPC.rotation, halfSize, NPC.scale, spriteEffects, 0f);
            }
        }
    }

}

// ModifyTypeName 未接入 Behavior 调度，使用无实例数据的 GlobalNPC 覆盖类型名。
public sealed class SkeletronHandName : GlobalNPC
{
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.SkeletronHand;

    public override void ModifyTypeName(NPC npc, ref string typeName)
    {
        if (SkeletronHand.GetInstance(npc).ShouldProcess)
            typeName = Language.GetTextValue("Mods.Anomalies.Bosses.Skeletron.SkeletronHand.DisplayName");
    }
}
