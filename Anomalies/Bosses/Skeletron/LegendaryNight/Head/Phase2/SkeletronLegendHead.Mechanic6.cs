namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class SkeletronLegendHead
{
    /// <summary>机制 6 的音乐绝对帧节点；从转场音乐开始累计，单位为游戏帧。</summary>
    public static class Mechanic6Timeline
    {
        /// <summary>本机制开始的音乐绝对帧（包含此帧）。</summary>
        public const int StartMusicTime = 113 * 60 + 35;
        /// <summary>本机制结束的音乐绝对帧（不包含此帧），与下一机制的开始帧衔接。</summary>
        public const int EndMusicTime = Mechanic7Timeline.StartMusicTime;
        /// <summary>伴星开始的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int CompanionStartMusicTime = 138 * 60 + 45;
        /// <summary>伴星撞击的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int CompanionImpactMusicTime = 153 * 60;
        /// <summary>手部下垂开始的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int HandLowerStartMusicTime = CompanionStartMusicTime;
        /// <summary>头部回归开始的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int HeadReturnStartMusicTime = HandLowerStartMusicTime + Mechanic6Parameters.HandLowerDuration;
        /// <summary>手部离场开始的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int HandDepartureStartMusicTime = HeadReturnStartMusicTime + Mechanic6Parameters.HeadReturnDuration;
        /// <summary>手部准备开始的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int HandPrepareStartMusicTime = Mechanic5Timeline.StarburstEndMusicTime;
        /// <summary>手部准备结束的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int HandPrepareEndMusicTime = StartMusicTime - 1;
        /// <summary>重新定位开始的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int RepositionStartMusicTime = StartMusicTime - Mechanic6Parameters.RepositionDuration;
        /// <summary>蓄力完成的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int ChargeCompleteMusicTime = StartMusicTime + 16 * Mechanic6Parameters.LightOrbInterval;
        /// <summary>吸收的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int AbsorbMusicTime = 114 * 60 + 5;
        /// <summary>膨胀的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int ExpandMusicTime = 114 * 60 + 22;
        /// <summary>光球首次的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int LightOrbFirstMusicTime = StartMusicTime;
        /// <summary>光球吸收完成的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int LightOrbAbsorptionCompleteMusicTime = CompanionImpactMusicTime - 30;
        /// <summary>光球结束的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int LightOrbEndMusicTime = 161 * 60;
        /// <summary>激光首次的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int LaserFirstMusicTime = 118 * 60 + 20;
        /// <summary>最后一波激光开始造成伤害的音乐绝对帧。</summary>
        public const int LastLaserImpactMusicTime = LaserFirstMusicTime
            + (Mechanic6Parameters.LaserWaveCount - 1) * Mechanic6Parameters.LaserInterval + Mechanic6Parameters.LaserWarningDuration;
    }

    /// <summary>机制 6 的攻击、运动和表现参数及纯计算；时长单位为游戏帧。</summary>
    public static class Mechanic6Parameters
    {
        /// <summary>手部下垂持续时间，单位为游戏帧。</summary>
        public const int HandLowerDuration = 60;
        /// <summary>头部回归持续时间，单位为游戏帧。</summary>
        public const int HeadReturnDuration = 90;
        /// <summary>手部离场持续时间，单位为游戏帧。</summary>
        public const int HandDepartureDuration = 45;
        /// <summary>计算手部离场透明度。</summary>
        public static float HandDepartureOpacity(int time) => 1f - MathHelper.SmoothStep(0f, 1f,
            MathHelper.Clamp((time - Mechanic6Timeline.HandDepartureStartMusicTime) / (float)HandDepartureDuration, 0f, 1f));
        /// <summary>计算手部下垂进度。</summary>
        public static float HandLowerProgress(int time) => MathHelper.SmoothStep(0f, 1f,
            MathHelper.Clamp((time - Mechanic6Timeline.HandLowerStartMusicTime) / (float)HandLowerDuration, 0f, 1f));
        /// <summary>计算头部回归进度。</summary>
        public static float HeadReturnProgress(int time) => MathHelper.SmoothStep(0f, 1f,
            MathHelper.Clamp((time - Mechanic6Timeline.HeadReturnStartMusicTime) / (float)HeadReturnDuration, 0f, 1f));

        /// <summary>重新定位持续时间，单位为游戏帧。</summary>
        public const int RepositionDuration = 60;
        /// <summary>场地过渡持续时间，单位为游戏帧。</summary>
        public const int ArenaTransitionDuration = 30;
        /// <summary>场地目标半径，单位为世界像素。</summary>
        public const float ArenaTargetRadius = 800f;
        /// <summary>恒星半径，单位为世界像素。</summary>
        public const float SphereRadius = 240f;
        /// <summary>光球间隔，单位为游戏帧。</summary>
        public const int LightOrbInterval = 95;
        /// <summary>光球数量。</summary>
        public const int LightOrbCount = 12;
        /// <summary>光球最小存活时间，单位为游戏帧。</summary>
        public const int LightOrbMinimumLifetime = 12 * 60;
        /// <summary>光球存活时间步长。</summary>
        public const int LightOrbLifetimeStep = 30;
        /// <summary>光球存活时间窗口，单位为游戏帧。</summary>
        public const int LightOrbLifetimeWindow = 60;
        /// <summary>光球显现持续时间，单位为游戏帧。</summary>
        public const int LightOrbAppearanceDuration = 30;
        /// <summary>光球吸收持续时间，单位为游戏帧。</summary>
        public const int LightOrbAbsorptionDuration = 30;
        /// <summary>光球吸收半径比例。</summary>
        public const float LightOrbAbsorptionRadiusRatio = 1.5f;
        /// <summary>光球核心闪光持续时间，单位为游戏帧。</summary>
        public const int LightOrbCoreFlashDuration = 24;

        /// <summary>判断当前音乐帧是否生成光球。</summary>
        public static bool ShouldSpawnLightOrb(int time) => time >= Mechanic6Timeline.LightOrbFirstMusicTime
            && time <= Mechanic6Timeline.LightOrbFirstMusicTime + (LightOrbCount - 1) * LightOrbInterval
            && (time - Mechanic6Timeline.LightOrbFirstMusicTime) % LightOrbInterval == 0;

        /// <summary>计算光球存活时间。</summary>
        public static int GetLightOrbLifetime(int index, float sample) => LightOrbMinimumLifetime
            + index * LightOrbLifetimeStep + (int)(MathHelper.Clamp(sample, 0f, 1f) * LightOrbLifetimeWindow);

        /// <summary>计算光球显现。</summary>
        public static float LightOrbAppearance(int age) => MathHelper.SmoothStep(0f, 1f,
            MathHelper.Clamp(age / (float)LightOrbAppearanceDuration, 0f, 1f));
        /// <summary>计算光球吸收。</summary>
        public static float LightOrbAbsorption(int age, int lifetime = LightOrbMinimumLifetime) => MathHelper.Clamp(
            (age - (lifetime - LightOrbAbsorptionDuration)) / (float)LightOrbAbsorptionDuration, 0f, 1f);
        /// <summary>计算光球距离。</summary>
        public static float LightOrbDistance(int age, float spawnRadius, float sphereRadius,
            int lifetime = LightOrbMinimumLifetime, int? originalLifetime = null)
        {
            float edge = sphereRadius * LightOrbAbsorptionRadiusRatio;
            int driftDuration = lifetime - LightOrbAbsorptionDuration;
            int originalDriftDuration = (originalLifetime ?? lifetime) - LightOrbAbsorptionDuration;
            if (age <= driftDuration)
                return MathHelper.Lerp(spawnRadius, edge, MathHelper.Clamp(age / (float)originalDriftDuration, 0f, 1f));
            float absorb = LightOrbAbsorption(age, lifetime);
            // 提前吸入时从原漂移轨迹的当前位置衔接，避免瞬移。
            float absorptionRadius = MathHelper.Lerp(spawnRadius, edge,
                MathHelper.Clamp(driftDuration / (float)originalDriftDuration, 0f, 1f));
            return absorptionRadius * (1f - absorb * absorb);
        }

        /// <summary>计算光球径向和切向的绘制缩放。</summary>
        public static Vector2 LightOrbDrawScale(int age, int lifetime = LightOrbMinimumLifetime)
        {
            float absorb = LightOrbAbsorption(age, lifetime);
            float remaining = 1f - absorb;
            float stretch = 1f + 5f * MathF.Sin(absorb * MathHelper.Pi);
            // 径向拉长、切向压缩；面积始终按remaining²缩小，抵达中心时两轴均为零。
            return new Vector2(stretch * remaining, remaining / stretch) * LightOrbAppearance(age);
        }

        /// <summary>计算光球核心闪光。</summary>
        public static float LightOrbCoreFlash(float age) => age < 0f ? 0f
            : MathF.Pow(MathHelper.Clamp(1f - age / LightOrbCoreFlashDuration, 0f, 1f), 2f);
        /// <summary>激光预警持续时间，单位为游戏帧。</summary>
        public const int LaserWarningDuration = 95;
        /// <summary>激光显现持续时间，单位为游戏帧。</summary>
        public const int LaserAppearDuration = 15;
        /// <summary>激光旋转持续时间，单位为游戏帧。</summary>
        public const int LaserRotationDuration = 50;
        /// <summary>激光保持持续时间，单位为游戏帧。</summary>
        public const int LaserHoldDuration = 30;
        /// <summary>激光持续时间，单位为游戏帧。</summary>
        public const int LaserDuration = 20;
        /// <summary>激光间隔，单位为游戏帧。</summary>
        public const int LaserInterval = 190;
        /// <summary>激光波次数量。</summary>
        public const int LaserWaveCount = 8;
        /// <summary>激光长度，单位为世界像素。</summary>
        public const float LaserLength = 2000f;
        /// <summary>激光宽度，单位为世界像素。</summary>
        public const float LaserWidth = 32f;
        /// <summary>各波次激光数量（按波次从零开始索引）。</summary>
        public static ReadOnlySpan<int> LaserCounts => [3, 3, 4, 4, 5, 5, 6, 12];
        /// <summary>激光最大转角。</summary>
        public const float LaserMaxTurnAngle = MathHelper.Pi;

        /// <summary>判断当前音乐帧是否生成激光。</summary>
        public static bool ShouldSpawnLaser(int time) => time >= Mechanic6Timeline.LaserFirstMusicTime
            && time <= Mechanic6Timeline.LaserFirstMusicTime + (LaserWaveCount - 1) * LaserInterval
            && (time - Mechanic6Timeline.LaserFirstMusicTime) % LaserInterval == 0;

        // 从正上方开始均分扇区；相邻扇区各留一半最小夹角，连同首尾接缝保证90° / count。
        /// <summary>计算激光角度。</summary>
        public static float GetLaserAngle(int sector, int count, float sample)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
            if ((uint)sector >= (uint)count)
                throw new ArgumentOutOfRangeException(nameof(sector));
            float sectorAngle = MathHelper.TwoPi / count;
            float separation = count == 1 ? 0f : MathHelper.PiOver2 / count;
            return -MathHelper.PiOver2 + sector * sectorAngle + separation * 0.5f
                + MathHelper.Clamp(sample, 0f, 1f) * (sectorAngle - separation);
        }

        /// <summary>计算激光转角。</summary>
        public static float GetLaserTurnAngle(float sample) => MathHelper.Lerp(-LaserMaxTurnAngle,
            LaserMaxTurnAngle, MathHelper.Clamp(sample, 0f, 1f));

        /// <summary>计算激光旋转。</summary>
        public static float GetLaserRotation(float targetAngle, float turnAngle, int age) =>
            targetAngle - turnAngle * (1f - LaserRotationProgress(age));

        /// <summary>计算激光旋转进度。</summary>
        public static float LaserRotationProgress(int age) => MathHelper.SmoothStep(0f, 1f,
            MathHelper.Clamp((age - LaserAppearDuration) / (float)LaserRotationDuration, 0f, 1f));
        // 头部中心距球面64像素，按默认102像素头部高度留出约13像素空隙。
        /// <summary>头部中心至恒星边缘距离。</summary>
        public const float HeadCenterToSphereEdge = 64f;
        /// <summary>计算蓄力进度。</summary>
        public static float ChargeProgress(int time) => MathHelper.Clamp(
            (time - Mechanic6Timeline.StartMusicTime) / (float)(Mechanic6Timeline.ChargeCompleteMusicTime - Mechanic6Timeline.StartMusicTime), 0f, 1f);
        // 指数1.5缓出增长，在16×95帧后达到并保持最大半径。
        /// <summary>计算半径。</summary>
        public static float Radius(int time) => TOMathUtils.Interpolation.ExponentialEaseOut(2f, SphereRadius, ChargeProgress(time), 1.5f);
        /// <summary>计算恒星偏移。</summary>
        public static Vector2 SphereOffset(int time) => new(0f, -HeadCenterToSphereEdge - Radius(time));
        /// <summary>计算吸收进度。</summary>
        public static float AbsorbProgress(int time) => MathHelper.SmoothStep(0f, 1f,
            MathHelper.Clamp((time - Mechanic6Timeline.AbsorbMusicTime) / (float)ArenaTransitionDuration, 0f, 1f));
        /// <summary>计算场地缩放。</summary>
        public static float ArenaScale(int time) => MathHelper.Lerp(1f,
            ArenaTargetRadius / (LegendaryNightPhase2Arena.MainCircleFullRadius * LegendaryNightPhase2Arena.OuterCircleRadiusRatio),
            MathHelper.SmoothStep(0f, 1f,
            MathHelper.Clamp((time - Mechanic6Timeline.ExpandMusicTime) / (float)ArenaTransitionDuration, 0f, 1f)));
    }

    private void Mechanic6AI()
    {
        #region 举球定位与蓄力步骤同步

        int time = Phase2MusicElapsedTime;
        AnomalyNPC.Z = 0f;
        NPC.rotation = 0f;
        NPC.velocity = Phase2Arena is LegendaryNightPhase2Arena arena
            ? arena.Projectile.Center - Mechanic6Parameters.SphereOffset(time) * (1f - Mechanic6Parameters.HeadReturnProgress(time)) - NPC.Center : Vector2.Zero;
        // LocalPhase：0蓄力，1持续保持满蓄力。
        int localPhase = time >= Mechanic6Timeline.ChargeCompleteMusicTime ? 1 : 0;
        if (TONetworkEnvironment.NotClient && CurrentLocalPhase != localPhase)
            ChangePhase2State(CurrentPhase, localPhase);

        #endregion

        #region 恒星生成

        if (time == Mechanic6Timeline.StartMusicTime)
        {
            SoundEngine.PlaySound(SoundID.ForceRoar);
            ReleaseSpellShockwave(Spell.Star);
            if (TONetworkEnvironment.NotClient)
                Projectile.NewProjectileAction<LegendEnergySphere>(SourceAI,
                    NPC.Center + Mechanic6Parameters.SphereOffset(time), Vector2.Zero, EnergySphereDamage, 0f,
                    action: p =>
                    {
                        p.ai[0] = NPC.whoAmI + 1;
                        p.netUpdate = true;
                    });
        }

        #endregion
    }
}
