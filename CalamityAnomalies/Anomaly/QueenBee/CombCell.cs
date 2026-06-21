using CalamityAnomalies.Anomaly.EyeofCthulhu;

namespace CalamityAnomalies.Anomaly.QueenBee;

public sealed class CombCell : CAModProjectile
{
    public enum Behavior : byte
    {
        Beehive,
        Beehell,
    }

    public const float HexagonRadius = 140f;

    public NPC Master
    {
        get
        {
            int temp = (int)Projectile.ai[0];
            return temp >= 0 && temp < Main.maxNPCs ? Main.npc[temp] : null;
        }
        set => Projectile.ai[0] = value.whoAmI;
    }
    public QueenBee_Anomaly MasterBehavior => QueenBee_Anomaly.GetNewInstance(Master);

    public Behavior BehaviorType
    {
        get
        {
            Union32 union = AI_Union_1;
            return (Behavior)union.byte0;
        }
        set
        {
            Union32 union = AI_Union_1;
            union.byte0 = (byte)value;
            AI_Union_1 = union;
        }
    }

    public float FinalScale = 1f;

    public override string LocalizationCategory => "Anomaly.QueenBee";

    public override void SetDefaults()
    {
        Projectile.width = 200;
        Projectile.height = 200;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 600;

        Projectile.Opacity = 0.5f;
    }

    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, 1f, 0.6f, 0f);

        Timer1++;

        Projectile.scale = FinalScale * TOMathUtils.Interpolation.QuadraticEaseOut(Timer1 / 20f);

        switch (BehaviorType)
        {
            case Behavior.Beehive:
                Behavior_Beehive();
                break;
            case Behavior.Beehell:
                Behavior_Beehell();
                break;
            default:
                break;
        }

        void Decelerate(float factor = 0.95f)
        {
            Projectile.velocity *= factor;
            if (Projectile.velocity.Length() < 0.1f)
                Projectile.velocity = Vector2.Zero;
        }

        void Behavior_Beehive()
        {
            Decelerate();

            if (Timer1 == 90)
            {
                for (int i = 0; i < 12; i++)
                {
                    float speed = Main.rand.NextFloat(10f, 15f);
                    Vector2 velocity = PolarVector2.UnitClocks[i].RotatedByRandom(MathHelper.ToRadians(10f)) * speed;
                    Projectile.NewProjectileAction<BeeProjectile>(SourceAI, Projectile.Center, velocity, 10, 0f);
                }

                Projectile.Kill();
            }
            return;
        }

        void Behavior_Beehell()
        {
            Projectile.Center = Master.Center;
            QueenBee_Anomaly masterBehavior = MasterBehavior;
            if (masterBehavior.CurrentAttackPhase >= 3 || masterBehavior.CurrentBehavior != QueenBee_Anomaly.Behavior.Phase1_Beehell)
                Projectile.Kill();
        }
    }

    public override void OnKill(int timeLeft)
    {
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => new Hexagon(Projectile.Center, HexagonRadius * Projectile.scale, Projectile.rotation + TOMathUtils.PiOver6).Collides(targetHitbox);

    public override bool PreDraw(ref Color lightColor)
    {
        Main.spriteBatch.DrawFromCenter(Projectile.Texture, Projectile.Center - Main.screenPosition, null, Color.White * Projectile.Opacity, Projectile.rotation, Projectile.scale);
        return false;
    }
}
