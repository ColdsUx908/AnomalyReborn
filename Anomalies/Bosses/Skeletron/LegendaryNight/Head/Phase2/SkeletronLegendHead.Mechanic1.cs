namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class SkeletronLegendHead
{
    /// <summary>机制 1 的音乐绝对帧节点；从转场音乐开始累计，单位为游戏帧。</summary>
    public static class Mechanic1Timeline
    {
        /// <summary>本机制开始的音乐绝对帧（包含此帧）。</summary>
        public const int StartMusicTime = PhaseChangeDuration;
        /// <summary>本机制结束的音乐绝对帧（不包含此帧），与下一机制的开始帧衔接。</summary>
        public const int EndMusicTime = 47 * 60 + 10;
        /// <summary>旋转结束的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int RotationEndMusicTime = 15 * 60 + 40;
        /// <summary>史莱姆冲击波的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int SlimeShockwaveMusicTime = 22 * 60;
        /// <summary>星体冲击波的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int StarShockwaveMusicTime = 28 * 60 + 20;
        /// <summary>铂金币冲击波的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int PlatinumCoinShockwaveMusicTime = 34 * 60 + 40;
        /// <summary>史莱姆结束的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int SlimeEndMusicTime = 41 * 60;
        /// <summary>恶魔之影结束的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int DemonShadowEndMusicTime = 42 * 60 + 33;
        /// <summary>铂金币结束的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int PlatinumCoinEndMusicTime = 44 * 60 + 6;
        /// <summary>落星雨结束的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int StarRainEndMusicTime = 45 * 60 + 39;
        /// <summary>二阶段场地召唤音效的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int Phase2ArenaSoundMusicTime = 46 * 60 + 45;
    }

    /// <summary>机制 1 的攻击、运动和表现参数及纯计算；时长单位为游戏帧。</summary>
    public static class Mechanic1Parameters
    {
        /// <summary>攻击节拍持续时间，单位为游戏帧。</summary>
        public const int AttackBeatDuration = Mechanic1Timeline.StarShockwaveMusicTime - Mechanic1Timeline.SlimeShockwaveMusicTime;
        /// <summary>攻击每拍间隔数。</summary>
        public const int AttackIntervalsPerBeat = 8;
        /// <summary>攻击动画持续时间，单位为游戏帧。</summary>
        public const int AttackAnimationDuration = (AttackBeatDuration + AttackIntervalsPerBeat - 1) / AttackIntervalsPerBeat;
        /// <summary>铂金币间隔，单位为游戏帧。</summary>
        public const int PlatinumCoinInterval = 16;
        /// <summary>落星雨间隔，单位为游戏帧。</summary>
        public const int StarRainInterval = 48;
        /// <summary>弹幕存活时间，单位为游戏帧。</summary>
        public const int ProjectileLifetime = 300;
        /// <summary>铂金币速度，以世界像素和游戏帧计量。</summary>
        public const float PlatinumCoinSpeed = 16f;
        /// <summary>史莱姆初始速度，以世界像素和游戏帧计量。</summary>
        public const float SlimeInitialSpeed = 0.75f;

        /// <summary>首拍生成第1个，每个冲击波间隔生成8次；整数帧间隔均匀分配，停止帧不再生成。</summary>
        public static bool IsAttackSpawnTime(int musicTime, int start, int end)
        {
            int age = musicTime - start;
            return age >= 0 && musicTime < end && (age == 0
                || (long)age * AttackIntervalsPerBeat / AttackBeatDuration
                    != (long)(age - 1) * AttackIntervalsPerBeat / AttackBeatDuration);
        }
    }

    private void Mechanic1AI()
    {
        int localPhase = Phase2MusicElapsedTime switch
        {
            >= Mechanic1Timeline.PlatinumCoinShockwaveMusicTime => 4,
            >= Mechanic1Timeline.StarShockwaveMusicTime => 3,
            >= Mechanic1Timeline.SlimeShockwaveMusicTime => 2,
            >= Mechanic1Timeline.RotationEndMusicTime => 1,
            _ => 0
        };
        bool stateChanged = CurrentLocalPhase != localPhase;
        if (stateChanged)
            ChangePhase2State(Phase.Phase2_1, localPhase);

        if (CurrentLocalPhase == 0)
        {
            if (Timer1 == 1)
            {
                SoundEngine.PlaySound(SoundID.ForceRoar);
                NPC.Face(Target);
            }
            SkeletronHead.RotatingMovement(NPC, Target, 0.3f, 10f);
            SummonRotatingSkullWarnings(Timer1);
            SummonStarMeteors(Timer1, reducedStarCount: true);
            return;
        }

        if (Phase2MusicElapsedTime >= Mechanic2Timeline.ArenaBrakeStartMusicTime)
        {
            // 归一化到最短回正方向，并在接近零时精确归零。
            float rotation = MathHelper.WrapAngle(NPC.rotation) * 0.85f;
            NPC.rotation = MathF.Abs(rotation) < 0.001f ? 0f : rotation;
        }
        int arenaBrakeRemaining = Mechanic2Timeline.ArenaBrakeEndMusicTime - Phase2MusicElapsedTime;
        if (arenaBrakeRemaining <= 0)
        {
            NPC.velocity = Vector2.Zero;
            SummonArenaChargeParticles();
        }
        else if (Phase2MusicElapsedTime >= Mechanic2Timeline.ArenaBrakeStartMusicTime)
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
            SoundEngine.PlaySound(SoundID.ForceRoar);
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
        bool summonSlime = Phase2SlimeRainActive && Mechanic1Parameters.IsAttackSpawnTime(
            Phase2MusicElapsedTime, Mechanic1Timeline.SlimeShockwaveMusicTime, Mechanic1Timeline.SlimeEndMusicTime);
        bool firePlatinumCoin = Phase2PlatinumCoinActive && (Phase2MusicElapsedTime - Mechanic1Timeline.PlatinumCoinShockwaveMusicTime) % Mechanic1Parameters.PlatinumCoinInterval == 0;
        if (!Main.dedServ)
        {
            if (summonSlime)
                SoundEngine.PlaySound(SoundID.Item81, NPC.Center);
            if (firePlatinumCoin)
                SoundEngine.PlaySound(PlatinumCoin.UnlimitedTossSound, NPC.Center);
        }
        if (TONetworkEnvironment.NotClient)
        {
            if (Phase2StarRainActive && (Phase2MusicElapsedTime - Mechanic1Timeline.RotationEndMusicTime) % Mechanic1Parameters.StarRainInterval == 0)
            {
                Vector2 position = Target.Center + new Vector2(Main.rand.NextFloat(-1200f, 1200f), -1000f);
                Projectile.NewProjectileAction<FallingStar>(SourceAI, position, new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(6f, 10f)), FallingStarDamage, 0f, action: p =>
                {
                    p.ai[2] = NPC.whoAmI + 1;
                    p.scale = 0.2f;
                    p.timeLeft = Mechanic1Parameters.ProjectileLifetime;
                    p.GetModProjectile<FallingStar>().TrailType = 1;
                    p.netUpdate = true;
                });
            }
            if (summonSlime)
            {
                Vector2 position = Target.Center + new Vector2(Main.rand.NextFloat(-600f, 600f), -1000f);
                Projectile.NewProjectileAction<SlimeMount>(SourceAI, position, Vector2.UnitY * Mechanic1Parameters.SlimeInitialSpeed, SlimeMountDamage, 0f, action: p =>
                {
                    p.ai[1] = 1f;
                    p.ai[2] = NPC.target + 1;
                    p.GetModProjectile<SlimeMount>().MasterIndex = NPC.whoAmI + 1;
                    p.netUpdate = true;
                });
            }
            if (firePlatinumCoin)
            {
                Projectile.NewProjectileAction<PlatinumCoin>(SourceAI, NPC.Center, NPC.GetVelocityTowards(Target, Mechanic1Parameters.PlatinumCoinSpeed), PlatinumCoinDamage, 0f, action: p =>
                {
                    p.ai[0] = NPC.target + 1;
                    p.ai[2] = NPC.whoAmI + 1;
                    p.timeLeft = Mechanic1Parameters.ProjectileLifetime;
                    p.netUpdate = true;
                    p.scale = 2f;
                });
            }
        }

        void RepositionForArena(int remaining)
        {
            const float RitualSelectionDistance = 1200f;
            const float ArenaSelectionDistance = 1000f;
            const float ArenaHeightAboveRitual = 850f;
            const int RitualDestinationSelected = -2;

            int age = Phase2MusicElapsedTime - Mechanic2Timeline.ArenaBrakeStartMusicTime;
            if (age == 0 && TONetworkEnvironment.NotClient)
            {
                foreach (Projectile projectile in Main.ActiveProjectiles)
                {
                    if (projectile.ModProjectile is not LegendaryNightRitual ritual
                        || ritual.SummonedSkeletronHeadIndex != NPC.whoAmI)
                        continue;

                    Vector2 ritualCenter = ritual.RitualCenter;
                    Vector2 arenaCenter = ritualCenter - Vector2.UnitY * ArenaHeightAboveRitual;
                    if (Vector2.DistanceSquared(Target.Center, ritualCenter) <= RitualSelectionDistance * RitualSelectionDistance
                        || Vector2.DistanceSquared(Target.Center, arenaCenter) <= ArenaSelectionDistance * ArenaSelectionDistance)
                    {
                        ArenaApproachDestination = arenaCenter;
                        ArenaApproachState = RitualDestinationSelected;
                        NPC.netUpdate = true;
                    }
                    break;
                }
            }
            if (age < Mechanic2Parameters.ArenaRepositionDelay)
            {
                NPC.velocity *= 0.9f;
                return;
            }
            Vector2 destination = ArenaApproachState == RitualDestinationSelected
                ? ArenaApproachDestination
                : Target.Center - Vector2.UnitY * Mechanic2Parameters.ArenaRepositionHeight;
            if (remaining > Mechanic2Parameters.ArenaRepositionBrakeDuration)
            {
                Vector2 desiredVelocity = (destination - NPC.Center) * 0.2f;
                float speed = desiredVelocity.Length();
                if (speed > Mechanic2Parameters.ArenaRepositionSpeed)
                    desiredVelocity *= Mechanic2Parameters.ArenaRepositionSpeed / speed;
                NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVelocity, 0.3f);
                return;
            }

            // AI64[2/3/4] 分别保存起点、停靠点、初速度；AI32[8] 的 -2/-1 表示仪式停靠/刹停锁定。
            // 机制二重置 AI32[8] 为拍击锁定序号，复用 AI64[2/3]，不再读取初速度。
            if (ArenaApproachState != -1)
            {
                if (!TONetworkEnvironment.NotClient)
                {
                    NPC.velocity *= 0.9f;
                    return;
                }
                ArenaApproachOrigin = NPC.Center;
                ArenaApproachDestination = destination;
                ArenaApproachState = -1;
                ArenaApproachInitialVelocity = NPC.velocity;
                NPC.netUpdate = true;
            }
            Vector2 origin = ArenaApproachOrigin;
            Vector2 target = ArenaApproachDestination;
            Vector2 initialVelocity = ArenaApproachInitialVelocity;
            float progress = MathHelper.Clamp((Mechanic2Parameters.ArenaRepositionBrakeDuration - remaining + 1f)
                / Mechanic2Parameters.ArenaRepositionBrakeDuration, 0f, 1f);
            // Hermite 保留快速移动末速度，抵达玩家上方时切线归零，不在停稳帧瞬移修正。
            Vector2 position = Vector2.Hermite(origin, initialVelocity * Mechanic2Parameters.ArenaRepositionBrakeDuration,
                target, Vector2.Zero, progress);
            NPC.velocity = position - NPC.Center;
        }

        void SummonArenaChargeParticles()
        {
            if (Main.dedServ)
                return;
            int remaining = Mechanic2Timeline.StartMusicTime - Phase2MusicElapsedTime;
            // 留足完整吸入时间，末段仅让已有星芒自然汇入，避免把生命周期压缩成几帧。
            if (remaining < Mechanic2Parameters.ChargeParticleMinLifetime)
                return;
            float charge = MathHelper.Clamp((Phase2MusicElapsedTime - Mechanic2Timeline.ArenaBrakeEndMusicTime)
                / (float)(Mechanic2Timeline.StartMusicTime - Mechanic2Timeline.ArenaBrakeEndMusicTime
                    - Mechanic2Parameters.ChargeParticleMinLifetime), 0f, 1f);
            const int SpiralArmCount = 4;
            int layers = Phase2MusicElapsedTime % 3 == 0 && charge > 0.5f ? 2 : 1;
            for (int layer = 0; layer < layers; layer++)
            {
                // 同批共享半径与寿命，沿用召唤仪式的反向发射和卷入，保持旋臂对称。
                float emissionAngle = -(Phase2MusicElapsedTime - Mechanic2Timeline.ArenaBrakeEndMusicTime) * 0.008f
                    + layer * 0.12f + Main.rand.NextFloat(-0.04f, 0.04f);
                float radius = Main.rand.NextFloat(Mechanic2Parameters.ChargeParticleMinRadius,
                    Mechanic2Parameters.ChargeParticleMaxRadius) * (1f + charge * Mechanic2Parameters.ChargeParticleRangeGrowth);
                int lifetime = Main.rand.Next(Mechanic2Parameters.ChargeParticleMinLifetime,
                    Math.Min(remaining, Mechanic2Parameters.ChargeParticleMaxLifetime) + 1);
                Color color = Color.Lerp(LegendaryNightHandler.StarColor, Color.White, Main.rand.NextFloat(0.3f, 0.9f));
                float scale = Main.rand.NextFloat(0.3f, 0.65f) * (1f + charge * Mechanic2Parameters.ChargeParticleScaleGrowth);
                for (int arm = 0; arm < SpiralArmCount; arm++)
                {
                    float angle = arm * MathHelper.TwoPi / SpiralArmCount + emissionAngle;
                    Vector2 offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
                    AnomalyNPC.SpawnVisualParticle(new LegendaryNightChargeSparkle(NPC.Center, offset, color,
                        scale, lifetime, spiralAngle: 4f));
                }
            }
        }
    }
}