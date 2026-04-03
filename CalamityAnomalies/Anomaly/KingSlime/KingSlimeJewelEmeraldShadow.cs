namespace CalamityAnomalies.Anomaly.KingSlime;

public sealed class KingSlimeJewelEmeraldShadow : CAModProjectile
{
    public override string LocalizationCategory => "Anomaly.KingSlime";
    public override string Texture => "CalamityAnomalies/Anomaly/KingSlime/KingSlimeJewelEmerald";

    public override void SetDefaults()
    {
        Projectile.width = 22;
        Projectile.height = 22;
        Projectile.penetrate = -1;
        Projectile.hostile = true;
        Projectile.timeLeft = 60;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        Timer1++;
        Lighting.AddLight(Projectile.Center, 0f, Projectile.Opacity, 0f);
        Projectile.Opacity = 0.6f * Math.Min(Math.Clamp(Timer1, 0f, 7f) / 7f, Math.Clamp(Projectile.timeLeft, 0f, 10f) / 10f);
    }

    public override bool? CanDamage() => Projectile.Opacity > 0.6f;

    public override bool PreDraw(ref Color lightColor)
    {
        Main.spriteBatch.DrawFromCenter(Projectile.Texture, Projectile.Center - Main.screenPosition, null, Color.Lerp(Main.zenithWorld ? Color.Purple : Color.White, Main.zenithWorld ? new Color(255, 175, 255) : new Color(175, 255, 175), Math.Clamp(Projectile.timeLeft, 0f, 12f) / 12f) * Projectile.Opacity, Projectile.rotation, Projectile.scale);
        return false;
    }
}