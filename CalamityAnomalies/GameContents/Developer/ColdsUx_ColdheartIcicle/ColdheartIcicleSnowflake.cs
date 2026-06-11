// Developed by ColdsUx

namespace CalamityAnomalies.GameContents.Developer.ColdsUx_ColdheartIcicle;

public sealed class ColdheartIcicleSnowflake : CAModProjectile, ICAModProjectile
{
    public const int LeftTime = 360;

    public override string Texture => TOTextures.InvisibleTexturePath;

    public override string LocalizationCategory => "GameContents.Developer";

    public override void SetDefaults()
    {
        Projectile.width = 80;
        Projectile.height = 80;
        Projectile.netImportant = true;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.timeLeft = LeftTime * 2;
        Projectile.extraUpdates = 1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.ArmorPenetration = 350258;
    }

    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, Color.White.ToVector3());

        Timer1++;

        float interpolation = TOMathUtils.Interpolation.QuadraticEaseOut(Timer1 <= 20 ? Timer1 / 20f : Timer1 >= (LeftTime - 25) * 2 ? (LeftTime - Timer1) / 50f : 1f);

        Projectile.velocity.Modulus = Utils.Remap(Timer1, 0, LeftTime, 10f, 15f) * interpolation;
        Projectile.BetterChangeScale(80, 80, interpolation, Projectile.Center);
        Projectile.rotation += 0.05f * interpolation;

        if (Main.rand.NextBool(3))
            ParticleHandler.SpawnParticle(new OrbParticle(Projectile.Center, Projectile.velocity + Main.rand.NextVector2Circular(4f, 4f), Main.rand.Next(40, 75), Main.rand.NextFloat(0.35f, 0.6f), Color.White, Main.rand.NextFloat(0.8f, 1.3f), 0.8f) { Important = true });
        if (Projectile.timeLeft <= 40 && Main.rand.NextBool(3))
            ParticleHandler.SpawnParticle(new OrbParticle(Projectile.Center, Projectile.velocity + Main.rand.NextVector2Circular(5f, 5f), Main.rand.Next(40, 75), Main.rand.NextFloat(0.35f, 0.6f), Color.White, Main.rand.NextFloat(0.8f, 1.3f), 0.8f) { Important = true });
        if (Projectile.timeLeft <= 15 && Main.rand.NextBool(3))
        {
            ParticleHandler.SpawnParticle(new OrbParticle(Projectile.Center, Projectile.velocity + Main.rand.NextVector2Circular(6f, 6f), Main.rand.Next(40, 75), Main.rand.NextFloat(0.35f, 0.6f), Color.White, Main.rand.NextFloat(0.8f, 1.3f), 0.8f) { Important = true });
            ParticleHandler.SpawnParticle(new OrbParticle(Projectile.Center, Projectile.velocity + Main.rand.NextVector2Circular(7f, 7f), Main.rand.Next(40, 75), Main.rand.NextFloat(0.35f, 0.6f), Color.White, Main.rand.NextFloat(0.8f, 1.3f), 0.8f) { Important = true });
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        DrawSnowflake(Projectile.Center, Projectile.scale, Projectile.rotation);
        return false;
    }

    public static void DrawSnowflake(Vector2 center, float scale, float rotation)
    {
        Texture2D scaleTexture = CATextures.Scale2;
        Vector2 position = center - Main.screenPosition;
        Vector2 origin = new(scaleTexture.Width / 2f, scaleTexture.Height);

        Main.spriteBatch.ChangeBlendState(BlendState.Additive);
        for (int i = 0; i < 6; i++)
        {
            float angle = rotation + MathHelper.PiOver2 + TOMathUtils.PiOver3 * i;
            Main.spriteBatch.Draw(scaleTexture, position, null, Color.White, angle, origin, scale, SpriteEffects.None, 0f);
        }
        Main.spriteBatch.ChangeBlendState(BlendState.AlphaBlend);
    }

    public override void OnKill(int timeLeft)
    {
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) =>
        new Circle(Projectile.Center, 64f * Projectile.scale).Collides(targetHitbox);

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        modifiers.ForceCrit();
    }

    public override void ModifyHitNPC_DR(NPC target, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier)
    {
        baseDRModifier *= 0f;
        timedDRModifier *= 0f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        SoundEngine.PlaySound(SoundID.Item30, Projectile.Center);
        TOCombatTextUtils.ChangeHitNPCText(t => t.color = TOSharedData.CelestialColor);

        foreach (Projectile p in Projectile.ActiveProjectiles)
        {
            if (p.ModProjectile is ColdheartIcicleDream dream && dream.Target == target)
            {
                p.Timer1 = 0; //重置计时器（即延长封锁时间）
                return;
            }
        }

        Projectile.NewProjectileAction<ColdheartIcicleDream>(Projectile.GetSource_OnHit(target), target.Center, Vector2.Zero, 0, 0, Projectile.owner, p =>
        {
            p.Center = target.Center;
            p.rotation = Projectile.rotation;
            ColdheartIcicleDream modP = p.GetModProjectile<ColdheartIcicleDream>();
            modP.Target = target;
        });
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
    {
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        SoundEngine.PlaySound(SoundID.Item30, Projectile.Center);

        foreach (Projectile p in Projectile.ActiveProjectiles)
        {
            if (p.ModProjectile is ColdheartIcicleDream dream && dream.Target == target)
                return;
        }

        Projectile.NewProjectileAction<ColdheartIcicleDream>(Projectile.GetSource_OnHit(target), target.Center, Vector2.Zero, 0, 0, Projectile.owner, p =>
        {
            p.Center = target.Center;
            p.ai[1] = Projectile.rotation;
            ColdheartIcicleDream modP = p.GetModProjectile<ColdheartIcicleDream>();
            modP.Target = target;
        });
    }
}
