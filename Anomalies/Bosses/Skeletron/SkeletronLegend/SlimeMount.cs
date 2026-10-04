namespace Anomalies.Bosses.Skeletron.SkeletronLegend;

public sealed class SlimeMount : AnomalyModProjectile
{
    /* 数组使用约定
     * 
     * Projectile.ai
     * [0] 预期飞行时间（单位：帧）
     * [1] 内部阶段计数器
     */

    public override string LocalizationCategory => "Bosses.Skeletron.Legend";

    public override string Texture => TOTextures.InvisibleTexturePath;

    public override void SetStaticDefaults() => Main.projFrames[Projectile.type] = 4;

    public override void SetDefaults()
    {
        Projectile.width = 54;
        Projectile.height = 34;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.timeLeft = 600;
    }

    public override void AI()
    {
        Texture2D texture = TextureAssets.SlimeMount.Value;
        Rectangle frame = texture.Frame(1, 4, 0, Projectile.frame);
        Projectile.SpawnAfterimage(new AfterimageParticle(texture, frame, Projectile.Center, 3, Projectile.rotation, Projectile.scale, Color.White));

        Lighting.AddLight(Projectile.Center, (LegendShockwave.SlimeColor * 1.5f).ToVector3());

        Timer1++;
        if (Projectile.ai[1] == 0f)
        {
            if (Timer1 > Projectile.ai[0])
            {
                Projectile.ai[1] = 1f;
                Projectile.velocity = Vector2.Zero;
            }

            Projectile.frame = 3;
        }
        else
        {
            Projectile.velocity.X = 0f;
            Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + 0.75f, 25f);

            Projectile.frame = 1;
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.SlimeMount.Value;
        Rectangle frame = texture.Frame(1, 4, 0, Projectile.frame);
        Main.spriteBatch.DrawFromCenter(texture, Projectile.Center - new Vector2(0f, 6f) - Main.screenPosition, frame, lightColor, Projectile.rotation, Projectile.scale);

        return false;
    }

    public override bool CanHitPlayer(Player target) => Projectile.ai[1] > 0f;

    public override void OnHitPlayer(Player target, Player.HurtInfo info) => Projectile.velocity *= -0.8f;
}
