namespace Anomalies.Bosses.Skeletron;

public sealed partial class SkeletronHead
{
    public override bool PreAI()
    {
        using BossNetUpdateScope netUpdate = new(NPC);

        // Late joiners and predicted timers receive the same complete per-NPC snapshot.
        if (TONetworkEnvironment.NotClient && ++ResyncTimer >= 60f)
        {
            ResyncTimer = 0f;
            NPC.netUpdate = true;
        }

        NPC.defense = NPC.defDefense;
        NPC.damage = NPC.defDamage;
        NPC.reflectsProjectiles = false; //不反弹弹幕

        if (CurrentBehavior == Behavior.Despawn || !NPC.TargetClosestIfInvalid(true, DespawnDistance))
        {
            if (TONetworkEnvironment.NotClient)
                CurrentBehavior = Behavior.Despawn;
            if (CurrentBehavior == Behavior.Despawn)
                Despawn();
            return false;
        }

        if (NPC.timeLeft < 1800)
            NPC.timeLeft = 1800;

        if (CurrentPhase == Phase.Initialize)
        {
            if (TONetworkEnvironment.NotClient)
            {
                SpawnHand(-1.3f, 0f, 0f);
                SpawnHand(1.3f, 0f, 150f);
                SpawnHand(-1.3f, -1f, UpperHandNegativeSideInitialTimer);
                SpawnHand(1.3f, -1f, UpperHandPositiveSideInitialTimer);
                NPC.netUpdate = true;

                void SpawnHand(float side, float verticalSide, float timer)
                {
                    NPC.NewNPCAction(SourceAI, NPC.Center, NPCID.SkeletronHand, NPC.whoAmI, hand =>
                    {
                        hand.ai[0] = side;
                        hand.ai[1] = NPC.whoAmI;
                        hand.ai[3] = timer;
                        hand.target = NPC.target;
                        hand.Anomaly.EnsureAICapacity(9, 1);
                        hand.Anomaly.AnomalyAI32[0].F = verticalSide;
                        hand.Anomaly.AnomalyAI32[2].F = timer;
                        hand.netUpdate = true;
                    });
                }
            }

            CurrentPhase = Phase.Phase1;
        }

        int numHandsAlive = UpdateHandProtection();
        bool handsDead = numHandsAlive == 0;

        AnomalyNPC.DR = 0f;
        AnomalyNPC.ExtraDR = 0f;
        AnomalyNPC.BossBar_IncreasingDefenseOrDR = false;

        if (TONetworkEnvironment.NotClient && handsDead && ShouldEnterPhase3)
        {
            Phase3HealStartLife = NPC.life;
            CurrentPhase = Phase.PhaseChange_2To3;
            CurrentBehavior = Behavior.PhaseChange_2To3;
            CurrentLocalPhase = 0;
            Timer = AttackTimer = ChargeReadiness = ChargeTelegraphTimer = 0f;
            TeleportTimer = TeleportX = TeleportY = 0f;
            CompletedCharges = 0;
        }

        if (!IsEnteringPhase3 && !Phase3 && Main.IsItDay() && CurrentBehavior != Behavior.Enraged)
        {
            CurrentBehavior = Behavior.Enraged;
            SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
        }

        AnomalyNPC.BossBar_Enraged = CurrentBehavior == Behavior.Enraged;
        switch (CurrentPhase)
        {
            case Phase.PhaseChange_2To3:
                PhaseChange_2To3AI();
                break;
            case Phase.Phase3:
                Phase3AI();
                break;
            case Phase.Phase1 or Phase.Phase2 or Phase.Phase2_2 when CurrentBehavior == Behavior.Enraged:
                Enraged();
                break;
            case Phase.Phase1:
                Phase1AI();
                break;
            case Phase.Phase2 or Phase.Phase2_2:
                Phase2AI();
                break;
        }

        // Emit dust
        if (CurrentBehavior != Behavior.Enraged && numHandsAlive != 0)
        {
            Dust.NewDustAction(new Vector2(NPC.Center.X - 15f - NPC.velocity.X * 5f, NPC.position.Y + NPC.height - 2f) + new Vector2(30 / 2f, 10 / 2f), 30, 10, DustID.Blood, new Vector2(-NPC.velocity.X * 0.2f, 3f), dust =>
            {
                dust.alpha = 0;
                dust.scale *= 2f;
                dust.noGravity = true;
                dust.velocity.X *= 1.3f;
                dust.velocity.X += NPC.velocity.X * 0.4f;
                dust.velocity.Y += (2f + NPC.velocity.Y);
            });
            for (int j = 0; j < 2; j++)
            {
                Dust.NewDustAction(new Vector2(NPC.position.X, NPC.position.Y + 120f) + new Vector2(NPC.width / 2f, 60 / 2f), NPC.width, 60, DustID.Blood, new Vector2(NPC.velocity.X, NPC.velocity.Y), dust =>
                {
                    dust.alpha = 0;
                    dust.scale *= 2f;
                    dust.noGravity = true;
                    dust.velocity -= NPC.velocity;
                    dust.velocity.Y += 5f;
                });
            }
        }

        return false;

        int UpdateHandProtection()
        {
            int count = CountLivingHands();
            NPC.dontTakeDamage = count > 0;
            NPC.chaseable = count == 0;
            return count;
        }

        void Phase1AI()
        {
            switch (CurrentBehavior)
            {
                case Behavior.Hover:
                    Hover();
                    break;
                case Behavior.Spin:
                    Spin();
                    break;
            }

            void Hover()
            {
                if (TONetworkEnvironment.NotClient && handsDead && CurrentLocalPhase != 2)
                {
                    CurrentLocalPhase = 2;
                    Timer = 0f;
                    ChargeTelegraphTimer = 0f;
                }

                if (CurrentLocalPhase != 2)
                    AttackTimer++;
                switch (CurrentLocalPhase)
                {
                    case 0 when AttackTimer <= 0f: //悬停静默期，等待攻击计时恢复
                        break;
                    case 0: //静默期结束，当帧开始普通射击
                        CurrentLocalPhase = 1;
                        goto case 1;
                    case 1: //普通悬停射击并累积旋转蓄力
                        FireHoverSkulls(SkullInterval);
                        Timer += ChargeGateValue / WithHandsDuration;
                        if (TONetworkEnvironment.NotClient && Timer >= ChargeGateValue)
                        {
                            BeginSpin(Behavior.Spin, 0);
                            return;
                        }
                        break;
                    case 2: //双手被击败后静默蓄力，随后开始无手旋转
                        Timer++;
                        if (TONetworkEnvironment.NotClient && Timer >= HandsClearedHoverQuietDuration)
                            BeginSpin(Behavior.Spin, 1);
                        else
                            MoveWhileHovering();
                        return;
                }

                ChargeTelegraphTimer = Math.Min(Timer, ChargeGateValue);
                MoveWhileHovering();
            }

            void Spin()
            {
                BeginSpinFrame(SoundID.ForceRoar);
                FadeChargeTelegraph();

                switch (CurrentLocalPhase)
                {
                    case 0 when TONetworkEnvironment.NotClient && handsDead: //旋转中双手被击败，切换到无手旋转计时
                        float remaining = Math.Clamp(WithHandsDuration - Timer, HandsClearedMinSpinRemaining, HandsClearedSpinDuration);
                        Timer = HandsClearedSpinDuration - remaining;
                        CurrentLocalPhase = 1;
                        goto case 1;
                    case 0: //双手仍存活，沿用有手旋转时长
                        TrackTarget(WithHandsDuration);
                        break;
                    case 1: //无手旋转，追踪玩家
                        TrackTarget(HandsClearedSpinDuration);
                        break;
                }

                void TrackTarget(float duration)
                {
                    if (TONetworkEnvironment.NotClient && Timer >= duration)
                        FinishSpin();
                    else
                        SkeletronHandler.SpinMovement(NPC, Target, SpinSpeed, SkeletronHandler.GetSpinMoveSpeed(NPC.Distance(Target.Center)) * 0.8f);
                }
            }
        }

        void Phase2AI()
        {
            switch (CurrentBehavior)
            {
                case Behavior.Hover:
                    Hover();
                    break;
                case Behavior.SkullWarningSpin:
                    SkullWarningSpin();
                    break;
                case Behavior.ForwardSkullSpin:
                    ForwardSkullSpin();
                    break;
                case Behavior.TripleSpinCharge:
                    TripleSpinCharge();
                    break;
            }

            void Hover()
            {
                int shockwaveTime = HasCompletedNormalHover ? 0
                    : (int)MathF.Round(MathHelper.Lerp(MinNormalSkullDuration, MaxNormalSkullDuration, HoverLifeRatio));
                AttackTimer++;
                switch (CurrentLocalPhase)
                {
                    case 0 when AttackTimer <= 0f: //悬停静默期，等待攻击计时恢复
                        break;
                    case 0 when HasCompletedNormalHover: //后续轮次跳过普通射击与冲击波，直接开始快速射击
                        CurrentLocalPhase = 2;
                        goto case 2;
                    case 0: //静默期结束，当帧开始普通射击
                        CurrentLocalPhase = 1;
                        goto case 1;
                    case 1 when AttackTimer > shockwaveTime: //普通射击结束，切换到快速射击
                        CurrentLocalPhase = 2;
                        goto case 2;
                    case 1: //普通射击，在指定帧释放冲击波
                        bool releaseShockwave = AttackTimer == shockwaveTime && TONetworkEnvironment.NotClient;
                        if (releaseShockwave)
                        {
                            RapidSkullDuration = (int)MathF.Round(MathHelper.Lerp(MinRapidSkullDuration, MaxRapidSkullDuration, NPC.LifeRatio));
                            NPC.netUpdate = true;
                        }

                        if (AttackTimer < shockwaveTime + RapidSkullDuration - QuietDuration)
                            FireHoverSkulls(SkullInterval);

                        if (releaseShockwave)
                            Projectile.NewProjectileAction<BoneShockwave>(SourceAI, NPC.Center, Vector2.Zero, 0, 0f, action: p => p.scale = 0f);
                        break;
                    case 2: // 快速射击，收尾静默期停止发射
                        if (AttackTimer < shockwaveTime + RapidSkullDuration - QuietDuration)
                            FireHoverSkulls(EnhancedSkullInterval);
                        break;
                }

                int hoverDuration = shockwaveTime + RapidSkullDuration;
                if (AttackTimer > 0f)
                    Timer = ChargeGateValue * AttackTimer / hoverDuration;
                ChargeTelegraphTimer = Math.Min(Timer, ChargeGateValue);

                if (TONetworkEnvironment.NotClient && AttackTimer >= hoverDuration)
                {
                    AdvancePhase2Subphase();
                    Behavior nextSpin = NextEnhancedSpin;
                    BeginSpin(nextSpin, 0);
                    NextEnhancedSpin = nextSpin switch
                    {
                        Behavior.SkullWarningSpin => Behavior.TripleSpinCharge,
                        Behavior.TripleSpinCharge => Behavior.ForwardSkullSpin,
                        _ => Behavior.SkullWarningSpin,
                    };
                    return;
                }

                MoveWhileHovering();
            }

            void SkullWarningSpin() =>
                UpdateWarningSpin(SkullWarningInterval, EnhancedSpinDuration, forwardSkullSpin: false);

            void ForwardSkullSpin() =>
                UpdateWarningSpin(ForwardSkullInterval, ForwardSkullSpinDuration, forwardSkullSpin: true);

            void UpdateWarningSpin(int interval, int duration, bool forwardSkullSpin)
            {
                BeginSpinFrame(SoundID.ForceRoar);
                switch (CurrentLocalPhase)
                {
                    case 0 when AttackTimer > duration - SkeletronSkullWarning.WarningTime: //预警窗口结束，切换到旋转收尾
                        CurrentLocalPhase = 1;
                        break;
                    case 0: //旋转中按间隔生成骷髅预警
                        if (TONetworkEnvironment.NotClient)
                            SpawnSkullWarnings(forwardSkullSpin, interval);
                        break;
                    case 1: //旋转收尾，停止生成预警
                        break;
                }

                FadeChargeTelegraph();

                if (TONetworkEnvironment.NotClient && AttackTimer >= duration + WarningSpinQuietDuration)
                    FinishSpin();
                else
                    SkeletronHandler.SpinMovement(NPC, Target, SpinSpeed, SkeletronHandler.GetSpinMoveSpeed(NPC.Distance(Target.Center)));

                void SpawnSkullWarnings(bool forwardSkullSpin, int interval)
                {
                    if (AttackTimer % interval != 0f)
                        return;

                    Player warningTarget = Target;
                    if (forwardSkullSpin)
                    {
                        Vector2 forward = warningTarget.velocity.SafeNormalize(new Vector2(warningTarget.direction, 0f));
                        float arc = MathHelper.ToRadians(ForwardSkullArc);
                        int count = ForwardSkullCount;
                        for (int i = 0; i < count; i++)
                        {
                            float angle = MathHelper.Lerp(-arc / 2f, arc / 2f, i / (float)(count - 1));
                            Vector2 offset = forward.RotatedBy(angle) * ForwardSkullRadius;
                            SpawnWarning(offset);
                            if (Phase2_2)
                                SpawnWarning(-offset);
                        }
                    }
                    else
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        int count = Phase2_2 ? Phase22SkullWarningCount : SkullWarningCount;
                        for (int i = 0; i < count; i++)
                        {
                            Vector2 offset = new Vector2(Main.rand.NextFloat(800f, 1000f), 0f)
                                .RotatedBy(angle + MathHelper.TwoPi * i / count);
                            SpawnWarning(offset);
                        }
                    }

                    void SpawnWarning(Vector2 offset)
                    {
                        Projectile.NewProjectileAction<SkeletronSkullWarning>(SourceAI,
                            warningTarget.Center + offset, Vector2.Zero, SkullDamage, 0f, action: p =>
                            {
                                p.ai[0] = warningTarget.whoAmI + 1;
                                p.ai[2] = SkeletronSkullWarning.EncodeSource(NPC.whoAmI, forwardSkullSpin);
                            });
                    }
                }
            }

            void TripleSpinCharge()
            {
                switch (CurrentLocalPhase)
                {
                    case 0: //后撤并蓄力，为下一次冲锋拉开距离
                        RetreatForCharge();
                        return;
                    case 1: //追踪玩家并准备锁定冲锋方向
                    case 2: //锁定方向后继续冲锋，直到本次冲锋结束
                        BeginSpinFrame(SoundID.ForceRoarPitched);
                        FadeChargeTelegraph();
                        if (TONetworkEnvironment.NotClient && Timer >= SpinChargeDuration) //冲锋结束
                        {
                            //释放头骨弹幕

                            FireChargeEndSkulls();
                            if (++CompletedCharges < ChargesPerRound)
                            {
                                Timer = AttackTimer = 0f;
                                CurrentLocalPhase = 0;
                                RetreatForCharge();
                            }
                            else
                                FinishSpin();
                            return;
                        }
                        break;
                }

                NPC.rotation += NPC.direction * SpinSpeed;
                float trackingEndTime = SpinChargeDuration * 0.9f;
                if (CurrentLocalPhase != 1 || Timer >= trackingEndTime)
                    return;

                Player target = Target;
                float distance = NPC.Distance(target.Center);
                if (distance > ChargeDistance || Timer == 1f)
                {
                    float speed = SkeletronHandler.GetSpinMoveSpeed(distance) * ChargeTrackingSpeedMultiplier;
                    NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * (speed + 2f);
                }
                else if (TONetworkEnvironment.NotClient)
                {
                    CurrentLocalPhase = 2;
                    Timer = trackingEndTime;
                }

                void FireChargeEndSkulls()
                {
                    if (Phase2_2 && CompletedCharges == ChargesPerRound - 1)
                    {
                        int fanCount = FinalChargeFanCount;
                        float halfAngle = MathHelper.ToRadians(FinalChargeFanHalfAngle);
                        float speed = FinalChargeSkullSpeed;
                        float maxSpeed = FinalChargeSkullMaxSpeed;
                        Vector2 aim = (Target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);

                        //致敬灾厄的扇形头骨
                        for (int i = 0; i <= fanCount; i++)
                        {
                            float angle = MathHelper.Lerp(-halfAngle, halfAngle, i / (fanCount - 1f));
                            Vector2 direction = aim.RotatedBy(angle);
                            Vector2 position = NPC.Center - direction * FinalChargeSpawnOffset;
                            Vector2 velocity = direction * speed;
                            FireSkullMode(Skull.Homing);
                            FireSkullMode(Skull.Straight);

                            void FireSkullMode(int mode) =>
                                Projectile.NewProjectileAction(SourceAI, position, velocity, ProjectileID.Skull, SkullDamage, 0f, action: p =>
                                {
                                    Skull.InitializeFlight(p, mode, maxSpeed, Skull.RegularAcceleration);
                                    Skull.SetSource(p, NPC);
                                    p.timeLeft = 600;
                                });
                        }
                    }
                    else
                    {
                        float firstAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float centerAngle = firstAngle + ChargeEndSkullSpacing * (ChargeEndSkullCount - 1) / 2f;
                        Vector2 centerVelocity = centerAngle.ToRotationVector2() * ChargeEndSkullSpeed;
                        Projectile.NewProjectilesArc(ChargeEndSkullCount, ChargeEndSkullSpacing, SourceAI, NPC.Center,
                            centerVelocity, ProjectileID.Skull, SkullDamage, 0f, action: p =>
                            {
                                Skull.InitializeFlight(p, Skull.Homing, RadialSkullMaxSpeed, RadialSkullAcceleration,
                                    homingStartFrame: RadialSkullHomingStartFrame, homingDuration: RadialSkullHomingDuration);
                                Skull.SetSource(p, NPC);
                                p.timeLeft = 600;
                            });
                    }
                }

                void RetreatForCharge()
                {
                    Timer++;
                    Vector2 away = NPC.Center - Target.Center;
                    float moveAwayVelocity = SpinBaseMoveSpeed * RetreatSpeedMultiplier;
                    float speed = moveAwayVelocity * MathHelper.Lerp(0.35f, 1f, Math.Min(Timer / ChargeRetreatDuration, 1f));
                    NPC.velocity = away.SafeNormalize(-Vector2.UnitY) * speed;
                    NPC.rotation += NPC.direction * SpinSpeed;
                    ChargeTelegraphTimer = MathHelper.Lerp(ChargeGateValue - ChargeTelegraphTime, ChargeGateValue,
                        Math.Min(Timer / ChargeRetreatDuration, 1f));

                    if (TONetworkEnvironment.NotClient && Timer >= ChargeRetreatDuration
                        && (away.LengthSquared() >= ChargeDistance * ChargeDistance || Timer >= MaxChargeRetreatDuration))
                    {
                        Timer = 0f;
                        CurrentLocalPhase = 1;
                    }
                }
            }
        }

