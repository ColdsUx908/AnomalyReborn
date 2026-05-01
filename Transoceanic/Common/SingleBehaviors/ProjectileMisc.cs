// Developed by ColdsUx

using Transoceanic.DataStructures.Particles;

namespace Transoceanic.Common.SingleBehaviors;

public sealed class ProjectileMisc : TOGlobalProjectileBehavior
{
    public override decimal Priority => 500m;

    public override bool PreAI(Projectile projectile)
    {
        return true;
    }

    public override void PostAI(Projectile projectile)
    {
        if (projectile.AlwaysRotating)
            projectile.VelocityToRotation(projectile.RotationOffset);

        TOGlobalProjectile ocean = projectile.Ocean;
        foreach (AfterimageParticle afterimage in ocean.Afterimages)
            ParticleHandler.UpdateParticle(afterimage);
        ocean.Afterimages.RemoveAll(a => a.Timer >= a.Lifetime);
    }

    public override bool PreDraw(Projectile projectile, ref Color lightColor)
    {
        SpriteBatch spriteBatch = Main.spriteBatch;
        foreach (AfterimageParticle afterimage in projectile.Ocean.Afterimages)
            afterimage.Draw(spriteBatch);

        return true;
    }
}
