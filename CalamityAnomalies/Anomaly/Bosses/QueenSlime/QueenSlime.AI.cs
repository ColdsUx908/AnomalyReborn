// Developed by ColdsUx

using CalamityAnomalies.Anomaly.Bosses.KingSlime;
using CalamityMod.Dusts;

namespace CalamityAnomalies.Anomaly.Bosses.QueenSlime;

public sealed partial class QueenSlime
{
    public override bool PreAI()
    {
        if (CurrentBehavior == Behavior.Despawn || !NPC.TargetClosestIfInvalid(true, DespawnDistance))
        {
            Despawn();
            ChangeScale();
            return false;
        }
        else
            NPC.FaceTarget(Target);

        switch (CurrentPhase)
        {
            case Phase.Initialize:
                Timer1++;
                if (NPC.velocity.Y == 0f)
                {
                    NPC.velocity = Vector2.Zero;
                    LastSpawnSlimeLife = NPC.life;
                    CurrentPhase = Phase.Phase1;
                    Phase1AI();
                }
                else if (Timer1 > 600)
                {
                    CurrentBehavior = Behavior.Despawn;
                    Despawn();
                }
                break;

            case Phase.Phase1 or Phase.Phase1_2:
                Phase1AI();
                break;

            case Phase.Phase2 or Phase.Phase2_2:
                Phase2AI();
                break;
        }

        return false;

        #region 行为函数
        bool StopHorizontalMovement()
        {
            NPC.velocity.X *= 0.85f;
            float velocityXLength = Math.Abs(NPC.velocity.X);
            if (velocityXLength < 0.1f)
            {
                NPC.velocity.X = 0f;
                return true;
            }
            else if (velocityXLength > 0.2f)
                NPC.velocity.X -= 0.15f * Math.Sign(NPC.velocity.X);
            return false;
        }

        void MakeSlimeDust(int amount, float velocityMultiplier)
        {
            for (int i = 0; i < amount; i++)
            {
                Dust.NewDustAction(NPC.Center, NPC.width + 25, NPC.height, DustID.TintableDust, Vector2.Zero, d =>
                {
                    d.color = NPC.AI_121_QueenSlime_GetDustColor() with { A = 150 };
                    d.noGravity = true;
                    d.velocity *= velocityMultiplier;
                });
            }
        }

        void MakeSmokeDust()
        {
            float slamDustRadius = 100f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 slamDustArea = NPC.Center + Main.rand.NextPolarVector2(slamDustRadius);
                Dust.NewDustPerfectAction(slamDustArea, DustID.Smoke, d =>
                {
                    d.alpha = 250;
                    d.color = NPC.AI_121_QueenSlime_GetDustColor();
                    d.scale = 1.8f;
                    d.position = slamDustArea;
                    d.velocity = Main.rand.NextPolarVector2(8f);
                    d.noGravity = true;
                    d.customData = NPC;
                });
            }
        }

        void Despawn()
        {
            StopHorizontalMovement(); //停止水平移动，避免奇怪的滑行现象
            NPC.dontTakeDamage = true;
            NPC.damage = 0;
            DespawnScaleMultiplier *= 0.97f;
            MakeSlimeDust((int)Utils.Remap(NPC.scale, 0f, 1f, 2f, 10f), 1f);

            if (NPC.scale < 0.2f) //体积足够小时执行脱战逻辑
            {
                NPC.active = false;
                NPC.netUpdate = true;
            }
        }

        void SpawnJewelParticle(NPC jewel, int amount)
        {
            for (int i = 0; i < amount; i++)
                JewelHandler.SpawnOrbParticle(jewel, Main.rand.NextFloat(5f, 10f), Main.rand.Next(40, 60), Main.rand.NextFloat(0.4f, 0.7f));
        }

        void ChangeScale() => NPC.ChangeScaleFixBottom(114, 100, TeleportScaleMultiplier * DespawnScaleMultiplier);

        Vector2 GetJewelSpawnPosition() => NPC.Top + new Vector2(0, -NPC.height);

        void SpawnJewelAction(NPC n)
        {
            n.Master = NPC;
            n.netUpdate = true;
            SoundEngine.PlaySound(JewelHandler.SpawnSound, GetJewelSpawnPosition());
            SpawnJewelParticle(n, 50);
        }

        void SpawnSpikeAction_NoGravity(Projectile p)
        {
            p.ai[1] = -2f;
            if (p.timeLeft > 600)
                p.timeLeft = 600;
        }

        void SpawnSpikeAction_Scaled(Projectile p)
        {
            p.scale *= 1.5f;
            if (p.timeLeft > 600)
                p.timeLeft = 600;
        }

