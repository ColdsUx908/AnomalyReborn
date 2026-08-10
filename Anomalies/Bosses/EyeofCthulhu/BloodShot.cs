namespace Anomalies.Bosses.EyeofCthulhu;

public sealed class BloodShot : AnomalyProjectileBehavior<BloodShot>
{
    public override int ApplyingType => ProjectileID.BloodShot;

    public override bool AllowCalamityLogic(CalamityLogicType_ProjectileBehavior type) => type switch
    {
        CalamityLogicType_ProjectileBehavior.PreAI => false,
        _ => true,
    };

    public override bool PreAI()
    {
        if (Projectile.localAI[0] == 0f)
        {
            SoundEngine.PlaySound(SoundID.Item17, Projectile.Center);
            Projectile.localAI[0] = 1f;
            for (int i = 0; i < 8; i++)
            {
                Dust blood1 = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Projectile.velocity.X, Projectile.velocity.Y, 100)];
                blood1.velocity = (Main.rand.NextFloatDirection() * (float)Math.PI).ToRotationVector2() * 2f + Projectile.velocity.SafeNormalize(Vector2.Zero) * 3f;
                blood1.scale = 1.5f;
                blood1.fadeIn = 1.7f;
                blood1.position = Projectile.Center;
            }
        }

        Projectile.alpha = 0;

        Dust blood2 = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Projectile.velocity.X, Projectile.velocity.Y, 100)];
        blood2.velocity = blood2.velocity / 4f + Projectile.velocity / 2f;
        blood2.scale = 1.2f;
        blood2.position = Projectile.Center + Main.rand.NextFloat() * Projectile.velocity * 2f;

        Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.PiOver2;

        return false;
    }
}
