using CalamityAnomalies.DataStructures;
using CalamityAnomalies.VanillaOverrideEnums;
using CalamityMod.Dusts;
using Transoceanic.Framework.Helpers.AbstractionHandlers;

namespace CalamityAnomalies.Anomaly.EyeofCthulhu;

public sealed class EyeofCthulhu_Anomaly : AnomalyNPCBehavior
{
    #region 数据
    public enum Phase : byte
    {
        Initialize,
        Phase1,
        PhaseChange_1To2,
        Phase2,
        Phase2_2,
        Phase2_3,
        PhaseChange_2To3,
        Phase3,
        Phase3_2,
    }

    public enum Behavior : byte
    {
        Despawn = byte.MaxValue,

        None = 0,

        Phase1_Hover,
        Phase1_Charge,

        PhaseChange_1To2,

        Phase2_Hover,
        Phase2_NormalCharge,
        Phase2_RapidCharge,
        Phase2_Hover2,
        Phase2_HorizontalCharge,
        Phase2_ZenithSpin,

        PhaseChange_2To3,

        Phase3_Charge,
        Phase3_EyeSpin,
        Phase3_3,
    }

    public const float DespawnDistance = 6000f;
    public const float ProjectileOffset = 50f;
    public const int PhaseChangeTime_1To2 = 150;
    public const int PhaseChangeGateValue_1To2_1 = PhaseChangeTime_1To2 / 5 * 2;
    public const int PhaseChangeGateValue_1To2_2 = PhaseChangeTime_1To2 / 5 * 3;
    public const int PhaseChangeTime_2To3 = 195;
    public const int PhaseChangeGateValue_2To3_1 = 75;
    public const int PhaseChangeGateValue_2To3_2 = 120;

    public static float Phase2LifeRatio => Ultra ? 0.8f : 0.75f;
    public static float Phase2_2LifeRatio => Ultra ? 0.55f : 0.45f;
    public static float Phase2_3LifeRatio => Ultra ? 0.3f : 0.2f;
    public static float Phase3LifeRatio => Ultra ? 0.1f : 0f;
    public static float Phase3_2LifeRatio => Ultra ? 0.25f : 0f;

    private static readonly ProjectileDamageContainer _bloodDamage = new(30, 60, 75, 90, 84, 108);
    public static int BloodDamage => _bloodDamage.Value;

    private static readonly ProjectileDamageContainer _arenaDamage = new(100, 140, 210, 240, 240, 300);
    public static int ArenaDamage => _arenaDamage.Value;

    private static readonly ProjectileDamageContainer _bloodFlameDamage = new(50, 80, 102, 120, 120, 150);
    public static int BloodFlameDamage => _bloodFlameDamage.Value;

    public static readonly Color Phase3Color = Color.Lerp(Color.DarkRed, Color.Tomato, 0.4f);

    public float DamageMultiplier => Phase3 ? 1.5f : Phase2_2 ? 1.25f : 1f;

    public int SetDamage => (int)Math.Round(NPC.defDamage * DamageMultiplier);
    public int ReducedSetDamage => (int)Math.Round(NPC.defDamage * DamageMultiplier * 0.6f);

    public bool IsInPhase3Arena => Phase3 && ArenaProjectileAlive && NPC.Distance(ArenaProjectile.Center) < ArenaProjectile.GetModProjectile<EyeofCthulhuArena>().RealArenaRadius + 30f;

    public float EyeRotation => TOMathUtils.NormalizeWithPeriod((Target.Center - NPC.Center).ToRotation(-MathHelper.PiOver2));
    public float ActualRotation => NPC.rotation + MathHelper.PiOver2;
    public Vector2 DrawOffset => -new PolarVector2(24f, ActualRotation);

    public int RapidChargeTime => Phase2_3 ? 11 : 15;
    public int HorizontalChargeTime => Phase2_3 ? 30 : 35;

    public static readonly UnaryFunctionWithDomain PhaseChange_1To2_RotationSpeedFunction = UnaryFunctionWithDomain.Piecewise(
        (new MathInterval(float.NegativeInfinity, PhaseChangeGateValue_1To2_2, false, false), x => 0.5f * TOMathUtils.Interpolation.QuadraticEaseInOut(x / PhaseChangeGateValue_1To2_1)),
        (new MathInterval(PhaseChangeGateValue_1To2_2, float.PositiveInfinity, true, false), x => 0.5f * TOMathUtils.Interpolation.QuadraticEaseInOut((PhaseChangeTime_1To2 - x) / PhaseChangeGateValue_1To2_1))
    );
    public static readonly UnaryFunctionWithDomain PhaseChange_2To3_RotationSpeedFunction = UnaryFunctionWithDomain.Piecewise(
        (new MathInterval(float.NegativeInfinity, PhaseChangeGateValue_2To3_2, false, false), x => 0.5f * TOMathUtils.Interpolation.QuadraticEaseInOut(x / PhaseChangeGateValue_2To3_1)),
        (new MathInterval(PhaseChangeGateValue_2To3_2, float.PositiveInfinity, true, false), x => 0.5f * TOMathUtils.Interpolation.QuadraticEaseInOut((PhaseChangeTime_2To3 - x) / PhaseChangeGateValue_2To3_1))
        );

    public Phase CurrentPhase
    {
        get
        {
            Union32 union = AI_Union_0;
            return (Phase)union.byte0;
        }
        set
        {
            Union32 union = AI_Union_0;
            union.byte0 = (byte)value;
            AI_Union_0 = union;
        }
    }

    public bool ShouldEnterPhase3 => Ultra && NPC.LifeRatio < Phase3LifeRatio;
    public bool InvalidPhase2 => ShouldEnterPhase3 && !Phase3;
    public bool Phase2_2 => CurrentPhase is Phase.Phase2_2 or Phase.Phase2_3;
    public bool Phase2_3 => CurrentPhase == Phase.Phase2_3;
    public bool Phase3 => CurrentPhase is Phase.Phase3 or Phase.Phase3_2;
    public bool Phase3_2 => CurrentPhase == Phase.Phase3_2;

    public Behavior CurrentBehavior
    {
        get
        {
            Union32 union = AI_Union_0;
            return (Behavior)union.byte1;
        }
        set
        {
            Union32 union = AI_Union_0;
            union.byte1 = (byte)value;
            AI_Union_0 = union;
        }
    }

    public int CurrentAttackPhase
    {
        get => (int)NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    public bool Hover2DirectionIsNegative
    {
        get => AnomalyNPC.AnomalyAI32[0].bits[0];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].bits[0] != value)
            {
                AnomalyNPC.AnomalyAI32[0].bits[0] = value;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }

    public int Hover2Direction
    {
        get => Hover2DirectionIsNegative ? -1 : 1;
        set => Hover2DirectionIsNegative = value == -1;
    }

