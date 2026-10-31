namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>保持清晰主体及渐淡残影的史莱姆吸收粒子；所有位置都相对当前本体计算。</summary>
public sealed class LegendaryNightSlimeChargeParticle : CustomSpriteParticle
{
    public const int ChargeLifetime = 32;
    public const int SpawnCount = 2;
    private const int FadeDuration = 4;
    private const int TrailLength = 8;
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
        Opacity = _Master.active && _Master.type == NPCID.SkeletronHead
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
