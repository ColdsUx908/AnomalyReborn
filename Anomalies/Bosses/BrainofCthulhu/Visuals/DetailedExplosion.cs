namespace Anomalies.Bosses.BrainofCthulhu;

public sealed class DetailedExplosion : TexturePulseParticle
{
    public DetailedExplosion(Vector2 position, Vector2 velocity, Color color, Vector2 squish, float rotation, float originalScale, float finalScale, int lifeTime, bool additive = true) : base(position, velocity, color, "Anomalies/Bosses/BrainofCthulhu/Assets/Particles/DetailedExplosion", squish, rotation, originalScale, finalScale, lifeTime, additive)
    {
    }
}
