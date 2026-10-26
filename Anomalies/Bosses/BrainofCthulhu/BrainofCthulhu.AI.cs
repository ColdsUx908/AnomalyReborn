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
            NPCTargetingParameters options = NPCTargetingParameters.BossDefaults;
            options.AggroRatio = -1f;
            options.FinishThemOff = true;
            options.MaxSearchRange = DespawnRange;
            TOCombatUtils.TargetWithOptions(NPC, options);
        }

        // Despawn check
        if (!BridgeUtils.CalamityBossRushActive && CurrentBehavior != Behavior.DeathAnimation)
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

        // Protect the creeper phase, phase transition, and death sequence.
        NPC.dontTakeDamage = CurrentPhase is Phase.Phase1 or Phase.Transition or Phase.Death;
        NPC.HitSound = SoundID.NPCHit9;
        if (CurrentPhase == Phase.Phase1 && !Main.npc.Any(n => n.active && n.type == NPCID.Creeper && !n.GetCreeperBehavior().IsPhantom))
        {
            CurrentBehavior = Behavior.Phase2TransitionClosed;
            Time = 0;
            TeleportTime = 0;
            ResetAttackValues();
            NPC.dontTakeDamage = true;
            ShieldOpacity = 0f;
            foreach (NPC creeper in NPC.ActiveNPCs)
            {
                if (creeper.type != NPCID.Creeper) continue;
                creeper.damage = 0;
                creeper.velocity = Vector2.Zero;
                creeper.netUpdate = true;
            }
        }

        switch (CurrentPhase)
        {
            case Phase.Introduction:
                SpawnAnimation();
                break;
            case Phase.Phase1:
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
                NPCTargetingParameters options = NPCTargetingParameters.BossDefaults;
                options.AggroRatio = -1f;
                options.FinishThemOff = true;
                TOCombatUtils.TargetWithOptions(NPC, options);
            }

            NPC.velocity = Vector2.Zero;
            NPC.netUpdate = true;
            NPC.dontTakeDamage = false;
            CurrentBehavior = Behavior.SurfaceSpawnAnimation;
            BoCAfterImages = [];
            TeleportDuration = 60;
            PreviousAttack = Behavior.Phase1Idle;
            SpawnDelay = (SummonedViaItem || BridgeUtils.CalamityBossRushActive) ? 2 : 60;
            if (SummonedViaItem || BridgeUtils.CalamityBossRushActive)
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
            NPC.dontTakeDamage = false;
            NPC.velocity = Vector2.Zero;
            foreach (Player p in Main.ActivePlayers)
                BridgeUtils.ResetCalamityAdrenaline(p);
            if (Time < 60)
            {
                // Fade out at the spawn point, relocate while invisible, then fade in.
                TeleportDuration = 60;
                TeleportTime = Time <= 30 ? Time : 60 - Time;
                NPC.Opacity = 1f - TeleportTime / 30f;
                NPC.dontTakeDamage = true;
                if (Time == 30 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.Center = Target.Center - Vector2.UnitY * 300;
                    NPC.netOffset = Vector2.Zero;
                    NPC.netUpdate = true;
                    foreach (NPC creeper in NPC.ActiveNPCs)
                    {
                        if (creeper.type != NPCID.Creeper)
                            continue;
                        creeper.Center = NPC.Center;
                        creeper.velocity = Vector2.Zero;
                        creeper.netOffset = Vector2.Zero;
                        creeper.netUpdate = true;
                    }
                }
                return;
            }
            TeleportTime = 0;
            NPC.Opacity = 1f;
            float roarTime = Time - 60;
            if (roarTime == 0)
            {
                SoundEngine.PlaySound(_RoarSound, NPC.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    TOLocalizationUtils.ChatLocalizedTextFormat("Announcement.HasAwoken", new Color(175, 75, 255), NPC.TypeName);
            }
            if (roarTime < 60)
            {
                NPC.frameCounter++;
                BoCDrawOffset = Main.rand.NextVector2Circular(4, 4) * (1f - roarTime / 60f);
                Main.LocalPlayer.SetScreenshake(3f * (1f - roarTime / 60f));
                if (roarTime % 15 == 0)
                    ParticleHandler.SpawnParticle(new BossRoar(NPC.Center, Color.Black, Main.rand.NextFloatDirection(), 0.1f, 3f, 30));
                return;
            }
            BoCDrawOffset = Vector2.Zero;
            CurrentBehavior = Behavior.Phase1Idle;
            NPC.dontTakeDamage = true;
            NPC.damage = NPC.defDamage;
            ResetAttackValues();
            Time = -1;
            SpawnTime = -1;
            Main.musicFade[Main.curMusic] = 1f;
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
                            SoundEngine.PlaySound(_DeathRoarSound, NPC.Center);
                        Vector2 bloodDir = bloodGushingData[i].angle.ToRotationVector2();
                        BloodParticle2 p2 = new(NPC.Center + bloodGushingData[i].offset.RotatedBy(NPC.rotation), bloodDir * 7.5f, 16, 0.5f, Color.Red);
                        ParticleHandler.SpawnParticle(p2);
                        NPC.velocity = bloodDir * -4f;
                        SoundStyle bleed = _BloodShotSound with
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
                SoundEngine.PlaySound(_ExplosionSound, NPC.Center);
                SoundEngine.PlaySound(_BloodShotSound, NPC.Center);
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
        NPCTargetingParameters options = NPCTargetingParameters.BossDefaults;
        options.AggroRatio = -1f;
        options.FinishThemOff = true;
        options.MaxSearchRange = DespawnRange;
        options.TargetType = NPCTargetPreference.ForceSwitch;
        List<int> available = GetAllValidTargets(NPC.Center);
        if (available.Any(p => !TargetsSet.Contains(p)))
            options.ExcludedPlayers = [.. TargetsSet];
        else
        {
            TargetsSet.Clear();
            options.ExcludedPlayers = [NPC.target];
        }

        NPC.TargetWithOptions(options);
        TargetsSet.Add(NPC.target);
    }
}
