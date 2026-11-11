namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>使用解包并调色后的原版星芒贴图，统一实现飞行星屑的本地运动与寿命。</summary>
/// <remarks>仅保存客户端粒子状态，不使用实体 AI 或网络槽位。</remarks>
public abstract class StarTrailParticle : Particle
{
    /// <summary>粒子的发光颜色。</summary>
    protected abstract Vector3 LightColor { get; }

    /// <summary>以原版星芒的初始缩放和随机漂移创建粒子。</summary>
    protected StarTrailParticle(Vector2 center, Vector2 velocity)
    {
        Center = center;
        velocity.Y -= Main.rand.Next(10, 31) * 0.1f;
        velocity.X += Main.rand.Next(-20, 21) * 0.1f;
        Velocity = velocity;
        Lifetime = 60;
        Scale = 0.7f;
        Color = Color.White;
    }

    /// <summary>保留原版星芒的速度衰减、缩小速度和发光强度。</summary>
    public override void Update()
    {
        Velocity *= 0.98f;
        Scale -= 0.01f;
        if (Scale < 0.1f)
        {
            Kill();
            return;
        }
        Lighting.AddLight(Center, LightColor * Scale);
    }

    /// <summary>使用原版星芒的环境光 RGB 与固定透明度绘制。</summary>
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        Color lightColor = Lighting.GetColor(Center.ToTileCoordinates()) with { A = 155 };
        spriteBatch.DrawFromCenter(Texture, Center + drawOffset - Main.screenPosition, null, lightColor, Rotation, Scale);
        return false;
    }
}

/// <summary>保留 Gore 17 原色的蓝色星芒。</summary>
public sealed class StarTrailBlueParticle : StarTrailParticle
{
    /// <inheritdoc/>
    public override string TexturePath => LegendaryNightHandler.LegendaryNightPath + "Attacks/Stars/StarTrailBlueParticle";
    /// <inheritdoc/>
    protected override Vector3 LightColor => new(0.3f, 0.6f, 1f);
    /// <summary>创建蓝色星芒。</summary>
    public StarTrailBlueParticle(Vector2 center, Vector2 velocity) : base(center, velocity) { }
}

/// <summary>保留 Gore 16 原色的黄色星芒。</summary>
public sealed class StarTrailYellowParticle : StarTrailParticle
{
    /// <inheritdoc/>
    public override string TexturePath => LegendaryNightHandler.LegendaryNightPath + "Attacks/Stars/StarTrailYellowParticle";
    /// <inheritdoc/>
    protected override Vector3 LightColor => new(1f, 0.8f, 0.3f);
    /// <summary>创建黄色星芒。</summary>
    public StarTrailYellowParticle(Vector2 center, Vector2 velocity) : base(center, velocity) { }
}

/// <summary>由原版星芒调色得到的橙色星芒。</summary>
public sealed class StarTrailOrangeParticle : StarTrailParticle
{
    /// <inheritdoc/>
    public override string TexturePath => LegendaryNightHandler.LegendaryNightPath + "Attacks/Stars/StarTrailOrangeParticle";
    /// <inheritdoc/>
    protected override Vector3 LightColor => new(1f, 0.65f, 0.15f);
    /// <summary>创建橙色星芒。</summary>
    public StarTrailOrangeParticle(Vector2 center, Vector2 velocity) : base(center, velocity) { }
}

/// <summary>由原版星芒调色得到的红色星芒。</summary>
public sealed class StarTrailRedParticle : StarTrailParticle
{
    /// <inheritdoc/>
    public override string TexturePath => LegendaryNightHandler.LegendaryNightPath + "Attacks/Stars/StarTrailRedParticle";
    /// <inheritdoc/>
    protected override Vector3 LightColor => new(1f, 0.24f, 0.14f);
    /// <summary>创建红色星芒。</summary>
    public StarTrailRedParticle(Vector2 center, Vector2 velocity) : base(center, velocity) { }
}
