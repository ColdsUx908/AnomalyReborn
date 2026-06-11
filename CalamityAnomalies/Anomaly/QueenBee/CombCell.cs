namespace CalamityAnomalies.Anomaly.QueenBee;

public sealed class CombCell : CAModProjectile
{
    public enum Behavior : byte
    {
        Beehive,
    }

    public const float HexagonRadius = 140f;

    public Behavior BehaviorType
    {
        get
        {
            Union32 union = AI_Union_0;
            return (Behavior)union.byte0;
        }
        set
        {
            Union32 union = AI_Union_0;
            union.byte0 = (byte)value;
            AI_Union_0 = union;
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
    }

    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, 1f, 0.6f, 0f);

        Timer1++;

        Projectile.scale = FinalScale * TOMathUtils.Interpolation.QuadraticEaseOut(Timer1 / 20f);

        switch (BehaviorType)
        {
            case Behavior.Beehive:
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
    }

    public override void OnKill(int timeLeft)
    {
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => new Hexagon(Projectile.Center, HexagonRadius * Projectile.scale, Projectile.rotation + TOMathUtils.PiOver6).Collides(targetHitbox);
}