        void BeginSpin(Behavior behavior, int localPhase)
        {
            Timer = AttackTimer = 0f;
            CurrentBehavior = behavior;
            CurrentLocalPhase = localPhase;
            ChargeReadiness = 1f;
            ChargeTelegraphTimer = ChargeGateValue;
        }

        void FireHoverSkulls(int interval)
        {
            if (!TONetworkEnvironment.NotClient)
                return;

            int age = (int)AttackTimer;
            if (!handsDead)
                interval *= 2;
            if (age <= SkullInterval || age % interval != 0)
                return;

            Player target = Target;
            SkeletronHandler.FireSkull(NPC, target, HoverDoubleSkullChanceDenominator > 0 && Main.rand.NextBool(HoverDoubleSkullChanceDenominator) ? 2 : 1, TOMathUtils.PiOver10, HeadSkullSpeed,
                SkullDamage, addNPCVelocity: false);
            if (Phase2_2)
            {
                float speed = HoverCrossboneSpeed;
                Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                Vector2 position = NPC.Center + direction * HeadSkullSpeed
                    + new Vector2(0f, 25f * NPC.scale).RotatedBy(NPC.rotation);
                Crossbone.Fire(NPC, position, direction * speed, SkullDamage,
                    CrossboneMaxSpeed);
            }
        }

