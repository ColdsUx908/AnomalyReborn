using Terraria.Graphics.CameraModifiers;

namespace Anomalies.SpecialContents.Contributor.MeguminExplosion;

public sealed class MeguminBlast : AnomalyModProjectile
{
    public override bool IsLoadingEnabled(Mod mod) => AnomalySpecialContentToggler.MeguminExplosion;

    private Vector2 _LastSyncedCenter;
    public const int WarmupTime = 18;
    public const int FadeDuration = 18;
    public const int ManaInterval = 10;
    public const int ManaPerPulse = 12;
    public float Time => Projectile.ai[0];
    public bool Stopping => Projectile.ai[1] > 0f;
    public float Opacity => MathHelper.Clamp(Time / 3f, 0f, 1f)
        * MathHelper.Clamp(1f - Projectile.ai[1] / FadeDuration, 0f, 1f);
    public float BeamPower => MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(Time / WarmupTime, 0f, 1f));
    public Vector2 BeamStart => new(Projectile.Center.X, 16f);
    public Vector2 BeamEnd => new(Projectile.Center.X, Main.maxTilesY * 16f - 16f);
    public override string Texture => "Terraria/Images/Projectile_0";
    public override string LocalizationCategory => "SpecialContents.Contributor";

    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 4000;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 2;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 15;
    }

    public override bool ShouldUpdatePosition() => false;
    public override bool? CanDamage() => !Stopping && Time >= WarmupTime ? null : false;

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        return RotatedRectangle.FromLineSegment(BeamStart, BeamEnd, 150f * BeamPower).Collides(targetHitbox);
    }

    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        if (!player.active || player.dead)
        {
            Projectile.Kill();
            return;
        }

        // Only the owner reads the mouse and pays mana; normal projectile position packets synchronize the column.
        if (Projectile.owner == Main.myPlayer && !Stopping)
        {
            if (!player.controlUseItem || player.noItems || player.CCed || player.HeldItem.type != ModContent.ItemType<MeguminExplosion>())
                StopChannel(player);
            else
            {
                Vector2 mouse = Main.MouseWorld;
                if (player.gravDir == -1f)
                    mouse.Y = Main.screenPosition.Y + Main.screenHeight - Main.mouseY;
                Vector2 target = MeguminExplosion.ClampTarget(player, mouse);
                Projectile.Center = target;
                if ((Time % 4f == 0f && Vector2.DistanceSquared(_LastSyncedCenter, target) > 1f) || Time % 15f == 0f)
                {
                    _LastSyncedCenter = target;
                    Projectile.netUpdate = true;
                }
                if (Time >= WarmupTime && (Time - WarmupTime) % ManaInterval == 0f
                    && !player.CheckMana(player.HeldItem, ManaPerPulse, true))
                    StopChannel(player);
            }
        }

        if (!Stopping)
        {
            player.channel = true;
            player.heldProj = Projectile.whoAmI;
            Vector2 aim = (Projectile.Center - player.MountedCenter).SafeNormalize(Vector2.UnitX * player.direction);
            player.ChangeDir(aim.X >= 0f ? 1 : -1);
            player.itemRotation = (aim * player.direction).ToRotation();
            player.itemTime = player.itemAnimation = 2;
            player.manaRegenDelay = Math.Max(player.manaRegenDelay, 30);
        }
        else if (++Projectile.ai[1] >= FadeDuration)
        {
            Projectile.Kill();
            return;
        }
        Projectile.timeLeft = 2;

        if (!Main.dedServ)
        {
            if (Time == 0f)
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.6f, Pitch = -0.6f }, Projectile.Center);
            if (Time == WarmupTime && !Stopping)
            {
                SoundEngine.PlaySound(SoundID.Zombie104 with { Volume = 0.65f, Pitch = -0.45f }, Projectile.Center);
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, Vector2.UnitX,
                    5f, 4f, 20, 2000f, FullName));
            }
            GetVisibleSegment(out Vector2 top, out Vector2 bottom);
            DelegateMethods.v3_1 = new Vector3(1.3f, 0.5f, 0.12f) * (BeamPower * Opacity);
            Utils.PlotTileLine(Vector2.Clamp(top, BeamStart, BeamEnd), Vector2.Clamp(bottom, BeamStart, BeamEnd),
                180f * BeamPower, DelegateMethods.CastLight);
        }
        Projectile.ai[0]++;
    }

    private void StopChannel(Player player)
    {
        Projectile.ai[1] = 1f;
        Projectile.netUpdate = true;
        player.channel = false;
    }

    public override void OnKill(int timeLeft)
    {
        if (Projectile.owner == Main.myPlayer && Main.player[Projectile.owner] is Player { active: true, dead: false } player)
            player.AddBuff(ModContent.BuffType<MeguminExhaustion>(), 480);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.OnFire3, 300);

    private void GetVisibleSegment(out Vector2 top, out Vector2 bottom)
    {
        // Invert the world camera, not UI scaling. Keep both caps outside the viewport at every zoom level.
        Viewport viewport = Main.instance.GraphicsDevice.Viewport;
        Matrix inverse = Matrix.Invert(Main.GameViewMatrix.TransformationMatrix);
        Vector2 a = Vector2.Transform(Vector2.Zero, inverse);
        Vector2 b = Vector2.Transform(new Vector2(viewport.Width, 0f), inverse);
        Vector2 c = Vector2.Transform(new Vector2(0f, viewport.Height), inverse);
        Vector2 d = Vector2.Transform(new Vector2(viewport.Width, viewport.Height), inverse);
        float minY = Math.Min(Math.Min(a.Y, b.Y), Math.Min(c.Y, d.Y)) + Main.screenPosition.Y - 256f;
        float maxY = Math.Max(Math.Max(a.Y, b.Y), Math.Max(c.Y, d.Y)) + Main.screenPosition.Y + 256f;
        top = new Vector2(Projectile.Center.X, minY);
        bottom = new Vector2(Projectile.Center.X, maxY);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        GetVisibleSegment(out Vector2 top, out Vector2 bottom);
        ExplosionDrawing.Laser(Projectile.Center, top, bottom, Time, BeamPower, Opacity);
        return false;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs,
        List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => overPlayers.Add(index);
}
