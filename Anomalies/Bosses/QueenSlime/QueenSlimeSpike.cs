namespace Anomalies.Bosses.QueenSlime;

public sealed class QueenSlimeSpike : AnomalyProjectileBehavior<QueenSlimeSpike>
{
    public Color LightColor
    {
        get => AnomalyProjectile.AnomalyAI32[0].GetValue<Color>();
        set
        {
            if (AnomalyProjectile.AnomalyAI32[0].GetValue<Color>() != value)
            {
                AnomalyProjectile.AnomalyAI32[0].SetValue(value);
                AnomalyProjectile.AIChanged32[0] = true;
            }
        }
    }

    public override int ApplyingType => ProjectileID.QueenSlimeMinionBlueSpike;

    public override bool AllowCalamityLogic(CalamityLogicType_ProjectileBehavior type) => type switch
    {
        CalamityLogicType_ProjectileBehavior.GetAlpha => false,
        _ => true,
    };

    public override void SetDefaults()
    {
        LightColor = NPC.AI_121_QueenSlime_GetDustColor();
    }

    public override bool PreAI()
    {
        Lighting.AddLight(Projectile.Center, LightColor.ToVector3());
        return true;
    }

    public override Color? GetAlpha(Color lightColor) => lightColor;
}