        void MoveWhileHovering()
        {
            float headYAcceleration = HoverBaseAccelerationY + HoverLowHealthAccelerationY * (1f - (float)NPC.LifeRatio);
            float headYTopSpeed = headYAcceleration * 100f;
            float headXAcceleration = HoverBaseAccelerationX + HoverLowHealthAccelerationX * (1f - (float)NPC.LifeRatio);
            float headXTopSpeed = headXAcceleration * 100f;
            float rotation = NPC.velocity.X / 15f;
            SkeletronHandler.NormalMovement(NPC, Target, headXTopSpeed, headYTopSpeed,
                headXAcceleration, headYAcceleration, 250f);
            NPC.rotation = rotation;
        }

        void BeginSpinFrame(SoundStyle roar)
        {
            if (Aroma && TONetworkEnvironment.NotClient && Timer == 0f && CompletedCharges == 0)
            {
                if (NPC.CountNPCS(NPCID.DarkCaster) < 6)
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        int headYAcceleration = (int)(NPC.Center.X / 16f) + Main.rand.Next(-50, 51);
                        int headYTopSpeed;
                        for (headYTopSpeed = (int)(NPC.Center.Y / 16f) + Main.rand.Next(-50, 51); headYTopSpeed < Main.maxTilesY - 10 && !WorldGen.SolidTile(headYAcceleration, headYTopSpeed); headYTopSpeed++)
                            continue;

                        headYTopSpeed--;
                        if (!WorldGen.SolidTile(headYAcceleration, headYTopSpeed))
                        {
                            NPC.NewNPCAction(SourceAI, new Vector2(headYAcceleration * 16 + 8, headYTopSpeed * 16), NPCID.DarkCaster);
                            break;
                        }
                    }
                }

