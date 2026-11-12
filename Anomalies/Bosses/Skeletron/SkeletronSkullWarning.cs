namespace Anomalies.Bosses.Skeletron;

/// <summary>预警结束后，沿预警方向发射带光束拖尾的直线头骨。</summary>
public sealed class SkeletronSkullWarning : AnomalyModProjectile
{
    internal const int WarningTime = 60;

    // ai[0]: target player + 1, ai[1]: age, abs(ai[2]): head index + 1. Negative ai[2] selects fast flight.
    internal static int EncodeSource(int headIndex, bool fast) => (headIndex + 1) * (fast ? -1 : 1);
    private const int FadeDuration = 30;
    private const float FadeInDuration = 10f;
    private int SourceHeadIndex => Math.Abs((int)Projectile.ai[2]) - 1;
    private int TargetPlayerIndex => (int)Projectile.ai[0] - 1;
    private bool FastFlight => Projectile.ai[2] < 0f;
    private ref float Age => ref Projectile.ai[1];

    public override string LocalizationCategory => "Bosses.Skeletron";
    public override string Texture => TOTextures.InvisibleTexturePath;

    /// <summary>预警年龄达到此帧数后固定方向；0 表示始终跟踪。</summary>
    public int StopTrackingTime { get; set; }

    protected override void SendAnomalyExtraAI(BinaryWriter writer)
    {
        base.SendAnomalyExtraAI(writer);
        writer.Write(StopTrackingTime);
    }

    protected override void ReceiveAnomalyExtraAI(BinaryReader reader)
    {
        base.ReceiveAnomalyExtraAI(reader);
        StopTrackingTime = reader.ReadInt32();
    }

    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.netImportant = true;
        Projectile.timeLeft = WarningTime + 10;
    }

    public override void AI()
    {
        int headIndex = SourceHeadIndex;
        if ((uint)headIndex >= Main.maxNPCs || Main.npc[headIndex] is not { active: true, type: NPCID.SkeletronHead }
            || SkeletronHead.GetInstance(Main.npc[headIndex]) is not
                { ShouldProcess: true, CurrentBehavior: not SkeletronHead.Behavior.Despawn } head
            || !Player.TryGetPlayerFromIndex(TargetPlayerIndex, out Player target) || !target.active || target.dead)
        {
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }

        if (head.IsFinalStage || Age < 0f)
        {
            if (Age >= 0f)
            {
                Projectile.Opacity = MathHelper.Clamp(Age / FadeInDuration, 0f, 1f);
                Age = -1f;
                if (TONetworkEnvironment.NotClient)
                    Projectile.netUpdate = true;
            }
            int fadeAge = Math.Min(FadeDuration, (int)-Age);
            Projectile.Opacity *= (FadeDuration - fadeAge) / (float)(FadeDuration + 1 - fadeAge);
            Projectile.timeLeft = Math.Max(Projectile.timeLeft, 2);
            Age--;
            if (fadeAge >= FadeDuration && TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }

        if (StopTrackingTime <= 0 || Age < StopTrackingTime)
            Projectile.rotation = Projectile.GetVelocityTowards(target).ToRotation();
        if (StopTrackingTime > 0 && Age == StopTrackingTime && TONetworkEnvironment.NotClient)
            Projectile.netUpdate = true;
        if (Projectile.localAI[0] == 0f && !Main.dedServ)
        {
            Projectile.localAI[0] = 1f;
            int remainingTime = Math.Max(1, WarningTime - (int)Age);
            ParticleHandler.SpawnParticle(new BloomParticle(Projectile.Center, Vector2.Zero,
                Color.White, 0.1f, 0.5f, remainingTime, 0.85f));
        }

        Age++;
        if (Age >= WarningTime && TONetworkEnvironment.NotClient)
        {
            Skull.FireFromWarning(Projectile, target, fast: FastFlight,
                direction: StopTrackingTime > 0 && Age >= StopTrackingTime
                    ? Projectile.rotation.ToRotationVector2() : null);
            Projectile.Kill();
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (!Player.TryGetPlayerFromIndex(TargetPlayerIndex, out Player target) || !target.active || target.dead)
            return false;

        float length = Vector2.Distance(Projectile.Center, target.Center) * 1.5f;
        float opacity = Age < 0f ? Projectile.Opacity : MathHelper.Clamp(Age / FadeInDuration, 0f, 1f);
        AnomalyDrawUtils.DrawLayeredWarning(Main.spriteBatch, Projectile.Center, Projectile.rotation,
            length, 10f, SkeletronCombatVisuals.BoneColor, Color.White, opacity);
        return false;
    }
}
