namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class SkeletronLegendHead
{
    // 6：三次拍击的随机左右侧位图（bit3 标记已初始化）；7：本轮机制的音乐起点。
    public int Mechanic2HandPattern
    {
        get => AnomalyNPC.AnomalyAI32[6].I;
        private set
        {
            AnomalyNPC.AnomalyAI32[6].I = value;
            AnomalyNPC.AIChanged32[6] = true;
        }
    }

    public int Mechanic2StartMusicTime
    {
        get => AnomalyNPC.AnomalyAI32[7].I;
        private set
        {
            AnomalyNPC.AnomalyAI32[7].I = value;
            AnomalyNPC.AIChanged32[7] = true;
        }
    }

    public int Mechanic2Age => Phase2MusicElapsedTime - Mechanic2StartMusicTime;
    public bool Mechanic3Active => CurrentPhase == Phase.Phase2_3;
    public bool Mechanic4Active => CurrentPhase == Phase.Phase2_4;
    public bool Mechanic5Active => CurrentPhase == Phase.Phase2_5;
    public bool Phase2CoinAvatarActive => Mechanic4Active;
    public bool StarburstActive => Mechanic2Active || Mechanic3Active || Mechanic4Active || Mechanic5Active;
    public bool Phase2SlimeAvatarActive => Mechanic3Active
        && Phase2MusicElapsedTime >= Mechanic3Data.StartMusicTime
        && Phase2MusicElapsedTime < Mechanic3Data.LandingTime;
    public bool Phase2SlimeJumpActive => Phase2SlimeAvatarActive && Phase2MusicElapsedTime >= Mechanic3Data.JumpTime;

    public bool Mechanic2Active => CurrentPhase == Phase.Phase2_2;
    public bool Mechanic2InBackground => Mechanic2Active && Mechanic2Age < Mechanic2Data.ReturnStart + Mechanic2Data.ReturnDuration;
    public int Mechanic2StrikeSide(int strike) => (Mechanic2HandPattern & (1 << strike)) == 0 ? -1 : 1;
    public LegendaryNightPhase2Arena Phase2Arena => Phase2ArenaProjectileAlive ? ArenaProjectile.GetModProjectile<LegendaryNightPhase2Arena>() : null;

    // Phase2_1 的 LocalPhase：0 旋转，1 常态星星雨，2 史莱姆，3 叠加恶魔之影，4 再叠加铂金币。
    public bool Mechanic1Rotating => CurrentPhase == Phase.Phase2_1 && CurrentLocalPhase == 0;
    public bool Phase2SlimeRainActive => CurrentPhase == Phase.Phase2_1 && CurrentLocalPhase >= 2
        && Phase2MusicElapsedTime < Mechanic1Data.SlimeEnd;
    public bool Phase2DemonShadowActive => CurrentPhase == Phase.Phase2_1 && CurrentLocalPhase >= 3
        && Phase2MusicElapsedTime < Mechanic1Data.DemonShadowEnd;
    private bool Phase2PlatinumCoinActive => CurrentPhase == Phase.Phase2_1 && CurrentLocalPhase >= 4
        && Phase2MusicElapsedTime < Mechanic1Data.PlatinumCoinEnd;
    private bool Phase2StarRainActive => CurrentPhase == Phase.Phase2_1 && CurrentLocalPhase >= 1
        && Phase2MusicElapsedTime < Mechanic1Data.StarRainEnd;
    public bool Phase2ArenaRequested => CurrentPhase >= Phase.Phase2_2;
    public int Phase2ArenaStartMusicTime => Mechanic1Data.End;

    public void Phase2AI()
    {
        if (Phase2ElapsedTime < Phase2MinimumDuration)
        {
            Phase2ElapsedTime++;
            if (TONetworkEnvironment.NotClient && Phase2ElapsedTime % 60 == 0)
                NPC.netUpdate = true;
        }

        if (Phase2MusicElapsedTime == Mechanic1Data.Phase2ArenaSound)
            SoundEngine.PlaySound(LegendaryNightPhase2Arena.SummonSound);

        if (CurrentPhase == Phase.Phase2_1 && Phase2MusicElapsedTime >= Mechanic1Data.End)
        {
            ChangeState(Phase.Phase2_2);
            Mechanic2StartMusicTime = Mechanic2Data.StartMusicTime;
            Mechanic2HandPattern = 0;
            NPC.velocity = Vector2.Zero;
            EndMechanic1Attacks();
        }

        if (CurrentPhase == Phase.Phase2_2 && Phase2MusicElapsedTime >= Mechanic2Data.EndMusic)
        {
            ChangeState(Phase.Phase2_3);
            AnomalyNPC.Z = 0f;
        }

        if (TONetworkEnvironment.NotClient && CurrentPhase == Phase.Phase2_3 && Phase2MusicElapsedTime >= Mechanic4Data.StartMusicTime)
            ChangeState(Phase.Phase2_4);
        if (TONetworkEnvironment.NotClient && CurrentPhase == Phase.Phase2_4 && Phase2MusicElapsedTime >= Mechanic4Data.EndMusicTime)
            ChangeState(Phase.Phase2_5);

        if (Phase2ArenaRequested && !ArenaProjectileAlive && !Phase2ArenaProjectileAlive)
            EnsureArenaForPhase2();

        Timer1++;
        switch (CurrentPhase)
        {
            case Phase.Phase2_1:
                Mechanic1();
                break;
            case Phase.Phase2_2:
                Mechanic2();
                break;
            case Phase.Phase2_3:
                Mechanic3();
                break;
            case Phase.Phase2_4:
                Mechanic4();
                break;
            case Phase.Phase2_5:
                Mechanic5();
                break;
            default:
                // 后续机制尚未实现时的待机移动。
                NormalMovement();
                break;
        }

        void ChangeState(Phase phase, int localPhase = 0)
        {
            CurrentPhase = phase;
            CurrentLocalPhase = localPhase;
            Timer1 = 0;
            NPC.netUpdate = true;
        }

        void NormalMovement() => SkeletronHead.NormalMovement(NPC, Target, 20f, 8f, 0.2f, 0.08f, 300f, 20f);

        void Mechanic1()
        {
            int localPhase = Phase2MusicElapsedTime switch
            {
                >= Mechanic1Data.PlatinumCoinShockwave => 4,
                >= Mechanic1Data.StarShockwave => 3,
                >= Mechanic1Data.SlimeShockwave => 2,
                >= Mechanic1Data.RotationEnd => 1,
                _ => 0
            };
            bool stateChanged = CurrentLocalPhase != localPhase;
            if (stateChanged)
                ChangeState(Phase.Phase2_1, localPhase);

            if (CurrentLocalPhase == 0)
            {
                if (Timer1 == 1)
                {
                    SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
                    NPC.Face(Target);
                }
                SkeletronHead.RotatingMovement(NPC, Target, 0.3f, 10f);
                SummonRotatingSkullWarnings(Timer1);
                SummonStarMeteors(Timer1, reducedStarCount: true);
                return;
            }

            if (Phase2MusicElapsedTime >= Mechanic2Data.ArenaBrakeStartMusicTime)
            {
                // 归一化到最短回正方向，并在接近零时精确归零。
                float rotation = MathHelper.WrapAngle(NPC.rotation) * 0.85f;
                NPC.rotation = MathF.Abs(rotation) < 0.001f ? 0f : rotation;
            }
            int arenaBrakeRemaining = Mechanic2Data.ArenaBrakeEndMusicTime - Phase2MusicElapsedTime;
            if (arenaBrakeRemaining <= 0)
            {
                NPC.velocity = Vector2.Zero;
                SummonArenaChargeParticles();
            }
            else if (Phase2MusicElapsedTime >= Mechanic2Data.ArenaBrakeStartMusicTime)
            {
                RepositionForArena(arenaBrakeRemaining);
            }
            else
                NormalMovement();
            if (stateChanged && CurrentLocalPhase >= 2)
            {
                Spell shockwave = CurrentLocalPhase switch
                {
                    2 => Spell.Slime,
                    3 => Spell.Star,
                    _ => Spell.PlatinumCoin
                };
                SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
                ReleaseSpellShockwave(shockwave);
                if (CurrentLocalPhase == 3 && TONetworkEnvironment.NotClient)
                {
                    Projectile.NewProjectileAction<DemonShadow>(SourceAI, Target.Center + Vector2.UnitX * 500f,
                        Vector2.Zero, 0, 0f, action: p =>
                        {
                            p.ai[0] = NPC.target + 1;
                            p.ai[2] = NPC.whoAmI + 1;
                            p.GetModProjectile<DemonShadow>().HorizontalTracking = true;
                            p.netUpdate = true;
                        });
                }
            }

            // 全部常态子阶段共享音乐时钟，切换冲击波不会打断已激活的攻击节奏。
            bool summonSlime = Phase2SlimeRainActive && Mechanic1Data.IsAttackSpawnTime(
                Phase2MusicElapsedTime, Mechanic1Data.SlimeShockwave, Mechanic1Data.SlimeEnd);
            bool firePlatinumCoin = Phase2PlatinumCoinActive && (Phase2MusicElapsedTime - Mechanic1Data.PlatinumCoinShockwave) % Mechanic1Data.PlatinumCoinInterval == 0;
            if (!Main.dedServ)
            {
                if (summonSlime)
                    SoundEngine.PlaySound(SoundID.Item81, NPC.Center);
                if (firePlatinumCoin)
                    SoundEngine.PlaySound(PlatinumCoin.UnlimitedTossSound, NPC.Center);
            }
            if (TONetworkEnvironment.NotClient)
            {
                if (Phase2StarRainActive && (Phase2MusicElapsedTime - Mechanic1Data.RotationEnd) % Mechanic1Data.StarRainInterval == 0)
                {
                    Vector2 position = Target.Center + new Vector2(Main.rand.NextFloat(-1200f, 1200f), -1000f);
                    Projectile.NewProjectileAction<FallingStar>(SourceAI, position, new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(10f, 15f)), FallingStarDamage, 0f, action: p =>
                    {
                        p.ai[2] = NPC.whoAmI + 1;
                        p.scale = 0.2f;
                        p.timeLeft = Mechanic1Data.ProjectileLifetime;
                        p.GetModProjectile<FallingStar>().TrailType = 1;
                        p.netUpdate = true;
                    });
                }
                if (summonSlime)
                {
                    Vector2 position = Target.Center + new Vector2(Main.rand.NextFloat(-600f, 600f), -1000f);
                    Projectile.NewProjectileAction<SlimeMount>(SourceAI, position, Vector2.UnitY * Mechanic1Data.SlimeInitialSpeed, SlimeMountDamage, 0f, action: p =>
                    {
                        p.ai[1] = 1f;
                        p.ai[2] = NPC.target + 1;
                        p.GetModProjectile<SlimeMount>().MasterIndex = NPC.whoAmI + 1;
                        p.netUpdate = true;
                    });
                }
                if (firePlatinumCoin)
                {
                    Projectile.NewProjectileAction<PlatinumCoin>(SourceAI, NPC.Center, NPC.GetVelocityTowards(Target, Mechanic1Data.PlatinumCoinSpeed), PlatinumCoinDamage, 0f, action: p =>
                    {
                        p.ai[0] = NPC.target + 1;
                        p.ai[2] = NPC.whoAmI + 1;
                        p.timeLeft = Mechanic1Data.ProjectileLifetime;
                        p.netUpdate = true;
                        p.scale = 2f;
                    });
                }
            }
        }

        void RepositionForArena(int remaining)
        {
            int age = Phase2MusicElapsedTime - Mechanic2Data.ArenaBrakeStartMusicTime;
            if (age < Mechanic2Data.ArenaRepositionDelay)
            {
                NPC.velocity *= 0.9f;
                return;
            }
            Vector2 destination = Target.Center - Vector2.UnitY * Mechanic2Data.ArenaRepositionHeight;
            if (remaining > Mechanic2Data.ArenaRepositionBrakeDuration)
            {
                Vector2 desiredVelocity = (destination - NPC.Center) * 0.2f;
                float speed = desiredVelocity.Length();
                if (speed > Mechanic2Data.ArenaRepositionSpeed)
                    desiredVelocity *= Mechanic2Data.ArenaRepositionSpeed / speed;
                NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVelocity, 0.3f);
                return;
            }

            // 蓄力前复用8/9为起点、10/11为停靠点，13/14保存初速度；12=-1表示已锁定。
            // 进入机制2时重置12，拍击仍按原职责使用8至12槽；13/14不再读取。
            if (AnomalyNPC.AnomalyAI32[12].I != -1)
            {
                if (!TONetworkEnvironment.NotClient)
                {
                    NPC.velocity *= 0.9f;
                    return;
                }
                AnomalyNPC.AnomalyAI32[8].F = NPC.Center.X;
                AnomalyNPC.AnomalyAI32[9].F = NPC.Center.Y;
                AnomalyNPC.AnomalyAI32[10].F = destination.X;
                AnomalyNPC.AnomalyAI32[11].F = destination.Y;
                AnomalyNPC.AnomalyAI32[12].I = -1;
                AnomalyNPC.AnomalyAI32[13].F = NPC.velocity.X;
                AnomalyNPC.AnomalyAI32[14].F = NPC.velocity.Y;
                for (int slot = 8; slot <= 14; slot++)
                    AnomalyNPC.AIChanged32[slot] = true;
                NPC.netUpdate = true;
            }
            Vector2 origin = new(AnomalyNPC.AnomalyAI32[8].F, AnomalyNPC.AnomalyAI32[9].F);
            Vector2 target = new(AnomalyNPC.AnomalyAI32[10].F, AnomalyNPC.AnomalyAI32[11].F);
            Vector2 initialVelocity = new(AnomalyNPC.AnomalyAI32[13].F, AnomalyNPC.AnomalyAI32[14].F);
            float progress = MathHelper.Clamp((Mechanic2Data.ArenaRepositionBrakeDuration - remaining + 1f)
                / Mechanic2Data.ArenaRepositionBrakeDuration, 0f, 1f);
            // Hermite 保留快速移动末速度，抵达玩家上方时切线归零，不在停稳帧瞬移修正。
            Vector2 position = Vector2.Hermite(origin, initialVelocity * Mechanic2Data.ArenaRepositionBrakeDuration,
                target, Vector2.Zero, progress);
            NPC.velocity = position - NPC.Center;
        }

        void SummonArenaChargeParticles()
        {
            if (Main.dedServ)
                return;
            int remaining = Mechanic2Data.StartMusicTime - Phase2MusicElapsedTime;
            // 留足完整吸入时间，末段仅让已有星芒自然汇入，避免把生命周期压缩成几帧。
            if (remaining < Mechanic2Data.ChargeParticleMinLifetime)
                return;
            float charge = MathHelper.Clamp((Phase2MusicElapsedTime - Mechanic2Data.ArenaBrakeEndMusicTime)
                / (float)(Mechanic2Data.StartMusicTime - Mechanic2Data.ArenaBrakeEndMusicTime
                    - Mechanic2Data.ChargeParticleMinLifetime), 0f, 1f);
            const int SpiralArmCount = 4;
            int layers = Phase2MusicElapsedTime % 3 == 0 && charge > 0.5f ? 2 : 1;
            for (int layer = 0; layer < layers; layer++)
            {
                // 同批共享半径与寿命，沿用召唤仪式的反向发射和卷入，保持旋臂对称。
                float emissionAngle = -(Phase2MusicElapsedTime - Mechanic2Data.ArenaBrakeEndMusicTime) * 0.008f
                    + layer * 0.12f + Main.rand.NextFloat(-0.04f, 0.04f);
                float radius = Main.rand.NextFloat(Mechanic2Data.ChargeParticleMinRadius,
                    Mechanic2Data.ChargeParticleMaxRadius) * (1f + charge * Mechanic2Data.ChargeParticleRangeGrowth);
                int lifetime = Main.rand.Next(Mechanic2Data.ChargeParticleMinLifetime,
                    Math.Min(remaining, Mechanic2Data.ChargeParticleMaxLifetime) + 1);
                Color color = Color.Lerp(LegendaryNightHandler.StarColor, Color.White, Main.rand.NextFloat(0.3f, 0.9f));
                float scale = Main.rand.NextFloat(0.3f, 0.65f) * (1f + charge * Mechanic2Data.ChargeParticleScaleGrowth);
                for (int arm = 0; arm < SpiralArmCount; arm++)
                {
                    float angle = arm * MathHelper.TwoPi / SpiralArmCount + emissionAngle;
                    Vector2 offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
                    AnomalyNPC.SpawnVisualParticle(new LegendaryNightChargeSparkle(NPC.Center, offset, color,
                        scale, lifetime, spiralAngle: 4f));
                }
            }
        }

        void Mechanic2()
        {
            int age = Mechanic2Age;
            LegendaryNightPhase2Arena arena = Phase2Arena;
            if (Mechanic2HandPattern == 0 && TONetworkEnvironment.NotClient)
            {
                int hand = Main.rand.Next(2);
                int pattern = hand | (1 << Mechanic2Data.StrikeCount);
                for (int strike = 1; strike < Mechanic2Data.StrikeCount; strike++)
                {
                    if (Main.rand.NextProbability(0.7f))
                        hand ^= 1;
                    pattern |= hand << strike;
                }
                Mechanic2HandPattern = pattern;
                AnomalyNPC.AnomalyAI32[12].I = 0;
                AnomalyNPC.AIChanged32[12] = true;
                NPC.netUpdate = true;
            }
            if (age >= Mechanic2Data.ReturnStart && arena is not null)
            {
                HideHands();
                // 第三次拍击穿过观察者后才隐藏手部；从屏外沿上方弧线回中。
                // 起点由已同步的拍击目标推导，中途加入也能恢复相同轨迹。
                Vector2 impact = new(AnomalyNPC.AnomalyAI32[10].F, AnomalyNPC.AnomalyAI32[11].F);
                Vector2 retreatEnd = impact - Vector2.UnitY * (Mechanic2Data.HeightAbovePlayer - Mechanic2Data.BodyImpactHeight);
                (float advance, _) = Mechanic2Data.BodyStrikeMotion(Mechanic2Data.BodyFollowThroughDuration);
                Vector2 origin = Vector2.Lerp(retreatEnd, impact, advance);
                origin.Y = Math.Min(origin.Y, impact.Y);
                float progress = MathHelper.Clamp((age - Mechanic2Data.ReturnStart)
                    / (float)Mechanic2Data.ReturnDuration, 0f, 1f);
                float eased = MathHelper.SmoothStep(0f, 1f, progress);
                Vector2 position = Vector2.Lerp(origin, arena.Projectile.Center, eased);
                position.Y -= MathF.Sin(MathHelper.Pi * eased) * Mechanic2Data.BodyReturnArcHeight;
                NPC.velocity = position - NPC.Center;
                AnomalyNPC.Z = MathHelper.Lerp(Mechanic2Data.BodyExitZ, 0f, eased);
                NPC.rotation *= 0.85f;
                int remaining = Mechanic2Data.ReturnMusicTime - Phase2MusicElapsedTime;
                if (remaining == 0)
                {
                    NPC.Center = arena.Projectile.Center;
                    NPC.velocity = Vector2.Zero;
                    SoundEngine.PlaySound(SoundID.ForceRoar, arena.Projectile.Center);
                    ReleaseSpellShockwave(Spell.None);
                    // 外圈使用 CalRD BrimstoneGigaBlast 的算法 B；内圈使用 BrimstoneFireblast 的算法 A。
                    SpawnRadialStars(8f, 8, 45f * MathHelper.PiOver2 * 0.01f);
                    SpawnRadialStars(6f, 30, 12f * MathHelper.PiOver2 * 0.01f);
                }
                return;
            }

            // 法阵生成与各次拍击之间留在正常平面；仅在红色遮罩预警时后退。
            AnomalyNPC.Z = 0f;
            Vector2 center = arena?.Projectile.Center ?? NPC.Center;
            // 等待法阵从停靠点展开，不能在交接弹幕尚未生成时提前离开圆心。
            if (arena is null || age <= LegendaryNightPhase2Arena.AppearanceDuration)
            {
                NPC.velocity = Vector2.Zero;
                NPC.rotation *= 0.85f;
                NPC.Face(Target);
                return;
            }
            Vector2 destination = new(center.X, Target.Center.Y - Mechanic2Data.HeightAbovePlayer);
            float departure = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(
                (age - LegendaryNightPhase2Arena.AppearanceDuration) / (float)Mechanic2Data.ArenaDepartureDuration, 0f, 1f));
            NPC.velocity = (destination - NPC.Center) * (0.12f * departure);
            UpdateMechanic2BodyStrike(age);
            NPC.rotation *= 0.85f;
            NPC.Face(Target);

            for (int strike = 0; strike < Mechanic2Data.StrikeCount; strike++)
            {
                if (age != Mechanic2Data.GetStrikeTime(strike))
                    continue;
                SoundEngine.PlaySound(SoundID.Item14, center);
                if (TONetworkEnvironment.NotClient && arena is not null)
                {
                    SpawnMechanic2Stars(arena, Mechanic2StrikeSide(strike), strike);
                    SpawnStrikeFanStars();
                }
                NPC.netUpdate = true;
            }

            void SpawnStrikeFanStars()
            {
                Vector2 origin = arena.Projectile.Center;
                float targetAngle = (Target.Center - origin).SafeNormalize(Vector2.UnitY).ToRotation();
                for (int i = 0; i < Mechanic2Data.FanStarCount; i++)
                {
                    float angle = targetAngle + Mechanic2Data.FanStarSpread
                        * (i / (float)(Mechanic2Data.FanStarCount - 1) - 0.5f);
                    Vector2 velocity = angle.ToRotationVector2() * Mechanic2Data.GetFanStarSpeed(arena.Radius, 0);
                    Projectile.NewProjectileAction<FallingStar>(SourceAI, origin, velocity, FallingStarDamage, 0f, action: p =>
                    {
                        p.ai[1] = arena.Radius;
                        p.ai[2] = NPC.whoAmI + 1;
                        p.scale = 0.2f;
                        p.Resize(16, 16);
                        p.timeLeft = Mechanic2Data.StrikeInterval;
                        FallingStar star = p.GetModProjectile<FallingStar>();
                        star.CurrentMotionMode = FallingStar.MotionMode.StrikeFan;
                        star.TrailType = 2;
                        p.netUpdate = true;
                    });
                }
            }

            void SpawnRadialStars(float speed, int pairCount, float spread)
            {
                if (arena is null || speed <= 0f || pairCount <= 0)
                    return;
                if (!Main.dedServ)
                {
                    SoundEngine.PlaySound(SoundID.Item9, arena.Projectile.Center);
                    for (int i = 0; i < pairCount * 10; i++)
                    {
                        ParticleHandler.SpawnParticle(new OrbParticle(
                            arena.Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                            Main.rand.NextPolarVector2(speed * 0.5f, speed * 1.5f),
                            Main.rand.Next(20, 36), Main.rand.NextFloat(0.5f, 1.3f),
                            Color.Lerp(LegendaryNightHandler.StarColor, Color.White, Main.rand.NextFloat(0.5f))));
                    }
                }
                if (!TONetworkEnvironment.NotClient)
                    return;
                Vector2 direction = Main.rand.NextPolarVector2(1f);
                double startAngle = Math.Atan2(direction.X, direction.Y) - spread / 2f;
                double deltaAngle = spread / pairCount;
                for (int i = 0; i < pairCount; i++)
                {
                    // 保留原算法的弧度偏移及反向配对，不再叠加逐发随机扰动。
                    double offsetAngle = startAngle + deltaAngle * (i + i * i) / 2f + 32f * i;
                    Vector2 velocity = new((float)(Math.Sin(offsetAngle) * speed), (float)(Math.Cos(offsetAngle) * speed));
                    SpawnStar(velocity);
                    SpawnStar(-velocity);
                }

                void SpawnStar(Vector2 velocity)
                {
                    Projectile.NewProjectileAction<FallingStar>(SourceAI, arena.Projectile.Center, velocity,
                        FallingStarDamage, 0f, action: p =>
                        {
                            p.ai[2] = NPC.whoAmI + 1;
                            p.scale = 0.2f;
                            p.Resize(16, 16);
                            p.timeLeft = Math.Max(1, (int)MathF.Ceiling((arena.Radius + 32f) / speed));
                            FallingStar star = p.GetModProjectile<FallingStar>();
                            star.TrailType = 2;
                            star.AccelerationStartTime = int.MaxValue;
                            p.netUpdate = true;
                        });
                }
            }
        }

        void Mechanic3()
        {
            int time = Phase2MusicElapsedTime;
            LegendaryNightPhase2Arena arena = Phase2Arena;
            HideHands();
            AnomalyNPC.Z = 0f;
            NPC.rotation *= 0.85f;
            if (arena is null)
            {
                NPC.velocity = Vector2.Zero;
                return;
            }

            // LocalPhase：1飞行蓄力，2超级跳跃，3恢复原型回中，4等待下段。
            int localPhase = time < Mechanic3Data.JumpTime ? 1 : time < Mechanic3Data.LandingTime ? 2
                : time < Mechanic3Data.LandingTime + Mechanic3Data.ReturnDuration ? 3 : 4;
            if (TONetworkEnvironment.NotClient && CurrentLocalPhase != localPhase)
            {
                CurrentLocalPhase = localPhase;
                Timer1 = 0;
                NPC.netUpdate = true;
            }
            bool firstRain = time is >= Mechanic3Data.StartMusicTime
                and < Mechanic3Data.StartMusicTime + Mechanic3Data.RainDuration;
            bool finalRain = time is >= Mechanic3Data.LandingTime and < (Mechanic3Data.LandingTime + Mechanic3Data.RainDuration);
            if (time == Mechanic3Data.StartMusicTime)
            {
                SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
                ReleaseSpellShockwave(Spell.Slime);
            }
            // 15：每轮雨锁定的目标；8/9：跳跃起点；10/11：落底点。复用已结束的拍击槽。
            if ((time == Mechanic3Data.StartMusicTime || time == Mechanic3Data.LandingTime)
                && TONetworkEnvironment.NotClient)
            {
                AnomalyNPC.AnomalyAI32[15].I = NPC.target + 1;
                AnomalyNPC.AIChanged32[15] = true;
                NPC.netUpdate = true;
            }
            if ((firstRain || finalRain) && TONetworkEnvironment.NotClient
                && Player.TryGetPlayerFromIndex(AnomalyNPC.AnomalyAI32[15].I - 1, out Player rainTarget))
            {
                // 首层直接出现在玩家头顶；高层延长末轮雨，而不是让整轮在屏外等待数秒。
                int layers = finalRain ? Mechanic3Data.FinalRainLayers : Mechanic3Data.RainLayers;
                float spacing = Mechanic3Data.RainLayerSpacing;
                float speed = Mechanic3Data.RainSpeed;
                for (int i = 0; i < layers; i++)
                    Projectile.NewProjectileAction<LegendSlimeRain>(SourceAI,
                        rainTarget.Center + new Vector2(Main.rand.NextFloat(-Mechanic3Data.RainHalfWidth, Mechanic3Data.RainHalfWidth),
                            -Mechanic3Data.RainSpawnHeight - i * spacing),
                        Vector2.UnitY * speed, SlimeMountDamage, 0f,
                        action: p =>
                        {
                            p.ai[0] = Main.rand.Next(LegendSlimeRain.VariantCount);
                            p.timeLeft = Mechanic3Data.RainLifetime;
                            p.netUpdate = true;
                        });
            }
            if (time < Mechanic3Data.JumpTime)
            {
                SkeletronHead.NormalMovement(NPC, Target, 20f, 8f, 0.2f, 0.08f, 300f, 20f);
                Charge(Mechanic3Data.FanTime);
                Charge(Mechanic3Data.Ring1Time);
                Charge(Mechanic3Data.Ring2Time);
                Charge(Mechanic3Data.JumpTime);
                if (time == Mechanic3Data.FanTime)
                    FireMounts(Mechanic3Data.FanCount, TOMathUtils.PiOver3 * 2 / (Mechanic3Data.FanCount - 1), false);
                if (time is Mechanic3Data.Ring1Time or Mechanic3Data.Ring2Time)
                    FireMounts(Mechanic3Data.RingCount, MathHelper.TwoPi / Mechanic3Data.RingCount, true);
                return;
            }
            if (time == Mechanic3Data.JumpTime && TONetworkEnvironment.NotClient)
            {
                float x = MathHelper.Clamp(Target.Center.X - arena.Projectile.Center.X, -arena.Radius * 0.85f, arena.Radius * 0.85f);
                Vector2 landing = arena.Projectile.Center + new Vector2(x,
                    MathF.Sqrt(arena.Radius * arena.Radius - x * x) - NPC.height * 0.5f);
                AnomalyNPC.AnomalyAI32[8].F = NPC.Center.X;
                AnomalyNPC.AnomalyAI32[9].F = NPC.Center.Y;
                AnomalyNPC.AnomalyAI32[10].F = landing.X;
                AnomalyNPC.AnomalyAI32[11].F = landing.Y;
                for (int slot = 8; slot <= 11; slot++)
                    AnomalyNPC.AIChanged32[slot] = true;
                NPC.netUpdate = true;
            }
            Vector2 origin = new(AnomalyNPC.AnomalyAI32[8].F, AnomalyNPC.AnomalyAI32[9].F);
            Vector2 bottom = new(AnomalyNPC.AnomalyAI32[10].F, AnomalyNPC.AnomalyAI32[11].F);
            if (time < Mechanic3Data.LandingTime)
            {
                // Fargo超级跳跃：起跳时锁定横向落点，固定重力形成高抛物线，不在空中追踪。
                float age = time - Mechanic3Data.JumpTime + 1f;
                float progress = age / Mechanic3Data.JumpDuration;
                Vector2 position = Vector2.Lerp(origin, bottom, progress);
                position.Y -= 0.5f * Mechanic3Data.JumpGravity * age * (Mechanic3Data.JumpDuration - age);
                NPC.velocity = position - NPC.Center;
            }
            else
            {
                if (time == Mechanic3Data.LandingTime)
                    SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                float progress = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(
                    (time - Mechanic3Data.LandingTime) / (float)Mechanic3Data.ReturnDuration, 0f, 1f));
                NPC.velocity = Vector2.Lerp(bottom, arena.Projectile.Center, progress) - NPC.Center;
            }

            void FireMounts(int count, float spacing, bool spin)
            {
                SoundEngine.PlaySound(SoundID.Item81, NPC.Center);
                if (!TONetworkEnvironment.NotClient)
                    return;
                Projectile.NewProjectilesArc<SlimeMount>(count, spacing, SourceAI, NPC.Center,
                    NPC.GetVelocityTowards(Target, Mechanic3Data.MountSpeed), SlimeMountDamage, 0f, action: p =>
                    {
                        p.ai[1] = spin ? 3f : 2f;
                        p.rotation = p.velocity.ToRotation() - MathHelper.PiOver2;
                        p.timeLeft = 180;
                        p.netUpdate = true;
                    });
            }

            void Charge(int releaseTime)
            {
                int remaining = releaseTime - time;
                if (Main.dedServ || remaining <= LegendaryNightSlimeChargeParticle.ChargeLifetime
                    || remaining > Mechanic3Data.ChargeDuration)
                    return;
                for (int i = 0; i < LegendaryNightSlimeChargeParticle.SpawnCount; i++)
                {
                    Vector2 offset = Main.rand.NextPolarVector2(120f, 320f);
                    AnomalyNPC.SpawnVisualParticle(new LegendaryNightSlimeChargeParticle(NPC, offset));
                }
            }
        }

        void Mechanic4()
        {
            int time = Phase2MusicElapsedTime;
            LegendaryNightPhase2Arena arena = Phase2Arena;
            HideHands();
            AnomalyNPC.Z = 0f;
            NPC.rotation = 0f;
            if (arena is null)
            {
                NPC.velocity = Vector2.Zero;
                return;
            }
            if (time == Mechanic4Data.StartMusicTime)
            {
                SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
                ReleaseSpellShockwave(Spell.PlatinumCoin);
            }

            // LocalPhase：0首次待机及后退，1/3/5/7直冲，2/4/6后退，8回中。
            int charge = Math.Clamp((time - Mechanic4Data.FirstChargeTime) / Mechanic4Data.ChargeInterval,
                0, Mechanic4Data.ChargeCount - 1);
            int age = time - Mechanic4Data.GetChargeTime(charge);
            int localPhase = time >= Mechanic4Data.ReturnStart ? 8
                : time < Mechanic4Data.FirstChargeTime ? 0
                : charge * 2 + (age < Mechanic4Data.ChargeDuration ? 1 : 2);
            if (TONetworkEnvironment.NotClient && CurrentLocalPhase != localPhase)
            {
                CurrentLocalPhase = localPhase;
                Timer1 = 0;
                NPC.netUpdate = true;
            }
            if (time == Mechanic4Data.GetChargeTime(charge) + Mechanic4Data.ChargeDuration)
                SoundEngine.PlaySound(SoundID.Item14, arena.Projectile.Center);
            if (time >= Mechanic4Data.ReturnStart)
            {
                // 8/9：回中起点；10/11：冲刺锁定速度。复用已结束的史莱姆跳跃槽。
                if (time == Mechanic4Data.ReturnStart && TONetworkEnvironment.NotClient)
                {
                    AnomalyNPC.AnomalyAI32[8].F = NPC.Center.X;
                    AnomalyNPC.AnomalyAI32[9].F = NPC.Center.Y;
                    AnomalyNPC.AIChanged32[8] = AnomalyNPC.AIChanged32[9] = true;
                    NPC.netUpdate = true;
                }
                Vector2 origin = new(AnomalyNPC.AnomalyAI32[8].F, AnomalyNPC.AnomalyAI32[9].F);
                float progress = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(
                    (time - Mechanic4Data.ReturnStart + 1f) / (Mechanic4Data.EndMusicTime - Mechanic4Data.ReturnStart), 0f, 1f));
                NPC.velocity = Vector2.Lerp(origin, arena.Projectile.Center, progress) - NPC.Center;
                return;
            }
            if (age is >= 0 and < Mechanic4Data.ChargeDuration)
            {
                if (age == 0)
                {
                    SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
                    if (TONetworkEnvironment.NotClient)
                    {
                        Vector2 velocity = NPC.GetVelocityTowards(Target, Mechanic4Data.ChargeSpeed);
                        AnomalyNPC.AnomalyAI32[10].F = velocity.X;
                        AnomalyNPC.AnomalyAI32[11].F = velocity.Y;
                        AnomalyNPC.AIChanged32[10] = AnomalyNPC.AIChanged32[11] = true;
                        NPC.netUpdate = true;
                    }
                }
                Vector2 chargeVelocity = new(AnomalyNPC.AnomalyAI32[10].F, AnomalyNPC.AnomalyAI32[11].F);
                float speedFactor = Mechanic4Data.ChargeSpeedFactor(age);
                NPC.velocity = chargeVelocity * speedFactor;
            }
            else
            {
                int retreatStart = time < Mechanic4Data.FirstChargeTime ? Mechanic4Data.FirstRetreatStart
                    : Mechanic4Data.GetChargeTime(charge) + Mechanic4Data.ChargeDuration;
                float speed = Mechanic4Data.RetreatSpeed * MathHelper.Clamp((time - retreatStart + 1f) / 12f, 0f, 1f);
                NPC.velocity = -NPC.GetVelocityTowards(Target, speed);
            }
        }

        void Mechanic5()
        {
            int time = Phase2MusicElapsedTime;
            // LocalPhase：0星星雨，1公转加速，2连续星爆及手部收尾，3结束待机。
            int localPhase = time < LegendaryNightPhase2Arena.FourthStarburstMusicTime ? 0
                : time < LegendaryNightPhase2Arena.FifthStarburstMusicTime ? 1
                : time < Mechanic5Data.EndMusicTime ? 2 : 3;
            if (TONetworkEnvironment.NotClient && CurrentLocalPhase != localPhase)
            {
                CurrentLocalPhase = localPhase;
                Timer1 = 0;
                NPC.netUpdate = true;
            }
            AnomalyNPC.Z = 0f;
            NPC.rotation = 0f;
            NPC.velocity = Phase2Arena is { } arena ? arena.Projectile.Center - NPC.Center : Vector2.Zero;
            if (time == Mechanic5Data.StartMusicTime)
            {
                SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
                if (TONetworkEnvironment.NotClient)
                {
                    // 复用原有手部，从本体位置重新伸出，不生成或替换NPC。
                    foreach (NPC hand in NPC.ActiveNPCs)
                    {
                        if (hand.type != NPCID.SkeletronHand || (int)hand.ai[1] != NPC.whoAmI)
                            continue;
                        hand.Center = NPC.Center;
                        hand.velocity = Vector2.Zero;
                        hand.ai[2] = hand.ai[3] = 0f;
                        hand.target = NPC.target;
                        hand.hide = false;
                        hand.damage = 0;
                        hand.Anomaly.Z = 0f;
                        hand.netUpdate = true;
                    }
                }
            }
            if (!TONetworkEnvironment.NotClient)
                return;
            if (time == LegendaryNightPhase2Arena.FifthStarburstMusicTime)
            {
                foreach (Projectile projectile in Projectile.ActiveProjectiles)
                    if (projectile.ModProjectile is FallingStar && (int)projectile.ai[2] == NPC.whoAmI + 1
                        && projectile.ai[1] == 5f)
                        projectile.Kill();
            }
            int starburst = LegendaryNightPhase2Arena.GetStarburstIndex(time);
            if (starburst >= 4 && time == LegendaryNightPhase2Arena.GetStarburstTime(starburst)
                && Phase2Arena is { } starArena)
            {
                SpawnStarburstStars(starArena, starburst);
            }
            if (time >= Mechanic5Data.StartMusicTime && time < Mechanic5Data.RainEnd
                && (time - Mechanic5Data.StartMusicTime) % Mechanic5Data.RainInterval == 0)
            {
                Projectile.NewProjectileAction<FallingStar>(SourceAI,
                    Target.Center + new Vector2(Main.rand.NextFloat(-1200f, 1200f), -1000f),
                    new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(10f, 15f)), FallingStarDamage, 0f,
                    action: p =>
                    {
                        p.ai[1] = 5f; // 机制5星星雨，清场不影响其他攻击。
                        p.ai[2] = NPC.whoAmI + 1;
                        p.scale = 0.2f;
                        p.timeLeft = Mechanic5Data.RainLifetime;
                        p.GetModProjectile<FallingStar>().TrailType = 1;
                        p.netUpdate = true;
                    });
            }

            void SpawnStarburstStars(LegendaryNightPhase2Arena arena, int starburstIndex)
            {
                // 发射点直接读取法阵中心，不依赖本体本帧是否已经移动到圆心。
                Vector2 origin = arena.Projectile.Center;
                float aimAngle = (Target.Center - origin).SafeNormalize(Vector2.UnitY).ToRotation();
                int count = Mechanic5Data.GetStarburstStarCount(starburstIndex);
                float speed = Mechanic2Data.GetFanStarSpeed(arena.Radius, 0);
                for (int i = 0; i < count; i++)
                {
                    Vector2 velocity = (aimAngle + MathHelper.TwoPi * i / count).ToRotationVector2() * speed;
                    Projectile.NewProjectileAction<FallingStar>(SourceAI, origin, velocity, FallingStarDamage, 0f, action: p =>
                    {
                        p.ai[1] = arena.Radius;
                        p.ai[2] = NPC.whoAmI + 1;
                        p.scale = 0.2f;
                        p.Resize(16, 16);
                        p.timeLeft = Mechanic2Data.StrikeInterval;
                        FallingStar star = p.GetModProjectile<FallingStar>();
                        star.CurrentMotionMode = FallingStar.MotionMode.StrikeFan;
                        star.TrailType = 2;
                        p.netUpdate = true;
                    });
                }
            }
        }

        void UpdateMechanic2BodyStrike(int age)
        {
            for (int strike = 0; strike < Mechanic2Data.StrikeCount; strike++)
            {
                int relativeAge = age - Mechanic2Data.GetStrikeTime(strike);
                if (relativeAge is < -Mechanic2Data.BodyRetreatLeadTime
                    or > (Mechanic2Data.BodyFollowThroughDuration + Mechanic2Data.BodyRecoverDuration))
                    continue;

                // 8/9：预警开始时的位置；10/11：锁定目标点；12：已锁定的拍击序号（1-based）。
                // 保存在 NPC 的同步数据中，避免玩家移动改变冲刺方向，也避免 Behavior 单例保存实体状态。
                if (AnomalyNPC.AnomalyAI32[12].I != strike + 1)
                {
                    Vector2 impact = Target.Center - Vector2.UnitY * Mechanic2Data.BodyImpactHeight;
                    AnomalyNPC.AnomalyAI32[8].F = NPC.Center.X;
                    AnomalyNPC.AnomalyAI32[9].F = NPC.Center.Y;
                    AnomalyNPC.AnomalyAI32[10].F = impact.X;
                    AnomalyNPC.AnomalyAI32[11].F = impact.Y;
                    AnomalyNPC.AnomalyAI32[12].I = strike + 1;
                    for (int slot = 8; slot <= 12; slot++)
                        AnomalyNPC.AIChanged32[slot] = true;
                    if (TONetworkEnvironment.NotClient)
                        NPC.netUpdate = true;
                }

                Vector2 origin = new(AnomalyNPC.AnomalyAI32[8].F, AnomalyNPC.AnomalyAI32[9].F);
                Vector2 target = new(AnomalyNPC.AnomalyAI32[10].F, AnomalyNPC.AnomalyAI32[11].F);
                Vector2 retreatEnd = target - Vector2.UnitY * (Mechanic2Data.HeightAbovePlayer - Mechanic2Data.BodyImpactHeight);
                (float advance, _) = Mechanic2Data.BodyStrikeMotion(relativeAge);
                Vector2 position = relativeAge < -Mechanic2Data.BodyRushDuration
                    ? Vector2.Lerp(origin, retreatEnd, Mechanic2Data.BodyRetreatProgress(relativeAge))
                    : Vector2.Lerp(retreatEnd, target, advance);
                // 冲出观察者平面后，从上方绕回后景；前冲始终掠过锁定目标的头顶。
                if (relativeAge >= -Mechanic2Data.BodyRushDuration)
                    position.Y = Math.Min(position.Y, target.Y);
                float recovery = MathHelper.Clamp((relativeAge - Mechanic2Data.BodyFollowThroughDuration)
                    / (float)Mechanic2Data.BodyRecoverDuration, 0f, 1f);
                position.Y -= MathF.Sin(MathHelper.Pi * MathHelper.SmoothStep(0f, 1f, recovery)) * Mechanic2Data.BodyReturnArcHeight;
                // 按音乐时钟计算位置，以本帧位移作为速度，拍击帧准确抵达前景而不受追踪插值滞后影响。
                NPC.velocity = position - NPC.Center;
                AnomalyNPC.Z = Mechanic2Data.BodyStrikeDepth(relativeAge);
                // 以绘制底边为锚点反算投影偏移，避免负 Z 放大时底边离玩家越来越远。
                // 回程的前六帧仍在观察者之外，此时解除锚定，再从上方弧线重新入画。
                float anchorWeight = MathHelper.SmoothStep(0f, 1f,
                    MathHelper.Clamp((relativeAge + Mechanic2Data.BodyRushDuration)
                        / (float)Mechanic2Data.BodyRushDuration, 0f, 1f))
                    * (1f - MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(recovery * 10f, 0f, 1f)));
                float halfWidth = NPC.frame.Width * NPC.scale * 0.5f;
                float halfHeight = NPC.frame.Height * NPC.scale * 0.5f;
                float drawRotation = NPC.rotation * 0.85f;
                float bottomExtent = Math.Abs(MathF.Sin(drawRotation)) * halfWidth
                    + Math.Abs(MathF.Cos(drawRotation)) * halfHeight;
                float drawCenterOffset = NPC.height * 0.5f - halfHeight + 4f + NPC.gfxOffY;
                AnomalyNPC.Depth_DrawAnchor = (new Vector2(0f, drawCenterOffset + bottomExtent),
                    Target.Top - Vector2.UnitY * Mechanic2Data.BodyHeadClearance, anchorWeight);
                return;
            }
        }

        void SpawnMechanic2Stars(LegendaryNightPhase2Arena arena, int side, int strike)
        {
            const int Count = Mechanic2Data.StarCount;
            float radius = arena.Radius;
            // 生成时立即铺满整个受击半圆，而不是从半圆上沿之外开始下落。
            for (int i = 0; i < Count; i++)
            {
                Vector2 position = arena.Projectile.Center + Mechanic2Data.GetStarSpawnOffset(i, radius, side);
                Projectile.NewProjectileAction<Mechanic2Star>(SourceAI, position, Vector2.UnitY * Mechanic2Data.StarSpeed, FallingStarDamage, 0f, action: p =>
                {
                    p.ai[0] = strike;
                    p.ai[1] = side;
                    p.ai[2] = NPC.whoAmI + 1;
                    p.netUpdate = true;
                });
            }
        }
    }

    public bool HandsHidden => Mechanic3Active || Mechanic4Active || Mechanic2Active && Mechanic2Age >= Mechanic2Data.ReturnStart;

    private void HideHands()
    {
        foreach (NPC hand in NPC.ActiveNPCs)
        {
            if (hand.type != NPCID.SkeletronHand || (int)hand.ai[1] != NPC.whoAmI)
                continue;
            hand.hide = true;
            hand.Anomaly.Skeletron_HandBloodDust = null;
        }
    }

    private void EndMechanic1Attacks()
    {
        if (!TONetworkEnvironment.NotClient)
            return;
        foreach (Projectile p in Projectile.ActiveProjectiles)
        {
            bool owned = p.ModProjectile switch
            {
                SlimeMount slime => slime.MasterIndex == NPC.whoAmI + 1,
                FallingStar or PlatinumCoin or DemonShadow or SkullWarning or Meteor => (int)p.ai[2] == NPC.whoAmI + 1,
                _ => false
            };
            if (owned)
                p.Kill();
        }
    }

    private void EnsureArenaForPhase2()
    {
        if (!TONetworkEnvironment.NotClient)
            return;
        // 正常由现存一阶段竞技场交接；引用缺失时先补回交接方。
        Projectile.NewProjectileAction<LegendaryNightArena>(SourceAI, Target.Center, Vector2.Zero, ArenaDamage, 0f,
            action: p =>
            {
                p.GetModProjectile<LegendaryNightArena>().Master = NPC;
                ArenaProjectile = p;
            });
    }
}
