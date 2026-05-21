// Developed by ColdsUx

using CalamityMod;

namespace CalamityAnomalies.GameContents.Contributor.Mocangran_ImmaculateWhite;

public sealed class ImmaculateWhiteBow : CAModProjectile
{
    public int itemPentrate = 1;//穿透数存储中间站

    public float ShootSpeed = 20f;//实际武器攻速

    public int CirtLimit => (int)ShootSpeed;
    public override LocalizedText DisplayName => TOLocalizationUtils.GetItemName<ImmaculateWhite>();
    public override string Texture => "CalamityAnomalies/GameContents/Contributor/Mocangran_ImmaculateWhite/ImmaculateWhite";

    public override void SetDefaults()
    {
        Projectile.width = 22;
        Projectile.height = 60;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.ignoreWater = true;
    }

    /*public override void OnSpawn(IEntitySource source)
    {
        if(source is EntitySource_Parent parent && parent.Entity is Item item && item.ModItem is ImmaculateWhite immaculateWhite)
        {
            itemPentrate = immaculateWhite.itemPentrate;
        }
    }*/

    public override void AI()
    {
        //说实话手持弹幕来做这个武器感觉有点多余（？
        //这就导致了词缀的攻击速度加不到这里，不过以后再说吧（
        ShootSpeed = 20f;
        if (NPC.downedBoss2)//世吞克脑
            ShootSpeed -= 1f;
        if (NPC.downedBoss3)//骷髅王
            ShootSpeed -= 1f;
        if (NPC.downedGolemBoss)//石巨人
            ShootSpeed -= 1f;
        if (DownedBossSystem_Bridge.downedCeaselessVoid
            && DownedBossSystem_Bridge.downedSignus
            && DownedBossSystem_Bridge.downedStormWeaver)//无尽虚空、西格纳斯，风编
            ShootSpeed -= 1f;
        if (DownedBossSystem_Bridge.downedYharon)//犽戎
            ShootSpeed -= 1f;

        Lighting.AddLight(Projectile.Center, 1f, 1f, 1f);
        Player player = Projectile.Owner;
        bool canUseItem = (!player.CantUseHoldout() || !CantUseRightHoldout(player)) && player.HasAmmo(player.HeldItem);
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

                if (Timer1 % ShootSpeed == 0)
                {
                    player.PickAmmo(player.HeldItem, out int type, out float speed, out _, out float knockback, out _);
                    knockback = player.GetWeaponKnockback(player.HeldItem, knockback);

                    Vector2 originalProjectileSpawnCenter = Projectile.Center + new PolarVector2(20f, originalRotation);

                    float angleOffset = Main.rand.NextFloat(-0.05f, 0.05f);
                    float angle = originalRotation;// + angleOffset;
                    Vector2 projectileSpawnCenter = originalProjectileSpawnCenter + new PolarVector2(10f, angle);
                    if (Projectile.ai[2] >= 2f && player.altFunctionUse == 2)
                    {
                        Projectile.NewProjectileAction<InfiniteImmaculateBolt>(Projectile.GetSource_FromAI(), projectileSpawnCenter, new PolarVector2(Main.rand.NextFloat(2f, 2.5f), angle), Projectile.damage / 2, knockback, player.whoAmI);
                    }
                    else
                    {
                        Projectile.NewProjectileAction<ImmaculateBolt>(Projectile.GetSource_FromAI(), projectileSpawnCenter, new PolarVector2(Main.rand.NextFloat(2f, 2.5f), angle), Projectile.damage, knockback, player.whoAmI, p =>
                        {
                            if (Projectile.ai[2] >= 1f)
                                p.ai[2] = 1f;
                            if (p.ModProjectile is ImmaculateBolt bolt)
                            {
                                bolt.ItemPentrate = itemPentrate;
                            }
                        });
                    }
                }

                Timer1++;
            }
            else
            {
                player.Anomaly.ImmaculateWhite_Timer = Main.zenithWorld ? 0 : CirtLimit;
                //Projectile.Kill();
            }
        }
    }

    public override bool? CanHitNPC(NPC target) => false;
    public override bool CanHitPvp(Player target) => false;

    //不知道为啥右键使用没法触发channel，在这写一个额外的。
    public static bool CantUseRightHoldout(Player player, bool needsToHold = true)
    {
        if (player != null && player.active && !player.dead && !(!Main.mouseRight && needsToHold) && !player.CCed)
        {
            return player.noItems;
        }

        return true;
    }
}