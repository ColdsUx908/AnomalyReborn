namespace Anomalies.Bosses.KingSlime;

public class RubyJewelProjectile : AnomalyModProjectile
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
        for (int index = 0; index < 2; ++index)
        {
            int ruby = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GemRuby, Projectile.velocity.X, Projectile.velocity.Y, 90, new Color(), 1.2f);
            Dust dust = Main.dust[ruby];
            dust.noGravity = true;
            dust.velocity *= 0.3f;
        }

        Projectile.SpawnAfterimage(1, Color.White);
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
        for (int index1 = 0; index1 < 15; ++index1)
        {
            int ruby = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GemRuby, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, 50, new Color(), 1.2f);
            Dust dust = Main.dust[ruby];
            dust.noGravity = true;
            dust.scale *= 1.25f;
            dust.velocity *= 0.5f;
        }
    }
}

