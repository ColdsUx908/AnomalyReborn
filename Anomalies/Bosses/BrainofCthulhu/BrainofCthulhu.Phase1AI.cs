namespace Anomalies.Bosses.BrainofCthulhu;

public sealed partial class BrainofCthulhu
{
    private void Phase1AI()
    {
        switch (CurrentBehavior)
        {
            case Behavior.Phase1Idle:
                Phase1Idle();
                break;
            case Behavior.TelekineticOnslaught:
                TelekineticOnslaught();
                break;
            case Behavior.Stunned:
                Stunned();
                break;
            case Behavior.CreeperSwipes:
                CreeperSwipes();
                break;
            case Behavior.CreeperSwings:
                CreeperSwings();
                break;
            case Behavior.CreeperOrbit:
                CreeperOrbit();
                break;
            case Behavior.CreeperSpiral:
                CreeperSpiral();
                break;
        }

        void Phase1Idle()
        {
            BrainOfCthulhuSystem.ScreenBlurStrength = 0f;
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (CreeperHPRatio <= DesperateOnslaughtCreeperHealthGate)
                {
                    Time = -1;
                    CurrentBehavior = Behavior.TelekineticOnslaught;
                    AttackSign = Main.rand.NextBool() ? -1 : 1;
                    NPC.netUpdate = true;
                }
                else if (Time > IdlePeriodDuration)
                {
                    Time = -1;
                    ResetAttackValues();
                    if (AvailableAttacks.Count == 0)
                    {
                        AvailableAttacks.Replace([Behavior.CreeperSwipes, Behavior.CreeperSwings, Behavior.CreeperOrbit, Behavior.CreeperSpiral]);
                        if (PreviousAttack != Behavior.Phase1Idle)
                            AvailableAttacks.Remove(PreviousAttack);
                    }

                    int pick = Main.rand.Next(AvailableAttacks.Count);
                    CurrentBehavior = AvailableAttacks[pick];
                    AvailableAttacks.RemoveAt(pick);
                    PreviousAttack = CurrentBehavior;
                    foreach (NPC creep in Main.npc.Where(n => n.active && n.type == NPCID.Creeper))
                        creep.GetCreeperBehavior().Time = -1;
                    SoundEngine.PlaySound(GrowlSound, NPC.Center);
                    NPC.netUpdate = true;
                }
            }

            /*
        if (CurrentLocalPhase > 0)
            CurrentLocalPhase--;
        else if (Time < IdlePeriodDuration - 120)
        {
            if (Time >= 30)
            {
                bool targetLeft = target.Center.X < NPC.Center.X;
                List<NPC> creepers = Main.npc.Where(n => n.active && n.type == NPCID.Creeper && n.GetCreeperBehavior().Time == -1 && n.GetCreeperBehavior().CreeperID % 2 == (targetLeft ? 0 : 1)).ToList();
                if (creepers.Count == 0)
                    creepers = Main.npc.Where(n => n.active && n.type == NPCID.Creeper && n.GetCreeperBehavior().Time == -1).ToList();

                if (creepers.Count > 0)
                {
                    int rand = Main.rand.Next(creepers.Count);

                    NPC creeper = creepers[rand];
                    creeper.GetCreeperBehavior().Time = 0;
                }
            }
            CurrentLocalPhase = (int)MathHelper.Lerp(CreeperChargeDelayMax, CreeperChargeDelayMin, 1 - CreeperAmountRatio);
        }
        */
            if (Time == 0)
            {
                AttackSign = Main.rand.NextBool() ? -1 : 1;
                NPC.netUpdate = true;
                foreach (NPC creeper in Main.ActiveNPCs)
                {
                    if (creeper.type != NPCID.Creeper)
                        continue;
                    creeper.netUpdate = true;
                }
            }

            float rotateDir = AttackSign;
            Vector2 fromTarget = NPC.DirectionFrom(Target.Center);
            Vector2 dir = fromTarget.RotatedBy(Math.Sin(Time / 60f) * rotateDir) * new Vector2(2, 1);
            float rayDist = BrainCombatUtils.PreciseDistanceToTileCollisionHit(Target.Center, dir.ToRotation(), 360, 1);
            Vector2 offset = dir * (rayDist - NPC.width);
            Vector2 goalPos = Target.Center + offset;
            float distSQ = NPC.DistanceSQ(goalPos);
            if (distSQ > 129600)
            {
                NPC.velocity = NPC.DirectionTo(goalPos) * (4 + (NPC.Distance(goalPos) - 360) / 64f);
            }
            else if (distSQ <= 2048)
            {
                NPC.velocity *= 0.9f;
            }
            else if (NPC.velocity.LengthSquared() < 16f)
            {
                NPC.velocity += NPC.DirectionTo(goalPos).SafeNormalize(Vector2.Zero) * 0.15f;
            }
            else
            {
                NPC.velocity = NPC.DirectionTo(goalPos).SafeNormalize(Vector2.Zero) * 6f;
            }
        }

