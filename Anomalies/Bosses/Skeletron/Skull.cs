namespace Anomalies.Bosses.Skeletron;

public sealed partial class Skull : AnomalyProjectileBehavior<Skull>
{
    // ai[0]：飞行模式；ai[1]：追踪计时；ai[2]：死亡效果标记。
    public enum Behavior : byte
    {
        Homing = 0,
        Straight = 2,
    }

    // 保留生成入口使用的模式值及原有 ai 协议。
    public const int Homing = (int)Behavior.Homing;
    public const int Straight = (int)Behavior.Straight;

    internal const int DefaultHomingStartFrame = 31;
    internal const int DefaultHomingDuration = 79;

    public static float MaxSpeed => Ultra ? 18f : 15f;
    public static float Acceleration => Ultra ? 1.02f : 1.015f;

    internal const float SuppressDeathEffectsMarker = -7f;

    private const int MaxSpeedSlot = 0;
    private const int AccelerationSlot = 1;
    private const int HomingStartFrameSlot = 2;
    private const int HomingDurationSlot = 3;
    private const int TrailTypeSlot = 4;
    private const int SourceHeadSlot = 5;
    // AI32[6] reserved; keep the seven-slot snapshot layout.
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
        data.AIChanged32[SourceHeadSlot] = true;
    }

    public Behavior CurrentBehavior => (Behavior)(int)Projectile.ai[0];

    private bool HasBeamTrail => AnomalyProjectile.AnomalyAI32[TrailTypeSlot].I == (int)TrailType.Beam;

    // LegendaryNight and owner cleanup share this death-effect suppression marker.
    public override bool PreKill(int timeLeft) => Projectile.ai[2] != SuppressDeathEffectsMarker;
}
