namespace Transoceanic.Graphics.Particles.BuiltIn;

public class GenericSparkle : Particle
{
    public override string TexturePath => "Transoceanic/Assets/Textures/Particles/Sparkle";
    public override BlendState DrawBlendState => BlendState.Additive;

    private float _Spin;
    private float _Opacity;
    private Color _Bloom;
    private Color LightColor => _Bloom * _Opacity;
    private float _BloomScale;

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

    public override void Update()
    {
        _Opacity = (float)Math.Sin(LifetimeCompletion * MathHelper.Pi);
        Lighting.AddLight(Center, LightColor.R / 255f, LightColor.G / 255f, LightColor.B / 255f);
        Velocity *= 0.95f;
        Rotation += _Spin * ((Velocity.X > 0) ? 1f : -1f);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        Texture2D starTexture = Texture;
        Texture2D bloomTexture = ModContent.Request<Texture2D>("Transoceanic/Assets/Textures/Particles/BloomCircle").Value;
        //Ajust the bloom's texture to be the same size as the star's
        float properBloomSize = (float)starTexture.Height / (float)bloomTexture.Height;

        spriteBatch.Draw(bloomTexture, Center + drawOffset - Main.screenPosition, null, _Bloom * _Opacity * 0.5f, 0, bloomTexture.Size() / 2f, Scale * _BloomScale * properBloomSize, SpriteEffects.None, 0);
        spriteBatch.Draw(starTexture, Center + drawOffset - Main.screenPosition, null, Color * _Opacity * 0.5f, Rotation + MathHelper.PiOver4, starTexture.Size() / 2f, Scale * 0.75f, SpriteEffects.None, 0);
        spriteBatch.Draw(starTexture, Center + drawOffset - Main.screenPosition, null, Color * _Opacity, Rotation, starTexture.Size() / 2f, Scale, SpriteEffects.None, 0);
        return false;
    }
}
