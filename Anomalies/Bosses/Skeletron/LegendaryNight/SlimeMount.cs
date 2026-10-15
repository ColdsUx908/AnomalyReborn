namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed class SlimeMount : AnomalyModProjectile
{
    /* 数组使用约定
     * 
     * Projectile.ai
     * [0] 预期飞行时间（单位：帧）
     * [1] 内部阶段计数器
     * [2] 目标索引（1-based，即 1 表示玩家索引 0，2 表示玩家索引 1，以此类推）
     */

    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;

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

        Lighting.AddLight(Projectile.Center, (LegendaryNightHandler.SlimeColor * 1.5f).ToVector3());

        Timer1++;
        if (Projectile.ai[1] == 0f)
        {
            if (Timer1 > Projectile.ai[0])
            {
                Projectile.ai[1] = 1f;
                Projectile.velocity = Vector2.Zero;
            }

            Projectile.frame = 3;

            Projectile.tileCollide = false;
        }
        else
        {
            Projectile.velocity.X = 0f;
            Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + 0.75f, 25f);

            Projectile.frame = 1;

            if (Player.TryGetPlayerFromIndex((int)Projectile.ai[2] - 1, out Player target) && Projectile.position.Y > target.Bottom.Y)
                Projectile.tileCollide = true;
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

    public override void OnKill(int timeLeft)
    {
        SpawnSlimeMountDust(Projectile.Center, 30, Projectile.velocity * 0.8f);
        SpawnSlimeMountDust(Projectile.Center, 30, Vector2.Zero);
    }

    public static void SpawnSlimeMountDust(Vector2 position, int amount, Vector2 extraVelocity, Action<Dust> action = null)
    {
        if (!TOSharedData.NotClient)
            return;

        Color newColor = Color.Transparent;

        for (int i = 0; i < amount; i++)
        {
            Dust.NewDustAction(position, 32, 20, DustID.BlueFairy, action: d =>
            {
                d.color = newColor;
                d.scale += Main.rand.NextFloat(-0.10f, 0.2f);
                d.velocity *= 0.5f;
                d.velocity += extraVelocity;
                if (Main.rand.NextBool(2))
                {
                    d.scale *= 1.3f;
                    d.noGravity = true;
                }
                else
                    d.velocity *= 0.7f;

                action?.Invoke(d);
            });
        }
    }
}
