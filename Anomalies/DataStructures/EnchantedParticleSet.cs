namespace Anomalies.DataStructures;

public class EnchantedParticleSet : ParticleSet
{
    public float InterpolationSpeed;
    public float MaxEdgeRadius;
    public float Scale;
    public Func<Color> EdgeColorFunction;
    public Func<Color> CenterColorFunction;
    public override int ParticleLifetime => 50;
    public EnchantedParticleSet(int setLifetime, float particleSpawnRate, Func<Color> edgeColorFunction, Func<Color> centerColorFunction, float interpolationSpeed, float maxEdgeRadius, float scale = 1f) : base(setLifetime, particleSpawnRate)
    {
        EdgeColorFunction = edgeColorFunction ?? (() => Color.White);
        CenterColorFunction = centerColorFunction ?? (() => Color.White);
        InterpolationSpeed = interpolationSpeed;
        MaxEdgeRadius = maxEdgeRadius;
        Scale = scale;
    }

    public EnchantedParticleSet(int setLifetime, float particleSpawnRate, Color edgeColor, Color centerColor, float interpolationSpeed, float maxEdgeRadius, float scale = 1f) : base(setLifetime, particleSpawnRate)
    {
        EdgeColorFunction = () => edgeColor;
        CenterColorFunction = () => centerColor;
        InterpolationSpeed = interpolationSpeed;
        MaxEdgeRadius = maxEdgeRadius;
        Scale = scale;

    }

    public override Particle SpawnParticle() => new EnchantedParticle(Main.rand.NextPolarVector2(MaxEdgeRadius * 0.6f, MaxEdgeRadius), ParticleLifetime, 0.1f, EdgeColorFunction(), CenterColorFunction(), InterpolationSpeed, MaxEdgeRadius);
}