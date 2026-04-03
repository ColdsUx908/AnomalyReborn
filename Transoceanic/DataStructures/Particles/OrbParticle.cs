namespace Transoceanic.DataStructures.Particles;

public class OrbParticle : Particle
{
    public override BlendState DrawBlendState => UseAdditiveBlend ? BlendState.Additive : BlendState.AlphaBlend;

    public Color InitialColor;
    public bool UseAdditiveBlend = true;
    public float FadeOut = 1;
    public bool GlowCenter;
    public float InitialScale;
    public float GravityMultiplier;
    public float LifeEndRatio;
    public bool AffectedByGravity => GravityMultiplier > 0f;

    public OrbParticle(Vector2 center, Vector2 velocity, int lifetime, float scale, Color color, float gravityMultiplier = 0f, float lifeEndRatio = 0f, bool useAdditiveBlend = true, bool glowCenter = true)
    {
        Center = center;
        Velocity = velocity;
        GravityMultiplier = gravityMultiplier;
        Scale = scale;
        Lifetime = lifetime;
        Color = InitialColor = color;
        UseAdditiveBlend = useAdditiveBlend;
        GlowCenter = glowCenter;
        InitialScale = scale;
        LifeEndRatio = lifeEndRatio;
    }

    public override void Update()
    {
        if (LifetimeCompletion > LifeEndRatio)
        {
            float interpolation = TOMathUtils.Interpolation.QuadraticEaseOut(1f - (LifetimeCompletion - LifeEndRatio) / (1f - LifeEndRatio));
            FadeOut = interpolation;
            Scale = InitialScale * interpolation;
        }
        Color = Color.Lerp(InitialColor, InitialColor * 0.2f, MathF.Pow(LifetimeCompletion, 3));
        Velocity *= 0.98f;
        if (Velocity.Y < 12f * GravityMultiplier && AffectedByGravity)
            Velocity.Y += 0.25f * GravityMultiplier;
        Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
    }

    public override bool PreDraw(SpriteBatch spriteBatch)
    {
        Texture2D texture = Texture;
        Vector2 scale = new(Scale);

        spriteBatch.DrawFromCenter_VectorScale(texture, Center - Main.screenPosition, null, Color, Rotation, scale);
        if (GlowCenter)
            spriteBatch.DrawFromCenter_VectorScale(texture, Center - Main.screenPosition, null, Color.White * FadeOut, Rotation, scale * new Vector2(0.5f, 0.5f));

        return false;
    }
}