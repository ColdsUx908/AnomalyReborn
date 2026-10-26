using static Anomalies.Bosses.BrainofCthulhu.BrainofCthulhu;
using BrainBehavior = Anomalies.Bosses.BrainofCthulhu.BrainofCthulhu.Behavior;

namespace Anomalies.Bosses.BrainofCthulhu;

public sealed partial class CreeperAI : AnomalyNPCBehavior<CreeperAI>
{
    public override int ApplyingType => NPCID.Creeper;

    public override void SetDefaultsFinal()
    {
        NPC.lifeMax = CalamityEnabled ? 300 : 225;
        BridgeUtils.ApplyCalamityHealthBoost(NPC);
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) =>
        TONPCUtils.ApplyAnomalyBossDifficultyAndPlayerScaling(NPC, bossAdjustment);

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type != CalamityLogicType_NPCBehavior.VanillaOverrideAI;
    public float FlowAmount => MathHelper.Lerp(0.1f, 0.15f, NPC.localAI[3]);

    public enum Behavior
    {
        Idle,
        Charge
    }

    public int CreeperID { get => (int)NPC.ai[0]; set => NPC.ai[0] = value; }
    public Behavior CurrentBehavior { get => (Behavior)NPC.ai[1]; set => NPC.ai[1] = (float)value; }
    public ref float AttackAngle => ref NPC.ai[2];
    public ref float CachedValue1 => ref NPC.ai[3];
    public NPC Brain => Main.npc[NPC.crimsonBoss];
    public BrainofCthulhu BoCAI => Brain.GetBrainBehavior();
    public float BossCounter => BoCAI.Time;
    public bool EvenID => CreeperID % 2 == 0;

    public int CreeperCount
    {
        get
        {
            int c = NPC.CountNPCS(NPC.type);
            if (c > BrainofCthulhu.CreeperCount)
                c = BrainofCthulhu.CreeperCount;
            return c;
        }
    }

    public int LocalCreeperID => Main.npc.Where(n => n.active && n.type == NPCID.Creeper).ToList().IndexOf(NPC);
    public float CreeperAmountRatio => CreeperCount / (float)BrainofCthulhu.CreeperCount;

    public bool UseBossAIState
    {
        get => AnomalyNPC.AnomalyAI32[5].I != 0;
        set
        {
            AnomalyNPC.AnomalyAI32[5].I = value ? 1 : 0;
            AnomalyNPC.AIChanged32[5] = true;
        }
    }

    private static readonly BrainBehavior[] _BossAIStatesToUse = [BrainBehavior.SurfaceSpawnAnimation, BrainBehavior.CreeperSwipes, BrainBehavior.CreeperSwings, BrainBehavior.CreeperOrbit, BrainBehavior.CreeperSpiral];
    public override void SetDefaults()
    {
        NPC.damage = NPC.defDamage = 36; // 64 (1.8x expert scaling)
    }

    public override void OnSpawn(IEntitySource source)
    {
        NPC.damage = 0;
        Time = -1;
        PartnerIndex = -1;
        NPC.netUpdate = true;
    }

    /// <summary>幻影仅保留用于维持实体的生命值，不贡献混合血条的剩余生命值。</summary>
    public int CombinedLifeContribution => IsPhantom ? 0 : Math.Max(0, NPC.life);

