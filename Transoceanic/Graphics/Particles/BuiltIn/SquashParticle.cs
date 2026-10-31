using Transoceanic.Graphics.Assets;

namespace Transoceanic.Graphics.Particles.BuiltIn;

/// <summary>
/// 随速度改变双轴形状并逐渐缩小的光球粒子。
/// </summary>
public class SquashParticle : Particle
{
    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>false</c>。</remarks>
    public override bool AutoLoadTexture => false;
    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>TOTextures.InvisibleTexturePath</c>。</remarks>
    public override string TexturePath => TOTextures.InvisibleTexturePath;
    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>BlendState.Additive</c>。</remarks>
    public override BlendState DrawBlendState => BlendState.Additive;

    /// <summary>
    /// 按速度形变前的双轴缩放基准，默认 Vector2.One。
    /// </summary>
    public Vector2 BaseSize = Vector2.One;
    /// <summary>
    /// 是否关闭粒子的随机向下加速度。
    /// </summary>
    public bool NoGravity;
    /// <summary>
    /// 影响缩小速度和重力强度的附加系数，计算时使用 FadeIn + 1。
    /// </summary>
    public float FadeIn;

    /// <summary>
    /// 创建随速度形变的光球，设置中心、速度、帧寿命、缩放、颜色、双轴基准尺寸和重力开关。
    /// </summary>
    public SquashParticle(Vector2 center, Vector2 velocity, int lifetime, float scale, Color color, Vector2? baseSize = null, bool noGravity = false)
    {
        Center = center;
        Velocity = velocity;
        Lifetime = lifetime;
        Scale = scale;
        Color = color;
        BaseSize = baseSize ?? Vector2.One;
        NoGravity = noGravity;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 按速度旋转、衰减速度并缩小，按开关施加重力，添加光照；尺寸耗尽时死亡，本方法也会应用一次位移。
    /// </remarks>
    public override void Update()
    {
        float fadeSpeed = FadeIn + 1f;
        Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
        Velocity *= 0.96f;

        if (NoGravity)
            Scale -= 0.045f * fadeSpeed;
        else
        {
            Scale -= 0.03f * fadeSpeed;
            Velocity.Y += Main.rand.NextFloat(0.1f, 0.35f) * fadeSpeed;
        }

        float light = MathHelper.Clamp(Scale * 0.8f, 0f, 1f);
        Lighting.AddLight(Center, Color.ToVector3() * light);

        if (Scale <= 0f)
            Kill();

        if (AutoUpdatePosition)
            Center += Velocity;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 执行该粒子的自定义绘制，将中心加像素偏移后转换到屏幕坐标；返回 false 跳过默认绘制。
    /// </remarks>
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        Texture2D bloom = ParticleHandler.GetTexture<BloomParticle>();
        Texture2D solid = TOTextures.Extra.BasicCircle;

        Vector2 squash = new(Utils.Remap(Velocity.Length(), 2, 7, 1 * BaseSize.X, 0.5f * BaseSize.X), Utils.Remap(Velocity.Length(), 2, 7, 1 * BaseSize.Y, 2.5f * BaseSize.Y));

        // Glow Orb (larger subtle glow)
        spriteBatch.DrawFromCenter_VectorScale(bloom, Center + drawOffset - Main.screenPosition, null, Color * 0.6f, Rotation, squash * Scale * 0.1f, SpriteEffects.None, 0f);
        spriteBatch.DrawFromCenter_VectorScale(bloom, Center + drawOffset - Main.screenPosition, null, Color * 0.85f, Rotation, squash * Scale * 0.04f, SpriteEffects.None, 0f);

        // Solid center
        spriteBatch.DrawFromCenter_VectorScale(solid, Center + drawOffset - Main.screenPosition, null, Color.Lerp(Color, Color.White, 0.3f) * 0.9f, Rotation, squash * Scale * 0.075f, SpriteEffects.None, 0f);

        return false;
    }
}
