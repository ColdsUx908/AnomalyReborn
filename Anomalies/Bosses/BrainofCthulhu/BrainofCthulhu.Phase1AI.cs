namespace Anomalies.Bosses.BrainofCthulhu;

public sealed partial class BrainofCthulhu
{
    private void Phase1AI()
    {
        // Reserved for future attack enhancements without spawning a second wave.
        bool enableEnhancedCreeperAttacks = false;
        OnSecondCreeperPhase = enableEnhancedCreeperAttacks;
        switch (CurrentBehavior)
        {
            case Behavior.Phase1Idle:
                Phase1Idle();
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
                if (Time > IdlePeriodDuration)
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
                foreach (NPC creeper in NPC.ActiveNPCs)
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

        void CreeperSwipes()
        {
            //Hand Size check to determine if 1 hand variant should be used
            if (Time == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int leftAmt = 0;
                int rightAmt = 0;
                foreach (NPC creeper in NPC.ActiveNPCs)
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
                    foreach (NPC Npc in NPC.ActiveNPCs)
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
                        foreach (NPC Npc in NPC.ActiveNPCs)
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
                else if (wrappedCount is > 60f and < 80f) // Droops down when starting to fire ichor shots
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
                    List<NPC> creepers = [.. Main.npc.Where(n => n.active && n.type == NPCID.Creeper && n.GetCreeperBehavior().Time == -1 && !AttackList.Contains((byte)n.whoAmI))];
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
                    foreach (NPC n in NPC.ActiveNPCs)
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
                    foreach (NPC n in NPC.ActiveNPCs)
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
                foreach (NPC creep in NPC.ActiveNPCs)
                {
                    if (creep.type == NPCID.Creeper)
                        creep.GetCreeperBehavior().Time = -1;
                }
            }
            else if (Time >= OrbitAttackInterval && Time < OrbitDuration && Time % OrbitAttackInterval == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                List<NPC> mainOrbitMembers = [.. Main.npc.Where(n => n.active && n.type == NPCID.Creeper && n.TryGetCreeperBehavior(out CreeperAI ai) && ai.CachedValue2 == -1)];
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
                foreach (NPC creep in NPC.ActiveNPCs)
                {
                    if (creep.type == NPCID.Creeper)
                        creep.GetCreeperBehavior().Time = -1;
                }
            }
        }
    }
}
