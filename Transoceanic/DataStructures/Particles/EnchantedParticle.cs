namespace Transoceanic.DataStructures.Particles;

public class EnchantedParticle : Particle
{
    public float RelativePower;

    public float InterpolationSpeed;
    public float EdgeOffset;
    public Color EdgeColor;
    public Color CenterColor;

    public override string TexturePath => ParticleHandler.BaseParticleTexturePath + "Light";

    public EnchantedParticle(Vector2 relativePosition, int lifetime, float scale, Color edgeColor, Color centerColor, float interpolationSpeed, float edgeOffset)
    {
        Center = relativePosition;
        Velocity = Vector2.Zero;
        Scale = scale;
        Lifetime = lifetime;
        EdgeColor = edgeColor;
        CenterColor = centerColor;
        InterpolationSpeed = interpolationSpeed;
        EdgeOffset = edgeOffset;
    }

    public override void Update()
    {
        float distanceToCenter = Center.Length();
        Scale = MathHelper.SmoothStep(0.05f, 0.125f, Utils.GetLerpValue(EdgeOffset, 6f, distanceToCenter, true));
        Scale *= Utils.GetLerpValue(Lifetime, Lifetime - 10f, Timer, true);

        if (distanceToCenter > 4f)
            Center = Vector2.Lerp(Center, Vector2.Zero, InterpolationSpeed);
        else
        {
            Scale *= 0.92f;
            if (Scale < 0.05f)
                Kill();
        }

        Color = Color.Lerp(EdgeColor, CenterColor, Utils.GetLerpValue(0f, 0.67f, LifetimeCompletion, true));
        Color.A = 50;
    }
}
