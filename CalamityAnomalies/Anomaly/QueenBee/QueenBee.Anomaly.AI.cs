// Developed by ColdsUx

using CalamityMod;
using CalamityMod.Projectiles.Boss;

namespace CalamityAnomalies.Anomaly.QueenBee;

public sealed partial class QueenBee_Anomaly : AnomalyNPCBehavior
{
    public override bool PreAI()
    {
        if (CurrentBehavior == Behavior.Despawn || !NPC.TargetClosestIfInvalid(true, DespawnDistance))
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
                CurrentBehavior = Behavior.Phase1_Charge;
                break;
            case >= Phase.Phase1 and <= Phase.Phase2_3:
                Phase1And2AI();
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

        void TryMoveAboveTarget()
        {
            Vector2 stingerSpawnLocation = GetStingerSpawnLocation();

            float stingerAttackSpeed = 22.5f;
            float stingerAttackAccel = Phase2_3 ? 0.45f : 0.35f;

            bool canHitTarget = CanHitTarget();
            Vector2 hoverDestination = Target.Center - Vector2.UnitY * (!canHitTarget ? 0f : Phase2 ? 400f : Phase1_2 ? 360f : 320f);
            Vector2 idealVelocity = NPC.SafeDirectionTo(hoverDestination) * stingerAttackSpeed;

            if (Vector2.Distance(stingerSpawnLocation, hoverDestination) > 40f || !canHitTarget)
                NPC.SimpleFlyMovement(idealVelocity, stingerAttackAccel);
        }

        void SpawnCombCell(Vector2 position, Vector2 velocity, int damage, CombCell.Behavior behavior, float finalScale) => Projectile.NewProjectileAction<CombCell>(SourceAI, position, velocity, damage, 0f, action: p =>
        {
            CombCell modP = p.GetModProjectile<CombCell>();
            modP.Master = NPC;
            modP.BehaviorType = behavior;
            modP.FinalScale = finalScale;
        });

        void Phase1And2AI()
        {
            switch (CurrentBehavior)
            {
                case Behavior.Phase1_Charge:
                    Charge();
                    break;
                case Behavior.Phase1_Stinger:
                    Stinger();
                    break;
                case Behavior.Phase1_Beehell:
                    Beehell();
                    break;
                default:
                    SelectNextAttack();
                    break;
            }

            void SelectNextAttack()
            {
                Timer1 = 0;
                Timer2 = 0;
                CurrentAttackPhase = 0;
                ShouldDecelerate = false;

                switch (CurrentBehavior)
                {
                    case Behavior.Phase1_Charge:
                        CurrentAttackCounter++;
                        int chargeAmount = 4;
                        if (CurrentAttackCounter >= chargeAmount)
                        {
                            CurrentAttackCounter = 0;
                            SwitchToNext();
                        }
                        break;
                    default:
                        SwitchToNext();
                        break;
                }

                void SwitchToNext()
                {
                    int attackCycleLength = 4;
                    FinishedBehaviorCounter++;
                    CurrentBehavior = (FinishedBehaviorCounter % attackCycleLength) switch
                    {
                        0 => Behavior.Phase1_Charge,
                        1 => Behavior.Phase1_Stinger,
                        2 => Behavior.Phase1_Charge,
                        3 => Behavior.Phase1_Beehell,

                        _ => Behavior.Phase1_Charge
                    };
                }
            }

            bool CheckPhaseChange()
            {
                if (ShouldEnterPhase3)
                {
                    CurrentPhase = Phase.PhaseChange_2To3;
                    CurrentBehavior = Behavior.PhaseChange_2To3;
                    return true;
                }
                else if (NPC.LifeRatio <= Phase2_3LifeRatio)
                    CurrentPhase = Phase.Phase2_3;
                else if (NPC.LifeRatio <= Phase2_2LifeRatio)
                    CurrentPhase = Phase.Phase2_2;
                else if (NPC.LifeRatio <= Phase2LifeRatio)
                    CurrentPhase = Phase.Phase2;
                else if (NPC.LifeRatio <= Phase1_3LifeRatio)
                    CurrentPhase = Phase.Phase1_3;
                else if (NPC.LifeRatio <= Phase1_2LifeRatio)
                    CurrentPhase = Phase.Phase1_2;
                return false;
            }

            /*
            void SpawnBee()
            {
                float acceleration = 0.48f;
                float speed = 22.5f;

                bool canHitTarget = Collision.CanHit(NPC.Center, 1, 1, Target.position, Target.width, Target.height);
                float distanceAboveTarget = !canHitTarget ? 0f : 320f;
                Vector2 hoverDestination = Target.Center - Vector2.UnitY * distanceAboveTarget;
                Vector2 idealVelocity = NPC.GetVelocityTowards(hoverDestination, speed);

                switch (CurrentAttackPhase)
                {
                    case 0: //向玩家移动，准备生成蜜蜂
                        NPC.damage = 0;
                        NPC.FaceTarget(Target);
                        NPC.spriteDirection = NPC.direction;

                        NPC.SimpleFlyMovement(idealVelocity, acceleration);

                        Timer1++;
                        if ((Vector2.Distance(NPC.Center, hoverDestination) < 400f && canHitTarget) || Timer1 >= 90)
                        {
                            CurrentAttackPhase = 1;
                            Timer1 = 0;
                        }
                        break;
                    case 1:
                        NPC.damage = 0;

                        Vector2 beeSpawnLocation = new(NPC.Center.X + (Main.rand.Next(20) * NPC.direction), NPC.position.Y + NPC.height * 0.8f);
                        Vector2 beeSpawnCollisionLocation = new(beeSpawnLocation.X, beeSpawnLocation.Y - 30f);

                        if (Vector2.Distance(beeSpawnLocation, hoverDestination) > 400f || !canHitTarget)
                            NPC.SimpleFlyMovement(idealVelocity, acceleration);
                        else
                            StopMovement(0.85f);

                        NPC.FaceTarget(Target);
                        NPC.spriteDirection = NPC.direction;

                        // 蜜蜂生成限制
                        int totalBees = 0;
                        int totalHornets = 0;

                        foreach (NPC n in NPC.ActiveNPCs)
                        {
                            //ai[3] == 1f 表示此为蜂后召唤的仆从蜜蜂或黄蜂
                            if (n.type is NPCID.Bee or NPCID.BeeSmall && n.ai[3] == 1f)
                                totalBees++;
                            else if (n.type is NPCID.LittleHornetHoney or NPCID.HornetHoney or NPCID.BigHornetHoney)
                                totalHornets++;
                        }

                        //待重构：将蜜蜂NPC改为蜜蜂弹幕
                        bool beeLimitReached = totalBees >= beeLimit;
                        bool hornetLimitReached = totalHornets >= hornetLimit;

                        int beeSpawnTimerIncrement = Player.AlivePlayers.Count(p => NPC.Distance(p.Center) < 1000f) / 2 + 1 + Phase1_2.ToInt();
                        Timer1 += beeSpawnTimerIncrement;

                        float beeSpawnCheck = 15;
                        int spawnNum = 3;
                        if (Timer1 >= beeSpawnCheck)
                        {
                            Timer1 = 0;
                            AttackCounter++;

                            if (AttackCounter > spawnNum || (beeLimitReached && hornetLimitReached))
                            {
                                CheckPhaseChange();
                                SelectNextAttack();
                                break;
                            }
                            else if (Collision.CanHit(beeSpawnLocation, 1, 1, Target.position, Target.width, Target.height))
                            {
                                SoundEngine.PlaySound(SoundID.NPCHit18, beeSpawnLocation);
                                if (TOSharedData.GeneralClient)
                                {
                                    int spawnType = Main.rand.NextBool(2) ? NPCID.Bee : NPCID.BeeSmall;

                                    if (Aroma)
                                    {
                                        if (Phase1_3)
                                            spawnType = Main.rand.NextBool(3) ? ModContent.NPCType<PlagueChargerLarge>() : ModContent.NPCType<PlagueCharger>();
                                        else
                                            spawnType = NPCID.Hellbat;
                                    }
                                    else
                                    {
                                        int random = hornetLimitReached ? 0 : beeLimitReached ? Main.rand.Next(6, 12) : Main.rand.Next(12);
                                        switch (random)
                                        {
                                            case 6:
                                            case 7:
                                            case 8:
                                                spawnType = NPCID.LittleHornetHoney;
                                                break;
                                            case 9:
                                            case 10:
                                                spawnType = NPCID.HornetHoney;
                                                break;
                                            case 11:
                                                spawnType = NPCID.BigHornetHoney;
                                                break;
                                        }
                                    }

                                    NPC.NewNPCAction(SourceAI, beeSpawnLocation, spawnType, action: n =>
                                    {
                                        n.velocity = n.GetVelocityTowards(Target.Center, 5f);

                                        if (!Aroma)
                                        {
                                            n.ai[2] = 1f; //enrageScale
                                            n.ai[3] = 1f; //标记之为蜂后召唤的仆从
                                        }
                                        n.timeLeft = 600;
                                        n.netUpdate = true;
                                    });
                                }
                            }
                        }
                        break;
                }
            }
            */

            void Charge()
            {
                float speed = MathHelper.Lerp(22.5f, 28f, NPC.LostLifeRatio);

                switch (CurrentAttackPhase)
                {
                    case 0:
                        NPC.damage = 0;

                        float distanceFromTargetX = Math.Abs(NPC.Center.X - Target.Center.X);
                        float distanceFromTargetY = Math.Abs(NPC.Center.Y - Target.Center.Y);
                        if (distanceFromTargetY < chargeDistanceY && distanceFromTargetX >= chargeDistanceX && distanceFromTargetX <= chargeDistanceX * 2f)
                        {
                            NPC.damage = NPC.defDamage;
                            IsCharging = true;
                            CurrentAttackPhase = 1;

                            NPC.velocity = NPC.GetVelocityTowards(Target.Center, speed);

                            NPC.FaceTarget(Target);
                            NPC.spriteDirection = NPC.direction;
                            SoundEngine.PlaySound(SoundID.Zombie125, NPC.Center);
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

                            if (NPC.Center.Y < Target.Center.Y - chargeDistanceY)
                                NPC.velocity.Y += approachAccelerationY;
                            else if (NPC.Center.Y > Target.Center.Y + chargeDistanceY)
                                NPC.velocity.Y -= approachAccelerationY;
                            else
                                NPC.velocity.Y *= 0.7f;

                            if (NPC.velocity.Y < -approachVelocityY)
                                NPC.velocity.Y = -approachVelocityY;
                            if (NPC.velocity.Y > approachVelocityY)
                                NPC.velocity.Y = approachVelocityY;

                            float distanceXMax = 100f;
                            float distanceXMin = 20f;
                            if (distanceFromTargetX > chargeDistanceX + distanceXMax)
                                NPC.velocity.X += approachAccelerationX * NPC.direction;
                            else if (distanceFromTargetX < chargeDistanceX + distanceXMin)
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
                        if (NPC.direction == chargeDirection && Math.Abs(NPC.Center.X - Target.Center.X) > chargeDistanceX)
                            ShouldDecelerate = true;
                        if (Math.Abs(NPC.Center.Y - Target.Center.Y) > chargeDistanceX * 1.5f)
                            ShouldDecelerate = true;

                        if (ShouldDecelerate)
                        {
                            NPC.damage = 0;
                            float playerLocation = NPC.Center.X - Target.Center.X;
                            NPC.direction = playerLocation < 0 ? 1 : -1;
                            NPC.spriteDirection = NPC.direction;
                            IsCharging = false;
                            NPC.velocity *= 0.8f;

                            float chargeDeceleration = 0.2f;
                            if (Phase1_2)
                            {
                                NPC.velocity *= 0.9f;
                                chargeDeceleration += 0.05f;
                            }
                            if (Phase2)
                            {
                                NPC.velocity *= 0.8f;
                                chargeDeceleration += 0.1f;
                            }

                            if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < chargeDeceleration)
                            {
                                CheckPhaseChange();
                                SelectNextAttack();
                                break;
                            }
                        }
                        else
                        {
                            if (NPC.velocity.Length() < speed)
                                NPC.velocity.X = speed * NPC.direction;

                            int accelerateGateValue = 30;

                            Timer2++;
                            if (Timer2 > accelerateGateValue) //加速
                            {
                                float velocityXLimit = speed * 2f;
                                if (Math.Abs(NPC.velocity.X) < velocityXLimit)
                                    NPC.velocity.X *= 1.02f;
                            }

                            IsCharging = true;
                        }
                        break;
                }
            }

            void Stinger()
            {
                NPC.damage = 0;
                float playerLocation = NPC.Center.X - Target.Center.X;
                NPC.direction = playerLocation < 0 ? 1 : -1;
                NPC.spriteDirection = NPC.direction;

                Vector2 stingerSpawnLocation = GetStingerSpawnLocation();

                TryMoveAboveTarget();

                Timer1++;
                int stingerAttackTimer = Phase2_3 ? 24 : Phase1_2 ? 18 : 12;

                if (Timer1 % stingerAttackTimer == 0)
                {
                    int numStingerShots = Phase2_3 ? 7 : Phase1_2 ? 9 : 12;
                    int num = Timer1 / numStingerShots;

                    if (num > numStingerShots)
                    {
                        CheckPhaseChange();
                        SelectNextAttack();
                        return;
                    }
                    else if (num > 0 && NPC.Bottom.Y < Target.Top.Y && Collision.CanHit(stingerSpawnLocation, 1, 1, Target.position, Target.width, Target.height))
                    {
                        SoundEngine.PlaySound(SoundID.Item17, stingerSpawnLocation);
                        if (TOSharedData.NotClient)
                        {
                            float stingerSpeed = Ultra ? 18f : 15f;
                            Vector2 stingerVelocity = (Target.Center - stingerSpawnLocation).ToCustomLength(stingerSpeed);
                            int type = Aroma ? (Phase1_3 ? ModContent.ProjectileType<PlagueStingerGoliathV2>() : ProjectileID.FlamingWood) : ProjectileID.QueenBeeStinger;

                            Projectile.NewProjectileAction(SourceAI, stingerSpawnLocation, stingerVelocity, type, StingerDamage, 0f, action: p =>
                            {
                                p.ai[1] = (Aroma && Phase1_3) ? Target.position.Y : 0f;
                                p.timeLeft = 600;
                            });

                            if (Phase1_2)
                            {
                                int numExtraStingers = Phase2_3 ? 4 : 2;
                                for (int i = 0; i < numExtraStingers; i++)
                                {
                                    Projectile.NewProjectileAction(SourceAI, stingerSpawnLocation + Main.rand.NextVector2CircularEdge(16f, 16f) * (i + 1), stingerVelocity * MathHelper.Lerp(0.75f, 1f, i / (float)numExtraStingers), type, StingerDamage, 0f, action: p =>
                                    {
                                        p.timeLeft = 600;
                                    });
                                }
                            }

                            if (num == numStingerShots) //生成蜂巢
                                SpawnCombCell(stingerSpawnLocation, stingerVelocity * 0.4f, StingerDamage, CombCell.Behavior.Beehive, 0.2f);
                        }
                    }
                }
            }

            void Beehell()
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
                        StopMovement();

                        switch (Timer1)
                        {
                            case 1:
                                SpawnCombCell(NPC.Center, Vector2.Zero, 0, CombCell.Behavior.Beehell, 0.95f);
                                break;
                            case 25:
                                Timer1 = 0;
                                CurrentAttackPhase = 2;
                                break;
                        }
                        break;
                    case 2:
                        StopMovement(0.9f);

                        int adjustedTimer = Timer1 - 1;
                        int attackTimer = 25;
                        if (adjustedTimer % attackTimer == 0)
                        {
                            int num = adjustedTimer / attackTimer;

                            int amount = (int)MathHelper.Lerp(16, 20, NPC.LostLifeRatio) + num;
                            float speed = Main.rand.NextFloat(12f, 15f) + num * 2f;
                            Vector2 velocity = Main.rand.NextPolarVector2(speed);
                            Projectile.RotatedProj<BeeProjectile>(amount, MathHelper.TwoPi / amount, SourceAI, NPC.Center, velocity, 10, 0f, action: p =>
                            {
                                p.velocity *= Main.rand.NextFloat(0.9f, 1.2f);
                                p.Timer1 += 120;
                            });

                            if (num >= 4)
                            {
                                Timer1 = 0;
                                CurrentAttackPhase = 3;
                            }
                        }
                        break;
                    case 3:
                        if (Timer1 >= 50)
                            SelectNextAttack();
                        break;
                }
            }
        }

        void PhaseChange_2To3()
        {

        }

        void Phase3AI()
        {
            switch (CurrentBehavior)
            {
                default:
                    CheckPhaseChange();
                    SelectNextAttack();
                    break;
            }

            void SelectNextAttack()
            {

            }

            void CheckPhaseChange()
            {
                if (NPC.LifeRatio <= Phase3_2LifeRatio)
                    CurrentPhase = Phase.Phase3_2;
            }
        }
        #endregion 行为函数
    }
}