namespace Anomalies.Bosses.Skeletron;

public sealed partial class SkeletronHead
{
    public override bool PreAI()
    {
        using BossNetUpdateScope netUpdate = new(NPC);
        // Late joiners and predicted timers receive the same complete per-NPC snapshot.
        if (Main.netMode != NetmodeID.MultiplayerClient && ++ResyncTimer >= 60f)
        {
            ResyncTimer = 0f;
            NPC.netUpdate = true;
        }

        // Percent life remaining
        float lifeRatio = NPC.life / (float)NPC.lifeMax;

        // Set defense
        NPC.defense = NPC.defDefense;
        NPC.damage = NPC.defDamage;

        NPC.reflectsProjectiles = false;

        // Get a target
        if (NPC.target < 0 || NPC.target >= Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            NPC.TargetClosest();
        if (NPC.target < 0 || NPC.target >= Main.maxPlayers || !Main.player[NPC.target].active || Main.player[NPC.target].dead)
        {
            NPC.velocity.Y += 0.1f;
            NPC.EncourageDespawn(10);
            return false;
        }

        bool initialize = CurrentPhase == Phase.Initialize;
        CurrentPhase = lifeRatio < Phase5LifeRatio ? Phase.Phase5
            : lifeRatio < Phase4LifeRatio ? Phase.Phase4
            : lifeRatio < RespawnHandsLifeRatio ? Phase.Phase3_2
            : lifeRatio < Phase3LifeRatio ? Phase.Phase3
            : lifeRatio < 1f ? Phase.Phase2 : Phase.Phase1;

        // Spawn hands
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            if (initialize)
            {
                SpawnHands();
                NPC.netUpdate = true;
            }

            // Respawn hands
            if (RespawnHands && AnomalyNPC.AnomalyAI32[0].f == 0f && Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) > 160f)
            {
                AnomalyNPC.AnomalyAI32[0].f = 1f;
                SoundEngine.PlaySound(SoundID.ForceRoar with { Pitch = SoundID.ForceRoar.Pitch - 0.25f }, NPC.Center);
                SpawnHands();

                NPC.netUpdate = true;
            }

            void SpawnHands()
            {
                SpawnHand(-1.3f, 0f, 0f);
                SpawnHand(1.3f, 0f, 150f);
                SpawnHand(-1.3f, -1f, RespawnHands ? -75f : 0f);
                SpawnHand(1.3f, -1f, RespawnHands ? 75f : 150f);
            }

            void SpawnHand(float side, float verticalSide, float timer)
            {
                NPC.NewNPCAction(NPC.GetSource_FromAI(), NPC.Center, NPCID.SkeletronHand, NPC.whoAmI, hand =>
                {
                    hand.ai[0] = side;
                    hand.ai[1] = NPC.whoAmI;
                    hand.ai[3] = timer;
                    hand.target = NPC.target;
                    hand.Anomaly.AnomalyAI32[0].f = verticalSide;
                    hand.Anomaly.AnomalyAI32[2].f = timer;
                    hand.netUpdate = true;
                });
            }
        }

        // Despawn
        if (CurrentBehavior != Behavior.Despawn)
        {
            int despawnDistanceInTiles = 500;
            if (Main.player[NPC.target].dead || Math.Abs(NPC.Center.X - Main.player[NPC.target].Center.X) / 16f > despawnDistanceInTiles)
            {
                NPC.TargetClosest();
                if (Main.player[NPC.target].dead || Math.Abs(NPC.Center.X - Main.player[NPC.target].Center.X) / 16f > despawnDistanceInTiles)
                    CurrentBehavior = Behavior.Despawn;
            }
            else if (NPC.timeLeft < 1800)
                NPC.timeLeft = 1800;
        }

        // Daytime enrage
        if (Main.IsItDay() && CurrentBehavior != Behavior.Despawn && CurrentBehavior != Behavior.Enraged)
        {
            CurrentBehavior = Behavior.Enraged;
            SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
        }