        void TelekineticOnslaught()
        {
            if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
            {
                NPC.velocity = NPC.DirectionTo(Target.Center) * 4f;
                Time = -1;
            }
            else
            {
                float distSQ = NPC.DistanceSQ(Target.Center);
                if (distSQ > 230400) //480^2
                    NPC.velocity = NPC.DirectionTo(Target.Center) * (MathF.Sqrt(distSQ) - 480) / 128f;
                else
                    NPC.velocity *= 0.9f;
            }

            float wrappedCounter = Time % 90;
            if (Time <= 60)
            {
                if (Time == 0)
                {
                    SoundEngine.PlaySound(ShortRoarSound, NPC.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        foreach (NPC creeper in Main.ActiveNPCs)
                        {
                            if (creeper.type != NPCID.Creeper)
                                continue;
                            CreeperAI ai = creeper.GetCreeperBehavior();
                            int dirSign = ai.EvenID ? -1 : 1;
                            Vector2 dir = Vector2.UnitX.RotatedBy(Main.rand.NextFloat(-MathHelper.TwoPi / 3f, MathHelper.TwoPi / 3f)) * dirSign;
                            float dirY = dir.Y;
                            dir.Y -= dirY * 0.5f;
                            dir.X += dirY * 0.5f * Math.Sign(dir.X);
                            dir.SafeNormalize(Vector2.UnitX * dirSign);

                            ai.AttackAngle = dir.ToRotation();
                            float rayDist = BrainCombatUtils.PreciseDistanceToTileCollisionHit(NPC.Center, ai.AttackAngle, 800, 4);
                            ai.AttackPosition = NPC.Center + (dir * (rayDist - 64));
                            creeper.netUpdate = true;
                        }

                        NPC.netUpdate = true;
                    }
                }

                if (Time < 30)
                {
                    BrainOfCthulhuSystem.ScreenBlurStrength = 0.5f;
                    NPC.frameCounter += 1f;
                    for (int i = 0; i < 3; i++)
                    {
                        Point start = Target.Center.ToTileCoordinates() + new Point(Main.rand.Next(-64, 65), 48);
                        for (int j = 0; j < 96; j++)
                        {
                            Point current = start - new Point(0, j);
                            if (!Framing.GetTileSafely(current).IsTileSolid() && Framing.GetTileSafely(current - new Point(0, 1)).TileType == TileID.Crimstone)
                                Dust.NewDustAction(current.ToWorldCoordinates(8, -8), 16, 16, DustID.Crimstone, new Vector2(0, 3));
                        }
                    }

                    float d = Main.LocalPlayer.DistanceSQ(NPC.Center);
                    float distanceScaleFactor = 1;
                    if (d > 592900) //770^2
                        distanceScaleFactor = 1 / (1 + (((float)Math.Sqrt(d) - 770) / 32f));
                    Main.LocalPlayer.SetScreenshake(4 * BrainOfCthulhuSystem.ScreenBlurStrength * distanceScaleFactor);
                    if (Time % 15 == 0)
                    {
                        BossRoar pulse = new(NPC.Center, Color.Black, Main.rand.NextFloatDirection(), 0.1f, 3f, 30);
                        ParticleHandler.SpawnParticle(pulse);
                    }
                }
                else
                    BrainOfCthulhuSystem.ScreenBlurStrength = MathHelper.Lerp(0.5f, 0, BrainCombatUtils.CircOutEasing((Time - 30) / 30f, 1));
            }
            else
            {
                BrainOfCthulhuSystem.ScreenBlurStrength = 0f;
                if (wrappedCounter == 65)
                {
                    if (Main.netMode != NetmodeID.SinglePlayer)
                    {
                        SelectNewTarget();
                        NPC.netUpdate = true;
                    }

                    int checkCount = 8;
                    float wallDist = BrainCombatUtils.PreciseDistanceToTileCollisionHit(NPC.Center, AttackSign == -1 ? MathHelper.Pi : 0, 480 + NPC.width) - NPC.width;
                    Vector2[] starts = new Vector2[checkCount];
                    for (int i = 0; i < checkCount; i++)
                    {
                        float completion = (i + 1) / (float)(checkCount + 1);
                        starts[i] = NPC.Center + (Vector2.UnitX * ((wallDist * completion) + NPC.width) * AttackSign);
                    }

                    Vector2[] ends = new Vector2[checkCount];
                    List<Vector2> goodEnds = [];
                    List<Vector2> farEnds = [];
                    List<Vector2> closeEnds = [];
                    for (int i = 0; i < checkCount; i++)
                    {
                        float maxDist = 960;
                        float floorDist = BrainCombatUtils.PreciseDistanceToTileCollisionHit(NPC.Center, Vector2.UnitY.ToRotation(), maxDist);
                        ends[i] = starts[i] + (Vector2.UnitY * (floorDist + 48));
                        if (floorDist >= 600)
                            farEnds.Add(ends[i]);
                        else if (floorDist > 240)
                            goodEnds.Add(ends[i]);
                        else
                            closeEnds.Add(ends[i]);
                    }

                    Vector2 chosenEnd;
                    if (goodEnds.Count > 0)
                        chosenEnd = goodEnds[Main.rand.Next(goodEnds.Count)];
                    else if (closeEnds.Count > 0)
                        chosenEnd = closeEnds[Main.rand.Next(closeEnds.Count)];
                    else
                        chosenEnd = farEnds[Main.rand.Next(farEnds.Count)];
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectileAction(source: NPC.GetSource_FromThis(), position: chosenEnd, velocity: Vector2.Zero, type: ModContent.ProjectileType<TelekineticEnemyGrab>(), damage: 10, knockback: 0.5f);
                    AttackSign *= -1;
                }
            }
        }

