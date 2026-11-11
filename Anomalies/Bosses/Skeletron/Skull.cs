namespace Anomalies.Bosses.Skeletron;

public sealed partial class Skull : AnomalyProjectileBehavior<Skull>
{
    // ai[0]：飞行模式；ai[1]：追踪计时；ai[2]：死亡效果标记。
    public const int Homing = 0;
    public const int Straight = 2;

    internal const int DefaultHomingStartFrame = 31;
    internal const int DefaultHomingDuration = 79;

    internal const float RegularMaxSpeed = 15f;
    internal const float UltraMaxSpeed = 18f;
    internal const float RegularAcceleration = 1.015f;
    internal const float UltraAcceleration = 1.02f;
    public static float MaxSpeed => Ultra ? UltraMaxSpeed : RegularMaxSpeed;
    public static float Acceleration => Ultra ? UltraAcceleration : RegularAcceleration;

    internal const float SuppressDeathEffectsMarker = -7f;

    private const int MaxSpeedSlot = 0;
    private const int AccelerationSlot = 1;
    private const int HomingStartFrameSlot = 2;
    private const int HomingDurationSlot = 3;
    private const int TrailTypeSlot = 4;
    private const int SourceHeadSlot = 5;
    private const int Phase3SourceSlot = 6;
    private const int FlightDataCapacity = 7;
    private const int WarningSkullLifetime = 600;
    private const float WarningSkullSpeed = 15f;
    private const float WarningSkullMaxSpeed = 22.5f;
    private const float WarningSkullAcceleration = 1.12f;
    private const int FastUpdatesPerTick = 4;
    private const float FastMaxSpeed = 45f;

    private enum TrailType
    {
        Blood,
        Beam,
    }

    protected override int AI32Capacity => FlightDataCapacity;

    public override bool ShouldProcess => base.ShouldProcess && (Projectile.ai[0] >= 0f || AnomalyBossChangeToggler.Skeletron);
    public override int ApplyingType => ProjectileID.Skull;

    public override bool AllowCalamityLogic(CalamityLogicType_ProjectileBehavior type) => false;

    internal static void InitializeFlight(Projectile projectile, int mode, float maxSpeed, float acceleration,
        int homingStartFrame = DefaultHomingStartFrame, int homingDuration = DefaultHomingDuration)
    {
        projectile.ai[0] = mode;
        projectile.tileCollide = false;
        AnomalyGlobalProjectile data = projectile.Anomaly;
        data.EnsureAICapacity(FlightDataCapacity, 1);
        data.AnomalyAI32[MaxSpeedSlot].F = maxSpeed;
        data.AnomalyAI32[AccelerationSlot].F = acceleration;
        data.AIChanged32[MaxSpeedSlot] = true;
        data.AIChanged32[AccelerationSlot] = true;
        if (mode == Homing)
        {
            data.AnomalyAI32[HomingStartFrameSlot].I = homingStartFrame;
            data.AnomalyAI32[HomingDurationSlot].I = homingDuration;
            data.AIChanged32[HomingStartFrameSlot] = true;
            data.AIChanged32[HomingDurationSlot] = true;
        }
    }

    internal static void FireFromWarning(Projectile warning, Player target, bool fast = false, Vector2? direction = null)
    {
        int updates = fast ? FastUpdatesPerTick : 1;
        Vector2 velocity = (direction ?? warning.GetVelocityTowards(target)) * (WarningSkullSpeed / updates);
        Projectile.NewProjectileAction(warning.GetSource_FromAI(), warning.Center, velocity, ProjectileID.Skull,
            warning.damage, 0f, warning.owner, action: p =>
            {
                float maxSpeed = fast ? FastMaxSpeed : WarningSkullMaxSpeed;
                float acceleration = MathF.Pow(WarningSkullAcceleration, 1f / updates);
                InitializeFlight(p, Straight, maxSpeed / updates, acceleration);
                AnomalyGlobalProjectile data = p.Anomaly;
                data.AnomalyAI32[TrailTypeSlot].I = (int)TrailType.Beam;
                data.AIChanged32[TrailTypeSlot] = true;
                int headIndex = Math.Abs((int)warning.ai[2]) - 1;
                if ((uint)headIndex < Main.maxNPCs)
                    SetSource(p, Main.npc[headIndex]);
                p.extraUpdates = updates - 1;
                p.timeLeft = WarningSkullLifetime * updates;
                if ((uint)headIndex < Main.maxNPCs
                    && SkeletronHead.GetInstance(Main.npc[headIndex]) is { ShouldProcess: true, CurrentPhase: SkeletronHead.Phase.Phase3 })
                    p.timeLeft = 120 * updates;
            });
    }

    internal static void SetSource(Projectile projectile, NPC head)
    {
        if (head.type == NPCID.SkeletronHand)
            head = NPC.GetNPCFromIndex((int)head.ai[1]);
        if (head is not { active: true, type: NPCID.SkeletronHead }
            || !SkeletronHead.GetInstance(head).ShouldProcess)
            return;
        AnomalyGlobalProjectile data = projectile.Anomaly;
        data.EnsureAICapacity(FlightDataCapacity, 1);
        data.AnomalyAI32[SourceHeadSlot].I = head.whoAmI + 1;
        data.AnomalyAI32[Phase3SourceSlot].I = SkeletronHead.GetInstance(head).CurrentPhase == SkeletronHead.Phase.Phase3 ? 1 : 0;
        data.AIChanged32[SourceHeadSlot] = data.AIChanged32[Phase3SourceSlot] = true;
    }

