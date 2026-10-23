namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class SkeletronLegendHead
{
    // 6：三次拍击的随机左右侧位图（bit3 标记已初始化）；7：本轮机制的音乐起点。
    public int Mechanic2HandPattern
    {
        get => AnomalyNPC.AnomalyAI32[6].i;
        private set
        {
            AnomalyNPC.AnomalyAI32[6].i = value;
            AnomalyNPC.AIChanged32[6] = true;
        }
    }

    public int Mechanic2StartMusicTime
    {
        get => AnomalyNPC.AnomalyAI32[7].i;
        private set
        {
            AnomalyNPC.AnomalyAI32[7].i = value;
            AnomalyNPC.AIChanged32[7] = true;
        }
    }

    public int Mechanic2Age => Phase2MusicElapsedTime - Mechanic2StartMusicTime;
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
            if (TOSharedData.NotClient && Phase2ElapsedTime % 60 == 0)
                NPC.netUpdate = true;
        }

        if (Phase2MusicElapsedTime == Mechanic1Data.Phase2ArenaSound)
            SoundEngine.PlaySound(LegendaryNightPhase2Arena.SummonSound);

        if (CurrentPhase == Phase.Phase2_1 && Phase2MusicElapsedTime >= Mechanic1Data.End)
        {
            ChangeState(Phase.Phase2_2);
            Mechanic2StartMusicTime = Mechanic2Data.StartMusicTime;
            Mechanic2HandPattern = 0;
            EndMechanic1Attacks();
        }

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
                if (CurrentLocalPhase == 3 && TOSharedData.NotClient)
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
            if (TOSharedData.NotClient)
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

        void Mechanic2()
        {
            int age = Mechanic2Age;
            LegendaryNightPhase2Arena arena = Phase2Arena;
            if (Mechanic2HandPattern == 0 && TOSharedData.NotClient)
            {
                int hand = Main.rand.Next(2);
                int pattern = hand | (1 << Mechanic2Data.StrikeCount);
                for (int strike = 1; strike < Mechanic2Data.StrikeCount; strike++)
                {
                    if (Main.rand.NextProbability(0.6f))
                        hand ^= 1;
                    pattern |= hand << strike;
                }
                Mechanic2HandPattern = pattern;
                AnomalyNPC.AnomalyAI32[12].i = 0;
                AnomalyNPC.AIChanged32[12] = true;
                NPC.netUpdate = true;
            }
            if (age >= Mechanic2Data.End)
            {
                AnomalyNPC.Z = 0f;
                CurrentPhase = Phase.Phase2_3;
                CurrentLocalPhase = 0;
                Timer1 = 0;
                NPC.netUpdate = true;
                Mechanic3();
                return;
            }
            // 每个音乐节点独立保留速度、数量参数，方便逐波调整。
            switch (Phase2MusicElapsedTime)
            {
                case Mechanic2Data.Star1MusicTime: SpawnRadialStars(10f, 12); break;
                case Mechanic2Data.Star2MusicTime: SpawnRadialStars(8f, 30); break;
                case Mechanic2Data.Star3MusicTime: SpawnRadialStars(10f, 6); break;
                case Mechanic2Data.Star4MusicTime: SpawnRadialStars(9f, 8); break;
                case Mechanic2Data.Star5MusicTime: SpawnRadialStars(8f, 10); break;
                case Mechanic2Data.Star6MusicTime: SpawnRadialStars(7f, 20); break;
                case Mechanic2Data.Star7MusicTime: SpawnRadialStars(6f, 3); break;
                case Mechanic2Data.Star8MusicTime: SpawnRadialStars(6f, 5); break;
                case Mechanic2Data.Star9MusicTime: SpawnRadialStars(6f, 6); break;
                case Mechanic2Data.Star10MusicTime: SpawnRadialStars(6f, 8); break;
                case Mechanic2Data.Star11MusicTime: SpawnRadialStars(8f, 4); break;
                case Mechanic2Data.Star12MusicTime: SpawnRadialStars(7f, 3); break;
                case Mechanic2Data.Star13MusicTime: SpawnRadialStars(6f, 16); break;
            }

            if (age >= Mechanic2Data.ReturnStart + Mechanic2Data.ReturnDuration)
            {
                AnomalyNPC.Z = 0f;
                SkeletronHead.NormalMovement(NPC, Target, 20f, 8f, 0.2f, 0.08f, 300f, 20f);
                return;
            }

            // 法阵生成与各次拍击之间留在正常平面；仅在红色遮罩预警时后退。
            AnomalyNPC.Z = 0f;
            Vector2 center = arena?.Projectile.Center ?? Target.Center;
            Vector2 destination = new(center.X, Target.Center.Y - Mechanic2Data.HeightAbovePlayer);
            NPC.velocity = (destination - NPC.Center) * 0.12f;
            UpdateMechanic2BodyStrike(age);
            NPC.rotation *= 0.85f;
            NPC.Face(Target);

            for (int strike = 0; strike < Mechanic2Data.StrikeCount; strike++)
            {
                if (age != Mechanic2Data.GetStrikeTime(strike))
                    continue;
                SoundEngine.PlaySound(SoundID.Item14, center);
                if (TOSharedData.NotClient && arena is not null)
                    SpawnMechanic2Stars(arena, Mechanic2StrikeSide(strike), strike);
                NPC.netUpdate = true;
            }

            void SpawnRadialStars(float speed, int count)
            {
                if (arena is null || speed <= 0f || count <= 0)
                    return;
                if (!Main.dedServ)
                {
                    SoundEngine.PlaySound(SoundID.Item9, arena.Projectile.Center);
                    for (int i = 0; i < count * 5; i++)
                    {
                        ParticleHandler.SpawnParticle(new OrbParticle(
                            arena.Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                            Main.rand.NextPolarVector2(speed * 0.5f, speed * 1.5f),
                            Main.rand.Next(20, 36), Main.rand.NextFloat(0.5f, 1.3f),
                            Color.Lerp(LegendaryNightHandler.StarColor, Color.White, Main.rand.NextFloat(0.5f))));
                    }
                }
                if (!TOSharedData.NotClient)
                    return;
                Projectile.NewProjectilesArc<FallingStar>(count, MathHelper.TwoPi / count, SourceAI, arena.Projectile.Center, Main.rand.NextPolarVector2(speed),
                    FallingStarDamage, 0f, action: p =>
                    {
                        p.ai[2] = NPC.whoAmI + 1;
                        p.scale = 0.2f;
                        p.Resize(16, 16);
                        p.timeLeft = Math.Max(1, (int)MathF.Ceiling((arena.Radius + 32f) / speed));
                        FallingStar star = p.GetModProjectile<FallingStar>();
                        star.TrailType = 2;
                        star.AccelerationStartTime = int.MaxValue;
                        p.velocity.Rotation += Main.rand.NextFloat(-0.1f, 0.1f);
                        p.netUpdate = true;
                    });
            }
        }

        void Mechanic3()
        {
            NormalMovement();
            LegendaryNightPhase2Arena arena = Phase2Arena;
            if (arena is null || CurrentLocalPhase >= Mechanic3Data.CircleCount || !TOSharedData.NotClient)
                return;

            int circleIndex = CurrentLocalPhase + 1;
            int age = Phase2MusicElapsedTime - Mechanic3Data.GetStartMusicTime(circleIndex);
            if (age < 0)
                return;
            for (int i = 0; i < Mechanic3Data.StarCount; i++)
            {
                int index = i;
                Projectile.NewProjectileAction<Mechanic3Star>(SourceAI,
                    Mechanic3Data.GetStarPosition(arena, index, age, circleIndex), Vector2.Zero, FallingStarDamage, 0f,
                    action: p =>
                    {
                        p.ai[0] = index;
                        p.ai[1] = circleIndex;
                        p.ai[2] = NPC.whoAmI + 1;
                        p.netUpdate = true;
                    });
            }
            CurrentLocalPhase = circleIndex;
            NPC.netUpdate = true;
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
                if (AnomalyNPC.AnomalyAI32[12].i != strike + 1)
                {
                    Vector2 impact = Target.Center - Vector2.UnitY * Mechanic2Data.BodyImpactHeight;
                    AnomalyNPC.AnomalyAI32[8].f = NPC.Center.X;
                    AnomalyNPC.AnomalyAI32[9].f = NPC.Center.Y;
                    AnomalyNPC.AnomalyAI32[10].f = impact.X;
                    AnomalyNPC.AnomalyAI32[11].f = impact.Y;
                    AnomalyNPC.AnomalyAI32[12].i = strike + 1;
                    for (int slot = 8; slot <= 12; slot++)
                        AnomalyNPC.AIChanged32[slot] = true;
                    if (TOSharedData.NotClient)
                        NPC.netUpdate = true;
                }

                Vector2 origin = new(AnomalyNPC.AnomalyAI32[8].f, AnomalyNPC.AnomalyAI32[9].f);
                Vector2 target = new(AnomalyNPC.AnomalyAI32[10].f, AnomalyNPC.AnomalyAI32[11].f);
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

    private void EndMechanic1Attacks()
    {
        if (!TOSharedData.NotClient)
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
        if (!TOSharedData.NotClient)
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
