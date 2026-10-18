namespace Anomalies.Bosses.BrainofCthulhu;

public sealed partial class BrainofCthulhu
{
    private void Phase2AI()
    {
        switch (CurrentBehavior)
        {
            case Behavior.Phase2Idle:
                Phase2Idle();
                break;
            case Behavior.Bloodletting:
                Bloodletting();
                break;
            case Behavior.SanguineScythes:
                SanguineScythes();
                break;
            case Behavior.CrimsonEyes:
                CrimsonEyes();
                break;
            case Behavior.IllusionDash:
                IllusionDash();
                break;
            case Behavior.IllusionTrick:
                IllusionTrick();
                break;
        }

        void Phase2Idle()
        {
            NPC.rotation = NPC.velocity.X / 6f * MathHelper.Pi / 8f;
            if (Time == ChaseTime - 5)
                CurrentLocalPhase++;
            if (Time <= ChaseTime)
            {
                NPC.damage = NPC.defDamage;
                float speedUp = MathHelper.Clamp((Time - 10) / 10f, 0f, 1f);
                float slowDown = 1 - MathHelper.Clamp((Time - (ChaseTime - 15)) / 15f, 0f, 1f);
                float angleChange = MathHelper.Lerp(MathHelper.Pi / 24f, 0f, MathHelper.Clamp(Time / (ChaseTime * 0.666f), 0f, 1f));
                NPC.velocity = NPC.velocity.RotateDirectionTowards(NPC.DirectionTo(Target.Center).ToRotation(), angleChange) * (MathHelper.Lerp(ChaseMinSpeed, ChaseMaxSpeed, Time / ChaseTime) * speedUp * slowDown);
                // Teleport ahead of the player's movement
                if (Time == ChaseTime)
                {
                    if (Main.netMode != NetmodeID.SinglePlayer)
                        SelectNewTarget();
                    Vector2 direction = Target.velocity.RotatedByRandom(MathHelper.Pi * 0.1f);
                    if (direction.Length() < 3.5f)
                        direction = direction.SafeNormalize(Vector2.UnitX * Target.direction) * 3.5f;
                    AttackPosition = Target.Center + (direction * IdleTeleportDuration * 1.8f);
                    BoCAfterImages = [];
                    NPC.damage = 0;
                    NPC.netUpdate = true;
                }
                else
                {
                    TeleportTime = 0;
                    NPC.Opacity = 1f;
                }
            }
            else
            {
                TeleportDuration = IdleTeleportDuration;
                NPC.damage = 0;
                Vector2 endPoint = AttackPosition;
                NPC.velocity = Vector2.Zero;
                if (Time < ChaseTime + (TeleportDuration / 2f))
                {
                    if (Time % 4 == 0)
                    {
                        Vector2 startPoint = NPC.Center;
                        Vector2 direction = endPoint - startPoint;
                        float curveIntensity = Main.rand.NextFloat(-0.2f, 0.2f);
                        Vector2 perpindicular = direction.RotatedBy(MathHelper.PiOver2);
                        Vector2 controlPoint1 = startPoint + (direction * 0.25f) + (perpindicular * curveIntensity);
                        Vector2 controlPoint2 = startPoint + (direction * 0.75f) + (perpindicular * curveIntensity);
                        BezierCurve path = new(startPoint, controlPoint1, controlPoint2, endPoint);
                        BrainOfCthulhuAfterImage afterimage = new(path, NPC.rotation, Vector2.One, (int)(ChaseTime + (TeleportDuration * 0.75f) - Time), BoCFrame);
                        BoCAfterImages.Add(afterimage);
                        ParticleHandler.SpawnParticle(afterimage);
                    }

                    TeleportTime++;
                }
                else if (Time == ChaseTime + (TeleportDuration / 2f) && !AttackFlag)
                {
                    NPC.Center = endPoint;
                    NPC.damage = NPC.defDamage;
                    NPC.netUpdate = true;
                    AttackFlag = true;
                    NPC.netUpdate = true;
                }
                else
                {
                    TeleportTime--;
                    if (TeleportTime < 0)
                    {
                        TeleportTime = 0;
                        Time = -1;
                        AttackFlag = false;
                        if (CurrentLocalPhase >= ChaseAmount) //Pick attack
                        {
                            NPC.rotation = 0;
                            ResetAttackValues();
                            if (!ForcedIllusionAttack && (NPC.life / (float)NPC.lifeMax) <= ForcedIllusionHealthGate)
                            {
                                ForcedIllusionAttack = true;
                                CurrentBehavior = Behavior.IllusionTrick;
                                PreviousAttack = CurrentBehavior;
                            }
                            else
                            {
                                if (AvailableAttacks.Count == 0)
                                {
                                    bool quickChoice = Main.rand.NextBool();
                                    AvailableAttacks.Replace([Behavior.Bloodletting, quickChoice ? Behavior.SanguineScythes : Behavior.IllusionDash, Main.rand.NextBool() ? Behavior.Phase2Idle : Behavior.Bloodletting, quickChoice ? Behavior.IllusionDash : Behavior.SanguineScythes, Behavior.IllusionTrick]);
                                }

                                CurrentBehavior = AvailableAttacks[0];
                                AvailableAttacks.RemoveAt(0);
                                PreviousAttack = CurrentBehavior;
                                if (CurrentBehavior == Behavior.SanguineScythes)
                                {
                                    Time = -31;
                                    BoCAfterImages = [];
                                }
                            }

                            NPC.netUpdate = true;
                        }
                    }
                }

                NPC.Opacity = 1 - (TeleportTime / (TeleportDuration / 2f));
            }
        }

        void Bloodletting()
        {
            float endTime = Time - BloodlettingDuration;
            if (endTime < 0)
            {
                if (Time == 0)
                {
                    AttackPosition = NPC.Center;
                    if (NPC.Center.X < Target.Center.X)
                        AttackSign = -1;
                    else
                        AttackSign = 1;
                    CachedRatio = float.MaxValue;
                    AttackTime = float.MaxValue;
                    NPC.netUpdate = true;
                }

                if (Time == 30)
                    AttackPosition = Vector2.Zero;
                //Make sure we have an accurate view on who the furthest players
                if (Main.netMode != NetmodeID.SinglePlayer && Time % 2 == 0)
                {
                    Player furthestLeft = null;
                    Player furthestRight = null;
                    Player highestUp = null;
                    foreach (int who in GetAllValidTargets(NPC.Center))
                    {
                        Player p = Main.player[who];
                        if (furthestLeft == null || p.Center.X < furthestLeft.Center.X)
                            furthestLeft = p;
                        if (furthestRight == null || p.Center.X > furthestRight.Center.X)
                            furthestRight = p;
                        if (highestUp == null || p.Center.Y < highestUp.Center.Y)
                            highestUp = p;
                    }

                    AttackList.Clear();
                    AttackList.Add((byte)furthestLeft.whoAmI);
                    AttackList.Add((byte)furthestRight.whoAmI);
                    AttackList.Add((byte)highestUp.whoAmI);
                }

                float waveValue = Time * MathHelper.Pi / BloodshotRate;
                Vector2 goalPos;
                if (Main.netMode == NetmodeID.SinglePlayer)
                    goalPos = Target.Center + new Vector2((float)Math.Cos(waveValue) * HoverDistance.X * AttackSign, (float)(-0.5f * Math.Cos(2 * waveValue) + 0.5f) * -HoverDistance.Y);
                else
                {
                    Player furthestLeft = Main.player[AttackList[0]];
                    Player furthestRight = Main.player[AttackList[1]];
                    Player highestUp = Main.player[AttackList[2]];
                    Vector2 hoverCenter = (furthestLeft.Center + furthestRight.Center) / 2f;
                    float xDist = furthestRight.Center.X - furthestLeft.Center.X;
                    float yDist = hoverCenter.Y - highestUp.Center.Y;
                    if (AttackPosition == Vector2.Zero)
                        AttackPosition = hoverCenter;
                    else
                        AttackPosition += (hoverCenter - AttackPosition) / 30f;
                    if (CachedRatio == float.MaxValue || Math.Abs(xDist - CachedRatio) < 0.01f)
                        CachedRatio = xDist;
                    else
                        CachedRatio += (xDist - CachedRatio) / 30f;
                    if (AttackTime == float.MaxValue || Math.Abs(yDist - AttackTime) < 0.01f)
                        AttackTime = yDist;
                    AttackTime += (yDist - AttackTime) / 30f;
                    float xMag = HoverDistance.X + CachedRatio;
                    float yMag = HoverDistance.Y + AttackTime;
                    Vector2 destination = AttackPosition + new Vector2((float)Math.Cos(waveValue) * xMag * AttackSign, (float)(-0.5f * Math.Cos(2 * waveValue) + 0.5f) * -yMag);
                    goalPos = destination;
                }

                NPC.velocity = Vector2.Zero;
                NPC.netUpdate = true;
                if (Time < 30f)
                    NPC.Center = Vector2.Lerp(AttackPosition, goalPos, BrainCombatUtils.SineOutEasing(Time / 30f, 1));
                else
                    NPC.Center = goalPos;
            }
            else if (endTime < DashPrepTime)
            {
                if (endTime == 0)
                    NPC.velocity = Vector2.UnitX * AttackSign * 10f;
                else
                {
                    Vector2 goalPos = Target.Center - Vector2.UnitY * HoverEndHeight;
                    Vector2 accel = new(0.5f, 1.5f);
                    NPC.velocity += NPC.DirectionTo(goalPos).SafeNormalize(Vector2.Zero) * accel;
                    NPC.velocity = NPC.velocity.ClampMagnitude(0f, 8f);
                }
            }

            if (endTime < 0)
            {
                NPC.rotation = (float)Math.Sin(Time / 8f) * MathHelper.Pi / 8f;
                BoCDrawOffset = Vector2.Zero;
                NPC.damage = 0;
                if (Time > BloodshotRate) //Doesnt fire first bloodshot
                {
                    if (Time % IchorRate == 0)
                    {
                        if (Time > BloodshotRate + 30 && Time % (IchorRate * 10) == 0)
                        {
                            SoundEngine.PlaySound(BloodBombSound, NPC.Center);
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                NPC.NewNPCAction(source: NPC.GetSource_FromThis(), position: new Vector2((int)NPC.Center.X, (int)NPC.Center.Y), type: ModContent.NPCType<BloodBomb>(), start: NPC.whoAmI);
                        }
                        else
                        {
                            if (Time % (IchorRate * 2) == 0)
                                SoundEngine.PlaySound(SoundID.Item17, NPC.Center);
                            if (Main.netMode == NetmodeID.SinglePlayer)
                                Projectile.NewProjectileAction(source: NPC.GetSource_FromThis(), position: NPC.Center + NPC.velocity + Main.rand.NextVector2Circular(72, 72), velocity: new Vector2(Main.rand.NextFloat(-IchorSpread, IchorSpread), -IchorVelocity), type: ModContent.ProjectileType<IchorShower>(), damage: IchorShotDamage, knockback: 0.5f);
                            else if (Main.dedServ)
                            {
                                Player furthestLeft = Main.player[AttackList[0]];
                                Player furthestRight = Main.player[AttackList[1]];
                                float xDist = furthestRight.Center.X - furthestLeft.Center.X;
                                int projCount = 1 + (int)(xDist / 360);
                                for (int i = 0; i < projCount; i++)
                                    Projectile.NewProjectileAction(source: NPC.GetSource_FromThis(), position: NPC.Center + NPC.velocity + Main.rand.NextVector2Circular(72, 72), velocity: new Vector2(Main.rand.NextFloat(-IchorSpread, IchorSpread), -IchorVelocity), type: ModContent.ProjectileType<IchorShower>(), damage: IchorShotDamage, knockback: 0.5f);
                            }
                        }
                    }

                    if (Time % BloodshotRate == 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Vector2 dir = NPC.DirectionTo(Target.Center);
                            for (int i = -2; i <= 2; i++)
                            {
                                Vector2 initialDir = dir.RotatedBy(i * MathHelper.Pi / 4f);
                                Projectile.NewProjectileAction(source: NPC.GetSource_FromThis(), position: NPC.Center, velocity: initialDir * BloodshotVelocity, type: ModContent.ProjectileType<BrainBloodShot>(), damage: BloodShotDamage, knockback: 0.5f, action: entity =>
                                {
                                    entity.ai[0] = dir.ToRotation() + MathHelper.TwoPi;
                                    entity.ai[1] = initialDir.ToRotation();
                                });
                            }

                            if (AnomalySharedData.Anomaly)
                            {
                                Vector2 initialDir = dir.RotatedBy(MathHelper.Pi / 6f);
                                Projectile.NewProjectileAction(source: NPC.GetSource_FromThis(), position: NPC.Center, velocity: initialDir * BloodshotVelocity / 2.1f, type: ModContent.ProjectileType<BrainBloodShot>(), damage: BloodShotDamage, knockback: 0.5f, action: entity =>
                                {
                                    entity.ai[0] = dir.ToRotation() + MathHelper.TwoPi;
                                    entity.ai[1] = initialDir.ToRotation();
                                });
                                initialDir = dir.RotatedBy(-MathHelper.Pi / 6f);
                                Projectile.NewProjectileAction(source: NPC.GetSource_FromThis(), position: NPC.Center, velocity: initialDir * BloodshotVelocity / 2.1f, type: ModContent.ProjectileType<BrainBloodShot>(), damage: BloodShotDamage, knockback: 0.5f, action: entity =>
                                {
                                    entity.ai[0] = dir.ToRotation() + MathHelper.TwoPi;
                                    entity.ai[1] = initialDir.ToRotation();
                                });
                            }
                        }

                        SoundEngine.PlaySound(BloodShotSound with { PitchVariance = 0.5f }, NPC.Center);
                    }
                }
            }
            else
            {
                NPC.rotation *= 0.9f;
                NPC.damage = NPC.defDamage;
                if (endTime == 0)
                    SoundEngine.PlaySound(ShortRoarSound, NPC.Center);
                if (endTime >= DashPrepTime)
                {
                    if (endTime < DashPrepTime + DashReelbackTime)
                    {
                        float reelBackSpeedExponent = 2.6f;
                        float reelBackCompletion = Utils.GetLerpValue(0f, 30, endTime - DashPrepTime, true);
                        float reelBackSpeed = MathHelper.Lerp(4f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                        Vector2 reelBackVelocity = Vector2.UnitY * -reelBackSpeed;
                        NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);
                    }
                    else if (endTime == DashPrepTime + 20)
                        NPC.velocity = Vector2.UnitY * DashVelocity;
                    if (endTime >= DashPrepTime + DashReelbackTime && Time % DashScytheRate == 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectileAction(source: NPC.GetSource_FromThis(), position: NPC.Center, velocity: Vector2.UnitX * 16f, type: ModContent.ProjectileType<BloodScythe>(), damage: BloodScytheDamage, knockback: 0.5f);
                            Projectile.NewProjectileAction(source: NPC.GetSource_FromThis(), position: NPC.Center, velocity: Vector2.UnitX * -16f, type: ModContent.ProjectileType<BloodScythe>(), damage: BloodScytheDamage, knockback: 0.5f);
                        }
                    }

                    if (endTime > DashPrepTime + DashReelbackTime + DashDuration)
                    {
                        NPC.rotation = 0;
                        SetupForNextAttack();
                        return;
                    }
                }
            }
        }

        void SanguineScythes()
        {
            NPC.damage = 0;
            if (CurrentLocalPhase > SanguineTeleportCount)
            {
                if (Time == SanguineAttackEndDelay)
                {
                    SoundEngine.PlaySound(ShortRoarSound, NPC.Center);
                    bool left = Target.Center.X > NPC.Center.X;
                    NPC.velocity = Vector2.UnitX * (left ? 18 : -18);
                    NPC.rotation = MathHelper.Pi / 8f * (left ? 1 : -1);
                }

                if (Time > SanguineAttackEndDelay + SanguineAttackEndDuration)
                {
                    NPC.velocity *= 0.8f;
                    NPC.rotation *= 0.8f;
                }
                else if (Time >= SanguineAttackEndDelay && Time % SanguineAttackEndIchorRate == 0)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectileAction(source: NPC.GetSource_FromThis(), position: NPC.Center, velocity: new Vector2(Math.Sign(NPC.velocity.X), -2f), type: ModContent.ProjectileType<IchorShower>(), damage: IchorShotDamage, knockback: 0.5f);
                        Projectile.NewProjectileAction(source: NPC.GetSource_FromThis(), position: NPC.Center, velocity: new Vector2(Math.Sign(NPC.velocity.X), -6f), type: ModContent.ProjectileType<IchorShower>(), damage: IchorShotDamage, knockback: 0.5f);
                    }
                }

                if (Time > SanguineAttackEndDelay + SanguineAttackEndDuration + (AnomalySharedData.Anomaly ? 20 : 40))
                    SetupForNextAttack();
            }
            else if (Time < 0)
            {
                if (Time == -25)
                    NPC.netUpdate = true;
                if (Time == -24)
                {
                    SoundEngine.PlaySound(ShortRoarSound, NPC.Center);
                    for (int i = 1; i <= 3; i++)
                    {
                        Color color = i switch
                        {
                            1 => Color.Yellow,
                            2 => Color.Orange,
                            _ => Color.Red,
                        };
                        PulseRing ring = new(NPC.Center, NPC.velocity * 0.5f, color, 0f, 1f + i * 0.5f, 24);
                        ParticleHandler.SpawnParticle(ring);
                    }
                }

                if (Time == -1)
                {
                    Vector2 direction = Target.velocity.SafeNormalize(Vector2.UnitX * Target.direction).RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 3f, MathHelper.Pi / 3f));
                    float distance = SanguineTeleportDistance;
                    AttackPosition = Target.Center + (direction * distance);
                    NPC.netUpdate = true;
                }
            }
            else
            {
                TeleportDuration = SanguineTeleportDuration;
                Vector2 endPoint = AttackPosition;
                if (Time < (TeleportDuration / 2f))
                {
                    if (Time % 4 == 0)
                    {
                        Vector2 startPoint = NPC.Center;
                        Vector2 direction = endPoint - startPoint;
                        float curveIntensity = Main.rand.NextFloat(-0.2f, 0.2f);
                        Vector2 perpindicular = direction.RotatedBy(MathHelper.PiOver2);
                        Vector2 controlPoint1 = startPoint + (direction * 0.25f) + (perpindicular * curveIntensity);
                        Vector2 controlPoint2 = startPoint + (direction * 0.75f) + (perpindicular * curveIntensity);
                        BezierCurve path = new(startPoint, controlPoint1, controlPoint2, endPoint);
                        BrainOfCthulhuAfterImage afterimage = new(path, NPC.rotation, Vector2.One, (int)((TeleportDuration * 0.75f) - Time), BoCFrame);
                        BoCAfterImages.Add(afterimage);
                        ParticleHandler.SpawnParticle(afterimage);
                    }

                    TeleportTime++;
                }
                else if (Time == (TeleportDuration / 2f) && !AttackFlag)
                {
                    Vector2 start = NPC.Center;
                    NPC.Center = endPoint;
                    NPC.netUpdate = true;
                    AttackFlag = true;
                    NPC.netUpdate = true;
                }
                else
                {
                    TeleportTime--;
                    if (TeleportTime < 0)
                    {
                        TeleportTime = 0;
                        Time = -1;
                        AttackFlag = false;
                        CurrentLocalPhase++;
                        if (CurrentLocalPhase <= SanguineTeleportCount)
                        {
                            SoundEngine.PlaySound(ExplosionSound, NPC.Center);
                            for (int i = 0; i < SanguineScytheCount; i++)
                            {
                                float initalSpeed = 16f;
                                if (AnomalySharedData.Anomaly && i % 2 == 0)
                                    initalSpeed /= 2f;
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    Projectile.NewProjectileAction(source: NPC.GetSource_FromThis(), position: NPC.Center, velocity: Vector2.UnitX.RotatedBy(MathHelper.TwoPi / SanguineScytheCount * i) * initalSpeed, type: ModContent.ProjectileType<BloodScythe>(), damage: BloodScytheDamage, knockback: 0.5f);
                            }
                        }

                        Vector2 direction;
                        if (CurrentLocalPhase < SanguineTeleportCount)
                        {
                            direction = Main.rand.NextFloat(0f, MathHelper.TwoPi).ToRotationVector2();
                            AttackPosition = Target.Center + (direction * SanguineTeleportDistance);
                        }
                        else
                        {
                            direction = Vector2.UnitX * (Main.rand.NextBool() ? -1 : 1);
                            AttackPosition = Target.Center + (direction * SanguineFinalTeleportOffset.X) + (Vector2.UnitY * -SanguineFinalTeleportOffset.Y);
                        }

                        NPC.netUpdate = true;
                        BoCAfterImages = [];
                        NPC.Opacity = 1f;
                    }
                }

                NPC.Opacity = 1 - (TeleportTime / (TeleportDuration / 2f));
            }
        }

        void CrimsonEyes()
        {
            if (Time <= 30)
            {
                if (Time is >= 6 and <= 12)
                {
                    if (Time == 6)
                    {
                        SoundEngine.PlaySound(ShortRoarSound, NPC.Center);
                        for (int i = 1; i <= 3; i++)
                        {
                            Color color = i switch
                            {
                                1 => Color.Yellow,
                                2 => Color.Orange,
                                _ => Color.Red,
                            };
                            PulseRing ring = new(NPC.Center, NPC.velocity * 0.5f, color, 0f, 1f + i * 0.5f, 24);
                            ParticleHandler.SpawnParticle(ring);
                        }

                        BrainCombatUtils.AddScreenshakeAt(NPC.Center, 10f);
                    }

                    for (int i = 0; i < 12; i++)
                    {
                        Point start = Target.Center.ToTileCoordinates() + new Point(Main.rand.Next(-64, 65), 48);
                        for (int j = 0; j < 96; j++)
                        {
                            Point current = start - new Point(0, j);
                            if (!Framing.GetTileSafely(current).IsTileSolid() && Framing.GetTileSafely(current - new Point(0, 1)).IsTileSolid())
                                Dust.NewDustAction(current.ToWorldCoordinates(8, 8), 16, 16, DustID.Crimstone, new Vector2(0, 3));
                        }
                    }
                }
            }
            else
            {
                if (Time < CrimsonEyeAttackDuration && BrainCombatUtils.CountProjectiles(ModContent.ProjectileType<CrimsonEye>()) < CrimsonEyeCap && Time % CrimsonEyeRate == 0)
                {
                    Vector2 spawnPos = Target.Center;
                    int i = 0;
                    Outer:
                        for (i = 0; i <= 32; i++)
                        {
                            spawnPos = Target.Center + Main.rand.NextVector2Circular(256, 256);
                            if (Collision.IsWorldPointSolid(spawnPos))
                                continue;
                            foreach (Projectile p in Main.ActiveProjectiles)
                            {
                                if (p.type != ModContent.ProjectileType<CrimsonEye>())
                                    continue;
                                Rectangle hitbox = new((int)spawnPos.X - 50, (int)spawnPos.Y - 18, 100, 36);
                                if (p.Hitbox.Intersects(hitbox))
                                    continue Outer;
                            }

                            if (Collision.CanHitLine(spawnPos, 1, 1, Target.position, Target.width, Target.height))
                                break;
                        }

                    if (i == 32)
                        spawnPos = Target.Center;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectileAction(source: NPC.GetSource_FromThis(), position: spawnPos, velocity: Vector2.Zero, type: ModContent.ProjectileType<CrimsonEye>(), damage: CrimsonEyeDamage, knockback: 0f);
                }

                if (Time < 180)
                {
                    float dist = 210;
                    Vector2 fromTarget = (NPC.Center - Target.Center).SafeNormalize(Vector2.UnitX);
                    Vector2 goalPos = Target.Center + (fromTarget * dist) - (Vector2.UnitY * 32);
                    if (NPC.DistanceSQ(goalPos) <= 2048)
                        NPC.velocity *= 0.9f;
                    else if (NPC.velocity.LengthSquared() < 16f) //7.5^2
                        NPC.velocity += NPC.DirectionTo(goalPos).SafeNormalize(Vector2.Zero) * 0.1f;
                    else
                        NPC.velocity = NPC.DirectionTo(goalPos).SafeNormalize(Vector2.Zero) * 4f;
                }
                else
                {
                    if (Time < CrimsonEyeAttackIdleDuration)
                        NPC.velocity *= 0.9f;
                    if (Time >= CrimsonEyeAttackIdleDuration + CrimsonEyeAttackSetUpDuration && Time < CrimsonEyeAttackDuration)
                    {
                        if (Time == CrimsonEyeAttackIdleDuration + CrimsonEyeAttackSetUpDuration)
                            NPC.velocity = NPC.DirectionTo(Target.Center);
                        float speed = 10f;
                        if (Time < CrimsonEyeAttackIdleDuration + CrimsonEyeAttackSetUpDuration + CrimsonEyeAttackBuildUpDuration)
                        {
                            float lerp = BrainCombatUtils.SineInEasing((Time - (CrimsonEyeAttackIdleDuration + CrimsonEyeAttackSetUpDuration)) / CrimsonEyeAttackBuildUpDuration, 1);
                            speed = MathHelper.Lerp(0f, speed, lerp);
                        }

                        float turnAmt = TurnAccelerationMultiplier * ((NPC.Distance(Target.Center) - TurnAccelerationDistanceBuffer) / TurnAccelerationDistanceDivisor);
                        NPC.velocity = NPC.velocity.RotateDirectionTowards(NPC.AngleTo(Target.Center), turnAmt) * speed;
                    }
                    else
                    {
                        NPC.velocity *= 0.9f;
                        if (Time == CrimsonEyeAttackDuration)
                        {
                            foreach (Projectile p in Main.ActiveProjectiles)
                            {
                                if (p.type != ModContent.ProjectileType<CrimsonEye>())
                                    continue;
                                p.timeLeft = 60;
                            }
                        }

                        if (Time > CrimsonEyeAttackDuration + CrimsonEyeAttackEndDuration)
                            SetupForNextAttack();
                    }

                    if (Time == CrimsonEyeAttackIdleDuration)
                    {
                        SoundEngine.PlaySound(ExplosionSound, NPC.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            float projCount = 10;
                            for (int i = 0; i < projCount; i++)
                                Projectile.NewProjectileAction(source: NPC.GetSource_FromThis(), position: NPC.Center, velocity: Vector2.Zero, type: ModContent.ProjectileType<CirclingBloodScythe>(), damage: BloodScytheDamage, knockback: 0.5f, owner: -1, action: entity =>
                                {
                                    entity.ai[0] = MathHelper.TwoPi / projCount * i;
                                    entity.rotation = entity.ai[0];
                                });
                        }
                    }
                }
            }
        }

        void IllusionDash()
        {
            NPC.damage = 0;
            if (Time < IllusionDashTeleportDuration)
            {
                if (Time == 0)
                {
                    AttackPosition = Target.Center;
                    AttackRotation = Main.rand.NextFloat(0, MathHelper.TwoPi);
                    NPC.netUpdate = true;
                }

                TeleportDuration = IllusionDashTeleportDuration;
                if (Time < (TeleportDuration / 2f))
                {
                    if (Time % 4 == 0)
                    {
                        Vector2 startPoint = NPC.Center;
                        for (int i = 0; i < (AnomalySharedData.Anomaly ? 8 : 4); i++)
                        {
                            Vector2 myEndPoint = AttackPosition + ((AttackRotation + (AnomalySharedData.Anomaly ? MathHelper.PiOver4 : MathHelper.PiOver2) * i).ToRotationVector2() * IllusionDashTeleportDistance);
                            Vector2 direction = myEndPoint - startPoint;
                            float curveIntensity = Main.rand.NextFloat(-0.2f, 0.2f);
                            Vector2 perpindicular = direction.RotatedBy(MathHelper.PiOver2);
                            Vector2 controlPoint1 = startPoint + (direction * 0.25f) + (perpindicular * curveIntensity);
                            Vector2 controlPoint2 = startPoint + (direction * 0.75f) + (perpindicular * curveIntensity);
                            BezierCurve path = new(startPoint, controlPoint1, controlPoint2, myEndPoint);
                            BrainOfCthulhuAfterImage afterimage = new(path, NPC.rotation, Vector2.One, (int)((TeleportDuration * 0.75f) - Time), BoCFrame);
                            BoCAfterImages.Add(afterimage);
                            ParticleHandler.SpawnParticle(afterimage);
                        }
                    }

                    TeleportTime++;
                }
                else if (Time == (TeleportDuration / 2f) && !AttackFlag)
                {
                    AttackFlag = true;
                    for (int i = 0; i < (AnomalySharedData.Anomaly ? 8 : 4); i++)
                    {
                        float rot = AttackRotation + (AnomalySharedData.Anomaly ? MathHelper.PiOver4 : MathHelper.PiOver2) * i;
                        Vector2 spawnPos = AttackPosition + (rot.ToRotationVector2() * IllusionDashTeleportDistance);
                        if (i == 0)
                        {
                            NPC.Center = spawnPos;
                            NPC.netUpdate = true;
                        }
                        else if (Main.netMode != NetmodeID.MultiplayerClient)
                            NPC.NewNPCAction(source: NPC.GetSource_FromThis(), position: spawnPos, type: ModContent.NPCType<BrainIllusion>(), start: 0, action: entity =>
                            {
                                // Action runs after OnSpawn; initialize the resulting teleport state directly.
                                BrainIllusion illusion = (BrainIllusion)entity.ModNPC;
                                illusion.Time = 15;
                                illusion.TeleportDuration = 30;
                                illusion.TeleportTime = 15;
                                illusion.Angle = rot;
                                entity.target = NPC.target;
                            });
                    }

                    if (Main.netMode != NetmodeID.SinglePlayer)
                    {
                        List<int> targets = GetAllValidTargets(NPC.Center);
                        targets.Remove(Target.whoAmI);
                        foreach (int who in targets)
                        {
                            Player p = Main.player[who];
                            float baseRot = Main.rand.NextFloat(0, MathHelper.TwoPi);
                            for (int i = 0; i < (AnomalySharedData.Anomaly ? 4 : 2); i++)
                            {
                                float rot = baseRot + (AnomalySharedData.Anomaly ? MathHelper.PiOver2 : MathHelper.Pi) * i;
                                Vector2 spawnPos = p.Center + (rot.ToRotationVector2() * IllusionDashTeleportDistance);
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    NPC.NewNPCAction(source: NPC.GetSource_FromThis(), position: spawnPos, type: ModContent.NPCType<BrainIllusion>(), start: 0, action: entity =>
                                    {
                                        BrainIllusion illusion = (BrainIllusion)entity.ModNPC;
                                        illusion.Time = 15;
                                        illusion.TeleportDuration = 30;
                                        illusion.TeleportTime = 15;
                                        illusion.Angle = rot;
                                        entity.target = p.whoAmI;
                                    });
                            }
                        }
                    }

                    NPC.netUpdate = true;
                }
                else
                    TeleportTime--;
                NPC.Opacity = 1 - (TeleportTime / (TeleportDuration / 2f));
            }
            else
            {
                float startTime = Time - IllusionDashTeleportDuration;
                if (startTime == 0)
                {
                    AttackPosition = NPC.Center;
                    TeleportTime = 0;
                    AttackFlag = false;
                    BoCAfterImages = [];
                    NPC.Opacity = 1f;
                    GenericSparkle sparkle = new(NPC.Center + new Vector2(16, -8), Vector2.Zero, Color.Yellow, Color.Orange, 2f, 16, needed: true);
                    ParticleHandler.SpawnParticle(sparkle);
                    NPC.netUpdate = true;
                    foreach (NPC n in Main.ActiveNPCs)
                        if (n.type == ModContent.NPCType<BrainIllusion>())
                            n.netUpdate = true;
                }

                if (startTime < 30)
                {
                    float lerp = startTime / 30f;
                    float circleDist = MathHelper.Lerp(IllusionDashTeleportDistance, IllusionDashCloseInDistance, BrainCombatUtils.SineOutEasing(lerp, 1));
                    NPC.Center = Vector2.Lerp(AttackPosition, Target.Center + (AttackRotation.ToRotationVector2() * circleDist), lerp);
                    AttackRotation += MathHelper.Lerp(0f, IllusionDashStartingSpinSpeed, BrainCombatUtils.SineInEasing(lerp, 1));
                    NPC.netUpdate = true;
                }
                else if (startTime <= 30 + IllusionDashSpinDuration)
                {
                    NPC.Center = Target.Center + AttackRotation.ToRotationVector2() * IllusionDashCloseInDistance;
                    NPC.netUpdate = true;
                    AttackRotation += MathHelper.Lerp(IllusionDashStartingSpinSpeed, 0f, BrainCombatUtils.SineOutEasing((startTime - 30) / IllusionDashSpinDuration, 1));
                }
                else if (startTime < 30 + IllusionDashSpinDuration + 30)
                {
                    if (startTime < 30 + IllusionDashSpinDuration + 15)
                    {
                        float reelBackSpeedExponent = 2.6f;
                        float reelBackCompletion = Utils.GetLerpValue(0f, 30, startTime - 130, true);
                        float reelBackSpeed = MathHelper.Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                        Vector2 reelBackVelocity = (AttackRotation + MathHelper.Pi).ToRotationVector2() * -reelBackSpeed;
                        NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);
                    }
                    else if (startTime == 30 + IllusionDashSpinDuration + 15)
                        NPC.velocity = (AttackRotation + MathHelper.Pi).ToRotationVector2() * -32;
                    else
                        NPC.velocity *= 0.9f;
                }
                else if (startTime <= 30 + IllusionDashSpinDuration + 30 + IllusionDashFakeoutTeleportDuration) //176
                {
                    NPC.velocity = Vector2.Zero;
                    if (startTime == 30 + IllusionDashSpinDuration + 30)
                    {
                        AttackPosition = Target.Center;
                        AttackRotation = AttackRotation + MathHelper.Pi + Main.rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2);
                        NPC.netUpdate = true;
                    }

                    float wrappedCounter = startTime - (30 + IllusionDashSpinDuration + 30);
                    TeleportDuration = IllusionDashFakeoutTeleportDuration;
                    Vector2 endPoint = AttackPosition + (AttackRotation.ToRotationVector2() * 270);
                    if (wrappedCounter < (TeleportDuration / 2f))
                    {
                        if (wrappedCounter % 2 == 0)
                        {
                            Vector2 startPoint = NPC.Center;
                            Vector2 direction = endPoint - startPoint;
                            float curveIntensity = Main.rand.NextFloat(-0.2f, 0.2f);
                            Vector2 perpindicular = direction.RotatedBy(MathHelper.PiOver2);
                            Vector2 controlPoint1 = startPoint + (direction * 0.25f) + (perpindicular * curveIntensity);
                            Vector2 controlPoint2 = startPoint + (direction * 0.75f) + (perpindicular * curveIntensity);
                            BezierCurve path = new(startPoint, controlPoint1, controlPoint2, endPoint);
                            BrainOfCthulhuAfterImage afterimage = new(path, NPC.rotation, Vector2.One, (int)((TeleportDuration * 0.75f) - wrappedCounter), BoCFrame);
                            BoCAfterImages.Add(afterimage);
                            ParticleHandler.SpawnParticle(afterimage);
                        }

                        TeleportTime++;
                    }
                    else if (wrappedCounter == (int)(TeleportDuration / 2f) && !AttackFlag)
                    {
                        AttackFlag = true;
                        NPC.Center = endPoint;
                        NPC.netUpdate = true;
                    }
                    else
                    {
                        TeleportTime--;
                        if (TeleportTime <= 0)
                        {
                            TeleportTime = 0;
                            BoCAfterImages = [];
                            ResetAttackValues();
                            NPC.Opacity = 1f;
                            NPC.netUpdate = true;
                            AttackRotation = NPC.AngleTo(Target.Center);
                        }
                    }

                    NPC.Opacity = 1 - (TeleportTime / (TeleportDuration / 2f));
                }
                else if (startTime <= 150 + IllusionDashSpinDuration + 30 + IllusionDashFakeoutTeleportDuration + 30)
                {
                    NPC.velocity *= 0.9f;
                    if (startTime % 15 == 5 && startTime > 60 + IllusionDashSpinDuration + 30 + IllusionDashFakeoutTeleportDuration)
                    {
                        Vector2 dir = NPC.DirectionTo(Target.Center);
                        for (int i = 0; i < (AnomalySharedData.Anomaly ? 2 : 1); i++)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Vector2 initialDir = dir.RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-0.01f, 0.01f));
                                Vector2 spawnPos = NPC.Center + (dir.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-NPC.width, NPC.width) - (dir * 48));
                                Projectile.NewProjectileAction(source: NPC.GetSource_FromThis(), position: spawnPos, velocity: initialDir * Main.rand.NextFloat(6f, 8f), type: ModContent.ProjectileType<BrainBloodShot>(), damage: BloodShotDamage, knockback: 0.5f, action: entity =>
                                {
                                    entity.ai[0] = dir.ToRotation() + MathHelper.TwoPi;
                                    entity.ai[1] = initialDir.ToRotation();
                                });
                            }

                            NPC.velocity -= dir;
                        }
                    }
                }
                else if (startTime >= 180 + IllusionDashSpinDuration + 30 + IllusionDashFakeoutTeleportDuration + 30)
                {
                    foreach (NPC n in Main.ActiveNPCs)
                        if (n.type == ModContent.NPCType<BrainIllusion>())
                            n.active = false;
                    SetupForNextAttack();
                }
            }
        }

        void IllusionTrick()
        {
            if (Time >= 90)
            {
                if (Time == 90)
                {
                    foreach (Projectile p in Main.ActiveProjectiles)
                    {
                        if (!p.friendly)
                            continue;
                        p.Anomaly.IgnoreBrainIllusions = true;
                    }

                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        return;
                    int brainAngleSlot = Main.rand.Next(0, IllusionTrickAngleGroups);
                    int brainDistSlot = Main.rand.Next(0, IllusionTrickGroupSize);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        for (int a = 0; a < IllusionTrickAngleGroups; a++)
                            for (int d = 0; d < IllusionTrickGroupSize; d++)
                            {
                                if (a != brainAngleSlot || d != brainDistSlot)
                                    NPC.NewNPCAction(source: NPC.GetSource_FromThis(), position: new Vector2(0, 0), type: ModContent.NPCType<FalseBrain>(), start: 0, action: entity =>
                                    {
                                        entity.ai[0] = MathHelper.TwoPi / IllusionTrickAngleGroups * a;
                                        entity.ai[1] = FalseBrain.TimeDivisor / IllusionTrickGroupSize * d;
                                    });
                            }

                    AttackTime = (int)(FalseBrain.TimeDivisor / IllusionTrickGroupSize * brainDistSlot);
                    AttackRotation = MathHelper.TwoPi / IllusionTrickAngleGroups * brainAngleSlot;
                    AttackFlag = false;
                    if (Main.netMode == NetmodeID.SinglePlayer)
                        AttackPosition = Target.Center;
                    else
                    {
                        List<int> nearbyPlayers = GetAllValidTargets(NPC.Center);
                        Vector2 averagePosition = Vector2.Zero;
                        foreach (int p in nearbyPlayers)
                        {
                            TargetsSet.Add(p);
                            averagePosition += Main.player[p].Center;
                        }

                        averagePosition = nearbyPlayers.Count > 0 ? averagePosition / nearbyPlayers.Count : Target.Center;
                        AttackPosition = averagePosition;
                    }

                    NPC.ShowNameOnHover = false;
                    NPC.netUpdate = true;
                }
                else
                {
                    Vector2 goalPos;
                    if (Main.netMode == NetmodeID.SinglePlayer)
                        goalPos = Target.Center;
                    else
                    {
                        Vector2 averagePosition = Vector2.Zero;
                        foreach (int who in TargetsSet)
                            averagePosition += Main.player[who].Center;
                        averagePosition = TargetsSet.Count > 0 ? averagePosition / TargetsSet.Count : Target.Center;
                        goalPos = averagePosition;
                    }

                    float distSq = AttackPosition.DistanceSQ(goalPos);
                    if (distSq > 90000) //300^2
                    {
                        float dist = MathF.Sqrt(distSq);
                        Vector2 dir = (goalPos - AttackPosition).SafeNormalize(Vector2.Zero);
                        AttackPosition += dir * (dist - 300) / 60f;
                    }
                }

                NPC.damage = 0;
                NPC.dontTakeDamage = false;
                if (AttackFlag)
                {
                    if (CurrentLocalPhase == 0)
                    {
                        NPC.ShowNameOnHover = true;
                        NPC.velocity = NPC.DirectionFrom(Target.Center) * 4f;
                    }
                    else
                        NPC.velocity *= 0.95f;
                    if (CurrentLocalPhase >= IllusionTrickStunDuration)
                    {
                        SetupForNextAttack();
                        NPC.Opacity = 1f;
                        TeleportTime = 0;
                        return;
                    }

                    CurrentLocalPhase++;
                }
                else if (Time >= IllusionTrickTimeLimit) //Players have failed to find the real BoC within the time limit
                {
                    if (Time == IllusionTrickTimeLimit)
                    {
                        List<int> targets = GetAllValidTargets(NPC.Center);
                        AttackList.Clear();
                        foreach (int p in targets)
                            AttackList.Add((byte)p);
                        foreach (NPC n in Main.ActiveNPCs)
                        {
                            if (n.type != ModContent.NPCType<FalseBrain>())
                                continue;
                            n.GetModNPC<FalseBrain>().BeenHit = true;
                            n.netUpdate = true;
                        }

                        for (int i = 1; i <= 3; i++)
                        {
                            Color color = i switch
                            {
                                1 => Color.Yellow,
                                2 => Color.Orange,
                                _ => Color.Red,
                            };
                            PulseRing ring = new(NPC.Center, NPC.velocity * 0.5f, color, 0f, 1f + i * 0.5f, 24);
                            ParticleHandler.SpawnParticle(ring);
                        }

                        NPC.netUpdate = true;
                    }
                    else if (Time % 30 == 0)
                    {
                        if (AttackList.Count > 0)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                Projectile.NewProjectileAction(source: NPC.GetSource_FromThis(), position: NPC.Center, velocity: Vector2.Zero, type: ModContent.ProjectileType<TelekineticBlast>(), damage: 0, knockback: 0f, owner: -1, action: entity =>
                                {
                                    entity.ai[0] = AttackList[0];
                                    entity.ai[1] = 0;
                                    entity.ai[2] = NPC.whoAmI;
                                });
                            AttackList.RemoveAt(0);
                        }
                        else
                        {
                            Vector2 direction = Target.velocity.SafeNormalize(Vector2.UnitX * Target.direction).RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 3f, MathHelper.Pi / 3f));
                            float distance = DefaultTeleportDistance;
                            NPC.damage = NPC.defDamage;
                            AttackPosition = Target.Center + (direction * distance);
                            BoCAfterImages = [];
                            NPC.Opacity = 1f;
                            TeleportTime = 0;
                            Time = ChaseTime - 1;
                            NPC.netUpdate = true;
                            ResetAttackValues();
                            CurrentBehavior = Behavior.Phase2Idle;
                        }
                    }
                }
                else
                {
                    CachedRatio = (float)Math.Cos(AttackTime * MathHelper.TwoPi / FalseBrain.TimeDivisor);
                    float lerp = BrainCombatUtils.SineInOutEasing(MathHelper.Clamp((Time - 150) / 30f, 0f, 1f), 1);
                    float baseDist = 240;
                    float circleDist = 480;
                    if (Time - 150 < 30f)
                    {
                        baseDist = MathHelper.Lerp(480, 240, lerp);
                        circleDist = MathHelper.Lerp(240, 480, lerp);
                    }

                    NPC.Center = AttackPosition + Vector2.UnitX.RotatedBy(AttackRotation) * (baseDist + (circleDist * ((float)Math.Sin(-AttackTime * MathHelper.TwoPi / FalseBrain.TimeDivisor) / 2f + 0.5f)));
                    NPC.Center += Vector2.UnitX.RotatedBy(AttackRotation + MathHelper.PiOver2) * (90 * (float)Math.Cos(AttackTime * MathHelper.TwoPi / FalseBrain.TimeDivisor));
                    NPC.Opacity = 1f;
                    NPC.netUpdate = true;
                    CurrentLocalPhase = 0;
                }
            }
            else if (Time <= 60)
            {
                TeleportDuration = 60;
                TeleportTime++;
                NPC.Opacity = 1 - (Time / 60f);
                if (Time < 50)
                {
                    Vector2 startPoint = NPC.Center;
                    Vector2 endPoint = NPC.Center + Vector2.UnitX.RotatedBy(Main.rand.NextFloat(0, MathHelper.TwoPi)) * Main.rand.NextFloat(240, 480);
                    Vector2 direction = endPoint - startPoint;
                    float curveIntensity = Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 perpindicular = direction.RotatedBy(MathHelper.PiOver2);
                    Vector2 controlPoint1 = startPoint + (direction * 0.25f) + (perpindicular * curveIntensity);
                    Vector2 controlPoint2 = startPoint + (direction * 0.75f) + (perpindicular * curveIntensity);
                    BezierCurve path = new(startPoint, controlPoint1, controlPoint2, endPoint);
                    BrainOfCthulhuAfterImage afterimage = new(path, NPC.rotation, Vector2.One, (int)(60 - Time), BoCFrame);
                    BoCAfterImages.Add(afterimage);
                    ParticleHandler.SpawnParticle(afterimage);
                }
            }
            else
            {
                NPC.Opacity = 0f;
                BoCAfterImages = [];
                NPC.damage = 0;
                NPC.dontTakeDamage = true;
            }

            if (Time >= 180)
                AttackTime++;
            else if (Time >= 150)
                AttackTime += (Time - 150) / 30f;
        }

        void SetupForNextAttack()
        {
            Player target = Main.player[NPC.target];
            Vector2 direction = target.velocity.SafeNormalize(Vector2.UnitX * target.direction).RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 3f, MathHelper.Pi / 3f));
            float distance = DefaultTeleportDistance;
            AttackPosition = target.Center + (direction * distance);
            BoCAfterImages = [];
            Time = ChaseTime - 1;
            ResetAttackValues();
            NPC.netUpdate = true;
            if (AvailableAttacks.Count != 0)
            {
                if (AvailableAttacks[0] != Behavior.Phase2Idle)
                    CurrentLocalPhase = 4;
                else
                    AvailableAttacks.RemoveAt(0);
            }

            CurrentBehavior = Behavior.Phase2Idle;
        }
    }
}
