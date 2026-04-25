using CalamityMod.Buffs.DamageOverTime;

namespace CalamityAnomalies.Anomaly.EyeofCthulhu;

public class BloodOrbProjectile : CAModProjectile
{
    public const int StillTime = 25;

    public NPC Master
    {
        get => Main.npc[(int)Projectile.ai[0]];
        set => Projectile.ai[0] = value.whoAmI;
    }

    public Projectile ArenaProjectile
    {
        get => Main.projectile[(int)Projectile.ai[1]];
        set => Projectile.ai[1] = value.whoAmI;
    }
    public bool ArenaProjectileAlive => ArenaProjectile.active && ArenaProjectile.ModProjectile is EyeofCthulhuArena arena && arena.Master == Master;
    public EyeofCthulhuArena ArenaModProjectile => ArenaProjectile.GetModProjectile<EyeofCthulhuArena>();

    public Vector2 Destination;

    public int BehaviorType
    {
        get => (int)Projectile.ai[2];
        set => Projectile.ai[2] = value;
    }

    public override string LocalizationCategory => "Anomaly.EyeofCthulhu";

    public override string Texture => EyeofCthulhu_Handler.AnomalyEyeofCthulhuPath + "BloodOrb";

    public override void SetDefaults()
    {
        Projectile.width = 40;
        Projectile.height = 40;
        Projectile.scale = 0.6f;
        Projectile.hostile = true;
        Projectile.ignoreWater = true;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void AI()
    {
        if (!ArenaProjectileAlive)
            Projectile.Kill();

        Lighting.AddLight(Projectile.Center, 0.9f, 0f, 0.15f);
        if (Main.rand.NextBool(3))
        {
            Dust.NewDustAction(Projectile.Center, Projectile.width, Projectile.height, DustID.Blood, action: d =>
            {
                if (Main.rand.NextBool(2))
                    d.scale *= 1.5f;
            });
        }

        Timer1++;
        Projectile.scale = 0.7f * TOMathUtils.Interpolation.QuadraticEaseOut(Timer1 / 10f);

        Projectile.rotation += 0.05f;

        if (Timer2 <= 0)
        {
            if (Projectile.Distance(Destination) <= Projectile.velocity.Length())
            {
                Timer2++;
                Projectile.velocity = Vector2.Zero;
                Projectile.Center = Destination;
            }
        }
        else
        {
            Timer2++;

            switch (Timer2)
            {
                case StillTime:
                    if (BehaviorType == 1)
                    {
                        Projectile.Kill();
                        return;
                    }

                    Vector2 velocity = (Projectile.Center - ArenaProjectile.Center).ToCustomLength(Main.rand.NextFloat(12f, 17.5f)).RotatedByRandom(-MathHelper.PiOver4, MathHelper.PiOver4);
                    Projectile.SetVelocityandRotation(velocity);
                    break;
                case > StillTime:
                    if (BehaviorType == 1)
                    {
                        Projectile.Kill();
                        return;
                    }

                    goto default;
                default:
                    if (ArenaProjectileAlive && Projectile.Distance(ArenaProjectile.Center) > ArenaModProjectile.RealArenaRadius + 30f)
                        Projectile.Kill();
                    break;
            }
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => new Circle(Projectile.Center, 20f * Projectile.scale).Collides(targetHitbox);

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        if (info.Damage <= 0)
            return;

        target.AddBuff<BurningBlood>(120);
        target.AddBuff(BuffID.OnFire3, 120);

        if (Main.zenithWorld)
            target.AddBuff<MiracleBlight>(60);
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 10; i++)
        {
            Dust.NewDustAction(Projectile.Center, Projectile.width, Projectile.height, DustID.Blood, action: d =>
            {
                if (Main.rand.NextBool(2))
                    d.scale *= 1.5f;
            });
        }
    }

    public override bool PreDraw(ref Color lightColor) => false; //在竞技场的PreDraw中集中绘制
}
