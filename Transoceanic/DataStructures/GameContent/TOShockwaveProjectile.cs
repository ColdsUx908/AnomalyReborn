using Transoceanic.DataStructures.Geometry;
using Transoceanic.DataStructures.Particles;

namespace Transoceanic.DataStructures.GameContent;

public abstract class TOShockwaveProjectile : TOModProjectile
{
    public abstract bool Hostile { get; }
    public abstract List<int> NPCTypesToHit { get; }
    public abstract int LifeTime { get; }
    public abstract float FinalScale { get; }
    public virtual bool UseHDTexture => false;

    public float LifeCompletion => (float)Timer1 / LifeTime;

    public override string Texture => ParticleHandler.BaseParticleTexturePath + "HollowCircleHardEdge" + (UseHDTexture ? "HD" : "");

    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;

    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.penetrate = -1;
        Projectile.friendly = !Hostile;
        Projectile.hostile = Hostile;
        Projectile.timeLeft = LifeTime;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        Timer1++;
        Projectile.scale = MathHelper.Lerp(0f, FinalScale, TOMathUtils.Interpolation.ExponentialEaseOut(LifeCompletion, 4f));
        Projectile.Opacity = TOMathUtils.Interpolation.QuadraticEaseInOut((1f - LifeCompletion) / 0.1f);
    }

    public override bool? CanHitNPC(NPC target) => !Hostile && NPCTypesToHit is not null && NPCTypesToHit.Contains(target.type);

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => new Circle(Projectile.Center, (UseHDTexture ? PulseRing.TextureRadiusHD : PulseRing.TextureRadius) * Projectile.scale).Collides(targetHitbox);

    public override bool PreDraw(ref Color lightColor)
    {
        SpriteBatch spriteBatch = Main.spriteBatch;
        ParticleHandler.EnterDrawRegion_Additive(spriteBatch);
        spriteBatch.DrawFromCenter(Projectile.Texture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor) * Projectile.Opacity, 0f, Projectile.scale);
        ParticleHandler.ExitParticleDrawRegion(spriteBatch);
        return false;
    }
}
