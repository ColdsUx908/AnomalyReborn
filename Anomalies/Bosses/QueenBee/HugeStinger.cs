namespace Anomalies.Bosses.QueenBee;

public sealed class HugeStinger : AnomalyModProjectile
{
    public override string LocalizationCategory => "Bosses.QueenBee";

    public override void SetDefaults()
    {
        Projectile.width = 25;
        Projectile.height = 25;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 150;
    }

    public override void AI()
    {
        if (Projectile.Center.Y > Projectile.ai[1] + 150f)
        {
            Projectile.tileCollide = true;
            if (Projectile.timeLeft > 10)
                Projectile.timeLeft = 10;
        }
        else
            Projectile.tileCollide = false;

        if (Projectile.Center.Y > Projectile.ai[1] - 50f)
            Projectile.tileCollide = true;

        Projectile.VelocityToRotation(MathHelper.PiOver2);
    }

    public override void OnKill(int timeLeft)
    {
        if (Projectile.IsOnOwnerClient)
        {
            int amount = Main.rand.Next(5, 13);
            for (int i = 0; i < amount; i++)
            {
                Vector2 vector = Main.rand.NextPolarVector2(8f, 11f);
                Projectile.NewProjectileAction(SourceAI, Projectile.Center, vector, ProjectileID.QueenBeeStinger, QueenBee.BeeDamage, 0f, action: p =>
                {
                    p.timeLeft = 300;
                    //p.tileCollide = false;
                });
            }
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = Projectile.Texture;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Vector2 origin = texture.Size() * 0.5f;
        Color mainColor = Projectile.GetAlpha(lightColor);
        SpriteEffects effects = SpriteEffects.None;

        const float shadowOpacity = 0.3f;
        Color shadowColor = new Color(100, 100, 100, 255) * shadowOpacity;

        float shadowScale = Projectile.scale * 1.5f;
        Vector2 offsetBase = new PolarVector2(4f * shadowScale, Projectile.rotation);

        // 绘制沿速度方向拖尾的阴影
        for (int i = 0; i < 6; i++)
        {
            Vector2 trailOffset = -Projectile.velocity * i * 0.85f;
            Main.EntitySpriteDraw(texture, drawPosition + trailOffset, null, shadowColor * (0.9f - i * 0.07f), Projectile.rotation, origin, shadowScale * (1f - i * 0.05f), effects);
        }

        // 绘制环绕的阴影（3个方向，相隔90度）
        for (int angle = 0; angle < 3; angle++)
        {
            Vector2 orbOffset = offsetBase.RotatedBy(angle * MathHelper.PiOver2);
            Main.EntitySpriteDraw(texture, drawPosition + orbOffset, null, shadowColor * 0.9f, Projectile.rotation, origin, shadowScale, effects);
        }

        // 绘制主体
        Main.EntitySpriteDraw(texture, drawPosition, null, mainColor, Projectile.rotation, origin, Projectile.scale, effects);
        return false;
    }
}

