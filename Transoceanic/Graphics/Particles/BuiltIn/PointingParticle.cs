namespace Transoceanic.Graphics.Particles.BuiltIn;

/// <summary>
/// 沿速度方向旋转、可选重力和光照的指向粒子。
/// </summary>
public class PointingParticle : Particle
{
    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>ParticleHandler.BaseParticleTexturePath + &quot;PointingParticle&quot;</c>。</remarks>
    public override string TexturePath => ParticleHandler.BaseParticleTexturePath + "PointingParticle";

    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>_UseAdditiveBlend ? BlendState.Additive : BlendState.AlphaBlend</c>。</remarks>
    public override BlendState DrawBlendState => _UseAdditiveBlend ? BlendState.Additive : BlendState.AlphaBlend;

    /// <summary>
    /// 寿命颜色插值使用的初始颜色。
    /// </summary>
    public Color InitialColor;
    /// <summary>
    /// 重力倍率大于零时返回 true。
    /// </summary>
    public bool AffectedByGravity;
    /// <summary>
    /// 是否使用加法混合；关闭时使用 AlphaBlend。
    /// </summary>
    public bool _UseAdditiveBlend = true;

    /// <summary>
    /// 创建指向粒子，设置中心、速度、帧寿命、缩放、颜色、重力、混合模式与光照开关。
    /// </summary>
    public PointingParticle(Vector2 center, Vector2 velocity, bool affectedByGravity, int lifetime, float scale, Color color, bool useAddativeBlend = true, bool affectedByLight = false)
    {
        Center = center;
        Velocity = velocity;
        AffectedByGravity = affectedByGravity;
        Scale = scale;
        Lifetime = lifetime;
        Color = InitialColor = color;
        _UseAdditiveBlend = useAddativeBlend;
        AffectedByLight = affectedByLight;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 缩小并褪色，衰减速度，按开关施加光照和重力，旋转到速度方向。
    /// </remarks>
    public override void Update()
    {
        Scale *= 0.95f;
        Color = Color.Lerp(InitialColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 3D));
        if (AffectedByLight)
            Color.MultiplyWithWorldLight(Center);

        Velocity *= 0.95f;
        if (Velocity.Length() < 12f && AffectedByGravity)
        {
            Velocity.X *= 0.94f;
            Velocity.Y += 0.25f;
        }
        Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 执行该粒子的自定义绘制，将中心加像素偏移后转换到屏幕坐标；返回 false 跳过默认绘制。
    /// </remarks>
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        Vector2 scale = new Vector2(0.5f, 1.6f) * Scale;

        spriteBatch.DrawFromCenter_VectorScale(Texture, Center + drawOffset - Main.screenPosition, null, Color, Rotation, scale, 0, 0f);
        spriteBatch.DrawFromCenter_VectorScale(Texture, Center + drawOffset - Main.screenPosition, null, Color, Rotation, scale * new Vector2(0.45f, 1f), 0, 0f);

        return false;
    }
}

