namespace Anomalies.Bosses.BrainofCthulhu;

public class IchorShower : AnomalyModProjectile
{
    public override string LocalizationCategory => "Bosses.BrainofCthulhu";
    public override string Texture => TOTextures.InvisibleTexturePath;

    public override void SetDefaults()
    {
        Projectile.width = 8;
        Projectile.height = 8;
        Projectile.penetrate = -1;
        Projectile.Opacity = 1f;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 480;
        Projectile.damage = 10;
        Projectile.scale = 1;
        Projectile.hostile = true;
        Projectile.extraUpdates = 1;
    }

    public override void AI()
    {
        for (int i = 0; i < 3; i++)
            Dust.NewDustPerfectAction(Projectile.Center + Main.rand.NextVector2Circular(4, 4), DustID.Ichor, action: dust =>
            {
                dust.velocity = Vector2.Zero;
                dust.noGravity = true;
            });
        if (Main.rand.NextBool(8))
            Dust.NewDustPerfectAction(Projectile.Center + Main.rand.NextVector2Circular(4, 4), DustID.GoldFlame, action: dust =>
            {
                dust.velocity = Vector2.UnitX * Projectile.velocity / 10f;
                dust.scale *= 0.75f;
            });
        Projectile.velocity.Y += 0.075f;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        target.AddBuff(BuffID.Ichor, 600);
    }
}
