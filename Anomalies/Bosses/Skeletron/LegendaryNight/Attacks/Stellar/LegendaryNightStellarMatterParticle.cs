namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 传说之夜客户端视觉粒子。
/// </summary>
/// <remarks>
/// <para>数据槽位：不使用实体 ai、localAI 或网络槽位；Timer/Lifetime 为粒子框架本地生命周期，实例字段保存目标和轨迹参数，不同步。</para>
/// </remarks>
public sealed class LegendaryNightStellarMatterParticle : GenericSparkle
{
    private readonly Projectile _Sphere;
    private readonly int _SphereIdentity;
    private readonly Vector2 _Origin;
    private readonly float _InitialScale;

    public override bool AutoUpdatePosition => false;

    public LegendaryNightStellarMatterParticle(Projectile sphere, Vector2 origin, float scale, int lifetime)
        : base(origin, Vector2.Zero, Color.LightGoldenrodYellow, LegendaryNightHandler.StarColor, scale, lifetime, 0.035f, 1.6f)
    {
        _Sphere = sphere;
        _SphereIdentity = sphere.identity;
        _Origin = origin;
        _InitialScale = scale;
    }

    public override void Update()
    {
        base.Update();
        float progress = MathHelper.Clamp(LifetimeCompletion, 0f, 1f);
        float absorption = progress * progress;
        Vector2 offset = _Origin - _Sphere.Center;
        Center = _Sphere.Center + offset * (1f - absorption);
        Scale = _InitialScale * MathHelper.Lerp(1f, 0.35f, absorption);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        if (!_Sphere.active || _Sphere.identity != _SphereIdentity || _Sphere.ModProjectile is not LegendEnergySphere)
            return false;
        return base.PreDraw(spriteBatch, drawOffset);
    }
}