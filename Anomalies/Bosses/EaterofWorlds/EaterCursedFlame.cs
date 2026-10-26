namespace Anomalies.Bosses.EaterofWorlds;

// ai[0]: encounter owner + 1. Visuals mirror vanilla projectile 96, without optional-mod AI overrides.
public sealed class EaterCursedFlame : ModProjectile
{
    public override string Texture => TOAssetUtils.FormatVanillaProjectileTexturePath(ProjectileID.CursedFlameHostile);

    public override void SetDefaults()
    {
        // Vanilla Projectile.SetDefaults(ProjectileID.CursedFlameHostile): 16px hitbox, scale 1.3.
        Projectile.width = Projectile.height = 16;
        Projectile.scale = 1.3f;
        Projectile.alpha = 100;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 150;
        Projectile.aiStyle = -1;
    }

    public override void AI()
    {
        NPC owner = NPC.GetNPCFromIndex((int)Projectile.ai[0] - 1);
        if (!owner.active || owner.type != NPCID.EaterofWorldsHead
            || EaterofWorldsHead.GetInstance(owner).CurrentBehavior == EaterofWorldsHead.Behavior.Despawn)
        {
            Projectile.Kill();
            return;
        }
        Projectile.rotation += 0.3f * Projectile.direction;
        if (!Main.dedServ)
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                SoundEngine.PlaySound(SoundID.Item20, Projectile.position);
            }
            Lighting.AddLight(Projectile.Center, 0.28f, 0.8f, 0f);
            // Vanilla AI style 8 places a large dust inside the next-tick projectile rectangle.
            Dust.NewDustAction(Projectile.Center + Projectile.velocity, Projectile.width, Projectile.height,
                DustID.CursedTorch, Projectile.velocity, d =>
            {
                d.noGravity = true;
                d.alpha = 100;
                d.scale *= 3f * Projectile.scale;
            });
        }
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(BuffID.CursedInferno, 90);

    public override void OnKill(int timeLeft)
    {
        if (Main.dedServ)
            return;
        SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        for (int i = 0; i < 20; i++)
        {
            Dust.NewDustAction(Projectile.Center, Projectile.width, Projectile.height, DustID.CursedTorch,
                -Projectile.velocity * 0.2f, dust =>
                {
                    dust.alpha = 100;
                    dust.scale *= 2f * Projectile.scale;
                    dust.noGravity = true;
                    dust.velocity *= 2f;
                });
            Dust.NewDustAction(Projectile.Center, Projectile.width, Projectile.height, DustID.CursedTorch,
                -Projectile.velocity * 0.2f, dust =>
                {
                    dust.alpha = 100;
                    dust.scale *= Projectile.scale;
                    dust.velocity *= 2f;
                });
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, new Color(200, 200, 200, 25),
            Projectile.rotation, texture.Size() / 2f, Projectile.scale, SpriteEffects.None);
        return false;
    }
}
