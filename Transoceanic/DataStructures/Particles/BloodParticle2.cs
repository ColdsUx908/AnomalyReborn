namespace Transoceanic.DataStructures.Particles;

public class BloodParticle2 : Particle
{
    public Color InitialColor;

    public override BlendState DrawBlendState => BlendState.Additive;

    public override string TexturePath => "Transoceanic/DataStructures/Particles/Blood2";

    public BloodParticle2(Vector2 relativeCenter, Vector2 velocity, int lifetime, float scale, Color color)
    {
        Center = relativeCenter;
        Velocity = velocity;
        Scale = scale;
        Lifetime = lifetime;
        Color = InitialColor = color;
    }

    public override void Update()
    {
        Velocity *= 0.98f;
        Color = Color.Lerp(InitialColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 4D));
        Rotation = Velocity.ToRotation();
        // Cycle through disco colors if blood and gore is off
        if (!ChildSafety.Disabled)
        {
            Color = Main.DiscoColor;
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        float brightness = (float)Math.Pow(Lighting.Brightness((int)(Center.X / 16f), (int)(Center.Y / 16f)), 0.15);
        Texture2D texture = Texture;
        Rectangle frame = texture.Frame(1, 3, 0, Math.Clamp((int)(LifetimeCompletion * 3f), 0, 2));
        Vector2 origin = frame.Size() * 0.5f;

        spriteBatch.Draw(texture, Center + drawOffset - Main.screenPosition, frame, Color * brightness, Rotation, origin, Scale, 0, 0f);
        return false;
    }
}
