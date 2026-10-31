namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>史莱姆雨；贴图沿用Fargo史莱姆雨武器发射的四帧MutantSlimeBall_2，资源由本模块持有。</summary>
public sealed class LegendSlimeRain : AnomalyModProjectile
{
    public override string Texture => LegendaryNightHandler.LegendaryNightEntityPath + "LegendSlimeRain";
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;
    [LoadTexture(LegendaryNightHandler.LegendaryNightEntityPath + "LegendSlimeRain")]
    private static Asset<Texture2D> _RainTexture;
    public static Texture2D RainTexture => _RainTexture.Value;

    public override void SetStaticDefaults() => Main.projFrames[Type] = 4;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 14;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = SkeletronLegendHead.SlimeAvatarData.RainLifetime;
        Projectile.netImportant = true;
    }

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
        Projectile.frame = ++Projectile.frameCounter / 6 % 4;
        Lighting.AddLight(Projectile.Center, LegendaryNightHandler.SlimeColor.ToVector3() * 0.5f);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Main.spriteBatch.DrawFromCenter(RainTexture, Projectile.Center - Main.screenPosition,
            RainTexture.Frame(1, 4, 0, Projectile.frame), Color.White, Projectile.rotation, Projectile.scale);
        return false;
    }
}
