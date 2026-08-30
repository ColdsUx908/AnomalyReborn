using Transoceanic.DataStructures.Assets;

namespace Transoceanic.DataStructures.Particles;

public class SquashParticle : Particle
{
    public override bool AutoLoadTexture => false;
    public override string TexturePath => TOTextures.InvisibleTexturePath;
    public override BlendState DrawBlendState => BlendState.Additive;

    public Vector2 BaseSize = Vector2.One;
    public bool NoGravity;
    public float FadeIn;

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
