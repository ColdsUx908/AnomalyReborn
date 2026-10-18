using Anomalies.DataStructures;

namespace Anomalies.Bosses.BrainofCthulhu;

public sealed partial class BrainofCthulhu
{
    public override bool PreAI()
    {
        using BossNetUpdateScope netUpdate = new(NPC);
        if (CurrentPhase == Phase.Initialize)
            InitializeEncounter();
        // whoAmI variable
        NPC.crimsonBoss = NPC.whoAmI;
        bool phase2 = CurrentPhase is Phase.Transition or Phase.Phase2 or Phase.Death;
        //Takes more damage in Phase 1 to account for invulnerability phases
        if (phase2)
        {
            NPC.knockBackResist = 0f;
            NPC.defense = NPC.defDefense;
            NPC.chaseable = CurrentBehavior is not Behavior.IllusionDash and not Behavior.IllusionTrick;
        }
        else
            NPC.defense = (int)(NPC.defDefense * Phase1DefenseMultiplier);
        NPC.extraValue = 0;
        if (NPC.target < 0 || NPC.target >= Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
        {
            // Ignore tank players, target low HP players, Brain is smart
            BrainTargetingParameters options = BrainTargetingParameters.BossDefaults;
            options.aggroRatio = -1f;
            options.finishThemOff = true;
            options.maxSearchRange = DespawnRange;
            BrainCombatUtils.BrainTargeting(NPC, options);
        }

        // Despawn check
        if (!BrainCalamityBridge.BossRushActive && CurrentBehavior != Behavior.DeathAnimation)
        {
            bool despawn = Target.dead || !Target.active;
            if (despawn)
            {
                List<int> v = GetAllValidTargets(NPC.Center);
                if (v.Count > 0)
                {
                    despawn = false;
                    NPC.target = v[0];
                }
            }

            // Despawn
            if (despawn)
            {
                if (DespawnTime < 90)
                    DespawnTime += 1f;
                if (DespawnTime == 90)
                    NPC.velocity.Y += 0.1f;
            }
            else if (DespawnTime > 0f)
                DespawnTime -= 1f;
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (Target.DistanceSQ(NPC.Center) > DespawnRangeSQ)
                {
                    NPC.active = false;
                    NPC.life = 0;
                    if (Main.dedServ)
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, NPC.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                }
            }

            if (DespawnTime > 60)
                return false;
        }

        if (CurrentBehavior == Behavior.Stunned)
            NPC.HitSound = StunHitSound;
        else
            NPC.HitSound = SoundID.NPCHit9;
        if (!phase2 && CreeperHPRatio == 0f && CurrentBehavior != Behavior.Stunned && CurrentBehavior >= Behavior.Phase1Idle)
        {
            CurrentBehavior = Behavior.Stunned;
            Time = 0;
            ResetAttackValues();
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.type != ModContent.ProjectileType<TelekineticEnemyGrab>())
                    continue;
                p.ai[1] = 0;
            }
        }

        if (CurrentBehavior == Behavior.Stunned && (NPC.life / (float)NPC.lifeMax) < Phase2HealthGate)
        {
            CurrentBehavior = Behavior.Phase2TransitionClosed;
            Time = 0;
            TeleportTime = 0;
        }

        switch (CurrentPhase)
        {
            case Phase.Introduction:
                SpawnAnimation();
                break;
            case Phase.Phase1:
            case Phase.Stunned:
                Phase1AI();
                break;
            case Phase.Transition:
                PhaseChangeAI();
                break;
            case Phase.Phase2:
                Phase2AI();
                break;
            case Phase.Death:
                DeathAnimation();
                break;
        }

        NPC.oldVelocity = NPC.velocity;
        Time++;
        if (CurrentBehavior != Behavior.DeathAnimation && NPC.lifeRegen < 0 && Math.Abs(NPC.lifeRegen) >= NPC.life)
            TriggerDeathAnimation();
        return false;
        void InitializeEncounter()
        {
            if (NPC.target < 0 || NPC.target >= Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                // Ignore tank players, target low HP players, Brain is smart
                BrainTargetingParameters options = BrainTargetingParameters.BossDefaults;
                options.aggroRatio = -1f;
                options.finishThemOff = true;
                BrainCombatUtils.BrainTargeting(NPC, options);
            }

            Player target = Main.player[NPC.target];
            NPC.Center = target.Center + Vector2.UnitY * 900;
            NPC.netUpdate = true;
            NPC.dontTakeDamage = true;
            CurrentBehavior = Behavior.SurfaceSpawnAnimation;
            PreviousAttack = Behavior.Phase1Idle;
            SpawnDelay = (SummonedViaItem || BrainCalamityBridge.BossRushActive) ? 2 : 60;
            if (SummonedViaItem || BrainCalamityBridge.BossRushActive)
                SpawnTime = -1;
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int brainOfCthuluCreepersCount = CreeperCount;
                for (int i = 0; i < brainOfCthuluCreepersCount; i++)
                    NPC.NewNPCAction(source: NPC.GetSource_FromAI(), position: new Vector2((int)NPC.Center.X, (int)NPC.Center.Y), type: NPCID.Creeper, start: NPC.whoAmI, action: entity =>
                    {
                        entity.ai[0] = i;
                        entity.ai[2] = -1;
                    });
            }

            NPC.netUpdate = true;
        }

        void SpawnAnimation()
        {
            NPC.damage = 0;
            foreach (Player p in Main.ActivePlayers)
                BrainCalamityBridge.ResetAdrenaline(p);
            if (SpawnTime != 0) //BoC should begin appearing
            {
                float d = Main.LocalPlayer.DistanceSQ(NPC.Center);
                float distanceScaleFactor = 1;
                if (d > 592900) //770^2
                    distanceScaleFactor = 1 / (1 + (((float)Math.Sqrt(d) - 770) / 32f));
                // Time spent in the spawn animation is shortened when summoned via item
                float spawnAnimTimeReduction = (SummonedViaItem || BrainCalamityBridge.BossRushActive) ? 120 : 0;
                float spawnCounter = Time - Math.Abs(SpawnTime) + spawnAnimTimeReduction;
                if (spawnCounter < 180)
                {
                    float shakeIntensity = BrainCombatUtils.CircOutEasing(spawnCounter / 180f, 1) * 3f * distanceScaleFactor;
                    Main.LocalPlayer.SetScreenshake(shakeIntensity);
                    for (int i = 0; i < shakeIntensity; i++)
                    {
                        Point start = Target.Center.ToTileCoordinates() + new Point(Main.rand.Next(-64, 65), 48);
                        for (int j = 0; j < 96; j++)
                        {
                            Point current = start - new Point(0, j);
                            if (!Framing.GetTileSafely(current).IsTileSolid() && Framing.GetTileSafely(current - new Point(0, 1)).TileType == TileID.Crimstone)
                                Dust.NewDustAction(current.ToWorldCoordinates(8, -8), 16, 16, DustID.Crimstone, new Vector2(0, 3));
                        }
                    }
                }

                if (spawnCounter == 180)
                    NPC.velocity = Vector2.UnitY * -50;
                else if (spawnCounter > 180)
                {
                    NPC.velocity *= 0.955f;
                    if (spawnCounter == 240)
                    {
                        SoundEngine.PlaySound(RoarSound, NPC.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            TOLocalizationUtils.ChatLocalizedTextFormat("Announcement.HasAwoken", new Color(175, 75, 255), NPC.TypeName);
                    }

                    if (spawnCounter is> 240 and < 390)
                    {
                        BrainOfCthulhuSystem.ScreenBlurStrength = 0.5f; // 0.5f;
                        if (spawnCounter < 250)
                            BrainOfCthulhuSystem.ScreenBlurStrength = MathHelper.Lerp(0, 0.5f, (spawnCounter - 240) / 10f);
                        NPC.frameCounter += 1f;
                        BoCDrawOffset = Main.rand.NextVector2Circular(4, 4);
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

                        Main.LocalPlayer.SetScreenshake(6 * BrainOfCthulhuSystem.ScreenBlurStrength * distanceScaleFactor);
                        if (spawnCounter % 15 == 0)
                        {
                            BossRoar pulse = new(NPC.Center, Color.Black, Main.rand.NextFloatDirection(), 0.1f, 3f, 30);
                            ParticleHandler.SpawnParticle(pulse);
                        }
                    }
                    else if (spawnCounter is >= 390 and <= 420)
                    {
                        BrainOfCthulhuSystem.ScreenBlurStrength = MathHelper.Lerp(0.5f, 0f, (spawnCounter - 390) / 30f);
                        BoCDrawOffset *= 0.75f;
                    }
                    else if (spawnCounter > 420)
                    {
                        BrainOfCthulhuSystem.ScreenBlurStrength = 0f;
                        BoCDrawOffset = Vector2.Zero;
                        CurrentBehavior = Behavior.Phase1Idle;
                        NPC.damage = NPC.defDamage;
                        ResetAttackValues();
                        Time = -1;
                        SpawnTime = -1;
                        Main.musicFade[Main.curMusic] = 1f;
                        return;
                    }
                }
            }

            if (CurrentLocalPhase < CreeperCount)
            {
                if (SpawnTime == 0)
                {
                    NPC.Center = Target.Center + Vector2.UnitY * 900;
                    NPC.netUpdate = true;
                }

                if (SpawnDelay == 1 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    bool targetLeft = CurrentLocalPhase % 2 == 0;
                    List<NPC> creepers = [..Main.npc.Where(n => n.active && n.type == NPCID.Creeper && n.GetCreeperBehavior().Time == -1 && n.GetCreeperBehavior().CreeperID % 2 == (targetLeft ? 0 : 1))];
                    if (creepers.Count > 0)
                    {
                        AttackTime = creepers[Main.rand.Next(creepers.Count)].whoAmI;
                    }
                    else
                    {
                        creepers = [..Main.npc.Where(n => n.active && n.type == NPCID.Creeper && n.GetCreeperBehavior().Time == -1)];
                        AttackTime = creepers.Count == 0 ? -1 : creepers[Main.rand.Next(creepers.Count)].whoAmI;
                    }

                    NPC.netUpdate = true;
                }

                if (SpawnDelay <= 0)
                {
                    if (AttackTime != -1)
                    {
                        NPC creeper = Main.npc[(int)AttackTime];
                        creeper.GetCreeperBehavior().Time = 1;
                        creeper.netUpdate = true;
                        CurrentLocalPhase++;
                        if (SummonedViaItem || BrainCalamityBridge.BossRushActive)
                            SpawnDelay = 1;
                        else
                            SpawnDelay = CurrentLocalPhase switch
                            {
                                1 => 90,
                                2 or 3 or 4 => 24,
                                5 => 60,
                                6 or 7 or 8 => 24,
                                9 => 60,
                                _ => 4,
                            };
                    }
                    else
                        SpawnTime = Time;
                }
                else
                    SpawnDelay--;
            }
            else if (SpawnTime == 0)
                SpawnTime = Time;
        }

        void DeathAnimation()
        {
            if (Time == 0)
                NPC.velocity = NPC.DirectionFrom(Target.Center) * 6f;
            else
                NPC.velocity *= 0.95f;
            NPC.damage = 0;
            NPC.rotation = MathHelper.Pi / 24f * NPC.oldVelocity.X;
            TeleportTime *= 0.6f;
            if (TeleportTime < 0.005f)
                TeleportTime = 0;
            BoCDrawOffset *= 0.6f;
            NPC.Opacity = BringOpacityTo(NPC.Opacity, 1, 0.1f);
            (float angle, Vector2 offset, int time)[] bloodGushingData = [(MathHelper.Pi / 6f, new(18, 12), 60), (MathHelper.Pi, new(-30, -10), 150), (-MathHelper.Pi / 4f, new(20, -40), 210), (MathHelper.Pi + MathHelper.Pi / 4f, new(-20, -40), 240), (MathHelper.Pi / 1.75f, new(-5, 22), 260)];
            float EndTime = 270f;
            for (int i = 0; i < bloodGushingData.Length; i++)
            {
                if (Time >= bloodGushingData[i].time)
                {
                    if (Time == bloodGushingData[i].time)
                    {
                        if (i == 3)
                            SoundEngine.PlaySound(DeathRoarSound, NPC.Center);
                        Vector2 bloodDir = bloodGushingData[i].angle.ToRotationVector2();
                        BloodParticle2 p2 = new(NPC.Center + bloodGushingData[i].offset.RotatedBy(NPC.rotation), bloodDir * 7.5f, 16, 0.5f, Color.Red);
                        ParticleHandler.SpawnParticle(p2);
                        NPC.velocity = bloodDir * -4f;
                        SoundStyle bleed = BloodShotSound with
                        {
                            Pitch = i / (float)(bloodGushingData.Length - 1)
                        };
                        SoundEngine.PlaySound(bleed, NPC.Center);
                        Main.LocalPlayer.SetScreenshake(1f);
                    }

                    for (int j = 0; j < 2; j++)
                    {
                        BloodParticle p = new(NPC.Center + bloodGushingData[i].offset.RotatedBy(NPC.rotation), (bloodGushingData[i].angle + Main.rand.NextFloat(-MathHelper.Pi / 10f, MathHelper.Pi / 10f)).ToRotationVector2() * Main.rand.NextFloat(5f, 10f), 32, 1f, Color.Red);
                        ParticleHandler.SpawnParticle(p);
                    }
                }
            }

            if (Time >= EndTime)
            {
                SoundEngine.PlaySound(ExplosionSound, NPC.Center);
                SoundEngine.PlaySound(BloodShotSound, NPC.Center);
                Main.LocalPlayer.SetScreenshake(2f);
                int pCount = 10;
                for (int i = 0; i < pCount; i++)
                {
                    float initalSpeed = 24f;
                    Vector2 pVelo = Vector2.UnitX.RotatedBy(MathHelper.TwoPi / pCount * i) * initalSpeed;
                    for (int j = 0; j < 2; j++)
                    {
                        BloodParticle p = new(NPC.Center, pVelo.RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 6f, MathHelper.Pi / 6f)) * Main.rand.NextFloat(0.5f, 1f), 32, 1f, Color.Red);
                        ParticleHandler.SpawnParticle(p);
                    }

                    BloodParticle2 p2 = new(NPC.Center, pVelo * 0.75f, 16, 0.5f, Color.Red);
                    ParticleHandler.SpawnParticle(p2);
                }

                NPC.dontTakeDamage = false;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NPC.StrikeInstantKill();
            }

            float animationCompletion = Time / EndTime;
            NPC.frameCounter += 2 * animationCompletion;
            BoCDrawOffset = Main.rand.NextVector2Circular(4, 4) * animationCompletion;
            if (Main.rand.NextFloat(0.5f, 1f) < animationCompletion)
            {
                Vector2 edgeBloodDir = Main.rand.NextVector2CircularEdge(1, 1);
                BloodParticle b = new(NPC.Center + (edgeBloodDir * NPC.Size * 0.75f), edgeBloodDir.RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 10f, MathHelper.Pi / 10f)) * Main.rand.NextFloat(2f, 4f), 16, 0.75f, Color.Red);
                ParticleHandler.SpawnParticle(b);
            }
        }
    }

    private void ResetAttackValues()
    {
        IsNegative = false;
        AttackRotation = 0;
        AttackTime = 0;
        AttackFlag = false;
        AttackPosition = Vector2.Zero;
        CurrentLocalPhase = 0;
    }

    private void SelectNewTarget()
    {
        BrainTargetingParameters options = BrainTargetingParameters.BossDefaults;
        options.aggroRatio = -1f;
        options.finishThemOff = true;
        options.maxSearchRange = DespawnRange;
        options.targetType = BrainTargetType.ForceSwitch;
        List<int> available = GetAllValidTargets(NPC.Center);
        if (available.Any(p => !TargetsSet.Contains(p)))
            options.excludedPlayers = [..TargetsSet];
        else
        {
            TargetsSet.Clear();
            options.excludedPlayers = [NPC.target];
        }

        NPC.BrainTargeting(options);
        TargetsSet.Add(NPC.target);
    }
}
