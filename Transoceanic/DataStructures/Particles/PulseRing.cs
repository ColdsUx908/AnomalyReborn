// Designed by ColdsUx

namespace Transoceanic.DataStructures.Particles;

public class PulseRing : Particle
{
    public override string TexturePath => ParticleHandler.BaseParticleTexturePath + "HollowCircleHardEdge";
    public override BlendState DrawBlendState => BlendState.Additive;

    [LoadTexture(ParticleHandler.BaseParticleTexturePath + "HollowCircleHardEdgeHD")]
    private static Asset<Texture2D> _textureAssetHD;
    public static Texture2D TextureHD => _textureAssetHD?.Value;

    public const float TextureRadius = 78f;
    public const float TextureRadiusHD = 740f;
    /// <summary>
    /// 转换因子，用于将基于HD纹理的半径转换为基于普通纹理的半径。
    /// <br/>该因子的值为 TextureRadius / TextureRadiusHD，即 78f / 740f，约等于 0.1054。这意味着基于HD纹理的半径值需要乘以这个因子才能得到对应的基于普通纹理的半径值。
    /// <br/>该因子可确保绘制时无论使用哪种纹理，粒子的视觉大小都能保持一致。
    /// </summary>
    public const float TextureRadiusConversionFactor = TextureRadius / TextureRadiusHD;

    public float OriginalScale;
    public float FinalScale;
    public float Opacity;
    public Vector2 Squish;
    public Color BaseColor;
    public bool UseHDTexture;

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
    }

    public override void Update()
    {
        Scale = MathHelper.Lerp(OriginalScale, FinalScale, TOMathUtils.Interpolation.ExponentialEaseOut(LifetimeCompletion, 4f));
        Opacity = 1f - TOMathUtils.Interpolation.SineEaseIn(LifetimeCompletion);

        Color = BaseColor * Opacity;
        Lighting.AddLight(Center, Color.R / 255f, Color.G / 255f, Color.B / 255f);
        Velocity *= 0.95f;
    }

    public override bool PreDraw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawFromCenter_VectorScale(UseHDTexture ? TextureHD : Texture, Center - Main.screenPosition, null, Color * Opacity, Rotation, (UseHDTexture ? Scale * TextureRadiusConversionFactor : Scale) * Squish, SpriteEffects.None, 0);
        return false;
    }
}
