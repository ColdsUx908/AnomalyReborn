namespace Transoceanic.Graphics.Particles.BuiltIn;

/// <summary>
/// 可选重力、中心发光和淡出缩小的球形粒子。
/// </summary>
public class OrbParticle : Particle
{
    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>ParticleHandler.BaseParticleTexturePath + &quot;OrbParticle&quot;</c>。</remarks>
    public override string TexturePath => ParticleHandler.BaseParticleTexturePath + "OrbParticle";

    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>UseAdditiveBlend ? BlendState.Additive : BlendState.AlphaBlend</c>。</remarks>
    public override BlendState DrawBlendState => UseAdditiveBlend ? BlendState.Additive : BlendState.AlphaBlend;

    /// <summary>
    /// 寿命颜色插值使用的初始颜色。
    /// </summary>
    public Color InitialColor;
    /// <summary>
    /// 是否使用加法混合；关闭时使用 AlphaBlend。
    /// </summary>
    public bool UseAdditiveBlend = true;
    /// <summary>
    /// 中心发光与缩放淡出使用的当前系数，由寿命更新逻辑维护。
    /// </summary>
    public float FadeOut = 1f;
    /// <summary>
    /// 是否额外绘制较小的白色中心光球。
    /// </summary>
    public bool GlowCenter;
    /// <summary>
    /// 寿命淡出前的初始缩放。
    /// </summary>
    public float InitialScale;
    /// <summary>
    /// 重力倍率，正值时启用；同时缩放下落速度阈值和每帧加速度。
    /// </summary>
    public float GravityMultiplier;
    /// <summary>
    /// 开始淡出的寿命比例，通常在 [0, 1] 内；1 表示寿命内不进入淡出分支。
    /// </summary>
    public float LifeEndRatio;
    /// <summary>
    /// 重力倍率大于零时返回 true。
    /// </summary>
    public bool AffectedByGravity => GravityMultiplier > 0f;

    /// <summary>
    /// 创建球形粒子，设置帧寿命、初始缩放、重力倍率、淡出起点与中心发光；lifeEndRatio 使用寿命比例。
    /// </summary>
    public OrbParticle(Vector2 center, Vector2 velocity, int lifetime, float scale, Color color, float gravityMultiplier = 0f, float lifeEndRatio = 0f, bool useAdditiveBlend = true, bool glowCenter = true)
    {
        Center = center;
        Velocity = velocity;
        GravityMultiplier = gravityMultiplier;
        Scale = scale;
        Lifetime = lifetime;
        Color = InitialColor = color;
        UseAdditiveBlend = useAdditiveBlend;
        GlowCenter = glowCenter;
        InitialScale = scale;
        LifeEndRatio = lifeEndRatio;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 超过淡出起点后缩小，按寿命压暗颜色、衰减速度，并在启用时施加倍率重力。
    /// </remarks>
    public override void Update()
    {
        if (LifetimeCompletion > LifeEndRatio)
        {
            float interpolation = TOMathUtils.Interpolation.QuadraticEaseOut(1f - (LifetimeCompletion - LifeEndRatio) / (1f - LifeEndRatio));
            FadeOut = interpolation;
            Scale = InitialScale * interpolation;
        }
        Color = Color.Lerp(InitialColor, InitialColor * 0.2f, MathF.Pow(LifetimeCompletion, 3));
        Velocity *= 0.98f;
        if (Velocity.Y < 12f * GravityMultiplier && AffectedByGravity)
            Velocity.Y += 0.25f * GravityMultiplier;
        Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 执行该粒子的自定义绘制，将中心加像素偏移后转换到屏幕坐标；返回 false 跳过默认绘制。
    /// </remarks>
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        Texture2D texture = Texture;
        Vector2 scale = new(Scale);

        spriteBatch.DrawFromCenter_VectorScale(texture, Center + drawOffset - Main.screenPosition, null, Color, Rotation, scale);
        if (GlowCenter)
            spriteBatch.DrawFromCenter_VectorScale(texture, Center + drawOffset - Main.screenPosition, null, Color.White * FadeOut, Rotation, scale * new Vector2(0.5f, 0.5f));

        return false;
    }
}
