namespace Anomalies.Bosses.EaterofWorlds;

public sealed partial class EaterofWorldsHead
{
    // ai[0]: owner/local head/previous/next; ai[1]: immutable global ordinal;
    // ai[2]: Phase + Behavior; ai[3]: LocalPhase. Extra slots below are encounter state.
    public const int HuntDuration = 270;
    public const int ChargeWarningTime = 48;
    public const int ChargeDuration = 54;
    public const int FormationTimeout = 150;
    public const int CoilDuration = 72;
    public const int SplitLaunchDelay = 12;
    public const float OutwardChargeSpeed = 34f;
    public const float OutwardChargeDistance = 1200f;
    public const int TripleWarningTime = 36;
    public const int SweepWarningTime = 54;
    public const float FormationDistance = 560f;
    public const float CoilRadius = 80f;
    public const float SweepHalfGap = 132f;
    public const float SplitSpread = MathHelper.Pi / 3.6f;
    public const int HeadContactDamage = 108;
    public const int BodyContactDamage = 66;
    public const int TailContactDamage = 54;

    public bool ShouldEnterPhase1_2 => CurrentPhase == Phase.Phase1 && NPC.LifeRatio <= Phase1_2LifeRatio;
    public float ChargeSpeed => Phase1_2 ? 23f : 21f;
    public int TripleRounds => Phase1_2 ? 5 : 4;

    public int AttackIndex
    {
        get => AnomalyNPC.AnomalyAI32[3].I;
        set { AnomalyNPC.AnomalyAI32[3].I = value; AnomalyNPC.AIChanged32[3] = true; }
    }
    public int CycleIndex
    {
        get => AnomalyNPC.AnomalyAI32[4].I;
        set { AnomalyNPC.AnomalyAI32[4].I = value; AnomalyNPC.AIChanged32[4] = true; }
    }
    public int AttackSide => CycleIndex % 2 == 0 ? 1 : -1;
    public Vector2 AttackCenter
    {
        get => AnomalyNPC.AnomalyAI64[1].GetValue<Vector2>();
        set { AnomalyNPC.AnomalyAI64[1].SetValue(value); AnomalyNPC.AIChanged64[1] = true; }
    }
    public Vector2 AimPoint
    {
        get => AnomalyNPC.AnomalyAI64[2].GetValue<Vector2>();
        set { AnomalyNPC.AnomalyAI64[2].SetValue(value); AnomalyNPC.AIChanged64[2] = true; }
    }
    // Each head locks its launch vector on the final warning tick, never during the windup.
    public Vector2 LaunchDirection
    {
        get => AnomalyNPC.AnomalyAI64[3].GetValue<Vector2>();
        set { AnomalyNPC.AnomalyAI64[3].SetValue(value); AnomalyNPC.AIChanged64[3] = true; }
    }

    public bool ContactSuppressed => CurrentPhase == Phase.Initialize || CurrentBehavior == Behavior.Despawn
        || CurrentBehavior is Behavior.Phase1_Split or Behavior.Phase1_Combine && CurrentLocalPhase < 3;

    private void ChangeStep(int step)
    {
        if (!TONetworkEnvironment.NotClient)
            return;
        CurrentLocalPhase = step;
        Timer1 = 0;
        NPC.netUpdate = true;
    }

    private void ChangeAttack(Behavior behavior)
    {
        if (!TONetworkEnvironment.NotClient)
            return;
        if (ShouldEnterPhase1_2)
            CurrentPhase = Phase.Phase1_2;
        CurrentBehavior = behavior;
        CurrentLocalPhase = 0;
        Timer1 = Timer2 = 0;
        AttackIndex = 0;
        AttackCenter = Target.Center;
        AimPoint = Target.Center;
        NPC.netUpdate = true;
    }

    private static void MoveTowards(NPC head, Vector2 point, float speed, float acceleration = 0.65f)
    {
        Vector2 delta = point - head.Center;
        Vector2 desired = delta.SafeNormalize(Vector2.UnitY) * Math.Min(speed, delta.Length() * 0.12f);
        head.velocity = Vector2.Lerp(head.velocity, desired, acceleration / Math.Max(speed, 1f));
    }

    private void PursueTarget(NPC head)
    {
        Vector2 radial = (head.Center - Target.Center).SafeNormalize(Vector2.UnitX);
        Vector2 pursuit = Target.Center + (radial * 360f).RotatedBy(AttackSide * 0.65f);
        MoveTowards(head, pursuit, Phase1_2 ? 15f : 13.5f, 0.8f);
    }

    internal Vector2 WarningDirection(NPC head, Vector2 offset = default) =>
        (Target.Center + offset - head.Center).SafeNormalize(Vector2.UnitY);

    internal Vector2 CoilLaunchDirection(int group, bool combine) =>
        (Target.Center - CoilingCenter).SafeNormalize(Vector2.UnitY).RotatedBy(combine ? 0f : (group - 1) * SplitSpread);

    private static void AccelerateCharge(NPC head, Vector2 direction, float speed, float elapsed)
    {
        float acceleration = MathHelper.SmoothStep(0f, 1f, Math.Clamp(elapsed / 10f, 0f, 1f));
        head.velocity = Vector2.Lerp(head.velocity, direction * speed, 0.24f * acceleration);
    }

    internal static void SmoothHeadRotation(NPC head)
    {
        Vector2 facing = head.velocity;
        if (facing.LengthSquared() > 0.1f)
            head.rotation += MathHelper.WrapAngle(facing.ToRotation() + MathHelper.PiOver2 - head.rotation) * 0.2f;
    }

    private void LockDirection(NPC head, Vector2 destination)
    {
        if (!TONetworkEnvironment.NotClient)
            return;
        GetInstance(head).LaunchDirection = (destination - head.Center).SafeNormalize(Vector2.UnitY);
        head.netUpdate = true;
    }

    private void ShootFlames(NPC head, Vector2 direction, int count = 1, float speed = 10f)
    {
        if (!TONetworkEnvironment.NotClient)
            return;
        for (int i = 0; i < count; i++)
        {
            Vector2 velocity = direction.SafeNormalize(Vector2.UnitY).RotatedBy((i - (count - 1) * 0.5f) * 0.19f) * speed;
            Projectile.NewProjectileAction(head.GetSource_FromAI(), head.Center, velocity,
                ModContent.ProjectileType<EaterCursedFlame>(), CursedFireballDamage, 0f,
                action: p => { p.ai[0] = NPC.whoAmI + 1; p.timeLeft = 150; });
        }
    }
}
