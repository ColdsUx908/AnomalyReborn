using Transoceanic.Graphics.Assets.Loading;
namespace Transoceanic.Graphics.Particles.BuiltIn;

/// <summary>
/// 支持双轴形变及高清纹理的扩张淡出圆环。
/// </summary>
public class PulseRing : Particle
{
    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>ParticleHandler.BaseParticleTexturePath + &quot;HollowCircleHardEdge&quot;</c>。</remarks>
    public override string TexturePath => ParticleHandler.BaseParticleTexturePath + "HollowCircleHardEdge";
    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>BlendState.Additive</c>。</remarks>
    public override BlendState DrawBlendState => BlendState.Additive;

    [LoadTexture(ParticleHandler.BaseParticleTexturePath + "HollowCircleHardEdgeHD")]
    private static Asset<Texture2D> _TextureAssetHD;
    /// <summary>
    /// 高清圆环纹理；资源尚未加载时为 null。
    /// </summary>
    public static Texture2D TextureHD => _TextureAssetHD?.Value;

    /// <summary>
    /// 普通圆环纹理的基准像素半径。
    /// </summary>
    public const float TextureRadius = 78f;
    /// <summary>
    /// 高清圆环纹理的基准像素半径。
    /// </summary>
    public const float TextureRadiusHD = 740f;
    /// <summary>
    /// 转换因子，用于将基于HD纹理的半径转换为基于普通纹理的半径。
    /// <br/>该因子的值为 TextureRadius / TextureRadiusHD，即 78f / 740f，约等于 0.1054。这意味着基于HD纹理的半径值需要乘以这个因子才能得到对应的基于普通纹理的半径值。
    /// <br/>该因子可确保绘制时无论使用哪种纹理，粒子的视觉大小都能保持一致。
    /// </summary>
    public const float TextureRadiusConversionFactor = TextureRadius / TextureRadiusHD;

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
    public float Opacity;
    /// <summary>
    /// 绘制时应用的双轴形变倍率。
    /// </summary>
    public Vector2 Squish;
    /// <summary>
    /// 应用淡出前的基础颜色。
    /// </summary>
    public Color BaseColor;
    /// <summary>
    /// 是否使用高清圆环纹理，并按半径比例修正缩放。
    /// </summary>
    public bool UseHDTexture;

    /// <summary>
    /// 创建脉冲环，设置起止缩放、帧寿命及可选双轴形变、弧度旋转和高清纹理；简化重载随机初始旋转。
    /// </summary>
    public PulseRing(Vector2 center, Vector2 velocity, Color color, float originalScale, float finalScale, int lifeTime)
        : this(center, velocity, color, Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), originalScale, finalScale, lifeTime)
    { }

    /// <summary>
    /// 创建脉冲环，设置起止缩放、帧寿命及可选双轴形变、弧度旋转和高清纹理；简化重载随机初始旋转。
    /// </summary>
    public PulseRing(Vector2 center, Vector2 velocity, Color color, Vector2 squish, float rotation, float originalScale, float finalScale, int lifeTime, bool useHDTexture = false)
    {
        Center = center;
        Velocity = velocity;
        BaseColor = color;
        OriginalScale = originalScale;
        FinalScale = finalScale;
        Scale = originalScale;
        Lifetime = lifeTime;
        Squish = squish;
        Rotation = rotation;
        UseHDTexture = useHDTexture;
        UpdateDrawScreenCheckFluff();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 以指数缓出扩张并按正弦曲线淡出，添加光照并衰减速度。
    /// </remarks>
    public override void Update()
    {
        Scale = MathHelper.Lerp(OriginalScale, FinalScale, TOMathUtils.Interpolation.ExponentialEaseOut(LifetimeCompletion, 4f));
        UpdateDrawScreenCheckFluff();
        Opacity = 1f - TOMathUtils.Interpolation.SineEaseIn(LifetimeCompletion);

        Color = BaseColor * Opacity;
        Lighting.AddLight(Center, Color.R / 255f, Color.G / 255f, Color.B / 255f);
        Velocity *= 0.95f;
    }

    private void UpdateDrawScreenCheckFluff()
    {
        Texture2D texture = UseHDTexture ? TextureHD : Texture;
        float scale = Math.Abs(Scale) * (UseHDTexture ? TextureRadiusConversionFactor : 1f);
        float width = texture.Width * scale * Math.Abs(Squish.X);
        float height = texture.Height * scale * Math.Abs(Squish.Y);
        DrawScreenCheckFluff = MathF.Sqrt(width * width + height * height) * 0.5f;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 执行该粒子的自定义绘制，将中心加像素偏移后转换到屏幕坐标；返回 false 跳过默认绘制。
    /// </remarks>
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        spriteBatch.DrawFromCenter_VectorScale(UseHDTexture ? TextureHD : Texture, Center + drawOffset - Main.screenPosition, null, Color * Opacity, Rotation, (UseHDTexture ? Scale * TextureRadiusConversionFactor : Scale) * Squish, SpriteEffects.None, 0);
        return false;
    }
}

