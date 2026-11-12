using CalamityMod.UI.DialogueDisplay.TextEffects;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class SkeletronLegendHead
{
    #region 行为分派与符卡抽选

    private void Phase1AI()
    {
        switch (CurrentBehavior)
        {
            case Behavior.None:
            default:
                CurrentBehavior = Behavior.Normal;
                break;
            case Behavior.Normal:
                Phase1NormalAI();
                break;
            case Behavior.Spin:
                Phase1SpinAI();
                break;
        }
    }

    private void SelectNextPhase1Spell()
    {
        if (!FirstSpellCycleCompleted)
        {
            CurrentSpell = Spell.None;
            return;
        }

        if (!TONetworkEnvironment.NotClient)
            return;

        Union32 pool = AI_Union_2;
        if (CurrentSpell == Spell.None)
        {
            CurrentSpell = Spell.PlatinumCoin;
            pool.Byte0 = 0b011;
        }
        else
            CurrentSpell = (Spell)DrawSpell((byte)CurrentSpell, ref pool.Byte0);
        AI_Union_2 = pool;
        NPC.netUpdate = true;

        static byte DrawSpell(byte previousSpell, ref byte remainingSpells)
        {
            const byte FullPool = 0b111;
            if (remainingSpells == 0)
                remainingSpells = FullPool;

            // 补池后排除上次符卡，但保留其池中位置，保证每组三种均会释放。
            int candidates = remainingSpells;
            if (previousSpell != 0)
                candidates &= ~(1 << (previousSpell - 1));

            int index = Main.rand.Next(System.Numerics.BitOperations.PopCount((uint)candidates));
            for (byte spell = 1; spell <= 3; spell++)
            {
                int bit = 1 << (spell - 1);
                if ((candidates & bit) != 0 && index-- == 0)
                {
                    remainingSpells = (byte)(remainingSpells & ~bit);
                    return spell;
                }
            }

            return 0;
        }
    }

    #endregion

    private void Phase1NormalAI()
    {
        #region 常态攻击与移动

        bool shouldUseNormalMovement = true;

        const int ShockwaveTime = 300;
        // 保留 Timer1 的累计帧数，供旋转结束后的钱币审判接续计时。
        int normalTimer = ++Timer1 - (FirstSpellCycleCompleted ? SkeletronHead.PostSpinHoverQuietDuration : 0);

        if (normalTimer % 30 == 0 && normalTimer is > 30 and <= ShockwaveTime)
            SkeletronCombat.FireSkull(NPC, Target, Main.rand.NextBool(3) ? 2 : 1, TOMathUtils.PiOver10, 20f, SkullDamage, addNPCVelocity: false);

        if (normalTimer == ShockwaveTime)
        {
            SelectNextPhase1Spell();

            SoundEngine.PlaySound(SoundID.ForceRoar);
            ReleaseSpellShockwave();
        }

        if (normalTimer > ShockwaveTime)
        {
            int adjustedTimer = normalTimer - ShockwaveTime;

            switch (CurrentSpell)
            {
                case Spell.None:
                    NormalNoneSpell(adjustedTimer);
                    break;
                case Spell.Slime:
                    NormalSlimeSpell(adjustedTimer);
                    break;
                case Spell.Star:
                    NormalStarSpell(adjustedTimer);
                    break;
                case Spell.PlatinumCoin:
                    shouldUseNormalMovement = false;
                    NormalPlatinumCoinSpell(adjustedTimer);
                    break;
            }
        }

        if (shouldUseNormalMovement)
        {
            float maxSpeedX = 17.5f;
            float maxSpeedY = 8f;
            float accelerationX = 0.175f;
            float accelerationY = 0.08f;
            float targetDistanceAboveTarget = 300f;
            float rotationFactor = 20f;
            SkeletronKinematics.NormalMovement(NPC, Target, maxSpeedX, maxSpeedY, accelerationX, accelerationY, targetDistanceAboveTarget, rotationFactor);
        }

        #endregion

        #region 常态符卡

        void NormalNoneSpell(int adjustedTimer)
        {
            const int Duration = 300;
            const int QuietDuration = 60;
            if (adjustedTimer < Duration - QuietDuration && adjustedTimer % 10 == 0)
                SkeletronCombat.FireSkull(NPC, Target, Main.rand.NextBool(3) ? 2 : 1, TOMathUtils.PiOver10, 20f, SkullDamage, addNPCVelocity: false);

            if (adjustedTimer >= Duration)
            {
                BeginPhase1Spin();
                if (TONetworkEnvironment.NotClient)
                    SkeletronCombat.FireAcceleratingSpread(NPC, Target, SkeletronHead.MaximumSpreadCount, SkeletronHead.MaximumSpreadDegrees,
                        (position, velocity) => Projectile.NewProjectileAction(SourceAI, position, velocity, ProjectileID.Skull,
                            SkullDamage, 0f, Main.myPlayer, action: p =>
                            {
                                Skull.InitializeFlight(p, Skull.Straight, Skull.MaxSpeed, Skull.Acceleration);
                                p.timeLeft = 600;
                            }));
            }
        }

        void NormalSlimeSpell(int adjustedTimer)
        {
            if (adjustedTimer is 100 or 150 or 200)
            {
                int num = adjustedTimer switch
                {
                    100 => 0,
                    150 => 1,
                    200 => 2,
                    _ => 0
                };

                SoundEngine.PlaySound(SoundID.Item81, NPC.Center);

                SlimeMount.SpawnSlimeMountDust(NPC.Center, 300, Vector2.Zero, d =>
                {
                    d.scale *= Main.rand.NextFloat(1f, 1.5f);
                    d.velocity *= Main.rand.NextFloat(10f, 20f);
                });

                if (TONetworkEnvironment.NotClient)
                {
                    int delay = 30;
                    float distance = 280f - num * 60f;
                    int amount = 50;

                    float y = Math.Min(Target.Center.Y - NPC.Center.Y, 0f) - 320f + Target.velocity.Y * Utils.Remap(-Target.velocity.Y, 5f, 20f, 0f, delay * 1.2f);
                    Vector2 left = new(-distance * ((amount - 1) / 2f), y);
                    Vector2 interval = new(distance, 0f);
                    for (int i = 0; i < amount; i++)
                    {
                        Vector2 destination = left + interval * i;
                        Vector2 velocity = destination / delay;
                        Projectile.NewProjectileAction<SlimeMount>(SourceAI, NPC.Center, velocity, SlimeMountDamage, 0f, action: p =>
                        {
                            p.ai[0] = delay;
                            p.ai[2] = NPC.target + 1;
                            p.netUpdate = true;
                        });
                    }
                }
            }

            if (adjustedTimer >= 300)
            {
                ResumePhase1Spin();
            }
        }

        void NormalStarSpell(int adjustedTimer)
        {
            SummonStarMeteors(adjustedTimer);

            if (adjustedTimer >= 560)
            {
                ResumePhase1Spin();
            }
        }

        void NormalPlatinumCoinSpell(int adjustedTimer)
        {
            const int BufferDuration = 60;
            // 只推迟竞技场和攻击时间轴；冲击波后的减速仍立即生效。
            adjustedTimer -= BufferDuration;

            const int RainStart = 60;
            const int RainEnd = 420; // 安全区在此到达最后一个极值，转向后不再生成金币。
            const int SpinDelay = 90;
            const int RainInterval = 12;
            const float FallSpeed = 16f;
            const float ColumnSpacing = 40f;
            const float SafeZoneSpacing = 640f;
            const float SafeZoneWidth = 240f;

            if (ArenaProjectileAlive)
                ArenaModProjectile.IsActivated = adjustedTimer is >= 0 and <= RainEnd;

            StopMovement();
            NPC.rotation *= 0.9f;

            if (TONetworkEnvironment.NotClient)
            {
                if (ArenaProjectileAlive && adjustedTimer >= RainStart && adjustedTimer <= RainEnd && adjustedTimer % RainInterval == 0)
                {
                    // 左半边保持原有平移和缺列逻辑，右半边由左半边镜像生成。
                    // 已生成的金币始终竖直匀速下落，不追踪玩家或改变速度。
                    float offset = 250f * MathF.Sin(MathHelper.TwoPi * (adjustedTimer - RainStart) / 480f);
                    float horizontalRange = ArenaModProjectile.Radius * 1.2f;

                    // 只枚举平移后位于左半边的列，再沿竞技场中轴镜像。
                    int firstColumn = (int)MathF.Ceiling((-horizontalRange - offset) / ColumnSpacing);
                    int lastColumn = (int)MathF.Floor(-offset / ColumnSpacing);
                    for (int column = firstColumn; column <= lastColumn; column++)
                    {
                        float x = column * ColumnSpacing;
                        float distanceToSafeZone = Math.Abs(MathF.IEEERemainder(x, SafeZoneSpacing));
                        if (distanceToSafeZone < SafeZoneWidth * 0.5f)
                            continue;

                        float leftX = x + offset;
                        SpawnCoin(leftX);
                        if (leftX < 0f)
                            SpawnCoin(-leftX);
                    }

                    void SpawnCoin(float horizontalOffset)
                    {
                        Vector2 position = NPC.Center + new Vector2(horizontalOffset, -1200f);
                        Projectile.NewProjectileAction<PlatinumCoin>(SourceAI, position, Vector2.UnitY * FallSpeed, PlatinumCoinDamage, 0f, action: p =>
                        {
                            p.ai[0] = NPC.target + 1;
                            p.timeLeft = 150;
                        });
                    }
                }

                // 金币雨落到玩家附近后开始自机狙
                if (adjustedTimer >= 150 && adjustedTimer <= 420 && (adjustedTimer - 150) % 30 == 0)
                    SkeletronCombat.FireSkull(NPC, Target, 5, MathHelper.TwoPi / 5f, 15f, SkullDamage, addNPCVelocity: false);
            }

            if (adjustedTimer >= RainEnd + SpinDelay)
            {
                BeginPhase1Spin();
            }
        }

        #endregion

        #region 切换至自转行为

        // 史莱姆和落星常态符卡沿用已有局部步骤，仅重置行为计时。
        void ResumePhase1Spin()
        {
            CurrentBehavior = Behavior.Spin;
            Timer1 = 0;
        }

        void BeginPhase1Spin()
        {
            CurrentBehavior = Behavior.Spin;
            CurrentLocalPhase = 0;
            Timer1 = 0;
            NPC.netUpdate = true;
        }

        #endregion
    }

    private void Phase1SpinAI()
    {
        #region 自转入场与持续动作

        switch (CurrentLocalPhase)
        {
            case 0:
                BeginSpin();
                break;
            case 1:
                UpdateSpin();
                break;
        }

        void BeginSpin()
        {
            SoundEngine.PlaySound(SoundID.ForceRoar);
            ReleaseSpellShockwave();
            NPC.Face(Target);
            Timer1 = 0;
            if (CurrentSpell == Spell.Star && TONetworkEnvironment.NotClient)
            {
                Projectile.NewProjectileAction<DemonShadow>(SourceAI, NPC.Center + new Vector2(180f * NPC.direction, -160f),
                    Vector2.Zero, 0, 0f, action: p =>
                    {
                        p.ai[0] = NPC.target + 1;
                        p.ai[2] = NPC.whoAmI + 1;
                        p.netUpdate = true;
                    });
            }
            if (CurrentSpell == Spell.PlatinumCoin && TONetworkEnvironment.NotClient)
            {
                Projectile.NewProjectileAction<CoinRoulette>(SourceAI, Target.Center, Vector2.Zero, 0, 0f, action: p =>
                {
                    p.ai[0] = NPC.target + 1;
                    p.ai[2] = Main.rand.Next(4);
                    CoinRoulette roulette = p.GetModProjectile<CoinRoulette>();
                    roulette.MasterIndex = NPC.whoAmI + 1;
                    p.netUpdate = true;
                });
            }
            CurrentLocalPhase = 1;
            NPC.netUpdate = true;
        }

        void UpdateSpin()
        {
            float spinSpeed = 0.3f;
            float distance = NPC.Distance(Target.Center);
            float moveSpeed = SkeletronKinematics.GetSpinMoveSpeed(distance);
            SkeletronKinematics.SpinMovement(NPC, Target, spinSpeed, moveSpeed);

            Timer1++;

            // 各自转符卡的额外攻击。
            switch (CurrentSpell)
            {
                case Spell.None:
                    SpinNoneSpell();
                    break;
                case Spell.Slime:
                    SpinSlimeSpell();
                    break;
                case Spell.Star:
                    SpinStarSpell();
                    break;
                case Spell.PlatinumCoin:
                    SpinPlatinumCoinSpell();
                    break;
            }
        }

        #endregion

        #region 自转符卡与收尾

        void ApplyAvatarOpacity(float targetOpacity) => NPC.Opacity = MathHelper.Lerp(1f, targetOpacity, MathHelper.Clamp(Timer1 / 10f, 0f, 1f));

        void SpinNoneSpell()
        {
            if (Timer1 % 60 == 0 && Timer1 <= 360)
                SummonSpinSkullWarnings(6);

            if (Timer1 >= SkeletronHead.EnhancedSpinDuration + SkeletronHead.WarningSpinQuietDuration)
            {
                CompletePhase1Spin();
            }
        }

        void SpinSlimeSpell()
        {
            ApplyAvatarOpacity(0.5f);

            if (TONetworkEnvironment.NotClient)
            {
                // 每轮生成一个立即下落的坐骑，并同步绘制延伸预警。
                if (Timer1 >= 30 && Timer1 <= 450 && (Timer1 - 30) % 20 == 0)
                {
                    Vector2 position = Target.Center + new Vector2(Main.rand.NextFloat(-1200f, 1200f), Main.rand.NextFloat(-1200f, -1000f));
                    Projectile.NewProjectileAction<SlimeMount>(SourceAI, position, Vector2.UnitY * 0.75f, SlimeMountDamage, 0f, action: p =>
                    {
                        p.ai[1] = 1f;
                        p.ai[2] = NPC.target + 1;
                        p.GetModProjectile<SlimeMount>().MasterIndex = NPC.whoAmI + 1;
                        p.netUpdate = true;
                    });
                }

                if (Timer1 >= 90 && Timer1 <= 450 && (Timer1 - 90) % 40 == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item155, NPC.Center);

                    SlimeMount.SpawnSlimeMountDust(NPC.Center, 150, Vector2.Zero, d =>
                    {
                        d.scale *= Main.rand.NextFloat(1f, 1.5f);
                        d.velocity *= Main.rand.NextFloat(7f, 15f);
                    });

                    int amount = Main.rand.Next(1, 3);
                    for (int i = 0; i < amount; i++)
                    {
                        Vector2 position = NPC.Center + new Vector2((i == 0 ? -1f : 1f) * 110f, -60f);
                        NPC.NewNPCAction<FlyingSlime>(SourceAI, position, action: n =>
                        {
                            n.Center = position;
                            n.Master = NPC;
                            n.target = NPC.target;
                            n.scale = 1.5f;
                            n.netUpdate = true;
                        });
                    }
                }
            }

            if (Timer1 >= 540)
            {
                CompletePhase1Spin();
            }
        }

        void SpinStarSpell()
        {
            ApplyAvatarOpacity(0.5f);
            const int AttackInterval = 120;
            const int StarCount = 25;
            if (Timer1 < 500 && Timer1 % AttackInterval == 0)
            {
                SoundEngine.PlaySound(SoundID.Item9, NPC.Center);
                if (TONetworkEnvironment.NotClient)
                {
                    // 与陨石爆炸相同：隔点连接五个顶点，在五角星的五条边上插值速度。
                    Vector2 velocityTowardsTarget = NPC.GetVelocityTowards(Target, 15f);
                    if (velocityTowardsTarget == Vector2.Zero)
                        velocityTowardsTarget = -Vector2.UnitY * 10f;
                    PolarVector2 originalVelocity = (PolarVector2)velocityTowardsTarget;
                    List<Vector2> vertices = [];
                    for (int i = 0; i < 5; i++)
                        vertices.Add(originalVelocity.RotatedBy(TOMathUtils.PiOver5 * 4 * i));
                    vertices.Add(originalVelocity);

                    for (int i = 0; i < StarCount; i++)
                    {
                        Vector2 velocity = Vector2.LerpMany(vertices, (float)i / StarCount);
                        Projectile.NewProjectileAction<FallingStar>(SourceAI, NPC.Center, velocity, FallingStarDamage, 0f, action: p =>
                        {
                            p.scale = 0.2f;
                            p.timeLeft = 480;
                            p.GetModProjectile<FallingStar>().TrailType = 2;
                            p.GetModProjectile<FallingStar>().AccelerationStartTime = 150;
                            p.netUpdate = true;
                        });
                    }
                }
            }

            if (Timer1 >= 600)
            {
                CompletePhase1Spin();
            }
        }

        void SpinPlatinumCoinSpell()
        {
            ApplyAvatarOpacity(0.5f);
            if (Timer1 <= 420 && Timer1 % 6 == 0 && !Main.dedServ)
                SoundEngine.PlaySound(PlatinumCoin.UnlimitedTossSound, NPC.Center);

            if (TONetworkEnvironment.NotClient)
            {
                if (Timer1 <= 420 && Timer1 % 6 == 0)
                {
                    for (int side = -1; side <= 1; side += 2)
                    {
                        // 两侧独立随机出射点与初速，避免整齐成对的抛物线。
                        Vector2 position = NPC.Center + Main.rand.NextPolarVector2(10f);
                        Vector2 velocity = new(side * Main.rand.NextFloat(4f), Main.rand.NextFloat(-13f, -5f));
                        Projectile.NewProjectileAction<PlatinumCoin>(SourceAI, position, velocity, PlatinumCoinDamage, 0f, action: p =>
                        {
                            p.ai[0] = NPC.target + 1;
                            p.ai[1] = 0.22f;
                            p.timeLeft = 240;
                        });
                    }
                }
            }

            if (Timer1 >= CoinRoulette.RotationDuration)
            {
                CompletePhase1Spin();
            }
        }

        void CompletePhase1Spin()
        {
            FirstSpellCycleCompleted = true;
            CurrentBehavior = Behavior.Normal;
            CurrentLocalPhase = 0;
            Timer1 = 0;
            NPC.netUpdate = true;
        }

        #endregion
    }
}
