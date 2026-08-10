// Developed by ColdsUx

using Anomalies.Bosses.KingSlime;

namespace Anomalies.Bosses.QueenSlime;

public sealed class QueenSlimeGel : AnomalyProjectileBehavior<QueenSlimeGel>
{
    public int BehaviorType
    {
        get => (int)Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }

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

        switch (BehaviorType)
        {
            case 0:
                SlimeGel.Gel_AI(Projectile);
                break;
            case 1:
                Projectile.tileCollide = false;
                break;
        }

        return false;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (BehaviorType == 0)
        {
            return SlimeGel.Gel_OnTileCollide(Projectile, oldVelocity, DustID.TintableDust, d =>
            {
                d.velocity = Main.rand.NextPolarVector2(1f, 5f);
                d.noGravity = true;
                d.color = NPC.AI_121_QueenSlime_GetDustColor();
            });
        }
        return true;
    }

    public override Color? GetAlpha(Color lightColor) => lightColor;
}
