namespace Anomalies.Bosses.Skeletron;

public sealed partial class SkeletronHead
{
    private void Phase2AI(bool handsDead)
    {
        AdvancePhase2Subphase();

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
                        FireHoverSkulls(SkullInterval, handsDead);

                    if (releaseShockwave)
                        Projectile.NewProjectileAction<BoneShockwave>(SourceAI, NPC.Center, Vector2.Zero, 0, 0f, action: p => p.scale = 0f);
                    break;
                case 2: // 快速射击，收尾静默期停止发射
                    if (AttackTimer < shockwaveTime + RapidSkullDuration - QuietDuration)
                        FireHoverSkulls(EnhancedSkullInterval, handsDead);
                    break;
            }

            int hoverDuration = shockwaveTime + RapidSkullDuration;
            if (AttackTimer > 0f)
                Timer = ChargeGateValue * AttackTimer / hoverDuration;
            ChargeTelegraphTimer = Math.Min(Timer, ChargeGateValue);

            if (TONetworkEnvironment.NotClient && AttackTimer >= hoverDuration)
            {
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
                FinishSpin(handsDead);
            else
                SkeletronKinematics.SpinMovement(NPC, Target, SpinSpeed, SkeletronKinematics.GetSpinMoveSpeed(NPC.Distance(Target.Center), false));

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
                            FinishSpin(handsDead);
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
                float speed = SkeletronKinematics.GetSpinMoveSpeed(distance, false) * ChargeTrackingSpeedMultiplier;
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
                                Skull.InitializeFlight(p, mode, maxSpeed, Skull.Acceleration);
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
}
