namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 三种外观的直线史莱姆雨。
/// </summary>
/// <remarks>
/// <para>ai[0]：外观变体，0默认、1绿色、2圆形；ai[1..2]：未使用。</para>
/// <para>localAI：未使用；框架 Timer1..5：未使用；无实体专属 ExtraAI。</para>
/// </remarks>
public sealed class LegendSlimeRain : AnomalyModProjectile
{
    public override string Texture => LegendaryNightHandler.LegendaryNightPath + "Attacks/Slime/LegendSlimeRain";
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;
    /// <summary>雨幕贴图。</summary>
    [LoadTexture(LegendaryNightHandler.LegendaryNightPath + "Attacks/Slime/LegendSlimeRain")]
    private static Asset<Texture2D> _RainTexture;
    /// <summary>雨幕绿色变体贴图。</summary>
    [LoadTexture(LegendaryNightHandler.LegendaryNightPath + "Attacks/Slime/LegendSlimeRain_Green")]
    private static Asset<Texture2D> _RainTextureGreen;
    /// <summary>雨幕圆形变体贴图。</summary>
    [LoadTexture(LegendaryNightHandler.LegendaryNightPath + "Attacks/Slime/LegendSlimeRain_Round")]
    private static Asset<Texture2D> _RainTextureRound;

    /// <summary>变体数量。</summary>
    public const int VariantCount = 3;
    public static Texture2D GetRainTexture(int variant) => variant switch
    {
        1 => _RainTextureGreen.Value,
        2 => _RainTextureRound.Value,
        _ => _RainTexture.Value
    };

    /// <summary>拖尾长度。</summary>
    private const int TrailLength = 8;

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 4;
        ProjectileID.Sets.TrailCacheLength[Type] = TrailLength;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 14;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = SkeletronLegendHead.Mechanic3Parameters.RainLifetime;
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
        Texture2D texture = GetRainTexture((int)Projectile.ai[0]);
        Rectangle frame = texture.Frame(1, 4, 0, Projectile.frame);
        for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
        {
            if (Projectile.oldPos[i] == Vector2.Zero)
                continue;
            float strength = 1f - i / (float)Projectile.oldPos.Length;
            Main.spriteBatch.DrawFromCenter(texture,
                Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition, frame,
                Color.LightSkyBlue * (0.5f * strength), Projectile.oldRot[i], Projectile.scale);
        }
        Main.spriteBatch.DrawFromCenter(texture, Projectile.Center - Main.screenPosition,
            frame, Color.White, Projectile.rotation, Projectile.scale);
        return false;
    }
}