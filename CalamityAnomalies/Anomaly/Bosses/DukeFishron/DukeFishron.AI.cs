// Developed by ColdsUx

namespace CalamityAnomalies.Anomaly.Bosses.DukeFishron;

public sealed partial class DukeFishron
{
    public override bool PreAI()
    {
        if (!NPC.TargetClosestIfInvalid(false, DespawnDistance))
        {
            NPC.velocity.Y -= 0.4f;

            if (NPC.timeLeft > 10)
                NPC.timeLeft = 10;
        }

        if (CurrentPhase <= Phase.PhaseChange_2To3)
        {
            if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                NPC.alpha += 15;
            else
                NPC.alpha -= 15;

            if (NPC.alpha < 0)
                NPC.alpha = 0;
            if (NPC.alpha > 150)
                NPC.alpha = 150;
        }

        // Rotation
        float rateOfRotation = IsIdle ? 0.06f : CurrentBehavior switch
        {
            Behavior.Charge => 0f,
            Behavior.Bubble when Phase2 => 0f,
            Behavior.PhaseChange_1To2 or Behavior.PhaseChange_2To3 or Behavior.Sharknado => 0.02f,
            _ => 0.04f
        };

        Vector2 rotationVector = Target.Center - NPC.Center;

        float targetRotation = (float)Math.Atan2(rotationVector.Y, rotationVector.X);
        if (NPC.spriteDirection == 1)
            targetRotation += MathHelper.Pi;
        if (targetRotation < 0f)
            targetRotation += MathHelper.TwoPi;
        if (targetRotation > MathHelper.TwoPi)
            targetRotation -= MathHelper.TwoPi;

        bool shouldFaceHorizontally = CurrentPhase is Phase.Initialize or Phase.PhaseChange_1To2 or Phase.PhaseChange_2To3 || CurrentBehavior == Behavior.Sharknado;
        if (shouldFaceHorizontally)
            targetRotation = 0f;

        if (rateOfRotation != 0f)
            NPC.rotation = NPC.rotation.AngleTowards(targetRotation, rateOfRotation);

        bool shouldEnableShader = CurrentPhase switch
        {
            Phase.PhaseChange_2To3 => Timer1 >= PhaseChangeTime - 60,
            >= Phase.Phase3 => true,
            _ => false
        };
        if (shouldEnableShader) //适配原版猪鲨的天空效果
        {
            NPC.ai[2]++;
            NPC.ai[0] = 10f;
            NPC.alpha = Utils.Clamp(NPC.alpha, 120, 255);
        }

        switch (CurrentPhase)
        {
            case Phase.Initialize:
                Initialize();
                break;
            case Phase.Phase1 or Phase.Phase2 or Phase.Phase3 or Phase.Phase3_2:
                Phase123AI();
                break;
            case Phase.PhaseChange_1To2 or Phase.PhaseChange_2To3:
                PhaseChange();
                break;
        }

        return false;

        #region 行为函数

        void Initialize()
        {
            int spawnEffectPhaseTimer = InitializeTime;

            if (Timer1 == 0)
            {
                NPC.alpha = 255;
                NPC.rotation = 0f;
            }

            // Disable contact damage while spawning
            NPC.damage = 0;

            // Velocity
            NPC.velocity *= 0.98f;

            // Direction
            int faceDirection = GetPlayerDirection();
            if (faceDirection != 0)
            {
                NPC.direction = faceDirection;
                NPC.spriteDirection = -NPC.direction;
            }

            // Alpha
            if (Timer1 > 20)
            {
                NPC.velocity.Y = -2f;

                NPC.alpha -= 5;
                if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                    NPC.alpha += 15;
                if (NPC.alpha < 0)
                    NPC.alpha = 0;
                if (NPC.alpha > 150)
                    NPC.alpha = 150;
            }

            // Spawn dust and play sound
            if (Timer1 == spawnEffectPhaseTimer - 30)
            {
                int dustAmt = 36;
                for (int i = 0; i < dustAmt; i++)
                {
                    Vector2 dust = (Vector2.Normalize(NPC.velocity) * new Vector2(NPC.width / 2f, NPC.height) * 0.75f * 0.5f).RotatedBy((i - (dustAmt / 2 - 1)) * MathHelper.TwoPi / dustAmt) + NPC.Center;
                    Vector2 sharknadoDustDirection = dust - NPC.Center;
                    Dust.NewDustAction(dust + sharknadoDustDirection, 0, 0, DustID.DungeonWater, sharknadoDustDirection * 2f, d =>
                    {
                        d.alpha = 100;
                        d.scale = 1.4f;
                        d.noGravity = true;
                        d.noLight = true;
                        d.velocity = sharknadoDustDirection.ToCustomLength(3f);
                    });
                }

                SoundEngine.PlaySound(RoarSound, NPC.Center);
            }

            Timer1++;
            if (Timer1 >= spawnEffectPhaseTimer - 15)
            {
                CurrentBehavior = Behavior.Charge;
                Timer1 = 0;
                Timer5 = 0;
                CurrentAttackPhase = 0;
                CurrentAttackPhaseForIdle = 0;
                CurrentPhase = Phase.Phase1;
            }
        }

        int GetPlayerDirection() => Math.Sign(Target.Center.X - NPC.Center.X);

        void StopMovement()
        {
            NPC.velocity *= Ultra ? 0.95f : 0.965f;
            NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, 0f, Ultra ? 0.08f : 0.02f);
        }

