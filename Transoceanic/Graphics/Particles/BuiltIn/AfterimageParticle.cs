using Transoceanic.Graphics.Assets;

namespace Transoceanic.Graphics.Particles.BuiltIn;

/// <summary>
/// 由实体或调用方管理的纹理残影，使用自定义绘制而非全局默认绘制。
/// </summary>
public class AfterimageParticle : Particle
{
    /// <summary>
    /// 是否由实体默认残影流程调用此残影的 Draw。
    /// </summary>
    public bool UseDefaultDraw = true;

    /// <summary>
    /// 残影或自定义精灵粒子使用的外部纹理，不由粒子释放。
    /// </summary>
    public new Texture2D Texture;
    /// <summary>
    /// 残影纹理源矩形；null 表示绘制整张纹理。
    /// </summary>
    public Rectangle? Frame;
    /// <summary>
    /// 残影初始透明度系数；更新时还会乘以 0.5 和寿命淡出。
    /// </summary>
    public float OriginalOpacity = 1f;
    /// <summary>
    /// 当前绘制使用的透明度系数，由粒子更新逻辑维护。
    /// </summary>
    public float Opacity = 1f;
    /// <summary>
    /// 附加在残影中心上的像素绘制偏移。
    /// </summary>
    public Vector2 DrawOffset;

    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>false</c>。</remarks>
    public override bool AutoLoadTexture => false;
    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>TOTextures.InvisibleTexturePath</c>。</remarks>
    public override string TexturePath => TOTextures.InvisibleTexturePath;

    /// <summary>
    /// 创建由调用方管理的残影，记录纹理、源矩形、中心像素坐标、帧寿命、弧度旋转和缩放；不自动提交全局系统。
    /// </summary>
    public AfterimageParticle(Texture2D texture, Rectangle? frame, Vector2 center, int lifetime, float rotation, float scale, Color color, float originalOpacity = 1f, Vector2? drawOffset = null, bool affectedByLight = true)
    {
        Texture = texture;
        Frame = frame;
        Center = center;
        Lifetime = lifetime;
        Rotation = rotation;
        Scale = scale;
        Color = color;
        OriginalOpacity = originalOpacity;
        DrawOffset = drawOffset ?? Vector2.Zero;
        AffectedByLight = affectedByLight;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 返回 false，阻止普通残影加入全局粒子系统；由实体残影列表管理。
    /// </remarks>
    public override bool PreSpawn() => false;

    /// <inheritdoc/>
    /// <remarks>
    /// 按 Timer / (Lifetime + 1) 线性衰减透明度，初始透明度系数为 OriginalOpacity 的一半。
    /// </remarks>
    public override void Update()
    {
        float lifetimeCompletion = Timer / (Lifetime + 1f);
        Opacity = OriginalOpacity * 0.5f * (1f - lifetimeCompletion);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 返回 false，跳过默认粒子绘制；应由残影管理方显式调用 Draw。
    /// </remarks>
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default) => false;

    /// <summary>
    /// 使用自有纹理、源矩形和 DrawOffset 绘制残影，按需混合世界光照。
    /// </summary>
    public void Draw(SpriteBatch spriteBatch)
    {
        Color color = Color * Opacity;
        if (AffectedByLight)
            color.MultiplyWithWorldLight(Center);
        spriteBatch.DrawFromCenter(Texture, Center + DrawOffset - Main.screenPosition, Frame, color, Rotation, Scale, SpriteEffects.None);
    }
}
