namespace CalamityAnomalies.Anomaly.KingSlime;

public class JewelProjectileRainbow : CAModProjectile
{
    public override string LocalizationCategory => "Anomaly.KingSlime";

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 2;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
    }

    public override void SetDefaults()
    {
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.penetrate = -1;
        Projectile.hostile = true;
    }

    public override void AI()
    {
        Projectile.rotation += 0.3f * Projectile.direction;
        for (int index = 0; index < 2; ++index)
        {
            int rainbow = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, KingSlime_Handler.GetRandomDustID(), Projectile.velocity.X, Projectile.velocity.Y, 90, default, 1.2f);
            Dust dust = Main.dust[rainbow];
            dust.noGravity = true;
            dust.velocity *= 0.3f;
        }

        Projectile.SpawnAfterimage(5, Projectile.GetAlpha(Lighting.GetColor(Projectile.Center.WorldCoordinateSafe)));
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
        for (int index1 = 0; index1 < 15; ++index1)
        {
            int rainbow = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, KingSlime_Handler.GetRandomDustID(), Projectile.oldVelocity.X, Projectile.oldVelocity.Y, 50, default, 1.2f);
            Dust dust = Main.dust[rainbow];
            dust.noGravity = true;
            dust.scale *= 1.25f;
            dust.velocity *= 0.5f;
        }
    }

    public override Color? GetAlpha(Color lightColor) => Main.DiscoColor;

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = Projectile.Texture;
        TODrawUtils.DrawBorderTextureFromCenter(Main.spriteBatch, texture, Projectile.Center - Main.screenPosition, null, Color.Lerp(Main.DiscoColor, Color.White * 0.5f, 0.1f), Projectile.rotation, borderWidth: 1f + TOMathUtils.TimeWrappingFunction.GetTimeSin(0.2f, 1.2f, unsigned: true));
        Main.spriteBatch.DrawFromCenter(texture, Projectile.Center - Main.screenPosition, null, Color.Lerp(Main.DiscoColor, Color.White * 0.5f, 0.1f), Projectile.rotation);
        return false;
    }
}
