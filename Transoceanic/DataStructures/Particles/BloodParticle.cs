namespace Transoceanic.DataStructures.Particles;

public class BloodParticle : Particle
{
    public Color InitialColor;

    public override BlendState DrawBlendState => BlendState.Additive;

    public override string TexturePath => "Transoceanic/DataStructures/Particles/Blood";

    public BloodParticle(Vector2 relativeCenter, Vector2 velocity, int lifetime, float scale, Color color)
    {
        Center = relativeCenter;
        Velocity = velocity;
        Scale = scale;
        Lifetime = lifetime;
        Color = InitialColor = color;
    }

    public override void Update()
    {
        Scale *= 0.98f;
        Velocity.X *= 0.97f;
        Velocity.Y = MathHelper.Clamp(Velocity.Y + 0.9f, -22f, 22f);
        Color = Color.Lerp(InitialColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 3D));
        Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
        // Cycle through disco colors if blood and gore is off
        if (!ChildSafety.Disabled)
        {
            Color = Main.DiscoColor;
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        float verticalStretch = Utils.GetLerpValue(0f, 24f, Math.Abs(Velocity.Y), true) * 0.84f;
        float brightness = (float)Math.Pow(Lighting.Brightness((int)(Center.X / 16f), (int)(Center.Y / 16f)), 0.15);
        Vector2 scale = new Vector2(1f, verticalStretch + 1f) * Scale * 0.1f;
        Texture2D texture = Texture;

        spriteBatch.Draw(texture, Center + drawOffset - Main.screenPosition, null, Color * brightness, Rotation, texture.Size() * 0.5f, scale, 0, 0f);
        return false;
    }
}
