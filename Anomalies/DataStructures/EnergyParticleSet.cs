// Developed by ColdsUx

namespace Anomalies.DataStructures;

public class EnergyParticleSet : ParticleSet
{
    public float InterpolationSpeed;
    public float EdgeOffset;
    public Func<Color> EdgeColorFunction;
    public Func<Color> CenterColorFunction;
    public override int ParticleLifetime => 50;
    public EnergyParticleSet(int setLifetime, int particleSpawnRate, Func<Color> edgeColorFunction, Func<Color> centerColorFunction, float interpolationSpeed, float edgeOffset) : base(setLifetime, particleSpawnRate)
    {
        EdgeColorFunction = edgeColorFunction ?? (() => Color.White);
        CenterColorFunction = centerColorFunction ?? (() => Color.White);
        InterpolationSpeed = interpolationSpeed;
        EdgeOffset = edgeOffset;
    }

    public EnergyParticleSet(int setLifetime, int particleSpawnRate, Color edgeColor, Color centerColor, float interpolationSpeed, float edgeOffset) : base(setLifetime, particleSpawnRate)
    {
        EdgeColorFunction = () => edgeColor;
        CenterColorFunction = () => centerColor;
        InterpolationSpeed = interpolationSpeed;
        EdgeOffset = edgeOffset;
    }

    public override Particle SpawnParticle() => new EnchantedParticle(Main.rand.NextPolarVector2(EdgeOffset * 0.6f, EdgeOffset), ParticleLifetime, 0.1f, EdgeColorFunction(), CenterColorFunction(), InterpolationSpeed, EdgeOffset);
}