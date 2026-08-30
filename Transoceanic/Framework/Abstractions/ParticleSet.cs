namespace Transoceanic.Framework.Abstractions;

/// <summary>
/// A set of particles that aren't attached to any particular position in the world, but instead an arbitrary center point defined in the draw function
/// The particles in particle sets don't count towards the general particle handler's particle cap since it doesn't process them at all.
/// You'll have to manually update and draw these sets.
/// TLDR: Groups of particles that work with relative positions instead of world positions and have to be manually updated and drawn
/// </summary>
public abstract class ParticleSet
{
    public int LocalTimer { get; internal set; }
    public int SetLifetime { get; internal set; }
    /// <summary>
    /// The probability of a particle spawning each tick. This is a float between 0 and 1, where 1 means a particle will spawn every tick and 0 means no particles will spawn.
    /// </summary>
    public float ParticleSpawnRate;
    /// <summary>
    /// The particles in the set
    /// </summary>
    public List<Particle> Particles = [];
    /// <summary>
    /// The lifetime of the particles in the set.
    /// If the particles spawned don't have this same lifetime set they may get cut off when the set dies.
    /// </summary>
    public abstract int ParticleLifetime { get; }
    /// <summary>
    /// The particle spawned by the set
    /// </summary>
    /// <returns></returns>
    public abstract Particle SpawnParticle();
    public virtual Func<Particle, int> OrderFunction { get; } = null;

    public ParticleSet(int setLifetime, float particleSpawnRate)
    {
        SetLifetime = setLifetime;
        ParticleSpawnRate = particleSpawnRate;
    }

    public virtual void Update()
    {
        // Don't perform any operations on the server. Doing so would be a waste of space as these sets are entirely based on drawcode.
        if (Main.dedServ)
            return;

        //Spawn new particles if time remains
        bool closeToDeath = LocalTimer >= SetLifetime - ParticleLifetime && SetLifetime > 0;
        if (Main.rand.NextProbability(ParticleSpawnRate) && !closeToDeath)
        {
            Particle particle = SpawnParticle();
            Particles.Add(particle);
        }

        // Update and increment the time of all particles, alongside modifying their offset based on their velocity.
        foreach (Particle particle in Particles)
        {
            particle.Center += particle.Velocity;
            particle.Timer++;
            particle.Update();
        }

        // Clear all expired particles.
        Particles.RemoveAll(particle => (particle.Timer >= particle.Lifetime && particle.AutoKillByLifeTime) || particle.Dead);
        LocalTimer++;
    }

    public virtual void DrawSet(Vector2 basePosition)
    {
        if (Main.dedServ)
            return;

        IEnumerable<Particle> orderedParticles = Particles;
        if (OrderFunction is not null)
            orderedParticles = orderedParticles.OrderBy(OrderFunction);
        else
            orderedParticles = orderedParticles.OrderBy(p => p.Timer);

        foreach (Particle particle in orderedParticles)
            ParticleHandler.DrawParticle(Main.spriteBatch, particle, basePosition);
    }
}
