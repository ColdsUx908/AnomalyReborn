namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>从手掌沿柔和弧线吸入群星物质光球的客户端星芒。</summary>
public sealed class LegendaryNightStellarMatterParticle : GenericSparkle
{
    private readonly Projectile _Sphere;
    private readonly int _SphereIdentity;
    private readonly Vector2 _Origin;
    private readonly float _InitialScale;
    private readonly float _Curve;

    public override bool AutoUpdatePosition => false;

    public LegendaryNightStellarMatterParticle(Projectile sphere, Vector2 origin, float scale, int lifetime)
        : base(origin, Vector2.Zero, Color.LightGoldenrodYellow, LegendaryNightHandler.StarColor, scale, lifetime, 0.035f, 1.6f)
    {
        _Sphere = sphere;
        _SphereIdentity = sphere.identity;
        _Origin = origin;
        _InitialScale = scale;
        _Curve = Main.rand.NextFloat(-35f, 35f);
    }

    public override void Update()
    {
        base.Update();
        float progress = MathHelper.Clamp(LifetimeCompletion, 0f, 1f);
        float absorption = progress * progress;
        Vector2 direction = (_Sphere.Center - _Origin).SafeNormalize(Vector2.UnitY);
        Vector2 normal = new(-direction.Y, direction.X);
        Center = Vector2.Lerp(_Origin, _Sphere.Center, absorption)
            + normal * (MathF.Sin(MathHelper.Pi * absorption) * _Curve);
        Scale = _InitialScale * MathHelper.Lerp(1f, 0.35f, absorption);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        if (!_Sphere.active || _Sphere.identity != _SphereIdentity || _Sphere.ModProjectile is not LegendEnergySphere)
            return false;
        return base.PreDraw(spriteBatch, drawOffset);
    }
}
