namespace Transoceanic.DataStructures.Particles;

public class BloomParticle : Particle
{
    [LoadTexture(ParticleHandler.BaseParticleTexturePath + "BloomCircleLarge")]
    private static Asset<Texture2D> _bloomCircleLarge;
    public static Texture2D BloomCircleLarge => _bloomCircleLarge.Value;

    public override string TexturePath => ParticleHandler.BaseParticleTexturePath + "BloomCircle";
    public override BlendState DrawBlendState => BlendState.Additive;

    public float OriginalScale;
    public float FinalScale;
    public float Opacity = 1f;
    public Color BaseColor;
    public float LifeEndRatio;
    public bool UseLargeTexture;

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

    public override bool PreDraw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawFromCenter(UseLargeTexture ? BloomCircleLarge : Texture, Center - Main.screenPosition, null, Color, scale: Scale);
        return false;
    }
}
