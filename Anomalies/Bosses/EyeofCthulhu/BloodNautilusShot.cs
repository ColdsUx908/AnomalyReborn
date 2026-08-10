namespace Anomalies.Bosses.EyeofCthulhu;

public sealed class BloodNautilusShot : AnomalyProjectileBehavior<BloodShot>
{
    public override int ApplyingType => ProjectileID.BloodNautilusShot;

    public override bool AllowCalamityLogic(CalamityLogicType_ProjectileBehavior type) => type switch
    {
        CalamityLogicType_ProjectileBehavior.PreAI => false,
        CalamityLogicType_ProjectileBehavior.GetAlpha => false,
        _ => true,
    };

    public override bool PreAI()
    {
        if (Projectile.localAI[0] == 0f)
        {
            SoundEngine.PlaySound(SoundID.Item171, Projectile.Center);
            Projectile.localAI[0] = 1f;
            for (int i = 0; i < 8; i++)
            {
                Dust blood1 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Projectile.velocity.X, Projectile.velocity.Y, 100);
                blood1.velocity = (Main.rand.NextFloatDirection() * MathHelper.Pi).ToRotationVector2() * 2f + Projectile.velocity.SafeNormalize(Vector2.Zero) * 2f;
                blood1.scale = 0.9f;
                blood1.fadeIn = 1.1f;
                blood1.position = Projectile.Center;
            }
        }

        Projectile.alpha = 0;

        Dust blood2 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Projectile.velocity.X, Projectile.velocity.Y, 100);
        blood2.velocity = blood2.velocity / 4f + Projectile.velocity / 2f;
        blood2.scale = 1.2f;
        blood2.position = Projectile.Center + Main.rand.NextFloat() * Projectile.velocity * 2f;

        int trailLength = Projectile.oldPos.Length / 2;
        for (int j = 1; j < trailLength && !(Projectile.oldPos[j] == Vector2.Zero); j++)
        {
            if (Main.rand.NextBool(3))
            {
                Dust blood3 = Dust.NewDustDirect(Projectile.oldPos[j], Projectile.width, Projectile.height, DustID.Blood, Projectile.velocity.X, Projectile.velocity.Y, 100);
                blood3.velocity = blood3.velocity / 4f + Projectile.velocity / 2f;
                blood3.scale = 1.2f;
                blood3.position = Projectile.oldPos[j] + Projectile.Size / 2f + Main.rand.NextFloat() * Projectile.velocity * 2f;
            }
        }

        Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.PiOver2;

        return false;
    }

    public override Color? GetAlpha(Color lightColor) => new Color(200, 0, 0, Projectile.alpha);
}
