namespace Transoceanic.Graphics.Particles.BuiltIn;

/// <summary>
/// 使用随机纹理帧、按透明度变色与消散的雾粒子。
/// </summary>
public class MediumMistParticle : Particle
{
    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>ParticleHandler.BaseParticleTexturePath + &quot;MediumMistParticle&quot;</c>。</remarks>
    public override string TexturePath => ParticleHandler.BaseParticleTexturePath + "MediumMistParticle";

    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>BlendState.Additive</c>。</remarks>
    public override BlendState DrawBlendState => BlendState.Additive;

    /// <summary>
    /// 雾纹理的纵向帧索引，构造时随机取 0、1 或 2。
    /// </summary>
    public int Variant;
    /// <summary>
    /// 雾粒子透明度，使用 0 至 255 量级，每次更新递减；低于零时死亡。
    /// </summary>
    public float Opacity;
    /// <summary>
    /// 雾粒子渐变开始时的颜色。
    /// </summary>
    public Color ColorFire;
    /// <summary>
    /// 雾粒子透明度降低后渐变到的颜色。
    /// </summary>
    public Color ColorFade;
    /// <summary>
    /// 每次更新的旋转量，单位为弧度，方向随水平速度符号决定。
    /// </summary>
    public float Spin;

    /// <summary>
    /// 创建随机三帧雾粒子，设置像素坐标、速度、起止颜色、缩放、0 至 255 量级的透明度及每帧弧度转速；未设置 Lifetime，需调用方配置生命周期。
    /// </summary>
    public MediumMistParticle(Vector2 center, Vector2 velocity, Color colorFire, Color colorFade, float scale, float opacity, float rotationSpeed = 0f)
    {
        Center = center;
        Velocity = velocity;
        ColorFire = colorFire;
        ColorFade = colorFade;
        Scale = scale;
        Opacity = opacity;
        Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        Spin = rotationSpeed;
        Variant = Main.rand.Next(3);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 旋转并衰减速度，按透明度分段更新尺寸与颜色；透明度低于零时标记死亡。
    /// </remarks>
    public override void Update()
    {
        Rotation += Spin * ((Velocity.X > 0) ? 1f : -1f);
        Velocity *= 0.85f;

        if (Opacity > 90)
        {
            Lighting.AddLight(Center, Color.ToVector3() * 0.1f);
            Scale += 0.01f;
            Opacity -= 3;
        }
        else
        {
            Scale *= 0.975f;
            Opacity -= 2;
        }

        if (Opacity < 0)
            Kill();

        Color = Color.Lerp(ColorFire, ColorFade, MathHelper.Clamp((float)(255 - Opacity - 100) / 80, 0f, 1f)) * (Opacity / 255f);

    }

    /// <inheritdoc/>
    /// <remarks>
    /// 按 Variant 从纵向三帧纹理中选取一帧。
    /// </remarks>
    public override Rectangle? GetFrame(Texture2D texture) => texture.Frame(1, 3, 0, Variant);
}
