using Anomalies.Bosses.Skeletron.LegendaryNight.Entities;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class SkeletronLegendHead
{
    public void Phase1AI()
    {
        switch (CurrentBaseBehavior)
        {
            case BaseBehavior.None:
            default:
                CurrentBaseBehavior = BaseBehavior.Normal;
                break;
            case BaseBehavior.Normal:
                Normal();
                break;
            case BaseBehavior.Rotate:
                Rotate();
                break;
        }

        void SelectNextSpell()
        {
            if (!FirstSpellCycleCompleted)
            {
                CurrentSpell = Spell.None;
                return;
            }

            if (!TOSharedData.NotClient)
                return;

            Union32 pool = AI_Union_2;
            CurrentSpell = (Spell)DrawSpell((byte)CurrentSpell, ref pool.byte0);
            AI_Union_2 = pool;
            CurrentSpell = Spell.PlatinumCoin;
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

        void Normal()
        {
            bool shouldUseNormalMovement = true;

            Timer1++;

            if (Timer1 % 30 == 0 && Timer1 is > 30 and <= 300)
                SkeletronHead.FireSkull(NPC, Target, Main.rand.NextBool(3) ? 2 : 1, TOMathUtils.PiOver10, 20f, SkullDamage, addNPCVelocity: false);

            if (Timer1 == 300)
            {
                SelectNextSpell();

                SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
                ReleaseSpellShockwave();

                SkeletronHead.FireSkull(NPC, Target, 16, TOMathUtils.PiOver8, 15f, SkullDamage, ai0: 1);
            }

            if (Timer1 > 300)
            {
                int adjustedTimer = Timer1 - 300;

                switch (CurrentSpell)
                {
                    case Spell.None:
                        NormalSpell_None(adjustedTimer, ref shouldUseNormalMovement);
                        break;
                    case Spell.Slime:
                        NormalSpell_Slime(adjustedTimer, ref shouldUseNormalMovement);
                        break;
                    case Spell.Star:
                        NormalSpell_Star(adjustedTimer, ref shouldUseNormalMovement);
                        break;
                    case Spell.PlatinumCoin:
                        NormalSpell_PlatinumCoin(adjustedTimer, ref shouldUseNormalMovement);
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

                SkeletronHead.NormalMovement(NPC, Target, maxSpeedX, maxSpeedY, accelerationX, accelerationY, targetDistanceAboveTarget, rotationFactor);
            }

            void NormalSpell_None(int adjustedTimer, ref bool shouldUseNormalMovement)
            {
                if (adjustedTimer % 10 == 0)
                    SkeletronHead.FireSkull(NPC, Target, Main.rand.NextBool(3) ? 2 : 1, TOMathUtils.PiOver10, 20f, SkullDamage, addNPCVelocity: false);

                if (adjustedTimer >= 300)
                {
                    CurrentBaseBehavior = BaseBehavior.Rotate;
                    CurrentLocalPhase = 0;
                    Timer1 = 0;
                    NPC.netUpdate = true;
                }
            }

            void NormalSpell_Slime(int adjustedTimer, ref bool shouldUseNormalMovement)
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

                    if (TOSharedData.NotClient)
                    {
                        int delay = 30;
                        float distance = 300f - num * 35f;
                        int amount = 40;

                        float y = Math.Min(Target.Center.Y - NPC.Center.Y, 0f) - 160f + Target.velocity.Y * Utils.Remap(-Target.velocity.Y, 5f, 20f, 0f, delay * 1.2f);
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
                    CurrentBaseBehavior = BaseBehavior.Rotate;
                    Timer1 = 0;
                }
            }

            void NormalSpell_Star(int adjustedTimer, ref bool shouldUseNormalMovement)
            {
                SummonStarMeteors(adjustedTimer);

                if (adjustedTimer >= 560)
                {
                    CurrentBaseBehavior = BaseBehavior.Rotate;
                    Timer1 = 0;
                }
            }

            void NormalSpell_PlatinumCoin(int adjustedTimer, ref bool shouldUseNormalMovement)
            {
                const int BufferDuration = 60;
                // 只推迟竞技场和攻击时间轴；冲击波后的减速仍立即生效。
                adjustedTimer -= BufferDuration;
                if (ArenaProjectileAlive)
                    ArenaModProjectile.IsActivated = adjustedTimer is >= 0 and < 510;

                const int RainStart = 60;
                const int RainEnd = 480;
                const int RainInterval = 12;
                const float FallSpeed = 16f;
                const float ColumnSpacing = 40f;
                const float SafeZoneSpacing = 640f;
                const float SafeZoneWidth = 240f;

                shouldUseNormalMovement = false;
                StopMovement();
                NPC.rotation *= 0.9f;

                if (TOSharedData.NotClient)
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

                    // 金币雨落到玩家附近后开始低频自机狙，最后以一圈散射收尾。
                    if (adjustedTimer >= 150 && adjustedTimer <= 420 && (adjustedTimer - 150) % 90 == 0)
                        SkeletronHead.FireSkull(NPC, Target, 1, 0f, 15f, SkullDamage, addNPCVelocity: false);
                }

                if (adjustedTimer >= 510)
                {
                    CurrentBaseBehavior = BaseBehavior.Rotate;
                    CurrentLocalPhase = 0;
                    Timer1 = 0;
                    NPC.netUpdate = true;
                }
            }
        }

        void Rotate()
        {
            switch (CurrentLocalPhase)
            {
                case 0:
                    SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
                    ReleaseSpellShockwave();
                    NPC.Face(Target);
                    Timer1 = 0;
                    if (CurrentSpell == Spell.Star && TOSharedData.NotClient)
                    {
                        Projectile.NewProjectileAction<DemonShadow>(SourceAI, NPC.Center + new Vector2(180f * NPC.direction, -160f),
                            Vector2.Zero, 0, 0f, action: p =>
                            {
                                p.ai[0] = NPC.target + 1;
                                p.ai[2] = NPC.whoAmI + 1;
                                p.netUpdate = true;
                            });
                    }
                    CurrentLocalPhase = 1;
                    NPC.netUpdate = true;
                    break;
                case 1:
                    float rotationSpeed = 0.3f;
                    float moveSpeed = 9f;
                    SkeletronHead.RotatingMovement(NPC, Target, rotationSpeed, moveSpeed);

                    Timer1++;

                    // 各旋转符卡的额外攻击。
                    switch (CurrentSpell)
                    {
                        case Spell.None:
                            RotatingSpell_None();
                            break;
                        case Spell.Slime:
                            RotatingSpell_Slime();
                            break;
                        case Spell.Star:
                            RotatingSpell_Star();
                            break;
                        case Spell.PlatinumCoin:
                            RotatingSpell_PlatinumCoin();
                            break;
                    }
                    break;
            }

            void ApplyAvatarOpacity(float targetOpacity)
            {
                NPC.Opacity = MathHelper.Lerp(1f, targetOpacity, MathHelper.Clamp(Timer1 / 10f, 0f, 1f));
            }

            void RotatingSpell_None()
            {
                SummonRotatingSkullWarnings(Timer1);

                if (Timer1 >= 420)
                {
                    FirstSpellCycleCompleted = true;
                    CurrentBaseBehavior = BaseBehavior.Normal;
                    CurrentLocalPhase = 0;
                    Timer1 = 0;
                    NPC.netUpdate = true;
                }
            }

            void RotatingSpell_Slime()
            {
                ApplyAvatarOpacity(0.5f);

                if (TOSharedData.NotClient)
                {
                    // 每轮生成一个立即下落的坐骑，并同步绘制延伸预警。
                    if (Timer1 >= 30 && Timer1 <= 450 && (Timer1 - 30) % 20 == 0)
                    {
                        Vector2 position = Target.Center + new Vector2(Main.rand.NextFloat(-1200f, 1200f), -1000f);
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
                    FirstSpellCycleCompleted = true;
                    CurrentBaseBehavior = BaseBehavior.Normal;
                    CurrentLocalPhase = 0;
                    Timer1 = 0;
                    NPC.netUpdate = true;
                }
            }

            void RotatingSpell_Star()
            {
                ApplyAvatarOpacity(0.5f);
                const int AttackInterval = 120;
                const int StarCount = 25;
                if (Timer1 < 500 && Timer1 % AttackInterval == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item9, NPC.Center);
                    if (TOSharedData.NotClient)
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
                                p.GetModProjectile<FallingStar>().AccelerationStartTime = 240;
                                p.netUpdate = true;
                            });
                        }
                    }
                }

                if (Timer1 >= 600)
                {
                    FirstSpellCycleCompleted = true;
                    NPC.netUpdate = true;
                    CurrentBaseBehavior = BaseBehavior.Normal;
                    CurrentLocalPhase = 0;
                    Timer1 = 0;
                }
            }

            void RotatingSpell_PlatinumCoin()
            {
                ApplyAvatarOpacity(0.5f);
                if (Timer1 <= 420 && Timer1 % 6 == 0 && !Main.dedServ)
                    SoundEngine.PlaySound(PlatinumCoin.UnlimitedTossSound, NPC.Center);

                if (TOSharedData.NotClient)
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

                    if (Timer1 == 30)
                    {
                        Projectile.NewProjectileAction<CoinRoulette>(SourceAI, Target.Center, Vector2.Zero, 0, 0f, action: p =>
                        {
                            p.ai[0] = NPC.target + 1;
                            p.ai[2] = Main.rand.Next(4);
                            CoinRoulette roulette = p.GetModProjectile<CoinRoulette>();
                            roulette.MasterIndex = NPC.whoAmI + 1;
                            roulette.LockTime = Main.rand.Next(5, 9) * CoinRoulette.CycleTime;
                            p.netUpdate = true;
                        });
                    }
                }

                if (Timer1 >= 480)
                {
                    FirstSpellCycleCompleted = true;
                    CurrentBaseBehavior = BaseBehavior.Normal;
                    CurrentLocalPhase = 0;
                    Timer1 = 0;
                    NPC.netUpdate = true;
                }
            }
        }
    }

    private void ReleaseSpellShockwave(Spell? spell = null)
    {
        if (!TOSharedData.NotClient)
            return;

        Projectile.NewProjectileAction<LegendShockwave>(SourceAI, NPC.Center, Vector2.Zero, 0, 0f, action: p =>
        {
            p.scale = 0f;
            p.ai[0] = (spell ?? CurrentSpell) switch
            {
                Spell.Slime => 1f,
                Spell.Star => 2f,
                Spell.PlatinumCoin => 3f,
                _ => 0f
            };
            p.netUpdate = true;
        });
    }

    private void SummonStarMeteors(int timer, bool reducedStarCount = false)
    {
        const int AttackInterval = 120;
        const int MeteorCount = 3;
        const float MeteorSpeed = 20f;
        const int SummonSoundLeadTime = 10;

        int summonTimer = timer + SummonSoundLeadTime;
        if (!Main.dedServ && summonTimer >= AttackInterval && summonTimer <= AttackInterval * MeteorCount && summonTimer % AttackInterval == 0)
            SoundEngine.PlaySound(Meteor.SummonSound, Target.Center);

        if (timer >= AttackInterval && timer <= AttackInterval * MeteorCount && timer % AttackInterval == 0)
        {
            if (TOSharedData.NotClient)
            {
                Vector2 spawnPosition = Target.Center + new Vector2(Main.rand.NextFloat(-500f, 500f), -1000f);
                Vector2 velocity = Target.GetVelocityTowards(spawnPosition, -MeteorSpeed);
                Projectile.NewProjectileAction<Meteor>(SourceAI, spawnPosition, velocity, MeteorDamage, 0f, action: p =>
                {
                    p.ai[0] = NPC.target + 1;
                    p.ai[2] = NPC.whoAmI + 1;
                    p.GetModProjectile<Meteor>().ReducedAttack = reducedStarCount;
                });
            }
        }

    }

    private void SummonRotatingSkullWarnings(int timer)
    {
        if (TOSharedData.NotClient && timer % 75 == 0 && timer <= 300)
        {
            const int SkullCount = 4;
            float offset = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < SkullCount; i++)
            {
                Vector2 position = Target.Center + new Vector2(Main.rand.NextFloat(800f, 1000f), 0f)
                    .RotatedBy(offset + MathHelper.TwoPi * i / SkullCount);
                Projectile.NewProjectileAction<SkullWarning>(SourceAI, position, Vector2.Zero, SkullDamage, 0f, action: p =>
                {
                    p.ai[0] = NPC.target + 1;
                    p.ai[2] = NPC.whoAmI + 1;
                    p.netUpdate = true;
                });
            }
        }

    }
}
