using Transoceanic.DataStructures.Assets;

namespace Transoceanic.DataStructures.Particles;

/// <summary>使用任意纹理的扩散脉冲，支持非等比缩放、淡出、混合模式与发光强度。</summary>
public class TexturePulseParticle : Particle
{
    public Asset<Texture2D> SpriteAsset;
    public Vector2 Squish;
    public Color BaseColor;
    public float OriginalScale, FinalScale, Opacity = 1f;
    public float BaseOpacity, LightStrength;
    public bool FadeOut;
    public SpriteEffects SpriteEffects;
    public override bool AutoLoadTexture => false;
    public override string TexturePath => TOTextures.InvisibleTexturePath;
    public override BlendState DrawBlendState { get; }

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

    public override void Update()
    {
        Scale = TOMathUtils.Interpolation.ExponentialEaseOut(OriginalScale, FinalScale, LifetimeCompletion, 4f);
        Opacity = BaseOpacity * (FadeOut ? 1f - TOMathUtils.Interpolation.SineEaseIn(LifetimeCompletion) : 1f);
        Color = BaseColor * Opacity;
        if (LightStrength > 0f) Lighting.AddLight(Center, Color.ToVector3() * LightStrength);
        Velocity *= 0.95f;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        Texture2D texture = SpriteAsset.Value;
        spriteBatch.Draw(texture, Center + drawOffset - Main.screenPosition, null, Color * Opacity, Rotation,
            texture.Size() / 2f, Scale * Squish, SpriteEffects, 0f);
        return false;
    }
}
