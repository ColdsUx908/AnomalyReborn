using CalamityAnomalies.GameContents.Contributor.Mocangran_ImmaculateWhite;

namespace CalamityAnomalies.GameContents.Contributor.Mocangran_VacuousBlack;

public sealed class VacuousBlackMinion : CAModProjectile
{
    public int Phase;
    public int SubPhase;

    public ref bool MinionBool => ref Owner.Anomaly.Minion_VacuousBlack;

    public override string LocalizationCategory => "GameContents.Contributor";

    public override LocalizedText DisplayName => ModContent.GetModItem<VacuousBlack>()?.DisplayName;

    public override string Texture => TOTextures.InvisibleTexturePath;

    public float GetInterpolation(float baseValue, float additionalValue) => baseValue + additionalValue * MathF.Pow(Projectile.minionSlots, 0.8f);

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.MinionTargettingFeature[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.width = 50;
        Projectile.height = 50;
        Projectile.scale = 0.5f;
        Projectile.netImportant = true;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.minionSlots = 1f;
        Projectile.timeLeft = 18000;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 3;
        Projectile.ArmorPenetration = 200;
        Projectile.tileCollide = false;
        Projectile.timeLeft *= 5;
        Projectile.minion = true;
        Projectile.DamageType = DamageClass.Summon;
    }

    public override void AI()
    {
        if (Timer1 <= 50)
            Timer1++;

        Player player = Projectile.Owner;
        if (player.dead)
            MinionBool = false;
        if (MinionBool)
            Projectile.timeLeft = 2;

        Projectile.velocity = Vector2.Zero;
        Vector2 actualPlayerPosition = player.RotatedRelativePoint(player.MountedCenter, true);
        Projectile.Center = actualPlayerPosition;

        ImmaculateWhite.GetPhase(out Phase, out SubPhase);

        float baseDamage = 4;
        baseDamage *= Projectile.minionSlots;
        baseDamage *= ImmaculateWhite.GetDamageMultiplier(Phase, SubPhase);
        Projectile.damage = Math.Max(0, (int)(player.GetTotalDamage(DamageClass.Summon).ApplyTo(baseDamage) + 5E-06f));

        Projectile.scale = GetInterpolation(0.4f, 0.1f) * TOMathUtils.Interpolation.QuadraticEaseOut(Timer1 / 10f);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D texture = BloomParticle.BloomCircleLarge;
        //spriteBatch.ChangeBlendState(BlendState.Additive);
        spriteBatch.DrawFromCenter(texture, Projectile.Center - Main.screenPosition, null, Color.Black, 0f, Projectile.scale * (1f + TOMathUtils.TimeWrappingFunction.GetTimeSin(0.01f, 30f)));
        //spriteBatch.ChangeBlendState(BlendState.AlphaBlend);
        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => new Circle(Projectile.Center, BloomParticle.BloomCircleLargeRadius * Projectile.scale).Collides(targetHitbox);

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (TOSharedData.AprilFools || (!target.IsBossEnemy && !target.IsImportantBossMinion))
        {
            target.velocity = Owner.GetVelocityTowards(target.Center, GetInterpolation(10f, 1.35f));
        }
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        target.velocity = Owner.GetVelocityTowards(target.Center, GetInterpolation(10f, 1.35f));
    }
}
