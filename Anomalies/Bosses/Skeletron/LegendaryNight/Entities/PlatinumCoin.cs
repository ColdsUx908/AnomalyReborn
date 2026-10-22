namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed class PlatinumCoin : AnomalyModProjectile
{
    public static readonly SoundStyle TossSound = SoundID.Item11 with { Volume = 0.75f };
    public static readonly SoundStyle UnlimitedTossSound = TossSound with { MaxInstances = 0 };

    // ai[0]：目标玩家索引（1-based，0 表示未指定目标）。ai[1]：重力加速度，0 保持匀速。
    private const int FrameCount = 8;
    private const int TicksPerFrame = 6;
    private const int MidasDuration = 300;

    // 原版铂金币物品在世界中的动画贴图，区别于背包内的静态 Item_74。
    public override string Texture => "Terraria/Images/Coin_3";

    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;

    public override void SetStaticDefaults() => Main.projFrames[Type] = FrameCount;

    public override void SetDefaults()
    {
        Projectile.width = 12;
        Projectile.height = 12;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 450;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, LegendaryNightHandler.PlatinumColor.ToVector3() * 0.6f);

        if (Projectile.ai[1] > 0f)
            Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + Projectile.ai[1], 20f);

        if (Player.TryGetPlayerFromIndex((int)Projectile.ai[0] - 1, out Player target) && Projectile.Top.Y > target.Bottom.Y)
            Projectile.tileCollide = true;

        if (++Projectile.frameCounter >= TicksPerFrame)
        {
            Projectile.frameCounter = 0;
            Projectile.frame = (Projectile.frame + 1) % FrameCount;
        }

        Projectile.VelocityToRotation(MathHelper.PiOver2);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(BuffID.Midas, MidasDuration);

    public override void OnKill(int timeLeft)
    {
        if (Main.dedServ)
            return;

        SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
        for (int i = 0; i < 15; i++)
        {
            Dust.NewDustAction(Projectile.Center, Projectile.width, Projectile.height, DustID.PlatinumCoin, Projectile.oldVelocity, d =>
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
        Rectangle frame = texture.Frame(1, FrameCount, 0, Projectile.frame);
        // 与原版 DrawAnimationVertical 一致，裁掉帧底部的两像素间隔。
        frame.Height -= 2;
        Main.spriteBatch.DrawFromCenter(texture, Projectile.Center - Main.screenPosition, frame, lightColor * Projectile.Opacity, Projectile.rotation, Projectile.scale, SpriteEffects.None, 0f);
        return false;
    }
}
