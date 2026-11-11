namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 转阶段落星撞击时展开的金色冲击波，无伤害。
/// </summary>
/// <remarks>
/// <para>ai[0]、ai[2]：未使用；ai[1]：初始扩张曲线的基准半径（世界像素）。</para>
/// <para>localAI、框架 Timer 与 ExtraAI 均未使用；演出年龄由 timeLeft 推导。</para>
/// </remarks>
public sealed class LegendTransitionShockwave : LegendStellarShockwave
{
    internal const float ImpactRadius = 1850f;
    private const int ShellDuration = 126;
    private const int NebulaDuration = 165;
    // The default LegendShockwave travels about 36 world pixels per frame at launch.
    private const float LaunchSpeed = 43.2f;
    private const int BrakingStart = 24;
    private const float BrakingTime = 42f;

    protected override bool YellowPalette => true;
    protected override string ExplosionPass => "GoldenImpact";
    protected override int Lifetime => NebulaDuration;
    protected override float ShaderRadiusScale => 1.85f;
    protected override float InitialExpansion(int age) => Math.Min(OuterExpansion(age), 1f);
    protected override float OuterExpansion(int age)
    {
        float coastAge = Math.Max(0, age - BrakingStart);
        float expansionTime = Math.Min(age, BrakingStart) + coastAge / (1f + coastAge / BrakingTime);
        return LaunchSpeed * expansionTime / ImpactRadius;
    }
    protected override Vector4 RemnantState(int age) => NebulaRemnantState(age, ShellDuration);
    protected override Vector3 InteriorState(int age) => NebulaInteriorState(age, Lifetime);

    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 6000;

    protected override void UpdateExplosion(int age)
    {
        if (age != 0)
            return;

        Main.LocalPlayer.SetScreenshake(LegendFinalShockwave.FailureScreenshake * 0.8f);

        const int SparkCount = 96;
        for (int i = 0; i < SparkCount; i++)
        {
            float angle = MathHelper.TwoPi * (i + Main.rand.NextFloat()) / SparkCount;
            Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 21f);
            Color gold = Color.Lerp(Color.Orange, Color.Gold, Main.rand.NextFloat());
            ParticleHandler.SpawnParticle(new GenericSparkle(Projectile.Center, velocity, gold, gold * 0.42f,
                Main.rand.NextFloat(0.16f, 0.44f), Main.rand.Next(42, 101), rotationSpeed: 0.035f, bloomScale: 1.45f));
        }
    }
}
