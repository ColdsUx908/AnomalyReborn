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
        BeeSwarm3,
        BeeSwarm3_Move,
        PhaseChange,
    }

    public static readonly Vector2[] HexagonVerticesUnit =
    [
        PolarVector2.UnitClocks[0],
        PolarVector2.UnitClocks[2],
        PolarVector2.UnitClocks[4],
        PolarVector2.UnitClocks[6],
        PolarVector2.UnitClocks[8],
        PolarVector2.UnitClocks[10],
        PolarVector2.UnitClocks[0]
    ];

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

    #region 联合数据
    public Union32 ExtraUnion0;
    public Union32 ExtraUnion1;

    //使用场景：BeeSwarm2

    /// <summary>
    /// 位置相比中心蜂巢的偏移量。
    /// </summary>
    public Vector2 Offset 
    {
        get => new(ExtraUnion0.f, ExtraUnion1.f);
        set
        {
            ExtraUnion0.f = value.X;
            ExtraUnion1.f = value.Y;
        }
    }

    //使用场景：BeeSwarm3

    /// <summary>
    /// 在六边形边上的位置参数。
    /// <br/>为 0 表示上方顶点，为 1 表示右上顶点，为 2 表示右下顶点，为 3 表示下方顶点，为 4 表示左下顶点，为 5 表示左上顶点。
    /// <br/>赋值时自动模 6。
    /// </summary>
    public float CurrentPositionParameter
    {
        get => ExtraUnion0.f;
        set => ExtraUnion0.f = TOMathUtils.NormalizeWithPeriod(value, 6f);
    }
    /// <summary>
    /// 移动方向。
    /// <br/>为 1 表示顺时针移动，为 -1 表示逆时针移动。
    /// <br/>仅应被赋值为 1 或 -1。
    /// </summary>
    public ref int MoveDirection => ref ExtraUnion1.i;
    #endregion 联合数据

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
        Projectile.timeLeft = 1200;

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
            case Behavior.BeeSwarm3:
                Behavior_BeeSwarm3(0);
                break;
            case Behavior.BeeSwarm3_Move:
                Behavior_BeeSwarm3(1);
                break;
            case Behavior.PhaseChange:
                Behavior_PhaseChange();
                break;
            default:
                break;
        }

        bool isHoneyWet = BehaviorType is Behavior.BeeSwarm or Behavior.BeeSwarm2 or Behavior.BeeSwarm2_Safe or Behavior.BeeSwarm3_Move;
        if (isHoneyWet)
            QueenBee_Handler.HoneyWetCombCells.Add(Projectile.whoAmI);

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

            Projectile.Center = QueenBee_Handler.GetOwnedCombCellCenter(Master);

            if (Master is null || !Master.active || Master.type != NPCID.QueenBee)
                Projectile.Kill();

            QueenBee_Anomaly masterBehavior = MasterBehavior;
            if (masterBehavior.CurrentAttackPhase >= 3 || masterBehavior.CurrentBehavior != QueenBee_Anomaly.Behavior.Phase1_BeeSwarm)
                Projectile.Kill();
        }

        void Behavior_BeeSwarm2(int type)
        {
            HasContactDamage = false;

            Projectile.Center = QueenBee_Handler.GetOwnedCombCellCenter(Master) + Offset;

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
                    if (masterBehavior.CurrentAttackPhase >= 3 || masterBehavior.CurrentBehavior != QueenBee_Anomaly.Behavior.Phase1_BeeSwarm2 || (masterBehavior.CurrentAttackPhase == 1 && masterBehavior.Timer1 >= 300))
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

        void Behavior_BeeSwarm3(int type)
        {
            if (Master is null || !Master.active || Master.type != NPCID.QueenBee)
                Projectile.Kill();

            QueenBee_Anomaly masterBehavior = MasterBehavior;
            switch (type)
            {
                case 0:
                    HasContactDamage = true;
                    if (masterBehavior.CurrentAttackPhase >= 3 || masterBehavior.CurrentBehavior != QueenBee_Anomaly.Behavior.Phase2_BeeSwarm3)
                    {
                        Projectile.Kill();
                        break;
                    }

                    Projectile.Center = QueenBee_Handler.GetOwnedCombCellCenter(Master);
                    break;
                case 1:
                    HasContactDamage = true;
                    if (masterBehavior.CurrentAttackPhase >= 3 || masterBehavior.CurrentBehavior != QueenBee_Anomaly.Behavior.Phase2_BeeSwarm3)
                    {
                        Projectile.Kill();
                        break;
                    }

                    //以六边形轨迹移动
                    float radius = 350f;
                    if (Timer1 >= 50)
                    {
                        float speed = Utils.Remap(Timer1, 50f, 70f, 0f, 0.0114f);
                        CurrentPositionParameter += speed * MoveDirection;
                    }
                    Projectile.Center = QueenBee_Handler.GetOwnedCombCellCenter(Master) + radius * Vector2.LerpMany(HexagonVerticesUnit, CurrentPositionParameter / 6f);
                    break;
            }
        }

        void Behavior_PhaseChange()
        {
            HasContactDamage = true;
            Projectile.Center = QueenBee_Handler.GetOwnedCombCellCenter(Master);

            if (Master is null || !Master.active || Master.type != NPCID.QueenBee)
                Projectile.Kill();
            QueenBee_Anomaly masterBehavior = MasterBehavior;

            if (masterBehavior.CurrentPhase != QueenBee_Anomaly.Phase.PhaseChange_1To2)
                Projectile.Kill();
        }
    }

    public override void OnKill(int timeLeft)
    {
        QueenBee_Handler.SpawnGores(this);
    }

    #region 绘制与碰撞
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

    public override bool CanHitPlayer(Player target) => HasContactDamage;

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => new Hexagon(Projectile.Center, HexagonRadius * Projectile.scale, Projectile.rotation + TOMathUtils.PiOver6).Collides(targetHitbox);

    public override bool PreDraw(ref Color lightColor)
    {
        float innerOpacityMultiplier = BehaviorType switch
        {
            Behavior.Beehive => 0.75f,
            Behavior.BeeSwarm => 0.7f,
            Behavior.BeeSwarm2 => 0.7f,
            Behavior.BeeSwarm2_Fake => 0.6f,
            Behavior.BeeSwarm2_Safe => 0.8f,
            Behavior.BeeSwarm3 => 0.7f,
            Behavior.BeeSwarm3_Move => 0.65f,
            Behavior.PhaseChange => 0.7f,

            _ => 1f
        };

        DrawCell(Main.spriteBatch, Projectile.Center, Projectile.scale, Projectile.rotation, Color.White * Projectile.Opacity, innerOpacityMultiplier);
        return false;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => overPlayers.Add(index);
    #endregion 绘制与碰撞
}
