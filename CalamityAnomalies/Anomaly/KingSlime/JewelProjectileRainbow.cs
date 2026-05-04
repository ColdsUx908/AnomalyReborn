// Developed by ColdsUx

namespace CalamityAnomalies.Anomaly.KingSlime;

public class JewelProjectileRainbow : CAModProjectile
{
    public override string LocalizationCategory => "Anomaly.KingSlime";

    //TODO: 四种材质
    public override string Texture => TOTextures.InvisibleTexturePath;

    [LoadTexture(KingSlime_Handler.AnomalyKingSlimePath + "JewelProjectileRainbow_Triangle")]
    private static Asset<Texture2D> _texture_Triangle;
    public static Texture2D Texture_Triangle => _texture_Triangle.Value;

    [LoadTexture(KingSlime_Handler.AnomalyKingSlimePath + "JewelProjectileRainbow_Star")]
    private static Asset<Texture2D> _texture_Star;
    public static Texture2D Texture_Star => _texture_Star.Value;

    [LoadTexture(KingSlime_Handler.AnomalyKingSlimePath + "JewelProjectileRainbow_Square")]
    private static Asset<Texture2D> _texture_Square;
    public static Texture2D Texture_Square => _texture_Square.Value;

    [LoadTexture(KingSlime_Handler.AnomalyKingSlimePath + "JewelProjectileRainbow_Circle")]
    private static Asset<Texture2D> _texture_Circle;
    public static Texture2D Texture_Circle => _texture_Circle.Value;

    public const int TextureType_Triangle = 0;
    public const int TextureType_Star = 1;
    public const int TextureType_Square = 2;
    public const int TextureType_Circle = 3;

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
        Projectile.rotation += 0.25f;
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
        for (int i = 0; i < 15; i++)
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
        //用ai[0]来区分四种形状

        int type = (int)Projectile.ai[0];
        Texture2D texture = type switch
        {
            TextureType_Triangle => Texture_Triangle,
            TextureType_Star => Texture_Star,
            TextureType_Square => Texture_Square,
            TextureType_Circle => Texture_Circle,
            _ => Texture_Triangle
        };
        Vector2 origin = texture.Size() / 2f;
        if (type == TextureType_Star)
            origin.Y = 41;
        else if (type == TextureType_Triangle)
            origin.Y = 47;
        float scale = type == TextureType_Circle ? 0.22f : 0.25f;
        TODrawUtils.DrawBorderTexture(Main.spriteBatch, texture, Projectile.Center - Main.screenPosition, null, Color.Lerp(Main.DiscoColor, Color.White * 0.5f, 0.1f), Projectile.rotation, origin, scale, way: 12, borderWidth: 1.5f + TOMathUtils.TimeWrappingFunction.GetTimeSin(0.4f, 1.2f, unsigned: true));
        Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.Lerp(Main.DiscoColor, Color.White * 0.5f, 0.1f), Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => new Circle(Projectile.Center, 7f * Projectile.scale).Collides(targetHitbox);
}
