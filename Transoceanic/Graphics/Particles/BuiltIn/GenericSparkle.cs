namespace Transoceanic.Graphics.Particles.BuiltIn;

/// <summary>
/// 具有旋转星芒、光晕和正弦亮度的加法粒子。
/// </summary>
public class GenericSparkle : Particle
{
    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>&quot;Transoceanic/Assets/Textures/Particles/Sparkle&quot;</c>。</remarks>
    public override string TexturePath => "Transoceanic/Assets/Textures/Particles/Sparkle";
    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>BlendState.Additive</c>。</remarks>
    public override BlendState DrawBlendState => BlendState.Additive;

    private float _Spin;
    private float _Opacity;
    private Color _Bloom;
    private Color LightColor => _Bloom * _Opacity;
    private float _BloomScale;

    /// <summary>
    /// 创建星光及光晕粒子，设置中心、速度、帧寿命、每帧弧度转速、光晕缩放和重要性；初始旋转随机。
    /// </summary>
    public GenericSparkle(Vector2 position, Vector2 velocity, Color color, Color bloom, float scale, int lifeTime, float rotationSpeed = 1f, float bloomScale = 1f, bool needed = false)
    {
        Center = position;
        Velocity = velocity;
        Color = color;
        _Bloom = bloom;
        Scale = scale;
        Lifetime = lifeTime;
        Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        _Spin = rotationSpeed;
        _BloomScale = bloomScale;
        Important = needed;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 按正弦寿命曲线更新亮度，添加光照、衰减速度，并按水平速度方向旋转。
    /// </remarks>
    public override void Update()
    {
        _Opacity = (float)Math.Sin(LifetimeCompletion * MathHelper.Pi);
        Lighting.AddLight(Center, LightColor.R / 255f, LightColor.G / 255f, LightColor.B / 255f);
        Velocity *= 0.95f;
        Rotation += _Spin * ((Velocity.X > 0) ? 1f : -1f);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 执行该粒子的自定义绘制，将中心加像素偏移后转换到屏幕坐标；返回 false 跳过默认绘制。
    /// </remarks>
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        Texture2D starTexture = Texture;
        Texture2D bloomTexture = ParticleHandler.GetTexture<BloomParticle>();
        //Ajust the bloom's texture to be the same size as the star's
        float properBloomSize = (float)starTexture.Height / bloomTexture.Height;

        spriteBatch.Draw(bloomTexture, Center + drawOffset - Main.screenPosition, null, _Bloom * _Opacity * 0.5f, 0, bloomTexture.Size() / 2f, Scale * _BloomScale * properBloomSize, SpriteEffects.None, 0);
        spriteBatch.Draw(starTexture, Center + drawOffset - Main.screenPosition, null, Color * _Opacity * 0.5f, Rotation + MathHelper.PiOver4, starTexture.Size() / 2f, Scale * 0.75f, SpriteEffects.None, 0);
        spriteBatch.Draw(starTexture, Center + drawOffset - Main.screenPosition, null, Color * _Opacity, Rotation, starTexture.Size() / 2f, Scale, SpriteEffects.None, 0);
        return false;
    }
}