    private bool HasBeamTrail => AnomalyProjectile.AnomalyAI32[TrailTypeSlot].I == (int)TrailType.Beam;

    public override bool PreAI()
    {
        int headIndex = AnomalyProjectile.AnomalyAI32[SourceHeadSlot].I - 1;
        if (headIndex >= 0 && ((uint)headIndex >= Main.maxNPCs
            || Main.npc[headIndex] is not { active: true, type: NPCID.SkeletronHead }
            || SkeletronHead.GetInstance(Main.npc[headIndex]) is not { ShouldProcess: true } head
            || head.CurrentBehavior == SkeletronHead.Behavior.Despawn))
        {
            if (TONetworkEnvironment.NotClient)
            {
                Projectile.ai[2] = SuppressDeathEffectsMarker;
                Projectile.Kill();
            }
            return false;
        }
        if (Projectile.numUpdates == -1)
            Projectile.frame = (Projectile.frame + 1) % 3;
        Projectile.alpha = 0;

        bool homing = Projectile.ai[0] == Homing;
        if (homing || Projectile.ai[0] == Straight)
        {
            SpawnEffects(Projectile.position);
            if (homing)
                UpdateHoming();

            AnomalyGlobalProjectile data = AnomalyProjectile;
            float maxSpeed = data.AnomalyAI32[MaxSpeedSlot].F;
            float acceleration = data.AnomalyAI32[AccelerationSlot].F;
            // 原版生成的头骨没有额外飞行参数，沿用默认追踪弹的加速。
            if (homing && acceleration == 0f)
            {
                maxSpeed = UltraMaxSpeed;
                acceleration = UltraAcceleration;
            }

            float speed = Projectile.velocity.Length();
            if (acceleration > 1f && speed > 0f && speed < maxSpeed)
                Projectile.velocity *= Math.Min(speed * acceleration, maxSpeed) / speed;
        }

        if (!Main.dedServ)
        {
            if (HasBeamTrail)
                Projectile.localAI[1] = Math.Min(TrailLength, Projectile.localAI[1] + Projectile.velocity.Length());
            else
                EmitTrail();
        }
        Projectile.spriteDirection = Projectile.direction;
        Projectile.rotation = (Projectile.velocity * (Projectile.direction < 0 ? -1f : 1f)).ToRotation();
        return false;

        void UpdateHoming()
        {
            AnomalyGlobalProjectile data = AnomalyProjectile;
            int duration = data.AnomalyAI32[HomingDurationSlot].I;
            // Vanilla skulls have no explicit homing window.
            int startFrame = duration > 0 ? data.AnomalyAI32[HomingStartFrameSlot].I : DefaultHomingStartFrame;
            int endTime = startFrame + (duration > 0 ? duration : DefaultHomingDuration);
            if (Projectile.ai[1] >= endTime || ++Projectile.ai[1] >= endTime)
                return;

            Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
            const float StopDistance = 96f;
            if (Projectile.Distance(target.Center) < StopDistance)
            {
                Projectile.ai[1] = endTime;
                if (TONetworkEnvironment.NotClient)
                    Projectile.netUpdate = true;
                return;
            }
            if (Projectile.ai[1] < startFrame)
                return;

            const float Inertia = 25f;
            float speed = Projectile.velocity.Length();
            Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
            Vector2 velocity = (Projectile.velocity * (Inertia - 1f) + direction * speed) / Inertia;
            Projectile.velocity = velocity.SafeNormalize(direction) * speed;
        }

        void SpawnEffects(Vector2 soundPosition)
        {
            if (Projectile.localAI[0] != 0f)
                return;
            Projectile.localAI[0] = 1f;
            SoundEngine.PlaySound(SoundID.Item8, soundPosition);
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDustAction(Projectile.Center, Projectile.width, Projectile.height, DustID.Blood, Projectile.velocity, dust =>
                {
                    dust.scale *= 2f;
                    dust.noGravity = true;
                    dust.velocity = (Projectile.Center - dust.position).SafeNormalize(Vector2.UnitY) * -5f + Projectile.velocity / 2f;
                });
            }
        }

        void EmitTrail()
        {
            for (int i = 0; i < 2; i++)
            {
                Dust.NewDustAction(Projectile.Center, Projectile.width - 8, Projectile.height - 8, DustID.Blood, Projectile.velocity * 0.2f, dust =>
                {
                    dust.alpha = 100;
                    dust.scale *= 1.5f;
                    dust.position -= Projectile.velocity;
                    dust.noGravity = true;
                    dust.velocity *= 0.3f;
                });
            }
        }
    }

    // 机制 7 的头骨到达场心即被吸收，不触发原版死亡音效和爆散。
    public override bool PreKill(int timeLeft) => Projectile.ai[2] != SuppressDeathEffectsMarker;
}