        // Hand count
        int numHandsAlive = 0;
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            if (Main.npc[i].active && Main.npc[i].type == NPCID.SkeletronHand && Main.npc[i].ai[1] == NPC.whoAmI)
                numHandsAlive++;
        }

        // Hand variables
        bool handsDead = numHandsAlive == 0;
        int numProj = Main.getGoodWorld ? 22 : 5;
        float spread = Main.getGoodWorld ? 180 : 60;
        float headSpinVelocityMult = Phase3 ? 12f : 4.5f;

        switch (numHandsAlive)
        {
            case 0:
                numProj = Main.getGoodWorld ? 36 : 9;
                spread = Main.getGoodWorld ? 180 : 90;
                headSpinVelocityMult = Phase3 ? 12f : 6f;
                break;

            case 1:
                numProj = Main.getGoodWorld ? 27 : 7;
                spread = Main.getGoodWorld ? 150 : 76;
                headSpinVelocityMult = Phase3 ? 11.5f : 5f;
                break;

            case 2:
                numProj = Main.getGoodWorld ? 18 : 6;
                spread = Main.getGoodWorld ? 140 : 70;
                headSpinVelocityMult = Phase3 ? 11f : 4.5f;
                break;

            case 3:
                numProj = Main.getGoodWorld ? 15 : 5;
                spread = Main.getGoodWorld ? 130 : 64;
                headSpinVelocityMult = Phase3 ? 10.5f : 4f;
                break;

            case 4:
                numProj = Main.getGoodWorld ? 12 : 4;
                spread = Main.getGoodWorld ? 120 : 56;
                headSpinVelocityMult = Phase3 ? 10f : 3.5f;
                break;
        }

        headSpinVelocityMult *= 1.08f; // Very volatile value

        // Velocity used to move Skeletron away from the target before charging
        float moveAwayVelocity = headSpinVelocityMult;
        if (!Phase3)
            moveAwayVelocity *= 2f;

        // Hand DR, scale DR up if the hands are still alive as Skeletron's HP lowers
        NPC.chaseable = handsDead;
        float minDR = 0f;
        float maxDR = 0.9999f;
        AnomalyNPC.DR = !handsDead ? (float)Math.Sqrt(MathHelper.Lerp(minDR, maxDR, RespawnHands ? (RespawnHandsLifeRatio - lifeRatio) / RespawnHandsLifeRatio : 2f - lifeRatio / RespawnHandsLifeRatio)) : minDR;
        AnomalyNPC.ExtraDR = AnomalyNPC.DR;
        AnomalyNPC.DR = 0f;
        AnomalyNPC.CurrentlyIncreasingDefenseOrDR = !handsDead;

        // Value to start teleport dust
        int teleportGateValue = Phase5 ? 180 : 300;

        // Bool to disable skull firing after charging if teleport was recent or is about to happen
        bool disableSkullsAfterCharge = TeleportTimer <= 60f || TeleportTimer > teleportGateValue + 60f;

        UpdateTeleport();

        ShootSkulls();

        switch (CurrentBehavior)
        {
            case Behavior.Hover:
                if (Hover())
                    return false;
                break;
            case Behavior.SpinCharge:
                SpinCharge();
                break;
            case Behavior.Enraged:
                Enraged();
                break;
            case Behavior.Despawn:
                Despawn();
                break;
        }

        // Emit dust
        if (CurrentBehavior != Behavior.Enraged && CurrentBehavior != Behavior.Despawn && numHandsAlive != 0)
        {
            Dust.NewDustAction(new Vector2(NPC.Center.X - 15f - NPC.velocity.X * 5f, NPC.position.Y + NPC.height - 2f) + new Vector2(30 / 2f, 10 / 2f), 30, 10, DustID.Blood, new Vector2(-NPC.velocity.X * 0.2f, 3f), dust =>
            {
                dust.alpha = 0;
                dust.scale *= 2f;
                dust.noGravity = true;
                dust.velocity.X = dust.velocity.X * 1.3f;
                dust.velocity.X = dust.velocity.X + NPC.velocity.X * 0.4f;
                dust.velocity.Y = dust.velocity.Y + (2f + NPC.velocity.Y);
            });
            for (int j = 0; j < 2; j++)
            {
                Dust.NewDustAction(new Vector2(NPC.position.X, NPC.position.Y + 120f) + new Vector2(NPC.width / 2f, 60 / 2f), NPC.width, 60, DustID.Blood, new Vector2(NPC.velocity.X, NPC.velocity.Y), dust =>
                {
                    dust.alpha = 0;
                    dust.scale *= 2f;
                    dust.noGravity = true;
                    dust.velocity -= NPC.velocity;
                    dust.velocity.Y = dust.velocity.Y + 5f;
                });
            }
        }

        return false;

        void UpdateTeleport()
        {
            // Teleport while not despawning
            if (CurrentBehavior != Behavior.Despawn)
            {
                int dustType = DustID.GemDiamond;

                // Post-teleport
                if (TeleportTimer == -60f)
                {
                    TeleportTimer = 0f;

                    SoundEngine.PlaySound(SoundID.Item66, NPC.Center);

                    // Fire skulls after teleport
                    if (Collision.CanHit(NPC.Center, 1, 1, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height))
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int type = ProjectileID.Skull;

                            // Inverse parabolic projectile spreads
                            Vector2 baseVel = (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.UnitY) * (6f);
                            Vector2 firingPos = NPC.Center + baseVel * 5f;
                            float centralCount = 0.5f * (numProj - 1f);
                            for (int i = 0; i < numProj; i++)
                            {
                                float offset = MathHelper.ToRadians(MathHelper.Lerp(-spread * 0.5f, spread * 0.5f, i / (numProj - 1f)));
                                float velocityMult = MathHelper.Lerp(0.5f, 1.5f, MathF.Abs(centralCount - i) / centralCount);
                                Projectile.NewProjectileAction(NPC.GetSource_FromAI(), firingPos, baseVel.RotatedBy(offset) * velocityMult, type, SkullDamage, 0f, Main.myPlayer, action: p =>
                                {
                                    p.ai[0] = Skull.AcceleratingCrossbone;
                                    p.timeLeft = 600;
                                });
                            }

                            NPC.netUpdate = true;
                        }
                    }

                    // Teleport dust
                    for (int m = 0; m < 30; m++)
                    {
                        Dust.NewDustAction(NPC.position + new Vector2(NPC.width / 2f, NPC.height / 2f), NPC.width, NPC.height, dustType, new Vector2(0f, 0f), dust =>
                        {
                            dust.alpha = 100;
                            dust.scale *= 3f;
                            dust.noGravity = true;
                            dust.velocity.X *= 2f;
                        });
                    }
                }

                // Teleport after a certain time
                // If hands are dead: 7 seconds
                // If hands are not dead: 14 seconds
                // If hands are dead in phase 2: 4.7 seconds
                TeleportTimer += 1f + (((Phase2 && handsDead) || Phase4) ? 0.5f : 0f) - (handsDead ? 0f : 0.5f);

                // Dust to show teleport
                int ai3 = (int)TeleportTimer; // 0 to 30, and -60

                if (ChargeReadiness == 1f && TeleportX == 0f && TeleportY == 0f)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 skullFaceDirection = NPC.Center + new Vector2(NPC.direction * 20, 6f);
                        Vector2 skullTargetDirection = Main.player[NPC.target].Center - skullFaceDirection;
                        Point skullTileCoords = NPC.Center.ToTileCoordinates();
                        Point targetTileCoords = Main.player[NPC.target].Center.ToTileCoordinates();
                        int randomTeleportOffset = 20 - (int)Math.Ceiling(MathHelper.Lerp(0f, 10f, 1f - lifeRatio));
                        int skullPositionOffset = 4;
                        int targetPositionOffset = randomTeleportOffset - 4;
                        int teleportTries = 0;

                        bool targetTooFar = false;
                        if (skullTargetDirection.Length() > 2000f)
                            targetTooFar = true;

                        while (!targetTooFar && teleportTries < 100)
                        {
                            teleportTries++;
                            int teleportTileX = Main.rand.Next(targetTileCoords.X - randomTeleportOffset, targetTileCoords.X + randomTeleportOffset + 1);
                            int teleportTileY = Main.rand.Next(targetTileCoords.Y - randomTeleportOffset, targetTileCoords.Y + randomTeleportOffset + 1);
                            if ((teleportTileY < targetTileCoords.Y - targetPositionOffset || teleportTileY > targetTileCoords.Y + targetPositionOffset || teleportTileX < targetTileCoords.X - targetPositionOffset || teleportTileX > targetTileCoords.X + targetPositionOffset) && (teleportTileY < skullTileCoords.Y - skullPositionOffset || teleportTileY > skullTileCoords.Y + skullPositionOffset || teleportTileX < skullTileCoords.X - skullPositionOffset || teleportTileX > skullTileCoords.X + skullPositionOffset) && WorldGen.InWorld(teleportTileX, teleportTileY, 10) && !Main.tile[teleportTileX, teleportTileY].HasUnactuatedTile)
                            {
                                // New location params
                                TeleportX = teleportTileX * 16;
                                TeleportY = teleportTileY * 16;
                                NPC.netUpdate = true;
                                break;
                            }
                        }
                    }
                }

                // Teleport location telegraph
                if (TeleportX != 0f && TeleportY != 0f)
                {
                    for (int m = 0; m < 5; m++)
                    {
                        Vector2 position = new Vector2(TeleportX, TeleportY);
                        Dust.NewDustAction(position + new Vector2(NPC.width / 2f, NPC.height / 2f), NPC.width, NPC.height, dustType, new Vector2(0f, 0f), dust =>
                        {
                            dust.alpha = 100;
                            dust.scale *= 2f;
                            dust.noGravity = true;
                        });
                    }
                }

                // Teleport
                if (Main.netMode != NetmodeID.MultiplayerClient && ChargeReadiness == 0f && CurrentBehavior != Behavior.SpinCharge && TeleportX != 0f && TeleportY != 0f)
                {
                    // Teleport dust
                    for (int m = 0; m < 30; m++)
                    {
                        Dust.NewDustAction(NPC.position + new Vector2(NPC.width / 2f, NPC.height / 2f), NPC.width, NPC.height, dustType, new Vector2(0f, 0f), dust =>
                        {
                            dust.alpha = 100;
                            dust.scale *= 3f;
                            dust.noGravity = true;
                            dust.velocity.X *= 2f;
                        });
                    }

                    // New location
                    NPC.Center = new Vector2(TeleportX, TeleportY);
                    foreach (NPC hand in Main.npc)
                    {
                        if (!hand.active || hand.type != NPCID.SkeletronHand || hand.ai[1] != NPC.whoAmI)
                            continue;
                        hand.Center = NPC.Center;
                        hand.velocity = Vector2.Zero;
                        hand.netUpdate = true;
                    }
                    NPC.velocity = Vector2.Zero;

                    TeleportTimer = -60f;
                    TeleportX = TeleportY = 0f;
                    NPC.netUpdate = true;
                }
            }
        }

        void ShootSkulls()
        {
            // Skull shooting
            if ((handsDead) && CurrentBehavior == Behavior.Hover && !Phase4)
            {
                float skullProjFrequency = Phase2 ? (48f - (10f * (1f - lifeRatio))) : 60f;
                if (Main.getGoodWorld)
                    skullProjFrequency *= 0.8f;
                skullProjFrequency = (float)Math.Ceiling(skullProjFrequency);

                if (Main.netMode != NetmodeID.MultiplayerClient && AttackTimer % skullProjFrequency == 0f && AttackTimer > 45f)
                {
                    Vector2 skullFiringPos = NPC.Center;
                    float skullProjTargetX = Main.player[NPC.target].Center.X - skullFiringPos.X;
                    float skullProjTargetY = Main.player[NPC.target].Center.Y - skullFiringPos.Y;
                    if (Collision.CanHit(skullFiringPos, 1, 1, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height))
                    {
                        float skullProjSpeed = Phase2 ? (5f + (1f * (1f - lifeRatio))) : 4f;
                        int spread2 = 50;
                        Vector2 skullProjDirection = new Vector2(skullProjTargetX + Main.rand.Next(-spread2, spread2 + 1) * 0.01f, skullProjTargetY + Main.rand.Next(-spread2, spread2 + 1) * 0.01f).SafeNormalize(Vector2.UnitY);
                        skullProjDirection *= skullProjSpeed;
                        skullProjDirection += NPC.velocity;
                        skullFiringPos += skullProjDirection * 5f;

                        int type = ProjectileID.Skull;

                        Projectile.NewProjectileAction(NPC.GetSource_FromAI(), skullFiringPos, skullProjDirection, type, SkullDamage, 0f, Main.myPlayer, action: p =>
                        {
                            p.ai[0] = Skull.Homing;
                            p.timeLeft = 600;
                        });
                        if (handsDead)
                        {
                            skullProjDirection = new Vector2(skullProjTargetX, skullProjTargetY).SafeNormalize(Vector2.UnitY);
                            skullProjDirection *= skullProjSpeed * 2f;
                            Projectile.NewProjectileAction(NPC.GetSource_FromAI(), skullFiringPos, skullProjDirection, type, SkullDamage, 0f, Main.myPlayer, action: p =>
                            {
                                p.ai[0] = Skull.AcceleratingCrossbone;
                                p.timeLeft = 600;
                            });
                        }

                        NPC.netUpdate = true;
                    }
                }
            }
        }

        bool Hover()
        {
            AttackTimer += 1f;
            float chargePhaseChangeRateBoost = Phase5 ? (24f) : Phase4 ? (6f) : ((4.5f) * ((1f - lifeRatio) / (1f - Phase4LifeRatio)));
            if (!handsDead)
                chargePhaseChangeRateBoost *= 0.25f;

            float chargePhaseChangeRate = chargePhaseChangeRateBoost + 1f;
            Timer += chargePhaseChangeRate;
            ChargeTelegraphTimer += chargePhaseChangeRate;
            float chargePhaseGateValue = ChargeGateValue;
            if (ChargeTelegraphTimer > chargePhaseGateValue)
                ChargeTelegraphTimer = chargePhaseGateValue;

            float forcedMoveAwayTime = 30f;
            float canChargeDistance = 320f; // 20 tile distance
            bool hasMovedForcedDistance = RetreatTimer >= forcedMoveAwayTime;
            bool canCharge = Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) >= canChargeDistance;
            bool charge = Timer >= chargePhaseGateValue && canCharge;
            bool forceCharge = Timer > chargePhaseGateValue + 120f;
            if (charge || forceCharge)
            {
                RetreatTimer += 1f;
                if ((hasMovedForcedDistance || !Phase3) && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Timer = 0f;
                    CurrentBehavior = Behavior.SpinCharge;
                    CurrentLocalPhase = 0;
                    ChargeReadiness = 1f;
                    ChargeTelegraphTimer = chargePhaseGateValue;
                    RetreatTimer = 0f;
                    AttackTimer = 0f;

                    NPC.netUpdate = true;
                }
            }

            float headYAcceleration = (Main.getGoodWorld ? 0.07f : (0.06f + 0.04f * (1f - lifeRatio)));
            float headYTopSpeed = headYAcceleration * 100f;
            float headXAcceleration = (Main.getGoodWorld ? 0.21f : (0.16f + 0.08f * (1f - lifeRatio)));
            float headXTopSpeed = headXAcceleration * 100f;
            float deceleration = Main.getGoodWorld ? 0.83f : 0.86f;

            float moveAwayGateValue = chargePhaseGateValue - (5f + chargePhaseChangeRate);
            bool moveAwayBeforeCharge = Timer >= moveAwayGateValue;
            if (CurrentBehavior == Behavior.Hover)
                CurrentLocalPhase = moveAwayBeforeCharge ? 1 : 0;
            if (moveAwayBeforeCharge)
            {
                if (!canCharge || !hasMovedForcedDistance)
                {
                    float phase5Multiplier = 1.2f;
                    float maxVelocity = (Timer - moveAwayGateValue) * (moveAwayVelocity * 0.002f);
                    if (Phase5)
                        maxVelocity *= phase5Multiplier;

                    float maxVelocityCap = moveAwayVelocity;
                    if (Phase5)
                        maxVelocityCap *= phase5Multiplier;
                    if (maxVelocity > maxVelocityCap)
                        maxVelocity = maxVelocityCap;

                    NPC.velocity = (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.UnitY) * -maxVelocity;
                }

                // New charge attack
                if (Phase3)
                {
                    NPC.rotation += NPC.direction * 0.3f;

                    if (ChargeReadiness == 0f)
                    {
                        ChargeReadiness = 1f;
                        NPC.netUpdate = true;

                        SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
                    }
                }
                else
                    NPC.rotation = NPC.velocity.X / 15f;

                return true;
            }

            float rotation = NPC.velocity.X / 15f;
            NormalMovement(NPC, Main.player[NPC.target], headXTopSpeed, headYTopSpeed,
                headXAcceleration, headYAcceleration, 250f);
            NPC.rotation = rotation;

            return false;
        }

        void SpinCharge()
        {
            if (Main.getGoodWorld)
                NPC.reflectsProjectiles = true;
            if (Aroma)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient && Timer == 0f)
                {
                    if (NPC.CountNPCS(NPCID.DarkCaster) < 6)
                    {
                        for (int i = 0; i < 1000; i++)
                        {
                            int headYAcceleration = (int)(NPC.Center.X / 16f) + Main.rand.Next(-50, 51);
                            int headYTopSpeed;
                            for (headYTopSpeed = (int)(NPC.Center.Y / 16f) + Main.rand.Next(-50, 51); headYTopSpeed < Main.maxTilesY - 10 && !WorldGen.SolidTile(headYAcceleration, headYTopSpeed); headYTopSpeed++)
                            {
                            }

                            headYTopSpeed--;
                            if (!WorldGen.SolidTile(headYAcceleration, headYTopSpeed))
                            {
                                NPC.NewNPCAction(NPC.GetSource_FromAI(), new Vector2(headYAcceleration * 16 + 8, headYTopSpeed * 16), NPCID.DarkCaster);

                                break;
                            }
                        }
                    }

                    if (Aroma)
                    {
                        if (!NPC.AnyNPCs(NPCID.DiabolistWhite))
                        {
                            for (int i = 0; i < 1000; i++)
                            {
                                int headYAcceleration = (int)(NPC.Center.X / 16f) + Main.rand.Next(-50, 51);
                                int headYTopSpeed;
                                for (headYTopSpeed = (int)(NPC.Center.Y / 16f) + Main.rand.Next(-50, 51); headYTopSpeed < Main.maxTilesY - 10 && !WorldGen.SolidTile(headYAcceleration, headYTopSpeed); headYTopSpeed++)
                                {
                                }

                                headYTopSpeed--;
                                if (!WorldGen.SolidTile(headYAcceleration, headYTopSpeed))
                                {
                                    NPC.NewNPCAction(NPC.GetSource_FromAI(), new Vector2(headYAcceleration * 16 + 8, headYTopSpeed * 16), NPCID.DiabolistWhite);

                                    break;
                                }
                            }
                        }
                    }
                }
            }

            NPC.defense = NPC.defDefense - 10;
            NPC.damage = (int)Math.Round(NPC.defDamage * SpinDamageMult);

            float phaseChangeRateBoost = Phase3 ? 0f : 1f - (lifeRatio - Phase3LifeRatio) / (1f - Phase3LifeRatio);
            Timer += 1f + phaseChangeRateBoost;

            AttackTimer += 1f;
            if (AttackTimer == 2f)
                SoundEngine.PlaySound(Phase3 ? SoundID.ForceRoarPitched : SoundID.ForceRoar, NPC.Center);

            // Reset telegraph timer to create color fade
            if (ChargeTelegraphTimer > 0f)
            {
                ChargeTelegraphTimer -= 2f;
                if (ChargeTelegraphTimer <= 0f)
                {
                    ChargeTelegraphTimer = 0f;
                    NPC.netUpdate = true;
                }
            }

            bool dontGoMach10 = false;
            float dashPhaseTime = 210f;
            //Netmode check fixes issues where attack ends at different times for server vs clients
            if (Timer >= dashPhaseTime && Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (Aroma)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient && NPC.CountNPCS(NPCID.DarkCaster) < 6)
                    {
                        for (int j = 0; j < 1000; j++)
                        {
                            int headYAcceleration = (int)(NPC.Center.X / 16f) + Main.rand.Next(-50, 51);
                            int headYTopSpeed;
                            for (headYTopSpeed = (int)(NPC.Center.Y / 16f) + Main.rand.Next(-50, 51); headYTopSpeed < Main.maxTilesY - 10 && !WorldGen.SolidTile(headYAcceleration, headYTopSpeed); headYTopSpeed++)
                            {
                            }

                            headYTopSpeed--;
                            if (!WorldGen.SolidTile(headYAcceleration, headYTopSpeed))
                            {
                                NPC.NewNPCAction(NPC.GetSource_FromAI(), new Vector2(headYAcceleration * 16 + 8, headYTopSpeed * 16), NPCID.DarkCaster);

                                break;
                            }
                        }
                    }
                }

                Timer = 0f;
                CurrentBehavior = Behavior.Hover;
                CurrentLocalPhase = 0;
                ChargeReadiness = 0f;
                ChargeTelegraphTimer = 0f;
                AttackTimer = 0f;

                NPC.TargetClosest();
                NPC.netUpdate = true;

                dontGoMach10 = true;
            }

            NPC.rotation += NPC.direction * 0.3f;

            Vector2 headSpinPos = NPC.Center;
            float headSpinTargetX = Main.player[NPC.target].Center.X - headSpinPos.X;
            float headSpinTargetY = Main.player[NPC.target].Center.Y - headSpinPos.Y;
            float headSpinTargetDist = (float)Math.Sqrt(headSpinTargetX * headSpinTargetX + headSpinTargetY * headSpinTargetY);

            // Increase speed while charging
            if (!Phase3)
            {
                float velocityBoost = MathHelper.Lerp(0f, 3f, (1f - lifeRatio) / (1f - Phase3LifeRatio));
                if (handsDead)
                    headSpinVelocityMult += velocityBoost;
            }

            float altDashStopDistance = 320f;
            float headSpeedIncreaseDist = Phase3 ? altDashStopDistance : 160f;
            if (headSpinTargetDist > headSpeedIncreaseDist)
            {
                float velocityMult = Phase3 ? 0.00075f : 0.0015f;
                float baseDistanceVelocityMult = 1f + MathHelper.Clamp((headSpinTargetDist - headSpeedIncreaseDist) * 0.0015f, 0.05f, 2f);
                headSpinVelocityMult *= baseDistanceVelocityMult;
            }

            if (Main.getGoodWorld)
                headSpinVelocityMult *= 1.3f;

            Vector2 headSpinVelocity = new Vector2(headSpinTargetX, headSpinTargetY).SafeNormalize(Vector2.UnitY) * headSpinVelocityMult;

            if (!dontGoMach10)
            {
                if (Phase3)
                {
                    // Dash directly towards the target until within 15 tiles of the target, and then continue in the same direction
                    float altDashPhaseTime = dashPhaseTime * (0.9f);
                    if (CurrentLocalPhase == 0 && Timer < altDashPhaseTime)
                    {
                        if (NPC.Center.Distance(Main.player[NPC.target].Center) > altDashStopDistance || Timer == 1f + phaseChangeRateBoost)
                            NPC.velocity = headSpinVelocity.SafeNormalize(Vector2.UnitY) * headSpinVelocityMult + NPC.Center.DirectionTo(Main.player[NPC.target].Center) * 2f;
                        else if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            CurrentLocalPhase = 1;
                            Timer = altDashPhaseTime;
                            NPC.netUpdate = true;
                        }
                    }
                }
                else
                    NPC.velocity = headSpinVelocity;
            }
                }

        void Enraged()
        {
            NPC.damage = 1000;
            AnomalyNPC.ExtraDR = 0.9999f;
            AnomalyNPC.DR = 0f;

            AnomalyNPC.CurrentlyEnraged = true;
            AnomalyNPC.CurrentlyIncreasingDefenseOrDR = true;

            RotatingMovement(NPC, Main.player[NPC.target], 0.3f, 8f);
                }

        void Despawn()
        {
            // Disable teleports
            if (TeleportTimer != 0f || TeleportX != 0f || TeleportY != 0f)
            {
                TeleportTimer = 0f;
                TeleportX = 0f;
                TeleportY = 0f;
                NPC.netUpdate = true;
            }

            NPC.velocity.Y += 0.1f;
            if (NPC.velocity.Y < 0f)
                NPC.velocity.Y *= 0.95f;
            NPC.velocity.X *= 0.95f;
            if (NPC.timeLeft > 50)
                NPC.timeLeft = 50;
                }
    }
}
