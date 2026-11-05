namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>撞碎小光球后散开、减速，再带尾迹汇入碰撞玩家的纯视觉光点。</summary>
public sealed class LegendaryNightAbsorptionParticle : Particle
{
    private const int TrailLength = 12;
    internal const int PauseDuration = 3;
    private readonly Player _Target;
    private readonly bool _FadeInPlace;
    private readonly Projectile _SphereTarget;
    private readonly Vector2[] _TrailPositions = new Vector2[TrailLength];
    private readonly float _InitialScale;
    private readonly int _ScatterDuration;
    private readonly int _BrakeDuration;
    private readonly float _Spin;

    public override string TexturePath => ParticleHandler.BaseParticleTexturePath + "OrbParticle";
    public override BlendState DrawBlendState => BlendState.Additive;
    public override bool AutoUpdatePosition => false;

    public LegendaryNightAbsorptionParticle(Player target, Vector2 position, Vector2 velocity, Color color, float scale, bool fadeInPlace = false, Projectile sphereTarget = null)
    {
        _Target = target;
        _FadeInPlace = fadeInPlace;
        _SphereTarget = sphereTarget;
        Center = position;
        Velocity = velocity;
        Color = color;
        Scale = _InitialScale = scale;
        Lifetime = 240;
        _ScatterDuration = Main.rand.Next(6, 11);
        _BrakeDuration = Main.rand.Next(10, 15);
        if (fadeInPlace)
            Lifetime = _ScatterDuration + _BrakeDuration + 30 + 24;
        _Spin = Main.rand.NextFloat(-0.018f, 0.018f);
        Array.Fill(_TrailPositions, position);
    }

    internal static Vector2 AdvanceVelocity(Vector2 velocity, Vector2 targetOffset, int age,
        int scatterDuration, int brakeDuration, float spin)
    {
        if (age <= scatterDuration)
            return velocity.RotatedBy(spin);
        if (age <= scatterDuration + brakeDuration)
        {
            int remaining = scatterDuration + brakeDuration - age;
            // 有限帧内准确降至零，避免指数阻尼在低速段长时间漂浮。
            return velocity.RotatedBy(spin) * (remaining / (remaining + 1f));
        }
        if (age <= scatterDuration + brakeDuration + PauseDuration)
            return Vector2.Zero;

        // 散射阶段保留弯曲惯性，追踪阶段逐渐提高目标速度，平滑转向移动中的玩家。
        int homingAge = age - scatterDuration - brakeDuration - PauseDuration;
        float speed = Math.Min(24f, 4f + homingAge * 0.6f);
        Vector2 desiredVelocity = targetOffset.SafeNormalize(Vector2.Zero) * speed;
        return Vector2.Lerp(velocity, desiredVelocity, 0.18f);
    }

    public override void Update()
    {
        if (!_FadeInPlace && (_SphereTarget is not null ? !_SphereTarget.active : !_Target.active || _Target.dead || _Target.ghost))
        {
            Kill();
            return;
        }

        Vector2 offset = (_SphereTarget?.Center ?? _Target.Center) - Center;
        Velocity = AdvanceVelocity(Velocity, offset, Timer, _ScatterDuration, _BrakeDuration, _Spin);
        if (_FadeInPlace && Timer > _ScatterDuration + _BrakeDuration)
            Velocity = Vector2.Zero;
        bool homing = !_FadeInPlace && Timer > _ScatterDuration + _BrakeDuration + PauseDuration;
        if (homing && offset.Length() <= Math.Max(8f, Velocity.Length()))
        {
            Kill();
            return;
        }

        Array.Copy(_TrailPositions, 0, _TrailPositions, 1, TrailLength - 1);
        _TrailPositions[0] = Center;
        Center += Velocity;
        float arrivalScale = homing ? MathHelper.Clamp(offset.Length() / 32f, 0.2f, 1f) : 1f;
        float lifetimeFade = MathHelper.Clamp((Lifetime - Timer) / 24f, 0f, 1f);
        Scale = _InitialScale * arrivalScale * lifetimeFade;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        Texture2D bloom = ParticleHandler.GetTexture<BloomParticle>();
        Vector2 newerPosition = Center;
        for (int i = 0; i < TrailLength; i++)
        {
            Vector2 olderPosition = _TrailPositions[i];
            Vector2 segment = newerPosition - olderPosition;
            float strength = 1f - (float)i / TrailLength;
            float width = 12f * Scale * strength;
            spriteBatch.DrawFromCenter_VectorScale(bloom,
                (newerPosition + olderPosition) * 0.5f + drawOffset - Main.screenPosition,
                null, Color * (strength * strength * 0.65f), segment.ToRotation(),
                new Vector2((segment.Length() + width * 2f) / bloom.Width, width / bloom.Height));
            newerPosition = olderPosition;
        }
        spriteBatch.DrawFromCenter(Texture, Center + drawOffset - Main.screenPosition, null, Color, scale: Scale);
        return false;
    }
}
