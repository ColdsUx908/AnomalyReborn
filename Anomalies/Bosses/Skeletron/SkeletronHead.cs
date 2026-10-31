namespace Anomalies.Bosses.Skeletron;

public sealed partial class SkeletronHead : AnomalyNPCBehavior<SkeletronHead>
{
    protected override int AI32Capacity => 9;

    public override int ApplyingType => NPCID.SkeletronHead;

    public override bool ShouldProcess => base.ShouldProcess && AnomalyNPC.CustomAIType != 1;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        _ => true,
    };

    public enum Phase : byte
    {
        Initialize,
        Phase1,
        Phase2,
        Phase3,
        Phase3_2,
        Phase4,
        Phase5,
    }

    public enum Behavior : byte
    {
        Hover,
        SpinCharge,
        Enraged,
        Despawn = byte.MaxValue,
    }


    public Phase CurrentPhase
    {
        get => (Phase)AI_Union_0.Byte0;
        set
        {
            if (CurrentPhase != value && Main.netMode != NetmodeID.MultiplayerClient)
                NPC.netUpdate = true;
            Union32 union = AI_Union_0;
            union.Byte0 = (byte)value;
            AI_Union_0 = union;
        }
    }

    public Behavior CurrentBehavior
    {
        get => (Behavior)AI_Union_0.Byte1;
        set
        {
            if (CurrentBehavior != value && Main.netMode != NetmodeID.MultiplayerClient)
                NPC.netUpdate = true;
            Union32 union = AI_Union_0;
            union.Byte1 = (byte)value;
            AI_Union_0 = union;
        }
    }

    // Hover: 0 following, 1 retreating. SpinCharge: 0 tracking, 1 committed.
    public int CurrentLocalPhase
    {
        get => (int)NPC.ai[1];
        set
        {
            if (CurrentLocalPhase != value && Main.netMode != NetmodeID.MultiplayerClient)
                NPC.netUpdate = true;
            NPC.ai[1] = value;
        }
    }

    private ref float AttackTimer => ref AnomalyNPC.AnomalyAI32[1].F;
    private ref float TeleportX => ref AnomalyNPC.AnomalyAI32[2].F;
    private ref float TeleportY => ref AnomalyNPC.AnomalyAI32[3].F;
    private ref float ChargeReadiness => ref AnomalyNPC.AnomalyAI32[4].F;
    private ref float ChargeTelegraphTimer => ref AnomalyNPC.AnomalyAI32[5].F;
    private ref float RetreatTimer => ref AnomalyNPC.AnomalyAI32[6].F;
    private ref float ResyncTimer => ref AnomalyNPC.AnomalyAI32[8].F;

    public const float Phase3LifeRatio = 0.9f;
    public const float RespawnHandsLifeRatio = 0.5f;
    public const float Phase4LifeRatio = 0.4f;
    public const float Phase5LifeRatio = 0.15f;

    public ref float Timer => ref NPC.ai[2];
    public ref float TeleportTimer => ref NPC.ai[3];
    public bool Phase2 => CurrentPhase >= Phase.Phase2;
    public bool Phase3 => CurrentPhase >= Phase.Phase3;
    public bool RespawnHands => CurrentPhase >= Phase.Phase3_2;
    public bool Phase4 => CurrentPhase >= Phase.Phase4;
    public bool Phase5 => CurrentPhase >= Phase.Phase5;

    public const float ChargeGateValue = 600f;
    public const float ChargeTelegraphTime = 120f;
    public const float HandSlapGateValue = 300f;
    public const float HandSlapTelegraphTime = 120f;
    public const float HandSwipeDistanceMaster = 1280f;
    public const float SpinDamageMult = 1.3f;
    // Vanilla hostile projectiles apply their own difficulty scaling.
    public const int SkullDamage = 17;

    // NPC.ai: [0] packed phase/behavior, [1] local phase, [2] timer, [3] teleport timer.
    // AI32: [0] hands respawned, [1] attack age, [2..3] teleport destination,
    // [4] charge readiness, [5] charge telegraph, [6] retreat timer, [8] resync timer.
    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        // Telegraph for charges
        float beginTelegraphGateValue = ChargeGateValue - ChargeTelegraphTime;
        if (ChargeTelegraphTimer > beginTelegraphGateValue)
        {
            float colorScale = MathHelper.Clamp((ChargeTelegraphTimer - beginTelegraphGateValue) / ChargeTelegraphTime, 0f, 1f);
            Color drawColor2 = new Color(150, 150, 150, 0) * colorScale;
            Vector2 halfSize = NPC.frame.Size() / 2;
            SpriteEffects spriteEffects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            for (int i = 0; i < 2; i++)
            {
                spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, NPC.Center - screenPos + new Vector2(0, NPC.gfxOffY), NPC.frame,
                    drawColor2, NPC.rotation, halfSize, NPC.scale, spriteEffects, 0f);
            }
        }
    }

    public static void NormalMovement(NPC npc, Player target, float maxSpeedX, float maxSpeedY, float accelerationX, float accelerationY, float targetDistanceAboveTarget, float rotationFactor = 15f)
    {
        npc.Face(target);

        // Y 方向：保持自己在玩家上方
        if (npc.Top.Y > target.Top.Y - targetDistanceAboveTarget)
        {
            if (npc.velocity.Y > 0f)
                npc.velocity.Y *= 0.98f;

            npc.velocity.Y -= accelerationY;
            if (npc.velocity.Y > maxSpeedY)
                npc.velocity.Y = maxSpeedY;
        }
        else if (npc.Top.Y < target.Top.Y - targetDistanceAboveTarget)
        {
            if (npc.velocity.Y < 0f)
                npc.velocity.Y *= 0.98f;

            npc.velocity.Y += accelerationY;
            if (npc.velocity.Y < -maxSpeedY)
                npc.velocity.Y = -maxSpeedY;
        }

        // X 方向：水平追踪玩家
        if (npc.Center.X > target.Center.X)
        {
            if (npc.velocity.X > 0f)
                npc.velocity.X *= 0.98f;

            npc.velocity.X -= accelerationX;
            if (npc.velocity.X > maxSpeedX)
                npc.velocity.X = maxSpeedX;
        }

        if (npc.Center.X < target.Center.X)
        {
            if (npc.velocity.X < 0f)
                npc.velocity.X *= 0.98f;

            npc.velocity.X += accelerationX;
            if (npc.velocity.X < -maxSpeedX)
                npc.velocity.X = -maxSpeedX;
        }

        npc.rotation = npc.velocity.X / rotationFactor;
    }

    public static void RotatingMovement(NPC npc, Player target, float rotationSpeed, float moveSpeed)
    {
        // 旋转：围绕自身朝向持续旋转
        npc.rotation += npc.direction * rotationSpeed;

        // 计算指向玩家的向量
        Vector2 targetVector = target.Center - npc.Center;
        float distance = targetVector.Length();

        if (distance > 0f)
        {
            // 归一化后乘以移动速度，等价于原代码的 8f / distance 缩放
            float speedFactor = moveSpeed / distance;
            npc.velocity.X = targetVector.X * speedFactor;
            npc.velocity.Y = targetVector.Y * speedFactor;
        }
        else
        {
            npc.velocity = Vector2.Zero;
        }
    }

    public static void FireSkull(NPC npc, Player target, int amount, float radian, float skullProjSpeed, int damage, bool fireFromMouth = true, bool addNPCVelocity = true, int ai0 = 0)
    {
        Vector2 velocity = npc.GetVelocityTowards(target, skullProjSpeed) + (addNPCVelocity ? npc.velocity : Vector2.Zero);
        Vector2 position = npc.Center + velocity;
        if (fireFromMouth)
            position += new Vector2(0f, 25f * npc.scale).RotatedBy(npc.rotation);

        int type = ProjectileID.Skull;

        if (amount == 1)
            Projectile.NewProjectileAction(npc.GetSource_FromAI(), position, velocity, type, damage, 0f, action: FireSkullAction);
        else
            Projectile.NewProjectilesArc(amount, radian, npc.GetSource_FromAI(), position, velocity, type, damage, 0f, action: FireSkullAction);

        void FireSkullAction(Projectile p)
        {
            p.ai[0] = ai0;
            p.timeLeft = 600;
        }
    }
}