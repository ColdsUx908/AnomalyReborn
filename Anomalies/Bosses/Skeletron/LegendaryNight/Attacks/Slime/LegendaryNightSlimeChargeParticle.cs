namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 传说之夜客户端视觉粒子。
/// </summary>
/// <remarks>
/// <para>数据槽位：不使用实体 ai、localAI 或网络槽位；Timer/Lifetime 为粒子框架本地生命周期，实例字段保存目标和轨迹参数，不同步。</para>
/// </remarks>
public sealed class LegendaryNightSlimeChargeParticle : CustomSpriteParticle
{
    /// <summary>蓄力存活时间，单位为游戏帧。</summary>
    public const int ChargeLifetime = 32;
    /// <summary>生成数量。</summary>
    public const int SpawnCount = 2;
    /// <summary>渐隐持续时间，单位为游戏帧。</summary>
    private const int FadeDuration = 4;
    /// <summary>拖尾长度。</summary>
    private const int TrailLength = 8;
    /// <summary>初始缩放。</summary>
    private const float InitialScale = 1f;

    private readonly NPC _Master;
    private readonly Vector2 _Offset;

    public override bool AutoUpdatePosition => false;

    public LegendaryNightSlimeChargeParticle(NPC master, Vector2 offset)
        : base(LegendSlimeRain.GetRainTexture(Main.rand.Next(LegendSlimeRain.VariantCount)), master.Center + offset, Vector2.Zero,
            ChargeLifetime, (-offset).ToRotation() - MathHelper.PiOver2, InitialScale, Color.White)
    {
        _Master = master;
        _Offset = offset;
    }

    public override void Update()
    {
        // 不调用通用精灵粒子的末20帧衰减：短寿命吸收粒子需要保持主体清晰直到汇入。
        float progress = MathHelper.Clamp(LifetimeCompletion, 0f, 1f);
        Center = GetPosition(Timer);
        Scale = InitialScale * MathHelper.Lerp(1f, 0.75f, progress);
        Opacity = LegendaryNightHandler.IsLegendaryNightHead(_Master)
            ? Math.Min(MathHelper.Clamp(Timer / (float)FadeDuration, 0f, 1f),
                MathHelper.Clamp((Lifetime - Timer) / (float)FadeDuration, 0f, 1f))
            : 0f;
    }

    private Vector2 GetPosition(int age)
    {
        float progress = MathHelper.Clamp(age / (float)ChargeLifetime, 0f, 1f);
        return _Master.Center + _Offset * (1f - progress * progress);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        if (Opacity <= 0f)
            return false;
        Rectangle frame = Texture.Frame(1, 4, 0, Timer / 6 % 4);
        // 参考突变体史莱姆雨的全亮主体和八帧渐淡残影，不受环境光压暗。
        for (int i = Math.Min(Timer, TrailLength); i >= 1; i--)
        {
            float strength = 1f - i / (float)(TrailLength + 1);
            spriteBatch.DrawFromCenter(Texture, GetPosition(Timer - i) + drawOffset - Main.screenPosition,
                frame, Color.LightSkyBlue * (0.5f * strength * Opacity), Rotation, Scale);
        }
        spriteBatch.DrawFromCenter(Texture, GetPosition(Timer) + drawOffset - Main.screenPosition,
            frame, Color.White * Opacity, Rotation, Scale);
        return false;
    }
}