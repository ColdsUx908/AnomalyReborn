// Developed by ColdsUx

namespace CalamityAnomalies.Anomaly.QueenBee;

public sealed class CombCell : CAModProjectile
{
    public enum Behavior : byte
    {
        Beehive,
        BeeSwarm,
        BeeSwarm2,
        BeeSwarm2_Fake,
        BeeSwarm2_Safe,
    }

    public const float HexagonRadius = 146f;

    public const string AnomalyQueenBeePath = "CalamityAnomalies/Anomaly/QueenBee/";

    [LoadTexture(AnomalyQueenBeePath + "CombCell")]
    private static Asset<Texture2D> _cellTexture;
    public static Texture2D CellTexture => _cellTexture.Value;

    [LoadTexture(AnomalyQueenBeePath + "CombCellBorder")]
    private static Asset<Texture2D> _cellBorderTexture;
    public static Texture2D CellBorderTexture => _cellBorderTexture.Value;

    public static void DrawCell(SpriteBatch spriteBatch, Vector2 center, float scale, float rotation, Color color, float innerOpacityMultiplier)
    {
        spriteBatch.DrawFromCenter(CellBorderTexture, center - Main.screenPosition, null, color, rotation, scale);
        color *= innerOpacityMultiplier;
        spriteBatch.DrawFromCenter(CellTexture, center - Main.screenPosition, null, color, rotation, scale);
    }

    public Hexagon HitBox => new(Projectile.Center, HexagonRadius * Projectile.scale, Projectile.rotation + TOMathUtils.PiOver6);

    public NPC Master
    {
        get => NPC.TryGetNPC((int)Projectile.ai[0]);
        set => Projectile.ai[0] = value?.whoAmI ?? -1;
    }
    public QueenBee_Anomaly MasterBehavior => QueenBee_Anomaly.GetInstance(Master);

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

    public bool HasContactDamage
    {
        get
        {
            Union32 union = AI_Union_2;
            return union.bits[0];
        }
        set
        {
            Union32 union = AI_Union_2;
            union.bits[0] = value;
            AI_Union_2 = union;
        }
    }

    public float FinalScale = 1f;
    public Vector2 Offset;

    public override string LocalizationCategory => "Anomaly.QueenBee";

    public override void SetStaticDefaults()
    {
    }

    public override void SetDefaults()
    {
        Projectile.width = 200;
        Projectile.height = 200;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 600;

        Projectile.hide = true;
    }

    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, 1f, 0.6f, 0f);

        Timer1++;

        Projectile.scale = FinalScale * TOMathUtils.Interpolation.QuadraticEaseOut(Timer1 / 20f);
        HasContactDamage = false;

        switch (BehaviorType)
        {
            case Behavior.Beehive:
                Behavior_Beehive();
                break;
            case Behavior.BeeSwarm:
                Behavior_BeeSwarm();
                break;
            case Behavior.BeeSwarm2:
                Behavior_BeeSwarm2(0);
                break;
            case Behavior.BeeSwarm2_Fake:
                Behavior_BeeSwarm2(1);
                break;
            case Behavior.BeeSwarm2_Safe:
                Behavior_BeeSwarm2(2);
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

            HasContactDamage = true;

            if (Timer1 == 90)
            {
                for (int i = 0; i < 12; i++)
                {
                    float speed = Main.rand.NextFloat(10f, 15f);
                    Vector2 velocity = PolarVector2.UnitClocks[i].RotatedByRandom(MathHelper.ToRadians(10f)) * speed;
                    Projectile.NewProjectileAction<BeeProjectile>(SourceAI, Projectile.Center, velocity, QueenBee_Anomaly.BeeDamage, 0f);
                }

                Projectile.Kill();
            }
            return;
        }

        void Behavior_BeeSwarm()
        {
            HasContactDamage = true;

            Projectile.Center = Master.Center + new Vector2(0f, -30f * Master.scale);

            if (Master is null || !Master.active || Master.type != NPCID.QueenBee)
                Projectile.Kill();

            QueenBee_Anomaly masterBehavior = MasterBehavior;
            if (masterBehavior.CurrentAttackPhase >= 3 || masterBehavior.CurrentBehavior != QueenBee_Anomaly.Behavior.Phase1_BeeSwarm)
                Projectile.Kill();
        }

        void Behavior_BeeSwarm2(int type)
        {
            HasContactDamage = false;

            Projectile.Center = Master.Center + new Vector2(0f, -30f * Master.scale) + Offset;

            if (Master is null || !Master.active || Master.type != NPCID.QueenBee)
                Projectile.Kill();

            QueenBee_Anomaly masterBehavior = MasterBehavior;
            switch (type)
            {
                case 0:
                    HasContactDamage = true;
                if (masterBehavior.CurrentAttackPhase >= 3 || masterBehavior.CurrentBehavior != QueenBee_Anomaly.Behavior.Phase1_BeeSwarm2)
                    Projectile.Kill();
                    break;
                case 1:
                    HasContactDamage = false;
                    if (masterBehavior.CurrentAttackPhase >= 3 || masterBehavior.CurrentBehavior != QueenBee_Anomaly.Behavior.Phase1_BeeSwarm2 || (masterBehavior.CurrentAttackPhase == 1 && masterBehavior.Timer1 >= 285))
                        Projectile.Kill();
                    break;
                case 2:
                    HasContactDamage = false;
                    if (Timer2 > 0)
                    {
                        Timer2++;

                        if (Timer2 >= 75)
                            Projectile.Kill();
                    }
                    else if (masterBehavior.CurrentAttackPhase >= 3 || masterBehavior.CurrentBehavior != QueenBee_Anomaly.Behavior.Phase1_BeeSwarm2)
                        Timer2++;
                    break;
            }
        }
    }

    public override void OnKill(int timeLeft)
    {
        QueenBee_Handler.SpawnGores(this);
    }

    public override bool CanHitPlayer(Player target) => HasContactDamage;

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => new Hexagon(Projectile.Center, HexagonRadius * Projectile.scale, Projectile.rotation + TOMathUtils.PiOver6).Collides(targetHitbox);

    public override bool PreDraw(ref Color lightColor)
    {
        float innerOpacityMultiplier = BehaviorType switch
        {
            Behavior.Beehive => 0.8f,
            Behavior.BeeSwarm => 0.7f,
            Behavior.BeeSwarm2 => 0.7f,
            Behavior.BeeSwarm2_Fake => 0.6f,
            Behavior.BeeSwarm2_Safe => 0.8f,

            _ => 1f
        };

        DrawCell(Main.spriteBatch, Projectile.Center, Projectile.scale, Projectile.rotation, Color.White * Projectile.Opacity, innerOpacityMultiplier);
        return false;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => overPlayers.Add(index);
}