                if (!NPC.AnyNPCs(NPCID.DiabolistWhite))
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        int headYAcceleration = (int)(NPC.Center.X / 16f) + Main.rand.Next(-50, 51);
                        int headYTopSpeed;
                        for (headYTopSpeed = (int)(NPC.Center.Y / 16f) + Main.rand.Next(-50, 51); headYTopSpeed < Main.maxTilesY - 10 && !WorldGen.SolidTile(headYAcceleration, headYTopSpeed); headYTopSpeed++)
                            continue;

                        headYTopSpeed--;
                        if (!WorldGen.SolidTile(headYAcceleration, headYTopSpeed))
                        {
                            NPC.NewNPCAction(SourceAI, new Vector2(headYAcceleration * 16 + 8, headYTopSpeed * 16), NPCID.DiabolistWhite);

                            break;
                        }
                    }
                }
            }

            NPC.defense = NPC.defDefense - 10;
            NPC.damage = (int)Math.Round(NPC.defDamage * SpinDamageMult);

            Timer++;

            AttackTimer += 1f;
            if (AttackTimer == 2f)
                SoundEngine.PlaySound(roar, NPC.Center);
        }

        void FadeChargeTelegraph()
        {
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
        }

        void FinishSpin()
        {
            if (Aroma)
            {
                if (TONetworkEnvironment.NotClient && NPC.CountNPCS(NPCID.DarkCaster) < 6)
                {
                    for (int j = 0; j < 1000; j++)
                    {
                        int headYAcceleration = (int)(NPC.Center.X / 16f) + Main.rand.Next(-50, 51);
                        int headYTopSpeed;
                        for (headYTopSpeed = (int)(NPC.Center.Y / 16f) + Main.rand.Next(-50, 51); headYTopSpeed < Main.maxTilesY - 10 && !WorldGen.SolidTile(headYAcceleration, headYTopSpeed); headYTopSpeed++)
                            continue;

                        headYTopSpeed--;
                        if (!WorldGen.SolidTile(headYAcceleration, headYTopSpeed))
                        {
                            NPC.NewNPCAction(SourceAI, new Vector2(headYAcceleration * 16 + 8, headYTopSpeed * 16), NPCID.DarkCaster);
                            break;
                        }
                    }
                }
            }

            // Start counting the alternating spins from the next complete hover round.
            if (handsDead)
            {
                if (CurrentPhase == Phase.Phase1)
                    CurrentPhase = Phase.Phase2;
                AdvancePhase2Subphase();
                HoverLifeRatio = MathHelper.Clamp((float)NPC.LifeRatio, 0f, 1f);
                RapidSkullDuration = (int)MathF.Round(MathHelper.Lerp(MinRapidSkullDuration, MaxRapidSkullDuration, NPC.LifeRatio));
            }

            CompletedCharges = 0;
            Timer = 0f;
            CurrentBehavior = Behavior.Hover;
            CurrentLocalPhase = 0;
            ChargeReadiness = 0f;
            ChargeTelegraphTimer = 0f;
            AttackTimer = -PostSpinHoverQuietDuration;

            NPC.TargetClosest();
            NPC.netUpdate = true;
            NPC.rotation += NPC.direction * SpinSpeed;
        }

        void AdvancePhase2Subphase()
        {
            if (ShouldEnterPhase2_2)
                CurrentPhase = Phase.Phase2_2;
        }

        void PhaseChange_2To3AI()
        {
            NPC.damage = 0;
            NPC.dontTakeDamage = true;
            NPC.chaseable = false;
            NPC.rotation = NPC.rotation.AngleTowards(0f, MathHelper.Pi / Phase3StillDuration);

            switch (CurrentLocalPhase)
            {
                case 0: // 静止，随后开始向玩家上方移动。
                    NPC.velocity = Vector2.Zero;
                    if (++Timer >= Phase3StillDuration && TONetworkEnvironment.NotClient)
                    {
                        CurrentLocalPhase = 1;
                        Timer = 0f;
                    }
                    break;
                case 1: // 追踪玩家上方，刹停前由服务器锁定停靠点。
                    Vector2 destination = Target.Center - Vector2.UnitY * Phase3ApproachHeight;
                    Vector2 desiredVelocity = (destination - NPC.Center) * 0.2f;
                    float speed = desiredVelocity.Length();
                    if (speed > Phase3ApproachSpeed)
                        desiredVelocity *= Phase3ApproachSpeed / speed;
                    NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVelocity, 0.3f);
                    if (++Timer >= Phase3ApproachDuration && TONetworkEnvironment.NotClient)
                    {
                        SetBrakeVector(0, NPC.Center + NPC.velocity);
                        SetBrakeVector(1, destination);
                        SetBrakeVector(2, NPC.velocity);
                        CurrentLocalPhase = 2;
                        Timer = 0f;
                    }
                    break;
                case 2: // 保留进入刹停的速度，抵达停靠点时切线归零。
                    if (Timer < Phase3BrakeDuration)
                    {
                        Vector2 position = Vector2.Hermite(
                            AnomalyNPC.AnomalyAI64[0].GetValue<Vector2>(),
                            AnomalyNPC.AnomalyAI64[2].GetValue<Vector2>() * Phase3BrakeDuration,
                            AnomalyNPC.AnomalyAI64[1].GetValue<Vector2>(), Vector2.Zero, ++Timer / Phase3BrakeDuration);
                        NPC.velocity = position - NPC.Center;
                        break;
                    }

                    NPC.velocity = Vector2.Zero;
                    if (!TONetworkEnvironment.NotClient)
                        break;

                    Projectile.NewProjectileAction<SkeletronPhase3Arena>(SourceAI, Target.Center,
                        Vector2.Zero, SkeletronBoneArena.BoundaryDamage, 0f, action: arena =>
                        {
                            SkeletronPhase3Arena field = arena.GetModProjectile<SkeletronPhase3Arena>();
                            field.Master = NPC;
                            field.IsActivated = true;
                            CurrentLocalPhase = 3;
                        });
                    if (CurrentLocalPhase == 3)
                        goto case 3;
                    break;
                case 3: // 竞技场已生成，释放冲击波后才开始回血。
                    NPC.velocity = Vector2.Zero;
                    if (TONetworkEnvironment.NotClient)
                    {
                        Projectile.NewProjectileAction<BoneShockwave>(SourceAI, NPC.Center, Vector2.Zero, 0, 0f, action: p =>
                        {
                            p.ai[0] = BoneShockwave.PhaseChangeVariant;
                            p.timeLeft = p.GetModProjectile<BoneShockwave>().LifeTime;
                            p.scale = 0f;
                            SetBrakeVector(0, NPC.Center);
                            CurrentLocalPhase = 4;
                            Timer = 0f;
                        });
                    }
                    break;
                case 4: // 冲击波后稍作停留，缓慢移至竞技场中心，同时回血。
                    NPC.velocity = Vector2.Zero;
                    Timer++;
                    if (Timer > Phase3CenterMoveDelay)
                    {
                        foreach (Projectile projectile in Projectile.ActiveProjectiles)
                        {
                            if (projectile.ModProjectile is SkeletronPhase3Arena arena && arena.Master == NPC && arena.IsActivated)
                            {
                                float progress = MathHelper.Clamp((Timer - Phase3CenterMoveDelay) / Phase3CenterMoveDuration, 0f, 1f);
                                Vector2 position = Vector2.SmoothStep(AnomalyNPC.AnomalyAI64[0].GetValue<Vector2>(),
                                    arena.ArenaPolygon.Center, progress);
                                NPC.velocity = position - NPC.Center;
                                break;
                            }
                        }
                    }
                    if (!TONetworkEnvironment.NotClient)
                        break;

                    if (Timer % Phase3HealInterval == 0f || Timer >= Phase3HealDuration)
                        HealDuringTransition();

                    if (Timer >= Phase3CenterMoveDelay + Phase3CenterMoveDuration)
                    {
                        CurrentPhase = Phase.Phase3;
                        CurrentBehavior = Behavior.Phase3_NodeVolley;
                        Phase3Cycle = 0;
                        CurrentLocalPhase = 0;
                        Timer = 0f;
                    }
                    break;
            }

            void HealDuringTransition()
            {
                int targetLife = (int)(NPC.lifeMax * Phase3HealLifeRatio);
                float progress = MathHelper.Clamp(Timer / Phase3HealDuration, 0f, 1f);
                int newLife = progress >= 1f ? targetLife : (int)MathHelper.Lerp(
                    Phase3HealStartLife, targetLife, TOMathUtils.Interpolation.LogarithmicEaseOut(progress));
                int increasedLife = newLife - NPC.life;
                if (increasedLife <= 0)
                    return;

                NPC.life = newLife;
                NPC.HealEffect(increasedLife, true);
                NPC.netUpdate = true;
            }

            void SetBrakeVector(int slot, Vector2 value)
            {
                AnomalyNPC.AnomalyAI64[slot].SetValue(value);
                AnomalyNPC.AIChanged64[slot] = true;
            }
        }

        void Enraged()
        {
            NPC.damage = 1000;
            AnomalyNPC.ExtraDR = 0.9999f;
            AnomalyNPC.DR = 0f;

            AnomalyNPC.BossBar_IncreasingDefenseOrDR = true;
            SkeletronHandler.
                        SpinMovement(NPC, Target, SpinSpeed, 8f);
        }

        void Despawn()
        {
            UpdateHandProtection();
            AnomalyNPC.DR = 0f;
            AnomalyNPC.ExtraDR = 0f;
            AnomalyNPC.BossBar_IncreasingDefenseOrDR = false;

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