        void Phase123AI()
        {
            if (IsIdle)
            {
                Timer5++;
                Idle();
            }
            else
            {
                switch (CurrentBehavior)
                {
                    case Behavior.Charge:
                        Charge();
                        break;
                    case Behavior.Bubble:
                        SpawnBubble();
                        break;
                    case Behavior.Sharknado:
                        SpawnSharknado();
                        break;
                    case Behavior.Phase3_Teleport:
                        Teleport();
                        break;
                    default:
                        SelectNextBehavior();
                        break;
                }
            }

            void SelectNextBehavior()
            {
                Timer1 = 0;
                Timer5 = 0;
                CurrentAttackPhase = 0;
                CurrentAttackPhaseForIdle = 0;

                if (InvalidPhase1)
                {
                    CurrentPhase = Phase.PhaseChange_1To2;
                    CurrentBehavior = Behavior.PhaseChange_1To2;
                    ChargeCounter = 0;
                }
                else if (InvalidPhase2)
                {
                    CurrentPhase = Phase.PhaseChange_2To3;
                    CurrentBehavior = Behavior.PhaseChange_2To3;
                    ChargeCounter = 0;
                }
                else
                {
                    switch (CurrentBehavior)
                    {
                        case Behavior.Charge:
                            ChargeCounter++;

                            switch (ChargeCounter)
                            {
                                case 5 when Phase1:
                                    CurrentBehavior = Behavior.Bubble;
                                    break;
                                case 10 when Phase1:
                                    CurrentBehavior = Behavior.Sharknado;
                                    ChargeCounter = 0;
                                    break;

                                case 3 when Phase2:
                                    CurrentBehavior = Behavior.Bubble;
                                    break;
                                case 6 when Phase2:
                                    CurrentBehavior = Behavior.Sharknado;
                                    ChargeCounter = 0;
                                    break;

                                case 1 or 3 when CurrentPhase == Phase.Phase3:
                                    CurrentBehavior = Behavior.Phase3_Teleport;
                                    break;
                                case 6 when CurrentPhase == Phase.Phase3:
                                    CurrentBehavior = Behavior.Phase3_Teleport;
                                    ChargeCounter = 0;
                                    break;
                            }

                            break;
                        default:
                            CurrentBehavior = Behavior.Charge;
                            break;
                    }

                    if (CurrentPhase == Phase.Phase3 && NPC.LifeRatio <= Phase3_2LifeRatio)
                    {
                        CurrentPhase = Phase.Phase3_2;
                        ChargeCounter = 0;
                    }
                }
            }

            void Idle()
            {
                NPC.damage = 0;

                switch (CurrentAttackPhaseForIdle)
                {
                    case 0:
                        OffsetX = (Phase3 ? 360 : 300) * Math.Sign((NPC.Center - Target.Center).X);
                        CurrentAttackPhaseForIdle = 1;
                        goto case 1;
                    case 1:
                        float idlePhaseAcceleration = Phase3 ? 0.8f : Phase2 ? 0.7f : 0.6f;
                        float idlePhaseVelocity = Phase3 ? 15f : Phase2 ? 12f : 10f;

                        if (Ultra)
                        {
                            idlePhaseAcceleration *= 1.15f;
                            idlePhaseVelocity *= 1.15f;
                        }

                        Vector2 idlePhaseDirection = (Target.Center + new Vector2(OffsetX, -200f) - NPC.Center - NPC.velocity).ToCustomLength(idlePhaseVelocity);
                        NPC.SimpleFlyMovement(idlePhaseDirection, idlePhaseAcceleration);

                        // Rotation and direction
                        int playerFaceDirection = GetPlayerDirection();
                        if (playerFaceDirection != 0)
                        {
                            if (Timer5 == 1 && playerFaceDirection != NPC.direction)
                            {
                                NPC.rotation += MathHelper.Pi;

                                if (Phase3)
                                {
                                    for (int i = 0; i < NPC.oldPos.Length; i++)
                                        NPC.oldPos[i] = Vector2.Zero;
                                }
                            }

                            NPC.direction = playerFaceDirection;

                            if (NPC.spriteDirection != -NPC.direction)
                                NPC.rotation += MathHelper.Pi;

                            NPC.spriteDirection = -NPC.direction;
                        }
                        break;
                }
            }

            void Charge()
            {
                NPC.damage = NPC.defDamage;

                switch (CurrentAttackPhase)
                {
                    case 0:
                        NPC.SetVelocityandRotation(NPC.GetVelocityTowards(Target.Center, GetChargeSpeed()));
                        CurrentAttackPhase = 1;

                        int playerFaceDirection = GetPlayerDirection();
                        if (playerFaceDirection != 0)
                        {
                            NPC.direction = playerFaceDirection;
                            NPC.spriteDirection = -NPC.direction;

                            if (NPC.spriteDirection == 1)
                                NPC.rotation += MathHelper.Pi;
                        }
                        goto case 1;
                    case 1:
                        NPC.velocity *= 1.01f;

                        if (Phase3) //逐渐变得不透明
                        {
                            NPC.alpha -= 25;
                            if (NPC.alpha < 0)
                                NPC.alpha = 0;
                        }

                        int chargeDustAmt = 7;
                        for (int j = 0; j < chargeDustAmt; j++)
                        {
                            Vector2 dust = (Vector2.Normalize(NPC.velocity) * new Vector2((NPC.width + 50) / 2f, NPC.height) * 0.75f).RotatedBy((j - (chargeDustAmt / 2 - 1)) * MathHelper.Pi / chargeDustAmt) + NPC.Center;
                            Vector2 chargeDustDirection = ((float)(Main.rand.NextDouble() * MathHelper.Pi) - MathHelper.PiOver2).ToRotationVector2() * Main.rand.Next(3, 8);

                            Dust.NewDustAction(dust + chargeDustDirection, 0, 0, DustID.DungeonWater, chargeDustDirection * 2f, d =>
                            {
                                d.alpha = 100;
                                d.scale = 1.4f;
                                d.noGravity = true;
                                d.noLight = true;
                                d.velocity /= 4f;
                                d.velocity -= NPC.velocity;
                            });
                        }

                        Timer1++;
                        if (Timer1 >= GetChargeTime())
                            SelectNextBehavior();

                        break;
                }

                float GetChargeSpeed()
                {
                    float chargeSpeed = Phase3 ? 33f : Phase2 ? 30f : 25f;
                    if (Ultra)
                        chargeSpeed *= 1.2f;
                    return chargeSpeed;
                }

                int GetChargeTime()
                {
                    int chargeTime = Ultra ? (Phase3 ? 27 : 30)
                        : (Phase3 ? 23 : Phase2 ? 25 : 27);
                    return chargeTime;
                }
            }

            void SpawnBubble()
            {
                if (Phase1)
                {
                    switch (CurrentAttackPhase)
                    {
                        case 0:
                            OffsetX = 300 * Math.Sign((NPC.Center - Target.Center).X);
                            SoundEngine.PlaySound(RoarSound, NPC.Center);
                            CurrentAttackPhase = 1;
                            break;
                        case 1:
                            float bubbleBelchPhaseVelocity = 10f;
                            float bubbleBelchPhaseAcceleration = 1.5f;
                            int bubbleBelchPhaseTimer = Ultra ? 50 : 70;
                            int bubbleBelchPhaseDivisor = Ultra ? 1 : 2;

                            Vector2 bubbleAttackDirection = Vector2.Normalize(Target.Center + new Vector2(OffsetX, -200f) - NPC.Center - NPC.velocity) * bubbleBelchPhaseVelocity;
                            NPC.SimpleFlyMovement(bubbleAttackDirection, bubbleBelchPhaseAcceleration);

                            if (Timer1 % bubbleBelchPhaseDivisor == 0f)
                            {
                                SoundEngine.PlaySound(SoundID.NPCDeath19, NPC.Center);

                                if (TOSharedData.NotClient)
                                {
                                    Vector2 bubbleSpawnDirection = Vector2.Normalize(Target.Center - NPC.Center) * (NPC.width + 20) / 2f + NPC.Center + new Vector2(0, 45f);
                                    NPC.NewNPCAction(SourceAI, bubbleSpawnDirection, NPCID.DetonatingBubble);
                                }
                            }

                            // Direction
                            int bubbleSpriteFaceDirection = GetPlayerDirection();
                            if (bubbleSpriteFaceDirection != 0)
                            {
                                NPC.direction = bubbleSpriteFaceDirection;
                                if (NPC.spriteDirection != -NPC.direction)
                                    NPC.rotation += MathHelper.Pi;
                                NPC.spriteDirection = -NPC.direction;
                            }

                            Timer1++;
                            if (Timer1 >= bubbleBelchPhaseTimer)
                                SelectNextBehavior();
                            break;
                    }
                }
                else //二阶段
                {
                    switch (CurrentAttackPhase)
                    {
                        case 0:
                            SoundEngine.PlaySound(RoarSound, NPC.Center);

                            float bubbleSpinPhaseVelocity = Ultra ? 50f : 25f;
                            NPC.SetVelocityandRotation(NPC.GetVelocityTowards(Target.Center, bubbleSpinPhaseVelocity));

                            int phase2SpriteFaceDirection = GetPlayerDirection();
                            if (phase2SpriteFaceDirection != 0)
                            {
                                NPC.direction = phase2SpriteFaceDirection;

                                if (NPC.spriteDirection == 1)
                                    NPC.rotation += MathHelper.Pi;

                                NPC.spriteDirection = -NPC.direction;
                            }

                            CurrentAttackPhase = 1;
                            break;
                        case 1:
                            int bubbleSpinPhaseTimer = Ultra ? 60 : 80;
                            int bubbleSpinPhaseDivisor = Ultra ? 1 : 2;
                            float bubbleSpinBubbleVelocity = 9f;
                            float bubbleSpinPhaseRotation = MathHelper.TwoPi / (bubbleSpinPhaseTimer / 2);

                            if (Timer1 % bubbleSpinPhaseDivisor == 0f)
                            {
                                SoundEngine.PlaySound(SoundID.NPCDeath19, NPC.Center);

                                if (TOSharedData.NotClient)
                                {
                                    Vector2 phase2BubbleSharkronDirection = Vector2.Normalize(NPC.velocity) * (NPC.width + 20) / 2f + NPC.Center + new Vector2(0, 45f);
                                    NPC.NewNPCAction(SourceAI, phase2BubbleSharkronDirection, NPCID.DetonatingBubble, action: n =>
                                    {
                                        n.target = NPC.target;
                                        n.velocity = NPC.velocity.RotatedBy(MathHelper.PiOver2 * NPC.direction).ToCustomLength(bubbleSpinBubbleVelocity * (Ultra ? (Main.rand.NextFloat() + 0.5f) : 1f));
                                        n.ai[3] = Main.rand.NextFloat(0.8f, 1.2f);
                                        n.netUpdate = true;
                                    });

                                    if (Timer1 % (bubbleSpinPhaseDivisor * 5) == 0f)
                                        NPC.NewNPCAction(SourceAI, phase2BubbleSharkronDirection, NPCID.Sharkron2, action: n => n.ai[1] = 89f);
                                }
                            }

                            // Velocity and rotation
                            float deltaRotation = bubbleSpinPhaseRotation * NPC.direction;
                            NPC.velocity.Rotation -= deltaRotation;
                            NPC.rotation -= deltaRotation;

                            Timer1++;
                            if (Timer1 >= bubbleSpinPhaseTimer)
                                SelectNextBehavior();

                            break;
                    }
                }
            }

            void SpawnSharknado()
            {
                int sharknadoPhaseTimer = SharknadoTime;

                // Velocity
                StopMovement();

                // Play sound and spawn sharknadoes
                if (Timer1 == sharknadoPhaseTimer - 30)
                {
                    SoundEngine.PlaySound(SoundID.Zombie9, NPC.Center);

                    if (TOSharedData.NotClient)
                    {
                        if (Phase1)
                        {
                            Vector2 sharknadoSpawnerDirection = NPC.rotation.ToRotationVector2() * (Vector2.UnitX * NPC.direction) * (NPC.width + 20) / 2f + NPC.Center;
                            bool normal = Main.rand.NextBool();
                            float velocityY = normal ? 8f : -4f;
                            float ai1 = normal ? 0f : -1f;

                            Projectile.NewProjectileAction(SourceAI, sharknadoSpawnerDirection, new Vector2(NPC.direction * 3, velocityY), ProjectileID.SharknadoBolt, 0, 0f, action: p => p.ai[1] = ai1);
                            Projectile.NewProjectileAction(SourceAI, sharknadoSpawnerDirection, new Vector2(-(float)NPC.direction * 3, velocityY), ProjectileID.SharknadoBolt, 0, 0f, action: p => p.ai[1] = ai1);

                            velocityY = normal ? -4f : 8f;
                            ai1 = normal ? -1f : 0f;
                            Projectile.NewProjectileAction(SourceAI, sharknadoSpawnerDirection, new Vector2(0f, velocityY), ProjectileID.SharknadoBolt, 0, 0f, action: p => p.ai[1] = ai1);
                        }

                        if (Phase2)
                        {
                            Projectile.NewProjectileAction(SourceAI, NPC.Center, Vector2.Zero, ProjectileID.SharknadoBolt, 0, 0f, action: p =>
                            {
                                p.ai[0] = 1f;
                                p.ai[1] = NPC.target + 1;
                                p.ai[2] = 1f;
                            });
                        }
                    }
                }

                Timer1++;
                if (Timer1 >= sharknadoPhaseTimer)
                    SelectNextBehavior();
            }

            void Teleport()
            {
                // Disable contact damage during the teleporting phase
                NPC.damage = 0;

                // Alpha
                if (NPC.alpha < 255)
                {
                    NPC.alpha += 17;
                    if (NPC.alpha > 255)
                        NPC.alpha = 255;
                }

                StopMovement();

                int teleportPhaseTimer = 36;

                if (Timer1 == teleportPhaseTimer / 2)
                {
                    SoundEngine.PlaySound(RoarSound, NPC.Center);

                    OffsetX = 300 * Math.Sign((NPC.Center - Target.Center).X);
                    NPC.Center = Target.Center + new Vector2(-OffsetX, -200f);

                    int phase3PlayerDirection = GetPlayerDirection();
                    if (phase3PlayerDirection != 0)
                    {
                        if (phase3PlayerDirection != NPC.direction)
                        {
                            NPC.rotation += MathHelper.Pi;
                            for (int n = 0; n < NPC.oldPos.Length; n++)
                                NPC.oldPos[n] = Vector2.Zero;
                        }

                        NPC.direction = phase3PlayerDirection;

                        if (NPC.spriteDirection != -NPC.direction)
                            NPC.rotation += MathHelper.Pi;

                        NPC.spriteDirection = -NPC.direction;
                    }
                }

                Timer1++;
                if (Timer1 >= teleportPhaseTimer)
                    SelectNextBehavior();
            }
        }

        void PhaseChange()
        {
            StopMovement();

            int phaseTransitionTimer = PhaseChangeTime;

            if (Timer1 == phaseTransitionTimer - 60)
                SoundEngine.PlaySound(RoarSound, NPC.Center);

            Timer1++;
            if (Timer1 >= phaseTransitionTimer)
            {
                CurrentBehavior = Behavior.Charge;
                Timer1 = 0;
                Timer5 = 0;
                CurrentAttackPhase = 0;
                CurrentAttackPhaseForIdle = 0;

                CurrentPhase = CurrentPhase switch
                {
                    Phase.PhaseChange_1To2 => Phase.Phase2,
                    Phase.PhaseChange_2To3 => NPC.LifeRatio <= Phase3_2LifeRatio ? Phase.Phase3_2 : Phase.Phase3,
                    _ => CurrentPhase
                };
            }
        }
        #endregion 行为函数
    }
}
