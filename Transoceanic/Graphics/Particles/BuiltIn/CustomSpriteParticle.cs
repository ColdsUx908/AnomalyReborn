using Transoceanic.Graphics.Assets;

namespace Transoceanic.Graphics.Particles.BuiltIn;

/// <summary>
/// 支持自定义纹理、更新回调和选帧回调的精灵粒子。
/// </summary>
public class CustomSpriteParticle : Particle
{
    /// <summary>
    /// 残影或自定义精灵粒子使用的外部纹理，不由粒子释放。
    /// </summary>
    public new Texture2D Texture;
    /// <summary>
    /// 自定义精灵粒子的重力速度阈值；为零时不施加该重力逻辑。
    /// </summary>
    public float MaxGravity;
    /// <summary>
    /// 当前绘制使用的透明度系数，由粒子更新逻辑维护。
    /// </summary>
    public float Opacity = 1f;
    /// <summary>
    /// 保存的精灵翻转设置；是否应用取决于具体粒子的绘制实现。
    /// </summary>
    public SpriteEffects SpriteEffects;

    /// <summary>
    /// 可选的纹理帧选择委托；为空时使用默认帧，返回 null 表示绘制整张纹理。
    /// </summary>
    public Func<CustomSpriteParticle, Rectangle?> CustomGetFrameAction;
    /// <summary>
    /// 默认速度和淡出更新后执行的自定义更新回调，可为 null。
    /// </summary>
    public Action<CustomSpriteParticle> CustomUpdateAction;

    /// <summary>
    /// 自定义精灵粒子的额外 Union32 数据槽数量。
    /// </summary>
    public const int ExtraDataSlots = 8;
    /// <summary>
    /// 调用方使用的粒子额外数据槽；每个粒子实例拥有独立数组。
    /// </summary>
    public Union32[] ExtraData = new Union32[ExtraDataSlots];

    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>false</c>。</remarks>
    public override bool AutoLoadTexture => false;
    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>TOTextures.InvisibleTexturePath</c>。</remarks>
    public override string TexturePath => TOTextures.InvisibleTexturePath;
    /// <inheritdoc/>
    public override BlendState DrawBlendState { get; }

    /// <summary>
    /// 创建自定义精灵粒子，设置像素坐标、像素/帧速度、帧寿命、弧度旋转和更新/选帧回调；纹理由调用方提供或按路径请求。
    /// </summary>
    public CustomSpriteParticle(Texture2D texture, Vector2 center, Vector2 velocity, int lifetime, float rotation, float scale, Color color, float maxGravity = 0f, BlendState blendState = null, SpriteEffects spriteEffects = SpriteEffects.None,
        Func<CustomSpriteParticle, Rectangle?> customGetFrameAction = null, Action<CustomSpriteParticle> customUpdateAction = null)
    {
        Texture = texture;
        Center = center;
        Velocity = velocity;
        Lifetime = lifetime;
        Rotation = rotation;
        Scale = scale;
        Color = color;
        MaxGravity = maxGravity;
        DrawBlendState = blendState ?? BlendState.AlphaBlend;
        SpriteEffects = spriteEffects;
        CustomGetFrameAction = customGetFrameAction;
        CustomUpdateAction = customUpdateAction;
    }

    /// <summary>
    /// 创建自定义精灵粒子，设置像素坐标、像素/帧速度、帧寿命、弧度旋转和更新/选帧回调；纹理由调用方提供或按路径请求。
    /// </summary>
    public CustomSpriteParticle(string texturePath, Vector2 center, Vector2 velocity, int lifetime, float rotation, float scale, Color color, float maxGravity = 0f, BlendState blendState = null, SpriteEffects spriteEffects = SpriteEffects.None,
        Func<CustomSpriteParticle, Rectangle?> customGetFrameAction = null, Action<CustomSpriteParticle> customUpdateAction = null)
        : this(ModContent.Request<Texture2D>(texturePath).Value, center, velocity, lifetime, rotation, scale, color, maxGravity, blendState, spriteEffects, customGetFrameAction, customUpdateAction)
    { }

    /// <inheritdoc/>
    /// <remarks>
    /// 末尾 20 帧缩小并淡出，衰减速度并按 MaxGravity 调整重力，最后调用自定义更新回调。
    /// </remarks>
    public override void Update()
    {
        if (Timer > Lifetime - 20)
        {
            Scale *= 0.9f;
            Opacity *= 0.9f;
        }

        Velocity *= 0.85f;
        if (MaxGravity != 0f)
        {
            if (Velocity.Length() < MaxGravity)
            {
                Velocity.X *= 0.94f;
                Velocity.Y += MaxGravity / 10f;
            }
        }

        CustomUpdateAction?.Invoke(this);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 执行该粒子的自定义绘制，将中心加像素偏移后转换到屏幕坐标；返回 false 跳过默认绘制。
    /// </remarks>
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        spriteBatch.DrawFromCenter(Texture, Center + drawOffset - Main.screenPosition, CustomGetFrameAction?.Invoke(this), Color * Opacity, Rotation, Scale, SpriteEffects.None);
        return false;
    }
}

