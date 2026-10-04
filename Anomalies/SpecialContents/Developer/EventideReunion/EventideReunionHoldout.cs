namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed partial class EventideReunionHoldout : AnomalyModProjectile
{
    public override LocalizedText DisplayName => ModContent.GetModItem<EventideReunion>()?.DisplayName;
    public override string Texture => ModContent.GetModItem<EventideReunion>()?.Texture;

    public const string AssetPath = "Anomalies/GameContents/Developer/EventideReunion/";

    public static readonly SoundStyle Thrust = new(AssetPath + "Thrust") { MaxInstances = 0 };
    public static readonly SoundStyle SweepUpward = new(AssetPath + "SweepUpward") { MaxInstances = 0 };

    public static Color GetRandomColor() => Color.Lerp(Color.Red, Color.Orange, Main.rand.NextFloat());
    /// <summary>
    /// 中央宝珠的位置。
    /// </summary>
    public Vector2 OrbCenter => Projectile.Center + new PolarVector2(60.81f * Projectile.scale, Projectile.rotation);

    public Vector2 InitialCenter;

    public int Phase;
    public bool FaceRight;

    public override void SetDefaults()
    {
        Projectile.width = 312;
        Projectile.height = 312;
        Projectile.scale = 0.7f;
        Projectile.alpha = 255;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 5;
        Projectile.ArmorPenetration = 9999;
    }

    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, 1f, 1f, 1f);

        if (!Projectile.IsOnOwnerClient)
            return;

        Player player = Projectile.Owner;
        bool canUseItem = !player.CanNotUseHoldOut() && player.HasAmmo(player.HeldItem);
        Vector2 actualPlayerPosition = player.RotatedRelativePoint(player.MountedCenter, true);

        if (!canUseItem)
        {
            Projectile.Kill();
            return;
        }

        player.heldProj = Projectile.whoAmI;

        Projectile.timeLeft = 30;
        player.itemTime = 30;
        player.itemAnimation = 30;

        Vector2 direction = Main.MouseWorld - actualPlayerPosition;
        if (player.gravDir == -1f)
            direction.Y = Main.screenHeight - Main.mouseY + Main.screenPosition.Y - actualPlayerPosition.Y;
        Vector2 initialCenter = actualPlayerPosition + new PolarVector2(60f * Projectile.scale, Projectile.rotation) + new Vector2(0f, 10f);

        switch (Phase)
        {
            case 0:
                Projectile.velocity = direction.SafeNormalize();
                FaceRight = direction.X >= 0;
                float originalRotation = direction.ToRotation();
                Projectile.rotation = originalRotation;
                Projectile.Center = initialCenter;

                Phase = 1;
                break;
            case 1:
                float a = 5f;
                float delay = 3f;

                if (Timer1 <= a * 2)
                {
                    if (Timer1 == 0)
                        SoundEngine.PlaySound(Thrust, OrbCenter);

                    Timer1++;

                    float ratio = Timer1 > a ? (a * 2 - Timer1) / a : Timer1 / a;
                    Projectile.Center = initialCenter + Projectile.velocity * 130f * Projectile.scale * ratio;
                }
                else
                {
                    int offset = (int)(a * 2 + delay);

                    if (Timer1 == offset)
                        SoundEngine.PlaySound(Thrust, OrbCenter);

                    Timer1++;

                    int timer = Math.Max(Timer1 - offset, 0);
                    float ratio = timer > a ? (a * 2 - timer) / a : timer / a;
                    Projectile.Center = initialCenter + Projectile.velocity * 170f * Projectile.scale * ratio;

                    if (timer >= a * 2)
                    {
                        Timer1 = 0;
                        Phase = 2;
                    }
                }
                break;
            case 2:
                Projectile.Kill();
                Timer1++;

                float ratio2 = Timer1 > 10 ? (20 - Timer1) / 10f : Timer1 / 10f;
                Projectile.Center = initialCenter + Projectile.velocity * 100f * Projectile.scale * ratio2;

                if (Timer1 >= 20)
                {
                    Projectile.Kill();
                    Phase = 3;
                    Timer1 = 0;
                }
                break;
        }

        player.ChangeDir(FaceRight.ToDirectionInt());

        Vector2 p = Projectile.Center - Projectile.velocity * 80f;
        float armRotation = (p - actualPlayerPosition).ToRotation(-MathHelper.PiOver2) * player.gravDir + (player.gravDir == -1 ? MathHelper.Pi : 0f);
        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation + 0f * Projectile.spriteDirection);
        //ParticleHandler.SpawnParticle(new OrbParticle(p, Vector2.Zero, 20, 1f, Color.Red));


        /*
        if (!HasInitialized && Timer1 == 1)
        {
            HasInitialized = true;
        }

        Timer1++;

    
        if (Projectile.IsOnOwnerClient)
        {
            if (canUseItem)
            {

                //Vector2 positionOffset = Projectile.velocity * 20f;
                //positionOffset.X *= 0.65f;
                //player.ChangeDir(Projectile.direction);
                float armRotation = (originalRotation - MathHelper.PiOver2) * player.gravDir + (player.gravDir == -1 ? MathHelper.Pi : 0f);
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation + 0f * Projectile.spriteDirection);
                //player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armRotation + 0f * Projectile.spriteDirection);

                //ParticleHandler.SpawnParticle(new OrbParticle(OrbCenter, Vector2.Zero, 30, 1.5f, Color.Red));

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

                        modP.Target = TOKinematicUtils.GetNPCTarget(Main.MouseWorld, 8000f, ignoreTiles: true);

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
                
            }
            else
            {
                //player.Anomaly.ImmaculateWhite_Timer = Main.zenithWorld ? 0 : CirtLimit;
                //Projectile.Kill();
            }
        */
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float scale = Projectile.scale;

        //快速检测
        //if (!FloatRectangle.FromCenter(Projectile.Center, 450f * scale, 450f * scale).Collides(targetHitbox))
        //    return false;

        Vector2 orbCenter = OrbCenter;

        if (RotatedRectangle.FromInnerPoint(orbCenter, 253f * scale, 0f, 8f * scale, 8f * scale, Projectile.rotation).Collides(targetHitbox))
            return true;

        if (RotatedRectangle.FromInnerPoint(orbCenter, 0f, 158f * scale, 24f * scale, 24f * scale, Projectile.rotation).Collides(targetHitbox))
            return true;

        if (RotatedRectangle.FromInnerPoint(orbCenter, 10f * scale, 10f * scale, 85f * scale, 85f * scale, Projectile.rotation).Collides(targetHitbox))
            return true;

        if (new Circle(orbCenter, 40f * scale).Collides(targetHitbox))
            return true;

        return false;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Main.spriteBatch.DrawFromCenter(Projectile.Texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation + MathHelper.PiOver4, Projectile.scale);
        return false;
    }
}