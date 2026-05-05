// Developed by ColdsUx

using CalamityMod;

namespace CalamityAnomalies.GameContents.Contributor.Mocangran_ImmaculateWhite;

public sealed class ImmaculateWhiteBow : CAModProjectile
{
    public override LocalizedText DisplayName => TOLocalizationUtils.GetItemName<ImmaculateWhite>();
    public override string Texture => "CalamityAnomalies/GameContents/Contributor/Mocangran_ImmaculateWhite/ImmaculateWhite";

    public override void SetDefaults()
    {
        Projectile.width = 22;
        Projectile.height = 60;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, 1f, 1f, 1f);
        Player player = Projectile.Owner;
        bool canUseItem = !player.CantUseHoldout() && player.HasAmmo(player.HeldItem);
        Vector2 actualPlayerPosition = player.RotatedRelativePoint(player.MountedCenter, true);

        if (Projectile.IsOnOwnerClient)
        {
            if (canUseItem)
            {
                Vector2 direction = Main.MouseWorld - actualPlayerPosition;
                if (player.gravDir == -1f)
                    direction.Y = Main.screenHeight - Main.mouseY + Main.screenPosition.Y - actualPlayerPosition.Y;
                Projectile.velocity = direction.SafeNormalize();
                float originalRotation = direction.ToRotation();

                Vector2 positionOffset = Projectile.velocity * 20f;
                positionOffset.X *= 0.65f;
                Projectile.Center = actualPlayerPosition + positionOffset;

                Projectile.rotation = originalRotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
                Projectile.spriteDirection = Projectile.direction;
                Projectile.timeLeft = 2;

                player.heldProj = Projectile.whoAmI;
                player.itemTime = 2;
                player.itemAnimation = 2;
                player.ChangeDir(Projectile.direction);
                float armRotation = (originalRotation - MathHelper.PiOver2) * player.gravDir + (player.gravDir == -1 ? MathHelper.Pi : 0f);
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation + 0f * Projectile.spriteDirection);
                player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armRotation + 0f * Projectile.spriteDirection);

                if (Timer1 % 20 == 0)
                {
                    player.PickAmmo(player.HeldItem, out int type, out float speed, out _, out float knockback, out _);
                    knockback = player.GetWeaponKnockback(player.HeldItem, knockback);

                    Vector2 originalProjectileSpawnCenter = Projectile.Center + new PolarVector2(20f, originalRotation);

                    float angleOffset = Main.rand.NextFloat(-0.05f, 0.05f);
                    float angle = originalRotation + angleOffset;
                    Vector2 projectileSpawnCenter = originalProjectileSpawnCenter + new PolarVector2(10f, angle);
                    Projectile.NewProjectileAction<ImmaculateBolt>(Projectile.GetSource_FromAI(), projectileSpawnCenter, new PolarVector2(Main.rand.NextFloat(2f, 2.5f), angle), Projectile.damage, knockback, player.whoAmI, p =>
                    {
                        if (Projectile.ai[2] == 1f)
                            p.ai[2] = 1f;
                    });
                }

                Timer1++;
            }
            else
            {
                player.Anomaly.ImmaculateWhite_Timer = 90;
                Projectile.Kill();
            }
        }
    }

    public override bool? CanHitNPC(NPC target) => false;
    public override bool CanHitPvp(Player target) => false;
}