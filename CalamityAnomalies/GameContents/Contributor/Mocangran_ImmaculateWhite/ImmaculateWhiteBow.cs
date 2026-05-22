// Developed by ColdsUx

using CalamityMod;

namespace CalamityAnomalies.GameContents.Contributor.Mocangran_ImmaculateWhite;

public sealed class ImmaculateWhiteBow : CAModProjectile
{
    public bool CanShootSplitBolt
    {
        get => AI_Union_0.bits[0];
        set
        {
            Union32 union = AI_Union_0;
            union.bits[0] = value;
            AI_Union_0 = union;
        }
    }

    public bool CanUseRightClickFunction
    {
        get => AI_Union_0.bits[1];
        set
        {
            Union32 union = AI_Union_0;
            union.bits[1] = value;
            AI_Union_0 = union;
        }
    }

    public float ShootSpeed = 20f; //实际武器攻速
    public float ShootSpeedMultiplier = 1f; //攻速倍率，主要用于修饰语加成

    public int CirtLimit => (int)ShootSpeed;
    public override LocalizedText DisplayName => ModContent.GetModItem<ImmaculateWhite>().DisplayName;
    public override string Texture => ModContent.GetModItem<ImmaculateWhite>().Texture;

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
        ShootSpeed = 20f;
        if (NPC.downedBoss2) //世吞克脑
            ShootSpeed -= 1f;
        if (NPC.downedBoss3) //骷髅王
            ShootSpeed -= 1f;
        if (NPC.downedGolemBoss) //石巨人
            ShootSpeed -= 1f;
        if (DownedBossSystem_Bridge.downedCeaselessVoid && DownedBossSystem_Bridge.downedSignus && DownedBossSystem_Bridge.downedStormWeaver)//无尽虚空、西格纳斯，风编
            ShootSpeed -= 1f;
        if (DownedBossSystem_Bridge.downedYharon) //犽戎
            ShootSpeed -= 1f;

        ShootSpeed = (int)(ShootSpeed * ShootSpeedMultiplier); //应用修饰语加成

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

                    Projectile.NewProjectileAction<ImmaculateBolt>(Projectile.GetSource_FromAI(), projectileSpawnCenter, new PolarVector2(Main.rand.NextFloat(2f, 2.5f), angle), Projectile.damage, knockback, player.whoAmI, p =>
                    {
                        ImmaculateBolt modP = p.GetModProjectile<ImmaculateBolt>();

                        if (CanShootSplitBolt && player.altFunctionUse == 2)
                        {
                            modP.IsInfiniteProjectile = true;
                            float damageMultiplier = 0.6f;
                            p.damage = (int)Math.Round(p.damage * damageMultiplier);
                        }

                        if (CanShootSplitBolt)
                            modP.IsSplittableProjectile = true;
                    });
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