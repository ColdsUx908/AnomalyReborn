// Developed by ColdsUx

using CalamityMod.Projectiles.Boss;

namespace CalamityAnomalies.Anomaly.QueenBee;

public sealed partial class QueenBee_Anomaly
{
    public override bool PreAI()
    {
        float despawnDistance = OwnCombCell ? DespawnDistance2 : DespawnDistance;
        if (CurrentBehavior == Behavior.Despawn || !NPC.TargetClosestIfInvalid(true, despawnDistance))
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

        switch (CurrentPhase)
        {
            case Phase.Initialize:
                CurrentPhase = Phase.Phase1;
                CurrentBehavior = Behavior.Phase1_NormalCharge;
                break;
            case >= Phase.Phase1 and <= Phase.Phase1_5:
                Phase1AI();
                break;
            case Phase.PhaseChange_1To2:
                PhaseChange_1To2();
                break;
            case Phase.Phase2 or Phase.Phase2_2:
                Phase2AI();
                break;
        }

        if (Main.dedServ)
            NPC.netUpdate = true;

        return false;

        #region 行为函数
        void StopMovement(float velocityMultiplier = 0.93f)
        {
            NPC.velocity *= velocityMultiplier;

            if (Math.Abs(NPC.velocity.X) < 0.1f)
                NPC.velocity.X = 0f;
            if (Math.Abs(NPC.velocity.Y) < 0.1f)
                NPC.velocity.Y = 0f;
        }

        Vector2 GetStingerSpawnLocation() => new(NPC.Center.X + (Main.rand.Next(20) * NPC.direction), NPC.position.Y + NPC.height * 0.8f);

        bool CanHitTarget()
        {
            Vector2 stingerSpawnLocation = GetStingerSpawnLocation();
            return Collision.CanHit(new Vector2(stingerSpawnLocation.X, stingerSpawnLocation.Y - 30f), 1, 1, Target.position, Target.width, Target.height);
        }

        void TryMoveAboveTarget(bool higher = false)
        {
            Vector2 stingerSpawnLocation = GetStingerSpawnLocation();

            float moveSpeed = Phase2_2 ? 28f : Ultra ? 25f : 22.5f;
            float moveAcceleration = Phase2_2 ? 0.6f : Phase2 ? 0.5f : Phase1_2 ? 0.45f : 0.35f;

            bool canHitTarget = CanHitTarget();
            Vector2 hoverDestination = Target.Center - Vector2.UnitY * (!canHitTarget ? 0f : higher ? 480f : 320f);
            Vector2 idealVelocity = NPC.GetVelocityTowards(hoverDestination, moveSpeed);

            if (Vector2.Distance(stingerSpawnLocation, hoverDestination) > 40f || !canHitTarget)
                NPC.SimpleFlyMovement(idealVelocity, moveAcceleration);

            NPC.FaceTarget(Target);
            NPC.spriteDirection = NPC.direction;
        }

        void SpawnCombCell(Vector2 position, Vector2 velocity, int damage, CombCell.Behavior behavior, float finalScale, Action<Projectile, CombCell> action = null)
        {
            if (TOSharedData.NotClient)
            {
                Projectile.NewProjectileAction<CombCell>(SourceAI, position, velocity, damage, 0f, action: p =>
                {
                    CombCell modP = p.GetModProjectile<CombCell>();
                    modP.Master = NPC;
                    modP.BehaviorType = behavior;
                    modP.FinalScale = finalScale;
                    action?.Invoke(p, modP);

                    if (behavior is
                        CombCell.Behavior.BeeSwarm
                        or CombCell.Behavior.BeeSwarm2
                        or CombCell.Behavior.BeeSwarm3
                        or CombCell.Behavior.PhaseChange)
                    {
                        OwnedCombCell = p;
                    }
                });
            }
        }

        void SpawnFriendlyBee(Vector2 position, Vector2 velocity, byte behavior, float ai1)
        {
            if (TOSharedData.NotClient)
            {
                Projectile.NewProjectileAction<FriendlyBee>(SourceAI, position, velocity, 0, 0f, action: p =>
                {
                    p.ai[0] = behavior;
                    p.ai[1] = ai1;
                });
            }
        }

        void Phase1AI()
        {
            switch (CurrentBehavior)
            {
                case Behavior.Phase1_NormalCharge:
                    NormalCharge();
                    break;
                case Behavior.Phase1_DirectCharge:
                    DirectCharge();
                    break;
                case Behavior.Phase1_Stinger:
                    Stinger();
                    break;
                case Behavior.Phase1_BeeSwarm:
                    BeeSwarm();
                    break;
                case Behavior.Phase1_BeeSwarm2:
                    BeeSwarm2();
                    break;
                default:
                    CheckPhaseChange();
                    SelectNextBehavior();
                    break;
            }

            void SelectNextBehavior()
            {
                Timer1 = 0;
                Timer2 = 0;
                CurrentAttackPhase = 0;
                ShouldDecelerate = false;

                switch (CurrentBehavior)
                {
                    case Behavior.Phase1_NormalCharge or Behavior.Phase1_DirectCharge:
                        CurrentAttackCounter++;
                        int chargeAmount = AttackRandomVariation_Charge ?
                            (int)MathHelper.Lerp(3, 6, NPC.LostLifeRatio) * 2
                            : (int)MathHelper.Lerp(4, 7, NPC.LostLifeRatio);
                        if (CurrentAttackCounter >= chargeAmount)
                        {
                            CurrentAttackCounter = 0;
                            SwitchToNext();
                        }
                        else if (AttackRandomVariation_Charge)
                            CurrentBehavior = CurrentBehavior == Behavior.Phase1_NormalCharge ? Behavior.Phase1_DirectCharge : Behavior.Phase1_NormalCharge;
                        break;
                    default:
                        SwitchToNext();
                        break;
                }

                void SwitchToNext()
                {
                    FinishedBehaviorCounter++;
                    int attackCycleLength;

                    if (!Phase1_2)
                    {
                        attackCycleLength = 4;
                        CurrentBehavior = (FinishedBehaviorCounter % attackCycleLength) switch
                        {
                            0 => Behavior.Phase1_NormalCharge,
                            1 => Behavior.Phase1_Stinger,
                            2 => Behavior.Phase1_NormalCharge,
                            3 => Behavior.Phase1_BeeSwarm,

                            _ => Behavior.Phase1_NormalCharge
                        };
                    }
                    else
                    {
                        attackCycleLength = 4;
                        CurrentBehavior = (FinishedBehaviorCounter % attackCycleLength) switch
                        {
                            0 => Behavior.Phase1_NormalCharge,
                            1 => Behavior.Phase1_Stinger,
                            2 => Behavior.Phase1_NormalCharge,
                            3 => FinishedBehaviorCounter is 7 || Main.rand.NextBool() ? Behavior.Phase1_BeeSwarm2 : Behavior.Phase1_BeeSwarm,

                            _ => Behavior.Phase1_NormalCharge
                        };
                    }
                }
            }

            bool CheckPhaseChange()
            {
                if (ShouldEnterPhase2)
                {
                    CurrentPhase = Phase.PhaseChange_1To2;
                    CurrentBehavior = Behavior.PhaseChange_1To2;
                    CurrentAttackPhase = 0;
                    Timer1 = 0;
                    Timer2 = 0;
                    return true;
                }
                else if (NPC.LifeRatio <= Phase1_2LifeRatio && CurrentPhase == Phase.Phase1)
                {
                    CurrentPhase = Phase.Phase1_2;
                    FinishedBehaviorCounter %= 4;
                }

                return false;
            }

            void NormalCharge()
            {
                switch (CurrentAttackPhase)
                {
                    case 0:
                        NPC.damage = 0;

                        float distanceFromTargetX = Math.Abs(NPC.Center.X - Target.Center.X);
                        float distanceFromTargetY = Math.Abs(NPC.Center.Y - Target.Center.Y);
                        if (distanceFromTargetY <= ChargeDistanceY && distanceFromTargetX >= 50f && distanceFromTargetX <= ChargeDistanceX * 2f)
                        {
                            NPC.damage = NPC.defDamage;
                            IsCharging = true;
                            CurrentAttackPhase = 1;

                            NPC.velocity = NPC.GetVelocityTowards(Target.Center, ChargeSpeed);

                            NPC.FaceTarget(Target);
                            NPC.spriteDirection = NPC.direction;
                            SoundEngine.PlaySound(ChargeSound, NPC.Center);
                        }
                        else
                        {
                            IsCharging = false;

                            Timer1++;
                            float velocityMultiplier = Utils.Remap(Timer1, 30, 90, 1f, 2f);

                            float approachVelocityX = MathHelper.Lerp(30f, 36f, NPC.LostLifeRatio) * velocityMultiplier;
                            float approachVelocityY = MathHelper.Lerp(20f, 24f, NPC.LostLifeRatio) * velocityMultiplier;
                            float approachAccelerationX = MathHelper.Lerp(0.9f, 1.2f, NPC.LostLifeRatio) * velocityMultiplier;
                            float approachAccelerationY = MathHelper.Lerp(0.6f, 0.9f, NPC.LostLifeRatio) * velocityMultiplier;

                            if (NPC.Center.Y < Target.Center.Y - ChargeDistanceY)
                                NPC.velocity.Y += approachAccelerationY;
                            else if (NPC.Center.Y > Target.Center.Y + ChargeDistanceY)
                                NPC.velocity.Y -= approachAccelerationY;
                            else
                                NPC.velocity.Y *= 0.7f;

                            if (NPC.velocity.Y < -approachVelocityY)
                                NPC.velocity.Y = -approachVelocityY;
                            if (NPC.velocity.Y > approachVelocityY)
                                NPC.velocity.Y = approachVelocityY;

                            float distanceXMax = ChargeDistanceX;
                            float distanceXMin = ChargeDistanceX * 0.5f;
                            if (distanceFromTargetX > distanceXMax)
                                NPC.velocity.X += approachAccelerationX * NPC.direction;
                            else if (distanceFromTargetX < distanceXMin)
                                NPC.velocity.X -= approachAccelerationX * NPC.direction;
                            else
                                NPC.velocity.X *= 0.7f;

                            if (NPC.velocity.X < -approachVelocityX)
                                NPC.velocity.X = -approachVelocityX;
                            if (NPC.velocity.X > approachVelocityX)
                                NPC.velocity.X = approachVelocityX;

                            NPC.FaceTarget(Target);
                            NPC.spriteDirection = NPC.direction;
                        }
                        break;
                    case 1:
                        NPC.damage = NPC.defDamage;
                        NPC.direction = NPC.velocity.X < 0f ? -1 : 1;
                        NPC.spriteDirection = NPC.direction;

                        int chargeDirection = NPC.Center.X < Target.Center.X ? -1 : 1;
                        if (NPC.direction == chargeDirection && Math.Abs(NPC.Center.X - Target.Center.X) > ChargeDistanceX * 0.5f)
                            ShouldDecelerate = true;
                        if (Vector2.Distance(NPC.Center, Target.Center) > ChargeDistanceX * 2f)
                            ShouldDecelerate = true;

                        if (ShouldDecelerate)
                        {
                            NPC.damage = 0;
                            float playerLocation = NPC.Center.X - Target.Center.X;
                            NPC.direction = playerLocation < 0 ? 1 : -1;
                            NPC.spriteDirection = NPC.direction;
                            IsCharging = false;
                            NPC.velocity *= 0.8f;

                            if (NPC.velocity.Length() < (Ultra ? (Phase1_2 ? 4f : 1f) : (Phase1_2 ? 2f : 0.3f)))
                            {
                                CheckPhaseChange();
                                SelectNextBehavior();
                                if (CurrentBehavior is not (Behavior.Phase1_NormalCharge or Behavior.Phase1_DirectCharge) && Main.rand.NextProbability(0.7f))
                                    AttackRandomVariation_Charge = !AttackRandomVariation_Charge;
                                break;
                            }
                        }
                        else
                        {
                            if (NPC.velocity.Length() < ChargeSpeed)
                                NPC.velocity.X = ChargeSpeed * NPC.direction;

                            int accelerateGateValue = 30;

                            Timer2++;
                            if (Timer2 > accelerateGateValue) //加速
                            {
                                float velocityXLimit = ChargeSpeed * 2f;
                                if (Math.Abs(NPC.velocity.X) < velocityXLimit)
                                    NPC.velocity.X *= 1.02f;
                            }

                            IsCharging = true;
                        }

                        break;
                }
            }

            void DirectCharge()
            {
                switch (CurrentAttackPhase)
                {
                    case 0:
                        ChargeStartDistance = NPC.Center - Target.Center;

                        float speed = ChargeSpeed * Utils.Remap(Vector2.Distance(NPC.Center, Target.Center), ChargeDistanceX, ChargeDistanceX * 2f, 1f, 1.3f);
                        NPC.damage = NPC.defDamage;
                        IsCharging = true;
                        CurrentAttackPhase = 1;

                        Vector2 velocity = NPC.GetVelocityTowards(Target.Center, speed);
                        NPC.velocity = velocity;

                        NPC.FaceTarget(Target);
                        NPC.spriteDirection = NPC.direction;
                        SoundEngine.PlaySound(ChargeSound, NPC.Center);

                        SpawnCombCell(NPC.Center, velocity * 0.4f, StingerDamage, CombCell.Behavior.Beehive, 0.3f);
                        break;
                    case 1:
                        NPC.damage = NPC.defDamage;
                        NPC.direction = NPC.velocity.X < 0f ? -1 : 1;
                        NPC.spriteDirection = NPC.direction;

                        Timer1++;

                        Vector2 distance = NPC.Center - Target.Center;
                        Vector2 chargeStartDistance = ChargeStartDistance;
                        bool chargeCompleted = chargeStartDistance * distance is ( < 0f, < 0f) && (
                            (Math.Abs(distance.X) > Math.Abs(chargeStartDistance.X) * 0.5f && Math.Abs(distance.Y) > Math.Abs(chargeStartDistance.Y) * 0.5f)
                            || distance.Length() > chargeStartDistance.Length());

                        if (Timer1 >= 35 || chargeCompleted)
                            ShouldDecelerate = true;
                        else if (!ShouldDecelerate || Timer1 >= 20)
                        {
                            float velocityXLimit = ChargeSpeed * 1.5f;
                            if (NPC.velocity.Length() < velocityXLimit)
                                NPC.velocity.Modulus *= 1.02f;
                        }

                        if (ShouldDecelerate)
                        {
                            NPC.damage = 0;
                            float playerLocation = NPC.Center.X - Target.Center.X;
                            NPC.direction = playerLocation < 0 ? 1 : -1;
                            NPC.spriteDirection = NPC.direction;
                            IsCharging = false;
                            NPC.velocity *= 0.8f;

                            if (NPC.velocity.Length() < 0.2f)
                            {
                                CheckPhaseChange();
                                SelectNextBehavior();
                                if (CurrentBehavior is not (Behavior.Phase1_NormalCharge or Behavior.Phase1_DirectCharge) && Main.rand.NextProbability(0.7f))
                                    AttackRandomVariation_Charge = !AttackRandomVariation_Charge;
                                break;
                            }
                        }
                        break;
                }
            }

            void Stinger()
            {
                NPC.damage = 0;

                TryMoveAboveTarget();

                Timer1++;
                int stingerAttackTimer = AttackRandomVariation_Stinger ? 18 : 12;

                if (Timer1 % stingerAttackTimer == 0)
                {
                    Vector2 stingerSpawnLocation = GetStingerSpawnLocation();
                    float stingerSpeed = Ultra ? 18f : 15f;
                    Vector2 stingerVelocity = (Target.Center - stingerSpawnLocation).ToCustomLength(stingerSpeed);

                    int numStingerShots = AttackRandomVariation_Stinger ? 8 : 12;
                    int num = Timer1 / stingerAttackTimer;

                    if (num > 0 && NPC.Bottom.Y < Target.Top.Y && Collision.CanHit(stingerSpawnLocation, 1, 1, Target.position, Target.width, Target.height))
                    {
                        SoundEngine.PlaySound(SoundID.Item17, stingerSpawnLocation);
                        if (TOSharedData.NotClient)
                        {
                            int type = Aroma ? (Phase1_2 ? ModContent.ProjectileType<PlagueStingerGoliathV2>() : ProjectileID.FlamingWood) : ProjectileID.QueenBeeStinger;

                            Projectile.NewProjectileAction(SourceAI, stingerSpawnLocation, stingerVelocity, type, StingerDamage, 0f, action: p =>
                            {
                                p.ai[1] = (Aroma && Phase1_2) ? Target.position.Y : 0f;
                                p.timeLeft = 600;
                            });

                            if (AttackRandomVariation_Stinger)
                            {
                                int numExtraStingers = 4;
                                for (int i = 0; i < numExtraStingers; i++)
                                {
                                    Projectile.NewProjectileAction(SourceAI, stingerSpawnLocation + Main.rand.NextVector2CircularEdge(16f, 16f) * (i + 1), stingerVelocity * MathHelper.Lerp(0.75f, 1f, i / (float)numExtraStingers), type, StingerDamage, 0f, action: p =>
                                    {
                                        p.ai[1] = (Aroma && Phase1_2) ? Target.position.Y : 0f;
                                        p.timeLeft = 600;
                                    });
                                }
                            }
                        }
                    }

                    if (num >= numStingerShots)
                    {
                        CheckPhaseChange();
                        SelectNextBehavior();
                        if (Main.rand.NextProbability(0.7f))
                            AttackRandomVariation_Stinger = !AttackRandomVariation_Stinger;
                    }
                }

                CheckPhaseChange();
            }

            void BeeSwarm()
            {
                Timer1++;

                switch (CurrentAttackPhase)
                {
                    case 0:
                        TryMoveAboveTarget();

                        if (Timer1 >= 40 && NPC.Bottom.Y < Target.Top.Y)
                        {
                            Timer1 = 0;
                            CurrentAttackPhase = 1;
                        }

                        CheckPhaseChange();
                        break;
                    case 1:
                        StopMovement(0.85f);
                        NPC.FaceTarget(Target);

                        switch (Timer1)
                        {
                            case 1:
                                SpawnCombCell(QueenBee_Handler.GetOwnedCombCellCenter(NPC), Vector2.Zero, 0, CombCell.Behavior.BeeSwarm, OwnedCombCellScaleMultiplier * NPC.scale);
                                break;
                            case 40:
                                SpawnFriendlyBee(NPC.Center, Vector2.Zero, FriendlyBee.Behavior_SwarmReminder, AttackRandomVariation_Stinger.ToDirectionInt());
                                break;
                            case 50:
                                Timer1 = 0;
                                CurrentAttackPhase = 2;
                                break;
                        }
                        break;
                    case 2:
                        NPC.velocity = Vector2.Zero;

                        int adjustedTimer = Timer1 - 1;
                        int attackTimer = 2;
                        if (adjustedTimer % attackTimer == 0)
                        {
                            int num = adjustedTimer / attackTimer;
                            float spread = MathHelper.ToRadians(Ultra ? 55f : 50f);

                            if (TOSharedData.NotClient)
                            {
                                for (int i = -1; i <= 1; i += 2)
                                {
                                    float speed = Main.rand.NextFloat(9f, 18f);
                                    Vector2 velocity = new PolarVector2(speed, MathHelper.ToRadians(2f * Math.Max(num - 15, 0) * AttackRandomVariation_Stinger.ToDirectionInt()) + Main.rand.NextFloat(-spread, spread)) * i;
                                    Projectile.NewProjectileAction<BeeProjectile>(SourceAI, NPC.Center, velocity, BeeDamage, 0f, action: p =>
                                    {
                                        p.ai[0] = BeeProjectile.Behavior_Rotate;
                                        p.ai[1] = Main.rand.NextFloat(0.012f, 0.015f) * (Ultra ? 1.25f : 1f) * Main.rand.NextDirectionInt();
                                    });

                                    if (Main.rand.NextBool(3))
                                    {
                                        velocity *= -1.3f;
                                        Projectile.NewProjectileAction<BeeProjectile>(SourceAI, NPC.Center, velocity, BeeDamage, 0f, action: p =>
                                        {
                                            p.ai[0] = BeeProjectile.Behavior_Rotate;
                                            p.ai[1] = Main.rand.NextFloat(0.012f, 0.015f) * (Ultra ? 1.25f : 1f) * Main.rand.NextDirectionInt();
                                        });
                                    }
                                }
                            }

                            if (Phase1_2 && num == 120)
                            {
                                Vector2 stingerSpawnLocation = GetStingerSpawnLocation();

                                SoundEngine.PlaySound(SoundID.Item17, stingerSpawnLocation);

                                if (TOSharedData.NotClient)
                                {
                                    float stingerSpeed = 15f;

                                    Vector2 projectileVelocity = (Target.Center - stingerSpawnLocation).ToCustomLength(stingerSpeed);
                                    int type = Aroma ? (Main.rand.NextBool() ? ModContent.ProjectileType<PlagueStingerGoliathV2>() : ProjectileID.QueenBeeStinger) : ProjectileID.QueenBeeStinger;
                                    int numProj = Ultra ? 11 : 7;

                                    float rotation = MathHelper.ToRadians(Ultra ? 90 : 60);
                                    for (int i = 0; i < numProj; i++)
                                    {
                                        Vector2 perturbedSpeed = projectileVelocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numProj - 1)));
                                        if (i % 2 != 0)
                                            perturbedSpeed *= 0.8f;

                                        Projectile.NewProjectileAction(SourceAI, stingerSpawnLocation + perturbedSpeed.ToCustomLength(10f), perturbedSpeed, type, StingerDamage, 0f, action: p =>
                                        {
                                            p.ai[1] = Aroma ? Target.position.Y : 0f;
                                            p.timeLeft = 600;

                                            if (!Aroma)
                                                p.tileCollide = false;
                                        });
                                    }
                                }
                            }
                            else if (Ultra && num >= 30 && num % 5 == 0)
                            {
                                Vector2 stingerSpawnLocation = GetStingerSpawnLocation();

                                SoundEngine.PlaySound(SoundID.Item17, stingerSpawnLocation);

                                if (TOSharedData.NotClient)
                                {
                                    float stingerSpeed = 15f;
                                    Vector2 stingerVelocity = (Target.Center - stingerSpawnLocation).ToCustomLength(stingerSpeed);

                                    int type = Aroma ? (Phase1_2 ? ModContent.ProjectileType<PlagueStingerGoliathV2>() : ProjectileID.FlamingWood) : ProjectileID.QueenBeeStinger;

                                    Projectile.NewProjectileAction(SourceAI, stingerSpawnLocation, stingerVelocity, type, StingerDamage, 0f, action: p =>
                                    {
                                        p.ai[1] = (Aroma && Phase1_2) ? Target.position.Y : 0f;
                                        p.timeLeft = 600;
                                    });
                                }
                            }

                            if (num >= 120)
                            {
                                Timer1 = 0;
                                CurrentAttackPhase = 3;
                            }
                        }
                        break;
                    case 3:
                        if (Timer1 >= 60)
                        {
                            CheckPhaseChange();
                            SelectNextBehavior();
                            if (Main.rand.NextProbability(0.7f))
                                AttackRandomVariation_BeeSwarm = !AttackRandomVariation_BeeSwarm;
                        }
                        break;
                }

