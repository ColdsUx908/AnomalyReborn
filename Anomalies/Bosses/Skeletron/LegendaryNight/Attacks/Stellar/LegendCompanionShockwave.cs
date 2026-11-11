namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 蓝星撞击后产生的结算冲击波。
/// </summary>
/// <remarks>
/// <para>ai[0]：爆发音乐帧；ai[1]：按吸收计数确定的最大半径；ai[2]：主体 NPC 索引 + 1。</para>
/// <para>localAI：未使用；框架 Timer1..5：未使用；继承共享冲击波协议，无额外载荷。</para>
/// <para>Projectile.damage 保存生成时结算伤害；_Hit 为本机单次命中标记，爆裂粒子与播放标记仅客户端使用。</para>
/// </remarks>
public sealed class LegendCompanionShockwave : LegendStellarShockwave
{
    private const int ShellDuration = 210;
    private const int NebulaDuration = 270;

    // 吸收计数公式已给出结算伤害，不再叠加弹幕难度倍率。
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        ProjectileID.Sets.PlayerHurtDamageIgnoresDifficultyScaling[Type] = true;
    }

    private bool _Hit;
    private bool _ImpactPlayed;
    private StellarDebrisSet _Particles;
    protected override bool YellowPalette => false;
    protected override int Lifetime => NebulaDuration;
    protected override string ExplosionPass => "SupernovaRemnant";
    protected override float ShaderRadiusScale => 1.85f;
    protected override float OuterExpansion(int age) => CoastingOuterExpansion(age);
    protected override Vector4 RemnantState(int age) => NebulaRemnantState(age, ShellDuration);
    protected override Vector3 InteriorState(int age) => NebulaInteriorState(age, Lifetime);

    private sealed class StellarDebrisSet : ParticleSet
    {
        public StellarDebrisSet() : base(0, 0f) => ParticleLimit = 1800;
        public override int ParticleLifetime => 0;
        public override Particle SpawnParticle() => throw new InvalidOperationException("Explicit debris only.");
    }
    protected override void UpdateExplosion(int age)
    {
        PlayImpactOnce();
        _Particles?.Update();
        Player player = Main.LocalPlayer;
        if (!_Hit && age < CompanionExplosionDuration && player.active && !player.dead && !player.ghost
            && new Circle(Projectile.Center, Projectile.ai[1] * CompanionShockwaveProgress(age)).Collides(player.Hitbox))
        {
            _Hit = true;
            LegendaryNightDamage.HurtLocalPlayer(Projectile, Projectile.damage, false);
        }

        void PlayImpactOnce()
        {
            if (!_ImpactPlayed)
            {
                _ImpactPlayed = true;
                if (age < CompanionExplosionDuration)
                {
                    foreach (Projectile other in Projectile.ActiveProjectiles)
                        if (other.ModProjectile is LegendEnergySphere sphere && other.ai[0] == Projectile.ai[2])
                            sphere.FlashCore();
                    if (age <= 6)
                    {
                        float proximity = 1f - MathHelper.Clamp(Main.LocalPlayer.Distance(Projectile.Center) / 3000f, 0f, 1f);
                        Main.LocalPlayer.SetScreenshake(14f * proximity);
                        SoundEngine.PlaySound(SoundID.Item14 with { Volume = 1.4f, Pitch = -0.65f }, Projectile.Center);
                    }
                    _Particles = new StellarDebrisSet();
                    for (int i = 0; i < 1800; i++)
                    {
                        float angle = MathHelper.TwoPi * (i + Main.rand.NextFloat()) / 1800;
                        Color color = Color.Lerp(new Color(75, 135, 255), new Color(255, 224, 165), Main.rand.NextFloat());
                        _Particles.TrySpawnParticle(new GenericSparkle(Projectile.Center + Main.rand.NextPolarVector2(32f),
                            angle.ToRotationVector2() * Main.rand.NextFloat(3f, 25f), color, color * 0.35f,
                            Main.rand.NextFloat(0.12f, 0.42f), Main.rand.Next(60, 121), rotationSpeed: 0.025f, bloomScale: 1.2f));
                    }
                }
            }
        }
    }
    internal void DrawDebris(SpriteBatch spriteBatch)
    {
        if (_Particles is null)
            return;
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        _Particles.DrawSet(Vector2.Zero);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
    }
    public override void OnKill(int timeLeft)
    {
        _Particles?.Clear();
        _Particles = null;
    }
}
