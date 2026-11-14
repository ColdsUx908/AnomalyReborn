namespace Anomalies.SpecialContents.Contributor.MeguminExplosion;

public sealed class MeguminCasting : AnomalyModProjectile
{
    public override bool IsLoadingEnabled(Mod mod) => AnomalySpecialContentToggler.MeguminExplosion;

    private Vector2 _LastSyncedTarget;
    public float Time => Projectile.ai[0];
    public float Charge => MathHelper.Clamp(Time / MeguminExplosion.ChargeDuration, 0f, 1f);
    public Vector2 Target => new(Projectile.ai[1], Projectile.ai[2]);
    public override string Texture => "Terraria/Images/Projectile_0";
    public override string LocalizationCategory => "SpecialContents.Contributor";

    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2200;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 16;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 2;
        Projectile.DamageType = DamageClass.Magic;
    }

    public override bool ShouldUpdatePosition() => false;
    public override bool? CanDamage() => false;

    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        if (!player.active || player.dead || player.noItems || player.CCed || !player.channel
            || player.HeldItem.type != ModContent.ItemType<MeguminExplosion>())
        {
            Projectile.Kill();
            return;
        }

        // Keep the ritual focus under the mouse, including the transition into the sustained beam.
        if (Projectile.owner == Main.myPlayer)
        {
            Vector2 mouse = Main.MouseWorld;
            if (player.gravDir == -1f)
                mouse.Y = Main.screenPosition.Y + Main.screenHeight - Main.mouseY;
            Vector2 target = MeguminExplosion.ClampTarget(player, mouse);
            Projectile.ai[1] = target.X;
            Projectile.ai[2] = target.Y;
            if (Time % 6f == 0f && Vector2.DistanceSquared(target, _LastSyncedTarget) > 16f)
            {
                _LastSyncedTarget = target;
                Projectile.netUpdate = true;
            }
        }

        Projectile.velocity = (Target - player.MountedCenter).SafeNormalize(Vector2.UnitX * player.direction);
        Projectile.Center = player.RotatedRelativePoint(player.MountedCenter, true) + Projectile.velocity * 62f;
        Projectile.timeLeft = 2;
        player.ChangeDir(Projectile.velocity.X >= 0 ? 1 : -1);
        player.itemRotation = (Projectile.velocity * player.direction).ToRotation();
        player.heldProj = Projectile.whoAmI;
        player.itemTime = player.itemAnimation = 2;
        player.manaRegenDelay = Math.Max(player.manaRegenDelay, 30);

        if (!Main.dedServ)
        {
            Lighting.AddLight(Projectile.Center, 0.7f * Charge, 0.12f * Charge, 0.45f * Charge);
            if (Time == 45f || Time == 105f || Time == 150f)
                SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.45f, Pitch = Charge * 0.5f - 0.2f }, Projectile.Center);
            for (int i = 0; i < 2; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(45f, 125f) * (0.5f + Charge);
                Color color = i == 0 ? Color.Cyan : Color.Magenta;
                Dust.NewDustPerfectAction(Projectile.Center + offset, DustID.PortalBoltTrail, dust =>
                {
                    dust.velocity = -offset * 0.055f;
                    dust.alpha = 80;
                    dust.color = color;
                    dust.scale = 0.65f + Charge * 0.65f;
                    dust.noGravity = true;
                });
            }
        }

        Projectile.ai[0]++;
        if (Time >= MeguminExplosion.ChargeDuration)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectileAction(Projectile.GetSource_FromThis(), Target, Vector2.Zero,
                    ModContent.ProjectileType<MeguminBlast>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
            Projectile.Kill();
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        float charge = Charge;
        float time = Time / 60f;
        Color amber = new(255, 120, 35);
        Color rose = new(255, 65, 180);
        float radius = MathHelper.Lerp(70f, 160f, charge);
        ExplosionDrawing.Circle(Projectile.Center, new Vector2(radius), time, charge, -time, rose);
        ExplosionDrawing.Circle(Projectile.Center, new Vector2(radius * 1.16f), -time, charge * 0.6f, time * 0.3f, Color.Cyan);
        ExplosionDrawing.Glow(Projectile.Center, new Vector2(65f + charge * 70f), rose * (charge * 0.7f));

        for (int i = 0; i < 4; i++)
        {
            float reveal = MathHelper.Clamp((Time - i * 28f) / 50f, 0f, 1f);
            float size = (480f + i * 90f) * (0.4f + charge * 0.6f);
            Vector2 center = Target - Vector2.UnitY * (18f + i * 112f);
            ExplosionDrawing.Circle(center, new Vector2(size, size * 0.3f), time * (i % 2 == 0 ? 1 : -1),
                reveal * 0.9f, 0f, i % 2 == 0 ? amber : rose);
        }

        // Helical light filaments connect the stacked circles to the point of impact.
        using (Main.spriteBatch.Scope(SpriteSortMode.Deferred, BlendState.Additive))
        {
            for (int strand = 0; strand < 2; strand++)
            {
                Vector2 previous = Target;
                for (int i = 1; i <= 64; i++)
                {
                    float t = i / 64f;
                    float angle = t * MathHelper.TwoPi * 2f - time * 2.5f + strand * MathHelper.Pi;
                    Vector2 point = Target + new Vector2(MathF.Sin(angle) * (140f + t * 150f) * charge, -t * 390f);
                    ExplosionDrawing.Line(previous, point, (1.4f + charge * 2.3f), amber * (charge * 0.55f));
                    previous = point;
                }
            }
        }
        return false;
    }
}