        void TrySpawnMinions()
        {
            if (!TOSharedData.NotClient || NPC.Master is not null) //史莱姆皇后召唤的史莱姆皇后不再召唤仆从
                return;

            /*
            if (NPC.LifeRatio <= Phase1_2LifeRatio && !JewelAmethystSpawned)
            {
                NPC.NewNPCAction<AmethystJewel>(SourceAI, GetJewelSpawnPosition(), NPC.whoAmI, action: n =>
                {
                    SpawnJewelAction(n);
                    JewelAmethyst = n;
                    JewelAmethystSpawned = true;
                });
            }
            */

            float distance = (float)(LastSpawnSlimeLife - NPC.life) / NPC.lifeMax;
            float distanceNeeded = SpawnSlimeDistance;
            if (distance >= distanceNeeded)
            {
                LastSpawnSlimeLife = NPC.life;
                int spawnAmount = Main.rand.Next(1, Aroma ? (int)MathHelper.Lerp(3f, 6f, NPC.LostLifeRatio) : 2) + Math.Clamp((int)Math.Pow(distance / distanceNeeded, SpawnSlimePow), 0, 5);

                for (int i = 0; i < spawnAmount; i++)
                {
                    int type = Main.rand.Next(1, 4) switch
                    {
                        1 => NPCID.QueenSlimeMinionBlue,
                        2 => NPCID.QueenSlimeMinionPink,
                        3 => NPCID.QueenSlimeMinionPurple,

                        _ => NPCID.QueenSlimeMinionBlue
                    };

                    SpawnSlime(type);
                }

                if (Aroma && Main.rand.NextBool(1000)) //GFB世界中0.1%概率生成史莱姆皇后！
                {
                    NPC.NewNPCAction(NPC.GetBossSpawnSource(Target.whoAmI), NPC.Center, NPCID.KingSlime, action: n =>
                    {
                        n.Master = NPC;
                        SoundEngine.PlaySound(SoundID.Roar, n.Center);
                        TOLocalizationUtils.ChatLocalizedText(this, "GFBSummon", Color.HotPink);
                    });
                }

                void SpawnSlime(int type)
                {
                    float spawnZoneWidth = NPC.width / 2f - 16f;
                    float spawnZoneHeight = NPC.height - 32f;
                    Vector2 spawnPosition = new(NPC.Center.X + Main.rand.NextFloat(-spawnZoneWidth, spawnZoneWidth), NPC.Bottom.Y - Main.rand.NextFloat(spawnZoneHeight));
                    NPC.NewNPCAction(SourceAI, spawnPosition, type, action: n =>
                    {
                        n.velocity = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-3f, 0f));
                        n.ai[0] = -500 * Main.rand.Next(3);
                        n.Master = NPC;
                    });
                }
            }
        }

        void CreateSmash(int smashAmount, float singleDelay)
        {
            if (!TOSharedData.NotClient)
                return;

            int smashAmount2 = smashAmount - 1;
            int type = ProjectileID.QueenSlimeSmash;
            int maxSmashesPerSide = smashAmount2 / 2;
            float maxExpandDelay = 15f * maxSmashesPerSide;
            float smashSpawnDistanceOffset = 200f;
            float maxSmashOffset = smashAmount2 * 100f;

            for (int j = 0; j <= maxSmashesPerSide; j++)
            {
                float delay3 = singleDelay * j;
                float offset = smashSpawnDistanceOffset * j;
                Projectile.NewProjectileAction(SourceAI, NPC.Bottom + new Vector2(offset, 0f), Vector2.Zero, type, SlamDamage, 0f, action: p => p.ai[0] = -delay3);
                if (j != 0) //j == 0 时生成的是中间的弹幕，不需要生成两侧的弹幕
                    Projectile.NewProjectileAction(SourceAI, NPC.Bottom + new Vector2(-offset, 0f), Vector2.Zero, type, SlamDamage, 0f, action: p => p.ai[0] = -delay3);
            }
        }

        void FlyMovement()
        {
            float flyVelocity = Ultra ? 25f : 20f;
            float flyAcceleration = Ultra ? 0.22f : 0.18f;
            float flyDistanceY = 450f;

            Vector2 desiredVelocity = NPC.Center;

            if (NPC.timeLeft > 10)
            {
                if (!Collision.CanHit(NPC, Target))
                {
                    bool flyToSolidTilesAboveTarget = false;
                    Vector2 center = Target.Center;
                    for (int i = 0; i < 16; i++)
                    {
                        float tileDistanceAboveTarget = 16 * i;
                        Point point = (center + new Vector2(0f, 0f - tileDistanceAboveTarget)).ToTileCoordinates();
                        if (WorldGen.SolidOrSlopedTile(point.X, point.Y))
                        {
                            desiredVelocity = center + new Vector2(0f, 0f - tileDistanceAboveTarget + 16f) - NPC.Center;
                            flyToSolidTilesAboveTarget = true;
                            break;
                        }
                    }

                    if (!flyToSolidTilesAboveTarget)
                        desiredVelocity = center - NPC.Center;
                }
                else
                    desiredVelocity = Target.Center + new Vector2(0f, -flyDistanceY) - NPC.Center;
            }
            else
                desiredVelocity = NPC.Center + new Vector2(500f * NPC.direction, -flyDistanceY) - NPC.Center;

            float distanceFromFlightTarget = desiredVelocity.Length();
            if (Math.Abs(desiredVelocity.X) < 40f)
                desiredVelocity.X = NPC.velocity.X;

            if (distanceFromFlightTarget > 100f && ((NPC.velocity.X < -12f && desiredVelocity.X > 0f) || (NPC.velocity.X > 12f && desiredVelocity.X < 0f)))
                flyAcceleration = 0.2f;

            if (distanceFromFlightTarget < 40f)
            {
                desiredVelocity = NPC.velocity;
            }
            else if (distanceFromFlightTarget < 80f)
            {
                desiredVelocity = desiredVelocity.SafeNormalize(Vector2.UnitY);
                desiredVelocity *= flyVelocity * 0.65f;
            }
            else
            {
                desiredVelocity = desiredVelocity.SafeNormalize(Vector2.UnitY);
                desiredVelocity *= flyVelocity;
            }

            NPC.SimpleFlyMovement(desiredVelocity, flyAcceleration);
            NPC.rotation = NPC.velocity.X * 0.1f;
            if (NPC.rotation > 0.5f)
                NPC.rotation = 0.5f;

            if (NPC.rotation < -0.5f)
                NPC.rotation = -0.5f;
        }

        void SpawnOrbParticles(int amount, float maxSpeed)
        {
            for (int i = 0; i < amount; i++)
                ParticleHandler.SpawnParticle(new OrbParticle(NPC.Center, Main.rand.NextPolarVector2(maxSpeed * 0.5f, maxSpeed), Main.rand.Next(20, 30), Main.rand.NextFloat(0.4f, 0.85f), Color.Lerp(Color.Purple, Color.HotPink, Main.rand.NextFloat(0.5f)), lifeEndRatio: 0.925f));
        }

        void Phase1AI()
        {
            NPC.noTileCollide = false;
            NPC.noGravity = false;

            Timer5++;

            if (IsAttacking)
            {
                switch (CurrentBehavior)
                {
                    case Behavior.Phase1_FirstJump or Behavior.Phase1_NormalJump or Behavior.Phase1_HighJump:
                        Jump();
                        break;
                    case Behavior.Phase1_SlamDown:
                        SlamDown();
                        break;
                    case Behavior.Phase1_Teleport:
                        Teleport();
                        break;
                    default:
                        CheckPhaseChange();
                        SelectNextBehavior();
                        break;
                }
            }
            else
            {
                StopHorizontalMovement();
                CheckPhaseChange();
            }

            TeleportTimer += Utils.Remap((Target.Center - NPC.Center).Y, 0f, 1200f, 1f, 5f);

            TrySpawnMinions();
            ChangeScale();

            void SelectNextBehavior()
            {
                Timer5 = 0;

                if (TeleportTimer > 1500f || !NPC.WithinRange(Target.Center, 2400f))
                {
                    CurrentAttackCounter = 0;
                    CurrentBehavior = Behavior.Phase1_Teleport;
                    TeleportTimer = 0f;
                    Timer5 = 40;
                }
                else
                {
                    switch (CurrentBehavior)
                    {
                        case Behavior.Phase1_FirstJump or Behavior.Phase1_NormalJump:
                            CurrentAttackCounter++;
                            if (CurrentAttackCounter >= 3)
                            {
                                CurrentAttackCounter = 0;
                                CurrentBehavior = Behavior.Phase1_HighJump;
                            }
                            else
                                CurrentBehavior = Behavior.Phase1_NormalJump;
                            Timer5 = 40;
                            break;
                        case Behavior.Phase1_HighJump:
                            CurrentBehavior = Behavior.Phase1_SlamDown;
                            break;
                        case Behavior.Phase1_SlamDown:
                            CurrentBehavior = Behavior.Phase1_FirstJump;
                            break;
                        case Behavior.Phase1_Teleport:
                        default:
                            CurrentBehavior = Main.rand.Next(1, 6) switch
                            {
                                1 or 2 or 3 => Behavior.Phase1_FirstJump,
                                4 or 5 => Behavior.Phase1_SlamDown,
                                _ => Behavior.Phase1_FirstJump
                            };
                            break;
                    }
                }
                CurrentAttackPhase = 0;
                DirectionChangeCounter = 0;
                Timer1 = 0;
                Timer2 = 0;
            }

            void Jump()
            {
                bool highJump = CurrentBehavior == Behavior.Phase1_HighJump;

                switch (CurrentAttackPhase)
                {
                    case 0: //静止一段时间后起跳
                        Timer2++;

                        NPC.damage = 0;
                        NPC.GravityMultiplier *= Ultra && NPC.velocity.Y > 0f ? Utils.Remap(Timer2, 15, 60, 1f, 1.5f) : 1f;
                        NPC.MaxFallSpeedMultiplier *= Ultra && NPC.velocity.Y > 0f ? Utils.Remap(Timer2, 15, 60, 1f, 1.75f) : 1f;
                        StopHorizontalMovement();
                        if (NPC.velocity.Y == 0f && TeleportScaleMultiplier > 0.6f)
                            Timer1 += 1;

                        if (Timer1 > JumpDelay)
                        {
                            NPC.damage = NPC.defDamage;
                            NPC.netUpdate = true;
                            NPC.velocity = GetInitialVelocity();
                            CurrentAttackPhase = 1;

                            if (highJump) //凝胶弹幕
                            {
                                SoundEngine.PlaySound(SoundID.Item155, NPC.Center);
                                SpawnOrbParticles(20, 10f);

                                if (TOSharedData.NotClient)
                                {
                                    int outerAmount = Ultra ? Main.rand.Next(6, 10) : 0;
                                    int innerAmount = Ultra ? 7 : 5;

                                    for (int i = 0; i < outerAmount; i++)
                                    {
                                        float rotation = Utils.Remap(i, 0, outerAmount - 1, -TOMathUtils.PiOver24, TOMathUtils.PiOver24 - MathHelper.Pi) + Main.rand.NextFloat(-0.05f, 0.05f);
                                        Projectile.NewProjectileAction(SourceAI, NPC.Center, new PolarVector2(Main.rand.NextFloat(10f, 12f), rotation), GelProjectileType, GelDamage, 0f);
                                    }
                                    for (int i = 0; i < innerAmount; i++)
                                    {
                                        float rotation = Utils.Remap(i, 0, innerAmount - 1, -TOMathUtils.PiOver12, TOMathUtils.PiOver12 - MathHelper.Pi) + Main.rand.NextFloat(-0.05f, 0.05f);
                                        Projectile.NewProjectileAction(SourceAI, NPC.Center, new PolarVector2(Main.rand.NextFloat(6f, 9.5f), rotation), GelProjectileType, GelDamage, 0f);
                                    }
                                }

                                if (JewelAmethystAlive)
                                {
                                    AmethystJewel amethyst = JewelAmethyst.GetModNPC<AmethystJewel>();
                                }
                            }
                        }

                        break;

                    case 1: //上升
                    case 2: //下降
                        NPC.damage = NPC.defDamage;
                        bool farAway = Math.Abs(NPC.Center.X - Target.Center.X) > 1600f;
                        if (NPC.velocity.X * NPC.direction <= 0.1f && farAway) //跳跃过度时调整水平速度
                        {
                            NPC.velocity.X *= highJump ? 0.945f : 0.965f;
                            switch (Math.Abs(NPC.velocity.X))
                            {
                                case < 0.1f:
                                    DirectionChangeCounter++;
                                    NPC.velocity.X += GetDeltaVelocityX() * NPC.direction;
                                    break;
                                case > 0.25f:
                                    NPC.velocity.X -= (highJump ? 0.0125f : 0.0075f) * Math.Sign(NPC.velocity.X);
                                    break;
                            }
                        }
                        else
                            NPC.velocity.X = Math.Min(Math.Abs(NPC.velocity.X) + GetDeltaVelocityX(), GetMaxVelocityX()) * Math.Sign(NPC.velocity.X);
                        switch (CurrentAttackPhase)
                        {
                            case 1:
                                NPC.noTileCollide = true; //上升时无视物块
                                if (NPC.velocity.Y >= 0) //检测是否已过最高点
                                    CurrentAttackPhase = 2;
                                break;
                            case 2:
                                if (NPC.velocity.Y == 0f)
                                {
                                    if (highJump)
                                        TeleportTimer += 500f;
                                    CheckPhaseChange();
                                    SelectNextBehavior();
                                }
                                else
                                {
                                    NPC.GravityMultiplier *= CurrentBehavior switch
                                    {
                                        Behavior.Phase1_HighJump => Ultra ? (Phase2 ? Utils.Remap(Timer2, 10, 55, 1.5f, 2f) : Utils.Remap(Timer2, 15, 60, 1.35f, 1.75f)) : 1.25f,
                                        Behavior.Phase1_NormalJump => Ultra && Phase2 ? Utils.Remap(Timer2, 15, 55, 1.15f, 1.5f) : 1f,
                                        _ => 1f
                                    };
                                    NPC.MaxFallSpeedMultiplier *= CurrentBehavior switch
                                    {
                                        Behavior.Phase1_HighJump => Ultra ? (Phase2 ? Utils.Remap(Timer2, 10, 55, 2f, 2.75f) : Utils.Remap(Timer2, 15, 60, 1.85f, 2.5f)) : 1.75f,
                                        Behavior.Phase1_NormalJump => Ultra && Phase2 ? Utils.Remap(Timer2, 15, 55, 1.15f, 1.5f) : 1f,
                                        _ => 1f
                                    };
                                }
                                break;
                        }
                        break;
                }

                Vector2 GetInitialVelocity() => new Vector2(
                    CurrentBehavior switch
                    {
                        Behavior.Phase1_FirstJump => MathHelper.Lerp(7f, 9f, LostLifeRatioForPhase1),
                        Behavior.Phase1_NormalJump => MathHelper.Lerp(7f, 10f, LostLifeRatioForPhase1),
                        Behavior.Phase1_HighJump => MathHelper.Lerp(8f, 10.5f, LostLifeRatioForPhase1),
                        _ => 0f
                    } * NPC.direction,
                    CurrentBehavior switch
                    {
                        Behavior.Phase1_FirstJump => 10f * (1f + Math.Clamp(Math.Max(NPC.Center.Y - Target.Center.Y, 0f) / 1000f, 0f, 0.65f)),
                        Behavior.Phase1_NormalJump => 8f * (1f + Math.Clamp(Math.Max(NPC.Center.Y - Target.Center.Y, 0f) / 1000f, 0f, 0.65f)),
                        Behavior.Phase1_HighJump => MathHelper.Lerp(11.5f, 13f, LostLifeRatioForPhase1) * (1f + Math.Clamp(Math.Max(NPC.Center.Y - Target.Center.Y, 0f) / 1000f, 0f, 1f)),
                        _ => 0f
                    } * -1f) * (Ultra ? 1.15f : 1f);

                float GetMaxVelocityX() => Math.Abs(GetInitialVelocity().X) * DirectionChangeCounter switch
                {
                    0 => 1f,
                    1 => 0.4f,
                    _ => 0.2f
                };

                float GetDeltaVelocityX() => DirectionChangeCounter switch
                {
                    0 => 0.5f,
                    1 => 0.2f,
                    _ => 0.1f
                };
            }

            void SlamDown() //将原本的下砸攻击和凝胶攻击合并，使攻击更紧凑
            {
                switch (CurrentAttackPhase)
                {
                    case 0: //停留一段时间后爆发起跳
                        StopHorizontalMovement();
                        Timer1++;

                        MakeSmokeDust();

                        int delay = (int)MathHelper.Lerp(30, 25, LostLifeRatioForPhase1) - (Ultra ? 5 : 0);
                        if (Timer1 >= delay)
                        {
                            Vector2 destination = Target.Center + new Vector2(0f, -384f);
                            NPC.velocity = NPC.GetVelocityTowards(destination, 28f);
                            Timer1 = 0;
                            CurrentAttackPhase = 1;

                            //将凝胶弹幕合并至此

                            SoundEngine.PlaySound(SoundID.Item155, NPC.Center);
                            SpawnOrbParticles(20, 10f);

                            if (TOSharedData.NotClient)
                            {
                                int outerAmount = Main.rand.Next(Ultra ? 8 : 5, Ultra ? 12 : 10);
                                int innerAmount = Ultra ? 7 : 5;

                                for (int i = 0; i < outerAmount; i++)
                                {
                                    float rotation = Utils.Remap(i, 0, outerAmount - 1, -TOMathUtils.PiOver24, TOMathUtils.PiOver24 - MathHelper.Pi) + Main.rand.NextFloat(-0.05f, 0.05f);
                                    Projectile.NewProjectileAction(SourceAI, NPC.Center, new PolarVector2(Main.rand.NextFloat(11f, 14f), rotation), GelProjectileType, GelDamage, 0f);
                                }
                                for (int i = 0; i < innerAmount; i++)
                                {
                                    float rotation = Utils.Remap(i, 0, innerAmount - 1, -TOMathUtils.PiOver12, TOMathUtils.PiOver12 - MathHelper.Pi) + Main.rand.NextFloat(-0.05f, 0.05f);
                                    Projectile.NewProjectileAction(SourceAI, NPC.Center, new PolarVector2(Main.rand.NextFloat(7f, 10.5f), rotation), GelProjectileType, GelDamage, 0f);
                                }
                            }
                        }
                        break;

                    case 1: //移动30帧
                        Timer1++;
                        NPC.noGravity = true;
                        NPC.noTileCollide = true;
                        NPC.velocity.Y *= 0.95f;
                        if (Timer2 > 0)
                        {
                            Timer2++;
                            int delay2 = Ultra ? 2 : 5;
                            if (Timer2 > delay2)
                            {
                                NPC.noGravity = false;
                                CurrentAttackPhase = 2;
                            }
                        }
                        else if (Timer1 == 30)
                        {
                            NPC.velocity = Vector2.Zero;
                            Timer1 = 0;
                            Timer2++;
                        }

                        NPC.SpawnAfterimage(7, Color.White, useDefaultDraw: false);
                        break;

                    case 2: //下砸
                        StopHorizontalMovement();
                        NPC.noGravity = false;
                        NPC.noTileCollide = false;
                        NPC.GravityMultiplier *= Ultra ? 25f : 15f;
                        NPC.MaxFallSpeedMultiplier *= 2f;

                        if (NPC.velocity.Y == 0f)
                        {
                            SoundEngine.PlaySound(SoundID.Item167, NPC.Center);
                            SpawnOrbParticles(20, 10f);

                            for (int i = 0; i < 20; i++)
                            {
                                Vector2 position = NPC.Bottom - new Vector2(Main.rand.NextFloatDirection() * 16f, Main.rand.Next(8));
                                Dust.NewDustPerfectAction(position, DustID.Smoke, d =>
                                {
                                    d.alpha = 40;
                                    d.color = NPC.AI_121_QueenSlime_GetDustColor();
                                    d.noGravity = true;
                                    d.velocity = new Vector2(NPC.velocity.X * 7f, Main.rand.NextFloat(-8f, -5f));
                                });
                            }

                            CreateSmash(21, 12f);
                            CheckPhaseChange();
                            SelectNextBehavior();
                        }
                        else
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                Vector2 position = NPC.Bottom - new Vector2(Main.rand.NextFloatDirection() * 16f, Main.rand.Next(8));
                                Dust.NewDustPerfectAction(position, DustID.Smoke, d =>
                                {
                                    d.alpha = 40;
                                    d.color = NPC.AI_121_QueenSlime_GetDustColor();
                                    d.scale = 1.4f;
                                    d.noGravity = true;
                                    d.velocity = new Vector2((Main.rand.NextBool() ? (-10f) : 10f) + Main.rand.NextFloatDirection() * 3f, NPC.velocity.Y * 0.9f);
                                });
                            }
                        }

                        NPC.SpawnAfterimage(7, Color.White, useDefaultDraw: false);
                        break;
                }
            }

            void Teleport()
            {
                NPC.damage = 0;
                switch (CurrentAttackPhase)
                {
                    case 0: //寻的
                        Vector2? destination = null;

                        if (!Phase1_2)
                        {
                            Vector2 vectorAimedAheadOfTarget = Target.Center + new Vector2(MathF.Round(Target.velocity.X / 2f), 0f).ToCustomLength(800f);
                            //目标点
                            Point predictiveTeleportPoint = vectorAimedAheadOfTarget.ToTileCoordinates();
                            predictiveTeleportPoint.X = Math.Clamp(predictiveTeleportPoint.X, 10, Main.maxTilesX - 10);
                            predictiveTeleportPoint.Y = Math.Clamp(predictiveTeleportPoint.Y, 10, Main.maxTilesY - 10);
                            int randomPredictiveTeleportOffset = 5;

                            for (int i = 0; i < 100; i++)
                            {
                                int teleportTileX = Main.rand.Next(predictiveTeleportPoint.X - randomPredictiveTeleportOffset, predictiveTeleportPoint.X + randomPredictiveTeleportOffset + 1);
                                int teleportTileY = Main.rand.Next(predictiveTeleportPoint.Y - randomPredictiveTeleportOffset, predictiveTeleportPoint.Y);
                                Tile potentialTile = Main.tile[teleportTileX, teleportTileY];
                                if (!potentialTile.HasUnactuatedTile)
                                {
                                    if (potentialTile.LiquidType != LiquidID.Lava && Collision.CanHitLine(NPC.Center, 0, 0, predictiveTeleportPoint.ToVector2() * 16, 0, 0))
                                    {
                                        destination = new Vector2((teleportTileX + 0.5f) * 16f, (teleportTileY + 1f) * 16f);
                                        break; //在此处退出循环
                                    }
                                    else
                                    {
                                        predictiveTeleportPoint.X += predictiveTeleportPoint.X < 0f ? 1 : -1;
                                        predictiveTeleportPoint.X = Math.Clamp(predictiveTeleportPoint.X, 10, Main.maxTilesX - 10);
                                    }
                                }
                                else
                                {
                                    predictiveTeleportPoint.X += predictiveTeleportPoint.X < 0f ? 1 : -1;
                                    predictiveTeleportPoint.X = Math.Clamp(predictiveTeleportPoint.X, 10, Main.maxTilesX - 10);
                                }
                            }
                        }

                        TeleportDestination = destination ?? Target.Bottom;
                        CurrentAttackPhase = 1;
                        break;

                    case 1: //停止水平移动并缩小体型，满足条件时传送
                        MakeSlimeDust((int)Utils.Remap(NPC.scale, 0f, 1f, 5f, 12.5f), 2f);
                        float teleportSpeed = 0.016f;
                        TeleportScaleMultiplier -= teleportSpeed;
                        if (StopHorizontalMovement() && TeleportScaleMultiplier <= 0.05f)
                        {
                            Gore.NewGoreAction(SourceAI, NPC.Top + new Vector2(-40f, -30f), NPC.velocity, GoreID.QueenSlimeCrown);

                            if (Phase1_2)
                                NPC.Center = TeleportDestination = Target.Top + new Vector2(0f, -500f); //重新设置目的地，避免史莱姆皇后锁定很久以前的位置；传送至玩家头顶；采用Center而不是Bottom
                            else
                                NPC.Bottom = TeleportDestination;

                            CurrentAttackPhase = 2;
                        }
                        break;

                    case 2: //传送后恢复阶段
                        float teleportSpeed2 = Phase2 ? MathHelper.Lerp(0.06f, 0.08f, LostLifeRatioForPhase3)
                            : MathHelper.Lerp(0.03f, 0.05f, LostLifeRatioForPhase1);
                        TeleportScaleMultiplier += teleportSpeed2;

                        if (Phase1_2) //加速下落
                        {
                            if (Timer2 > 0 || TeleportScaleMultiplier >= 1f)
                            {
                                TeleportScaleMultiplier = 1f;
                                Timer2++;

                                if (Timer2 >= 20)
                                {
                                    NPC.damage = NPC.defDamage;

                                    float bonusGravity = Utils.Remap(Timer2, 0f, 150f, 3f, 5f);
                                    NPC.GravityMultiplier *= bonusGravity;
                                    NPC.MaxFallSpeedMultiplier *= bonusGravity * 1.25f;

                                    if (NPC.velocity.Y == 0f || Math.Abs(NPC.Center.Y - TeleportDestination.Y) >= 2400f) //下落距离过长时同样结束下落
                                    {
                                        SoundEngine.PlaySound(SoundID.Item167, NPC.Center);
                                        SpawnOrbParticles(20, 10f);

                                        if (TOSharedData.NotClient)
                                        {
                                            float speed = 12f;
                                            int type = ProjectileID.QueenSlimeMinionBlueSpike;
                                            float rotation = MathHelper.ToRadians(200f);
                                            int numProj = 20;
                                            Vector2 originalVelocity = new(0f, -speed);
                                            Projectile.NewProjectilesArc(numProj, rotation / (numProj - 1), SourceAI, NPC.Center, originalVelocity, type, SpikeDamage, 0f, action: p =>
                                            {
                                                SpawnSpikeAction_NoGravity(p);
                                                SpawnSpikeAction_Scaled(p);
                                            });

                                            if (Ultra)
                                            {
                                                numProj = 12;
                                                originalVelocity *= 0.65f;
                                                Projectile.NewProjectilesArc(numProj, rotation / (numProj - 1), SourceAI, NPC.Center, originalVelocity, type, SpikeDamage, 0f, action: p =>
                                                {
                                                    SpawnSpikeAction_NoGravity(p);
                                                    SpawnSpikeAction_Scaled(p);
                                                });
                                            }
                                        }

                                        CreateSmash(5, 0f);

                                        if (JewelAmethystAlive)
                                        {

                                        }

                                        SelectNextBehavior();
                                        NPC.Timer5 += 40; //跳过攻击等待时间
                                    }

                                    NPC.SpawnAfterimage(7, Color.White, useDefaultDraw: false);
                                }
                                else //保持原位
                                {
                                    NPC.noGravity = true;
                                    NPC.Center = TeleportDestination;

                                    bool shouldSpawnSpike = Timer2 switch
                                    {
                                        1 => true,
                                        11 or 16 => Ultra,
                                        13 => !Ultra,
                                        _ => false
                                    };
                                    if (shouldSpawnSpike) //尖刺
                                    {
                                        SoundEngine.PlaySound(SoundID.Item154, NPC.Center);

                                        if (TOSharedData.NotClient)
                                        {
                                            for (int j = -1; j <= 1; j += 2)
                                            {
                                                Vector2 direction = new PolarVector2(MathHelper.ToRadians(Main.rand.NextFloat(20) * j));
                                                const int max = 12;
                                                for (int i = 0; i < max; i++)
                                                {
                                                    float speed = Main.rand.NextFloat(6f, 20f);
                                                    Vector2 velocity = speed * j * direction.RotatedBy(MathHelper.PiOver4 * 0.8f / max * i * -j);
                                                    Projectile.NewProjectileAction(SourceAI, NPC.Center, velocity, ProjectileID.QueenSlimeMinionBlueSpike, SpikeDamage, 0f, action: SpawnSpikeAction_Scaled);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else //保持原位
                            {
                                NPC.noGravity = true;
                                NPC.Center = TeleportDestination;

                                Vector2 dustVelocity = Main.rand.NextPolarVector2(15f, 20f);
                                Dust.NewDustPerfectAction<SquashDust>(NPC.Center - dustVelocity.ToCustomLength(Main.rand.NextFloat(250f, 400f)), d =>
                                {
                                    d.velocity = dustVelocity;
                                    d.scale = Main.rand.NextFloat(1.3f, 1.75f);
                                    d.noGravity = true;
                                    d.fadeIn = 0.25f;
                                    d.color = NPC.AI_121_QueenSlime_GetDustColor();
                                });
                            }
                        }
                        else if (TeleportScaleMultiplier >= 1f)
                        {
                            TeleportScaleMultiplier = 1f;
                            SelectNextBehavior();
                        }
                        break;
                }
            }

            void CheckPhaseChange()
            {
                if (NPC.LifeRatio <= Phase2LifeRatio)
                {
                    CurrentPhase = Phase.Phase2;
                    CurrentAttackPhase = 0;
                    Timer1 = 0;
                    Timer2 = 0;
                    Timer5 = 0;
                }
                else if (NPC.LifeRatio <= Phase1_2LifeRatio)
                    CurrentPhase = Phase.Phase1_2;
            }
        }

        void Phase2AI()
        {
            NPC.noTileCollide = true;
            NPC.noGravity = true;

            FlyMovement();

            Timer5++;

            if (IsAttacking)
            {
                switch (CurrentBehavior)
                {
                    case Behavior.Phase2_Gel:
                        Gel();
                        break;
                    case Behavior.Phase2_Gel2:
                        Gel2();
                        break;
                    default:
                        CheckPhaseChange();
                        SelectNextBehavior();
                        break;
                }
            }
            else
            {
                CheckPhaseChange();
            }

            void SelectNextBehavior()
            {
                CurrentAttackPhase = 0;
                Timer1 = 0;
                Timer2 = 0;
                Timer5 = 0;

                switch (CurrentBehavior)
                {
                    case Behavior.Phase2_Gel:
                        CurrentAttackCounter++;
                        if (CurrentAttackCounter >= 3)
                        {
                            CurrentAttackCounter = 0;
                            CurrentBehavior = Behavior.Phase2_Gel2;
                        }
                        break;
                    case Behavior.Phase2_Gel2:
                        CurrentAttackCounter++;
                        if (CurrentAttackCounter >= 2)
                        {
                            CurrentAttackCounter = 0;
                            CurrentBehavior = Behavior.Phase2_Gel;
                        }
                        break;
                    default:
                        CurrentAttackCounter = 0;
                        CurrentBehavior = Behavior.Phase2_Gel;
                        break;
                }
            }

            void Gel()
            {
                Timer1++;
                MakeSmokeDust();
                CheckPhaseChange();
                if (Timer1 >= 60)
                {
                    SoundEngine.PlaySound(SoundID.Item155, NPC.Center);
                    SpawnOrbParticles(25, 12.5f);

                    if (TOSharedData.NotClient)
                    {
                        int amount = Ultra ? 15 : 11;
                        float radian = MathHelper.ToRadians(Ultra ? 210f : 160f);
                        float singleRadian = radian / (amount - 1);
                        float speed = 16f;
                        Vector2 velocity = NPC.GetVelocityTowards(Target, speed).RotatedByRandom(TOMathUtils.PiOver3 * 2);

                        Projectile.NewProjectilesArc(amount, singleRadian, SourceAI, NPC.Center, velocity, GelProjectileType, GelDamage, 0f, action: p =>
                        {
                            p.ai[1] = 1f;
                            p.timeLeft = 600;
                            p.velocity *= Main.rand.NextFloat(0.8f, 1.2f);
                            p.velocity.Rotation += Main.rand.NextFloat(-0.2f, 0.2f);
                        });
                    }

                    SelectNextBehavior();
                }
            }

            void Gel2()
            {
                Timer1++;
                MakeSmokeDust();
                CheckPhaseChange();
                if (Timer1 >= 90)
                {
                    SoundEngine.PlaySound(SoundID.Item155, NPC.Center);
                    SpawnOrbParticles(30, 12.5f);

                    if (TOSharedData.NotClient)
                    {
                        int amount = Ultra ? 15 : 11;
                        float radian = MathHelper.ToRadians(Ultra ? 300f : 200f);
                        float singleRadian = radian / (amount - 1);
                        float speed = Ultra ? 12f : 10f;
                        Projectile.NewProjectilesArc(amount, singleRadian, SourceAI, NPC.Center, new Vector2(0f, -speed), GelProjectileType, GelDamage, 0f);

                        for (int j = -1; j <= 1; j += 2)
                        {
                            Vector2 direction = new PolarVector2(MathHelper.ToRadians(Main.rand.NextFloat(10) * j));
                            int spikeAmount = 12;
                            for (int i = 0; i < spikeAmount; i++)
                            {
                                Vector2 velocity = Main.rand.NextFloat(10f, 30f) * j * direction.RotatedBy(MathHelper.PiOver4 * 0.8f / spikeAmount * i * -j);
                                Projectile.NewProjectileAction(SourceAI, NPC.Center, velocity, ProjectileID.QueenSlimeMinionBlueSpike, SpikeDamage, 0f, action: SpawnSpikeAction_Scaled);
                            }
                        }
                    }

                    SelectNextBehavior();
                }
            }

            void CheckPhaseChange()
            {
            }
        }
        #endregion 行为函数
    }
}
