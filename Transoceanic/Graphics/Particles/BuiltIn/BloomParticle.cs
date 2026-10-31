using Transoceanic.Graphics.Assets.Loading;
namespace Transoceanic.Graphics.Particles.BuiltIn;

/// <summary>
/// 支持起止缩放插值和延迟淡出的加法光晕粒子。
/// </summary>
public class BloomParticle : Particle
{
    [LoadTexture(ParticleHandler.BaseParticleTexturePath + "BloomCircleLarge")]
    private static Asset<Texture2D> _BloomCircleLarge;
    /// <summary>
    /// 已加载的大尺寸光晕纹理。
    /// </summary>
    public static Texture2D BloomCircleLarge => _BloomCircleLarge.Value;

    /// <summary>
    /// 普通光晕纹理的基准半径，单位为像素。
    /// </summary>
    public const float BloomCircleRadius = 50f;
    /// <summary>
    /// 大尺寸光晕纹理的基准半径，单位为像素。
    /// </summary>
    public const float BloomCircleLargeRadius = 125f;

    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>ParticleHandler.BaseParticleTexturePath + &quot;BloomCircle&quot;</c>。</remarks>
    public override string TexturePath => ParticleHandler.BaseParticleTexturePath + "BloomCircle";
    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>BlendState.Additive</c>。</remarks>
    public override BlendState DrawBlendState => BlendState.Additive;

    /// <summary>
    /// 寿命插值开始时的缩放。
    /// </summary>
    public float OriginalScale;
    /// <summary>
    /// 寿命末端插值到的目标缩放。
    /// </summary>
    public float FinalScale;
    /// <summary>
    /// 当前绘制使用的透明度系数，由粒子更新逻辑维护。
    /// </summary>
    public float Opacity = 1f;
    /// <summary>
    /// 应用淡出前的基础颜色。
    /// </summary>
    public Color BaseColor;
    /// <summary>
    /// 开始淡出的寿命比例，通常在 [0, 1] 内；1 表示寿命内不进入淡出分支。
    /// </summary>
    public float LifeEndRatio;
    /// <summary>
    /// 是否使用大尺寸光晕纹理。
    /// </summary>
    public bool UseLargeTexture;

    /// <summary>
    /// 创建光晕粒子，设置中心、速度、起止缩放和帧寿命；lifeEndRatio 是开始淡出的寿命比例，1 表示不提前淡出。
    /// </summary>
    public BloomParticle(Vector2 center, Vector2 velocity, Color color, float originalScale, float finalScale, int lifeTime, float lifeEndRatio = 1f, bool useLargeTexture = false)
    {
        Center = center;
        Velocity = velocity;
        BaseColor = color;
        OriginalScale = originalScale;
        FinalScale = finalScale;
        Scale = originalScale;
        Lifetime = lifeTime;
        LifeEndRatio = lifeEndRatio;
        UseLargeTexture = useLargeTexture;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 以指数缓出更新缩放，超过淡出比例后衰减颜色和尺寸，并向世界添加光照。
    /// </remarks>
    public override void Update()
    {
        Scale = TOMathUtils.Interpolation.ExponentialEaseOut(OriginalScale, FinalScale, LifetimeCompletion, 4f);

        if (LifetimeCompletion > LifeEndRatio)
        {
            float interpolation = TOMathUtils.Interpolation.QuadraticEaseOut(1f - (LifetimeCompletion - LifeEndRatio) / (1f - LifeEndRatio));
            Opacity = interpolation;
            Scale *= interpolation;
        }

        Color = BaseColor * Opacity;
        Lighting.AddLight(Center, Color.R / 255f, Color.G / 255f, Color.B / 255f);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 执行该粒子的自定义绘制，将中心加像素偏移后转换到屏幕坐标；返回 false 跳过默认绘制。
    /// </remarks>
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        spriteBatch.DrawFromCenter(UseLargeTexture ? BloomCircleLarge : Texture, Center + drawOffset - Main.screenPosition, null, Color, scale: Scale);
        return false;
    }
}

