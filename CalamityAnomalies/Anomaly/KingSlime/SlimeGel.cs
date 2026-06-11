using CalamityAnomalies.DataStructures;

namespace CalamityAnomalies.Anomaly.KingSlime;

public sealed class SlimeGel : CAModProjectile
{
    /* 数组使用约定
     * 
     * Projectile.ai
     * [0] 当前已碰撞次数
     * [1] 是否启用彩虹色（大于 0 时启用）
     */

    public override string LocalizationCategory => "Anomaly.KingSlime";

    /*
    [LoadTextureWithCalamityStyle(KingSlime_Handler.AnomalyKingSlimePath + "SlimeGel")]
    private static TextureAssetWithCalamityStyle _texture_Normal;
    public static Texture2D Texture_Normal => _texture_Normal.Value;

    [LoadTextureWithCalamityStyle(KingSlime_Handler.AnomalyKingSlimePath + "SlimeGel_Rainbow")]
    private static TextureAssetWithCalamityStyle _texture_Rainbow;
    public static Texture2D Texture_Rainbow => _texture_Rainbow.Value;
    */

    [LoadTexture(KingSlime_Handler.AnomalyKingSlimePath + "SlimeGel_Rainbow")]
    private static Asset<Texture2D> _texture_Rainbow;
    public static Texture2D Texture_Rainbow => _texture_Rainbow.Value;

    public bool IsRainbow => Projectile.ai[1] > 0f;

    public override void SetDefaults()
    {
        Projectile.width = 25;
        Projectile.height = 25;
        Projectile.aiStyle = -1;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.timeLeft = 300;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = true;
        Projectile.penetrate = -1;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        for (int i = 0; i < 10; i++)
        {
            Dust.NewDustPerfectAction(Projectile.Center, DustID.Skyware, d =>
            {
                d.velocity = Main.rand.NextPolarVector2(1f, 5f);
                d.noGravity = true;
            });
        }

        ref float bounceCount = ref Projectile.ai[0]; // 当前已碰撞次数
        bounceCount++;

        if (bounceCount < 2f)
        {
            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -oldVelocity.X * 0.4f; // 反向并衰减至 40%

            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y * 0.8f; // 反向并衰减至 80%
                Projectile.velocity.X = oldVelocity.X * 0.8f;
            }

            return false; // 弹射物不销毁，继续存在
        }
        else
        {
            for (int j = 0; j < 15; j++)
            {
                Dust.NewDustPerfectAction(Projectile.Center, DustID.Skyware, d =>
                {
                    d.velocity = Main.rand.NextPolarVector2(1f, 15f); //速度范围更大
                    d.noGravity = true;
                });
            }

            return true; // 销毁弹射物
        }
    }

    public override void AI()
    {
        Projectile.velocity.X *= 0.996f;
        Projectile.velocity.Y += 0.15f;
        if (Projectile.velocity.Y > 15f)
            Projectile.velocity.Y = 15f;

        Projectile.VelocityToRotation();
    }

    public override Color? GetAlpha(Color lightColor)
    {
        if (IsRainbow)
            return Main.DiscoColor * Projectile.Opacity;

        return null;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = /* IsRainbow ? Texture_Rainbow : */ Projectile.Texture /* Texture_Normal */;
        Color color = Projectile.GetAlpha(lightColor);
        Main.spriteBatch.DrawFromCenter_VectorScale(texture, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, new Vector2(1f, 1f) * Projectile.scale, SpriteEffects.None, 0f);
        return false;
    }
}