        void Stunned()
        {
            NPC.velocity = NPC.velocity.ClampMagnitude(0f, 6f);
            if (Time == 0)
            {
                NPC.velocity = (NPC.Center - Target.Center).SafeNormalize(Vector2.UnitX) * 4f;
                SoundEngine.PlaySound(ShieldDownSound, NPC.Center);
            }

            if (NPC.velocity != Vector2.Zero)
            {
                if (Time >= StunDuration)
                    NPC.velocity *= 0.8f;
                else
                    NPC.velocity *= 0.93f;
            }

            if (Time < StunDuration)
                NPC.position.Y += (float)Math.Sin(Time / 8f) * 2 * (1 - MathHelper.Clamp((Time - (StunDuration - 30)) / 30f, 0f, 1f));
            if (Time <= StunDuration - 30)
            {
                if (AttackTime > 0)
                {
                    float kbCounter = 30 - AttackTime;
                    if (kbCounter < 10)
                    {
                        float lerp = BrainCombatUtils.SineOutEasing(kbCounter / 10f, 1);
                        NPC.rotation = AttackRotation.AngleLerp(-AttackRotation, lerp);
                    }
                    else
                    {
                        float lerp = BrainCombatUtils.SineInOutEasing((kbCounter - 10) / 20f, 1);
                        NPC.rotation = (-AttackRotation).AngleLerp(0, lerp);
                    }
                }
                else if (Math.Abs(NPC.oldVelocity.X) < Math.Abs(NPC.velocity.X) || Time <= 0)
                {
                    AttackRotation = NPC.rotation;
                    TeleportTime = 0;
                }
                else
                {
                    TeleportTime++;
                    NPC.rotation = MathHelper.Lerp(AttackRotation, MathHelper.Pi / 24f * NPC.oldVelocity.X, BrainCombatUtils.CircOutEasing(MathHelper.Clamp(TeleportTime / 30f, 0f, 1f), 1));
                }
            }

            if (Time < StunDuration)
            {
                if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                {
                    NPC.velocity = NPC.DirectionTo(Target.Center) * 4f;
                }
                else if (Collision.SolidCollision(NPC.position + NPC.velocity, NPC.width, NPC.height))
                {
                    if (NPC.velocity.X != NPC.oldVelocity.X)
                        NPC.velocity.X = -NPC.oldVelocity.X;
                    if (NPC.velocity.Y != NPC.oldVelocity.Y)
                        NPC.velocity.Y = -NPC.oldVelocity.Y;
                    //NPC.velocity *= 2f;
                    NPC.velocity = NPC.velocity.ClampMagnitude(0f, 8f);
                    AttackTime = 30;
                    AttackRotation = NPC.rotation;
                }

                if (AttackTime > 0)
                {
                    NPC.knockBackResist = 0f;
                    AttackTime--;
                    if (AttackTime == 0)
                    {
                        NPC.velocity = Vector2.Zero;
                        AttackRotation = 0;
                    }
                }
                else
                    NPC.knockBackResist = 1f;
            }

            BrainOfCthulhuSystem.ScreenBlurStrength = 0f;
            NPC.dontTakeDamage = false;
            NPC.damage = 0;
            if (Time <= 15)
            {
                float lerp = Time / 15f;
                ShieldOpacity = 1 - BrainCombatUtils.CircOutEasing(lerp, 1);
                ShieldScale = MathHelper.Lerp(1f, 1.5f, lerp);
            }

            if (Time > 15 && Time < StunDuration)
            {
                ShieldOpacity = 0f;
                ShieldScale = 1.5f;
            }

            if (OnSecondCreeperPhase && Time == StunDuration - 30)
            {
                CurrentBehavior = Behavior.Phase2TransitionClosed;
                Time = -1;
                TeleportTime = 0;
                return;
            }

            if (Time > StunDuration - 30)
            {
                NPC.rotation = NPC.rotation.AngleLerp(0f, BrainCombatUtils.SineInOutEasing((Time - (StunDuration - 30)) / 30f, 1));
                if (Time == StunDuration - 15)
                    SoundEngine.PlaySound(ShieldUpSound, NPC.Center);
            }

            if (Time >= StunDuration)
            {
                if (NPC.velocity.X is < 0.001f and < 0.001f)
                    NPC.velocity = Vector2.Zero;
                int creeperRate = 5;
                float wrappedCounter = (Time - StunDuration) % creeperRate;
                int spawnTime = CreeperCount / 2 * creeperRate;
                if (Time == StunDuration)
                {
                    CurrentLocalPhase = CreeperCount - 1;
                    SoundEngine.PlaySound(ShortRoarSound, NPC.Center);
                }

                NPC.knockBackResist = 0f;
                NPC.dontTakeDamage = true;
                NPC.rotation = 0f;
                float shieldAppearTime = 15f;
                float lerp = MathHelper.Clamp((Time - StunDuration) / shieldAppearTime, 0f, 1f);
                if (lerp >= 1)
                {
                    ShieldOpacity = 1f;
                    ShieldScale = 1f;
                }
                else
                {
                    ShieldOpacity = BrainCombatUtils.CircOutEasing(lerp, 1);
                    ShieldScale = MathHelper.Lerp(1.5f, 1f, BrainCombatUtils.SineOutEasing(lerp, 1));
                }

                if (CurrentLocalPhase == -1 && Time > StunDuration + spawnTime + 30)
                {
                    OnSecondCreeperPhase = true;
                    CurrentBehavior = Behavior.Phase1Idle;
                    Time = -1;
                    NPC.damage = NPC.defDamage;
                }
                else if (CurrentLocalPhase > -1 && wrappedCounter == 0)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 dir = Vector2.UnitY.RotatedBy((CurrentLocalPhase % 2 == 0 ? 1 : -1) * (MathHelper.Pi / 16f + ((MathHelper.Pi - MathHelper.Pi / 8f) * (CurrentLocalPhase / 2f / (CreeperCount / 2f)))));
                        Vector2 spawnPos = NPC.Center + (dir * 72f);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.NewNPCAction(source: NPC.GetSource_FromAI(), position: spawnPos, type: NPCID.Creeper, start: NPC.whoAmI, action: entity =>
                            {
                                entity.ai[0] = CurrentLocalPhase;
                                entity.ai[2] = -1;
                                entity.ai[3] = 1;
                                entity.velocity = dir * 24f;
                            });
                        }

                        for (int j = 0; j < 3; j++)
                        {
                            BloodParticle p = new(spawnPos, dir.RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 6f, MathHelper.Pi / 6f)) * Main.rand.NextFloat(8f, 12f), 32, 1f, Color.Red);
                            ParticleHandler.SpawnParticle(p);
                        }

                        BloodParticle2 p2 = new(spawnPos, dir * 10f, 16, 0.5f, Color.Red);
                        ParticleHandler.SpawnParticle(p2);
                        CurrentLocalPhase--;
                        if (CurrentLocalPhase <= -1)
                            return;
                    }

                    SoundEngine.PlaySound(SoundID.NPCHit9, NPC.Center);
                }
            }
        }

        void CreeperSwipes()
        {
            //Hand Size check to determine if 1 hand variant should be used
            if (Time == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int leftAmt = 0;
                int rightAmt = 0;
                foreach (NPC creeper in Main.ActiveNPCs)
                {
                    if (creeper.type != NPCID.Creeper)
                        continue;
                    if (creeper.GetCreeperBehavior().CreeperID % 2 == 0)
                        leftAmt++;
                    else
                        rightAmt++;
                }

                if (leftAmt > 5 && rightAmt > 5)
                {
                    AttackSign = Main.rand.NextBool() ? -1 : 1;
                    AttackFlag = false;
                }
                else
                    AttackFlag = true;
                NPC.damage = 0;
                NPC.netUpdate = true;
                AttackList.Clear();
                TargetsSet.Clear();
            }

            float wrappedCount = Time % (SwipeDuration + SwipeDelay);
            if (Time >= SwipesStartupDuration)
            {
                NPC.damage = NPC.defDamage;
                if (wrappedCount == 0)
                {
                    bool useEven = Main.rand.NextBool();
                    bool anyActivated = false;
                    foreach (NPC Npc in Main.ActiveNPCs)
                    {
                        if (Npc.type != NPCID.Creeper)
                            continue;
                        if (AttackFlag || Npc.GetCreeperBehavior().CreeperID % 2 == 0! ^ useEven)
                        {
                            AttackList.Add((byte)Npc.whoAmI);
                            //Npc.GetCreeperBehavior().Time = 0;
                            anyActivated = true;
                        }
                    }

                    if (!anyActivated)
                        foreach (NPC Npc in Main.ActiveNPCs)
                        {
                            if (Npc.type != NPCID.Creeper)
                                continue;
                            AttackList.Add((byte)Npc.whoAmI);
                        //Npc.GetCreeperBehavior().Time = 0;
                        }

                    if (Main.netMode != NetmodeID.SinglePlayer)
                        SelectNewTarget();
                    NPC.netUpdate = true;
                }
                else if (wrappedCount == SwipeDuration && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    AttackSign *= -1;
                    AttackList.Clear();
                    NPC.netUpdate = true;
                }

                if (wrappedCount > 1 && wrappedCount <= SwipeIchorDelay && Time % 3 == 0) // Telegraphs ichor rain w/ dripping particles
                {
                    Vector2 spawnPosition = NPC.Center;
                    spawnPosition.Y += Main.rand.NextFloat(38, 50);
                    spawnPosition.X += Main.rand.NextFloat(-56, 56);
                    BloodParticle blood = new(spawnPosition, Main.rand.NextVector2Unit() * Main.rand.NextFloat(1.5f, 2.5f), Main.rand.Next(30, 40), Main.rand.NextFloat(0.6f, 1f), Color.Gold);
                    ParticleHandler.SpawnParticle(blood);
                }

                if (wrappedCount < 60f) // Vibrates during telegraph
                {
                    Vector2 vibrationVector = Main.rand.NextVector2CircularEdge(1, 1) * MathHelper.Lerp(0f, 12f, BrainCombatUtils.CircInEasing(wrappedCount / 80f, 1));
                    BoCDrawOffset = vibrationVector;
                }
                else if (wrappedCount is> 60f and < 80f) // Droops down when starting to fire ichor shots
                {
                    float progress = (wrappedCount - 60f) / 20f;
                    BoCDrawOffset = new Vector2(0, MathHelper.Lerp(10, 0, 1f - (float)Math.Pow(1f - progress, 3f)));
                }

                if (Main.netMode != NetmodeID.MultiplayerClient)
                    if (wrappedCount > SwipeIchorDelay && wrappedCount <= SwipeDuration && Time % 2 == 0)
                        Projectile.NewProjectileAction(source: NPC.GetSource_FromThis(), position: NPC.Center + new Vector2(Main.rand.NextFloat(-72, 72), 56), velocity: Vector2.UnitY.RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 4f, MathHelper.Pi / 4f)) * 4f, type: ProjectileID.GoldenShowerHostile, damage: IchorShotDamage, knockback: 0.5f);
                if (Time >= SwipesStartupDuration + ((SwipeDuration + SwipeDelay) * SwipeAmount) && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Time = 0;
                    CurrentBehavior = Behavior.Phase1Idle;
                    NPC.netUpdate = true;
                    NPC.damage = NPC.defDamage;
                    AttackList.Clear();
                }
            }
            else
                NPC.damage = 0;
            Vector2 goalPos = Target.Center + (Vector2.UnitY * -270);
            float distSQ = NPC.DistanceSQ(goalPos);
            if ((Time > SwipesStartupDuration && wrappedCount > 30 && wrappedCount <= SwipeDuration) || NPC.DistanceSQ(goalPos) <= 2048)
                NPC.velocity *= 0.9f;
            else if (distSQ > 14400)
                NPC.velocity = NPC.DirectionTo(goalPos) * (8 + (NPC.Distance(goalPos) - 120) / 16f);
            else
                NPC.velocity = NPC.DirectionTo(goalPos).SafeNormalize(Vector2.UnitX * -Target.direction) * (8f * distSQ / 14400f);
        }

        void CreeperSwings()
        {
            NPC.damage = 0;
            Vector2 fromTarget;
            if (Main.netMode == NetmodeID.SinglePlayer)
                fromTarget = NPC.Center - Target.Center;
            else
            {
                Vector2 averagePlayerPos = Vector2.Zero;
                List<int> targets = GetAllValidTargets(NPC.Center);
                foreach (int p in targets)
                    averagePlayerPos += Main.player[p].Center;
                averagePlayerPos = targets.Count > 0 ? averagePlayerPos / targets.Count : Target.Center;
                fromTarget = NPC.Center - averagePlayerPos;
            }

            Vector2 goalDir;
            if (Math.Abs(fromTarget.X) > Math.Abs(fromTarget.Y))
                goalDir = Vector2.UnitX * Math.Sign(fromTarget.X);
            else
                goalDir = Vector2.UnitY * Math.Sign(fromTarget.Y);
            Vector2 goalPos = Target.Center + (goalDir * 360) - (Vector2.UnitY * 32);
            if (NPC.DistanceSQ(goalPos) <= 2048)
                NPC.velocity *= 0.9f;
            else if (NPC.velocity.LengthSquared() <= 56.25f) //7.5^2
                NPC.velocity += NPC.DirectionTo(goalPos).SafeNormalize(Vector2.UnitX * -Target.direction) * 0.5f;
            else
                NPC.velocity = NPC.DirectionTo(goalPos).SafeNormalize(Vector2.UnitX * -Target.direction) * 8f;
            int delay = OnSecondCreeperPhase ? StrongSwipeDelay : LightSwipeDelay;
            if (Time == 0)
                AttackList.Clear();
            if (Time > delay)
            {
                foreach (NPC creeper in Main.npc.Where(n => n.active && n.type == NPCID.Creeper && n.GetCreeperBehavior().Time == -1 && !AttackList.Contains((byte)n.whoAmI)))
                    creeper.position += NPC.velocity;
                int crushCount;
                int attackDelay;
                if (!OnSecondCreeperPhase)
                {
                    crushCount = LightSwipeAmount;
                    attackDelay = LightSwipeDuration;
                }
                else
                {
                    crushCount = StrongSwipeAmount;
                    attackDelay = StrongSwipeDuration;
                }

                int attackDur = delay + ((attackDelay + 1) * crushCount);
                if (Time < attackDur && Time % attackDelay == 0)
                {
                    List<NPC> creepers = [..Main.npc.Where(n => n.active && n.type == NPCID.Creeper && n.GetCreeperBehavior().Time == -1 && !AttackList.Contains((byte)n.whoAmI))];
                    if (creepers.Count > 1 && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float rotation;
                        if (!OnSecondCreeperPhase)
                        {
                            rotation = Target.velocity.ToRotation();
                            if (Target.velocity == Vector2.Zero)
                                rotation = Target.direction == 1 ? 0 : MathHelper.Pi;
                            rotation += Main.rand.NextFloat(-MathHelper.PiOver4 / 2f, MathHelper.PiOver4 / 2f);
                        }
                        else
                            rotation = Main.rand.NextFloat(0, MathHelper.TwoPi);
                        int rand = Main.rand.Next(creepers.Count);
                        NPC first = creepers[rand];
                        first.netUpdate = true;
                        CreeperAI creeper1 = first.GetCreeperBehavior();
                        AttackList.Add((byte)first.whoAmI);
                        //creeper1.Time = 0;
                        creeper1.AttackAngle = rotation;
                        creepers.RemoveAt(rand);
                        NPC second = creepers[Main.rand.Next(creepers.Count)];
                        second.netUpdate = true;
                        CreeperAI creeper2 = second.GetCreeperBehavior();
                        AttackList.Add((byte)second.whoAmI);
                        //creeper2.Time = 0;
                        creeper2.AttackAngle = rotation + MathHelper.Pi;
                        creeper1.PartnerIndex = second.whoAmI;
                        creeper2.PartnerIndex = first.whoAmI;
                        if (Main.netMode != NetmodeID.SinglePlayer)
                            SelectNewTarget();
                        NPC.netUpdate = true;
                    }
                }

                if (Time > attackDur + attackDelay)
                {
                    Time = 0;
                    NPC.damage = NPC.defDamage;
                    CurrentBehavior = Behavior.Phase1Idle;
                    AttackList.Clear();
                }
            }
        }

        void CreeperOrbit()
        {
            Vector2 fromTarget = (NPC.Center - Target.Center).SafeNormalize(Vector2.UnitX);
            Vector2 goalPos = Target.Center + (fromTarget * 440) - (Vector2.UnitY * 32);
            float distSQ = NPC.DistanceSQ(goalPos);
            if (distSQ > 14400)
                NPC.velocity = NPC.DirectionTo(goalPos) * (4 + (NPC.Distance(goalPos) - 120) / 16f);
            else
                NPC.velocity = NPC.DirectionTo(goalPos).SafeNormalize(Vector2.Zero) * (2 + (2f * (distSQ / 14400)));
            if (Time == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                AttackSign = Main.rand.NextBool() ? -1 : 1;
                AttackPosition = Target.Center;
                AttackList.Clear();
                NPC.netUpdate = true;
                NPC.damage = 0;
                if (Main.netMode != NetmodeID.SinglePlayer)
                {
                    List<int> extraTargets = GetAllValidTargets(NPC.Center);
                    extraTargets.Remove(NPC.target);
                    int targetCount = extraTargets.Count;
                    int creepersPerExtraPlayer = AnomalySharedData.Anomaly ? 3 : 2;
                    int creepersDesired = targetCount * creepersPerExtraPlayer;
                    int creepersAlive = NPC.CountNPCS(NPCID.Creeper);
                    int creepersToSpare = creepersAlive - 4;
                    for (int i = 0; i < creepersPerExtraPlayer; i++)
                    {
                        if (creepersToSpare >= creepersDesired || creepersPerExtraPlayer == 1)
                            break;
                        if (creepersToSpare < creepersDesired && creepersPerExtraPlayer > 1)
                        {
                            creepersPerExtraPlayer--;
                            creepersDesired = targetCount * creepersPerExtraPlayer;
                        }
                    }

                    int creepersSparedForTarget = 0;
                    foreach (NPC n in Main.ActiveNPCs)
                    {
                        if (n.type != NPCID.Creeper)
                            continue;
                        n.TryGetCreeperBehavior(out CreeperAI creeper);
                        if (creepersDesired > 0 && creepersToSpare > 0)
                        {
                            creeper.CachedValue2 = extraTargets[0];
                            creepersToSpare--;
                            creepersDesired--;
                            creeper.Time -= 30 * creepersSparedForTarget;
                            if (++creepersSparedForTarget >= creepersPerExtraPlayer)
                            {
                                extraTargets.RemoveAt(0);
                                creepersSparedForTarget = 0;
                            }

                            CurrentLocalPhase++;
                        }
                        else
                            creeper.CachedValue2 = -1;
                    }
                }
                else
                {
                    foreach (NPC n in Main.ActiveNPCs)
                    {
                        if (n.type != NPCID.Creeper)
                            continue;
                        n.TryGetCreeperBehavior(out CreeperAI creeper);
                        creeper.CachedValue2 = -1;
                    }
                }
            }

            if (Time < OrbitSetupDuration)
            {
                AttackPosition = Target.Center;
            }
            else
            {
                float prox = Target.DistanceSQ(AttackPosition);
                if (prox > 65536) //256^2
                    AttackPosition += Target.DirectionFrom(AttackPosition) * (((float)Math.Sqrt(prox) - 256) / 16f);
            }

            if (Time >= OrbitDuration + 30)
            {
                Time = -1;
                CurrentBehavior = Behavior.Phase1Idle;
                AttackList.Clear();
                NPC.damage = NPC.defDamage;
                foreach (NPC creep in Main.ActiveNPCs)
                {
                    if (creep.type == NPCID.Creeper)
                        creep.GetCreeperBehavior().Time = -1;
                }
            }
            else if (Time >= OrbitAttackInterval && Time < OrbitDuration && Time % OrbitAttackInterval == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                List<NPC> mainOrbitMembers = [..Main.npc.Where(n => n.active && n.type == NPCID.Creeper && n.TryGetCreeperBehavior(out CreeperAI ai) && ai.CachedValue2 == -1)];
                if (mainOrbitMembers.Count > 0)
                {
                    int rand = Main.rand.Next(mainOrbitMembers.Count);
                    for (int i = 0; i < OrbitAttackParticipantCount; i++)
                    {
                        if (rand >= mainOrbitMembers.Count)
                            rand -= mainOrbitMembers.Count;
                        NPC creeper = mainOrbitMembers[rand];
                        AttackList.Add((byte)creeper.whoAmI);
                        //creeper.GetCreeperBehavior().Time = 0;
                        rand += (int)Math.Round(mainOrbitMembers.Count / (float)OrbitAttackParticipantCount);
                    }

                    NPC.netUpdate = true;
                }
            }
        }

        void CreeperSpiral()
        {
            if (Time == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                AttackSign = Main.rand.NextBool() ? -1 : 1;
                AttackRotation = 0;
                NPC.netUpdate = true;
            }

            float startTimePerRev = MathHelper.Lerp(StartingTimePerRevolutionMax, StartingTimePerRevolutionMin, 1 - CreeperAmountRatio);
            float endTimePerRev = MathHelper.Lerp(EndingTimePerRevolutionMax, EndingTimePerRevolutionMin, 1 - CreeperAmountRatio);
            float spinSpeedCompletion = MathHelper.Clamp((Time - SpeedUpDelayTime) / (SpiralDuration - SpeedUpDelayTime - SpeedUpExtensionTime), 0f, 1f);
            float timePerRev = MathHelper.Lerp(startTimePerRev, endTimePerRev, spinSpeedCompletion);
            if (Time > SpiralDuration - 30)
                timePerRev *= MathHelper.Lerp(1f, 10f, BrainCombatUtils.CircOutEasing(MathHelper.Clamp((Time - (SpiralDuration - 30)) / 30f, 0f, 1f), 1));
            else if (Time < SpiralSetupTime)
                timePerRev *= MathHelper.Lerp(1f, 10f, BrainCombatUtils.CircInEasing(MathHelper.Clamp(Time / SpiralSetupTime, 0f, 1f), 1));
            float rotToAdd = MathHelper.TwoPi / timePerRev * AttackSign;
            if (OnSecondCreeperPhase)
            {
                float attackComplationRatio = Time / SpiralDuration;
                float lerp = Utils.GetLerpValue(TurnAroundRatio - (TurnAroundDurationRatio / 2f), TurnAroundRatio + (TurnAroundDurationRatio / 2f), attackComplationRatio, true);
                rotToAdd *= MathHelper.Lerp(1, -1, lerp);
            }

            AttackRotation += rotToAdd;
            if (NPC.DistanceSQ(Target.Center) > 57600)
                NPC.velocity = NPC.DirectionTo(Target.Center) * (NPC.Distance(Target.Center) - 240) / 32f;
            else
                NPC.velocity *= 0.9f;
            if (Time > SpiralDuration)
            {
                Time = -1;
                CurrentBehavior = Behavior.Phase1Idle;
                foreach (NPC creep in Main.ActiveNPCs)
                {
                    if (creep.type == NPCID.Creeper)
                        creep.GetCreeperBehavior().Time = -1;
                }
            }
        }
    }
}
