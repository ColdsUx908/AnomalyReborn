namespace Anomalies.Bosses.KingSlime;

public class RubyBullet : AnomalyModProjectile
{
    public override string LocalizationCategory => "Bosses.KingSlime";

    public override void SetDefaults()
    {
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.penetrate = -1;
        Projectile.hostile = true;
        Projectile.timeLeft = 450;
    }

    public override void AI()
    {
        Projectile.rotation += 0.3f * Projectile.direction;

        for (int i = 0; i < 2; i++)
        {
            Dust.NewDustAction(Projectile.Center, Projectile.width, Projectile.height, DustID.GemRuby, Projectile.velocity, d =>
            {
                d.alpha = 90;
                d.scale = 1.2f;
                d.noGravity = true;
                d.velocity *= 0.3f;
            });
        }

        Projectile.SpawnAfterimage(new AfterimageParticle(Projectile.Texture, null, Projectile.Center, 2, Projectile.rotation, Projectile.scale * 0.22f, Color.Red, Projectile.Opacity * 0.9f));
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
        for (int i = 0; i < 15; i++)
        {
            Dust.NewDustAction(Projectile.Center, Projectile.width, Projectile.height, DustID.GemRuby, Projectile.oldVelocity, d =>
            {
                d.alpha = 50;
                d.scale = 1.5f;
                d.noGravity = true;
                d.velocity *= 0.5f;
            });
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = Projectile.Texture;

        float scale = Projectile.scale * 0.22f;
        TODrawUtils.DrawBorderTextureFromCenter(Main.spriteBatch, texture, Projectile.Center - Main.screenPosition, null, Color.Red, Projectile.rotation, scale, way: 12, borderWidth: 1.5f + TOMathUtils.TimeWrappingFunction.GetTimeSin(0.4f, 1.2f, unsigned: true));
        Main.spriteBatch.DrawFromCenter(texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, scale, SpriteEffects.None, 0f);

        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => new Circle(Projectile.Center, 8f * Projectile.scale).Collides(targetHitbox);
}

