// Developed by ColdsUx

using CalamityAnomalies.Anomaly.KingSlime;

namespace CalamityAnomalies.Anomaly.QueenSlime;

public sealed class QueenSlimeGel_Anomaly : AnomalyProjectileBehavior<QueenSlimeGel_Anomaly>
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

    public override int ApplyingType => ProjectileID.QueenSlimeGelAttack;

    public override bool AllowCalamityLogic(CalamityLogicType_ProjectileBehavior type) => type switch
    {
        CalamityLogicType_ProjectileBehavior.PreAI => false,
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
        SlimeGel.Gel_AI(Projectile);
        return false;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) => SlimeGel.Gel_OnTileCollide(Projectile, oldVelocity, DustID.TintableDust, d =>
    {
        d.velocity = Main.rand.NextPolarVector2(1f, 5f);
        d.noGravity = true;
        d.color = NPC.AI_121_QueenSlime_GetDustColor();
    });

    public override Color? GetAlpha(Color lightColor) => lightColor;
}
