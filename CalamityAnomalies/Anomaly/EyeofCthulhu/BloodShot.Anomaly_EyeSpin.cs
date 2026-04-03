using CalamityAnomalies.VanillaOverrideEnums;

namespace CalamityAnomalies.Anomaly.EyeofCthulhu;

public sealed class BloodShot_Anomaly_EyeSpin : AnomalyProjectileBehavior
{
    public override int ApplyingType => ProjectileID.BloodShot;
    public override bool ShouldProcess => base.ShouldProcess && (OverrideType_BloodShot?)AnomalyProjectile?.OverrideType is OverrideType_BloodShot.AnomalyEyeofCthulhu_EyeSpin or OverrideType_BloodShot.AnomalyEyeofCthulhu_EyeSpin2;

    public NPC Master
    {
        get => Main.npc[AnomalyProjectile.AnomalyAI32[0].i];
        set
        {
            int temp = value.whoAmI;
            if (AnomalyProjectile.AnomalyAI32[0].i != temp)
            {
                AnomalyProjectile.AnomalyAI32[0].i = temp;
                AnomalyProjectile.AIChanged32[0] = true;
            }
        }
    }

    public Projectile ArenaProjectile
    {
        get => Main.projectile[AnomalyProjectile.AnomalyAI32[1].i];
        set
        {
            int temp = value.whoAmI;
            if (AnomalyProjectile.AnomalyAI32[1].i != temp)
            {
                AnomalyProjectile.AnomalyAI32[1].i = temp;
                AnomalyProjectile.AIChanged32[1] = true;
            }
        }
    }
    public bool ArenaProjectileAlive => ArenaProjectile.active && ArenaProjectile.ModProjectile is EyeofCthulhuArena arena && arena.Master == Master;
    public EyeofCthulhuArena ArenaModProjectile => ArenaProjectile.GetModProjectile<EyeofCthulhuArena>();

    public override bool PreAI()
    {
        Timer1++;

        if (!ArenaProjectileAlive)
            Projectile.Kill();

        switch (Timer1 - EyeofCthulhu_Handler.EyeSpinPhase2Time)
        {
            case >= 1 and < 20:
                Projectile.SetVelocityandRotation((ArenaProjectile.Center - Projectile.Center).ToCustomLength(0.001f), MathHelper.PiOver2);
                Projectile.position -= Projectile.velocity;
                break;
            case 20:
                if ((OverrideType_BloodShot)AnomalyProjectile.OverrideType == OverrideType_BloodShot.AnomalyEyeofCthulhu_EyeSpin2)
                {
                    Projectile.Kill();
                    return false;
                }

                Vector2 velocity = (Projectile.Center - ArenaProjectile.Center).ToCustomLength(Main.rand.NextFloat(12f, 17.5f)).RotatedByRandom(-MathHelper.PiOver4, MathHelper.PiOver4);
                Projectile.SetVelocityandRotation(velocity, MathHelper.PiOver2);
                break;
            case > 20:
            default:
                if (ArenaProjectileAlive && Projectile.Distance(ArenaProjectile.Center) > ArenaModProjectile.RealArenaRadius + 30f)
                    Projectile.Kill();
                break;
        }

        return false;
    }
}
