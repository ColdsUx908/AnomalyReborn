using Transoceanic.Graphics.Assets;

namespace Transoceanic.Graphics.Particles.BuiltIn;

/// <summary>
/// 使用任意纹理的扩散脉冲，支持非等比缩放、淡出、混合模式与发光强度。
/// </summary>
public class TexturePulseParticle : Particle
{
    /// <summary>
    /// 该脉冲粒子的自定义纹理资源；专用服务器上为 null。
    /// </summary>
    public Asset<Texture2D> SpriteAsset;
    /// <summary>
    /// 绘制时应用的双轴形变倍率。
    /// </summary>
    public Vector2 Squish;
    /// <summary>
    /// 应用淡出前的基础颜色。
    /// </summary>
    public Color BaseColor;
    /// <summary>
    /// 分别存储起始缩放 OriginalScale、目标缩放 FinalScale 和当前透明度 Opacity；仅 Opacity 初始为 1。
    /// </summary>
    public float OriginalScale, FinalScale, Opacity = 1f;
    /// <summary>
    /// 分别存储基础透明度 BaseOpacity 和光照倍率 LightStrength；光照倍率不大于零时跳过光照。
    /// </summary>
    public float BaseOpacity, LightStrength;
    /// <summary>
    /// 是否在寿命内按正弦曲线淡出。
    /// </summary>
    public bool FadeOut;
    /// <summary>
    /// 保存的精灵翻转设置；是否应用取决于具体粒子的绘制实现。
    /// </summary>
    public SpriteEffects SpriteEffects;
    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>false</c>。</remarks>
    public override bool AutoLoadTexture => false;
    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>TOTextures.InvisibleTexturePath</c>。</remarks>
    public override string TexturePath => TOTextures.InvisibleTexturePath;
    /// <inheritdoc/>
    public override BlendState DrawBlendState { get; }

    /// <summary>
    /// 请求指定纹理并创建脉冲粒子；lifetime 必须为正帧数，设置双轴形变、弧度旋转、起止缩放、透明度与光照，专用服务器不请求纹理。
    /// </summary>
    public TexturePulseParticle(Vector2 center, Vector2 velocity, Color color, string texturePath, Vector2 squish,
        float rotation, float originalScale, float finalScale, int lifetime, bool additive = true,
        float baseOpacity = 1f, bool fadeOut = true, float lightStrength = 1f, SpriteEffects spriteEffects = SpriteEffects.None)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(lifetime);
        SpriteAsset = Main.dedServ ? null : ModContent.Request<Texture2D>(texturePath);
        Center = center;
        Velocity = velocity;
        BaseColor = color;
        Squish = squish;
        Rotation = rotation;
        OriginalScale = Scale = originalScale;
        FinalScale = finalScale;
        Lifetime = lifetime;
        DrawBlendState = additive ? BlendState.Additive : BlendState.AlphaBlend;
        BaseOpacity = baseOpacity;
        FadeOut = fadeOut;
        LightStrength = lightStrength;
        SpriteEffects = spriteEffects;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 以指数缓出更新尺寸，按开关淡出，更新颜色和光照并衰减速度。
    /// </remarks>
    public override void Update()
    {
        Scale = TOMathUtils.Interpolation.ExponentialEaseOut(OriginalScale, FinalScale, LifetimeCompletion, 4f);
        Opacity = BaseOpacity * (FadeOut ? 1f - TOMathUtils.Interpolation.SineEaseIn(LifetimeCompletion) : 1f);
        Color = BaseColor * Opacity;
        if (LightStrength > 0f) Lighting.AddLight(Center, Color.ToVector3() * LightStrength);
        Velocity *= 0.95f;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 执行该粒子的自定义绘制，将中心加像素偏移后转换到屏幕坐标；返回 false 跳过默认绘制。
    /// </remarks>
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        Texture2D texture = SpriteAsset.Value;
        spriteBatch.Draw(texture, Center + drawOffset - Main.screenPosition, null, Color * Opacity, Rotation,
            texture.Size() / 2f, Scale * Squish, SpriteEffects, 0f);
        return false;
    }
}