                NPC.FaceTarget(Target);
                NPC.spriteDirection = NPC.direction;
            }

            void BeeSwarm2()
            {
                Timer1++;

                switch (CurrentAttackPhase)
                {
                    case 0:
                        TryMoveAboveTarget();

                        if (Timer1 >= 40 && NPC.Bottom.Y < Target.Top.Y)
                        {
                            Timer1 = 0;
                            CurrentAttackPhase = 1;
                        }

                        CheckPhaseChange();
                        break;
                    case 1:
                        StopMovement(0.85f);

                        NPC.FaceTarget(Target);

                        switch (Timer1)
                        {
                            case 1:
                                SpawnCombCell(NPC.Center, Vector2.Zero, 0, CombCell.Behavior.BeeSwarm2, OwnedCombCellScaleMultiplier * NPC.scale);
                                break;

                            case 75: //生成迷惑性蜂巢和安全蜂巢
                                int amount = Ultra ? Main.rand.Next(11, 16) : Main.rand.Next(6, 9);
                                int safeCombCellNumber = Main.rand.Next(amount);

                                if (TOSharedData.NotClient)
                                {
                                    PolarVector2 basis0 = new(0f);
                                    PolarVector2 basis1 = new(TOMathUtils.PiOver3 * 2);

                                    Vector2[] spawnCoordinates = new Vector2[amount];
                                    int value = Main.rand.Next(6);
                                    spawnCoordinates[0] = value switch
                                    {
                                        0 => new(3, 0),
                                        1 => new(-3, 0),
                                        2 => new(0, 3),
                                        3 => new(0, -3),
                                        4 => new(3, 3),
                                        5 => new(-3, -3),
                                        _ => new(3, 0)
                                    };
                                    for (int i = 1; i < amount; i++)
                                    {
                                        do spawnCoordinates[i] = new Vector2(Main.rand.Next(-4, 5), Main.rand.Next(-4, 5));
                                        while (!Check(i));

                                        bool Check(int i)
                                        {
                                            Vector2 newValue = spawnCoordinates[i];
                                            float length = (basis0 * newValue.X + basis1 * newValue.Y).Radius;
                                            if (length < 2.5f || length > (Ultra ? 5f : 3.3f))
                                                return false;

                                            for (int j = 0; j < i; j++)
                                            {
                                                if (newValue == spawnCoordinates[j])
                                                    return false;
                                            }

                                            return true;
                                        }
                                    }

                                    float scale = Ultra ? 0.525f : 0.7f;

                                    float radius = CombCell.HexagonRadius * MathF.Sqrt(3); //乘以sqrt(3)，得到内切圆直径，确保六边形各边相接
                                    radius -= 9.5f; //边框宽度为19像素
                                    radius *= scale; //缩放

                                    basis0 *= radius;
                                    basis1 *= radius;

                                    for (int i = 0; i < amount; i++)
                                    {
                                        Vector2 spawnCoordinate = spawnCoordinates[i];
                                        PolarVector2 offset = basis0 * spawnCoordinate.X + basis1 * spawnCoordinate.Y;
                                        bool safe = i == safeCombCellNumber;
                                        SpawnCombCell(QueenBee_Handler.GetOwnedCombCellCenter(NPC) + offset, Vector2.Zero, 0, safe ? CombCell.Behavior.BeeSwarm2_Safe : CombCell.Behavior.BeeSwarm2_Fake, scale, (p, c) =>
                                        {
                                            c.Offset = offset;
                                            if (safe)
                                            {
                                                SafeCombCellOffset = offset;
                                                SafeCombCell = p;
                                            }
                                        });
                                    }
                                }
                                break;

                            case 165:
                                if (HasSafeCombCell)
                                {
                                    Vector2 position = SafeCombCell.Center;
                                    int beeAmount = Ultra ? 12 : 8;
                                    for (int i = 0; i < beeAmount; i++)
                                    {
                                        float speed = Main.rand.NextFloat(7f, 11f);
                                        Vector2 velocity = new PolarVector2(speed, MathHelper.TwoPi / beeAmount * i).RotatedByRandom(MathHelper.ToRadians(10f));
                                        Projectile.NewProjectileAction<BeeProjectile>(SourceAI, position, velocity, BeeDamage, 0f, action: p => p.scale *= 1.5f);
                                    }
                                }
                                break;

                            case 345:
                                Timer1 = 0;
                                CurrentAttackPhase = 2;
                                break;
                        }
                        break;
                    case 2:
                        if (!HasSafeCombCell)
                        {
                            CurrentAttackPhase = 3;
                            break;
                        }

                        NPC.velocity = Vector2.Zero;

                        int adjustedTimer = Timer1 - 1;
                        int attackTimer = 2;
                        if (adjustedTimer % attackTimer == 0)
                        {
                            int num = adjustedTimer / attackTimer;
                            float spread = MathHelper.ToRadians(Ultra ? 55f : 50f);

                            if (TOSharedData.NotClient)
                            {
                                for (int i = 0; i < 5; i++)
                                {
                                    Vector2 velocity = Main.rand.NextPolarVector2(15f, 25f);
                                    Projectile.NewProjectileAction<KillerBeeSmall>(SourceAI, NPC.Center, velocity, BeeDamage, 0f, action: p =>
                                    {
                                        p.ai[0] = SafeCombCell.whoAmI;
                                        p.ai[1] = Main.rand.NextFloat(0.01f, 0.07f) * Main.rand.NextDirectionInt();
                                    });
                                }
                            }

                            if (num >= 120)
                            {
                                Timer1 = 0;
                                CurrentAttackPhase = 3;
                            }
                        }
                        break;
                    case 3:
                        if (Timer1 >= 135)
                        {
                            CheckPhaseChange();
                            SelectNextBehavior();
                            if (Main.rand.NextProbability(0.7f))
                                AttackRandomVariation_BeeSwarm = !AttackRandomVariation_BeeSwarm;
                        }
                        break;
                }

                NPC.FaceTarget(Target);
                NPC.spriteDirection = NPC.direction;
            }
        }

        void PhaseChange_1To2()
        {
            Timer1++;

            switch (CurrentAttackPhase)
            {
                case 0:
                    TryMoveAboveTarget();

                    if (Timer1 >= 40 && NPC.Bottom.Y < Target.Top.Y)
                    {
                        Timer1 = 0;
                        CurrentAttackPhase = 1;
                    }
                    break;
                case 1:
                    StopMovement(0.85f);
                    NPC.FaceTarget(Target);

                    switch (Timer1)
                    {
                        case 1:
                            SpawnCombCell(QueenBee_Handler.GetOwnedCombCellCenter(NPC), Vector2.Zero, 0, CombCell.Behavior.PhaseChange, OwnedCombCellScaleMultiplier * NPC.scale);
                            break;

                        case 90:
                            SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                            if (TOSharedData.NotClient)
                            {
                                Projectile.NewProjectileAction<BeeShockwave>(SourceAI, NPC.Center, Vector2.Zero, 100, 0f, action: p =>
                                {
                                    p.scale = 0f;
                                    BeeShockwave modP = p.GetModProjectile<BeeShockwave>();
                                    modP.Master = NPC;
                                });
                            }
                            ;
                            break;
                        case 161:
                            CurrentPhase = Phase.Phase2;
                            CurrentBehavior = Behavior.Phase2_BeeSwarm3;
                            CurrentAttackPhase = 0;
                            Timer1 = 0;
                            Timer2 = 0;
                            FinishedBehaviorCounter = 0;
                            CurrentAttackCounter = 0;
                            AttackRandomVariation_Charge = false;
                            AttackRandomVariation_Stinger = false;
                            AttackRandomVariation_BeeSwarm = false;

                            if (OwnCombCell)
                            {
                                CombCell modCombCell = ModOwnedCombCell;
                                modCombCell.BehaviorType = CombCell.Behavior.BeeSwarm3;
                                CurrentAttackPhase = 1;
                                Timer1 = 1; //跳过生成蜂巢的阶段
                            }
                            break;
                    }
                    break;
            }

            if (Timer1 is >= 100 and <= 160) //回复血量
            {
                float ratio = (Timer1 - 100f) / 60f;
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
        }

        void Phase2AI()
        {
            switch (CurrentBehavior)
            {
                case Behavior.Phase2_BeeSwarm3:
                    BeeSwarm3();
                    break;
                case Behavior.Phase2_Stinger:
                    Stinger();
                    break;
                default:
                    CheckPhaseChange();
                    SelectNextBehavior();
                    break;
            }

            void SelectNextBehavior()
            {
                Timer1 = 0;
                Timer2 = 0;
                CurrentAttackPhase = 0;
                ShouldDecelerate = false;

                SwitchToNext();

                void SwitchToNext()
                {
                    FinishedBehaviorCounter++;
                    int attackCycleLength = 2;

                    CurrentBehavior = (FinishedBehaviorCounter % attackCycleLength) switch
                    {
                        0 => Behavior.Phase2_BeeSwarm3,
                        1 => Behavior.Phase2_Stinger,

                        _ => Behavior.Phase2_Stinger
                    };
                }
            }

            void CheckPhaseChange()
            {
                if (NPC.LifeRatio <= Phase2_2LifeRatio)
                    CurrentPhase = Phase.Phase2_2;
            }

            void BeeSwarm3()
            {
                Timer1++;

                switch (CurrentAttackPhase)
                {
                    case 0:
                        TryMoveAboveTarget();

                        if (Timer1 >= 40 && NPC.Bottom.Y < Target.Top.Y)
                        {
                            Timer1 = 0;
                            CurrentAttackPhase = 1;
                        }

                        CheckPhaseChange();
                        break;
                    case 1:
                        StopMovement(0.85f);

                        NPC.FaceTarget(Target);

                        switch (Timer1)
                        {
                            case 1:
                                SpawnCombCell(NPC.Center, Vector2.Zero, 0, CombCell.Behavior.BeeSwarm3, OwnedCombCellScaleMultiplier * NPC.scale);
                                break;

                            case 40: //生成移动蜂巢
                                for (int i = 0; i < 12; i++)
                                {
                                    int initialPositionParameter = i / 2; //除以整数2来获得0-5的范围
                                    int direction = (i % 2 == 0).ToDirectionInt(); //偶数为顺，奇数为逆

                                    SpawnCombCell(QueenBee_Handler.GetOwnedCombCellCenter(NPC), Vector2.Zero, 0, CombCell.Behavior.BeeSwarm3_Move, 0.28f, (p, c) =>
                                    {
                                        c.CurrentPositionParameter = initialPositionParameter;
                                        c.MoveDirection = direction;
                                    });
                                }
                                break;

                            case 120:
                                Timer1 = 0;
                                CurrentAttackPhase = 2;
                                break;
                        }
                        break;
                    case 2:
                        NPC.velocity = Vector2.Zero;

                        int adjustedTimer = Timer1 - 1;
                        int attackTimer = Phase2_2 ? 55 : 70;
                        if (adjustedTimer % attackTimer == 0)
                        {
                            int num = adjustedTimer / attackTimer;
                            int attackAmount = Phase2_2 ? 7 : 5;

                            if (num <= attackAmount - 1 && TOSharedData.NotClient)
                            {
                                int amount = 100;
                                Projectile.NewProjectilesArc<BeeProjectile>(amount, MathHelper.TwoPi / amount, SourceAI, NPC.Center, NPC.GetVelocityTowards(Target.Center, 15f), BeeDamage, 0f, action: p =>
                                {
                                    p.ai[0] = BeeProjectile.Behavior_KilledByHoney;
                                    p.ai[1] = Main.rand.NextFloat(0.02f, 0.04f) * Main.rand.NextDirectionInt();
                                    p.ai[2] = Phase2_2 ? 3f : 5f;
                                    p.timeLeft = 300;
                                    p.velocity *= Main.rand.NextFloat(0.8f, 1.2f);
                                    p.velocity.Rotation += Main.rand.NextFloat(-0.03f, 0.03f);
                                });
                            }

                            if (num >= attackAmount + 1)
                            {
                                Timer1 = 0;
                                CurrentAttackPhase = 3;
                            }
                        }
                        break;
                    case 3:
                        if (Timer1 >= 45)
                        {
                            CheckPhaseChange();
                            SelectNextBehavior();
                        }
                        break;
                }
            }

            void Stinger()
            {
                NPC.damage = 0;

                Timer1++;
                switch (CurrentAttackPhase)
                {
                    case 0:
                        TryMoveAboveTarget();

                        int stingerAttackTimer = 45;

                        if (Timer1 % stingerAttackTimer == 0)
                        {
                            Vector2 stingerSpawnLocation = GetStingerSpawnLocation();
                            float stingerSpeed = 22f;
                            Vector2 stingerVelocity = (Target.Center - stingerSpawnLocation).ToCustomLength(stingerSpeed);

                            int numStingerShots = Phase2_2 ? 4 : 6;
                            int num = Timer1 / stingerAttackTimer;

                            if (num > 0 && NPC.Bottom.Y < Target.Top.Y && Collision.CanHit(stingerSpawnLocation, 1, 1, Target.position, Target.width, Target.height))
                            {
                                SoundEngine.PlaySound(HugeStingerShootSound, stingerSpawnLocation);
                                if (TOSharedData.NotClient)
                                    Projectile.NewProjectileAction<HugeStinger>(SourceAI, stingerSpawnLocation, stingerVelocity, StingerDamage, 0f, action: p => p.ai[1] = Target.Center.Y);
                            }

                            if (num >= numStingerShots)
                            {
                                if (Phase2_2)
                                {
                                    CurrentAttackPhase = 1;
                                    Timer1 = 0;
                                }
                                else
                                {
                                    CheckPhaseChange();
                                    SelectNextBehavior();
                                }
                            }
                        }
                        break;
                    case 1:
                        TryMoveAboveTarget(true);

                        int stingerAttackTimer2 = 70;

                        if (Timer1 % stingerAttackTimer2 == 0)
                        {
                            Vector2 stingerSpawnLocation = GetStingerSpawnLocation();
                            float stingerSpeed = 18f;
                            Vector2 stingerVelocity = (Target.Center - stingerSpawnLocation).ToCustomLength(stingerSpeed);

                            int numStingerShots = 4;
                            int num = Timer1 / stingerAttackTimer2;

                            if (num > 0 && NPC.Bottom.Y < Target.Top.Y && Collision.CanHit(stingerSpawnLocation, 1, 1, Target.position, Target.width, Target.height))
                            {
                                SoundEngine.PlaySound(HugeStingerShootSound, stingerSpawnLocation);
                                if (TOSharedData.NotClient)
                                {
                                    Vector2 projectileVelocity = (Target.Center - stingerSpawnLocation).ToCustomLength(stingerSpeed);
                                    int type = ProjectileID.QueenBeeStinger;
                                    int numProj = 25 - num * 4;

                                    float rotation = MathHelper.ToRadians(115 - num * 15);
                                    for (int i = 0; i < numProj; i++)
                                    {
                                        Vector2 perturbedSpeed = projectileVelocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numProj - 1)));
                                        if (i % 2 != 0)
                                            perturbedSpeed *= 0.8f;

                                        Projectile.NewProjectileAction(SourceAI, stingerSpawnLocation + perturbedSpeed.ToCustomLength(10f), perturbedSpeed, type, StingerDamage, 0f, action: p =>
                                        {
                                            p.timeLeft = 600;
                                            if (!Aroma)
                                                p.tileCollide = false;
                                        });
                                    }
                                }
                            }

                            if (num >= numStingerShots)
                            {
                                CheckPhaseChange();
                                SelectNextBehavior();
                            }
                        }
                        break;
                }
            }
        }
        #endregion 行为函数
    }
}