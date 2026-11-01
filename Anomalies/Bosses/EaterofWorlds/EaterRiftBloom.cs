// Adapted from CalamityMod; Copyright Azafure, LLC. See Portal/README.md.
namespace Anomalies.Bosses.EaterofWorlds;

// Keep the rift's sinusoidal flash rather than the standard bloom's delayed fade.
public sealed class EaterRiftBloom : BloomParticle
{
    public EaterRiftBloom(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        : base(position, velocity, color, scale, scale, lifetime)
    {
        Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
    }

    public override void Update()
    {
        Opacity = MathF.Sin(LifetimeCompletion * MathHelper.Pi);
        Color = BaseColor * Opacity;
        Lighting.AddLight(Center, Color.ToVector3());
        Velocity *= 0.95f;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        spriteBatch.Draw(Texture, Center + drawOffset - Main.screenPosition, null, Color * Opacity,
            Rotation, Texture.Size() * 0.5f, Scale, SpriteEffects.None, 0f);
        return false;
    }
}