    public bool NextChargeTypeIsHorizontal
    {
        get => AnomalyNPC.AnomalyAI32[0].bits[1];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].bits[1] != value)
            {
                AnomalyNPC.AnomalyAI32[0].bits[1] = value;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }

    public int ServantSpawnCounter
    {
        get => AnomalyNPC.AnomalyAI32[3].i;
        set
        {
            if (AnomalyNPC.AnomalyAI32[3].i != value)
            {
                AnomalyNPC.AnomalyAI32[3].i = value;
                AnomalyNPC.AIChanged32[3] = true;
            }
        }
    }

    public int AttackCounter
    {
        get => AnomalyNPC.AnomalyAI32[4].i;
        set
        {
            if (AnomalyNPC.AnomalyAI32[4].i != value)
            {
                AnomalyNPC.AnomalyAI32[4].i = value;
                AnomalyNPC.AIChanged32[4] = true;
            }
        }
    }

    public int AttackCounter2
    {
        get => AnomalyNPC.AnomalyAI32[5].i;
        set
        {
            if (AnomalyNPC.AnomalyAI32[5].i != value)
            {
                AnomalyNPC.AnomalyAI32[5].i = value;
                AnomalyNPC.AIChanged32[5] = true;
            }
        }
    }

    public float Phase3ColorRatio
    {
        get => AnomalyNPC.AnomalyAI32[6].f;
        set
        {
            float temp = Math.Clamp(value, 0f, 1f);
            if (AnomalyNPC.AnomalyAI32[6].f != temp)
            {
                AnomalyNPC.AnomalyAI32[6].f = temp;
                AnomalyNPC.AIChanged32[6] = true;
            }
        }
    }

    public int UsedEyeIndex1
    {
        get => AnomalyNPC.AnomalyAI32[7].byte0;
        set
        {
            byte temp = (byte)TOMathUtils.NormalizeWithPeriod(value, 32);
            if (AnomalyNPC.AnomalyAI32[7].byte0 != temp)
            {
                AnomalyNPC.AnomalyAI32[7].byte0 = temp;
                AnomalyNPC.AIChanged32[7] = true;
            }
        }
    }

    public int UsedEyeIndex2
    {
        get => AnomalyNPC.AnomalyAI32[7].byte1;
        set
        {
            byte temp = (byte)(value % 32);
            if (AnomalyNPC.AnomalyAI32[7].byte1 != temp)
            {
                AnomalyNPC.AnomalyAI32[7].byte1 = temp;
                AnomalyNPC.AIChanged32[7] = true;
            }
        }
    }

    public int UsedEyeIndex3
    {
        get => AnomalyNPC.AnomalyAI32[7].byte2;
        set
        {
            byte temp = (byte)(value % 32);
            if (AnomalyNPC.AnomalyAI32[7].byte2 != temp)
            {
                AnomalyNPC.AnomalyAI32[7].byte2 = temp;
                AnomalyNPC.AIChanged32[7] = true;
            }
        }
    }

    public int UsedEyeIndex4
    {
        get => AnomalyNPC.AnomalyAI32[7].byte3;
        set
        {
            byte temp = (byte)(value % 32);
            if (AnomalyNPC.AnomalyAI32[7].byte3 != temp)
            {
                AnomalyNPC.AnomalyAI32[7].byte3 = temp;
                AnomalyNPC.AIChanged32[7] = true;
            }
        }
    }

    public Vector2 Phase3ArenaCenter
    {
        get => AnomalyNPC.AnomalyAI64[0].GetValue<Vector2>();
        set
        {
            if (AnomalyNPC.AnomalyAI64[0].GetValue<Vector2>() != value)
            {
                AnomalyNPC.AnomalyAI64[0].SetValue(value);
                AnomalyNPC.AIChanged64[0] = true;
            }
        }
    }

    #region 仆从
    public bool ServantLeftSpawned
    {
        get => AnomalyNPC.AnomalyAI32[0].bits[2];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].bits[2] != value)
            {
                AnomalyNPC.AnomalyAI32[0].bits[2] = value;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }
    /// <summary>
    /// 左仆从实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <c>DummyNPC</c>。
    /// </summary>
    public NPC ServantLeft
    {
        get => Main.npc[AnomalyNPC.AnomalyAI32[1].byte0];
        set
        {
            byte temp = (byte)value.whoAmI;
            if (AnomalyNPC.AnomalyAI32[1].byte0 != temp)
            {
                AnomalyNPC.AnomalyAI32[1].byte0 = temp;
                AnomalyNPC.AIChanged32[1] = true;
            }
        }
    }
    public bool ServantLeftAlive => ServantLeft.active && ServantLeft.ModNPC is BloodlettingServant && ServantLeft.Master == NPC;

    public bool ServantRightSpawned
    {
        get => AnomalyNPC.AnomalyAI32[0].bits[3];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].bits[3] != value)
            {
                AnomalyNPC.AnomalyAI32[0].bits[3] = value;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }
    /// <summary>
    /// 右仆从实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <c>DummyNPC</c>。
    /// </summary>
    public NPC ServantRight
    {
        get => Main.npc[AnomalyNPC.AnomalyAI32[1].byte1];
        set
        {
            byte temp = (byte)value.whoAmI;
            if (AnomalyNPC.AnomalyAI32[1].byte1 != temp)
            {
                AnomalyNPC.AnomalyAI32[1].byte1 = temp;
                AnomalyNPC.AIChanged32[1] = true;
            }
        }
    }
    public bool ServantRightAlive => ServantRight.active && ServantRight.ModNPC is BloodlettingServant && ServantRight.Master == NPC;

    public bool ArenaProjectileSpawned
    {
        get => AnomalyNPC.AnomalyAI32[0].bits[4];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].bits[4] != value)
            {
                AnomalyNPC.AnomalyAI32[0].bits[4] = value;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }
    /// <summary>
    /// 竞技场弹幕实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <c>DummyProjectile</c>。
    /// </summary>
    public Projectile ArenaProjectile
    {
        get => Main.projectile[AnomalyNPC.AnomalyAI32[2].i];
        set
        {
            int temp = value.whoAmI;
            if (AnomalyNPC.AnomalyAI32[2].i != temp)
            {
                AnomalyNPC.AnomalyAI32[2].i = temp;
                AnomalyNPC.AIChanged32[2] = true;
            }
        }
    }
    public bool ArenaProjectileAlive => ArenaProjectile.active && ArenaProjectile.ModProjectile is EyeofCthulhuArena arena && arena.Master == NPC;
    public EyeofCthulhuArena ArenaModProjectile => ArenaProjectile.GetModProjectile<EyeofCthulhuArena>();
    #endregion 仆从

    /* 数组使用说明
     * 
     * NPC.ai
     *   [0]. (Union)
     *       byte0 CurrentPhase
     *       byte1 CurrentBehavior
     *   [1] CurrentAttackPhase
     * 
     * AnomalyAI32
     *   [0].
     *       bits[0] Hover2DirectionIsNegative
     *       bits[1] NextChargeTypeIsHorizontal
     *       bits[2] ServantLeftSpawned
     *       bits[3] ServantRightSpawned
     *       bits[4] ArenaProjectileSpawned
     *   [1].
     *       byte0 ServantLeft
     *       byte1 ServantRight
     *   [2].i ArenaProjectile
     *   [3].i ServantSpawnCounter
     *   [4].i AttackCounter
     *   [5].i AttackCounter2
     *   [6].f Phase3ColorRatio
     *   [7].
     *       byte0 UsedEyeIndex1
     *       byte1 UsedEyeIndex2
     *       byte2 UsedEyeIndex3
     *       byte3 UsedEyeIndex4
     * 
     * AnomalyAI64
     *   [0] (Vector2) Phase3ArenaCenter
     */
    #endregion 数据

    public override int ApplyingType => NPCID.EyeofCthulhu;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior method) => method switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        _ => true,
    };

    public override void SetStaticDefaults()
    {
        NPCID.Sets.TrailingMode[ApplyingType] = 3;
        NPCID.Sets.TrailCacheLength[ApplyingType] = 5;
    }

    public override void SetDefaults()
    {
        ServantLeft = NPC.DummyNPC;
        ServantRight = NPC.DummyNPC;
        ArenaProjectile = Projectile.DummyProjectile;
    }

    public override bool PreAI()
    {
        bool valid = Phase3
            ? NPC.TargetIfInvalid(true, p => ArenaProjectileAlive && ArenaModProjectile.ArenaRing.CircleContains(p.Hitbox, true, true))
            : NPC.TargetClosestIfInvalid(true, DespawnDistance);

        if (CurrentBehavior == Behavior.Despawn || !valid)
        {
            CurrentBehavior = Behavior.Despawn;

            NPC.dontTakeDamage = true;
            StopMovement();

            if (NPC.timeLeft > 10)
                NPC.timeLeft = 10;

            Timer5++;
            if (Timer5 >= 15)
            {
                NPC.active = false;
                NPC.netUpdate = true;
            }

            return false;
        }
        else if (Timer5 > 0)
            Timer5--;

        if (Main.rand.NextBool(5))
        {
            Dust.NewDustAction(NPC.Center, NPC.width, NPC.height / 2, DustID.Blood, new Vector2(NPC.velocity.X, 2f), d =>
            {
                d.velocity.X *= 0.5f;
                d.velocity.Y *= 0.1f;
            });
        }

        switch (CurrentPhase)
        {
            case Phase.Initialize:
                CurrentPhase = Phase.Phase1;
                CurrentBehavior = Behavior.Phase1_Hover;
                break;
            case Phase.Phase1:
                Phase1AI();
                break;
            case Phase.PhaseChange_1To2:
                PhaseChange_1To2();
                break;
            case >= Phase.Phase2 and <= Phase.Phase2_3:
                Phase2AI();
                break;
            case Phase.PhaseChange_2To3:
                PhaseChange_2To3();
                break;
            case Phase.Phase3 or Phase.Phase3_2:
                Phase3AI();
                break;
        }

        if (Main.dedServ)
            NPC.netUpdate = true;

        return false;

        #region 行为函数
        bool CanShootProjectile() => Vector2.IncludedAngle(new PolarVector2(ActualRotation), Target.Center - NPC.Center) < MathHelper.ToRadians(Ultra ? 30f : 20f)
            && Vector2.Distance(NPC.Center, Target.Center) > 160f;

        void NormalUpdateRotation(float acceleration, float? targetRotationOverride = null) =>
            EyeofCthulhu_Handler.UpdateRotation(ref NPC.rotation, targetRotationOverride ?? EyeRotation, acceleration);

        void StopMovement()
        {
            NPC.velocity *= 0.93f;

            if (Math.Abs(NPC.velocity.X) < 0.1f)
                NPC.velocity.X = 0f;
            if (Math.Abs(NPC.velocity.Y) < 0.1f)
                NPC.velocity.Y = 0f;
        }

        void TeleportTo(Vector2 destination, float timer, int fadeTime, int appearTime)
        {
            float completionRatio = timer <= fadeTime ? timer / fadeTime : (timer - fadeTime) / appearTime;
            switch (completionRatio)
            {
                case < 1f:
                    NPC.Opacity = 1f - completionRatio;
                    break;
                case 1f:
                    NPC.Opacity = 0f;
                    NPC.Center = destination;
                    NPC.velocity = Vector2.Zero;
                    break;
                case > 1f:
                    NPC.Opacity = completionRatio - 1f;
                    break;
            }
        }

        void TrySpawnZenithSpinServant(int timer)
        {
            int servantSpawnGateValue = 5;
            if (Main.zenithWorld && timer >= 0 && timer % servantSpawnGateValue == 0)
            {
                PolarVector2 servantVelocity = Main.rand.NextPolarVector2(6f, 7.5f);

                NPC.NewNPCAction(NPC.GetSource_FromAI(), NPC.Center, NPCID.ServantofCthulhu, action: n =>
                {
                    n.velocity = servantVelocity;
                    n.Master = NPC;
                    ServantSpawnCounter++;
                    SpawnServantAction(n);
                });

                if (TOSharedData.LegendaryMode)
                {
                    float radian = MathHelper.ToRadians(15);
                    Projectile.RotatedProj(3, radian, NPC.GetSource_FromAI(), NPC.Center, servantVelocity.RotatedBy(-radian) * 4f, ProjectileID.BloodNautilusShot, BloodDamage, 0f, action: p => p.timeLeft = Main.rand.Next(180, 240));
                }
            }
        }

        void SpawnServantAction(NPC n)
        {
            if (Main.zenithWorld)
                n.ai[2] = 2.5f;
        }

        #region 1、2阶段
        void Phase1AI()
        {
            switch (CurrentBehavior)
            {
                case Behavior.Phase1_Hover:
                    Hover();
                    break;
                case Behavior.Phase1_Charge:
                    Charge();
                    break;
                default:
                    SelectNextAttack();
                    break;
            }

            void SelectNextAttack()
            {
                CurrentAttackPhase = 0;
                Timer1 = 0;
                Timer2 = 0;
                switch (CurrentBehavior)
                {
                    case Behavior.Phase1_Hover:
                        AttackCounter = 0;
                        CurrentBehavior = Behavior.Phase1_Charge;
                        break;
                    case Behavior.Phase1_Charge:
                        AttackCounter++;
                        NPC.rotation = EyeRotation;
                        if (AttackCounter >= 3) //冲刺共3次
                        {
                            NPC.damage = 0;
                            AttackCounter = 0;
                            CurrentBehavior = Behavior.Phase1_Hover;
                        }
                        break;
                    default:
                        CurrentBehavior = Phase2_2 ? Behavior.Phase2_Hover2 : Behavior.Phase2_Hover;
                        break;
                }
            }

            bool CheckPhaseChange()
            {
                if (NPC.LifeRatio >= Phase2LifeRatio)
                    return false;

                //进入阶段转换
                NPC.damage = 0;
                CurrentPhase = Phase.PhaseChange_1To2;
                CurrentBehavior = Behavior.PhaseChange_1To2;
                CurrentAttackPhase = 0;
                Timer1 = -60; //60帧缓冲时间
                AttackCounter = 0;

                return true;
            }

            void Hover()
            {
                NPC.damage = 0;

                float hoverSpeed = (Ultra ? 22.5f : 15f) + 7.5f * NPC.LostLifeRatio;
                float hoverAcceleration = (Ultra ? 0.45f : 0.25f) + 0.15f * NPC.LostLifeRatio;

                Vector2 hoverDestination = Target.Center - Vector2.UnitY * 400f;
                Vector2 idealVelocity = NPC.GetVelocityTowards(hoverDestination, hoverSpeed);
                NPC.SimpleFlyMovement(idealVelocity, hoverAcceleration);

                Timer1++;

                if (Timer1 >= (Ultra ? 125 : 150))
                    SelectNextAttack();
                else if (NPC.WithinRange(hoverDestination, 1280f))
                {
                    int servantSpawnGateValue = Ultra ? 10 : 12;

                    if (Timer1 % servantSpawnGateValue == 0 && CanShootProjectile())
                    {
                        int num = Timer1 / servantSpawnGateValue;
                        NPC.rotation = EyeRotation;

                        Vector2 direction = (Target.Center - NPC.Center).SafeNormalize();
                        Vector2 servantSpawnVelocity = direction * 10f;
                        float projectileSpeed = (Ultra ? 15f : 12f);
                        Vector2 servantSpawnCenter = NPC.Center + servantSpawnVelocity.ToCustomLength(ProjectileOffset);

                        int maxServantsAlive = 2;
                        int maxServantsTotal = 8;
                        bool buff = num is 3 or 6 or 9 or 12;
                        bool shouldSpawnServant = buff && ServantSpawnCounter < maxServantsTotal && NPC.ActiveNPCs.Count(n => n.type == NPCID.ServantofCthulhu && n.Master == NPC) < maxServantsAlive;

                        if (shouldSpawnServant)
                        {
                            SoundEngine.PlaySound(SoundID.NPCHit1, servantSpawnCenter);
                            for (int i = 0; i < 10; i++)
                                Dust.NewDustAction(servantSpawnCenter, 20, 20, DustID.Blood, servantSpawnVelocity * 0.4f);
                        }

                        if (TOSharedData.GeneralClient)
                        {
                            if (shouldSpawnServant)
                            {
                                NPC.NewNPCAction(NPC.GetSource_FromAI(), servantSpawnCenter, NPCID.ServantofCthulhu, action: n =>
                                {
                                    n.velocity = servantSpawnVelocity;
                                    n.Master = NPC;
                                    ServantSpawnCounter++;
                                    SpawnServantAction(n);
                                });
                            }

                            int projectileAmount = buff ? (Ultra ? 3 : 2) : 1;
                            EyeofCthulhu_Handler.ShootProjectile(NPC, ProjectileID.BloodNautilusShot, BloodDamage, projectileSpeed, projectileAmount, MathHelper.ToRadians(Ultra ? 12f : 10f), p => p.timeLeft = 600);
                        }
                    }
                }

                NormalUpdateRotation(0.12f);
                CheckPhaseChange();
            }

            void Charge()
            {
                if (CurrentAttackPhase == 0) //冲刺
                {
                    NPC.damage = SetDamage;

                    float chargeSpeed = (Ultra ? 17.5f : 12f) + 6f * NPC.LostLifeRatio;
                    NPC.SetVelocityandRotation(NPC.GetVelocityTowards(Target.Center, chargeSpeed), -MathHelper.PiOver2);

                    if (AttackCounter >= 1)
                        chargeSpeed *= 1.1f;
                    if (AttackCounter >= 2)
                        chargeSpeed *= 1.1f;

                    CurrentAttackPhase = 1;
                }
                else //冲刺中
                {
                    NPC.damage = SetDamage;

                    int chargeDelay = (Ultra ? 40 : 70) - (int)Math.Round(40f * NPC.LostLifeRatio);
                    float slowDownGateValue = chargeDelay * 0.9f;

                    Timer1++;
                    if (Timer1 >= slowDownGateValue)
                    {
                        NPC.damage = 0;
                        NPC.velocity *= Utils.Remap(NPC.LifeRatio, Phase2LifeRatio, 1f, 0.76f, 0.88f);
                        if (Ultra)
                            NPC.velocity *= 0.99f;

                        if (Math.Abs(NPC.velocity.X) < 0.1f)
                            NPC.velocity.X = 0f;
                        if (Math.Abs(NPC.velocity.Y) < 0.1f)
                            NPC.velocity.Y = 0f;
                    }
                    else
                        NPC.VelocityToRotation(-MathHelper.PiOver2);

                    if (Timer1 >= chargeDelay && !CheckPhaseChange())
                        SelectNextAttack();
                }
            }
        }

        void PhaseChange_1To2()
        {
            Timer1++;

            NPC.damage = 0;
            StopMovement();
            NPC.rotation += PhaseChange_1To2_RotationSpeedFunction.Process(Timer1);

            Dust.NewDustAction(NPC.Center, NPC.width, NPC.height, DustID.Blood, new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f)));

            //仆从（GFB）、弹幕、粒子

            const int offset = 0;
            int adjustedTimer = Timer1 - offset;
            int num = adjustedTimer is >= PhaseChangeGateValue_1To2_2 - 50 and <= PhaseChangeGateValue_1To2_2 && adjustedTimer % 10 == 0 ? (adjustedTimer + 60 - PhaseChangeGateValue_1To2_2) / 10 : 0; //1~6
            if (num > 0)
            {
                if (Ultra || num % 2 == 0)
                {
                    bool lastAttack = num == 6;

                    float projectileMaxSpeed = 17.5f;
                    Vector2 projectileVelocity = new PolarVector2(projectileMaxSpeed, NPC.rotation + Main.rand.NextFloat(-0.15f, 0.15f));

                    int projectileAmountOver4 = Ultra ? num switch
                    {
                        1 or 2 => 1,
                        3 or 4 or 5 => 2,
                        6 => 8,
                        _ => 0
                    } : num switch
                    {
                        2 => 1,
                        4 => 2,
                        6 => 6,
                        _ => 0
                    };

                    int particleAmount = projectileAmountOver4 * 8;
                    for (int i = 0; i < particleAmount; i++)
                        EyeofCthulhu_Handler.SpawnOrbParticle(NPC.Center, lastAttack ? Main.rand.NextFloat(5f, 10f) : Main.rand.NextFloat(3f, 5f), Main.rand.Next(20, 30), Main.rand.NextFloat(0.5f, 1f));

                    EyeofCthulhu_Handler.ShootEyeProjectile(NPC, ProjectileID.BloodShot, BloodDamage, projectileVelocity, projectileAmountOver4, p => p.timeLeft = 300);

                    if (lastAttack)
                        EyeofCthulhu_Handler.SpawnEyeParticle(NPC, projectileVelocity * 1.4f);
                }
            }

            TrySpawnZenithSpinServant(adjustedTimer);

            switch (Timer1)
            {
                case PhaseChangeGateValue_1To2_2:
                    SoundEngine.PlaySound(SoundID.NPCHit1, NPC.Center);
                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

                    for (int phase2Gore = 0; phase2Gore < 2; phase2Gore++)
                    {
                        for (int type = 8; type >= 6; type--)
                            Gore.NewGoreAction(NPC.GetSource_FromAI(), NPC.position, new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f)), type);
                    }

                    for (int i = 0; i < 20; i++)
                        Dust.NewDustAction(NPC.Center, NPC.width, NPC.height, DustID.Blood, new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f)));

                    if (!Main.zenithWorld)
                    {
                        //非GFB世界
                        //生成流血仆从
                        //start参数设置为NPC.whoAmI，以确保发送命令时仆从能够同帧执行

                        for (int i = 0; i < 2; i++)
                        {
                            NPC.NewNPCAction<BloodlettingServant>(NPC.GetSource_FromAI(), NPC.Center, NPC.whoAmI, n =>
                            {
                                n.Master = NPC;
                                BloodlettingServant modN = n.GetModNPC<BloodlettingServant>();
                                modN.Place = i == 0 ? BloodlettingServant.ServantPlace.Left : BloodlettingServant.ServantPlace.Right;
                                modN.PositionRotation = NPC.rotation;

                                if (i == 0)
                                {
                                    ServantLeft = n;
                                    ServantLeftSpawned = true;
                                }
                                else
                                {
                                    ServantRight = n;
                                    ServantRightSpawned = true;
                                }
                            });
                        }
                    }
                    else
                    {
                        //GFB世界：生成三个流血仆从（原灾厄AI）

                        for (int i = 0; i < 3; i++)
                            NPC.NewNPCAction<BloodlettingServant>(NPC.GetSource_FromAI(), NPC.Center, NPC.whoAmI, n => n.velocity = Main.rand.NextPolarVector2(10f, 15f));
                    }

                    CurrentAttackPhase = 1;
                    break;
                case PhaseChangeTime_1To2: //进入二阶段
                    CurrentPhase = Phase.Phase2;
                    CurrentBehavior = Behavior.Phase2_Hover;
                    CurrentAttackPhase = 0;
                    Timer1 = -15; //15帧缓冲时间
                    break;
            }
        }

        void Phase2AI()
        {
            NPC.defense = 0;

            switch (CurrentBehavior)
            {
                case Behavior.Phase2_Hover:
                    Hover();
                    break;
                case Behavior.Phase2_NormalCharge:
                    NormalCharge();
                    break;
                case Behavior.Phase2_RapidCharge:
                    RapidCharge();
                    break;
                case Behavior.Phase2_Hover2:
                    Hover2();
                    break;
                case Behavior.Phase2_HorizontalCharge:
                    HorizontalCharge();
                    break;
                case Behavior.Phase2_ZenithSpin:
                    if (Main.zenithWorld)
                        ZenithSpin();
                    else
                        SelectNextAttack();
                    break;
                default:
                    SelectNextAttack();
                    break;
            }

            if (Main.zenithWorld)
                ZenithFlame();

            void SelectNextAttack()
            {
                CurrentAttackPhase = 0;
                Timer1 = 0;
                Timer2 = 0;
                switch (CurrentBehavior)
                {
                    case Behavior.Phase2_Hover:
                        CurrentBehavior = Phase2_2 ? Behavior.Phase2_RapidCharge : Behavior.Phase2_NormalCharge;
                        break;
                    case Behavior.Phase2_Hover2:
                        CurrentBehavior = NextChargeTypeIsHorizontal ? Behavior.Phase2_HorizontalCharge : Behavior.Phase2_RapidCharge;
                        break;
                    case Behavior.Phase2_NormalCharge:
                        AttackCounter++;
                        NPC.rotation = EyeRotation;
                        if (AttackCounter >= 3) //常规冲刺共3次
                        {
                            NPC.damage = 0;
                            AttackCounter = 0;
                            CurrentBehavior = Behavior.Phase2_RapidCharge;
                        }
                        break;
                    case Behavior.Phase2_RapidCharge:
                        AttackCounter++;
                        if (AttackCounter >= (Phase2_3 ? Main.rand.Next(7, 10) : Main.rand.Next(4, 7)))
                        {
                            NPC.damage = ReducedSetDamage;
                            if (Phase2_2)
                            {
                                if (Main.zenithWorld)
                                {
                                    CurrentBehavior = Behavior.Phase2_ZenithSpin;
                                    Timer1 = -15; //15帧缓冲时间
                                }
                                else
                                {
                                    CurrentBehavior = Behavior.Phase2_Hover2;
                                    NextChargeTypeIsHorizontal = true;
                                    SendCommandToServants(BehaviorCommand_Servant.IncreaseFollowDistance);
                                }
                            }
                            else
                                CurrentBehavior = Behavior.Phase2_Hover;
                            AttackCounter = 0;
                        }
                        break;
                    case Behavior.Phase2_HorizontalCharge:
                        AttackCounter2++;
                        if (AttackCounter2 > 3 || Main.rand.NextProbability(Phase2_3 ? 0.4f : 0.6f))
                        {
                            AttackCounter2 = 0;
                            NextChargeTypeIsHorizontal = false;

                            SendCommandToServants(BehaviorCommand_Servant.ReduceFollowDistance);
                        }
                        else
                            NextChargeTypeIsHorizontal = true;
                        CurrentBehavior = Behavior.Phase2_Hover2;
                        break;
                    case Behavior.Phase2_ZenithSpin:
                        CurrentBehavior = Behavior.Phase2_Hover2;
                        NextChargeTypeIsHorizontal = true;
                        Timer1 = -10; //10帧缓冲时间
                        SendCommandToServants(BehaviorCommand_Servant.IncreaseFollowDistance);
                        break;
                }

                ExecuteActionToServants((n, modN) => modN.ShouldUsePhase2Frame = CurrentBehavior is Behavior.Phase2_NormalCharge or Behavior.Phase2_RapidCharge or Behavior.Phase2_HorizontalCharge);
            }


            bool CheckPhaseChange()
            {
                if (InvalidPhase2)
                {
                    //进入阶段转换
                    NPC.damage = 0;
                    CurrentPhase = Phase.PhaseChange_2To3;
                    CurrentBehavior = Behavior.PhaseChange_2To3;
                    CurrentAttackPhase = 0;
                    Timer1 = -60; //60帧缓冲时间
                    AttackCounter = 0;
                    return true;
                }
                else if (NPC.LifeRatio < Phase2_3LifeRatio)
                    CurrentPhase = Phase.Phase2_3;
                else if (NPC.LifeRatio < Phase2_2LifeRatio)
                    CurrentPhase = Phase.Phase2_2;

                return false;
            }

            void Hover()
            {
                Timer1++;

                NPC.damage = ReducedSetDamage;

                float hoverSpeed = 5.5f + 3f * (Phase2LifeRatio - NPC.LifeRatio);
                float hoverAcceleration = 0.06f + 0.02f * (Phase2LifeRatio - NPC.LifeRatio);

                hoverSpeed += (Ultra ? 10f : 5f) + 5.5f * (Phase2LifeRatio - NPC.LifeRatio);
                hoverAcceleration += (Ultra ? 0.1f : 0.05f) + 0.06f * (Phase2LifeRatio - NPC.LifeRatio);

                Vector2 hoverDestination = Target.Center - Vector2.UnitY * 400f;
                float distanceFromHoverDestination = NPC.Distance(hoverDestination);

                if (distanceFromHoverDestination > 400f)
                {
                    hoverSpeed += 1.25f;
                    hoverAcceleration += 0.075f;
                    if (distanceFromHoverDestination > 600f)
                    {
                        hoverSpeed += 1.25f;
                        hoverAcceleration += 0.075f;
                        if (distanceFromHoverDestination > 800f)
                        {
                            hoverSpeed += 1.25f;
                            hoverAcceleration += 0.075f;
                        }
                    }
                }

                float phaseLimit = 160f - 150f * (Phase2LifeRatio - NPC.LifeRatio);
                Vector2 idealHoverVelocity = (hoverDestination - NPC.Center).ToCustomLength(hoverSpeed);
                NPC.SimpleFlyMovement(idealHoverVelocity, hoverAcceleration);

                int projectileGateValue = 60;

                if (Timer1 > 0 && Timer1 % projectileGateValue == 0 && CanShootProjectile())
                {
                    if (TOSharedData.GeneralClient)
                    {
                        int type = ProjectileID.BloodNautilusShot;
                        int damage = BloodDamage;
                        int numProj = 5;
                        float rotation = MathHelper.ToRadians(10f);
                        float projectileSpeed = (Ultra ? 20f : 17f) + 3f * (NPC.LifeRatio - Phase2LifeRatio);
                        EyeofCthulhu_Handler.ShootProjectile(NPC, type, damage, projectileSpeed, numProj, rotation, p => p.timeLeft = 600);
                    }

                    SendCommandToServants(BehaviorCommand_Servant.ShootBlood);
                }

                if (Timer1 >= phaseLimit && NPC.Distance(Target.Center) > 320f)
                    SelectNextAttack();

                NormalUpdateRotation(0.12f);
                CheckPhaseChange();
            }

            void NormalCharge()
            {
                if (CurrentAttackPhase == 0) //冲刺
                {
                    NPC.damage = SetDamage;

                    SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
                    NPC.rotation = EyeRotation;

                    float chargeSpeed = (Ultra ? 20f : 15f) + 6f * (Phase2LifeRatio - NPC.LifeRatio);

                    if (AttackCounter >= 1)
                        chargeSpeed *= 1.1f;
                    if (AttackCounter >= 2)
                        chargeSpeed *= 1.1f;

                    Vector2 chargeVelocity = NPC.GetVelocityTowards(Target, chargeSpeed);

                    NPC.SetVelocityandRotation(chargeVelocity, -MathHelper.PiOver2);
                    CurrentAttackPhase++;
                }
                else //冲刺中
                {
                    NPC.damage = SetDamage;

                    int phase2ChargeDelay = 60 - (int)Math.Round(35f * (Phase2LifeRatio - NPC.LifeRatio));

                    float slowDownGateValue = phase2ChargeDelay * 0.95f;

                    Timer1++;
                    if (Timer1 >= slowDownGateValue)
                    {
                        NPC.damage = ReducedSetDamage;

                        float decelerationScalar = Utils.GetLerpValue(Phase2_2LifeRatio, Phase2LifeRatio, NPC.LifeRatio, true);

                        NPC.velocity *= MathHelper.Lerp(0.6f, 0.7f, decelerationScalar);

                        if (Math.Abs(NPC.velocity.X) < 0.1f)
                            NPC.velocity.X = 0f;
                        if (Math.Abs(NPC.velocity.Y) < 0.1f)
                            NPC.velocity.Y = 0f;
                    }
                    else
                        NPC.VelocityToRotation(-MathHelper.PiOver2);

                    if (Timer1 >= phase2ChargeDelay && !CheckPhaseChange())
                        SelectNextAttack();
                }
            }

            void RapidCharge()
            {
                switch (CurrentAttackPhase) //冲刺
                {
                    case 0:
                        NPC.damage = SetDamage;

                        SoundEngine.PlaySound(SoundID.ForceRoarPitched, NPC.Center);

                        float baseChargeSpeed = 33f;
                        float speedBoost = 12f * (Phase2_2LifeRatio - NPC.LifeRatio);
                        float speedMultiplier = Ultra ? 1.3f : 1f;
                        float phase2_3Multiplier = Phase2_3 ? 1.2f : 1f;
                        float finalChargeSpeed = (baseChargeSpeed + speedBoost) * speedMultiplier * phase2_3Multiplier;

                        Vector2 eyeChargeDirection = NPC.Center;
                        float distanceX = Target.Center.X - eyeChargeDirection.X;
                        float distanceY = Target.Center.Y - eyeChargeDirection.Y;
                        float targetVelocity = Math.Abs(Target.velocity.X) + Math.Abs(Target.velocity.Y) / 4f;

                        if (targetVelocity < 2f)
                            targetVelocity = 2f;
                        if (targetVelocity > 6f)
                            targetVelocity = 6f;

                        if (AttackCounter == 0)
                        {
                            targetVelocity *= 4f;
                            finalChargeSpeed *= 1.3f;
                        }

                        distanceX -= Target.velocity.X * targetVelocity;
                        distanceY -= Target.velocity.Y * targetVelocity / 4f;

                        float targetDistance = MathF.Sqrt(distanceX * distanceX + distanceY * distanceY);
                        float targetDistCopy = targetDistance;

                        targetDistance = finalChargeSpeed / targetDistance;
                        NPC.velocity.X = distanceX * targetDistance;
                        NPC.velocity.Y = distanceY * targetDistance;

                        if (targetDistCopy < 100f)
                        {
                            if (Math.Abs(NPC.velocity.X) > Math.Abs(NPC.velocity.Y))
                            {
                                float absoluteXVel = Math.Abs(NPC.velocity.X);
                                float absoluteYVel = Math.Abs(NPC.velocity.Y);

                                if (NPC.Center.X > Target.Center.X)
                                    absoluteYVel *= -1f;
                                if (NPC.Center.Y > Target.Center.Y)
                                    absoluteXVel *= -1f;

                                NPC.velocity.X = absoluteYVel;
                                NPC.velocity.Y = absoluteXVel;
                            }
                        }
                        else if (Math.Abs(NPC.velocity.X) > Math.Abs(NPC.velocity.Y))
                        {
                            float absoluteEyeVel = (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y)) / 2f;
                            float absoluteEyeVelBackup = absoluteEyeVel;

                            if (NPC.Center.X > Target.Center.X)
                                absoluteEyeVelBackup *= -1f;
                            if (NPC.Center.Y > Target.Center.Y)
                                absoluteEyeVel *= -1f;

                            NPC.velocity.X = absoluteEyeVelBackup;
                            NPC.velocity.Y = absoluteEyeVel;
                        }

                        NPC.VelocityToRotation(-MathHelper.PiOver2);

                        CurrentAttackPhase = 1;
                        break;
                    case 1:
                        NPC.damage = SetDamage;

                        Timer1++;

                        if (Timer1 >= RapidChargeTime && (NPC.Distance(Target.Center) >= 200f || Timer2 > 0))
                        {
                            Timer2++;

                            NPC.damage = ReducedSetDamage;
                            StopMovement();
                        }
                        else
                            NPC.VelocityToRotation(-MathHelper.PiOver2);

                        if (Timer2 >= 13 && !CheckPhaseChange())
                            SelectNextAttack();
                        break;
                }
            }

            void Hover2()
            {
                switch (CurrentAttackPhase)
                {
                    case 0:
                        int direction = NextChargeTypeIsHorizontal ? Math.Sign(AttackCounter2 == 0 ? -Target.velocity.X : NPC.Center.X - Target.Center.X) : Math.Sign(NPC.Center.Y - Target.Center.Y);
                        if (direction == 0)
                            direction = Main.rand.NextBool(2) ? 1 : -1;
                        Hover2Direction = direction;
                        CurrentAttackPhase = 1;
                        break;
                    case 1:
                        Timer1++;

                        NPC.damage = ReducedSetDamage;

                        float baseHoverSpeed = 22.5f;
                        float baseHoverAcceleration = 0.45f;

                        float speedBoost = 10f * (Phase2_2LifeRatio - NPC.LifeRatio);
                        float accelerationBoost = 0.3f * (Phase2_2LifeRatio - NPC.LifeRatio);
                        float speedMultiplier = Ultra ? 1.25f : 1f;

                        float hoverSpeed = (baseHoverSpeed + speedBoost) * speedMultiplier;
                        float hoverAcceleration = baseHoverAcceleration + accelerationBoost * speedMultiplier;

                        float timeGateValue = 100f - 80f * (Phase2_2LifeRatio - NPC.LifeRatio);

                        if (Timer1 > timeGateValue)
                        {
                            float velocityScalar = Timer1 - timeGateValue;
                            hoverSpeed += velocityScalar * 0.075f;
                            hoverAcceleration += velocityScalar * 0.004f;
                        }

                        Vector2 hoverDestination = NextChargeTypeIsHorizontal ? Target.Center + new Vector2(800f * Hover2Direction, 0f)
                            : Target.Center + new Vector2(0f, 480f * Hover2Direction);
                        Vector2 idealHoverVelocity = NPC.GetVelocityTowards(hoverDestination, hoverSpeed);
                        NPC.SimpleFlyMovement(idealHoverVelocity, hoverAcceleration);

                        //弹幕
                        int projectileGateValue = NextChargeTypeIsHorizontal ? 20 : 30;
                        int maxProjectileSpawnsPerAttack = NextChargeTypeIsHorizontal ? (AttackCounter == 0 ? (Ultra ? 3 : 2) : Ultra ? 2 : 1) : 1;
                        int projectileDelay = 10;
                        int adjustedTimer1 = Timer1 - projectileDelay;
                        if (adjustedTimer1 % projectileGateValue == 0)
                        {
                            int num = adjustedTimer1 / projectileGateValue;
                            if (num >= 0 && num < maxProjectileSpawnsPerAttack && TOSharedData.GeneralClient && CanShootProjectile() && Timer2 == 0)
                            {
                                bool buff = Ultra && NextChargeTypeIsHorizontal && AttackCounter2 == 0;
                                int amount = buff ? 5 : 3;
                                float halfRange = MathHelper.ToRadians(buff ? 45f : 22.5f);
                                EyeofCthulhu_Handler.ShootProjectile(NPC, ProjectileID.BloodNautilusShot, BloodDamage, 20f, amount, halfRange, p => p.timeLeft = 600);
                            }

                            if (Main.zenithWorld)
                            {
                                int servantCount = 0;
                                int bloodlettingServantCount = 0;

                                foreach (NPC n in NPC.ActiveNPCs)
                                {
                                    if (n.type == NPCID.ServantofCthulhu)
                                        servantCount++;
                                    else if (n.ModNPC is BloodlettingServant)
                                        bloodlettingServantCount++;
                                }

                                if (bloodlettingServantCount < 3)
                                {
                                    SoundEngine.PlaySound(SoundID.NPCDeath13, NPC.Center);
                                    NPC.NewNPCAction<BloodlettingServant>(NPC.GetSource_FromAI(), NPC.Center, NPC.whoAmI, n => n.velocity = NPC.velocity);
                                }
                                else if (servantCount < 10)
                                {
                                    SoundEngine.PlaySound(SoundID.NPCDeath13, NPC.Center);
                                    NPC.NewNPCAction(NPC.GetSource_FromAI(), NPC.Center, NPCID.ServantofCthulhu, NPC.whoAmI, n => n.velocity = NPC.velocity);
                                }
                            }
                        }

                        if (Timer1 >= timeGateValue)
                        {
                            float requiredDistanceForHorizontalCharge = 200f;
                            Vector2 distance = hoverDestination - NPC.Center;
                            if (!NextChargeTypeIsHorizontal)
                                distance.X /= 20f;
                            if (distance.Length() < requiredDistanceForHorizontalCharge)
                            {
                                Timer2++;
                                int delay = NextChargeTypeIsHorizontal ? 12 : 4;
                                if (Timer2 > delay)
                                    SelectNextAttack();
                            }
                        }

                        break;
                }

                NormalUpdateRotation(0.16f);
                CheckPhaseChange();
            }

            void HorizontalCharge()
            {
                NPC.SpawnAfterimage(5, NPC.GetAlpha(Color.Red), DrawOffset);

                switch (CurrentAttackPhase)
                {
                    case 0:
                        NPC.damage = SetDamage;

                        float baseChargeSpeed = 33f;
                        float speedBoost = 12f * (Phase2_2LifeRatio - NPC.LifeRatio);
                        float speedMultiplier = Ultra ? 1.25f : 1f;
                        float chargeSpeed = (baseChargeSpeed + speedBoost) * speedMultiplier;

                        NPC.SetVelocityandRotation(NPC.GetVelocityTowards(Target.Center, chargeSpeed), -MathHelper.PiOver2);
                        CurrentAttackPhase++;
                        break;
                    case 1:
                        NPC.damage = SetDamage;

                        if (Timer1 == 0)
                            SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);

                        Timer1++;

                        if (Timer1 >= HorizontalChargeTime && (NPC.Distance(Target.Center) >= 200f || Timer2 > 0))
                        {
                            Timer2++;
                            NPC.damage = ReducedSetDamage;
                            StopMovement();
                        }
                        else
                            NPC.VelocityToRotation(-MathHelper.PiOver2);

                        if (Timer2 > 15 && !CheckPhaseChange())
                            SelectNextAttack();
                        break;
                }
            }

            void ZenithSpin()
            {
                NPC.rotation += PhaseChange_1To2_RotationSpeedFunction.Process(Timer1);

                TrySpawnZenithSpinServant(Timer1 - 1);

                switch (Timer1)
                {
                    case PhaseChangeGateValue_1To2_2:
                        SoundEngine.PlaySound(SoundID.NPCHit1, NPC.Center);
                        SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

                        for (int phase2Gore = 0; phase2Gore < 2; phase2Gore++)
                        {
                            for (int type = 8; type >= 6; type--)
                                Gore.NewGoreAction(NPC.GetSource_FromAI(), NPC.position, new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f)), type);
                        }

                        for (int i = 0; i < 20; i++)
                            Dust.NewDustAction(NPC.Center, NPC.width, NPC.height, DustID.Blood, new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f)));

                        int bloodLettingServantCount = 0;

                        foreach (NPC n in NPC.ActiveNPCs)
                        {
                            if (n.ModNPC is BloodlettingServant)
                                bloodLettingServantCount++;
                        }

                        int bloodlettingServantSpawnAmount = 3 - bloodLettingServantCount;
                        for (int i = 0; i < bloodlettingServantSpawnAmount; i++)
                            NPC.NewNPCAction<BloodlettingServant>(NPC.GetSource_FromAI(), NPC.Center, NPC.whoAmI, n => n.velocity = Main.rand.NextPolarVector2(10f, 15f));
                        break;
                    case PhaseChangeTime_1To2:
                        SelectNextAttack();
                        break;
                }
            }

            void ZenithFlame()
            {
                if (TOSharedData.GeneralClient) //火焰
                {
                    Timer3++;

                    float flamethrowerSpeed = 10f * Math.Clamp(Timer3 / 60f, 0f, 1f);
                    Vector2 flamethrowerVelocity = new PolarVector2(flamethrowerSpeed, ActualRotation) + NPC.velocity * 0.45f;
                    flamethrowerVelocity.Modulus = Math.Min(flamethrowerVelocity.Modulus, MathF.Sqrt(NPC.velocity.LengthSquared() + 16));
                    Projectile.NewProjectileAction<BloodFlame>(NPC.GetSource_FromAI(), NPC.Center + new PolarVector2(ProjectileOffset, ActualRotation), flamethrowerVelocity, BloodFlameDamage, 0f, Main.myPlayer);
                }
            }
        }

        void PhaseChange_2To3()
        {
            Timer1++;

            NPC.damage = 0;
            NPC.rotation += PhaseChange_2To3_RotationSpeedFunction.Process(Timer1);
            StopMovement();

            Dust.NewDustAction(NPC.Center, NPC.width, NPC.height, DustID.Blood, new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f)));

            if (Timer1 >= PhaseChangeGateValue_2To3_1) //改变颜色
                Phase3ColorRatio += 0.025f;

            if (Timer1 is > 0 and <= PhaseChangeGateValue_2To3_2) //移动竞技场中心
                Phase3ArenaCenter = Vector2.SmootherStep(Phase3ArenaCenter, Target.Center, TOMathUtils.Interpolation.ExponentialEaseIn(Timer1 * 1.5f / PhaseChangeGateValue_2To3_2, 4f));

            if (Timer1 is >= PhaseChangeGateValue_2To3_2 and <= PhaseChangeGateValue_2To3_2 + 60) //回复血量
            {
                float ratio = (Timer1 - PhaseChangeGateValue_2To3_2) / 60f;
                int newLife = (int)MathHelper.Lerp(NPC.life, NPC.lifeMax * MathHelper.Lerp(0.1f, 0.5f, ratio), TOMathUtils.Interpolation.LogarithmicEaseOut(ratio));
                int increasedLife = Math.Clamp(newLife - NPC.life, 0, NPC.lifeMax / 2 - NPC.life);

                if (increasedLife > 0)
                {
                    NPC.life += increasedLife;
                    NPC.HealEffect(increasedLife, true);
                }

                if (NPC.life > NPC.lifeMax)
                    NPC.life = NPC.lifeMax;
            }

            if (Timer1 is >= PhaseChangeGateValue_2To3_2 - 15 and < PhaseChangeGateValue_2To3_2) //提前开始第三阶段传送
                TeleportTo(Vector2.Zero, Timer1 - (PhaseChangeGateValue_2To3_2 - 15), EyeofCthulhu_Handler.NormalTeleportDuration + 29, 1);

            switch (Timer1)
            {
                case 0:
                    Phase3ArenaCenter = NPC.Center;
                    SendCommandToServants(BehaviorCommand_Servant.GetToArenaPosition);
                    if (TOSharedData.GeneralClient)
                        Projectile.NewProjectileAction<EyeofCthulhuArena>(NPC.GetSource_FromAI(), Phase3ArenaCenter, Vector2.Zero, ArenaDamage, 0f, action: p =>
                        {
                            p.GetModProjectile<EyeofCthulhuArena>().Master = NPC;
                            ArenaProjectile = p;
                        });
                    break;
                case PhaseChangeGateValue_2To3_2:
                    SoundEngine.PlaySound(SoundID.NPCHit1, NPC.Center);
                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

                    for (int i = 0; i < 20; i++)
                        Dust.NewDustAction(NPC.Center, NPC.width, NPC.height, DustID.Blood, new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f)));

                    //生成冲击波清除克苏鲁之仆；使竞技场激活，流血仆从脱战（通过弹幕实现），克苏鲁之仆死亡
                    Projectile.NewProjectileAction<BloodShockwave>(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, 100, 0f, action: p =>
                    {
                        p.scale = 0f;
                        BloodShockwave modP = p.GetModProjectile<BloodShockwave>();
                        modP.Master = NPC;
                    });

                    if (ArenaProjectileAlive)
                    {
                        EyeofCthulhuArena modP = ArenaModProjectile;
                        modP.IsActivated = true;
                        if (ServantLeftAlive)
                        {
                            ArenaProjectile.frameCounter = (int)ServantLeft.frameCounter;
                            ArenaProjectile.scale = ServantLeft.scale;
                            modP.RealArenaRadius = ServantLeft.GetModNPC<BloodlettingServant>().ArenaRadius;
                        }
                        else if (ServantRightAlive)
                        {
                            ArenaProjectile.frameCounter = (int)ServantRight.frameCounter;
                            ArenaProjectile.scale = ServantRight.scale;
                            modP.RealArenaRadius = ServantRight.GetModNPC<BloodlettingServant>().ArenaRadius;
                        }
                    }

                    ExecuteActionToServants((n, modN) =>
                    {
                        n.life = 0;
                        n.active = false;
                        n.netUpdate = true;
                    });

                    CurrentAttackPhase = 1;
                    break;
                case PhaseChangeTime_2To3: //进入三阶段
                    CurrentPhase = Phase.Phase3;
                    CurrentBehavior = Behavior.Phase3_Charge;
                    CurrentAttackPhase = 0;
                    Timer1 = 15;
                    Timer2 = 0;
                    break;
            }
        }
        #endregion 1、2阶段

        void Phase3AI()
        {
            if (!ArenaProjectileAlive)
                return;

            ArenaModProjectile.MasterPhase3_2 = Phase3_2;

            switch (CurrentBehavior)
            {
                case Behavior.Phase3_Charge:
                    Charge();
                    break;
                case Behavior.Phase3_EyeSpin:
                    EyeSpin();
                    break;
                case Behavior.Phase3_3:
                    P33();
                    break;
                default:
                    SelectNextAttack();
                    break;
            }

            void SelectNextAttack()
            {
                CurrentAttackPhase = 0;
                Timer1 = 0;
                Timer2 = 0;
                Timer3 = 0;
                switch (CurrentBehavior)
                {
                    case Behavior.Phase3_Charge:
                        AttackCounter++;
                        if (AttackCounter >= 7)
                        {
                            AttackCounter = 0;
                            CurrentBehavior = Behavior.Phase3_EyeSpin;
                            Timer1 = -20; //20帧缓冲时间
                        }
                        break;
                    case Behavior.Phase3_EyeSpin:
                        AttackCounter++;
                        if (AttackCounter >= 3)
                        {
                            AttackCounter = 0;
                            CurrentBehavior = Behavior.Phase3_Charge;
                            break;
                        }
                        Timer1 = -30; //30帧缓冲时间
                        break;
                }
            }

            void CheckPhaseChange()
            {
                if (NPC.LifeRatio < Phase3_2LifeRatio)
                    CurrentPhase = Phase.Phase3_2;
            }

            void Charge()
            {
                switch (AttackCounter)
                {
                    case <= 2: //普通冲刺
                        NormalCharge();
                        break;
                    case 3: //第一次快速冲刺
                        FirstRapidCharge();
                        break;
                    case 4 or 5: //第二、三次快速冲刺
                        SecondAndThirdRapidCharge();
                        break;
                    case 6: //最后一次快速冲刺
                        LastRapidCharge();
                        break;
                    default:
                        SelectNextAttack();
                        break;
                }

                NPC.SpawnAfterimage(5, Phase3Color);

                int CalculateDistance(int index1, int index2) => Math.Abs((int)TOMathUtils.ShortestDifference(index1, index2, 32f));

                void SpawnChargeParticle()
                {
                    int particleAmount = 50;
                    for (int i = 0; i < particleAmount; i++)
                        EyeofCthulhu_Handler.SpawnOrbParticle(NPC.Center, Main.rand.NextFloat(8f, 12f), Main.rand.Next(30, 45), Main.rand.NextFloat(1.2f, 1.6f));
                }

                void DoBehaviorDuringCharge(int firstAttackPhase, int timer2GateValue = 5)
                {
                    NPC.damage = SetDamage;
                    NPC.VelocityToRotation(-MathHelper.PiOver2);

                    //在第二亚阶段喷火
                    if (Phase3_2)
                    {
                        Vector2 flamethrowerVelocity = NPC.velocity * 1.15f;
                        flamethrowerVelocity.Modulus += 2f;
                        Projectile.NewProjectileAction<BloodFlame>(NPC.GetSource_FromAI(), NPC.Center + new PolarVector2(ProjectileOffset, ActualRotation), flamethrowerVelocity, BloodFlameDamage, 0f, Main.myPlayer);
                    }

                    if (CurrentAttackPhase == firstAttackPhase && NPC.Distance(ArenaProjectile.Center) <= ArenaModProjectile.RealArenaRadius + 20f)
                        CurrentAttackPhase = firstAttackPhase + 1;
                    if ((CurrentAttackPhase == firstAttackPhase + 1 && NPC.Distance(ArenaProjectile.Center) > ArenaModProjectile.RealArenaRadius + 100f) || Timer2 > 0)
                    {
                        StopMovement();
                        Timer2++;
                        if (Timer2 >= timer2GateValue)
                        {
                            CheckPhaseChange();
                            SelectNextAttack();
                        }
                    }
                }

                void NormalCharge()
                {
                    bool firstCharge = AttackCounter == 0;
                    bool shouldUseIndex2 = AttackCounter >= 1;
                    bool shouldUseIndex3 = AttackCounter >= 2;
                    bool shouldUseIndex4 = shouldUseIndex3 && Phase3_2;

                    int teleportDuration = firstCharge ? EyeofCthulhu_Handler.NormalTeleportDuration + 30 : EyeofCthulhu_Handler.NormalTeleportDuration;

                    switch (CurrentAttackPhase)
                    {
                        case 0: //初始化
                            CurrentAttackPhase = 1;

                            NPC.damage = ReducedSetDamage;
                            if (firstCharge)
                                SendCommandToArena(BehaviorCommand_Arena.Charge);

                            int usedIndex1 = Main.rand.Next(0, 32);
                            UsedEyeIndex1 = usedIndex1;

                            if (shouldUseIndex2)
                            {
                                int usedIndex2 = (int)TOMathUtils.NormalizeWithPeriod(usedIndex1 + Main.rand.Next(5, 17) * Main.rand.NextBool(2).ToDirectionInt(), 32);
                                UsedEyeIndex2 = usedIndex2;
                                if (shouldUseIndex3)
                                {
                                    int usedIndex3;
                                    do usedIndex3 = Main.rand.Next(0, 32);
                                    while (CalculateDistance(usedIndex1, usedIndex3) < 4 || CalculateDistance(usedIndex2, usedIndex3) < 4);
                                    UsedEyeIndex3 = usedIndex3;
                                    if (shouldUseIndex4)
                                    {
                                        int usedIndex4;
                                        do usedIndex4 = Main.rand.Next(0, 32);
                                        while (CalculateDistance(usedIndex1, usedIndex4) < 3 || CalculateDistance(usedIndex2, usedIndex4) < 3 || CalculateDistance(usedIndex3, usedIndex4) < 3);
                                        UsedEyeIndex4 = usedIndex4;
                                    }
                                }
                            }
                            goto case 1;
                        case 1: //传送
                            Timer1++;

                            NPC.damage = ReducedSetDamage;
                            StopMovement();
                            Vector2 destination = ArenaProjectile.Center + new PolarVector2(ArenaModProjectile.RealArenaRadius + 200f, ArenaModProjectile.GetEyeRotation(UsedEyeIndex1));
                            TeleportTo(destination, Timer1, teleportDuration - 1, 1);

                            if (Timer1 == teleportDuration)
                            {
                                Timer1 = 0;
                                CurrentAttackPhase = 2;
                            }
                            NormalUpdateRotation(0.16f);
                            break;
                        case 2: //冲刺初始化
                            NPC.damage = SetDamage;
                            SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                            SpawnChargeParticle();
                            NPC.SetVelocityandRotation(NPC.GetVelocityTowards(Target, 30f), -MathHelper.PiOver2);
                            NPC.damage = SetDamage;
                            CheckPhaseChange();
                            CurrentAttackPhase = 3;
                            break;
                        case 3 or 4: //冲刺中
                            DoBehaviorDuringCharge(3);
                            break;
                    }
                }

                void FirstRapidCharge()
                {
                    switch (CurrentAttackPhase)
                    {
                        case 0: //初始化：一次性生成4个Index
                            CurrentAttackPhase = 1;

                            NPC.damage = ReducedSetDamage;
                            int usedIndex1 = Main.rand.Next(0, 32);
                            UsedEyeIndex1 = usedIndex1;
                            int usedIndex2 = (int)TOMathUtils.NormalizeWithPeriod(usedIndex1 + Main.rand.Next(5, 17) * Main.rand.NextBool(2).ToDirectionInt(), 32);
                            UsedEyeIndex2 = usedIndex2;
                            int usedIndex3;
                            do usedIndex3 = Main.rand.Next(0, 32);
                            while (CalculateDistance(usedIndex1, usedIndex3) < 4 || CalculateDistance(usedIndex2, usedIndex3) < 4);
                            UsedEyeIndex3 = usedIndex3;
                            int usedIndex4;
                            do usedIndex4 = Main.rand.Next(0, 32);
                            while (CalculateDistance(usedIndex1, usedIndex4) < 3 || CalculateDistance(usedIndex2, usedIndex4) < 3 || CalculateDistance(usedIndex3, usedIndex4) < 3);
                            UsedEyeIndex4 = usedIndex4;
                            goto case 1;
                        case 1: //等待竞技场完成操作
                            Timer1++;

                            NPC.damage = ReducedSetDamage;
                            StopMovement();
                            switch (Timer1)
                            {
                                case 100: //进入传送冲刺阶段
                                    Timer1 = 0;
                                    CurrentAttackPhase = 2;
                                    break;
                            }
                            break;
                        case 2: //传送
                            Timer1++;

                            NPC.damage = ReducedSetDamage;
                            StopMovement();
                            int teleportDuration = 80;
                            Vector2 destination = ArenaProjectile.Center + new PolarVector2(ArenaModProjectile.RealArenaRadius + 200f, ArenaModProjectile.GetEyeRotation(UsedEyeIndex1));
                            TeleportTo(destination, Timer1, teleportDuration - 1, 1);

                            if (Timer1 == teleportDuration)
                            {
                                Timer1 = 0;
                                CurrentAttackPhase = 3;
                            }
                            break;
                        case 3: //冲刺初始化
                            SoundEngine.PlaySound(SoundID.ForceRoarPitched, NPC.Center);
                            SpawnChargeParticle();
                            NPC.SetVelocityandRotation(NPC.GetVelocityTowards(Target, 40f), -MathHelper.PiOver2);
                            NPC.damage = SetDamage;
                            CheckPhaseChange();
                            CurrentAttackPhase = 4;
                            break;
                        case 4 or 5: //冲刺中
                            DoBehaviorDuringCharge(4);
                            break;
                    }
                }

                void SecondAndThirdRapidCharge()
                {
                    int usedIndex = AttackCounter switch
                    {
                        4 => UsedEyeIndex2,
                        5 => UsedEyeIndex3,
                        _ => UsedEyeIndex1
                    };

                    switch (CurrentAttackPhase)
                    {
                        case 0: //传送
                            Timer1++;

                            NPC.damage = ReducedSetDamage;
                            StopMovement();
                            int teleportDuration = 15;
                            Vector2 destination = ArenaProjectile.Center + new PolarVector2(ArenaModProjectile.RealArenaRadius + 200f, ArenaModProjectile.GetEyeRotation(usedIndex));
                            TeleportTo(destination, Timer1, teleportDuration - 1, 1);

                            if (Timer1 == teleportDuration)
                            {
                                Timer1 = 0;
                                CurrentAttackPhase = 1;
                            }
                            break;
                        case 1: //冲刺初始化
                            SoundEngine.PlaySound(SoundID.ForceRoarPitched, NPC.Center);
                            SpawnChargeParticle();
                            NPC.SetVelocityandRotation(NPC.GetVelocityTowards(Target, 42f), -MathHelper.PiOver2);
                            NPC.damage = SetDamage;
                            CheckPhaseChange();
                            CurrentAttackPhase = 2;
                            break;
                        case 2 or 3: //冲刺中
                            DoBehaviorDuringCharge(2);
                            break;
                    }
                }

                void LastRapidCharge()
                {
                    int usedIndex = UsedEyeIndex4;

                    switch (CurrentAttackPhase)
                    {
                        case 0: //传送
                            Timer1++;

                            NPC.damage = ReducedSetDamage;
                            StopMovement();
                            int teleportDuration = 15;
                            Vector2 destination = ArenaProjectile.Center + new PolarVector2(ArenaModProjectile.RealArenaRadius + 200f, ArenaModProjectile.GetEyeRotation(usedIndex));
                            TeleportTo(destination, Timer1, teleportDuration - 1, 1);

                            if (Timer1 == teleportDuration)
                            {
                                Timer1 = 0;
                                CurrentAttackPhase = 1;
                            }
                            break;
                        case 1: //冲刺初始化
                            SoundEngine.PlaySound(SoundID.ForceRoarPitched, NPC.Center);
                            SpawnChargeParticle();
                            NPC.SetVelocityandRotation(NPC.GetVelocityTowards(Target, 48f), -MathHelper.PiOver2);
                            NPC.damage = SetDamage;
                            CheckPhaseChange();
                            CurrentAttackPhase = 2;
                            break;
                        case 2 or 3: //冲刺中
                            DoBehaviorDuringCharge(2, 15);

                            Timer3++;
                            int projectileSpawnGateValue = 7;
                            if (Phase3_2 && IsInPhase3Arena && Timer3 % projectileSpawnGateValue == 0)
                                Projectile.RotatedProj(2, MathHelper.Pi, NPC.GetSource_FromAI(), NPC.Center, NPC.velocity.RotatedBy(MathHelper.PiOver2) * 0.25f, ProjectileID.BloodShot, BloodDamage, 0f, action: p => p.timeLeft = 100);

                            if (Timer2 == 1) //下一次攻击的粒子预警
                            {
                                //粒子预警
                                Vector2 center = ArenaProjectile.Center;
                                ParticleHandler.SpawnParticle(new BloomParticle(center, Vector2.Zero, Color.Lerp(Color.Red, Color.Magenta, 0.05f), 0f, 2f, 115, 0.9f));
                                ParticleHandler.SpawnParticle(new BloomParticle(center, Vector2.Zero, Color.Lerp(Color.Red, Color.Magenta, 0.15f), 0f, 1.9f, 115, 0.9f));
                                ParticleHandler.SpawnParticle(new BloomParticle(center, Vector2.Zero, Color.White, 0f, 1.75f, 115, 0.85f));
                            }
                            break;
                    }
                }
            }

            void EyeSpin()
            {
                switch (AttackCounter)
                {
                    case 0:
                        FirstEyeSpin();
                        break;
                    case 1:
                        SecondEyeSpin();
                        break;
                    case 2:
                        LastEyeSpin();
                        break;
                }

                void SpawnSquashDust()
                {
                    Vector2 dustVelocity = Main.rand.NextPolarVector2(10.5f, 14.5f);
                    Dust.NewDustPerfectAction<SquashDust>(NPC.Center - dustVelocity.ToCustomLength(Main.rand.NextFloat(150f, 300f)), d =>
                    {
                        d.velocity = dustVelocity;
                        d.scale = Main.rand.NextFloat(0.9f, 1.2f);
                        d.noGravity = true;
                        d.fadeIn = 0.66f;
                        d.color = Color.Lerp(Color.Red, EyeofCthulhu_Handler.ChargeColor, Main.rand.NextFloat(0.3f - NPC.LifeRatio / 2f, 0.6f));
                    });
                }

                void FirstEyeSpin()
                {
                    switch (CurrentAttackPhase)
                    {
                        case 0:
                            int usedIndex1 = Main.rand.Next(0, 32);
                            UsedEyeIndex1 = usedIndex1;
                            SendCommandToArena(BehaviorCommand_Arena.EyeSpin);
                            CurrentAttackPhase = 1;
                            goto case 1;
                        case 1:
                            Timer1++;
                            TeleportTo(Phase3ArenaCenter, Timer1, EyeofCthulhu_Handler.EyeSpinPhase1Time - 3, 3);
                            NormalUpdateRotation(0.5f);
                            SpawnSquashDust();
                            if (Timer1 == EyeofCthulhu_Handler.EyeSpinPhase1Time)
                            {
                                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

                                for (int i = 0; i < 100; i++)
                                    ParticleHandler.SpawnParticle(new OrbParticle(NPC.Center, Main.rand.NextPolarVector2(10f, 20f), Main.rand.Next(30, 40), Main.rand.NextFloat(1.2f, 1.6f), Color.Lerp(Color.Red, EyeofCthulhu_Handler.ChargeColor, Main.rand.NextFloat(0.5f - NPC.LifeRatio, 0.8f - NPC.LifeRatio))));

                                ShootCircleProjectile(40);
                                CheckPhaseChange();
                                SelectNextAttack();
                            }
                            break;
                    }
                }

                void SecondEyeSpin()
                {
                    switch (CurrentAttackPhase)
                    {
                        case 0:
                            int usedIndex1 = (int)TOMathUtils.NormalizeWithPeriod(UsedEyeIndex1 + Main.rand.Next(5, 12) * Main.rand.NextBool(2).ToDirectionInt(), 32);
                            UsedEyeIndex1 = usedIndex1;
                            SendCommandToArena(BehaviorCommand_Arena.EyeSpin);
                            CurrentAttackPhase = 1;
                            goto case 1;
                        case 1:
                            Timer1++;
                            NormalUpdateRotation(0.5f);
                            SpawnSquashDust();
                            if (Timer1 == EyeofCthulhu_Handler.EyeSpinPhase1Time)
                            {
                                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                                ShootCircleProjectile(40);
                                CheckPhaseChange();
                                SelectNextAttack();
                            }
                            break;
                    }
                }

                void LastEyeSpin()
                {
                    switch (CurrentAttackPhase)
                    {
                        case 0:
                            int usedIndex1 = (int)TOMathUtils.NormalizeWithPeriod(UsedEyeIndex1 + Main.rand.Next(5, 12) * Main.rand.NextBool(2).ToDirectionInt(), 32);
                            UsedEyeIndex1 = usedIndex1;
                            SendCommandToArena(BehaviorCommand_Arena.EyeSpinLast);
                            CurrentAttackPhase = 1;
                            goto case 1;
                        case 1:
                            Timer1++;
                            NormalUpdateRotation(0.5f);
                            SpawnSquashDust();
                            if (Timer1 == EyeofCthulhu_Handler.EyeSpinPhase1Time)
                            {
                                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

                                ShootCircleProjectile(40);

                                if (Phase3_2)
                                {
                                    PolarVector2 originalvelocity = ArenaModProjectile.GetEyeCenterDirection(UsedEyeIndex1) * (ArenaModProjectile.RealArenaRadius - 15f) / 90f * 0.5f;
                                    int projectileAmount2 = 64;
                                    float singleRadian2 = MathHelper.TwoPi / projectileAmount2;
                                    Projectile.RotatedProj(projectileAmount2, singleRadian2, NPC.GetSource_FromAI(), NPC.Center, originalvelocity, ProjectileID.BloodShot, BloodDamage, 0f, action: p => p.timeLeft = 90);
                                }

                                CheckPhaseChange();
                                SelectNextAttack();
                            }
                            break;
                    }
                }

                void ShootCircleProjectile(int projectileAmount)
                {
                    PolarVector2 originalvelocity = ArenaModProjectile.GetEyeCenterDirection(UsedEyeIndex1) * (ArenaModProjectile.RealArenaRadius - 15f) * EyeofCthulhu_Handler.EyeShapeHelper.InnerVelocityMultiplier / EyeofCthulhu_Handler.EyeSpinPhase2Time;
                    float singleRadian = MathHelper.TwoPi / projectileAmount;
                    Projectile.RotatedProj(projectileAmount, singleRadian, NPC.GetSource_FromAI(), NPC.Center, originalvelocity, ProjectileID.BloodShot, BloodDamage, 0f, action: p =>
                    {
                        p.Anomaly.OverrideType = (int)OverrideType_BloodShot.AnomalyEyeofCthulhu_EyeSpin2;

                        BloodShot_Anomaly_EyeSpin behavior = new()
                        {
                            _entity = p,
                            Master = NPC,
                            ArenaProjectile = ArenaProjectile
                        };
                    });
                }
            }

            void P33()
            {

            }
        }
        #endregion 行为函数

        #region 控制命令
        void ExecuteActionToLeftServant(Action<NPC, BloodlettingServant> action)
        {
            if (ServantLeftAlive)
                action?.Invoke(ServantLeft, ServantLeft.GetModNPC<BloodlettingServant>());
        }

        void ExecuteActionToRightServant(Action<NPC, BloodlettingServant> action)
        {
            if (ServantRightAlive)
                action?.Invoke(ServantRight, ServantRight.GetModNPC<BloodlettingServant>());
        }

        void ExecuteActionToServants(Action<NPC, BloodlettingServant> action)
        {
            ExecuteActionToLeftServant(action);
            ExecuteActionToRightServant(action);
        }

        void ExecuteActionToArena(Action<Projectile, EyeofCthulhuArena> action)
        {
            if (ArenaProjectileAlive)
                action?.Invoke(ArenaProjectile, ArenaModProjectile);
        }

        void SendCommandToServants(BehaviorCommand_Servant command) => ExecuteActionToServants((n, modN) => modN.MasterCommandReceiver = command);

        void SendCommandToArena(BehaviorCommand_Arena command) => ExecuteActionToArena((p, modP) => modP.MasterCommandReceiver = command);
        #endregion 控制命令
    }

    public override void FindFrame(int frameHeight)
    {
        int frameNum;

        NPC.frameCounter += 1.0;

        switch (NPC.frameCounter)
        {
            case < 7.0:
                frameNum = 0;
                break;
            case < 14.0:
                frameNum = 1;
                break;
            case < 21.0:
                frameNum = 2;
                break;
            default:
                NPC.frameCounter = 0.0;
                frameNum = 0;
                break;
        }

        bool shouldUsePhase2Frame = CurrentPhase switch
        {
            Phase.Phase1 => false,
            Phase.PhaseChange_1To2 => Timer1 >= PhaseChangeGateValue_1To2_2,
            _ => true
        };

        if (shouldUsePhase2Frame)
            frameNum += 3;

        NPC.frame.Y = frameNum * frameHeight;
    }

    public override Color? GetAlpha(Color drawColor)
    {
        if (Phase3ColorRatio > 0f)
            return Color.Lerp(drawColor, Phase3Color, Phase3ColorRatio * 0.6f) with { A = NPC.GraphicAlpha };

        return null;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D npcTexture = NPC.Texture;
        Color originalColor = NPC.GetAlpha(drawColor);
        Rectangle frame = NPC.frame;
        spriteBatch.DrawFromCenter(npcTexture, NPC.Center + DrawOffset - screenPos, frame, originalColor, NPC.rotation, NPC.scale);
        return false;
    }

    public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
    {
        if (!Ultra)
            return;

        if (Phase3) //在竞技场内获得20%易伤，否则获得85%减伤
        {
            float damageMultiplier = IsInPhase3Arena ? 1.2f : 0.15f;
            modifiers.SourceDamage *= damageMultiplier;
        }
        else if (InvalidPhase2)
            modifiers.SetMaxDamage((int)(NPC.life - NPC.lifeMax * Phase3LifeRatio));
    }

    public override bool CheckDead()
    {
        if (Ultra && !Phase3 && !NPC.downedBoss1)
        {
            NPC.life = 1;
            NPC.active = true;
            NPC.netUpdate = true;
            return false;
        }

        return true;
    }

    public override void BossHeadSlot(ref int index)
    {
        if (Phase3 && !IsInPhase3Arena)
            index = -1;
    }

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => !IsInPhase3Arena ? false : null;

    public override bool PreHoverInteract(bool mouseIntersects)
    {
        if (Phase3 && !IsInPhase3Arena)
            return false;

        return true;
    }
}