    public const int PhantomFadeDuration = 45;
    public const int PhantomAppearDuration = 20;
    // Presentation only: start the fade-in when this client observes the phantom.
    public float PhantomAppearance => IsPhantom
        ? MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(NPC.localAI[1] / PhantomAppearDuration, 0f, 1f))
        : 0f;
    public float PhantomVisualOpacity => (IsPhantom
        ? (0.4f + 0.1f * MathF.Sin(Main.GlobalTimeWrappedHourly * 6f + CreeperID)) * PhantomAppearance
        : 1f) * PhantomFadeOpacity * NPC.Opacity;
    public float PhantomFadeOpacity => CurrentPhase is Phase.Transition or Phase.Phase2
        ? 1f - MathHelper.Clamp(BossCounter / PhantomFadeDuration, 0f, 1f)
        : 1f;

    public override bool PreAI()
    {
        if (!BrainBehaviorAccess.ActiveBrain)
        {
            NPC.active = false;
            NPC.netUpdate = true;
            return false;
        }

        if (IsPhantom)
        {
            EnsurePhantomDeathEffects();
            NPC.localAI[1] = Math.Min(NPC.localAI[1] + 1f, PhantomAppearDuration);
        }

        if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            TOCombatUtils.TargetWithOptions(NPC, default);
        if (!Main.dedServ)
            BrainOfCthulhuSystem.VerletTendrils[CreeperID].creeper = NPC.whoAmI;
        if (CurrentPhase is Phase.Transition or Phase.Phase2)
        {
            NPC.damage = 0;
            NPC.dontTakeDamage = true;
            NPC.chaseable = false;
            // Keep the formation attached to the moving brain while it dissolves.
            NPC.velocity = Brain.velocity;
            if (BossCounter >= PhantomFadeDuration && Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.active = false;
                NPC.netUpdate = true;
                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, NPC.whoAmI);
            }
            return false;
        }
        UseBossAIState = _BossAIStatesToUse.Contains(BoCAI.CurrentBehavior) && BossCounter >= 0;
        NPC.dontTakeDamage = IsPhantom || CurrentPhase == Phase.Introduction;
        NPC.chaseable = !IsPhantom;
        if (UseBossAIState)
            switch (BoCAI.CurrentBehavior)
            {
                case BrainBehavior.SurfaceSpawnAnimation:
                    NPC.damage = 0;
                    SpawnAnimation();
                    break;
                case BrainBehavior.CreeperSwipes:
                    CreeperSwipes();
                    break;
                case BrainBehavior.CreeperSwings:
                    CreeperSwings();
                    break;
                case BrainBehavior.CreeperOrbit:
                    CreeperOrbit();
                    break;
                case BrainBehavior.CreeperSpiral:
                    CreeperSpiral();
                    break;
            }
        else
            switch (CurrentBehavior)
            {
                case Behavior.Idle:
                    CreeperIdle();
                    break;
                case Behavior.Charge:
                    CreeperCharge();
                    break;
            }

        FlowTime += 0.01f * (2 * (1 + ConnectionOpacity));
        return false;
        void SpawnAnimation()
        {
            NPC.Opacity = Brain.Opacity;
            float rotation = MathHelper.TwoPi / (BrainofCthulhu.CreeperCount / 2f) * (CreeperID / 2f);
            Vector2 offset = new Vector2(EvenID ? -256 : 256, 0) + Vector2.UnitX.RotatedBy(rotation + BossCounter / 60f * (EvenID ? -1 : 1)) * 64;
            NPC.Center = Brain.Center + offset;
            NPC.velocity = Vector2.Zero;
            if (!Main.dedServ && BrainOfCthulhuSystem.VerletTendrils is { } tendrils && (uint)CreeperID < tendrils.Length)
            {
                List<VerletSimulatedSegment> segments = tendrils[CreeperID].tendril;
                for (int i = 0; i < segments.Count; i++)
                {
                    Vector2 position = Vector2.Lerp(Brain.Center + Vector2.UnitY * 32, NPC.Center, i / (float)(segments.Count - 1));
                    segments[i].Position = position;
                    segments[i].OldPosition = position;
                }
            }
        }

        void CreeperSwipes()
        {
            Vector2 goalLocation;
            if (BossCounter < 15)
            {
                UseBossAIState = false;
                Time = -1;
                CurrentBehavior = Behavior.Idle;
            }

            if (Brain.GetBrainBehavior().AttackList.Contains((byte)NPC.whoAmI))
            {
                if (Time < 0)
                    Time = 0;
            }
            else
            {
                if (Time >= 0)
                    Time = -1;
            }

            float baseRotation = MathHelper.TwoPi / (BrainofCthulhu.CreeperCount / 2f) * (CreeperID / 2f);
            bool singleHand = BoCAI.AttackFlag;
            int handSide = BoCAI.AttackSign;
            if (Time == -1)
            {
                NPC.damage = 0;
                NPC.knockBackResist = 0.72f;
                ConnectionOpacity = BringOpacityTo(ConnectionOpacity, 0f);
                if (!singleHand)
                    goalLocation = Brain.Center + (Vector2.UnitY * -32f) + (Vector2.UnitX * (EvenID ? -256 : 256)) + (Vector2.UnitX.RotatedBy(baseRotation + (BossCounter / 30f * (EvenID ? -1 : 1))) * new Vector2(32, 96));
                else
                    goalLocation = Brain.Center + (Vector2.UnitY * -32f) + (Vector2.UnitX * 256 * handSide) + (Vector2.UnitX.RotatedBy(baseRotation + (BossCounter / 30f * handSide)) * new Vector2(32, 96));
                goalLocation += Main.player[Brain.target].velocity * 24f;
                float distToGoal = NPC.Center.Distance(goalLocation);
                NPC.velocity = NPC.DirectionTo(goalLocation) * (2f + MathHelper.Clamp(distToGoal / 24f, 0f, 64f));
            }
            else
            {
                if (Time == 0)
                    NPC.netUpdate = true;
                NPC.damage = NPC.defDamage;
                NPC.knockBackResist = 0f;
                ConnectionOpacity = BringOpacityTo(ConnectionOpacity, 1f);
                float accel = 2f;
                float speed = 12f;
                if (Time < Brain.GetBrainBehavior().SwipeDelay)
                {
                    if (!singleHand)
                        goalLocation = Brain.Center + (Vector2.UnitY * -96f) + (Vector2.UnitX * (EvenID ? -300 : 300)) + (Vector2.UnitX.RotatedBy(baseRotation + (BossCounter / 30f * (EvenID ? -1 : 1))) * new Vector2(32, 96));
                    else
                        goalLocation = Brain.Center + (Vector2.UnitY * -96f) + (Vector2.UnitX * 300 * handSide) + (Vector2.UnitX.RotatedBy(baseRotation + (BossCounter / 30f * handSide)) * new Vector2(32, 96));
                    goalLocation += Main.player[Brain.target].velocity * 24f;
                    NPC.velocity += NPC.DirectionTo(goalLocation) * accel;
                    NPC.velocity = NPC.velocity.ClampMagnitude(0f, speed);
                }
                else
                {
                    if (!singleHand)
                        goalLocation = Brain.Center + (Vector2.UnitY * -96f) + (Vector2.UnitX * (EvenID ? -256 : 256)) + (Vector2.UnitX.RotatedBy(baseRotation + (BossCounter / 30f * (EvenID ? -1 : 1))) * new Vector2(32, 96));
                    else
                        goalLocation = Brain.Center + (Vector2.UnitY * -96f) + (Vector2.UnitX * 256 * handSide) + (Vector2.UnitX.RotatedBy(baseRotation + (BossCounter / 30f * handSide)) * new Vector2(32, 96));
                    goalLocation.Y += MathHelper.Lerp(0, 420, TOMathUtils.Interpolation.SineEaseInOut(MathHelper.Clamp((Time - Brain.GetBrainBehavior().SwipeDelay) / 20f, 0f, 1f), clamped: false));
                    if (!singleHand)
                        goalLocation.X += MathHelper.Lerp(0f, 900f, TOMathUtils.Interpolation.SineEaseInOut(MathHelper.Clamp((Time - (Brain.GetBrainBehavior().SwipeDelay + 5)) / 55f, 0f, 1f), clamped: false)) * (EvenID ? 1 : -1);
                    else
                        goalLocation.X += MathHelper.Lerp(0f, 900f, TOMathUtils.Interpolation.SineEaseInOut(MathHelper.Clamp((Time - (Brain.GetBrainBehavior().SwipeDelay + 5)) / 55f, 0f, 1f), clamped: false)) * (handSide == -1 ? 1 : -1);
                    NPC.Center = Vector2.Lerp(NPC.Center, goalLocation, MathHelper.Clamp((Time - Brain.GetBrainBehavior().SwipeDelay) / 10f, 0f, 1f));
                    NPC.netUpdate = true;
                }

                Time++;
            }
        }

        void CreeperSwings()
        {
            if (Brain.GetBrainBehavior().AttackList.Contains((byte)NPC.whoAmI))
            {
                if (Time < 0)
                {
                    Time = 0;
                    NPC.netUpdate = true;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        AttackPosition = NPC.Center;
                }
            }
            else
            {
                if (Time >= 0)
                    Time = -1;
            }

            if (Time < 0)
            {
                float baseRotation = (Brain.Center - Main.player[Brain.target].Center).ToRotation();
                bool evenID = CreeperID % 2 == 0;
                Vector2 goalLocation = Brain.Center + Vector2.UnitX.RotatedBy(baseRotation + Math.Sin(BossCounter / 60f + CreeperID) * (evenID ? -1 : 1)) * (evenID ? 175 : 250);
                if (NPC.DistanceSQ(goalLocation) > 9216)
                {
                    NPC.velocity += NPC.DirectionTo(goalLocation);
                    NPC.velocity = NPC.velocity.ClampMagnitude(0f, 8f);
                }

                ConnectionOpacity = BringOpacityTo(ConnectionOpacity, 0f);
            }
            else
            {
                Player sharedTarget = Main.player[Brain.target];
                Vector2 dashDir = AttackAngle.ToRotationVector2();
                if (!BoCAI.OnSecondCreeperPhase) // Swipe Attack
                {
                    float positioningTime = LightSwipeTravelTime + LightSwipeAttackDelay;
                    if (Time <= positioningTime)
                    {
                        Vector2 goalPosition = sharedTarget.Center - dashDir * 128;
                        goalPosition += dashDir.RotatedBy(MathHelper.PiOver2) * 16;
                        NPC.Center = Vector2.Lerp(AttackPosition, goalPosition, TOMathUtils.Interpolation.SineEaseOut(MathHelper.Clamp(Time / (float)LightSwipeTravelTime, 0f, 1f), clamped: false));
                        NPC.velocity = Vector2.Zero;
                        NPC.netUpdate = true;
                    }
                    else
                    {
                        AttackPosition = Vector2.Zero;
                        NPC.damage = NPC.defDamage;
                        NPC.knockBackResist = 0f;
                        int reelbackTime = 22;
                        if (Time < (positioningTime + reelbackTime))
                        {
                            float reelBackSpeedExponent = 2.6f;
                            float reelBackCompletion = Utils.GetLerpValue(0f, reelbackTime, Time - positioningTime, true);
                            float reelBackSpeed = MathHelper.Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                            Vector2 reelBackVelocity = dashDir * -reelBackSpeed;
                            NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);
                            if (Time == positioningTime + reelbackTime - 5)
                                SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, NPC.Center);
                        }
                        else if (Time == (positioningTime + reelbackTime))
                            NPC.velocity = dashDir * 32;
                        else
                        {
                            NPC.velocity *= 0.9f;
                            if (Time >= positioningTime + reelbackTime + 15)
                            {
                                NPC.damage = 0;
                                Time = -1;
                                Brain.GetBrainBehavior().AttackList.Remove((byte)NPC.whoAmI);
                                AttackPosition = Vector2.Zero;
                                ConnectionOpacity = BringOpacityTo(ConnectionOpacity, 0f);
                                return;
                            }
                        }
                    }
                }
                else //Crush Attack
                {
                    float positioningTime = StrongSwipeTravelTime + StrongSwipeAttackDelay;
                    if (Time <= positioningTime)
                    {
                        if (AttackPosition == Vector2.Zero)
                            AttackPosition = NPC.Center;
                        Vector2 goalPosition = sharedTarget.Center - dashDir * 128;
                        NPC.Center = Vector2.Lerp(AttackPosition, goalPosition, TOMathUtils.Interpolation.SineEaseOut(MathHelper.Clamp(Time / (float)StrongSwipeTravelTime, 0f, 1f), clamped: false));
                        NPC.velocity = Vector2.Zero;
                        NPC.netUpdate = true;
                    }
                    else
                    {
                        NPC.damage = NPC.defDamage;
                        NPC.knockBackResist = 0f;
                        int reelbackTime = 18;
                        float swingTime = 10f;
                        if (Time <= (positioningTime + reelbackTime))
                        {
                            float reelBackSpeedExponent = 2.6f;
                            float reelBackCompletion = Utils.GetLerpValue(0f, reelbackTime, Time - positioningTime, true);
                            float reelBackSpeed = MathHelper.Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                            Vector2 reelBackVelocity = dashDir * -reelBackSpeed;
                            NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);
                            if (Time == positioningTime + reelbackTime - 5)
                                SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, NPC.Center);
                            if (Time == (positioningTime + reelbackTime))
                                AttackPosition = NPC.Center;
                        }
                        else if (Time <= (positioningTime + reelbackTime + swingTime))
                        {
                            if (Main.npc[PartnerIndex].active)
                            {
                                float moveDist = 196;
                                NPC.Center = Vector2.Lerp(AttackPosition, AttackPosition + dashDir * moveDist, (Time - (positioningTime + reelbackTime)) / swingTime);
                                NPC.velocity = Vector2.Zero;
                                NPC.netUpdate = true;
                                if (Time == (positioningTime + reelbackTime + swingTime))
                                {
                                    SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, NPC.Center);
                                    SoundEngine.PlaySound(NPC.HitSound, NPC.Center);
                                    NPC.velocity = dashDir * -8;
                                    Vector2 attackCenter = AttackPosition + dashDir * 200;
                                    if (dashDir.X <= 0)
                                    {
                                        //WaterFoam (0.25 -> 1), SoftRoundExplosion (0.025 -> 0.1), SmokeExplosion(0.05 -> 0.15)
                                        CustomPulse splatter = new(attackCenter, Vector2.Zero, Color.Red, "Anomalies/Bosses/BrainofCthulhu/Assets/SmokeExplosion", Vector2.One, Main.rand.NextFloatDirection(), 0.05f, 0.15f, 24);
                                        ParticleHandler.SpawnParticle(splatter);
                                    }

                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                        for (int i = -1; i <= 1; i++)
                                            Projectile.NewProjectileAction(source: NPC.GetSource_FromThis(), position: attackCenter, velocity: dashDir.RotatedBy(MathHelper.PiOver2 + (MathHelper.Pi / 6f * i)) * 8f, type: ModContent.ProjectileType<CreeperBloodShot>(), damage: BloodShotDamage, knockback: 0.5f);
                                }
                            }
                            else
                                NPC.velocity = dashDir * 19.6f;
                        }
                        else if (Time < (positioningTime + reelbackTime + swingTime + 30))
                        {
                            NPC.velocity *= 0.96f;
                        }
                        else
                        {
                            NPC.damage = 0;
                            Time = -1;
                            Brain.GetBrainBehavior().AttackList.Remove((byte)NPC.whoAmI);
                            AttackPosition = Vector2.Zero;
                            ConnectionOpacity = BringOpacityTo(ConnectionOpacity, 0.5f);
                            return;
                        }
                    }
                }

                ConnectionOpacity = BringOpacityTo(ConnectionOpacity, 1f);
                Time++;
            }
        }

        void CreeperOrbit()
        {
            if (Main.netMode != NetmodeID.SinglePlayer && CachedValue2 != -1)
            {
                CreeperCharge();
                return;
            }

            BrainofCthulhu brainAI = Brain.GetBrainBehavior();
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                if (BossCounter == 0)
                {
                    CachedValue1 = MathHelper.TwoPi / CreeperCount * LocalCreeperID;
                    AttackPosition = NPC.Center;
                    AttackAngle = 0;
                    Time = -1;
                    NPC.netUpdate = true;
                }
            }
            else
            {
                if (BossCounter == 1)
                {
                    List<NPC> mainOrbitMembers = [.. Main.npc.Where(n => n.active && n.type == NPCID.Creeper && n.TryGetCreeperBehavior(out CreeperAI ai) && ai.CachedValue2 == -1)];
                    CachedValue1 = MathHelper.TwoPi / mainOrbitMembers.Count * mainOrbitMembers.IndexOf(NPC);
                }
                else if (BossCounter == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    AttackPosition = NPC.Center;
                    AttackAngle = 0;
                    NPC.netUpdate = true;
                }
            }

            float dist = OrbitStandardRadius + ((float)Math.Sin((CachedValue1 * 7) + BossCounter / 20f) * 24);
            if (Main.netMode != NetmodeID.MultiplayerClient)
                if (brainAI.AttackList.Contains((byte)NPC.whoAmI))
                {
                    if (Time < 0)
                    {
                        Time = 0;
                        NPC.netUpdate = true;
                    }
                }
                else
                {
                    if (Time >= 0)
                    {
                        Time = -1;
                        NPC.netUpdate = true;
                    }
                }

            if (Time >= 0)
            {
                float telegraphPeriod = OrbitAttackInterval * 0.25f;
                float shiftPeriod = (OrbitAttackInterval - telegraphPeriod) / 2f;
                if (Time < telegraphPeriod)
                    dist = MathHelper.Lerp(dist, OrbitTelegraphRadius, TOMathUtils.Interpolation.SineEaseOut(Time / telegraphPeriod, clamped: false));
                else if (Time < shiftPeriod + telegraphPeriod)
                    dist = MathHelper.Lerp(OrbitTelegraphRadius, 16, TOMathUtils.Interpolation.SineEaseInOut((Time - telegraphPeriod) / shiftPeriod, clamped: false));
                else
                {
                    dist = MathHelper.Lerp(16, dist, TOMathUtils.Interpolation.SineEaseInOut((Time - telegraphPeriod - shiftPeriod) / shiftPeriod, clamped: false));
                    if (Time > OrbitAttackInterval)
                    {
                        Time = -2;
                        Brain.GetBrainBehavior().AttackList.Remove((byte)NPC.whoAmI);
                    }
                }

                ConnectionOpacity = BringOpacityTo(ConnectionOpacity, 1f, 0.1f);
                Time++;
            }
            else
                ConnectionOpacity = BringOpacityTo(ConnectionOpacity, 0f, 0.05f);
            float slowDown = 1 - MathHelper.Clamp((BossCounter - OrbitDuration) / 30f, 0f, 1f);
            AttackAngle += BaseRotationSpeed * (MathHelper.Lerp(1f, 0.5f, CreeperAmountRatio) + (BoCAI.OnSecondCreeperPhase ? 0.75f : 0.5f)) * slowDown * BoCAI.AttackSign;
            Vector2 rotation = Vector2.UnitX.RotatedBy(CachedValue1 + AttackAngle) * dist;
            if (BossCounter < OrbitSetupDuration)
            {
                NPC.damage = 0;
                NPC.Center = Vector2.Lerp(AttackPosition, BoCAI.AttackPosition + rotation, TOMathUtils.Interpolation.SineEaseOut(BossCounter / OrbitSetupDuration, clamped: false));
            }
            else
            {
                NPC.damage = NPC.defDamage;
                NPC.Center = BoCAI.AttackPosition + rotation;
            }

            NPC.netUpdate = true;
        }

        void CreeperSpiral()
        {
            int tendrilID = (CreeperID % TendrilCount) + 1;
            if (BossCounter == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                AttackPosition = NPC.Center;
                List<NPC> myGroup = [.. Main.npc.Where(n => n.active && n.type == NPCID.Creeper && (n.ai[0] % TendrilCount) + 1 == tendrilID)];
                CachedValue1 = myGroup.IndexOf(NPC);
                CachedValue2 = myGroup.Count;
                AttackAngle = 0;
                NPC.netUpdate = true;
            }

            int index = (int)CachedValue1;
            int groupCount = CachedValue2;
            bool isLast = groupCount > 1 && index == (groupCount - 1);
            float placementRatio = (index + 1) / (float)(groupCount + 1);
            float spiralAngle = BoCAI.AttackRotation;
            float goalAngle = spiralAngle + (MathHelper.TwoPi / TendrilCount * tendrilID);
            float angularVelocity = (goalAngle - AttackAngle) / (16f + (placementRatio * 16f));
            AttackAngle += angularVelocity;
            float goalRadius = TendrilStartDistance + (TendrilLength * placementRatio);
            goalRadius += (float)Math.Sin(BossCounter / 20f) * MathHelper.Lerp(MaxCreeperSway, 0, groupCount / (float)(BrainofCthulhu.CreeperCount / 3)) * (index % 2 == 0 ? -1 : 1);
            Vector2 angleVec = AttackAngle.ToRotationVector2();
            Vector2 position = Brain.Center + (angleVec * goalRadius);
            if (BossCounter >= SpiralSetupTime)
            {
                NPC.Center = position;
                NPC.netUpdate = true;
                NPC.damage = NPC.defDamage;
                NPC.knockBackResist = 0f;
                ConnectionOpacity = BringOpacityTo(ConnectionOpacity, 1f);
            }
            else if (BossCounter < SpiralSetupTime / 10f)
                NPC.velocity *= 0.66f;
            else
            {
                NPC.velocity = Vector2.Zero;
                float lerp = (BossCounter - (SpiralSetupTime / 10f)) / (float)(SpiralSetupTime * 0.9f);
                NPC.Center = Vector2.Lerp(AttackPosition, position, TOMathUtils.Interpolation.SineEaseInOut(MathHelper.Clamp(lerp, 0f, 1f), clamped: false));
                NPC.netUpdate = true;
            }

            if (Main.dedServ && BossCounter > SpiralSetupTime && isLast && BossCounter % 15 == 0) //These projectiles are multiplayer exclusive to punish players outside the spiral
            {
                //explicitly does no knockback to allow player to more easily get back in the spiral
                Projectile.NewProjectileAction(source: NPC.GetSource_FromThis(), position: NPC.Center, velocity: angleVec.RotatedBy(MathHelper.Pi / 12f) * 18f, type: ModContent.ProjectileType<CreeperBloodShot>(), damage: BloodShotDamage, knockback: 0f, action: entity =>
                {
                    entity.timeLeft /= 4;
                });
                Projectile.NewProjectileAction(source: NPC.GetSource_FromThis(), position: NPC.Center, velocity: angleVec * 18f, type: ModContent.ProjectileType<CreeperBloodShot>(), damage: BloodShotDamage, knockback: 0f, action: entity =>
                {
                    entity.timeLeft /= 4;
                });
                Projectile.NewProjectileAction(source: NPC.GetSource_FromThis(), position: NPC.Center, velocity: angleVec.RotatedBy(-MathHelper.Pi / 12f) * 18f, type: ModContent.ProjectileType<CreeperBloodShot>(), damage: BloodShotDamage, knockback: 0f, action: entity =>
                {
                    entity.timeLeft /= 4;
                });
            }
        }

        void CreeperIdle()
        {
            if (Time == 0) //Originally this would be set by Brain. This currently doesn't happen due to idle phase speed up
            {
                CurrentBehavior = Behavior.Charge;
                return;
            }

            NPC.knockBackResist = 0.72f;
            NPC.damage = 0;
            float baseRotation = MathHelper.TwoPi / (BrainofCthulhu.CreeperCount / 2f) * (CreeperID / 2f);
            Vector2 goalLocation = Brain.Center + (Vector2.UnitX * (EvenID ? -256 : 256)) + Vector2.UnitX.RotatedBy(baseRotation + (BossCounter / 60f * (EvenID ? -1 : 1))) * 64;
            if (NPC.DistanceSQ(goalLocation) > 9216)
            {
                NPC.velocity += NPC.DirectionTo(goalLocation);
                NPC.velocity = NPC.velocity.ClampMagnitude(0f, 8f);
            }

            ConnectionOpacity = BringOpacityTo(ConnectionOpacity, 0f);
            Time = -1;
            CachedValue2 = -1;
            AttackPosition = Vector2.Zero;
        }

        void CreeperCharge()
        {
            Player target = Main.player[CachedValue2];
            if (Time < CreeperChargePositioningTime)
            {
                Vector2 goalLocation = target.Center + ((NPC.Center - target.Center).SafeNormalize(-Vector2.UnitY).RotatedBy(EvenID ? -MathHelper.PiOver4 : MathHelper.PiOver4) * 96);
                if (NPC.DistanceSQ(goalLocation) > 4096)
                {
                    NPC.velocity += NPC.DirectionTo(goalLocation);
                    NPC.velocity = NPC.velocity.ClampMagnitude(0f, 12f);
                }

                ConnectionOpacity = BringOpacityTo(ConnectionOpacity, 1f);
            }
            else
            {
                NPC.damage = NPC.defDamage;
                NPC.knockBackResist = 0f;
                if (Time < (CreeperChargePositioningTime + CreeperChargeWindUpTime))
                {
                    float reelBackSpeedExponent = 2.6f;
                    float reelBackCompletion = Utils.GetLerpValue(0f, CreeperChargeWindUpTime, Time - CreeperChargePositioningTime, true);
                    float reelBackSpeed = MathHelper.Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                    Vector2 reelBackVelocity = NPC.DirectionTo(target.Center) * -reelBackSpeed;
                    NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);
                    if (Time == CreeperChargePositioningTime + CreeperChargeWindUpTime - 5)
                        SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, NPC.Center);
                }
                else if (Time == CreeperChargePositioningTime + CreeperChargeWindUpTime)
                    NPC.velocity = NPC.DirectionTo(target.Center) * 24;
                else
                {
                    NPC.velocity *= 0.975f;
                    if (Time >= CreeperChargePositioningTime + CreeperChargeWindUpTime + 30)
                    {
                        NPC.damage = 0;
                        Time = -10;
                        CurrentBehavior = Behavior.Idle;
                        return;
                    }
                }
            }

            Time++;
        }
    }

    // Local presentation state: each client plays the effect once, including when
    // the synchronized phantom state arrives without the original lethal hit.
    private bool PhantomDeathEffectsPlayed
    {
        get => NPC.localAI[2] != 0f;
        set => NPC.localAI[2] = value ? 1f : 0f;
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        if (Main.dedServ || NPC.life > 0 || PhantomDeathEffectsPlayed || !BrainBehaviorAccess.ActiveBrain || CurrentPhase is not (Phase.Introduction or Phase.Phase1))
            return;

        // VanillaHitEffect has already emitted the creeper's blood dust and gore.
        PhantomDeathEffectsPlayed = true;
        SoundEngine.PlaySound(NPC.DeathSound, NPC.Center);
        if (BrainOfCthulhuSystem.VerletTendrils is { } tendrils && (uint)CreeperID < tendrils.Length && tendrils[CreeperID].tendril is { Count: >= 2 } segments)
        {
            Vector2 direction = (NPC.Center - segments[^2].Position).SafeNormalize(Vector2.UnitY);
            for (int i = 0; i < 5; i++)
                ParticleHandler.SpawnParticle(new BloodParticle(NPC.Center, direction.RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 10f, MathHelper.Pi / 10f)) * Main.rand.NextFloat(6f, 10f), 24, Main.rand.NextFloat(0.5f, 1f), Color.Yellow * 0.75f));
        }
    }

    private void EnsurePhantomDeathEffects()
    {
        if (Main.dedServ || PhantomDeathEffectsPlayed)
            return;

        int life = NPC.life;
        try
        {
            // Reuse vanilla death dust/gore for deaths without a local hit effect.
            NPC.life = 0;
            NPC.HitEffect(new NPC.HitInfo { HitDirection = NPC.direction });
        }
        finally
        {
            NPC.life = life;
        }
    }

    public override bool CheckDead()
    {
        if (!BrainBehaviorAccess.ActiveBrain || CurrentPhase is not (Phase.Introduction or Phase.Phase1))
            return true;
        EnsurePhantomDeathEffects();
        IsPhantom = true;
        NPC.life = NPC.lifeMax;
        NPC.dontTakeDamage = true;
        NPC.chaseable = false;
        NPC.lifeRegen = 0;
        NPC.netUpdate = true;
        return false;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;
}
