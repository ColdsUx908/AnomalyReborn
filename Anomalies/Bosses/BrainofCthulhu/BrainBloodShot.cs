namespace Anomalies.Bosses.BrainofCthulhu;

public class BrainBloodShot : AnomalyModProjectile
{
    public override string LocalizationCategory => "Bosses.BrainofCthulhu";
    public virtual bool IsNautilusShot => true;
    public override string Texture => $"Terraria/Images/Projectile_{(IsNautilusShot ? ProjectileID.BloodNautilusShot : ProjectileID.BloodShot)}";
    public ref float TargetAngle => ref Projectile.ai[0];
    public ref float InitialAngle => ref Projectile.ai[1];
    public ref float Time => ref Projectile.ai[2];

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = ProjectileID.Sets.TrailCacheLength[IsNautilusShot ? ProjectileID.BloodNautilusShot : ProjectileID.BloodShot];
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.CloneDefaults(IsNautilusShot ? ProjectileID.BloodNautilusShot : ProjectileID.BloodShot);
        // Calamity skips vanilla AI for this shot; its ai slots belong to the brain's steering.
        Projectile.aiStyle = -1;
    }

    public override void AI()
    {
        if (IsNautilusShot && TargetAngle != 0)
        {
            if (Time <= 20)
                Projectile.velocity *= 0.96f;
            else
            {
                Projectile.velocity *= 1.025f;
                if (Time <= 50)
                {
                    float angle = InitialAngle.AngleLerp(TargetAngle - MathHelper.TwoPi, (Time - 20) / 30f);
                    Projectile.velocity = angle.ToRotationVector2() * Projectile.velocity.Length();
                }
            }
            Time++;
        }

        if (Projectile.localAI[0] == 0f)
        {
            SoundEngine.PlaySound(IsNautilusShot ? SoundID.Item171 : SoundID.Item17, Projectile.Center);
            Projectile.localAI[0] = 1f;
            if (!Main.dedServ)
                for (int i = 0; i < 8; i++)
                    Dust.NewDustAction(Projectile.Center, Projectile.width, Projectile.height, DustID.Blood, Projectile.velocity, dust =>
                    {
                        dust.alpha = 100;
                        dust.velocity = (Main.rand.NextFloatDirection() * MathHelper.Pi).ToRotationVector2() * 2f + Projectile.velocity.SafeNormalize(Vector2.Zero) * (IsNautilusShot ? 2f : 3f);
                        dust.scale = IsNautilusShot ? 0.9f : 1.5f;
                        dust.fadeIn = IsNautilusShot ? 1.1f : 1.7f;
                        dust.position = Projectile.Center;
                    });
        }

        Projectile.alpha = 0;
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        if (!Main.dedServ)
        {
            SpawnTrailDust(Projectile.position);
            for (int i = 1; IsNautilusShot && i < Projectile.oldPos.Length / 2 && Projectile.oldPos[i] != Vector2.Zero; i++)
                if (Main.rand.NextBool(3))
                    SpawnTrailDust(Projectile.oldPos[i]);
        }

        void SpawnTrailDust(Vector2 position)
        {
            Dust.NewDustAction(position + Projectile.Size / 2f, Projectile.width, Projectile.height, DustID.Blood, Projectile.velocity, dust =>
            {
                dust.alpha = 100;
                dust.velocity = dust.velocity / 4f + Projectile.velocity / 2f;
                dust.scale = 1.2f;
                dust.position = position + Projectile.Size / 2f + Main.rand.NextFloat() * Projectile.velocity * 2f;
            });
        }
    }

    public override Color? GetAlpha(Color lightColor) => IsNautilusShot ? new Color(200, 0, 0, Projectile.alpha) : null;

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Vector2 origin = texture.Size() / 2f;
        Vector2 offset = Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY);
        SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        // Both vanilla blood shots have type-specific drawing that AIType cannot inherit.
        int samples = IsNautilusShot ? 18 : 5;
        Color baseColor = IsNautilusShot ? Color.Lerp(lightColor, Color.White, 0.35f)
            : Color.Lerp(lightColor, Color.White, Utils.Remap(TargetAngle, 0f, 15f, 1f, 0.3f));
        for (int i = samples; i > 0; i--)
        {
            if (i >= Projectile.oldPos.Length || Projectile.oldPos[i] == Vector2.Zero)
                continue;
            Color color = IsNautilusShot ? Projectile.GetAlpha(baseColor)
                : Projectile.GetAlpha(Color.Crimson * 0.5f);
            color *= (samples - i) / (Projectile.oldPos.Length * 1.5f);
            SpriteEffects trailEffects = effects ^ (Projectile.oldSpriteDirection[i] == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            Main.EntitySpriteDraw(texture, Projectile.oldPos[i] + offset, null, color, Projectile.oldRot[i], origin, MathHelper.Lerp(Projectile.scale, IsNautilusShot ? 1f : 2.6f, i / 15f), trailEffects);
        }
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), null, Projectile.GetAlpha(baseColor), Projectile.rotation, origin, Projectile.scale, effects);
        return false;
    }

    public override void OnKill(int timeLeft)
    {
        if (Main.dedServ)
            return;
        for (int i = 0; i < 30; i++)
            Dust.NewDustAction(Projectile.Center, Projectile.width, Projectile.height, DustID.Blood, action: dust =>
            {
                dust.scale = 1.25f + Main.rand.NextFloat();
                dust.velocity *= 2f;
            });
    }
}
