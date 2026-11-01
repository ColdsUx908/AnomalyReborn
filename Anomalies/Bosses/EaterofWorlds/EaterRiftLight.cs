// Adapted from CalamityMod; Copyright Azafure, LLC. See Portal/README.md.
namespace Anomalies.Bosses.EaterofWorlds;

public class EaterRiftLight : Particle
{
    public override string TexturePath => ParticleHandler.BaseParticleTexturePath + "Light";
    public override BlendState DrawBlendState => BlendState.Additive;

    public float Opacity;
    public float SquishStrength;
    public float MaxSquish;
    public float HueShift;

    public EaterRiftLight(Vector2 position, Vector2 velocity, float scale, Color color, int lifetime, float opacity = 1f, float squishStrength = 1f, float maxSquish = 3f, float hueShift = 0f)
    {
        Center = position;
        Velocity = velocity;
        Scale = scale;
        Color = color;
        Opacity = opacity;
        Rotation = 0;
        Lifetime = lifetime;
        SquishStrength = squishStrength;
        MaxSquish = maxSquish;
        HueShift = hueShift;
    }

    public override void Update()
    {
        Velocity *= (LifetimeCompletion >= 0.34f) ? 0.93f : 1.02f;

        Opacity = LifetimeCompletion > 0.5f ? (float)Math.Sin(LifetimeCompletion * MathHelper.Pi) * 0.2f + 0.8f : (float)Math.Sin(LifetimeCompletion * MathHelper.Pi);
        Scale *= 0.95f;

        Color = Main.hslToRgb(Main.rgbToHsl(Color).X + HueShift, Main.rgbToHsl(Color).Y, Main.rgbToHsl(Color).Z);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        Texture2D tex = Texture;
        Texture2D bloomTex = ParticleHandler.GetTexture<BloomParticle>();

        float squish = MathHelper.Clamp(Velocity.Length() / 10f * SquishStrength, 1f, MaxSquish);

        float rot = Velocity.ToRotation() + MathHelper.PiOver2;
        Vector2 origin = tex.Size() / 2f;
        Vector2 scale = new Vector2(Scale - Scale * squish * 0.3f, Scale * squish);
        float properBloomSize = (float)tex.Height / (float)bloomTex.Height;

        Vector2 drawCenter = Center - Main.screenPosition;

        Main.spriteBatch.Draw(bloomTex, drawCenter, null, Color * Opacity * 0.8f, rot, bloomTex.Size() / 2f, scale * 2 * properBloomSize, SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(tex, drawCenter, null, Color * Opacity * 0.8f, rot, origin, scale * 1.1f, SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(tex, drawCenter, null, Color.White * Opacity * 0.9f, rot, origin, scale, SpriteEffects.None, 0f);

        return false;
    }
}