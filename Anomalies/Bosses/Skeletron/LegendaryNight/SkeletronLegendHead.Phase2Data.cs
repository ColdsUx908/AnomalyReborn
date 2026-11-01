namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class SkeletronLegendHead
{
    /// <summary>
    /// 机制1配置；时间节点为音乐播放帧数，60帧/秒。
    /// </summary>
    public static class Mechanic1Data
    {
        public const int RotationEnd = 15 * 60 + 40;
        public const int SlimeShockwave = 22 * 60;
        public const int StarShockwave = 28 * 60 + 20;
        public const int PlatinumCoinShockwave = 34 * 60 + 40;
        public const int SlimeEnd = 41 * 60;
        public const int DemonShadowEnd = 42 * 60 + 33;
        public const int PlatinumCoinEnd = 44 * 60 + 6;
        public const int StarRainEnd = 45 * 60 + 39;
        public const int Phase2ArenaSound = 46 * 60 + 45;
        public const int End = 47 * 60 + 10;
        public const int AttackBeatDuration = StarShockwave - SlimeShockwave;
        public const int AttackIntervalsPerBeat = 8;
        public const int AttackAnimationDuration = (AttackBeatDuration + AttackIntervalsPerBeat - 1) / AttackIntervalsPerBeat;
        public const int PlatinumCoinInterval = 16;
        public const int StarRainInterval = 36;
        public const int ProjectileLifetime = 300;
        public const float PlatinumCoinSpeed = 16f;
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

    /// <summary>
    /// 机制2配置；MusicTime 为音乐绝对帧数，其他节点相对 StartMusicTime。
    /// </summary>
    public static class Mechanic2Data
    {
        public const int StartMusicTime = Mechanic1Data.End;
        public const int ArenaBrakeStartMusicTime = Mechanic1Data.PlatinumCoinEnd;
        public const int ArenaBrakeEndMusicTime = Mechanic1Data.StarRainEnd;
        public const int ArenaBrakeDuration = ArenaBrakeEndMusicTime - ArenaBrakeStartMusicTime;
        public const int ArenaRepositionDelay = 15;
        public const int ArenaRepositionBrakeDuration = 30;
        public const float ArenaRepositionHeight = 320f;
        public const float ArenaRepositionSpeed = 40f;
        public const int ChargeParticleMinLifetime = 60;
        public const int ChargeParticleMaxLifetime = 72;
        public const float ChargeParticleMinRadius = 310f;
        public const float ChargeParticleMaxRadius = 380f;
        public const float ChargeParticleRangeGrowth = 0.5f;
        public const float ChargeParticleScaleGrowth = 0.9f;
        public const int ArenaDepartureDuration = 60;
        public const int HandEntryDuration = 45;
        public static float HandEntryProgress(int age) => MathHelper.SmoothStep(0f, 1f,
            MathHelper.Clamp(age / (float)HandEntryDuration, 0f, 1f));
        public const int FirstStrikeMusicTime = 52 * 60;
        public const int SecondStrikeMusicTime = 55 * 60 + 8;
        public const int ThirdStrikeMusicTime = 58 * 60 + 16;
        public const int EndMusic = Mechanic3Data.StartMusicTime;
        public const int FirstStrike = FirstStrikeMusicTime - StartMusicTime;
        public const int LastStrike = ThirdStrikeMusicTime - StartMusicTime;
        public const int StrikeCount = 3;
        public const int StrikeInterval = SecondStrikeMusicTime - FirstStrikeMusicTime;
        public const int RetreatDuration = 60;
        public const int HandRaiseLeadTime = HalfFieldWarningDuration;
        public const int HandRaiseDuration = HandRaiseLeadTime - BodyRushDuration;
        public const int HandSlamDuration = BodyRushDuration;
        public const int HandRecoverDuration = 30;
        public const int BodyRushDuration = 24;
        public const int BodyRetreatLeadTime = RetreatDuration + BodyRushDuration;
        public const int BodyFollowThroughDuration = 12;
        public const int BodyRecoverDuration = 60;
        public const float BodyFollowThroughDamping = 2f;
        public const float BodyImpactHeight = 420f;
        public const float BodyHeadClearance = 8f;
        public const float BodyReturnArcHeight = 1200f;
        public const float BodyExitZ = AnomalyDepthDrawSystem.MinimumZ - 250f;
        public const int LaserDuration = 24;
        // 下一次预警在前一次拍击伤害窗口结束后开始，避免连续拍击的提示重叠。
        public const int HalfFieldWarningDuration = StrikeInterval - LaserDuration;
        public const int LaserWarningDuration = HalfFieldWarningDuration;
        public const int LaserFadeInDuration = 12;
        public const int LaserFadeOutDuration = 18;
        public const int RainDuration = 80;
        public const int ReturnMusicTime = 59 * 60 + 50;
        public const int ReturnStart = LastStrike + BodyFollowThroughDuration;
        public const int ReturnDuration = ReturnMusicTime - StartMusicTime - ReturnStart;
        public const int End = EndMusic - StartMusicTime;
        public const int FanStarCount = 15;
        public const float FanStarSpread = TOMathUtils.PiOver3;
        public const int FanStarFastDuration = 20;
        public const int FanStarDecelerationDuration = 12;
        public const float FanStarFastRadiusRatio = 0.35f;

        /// <summary>先快速走完35%半径，再减速至匀速；一个拍击间隔累计走完完整半径。</summary>
        public static float GetFanStarSpeed(float radius, int age)
        {
            float fastSpeed = radius * FanStarFastRadiusRatio / FanStarFastDuration;
            float slowSpeed = (radius * (1f - FanStarFastRadiusRatio) - fastSpeed * FanStarDecelerationDuration * 0.5f)
                / (StrikeInterval - FanStarFastDuration - FanStarDecelerationDuration * 0.5f);
            // 减速段按每帧中点采样，使离散速度总和等于目标距离。
            float progress = MathHelper.Clamp((age - FanStarFastDuration + 0.5f) / FanStarDecelerationDuration, 0f, 1f);
            return MathHelper.Lerp(fastSpeed, slowSpeed, progress);
        }

        public const int StarCount = 300;
        public const int StarTrailLength = 12;
        public const float StarSpeed = 32f;
        public const float BackgroundZ = 1800f;
        public const float ForegroundZ = -350f;
        public const float HeightAbovePlayer = 650f;
        /// <summary>接近原版的下垂位置，使靠近手掌的骨节大致竖直。</summary>
        public static Vector2 GetRestingHandOffset(int side) => new(side * 200f, 190f);

        /// <summary>常态衔接前臂；拍击手抬起时朝上、前冲拍下时朝下，回收时恢复相对位置决定的朝向。</summary>
        public static float GetHandRotation(int age, bool selected, float restingRotation)
        {
            int relativeAge = age - GetStrikeTime(GetStrikeIndex(age));
            if (!selected || relativeAge < -HandRaiseLeadTime || relativeAge >= LaserDuration + HandRecoverDuration)
                return restingRotation;
            if (relativeAge < -BodyRushDuration)
                return restingRotation + MathHelper.WrapAngle(MathHelper.Pi - restingRotation) * HandRaiseProgress(relativeAge);
            if (relativeAge <= LaserDuration)
                return 0f;
            return restingRotation * MathHelper.SmoothStep(0f, 1f,
                (relativeAge - LaserDuration) / (float)HandRecoverDuration);
        }

        public static int GetStrikeTime(int index) => FirstStrike + index * StrikeInterval;

        /// <summary>抬手与红色遮罩同步；后退独立采用较短的60帧时长。</summary>
        public static float HandRaiseProgress(int relativeAge) => MathHelper.SmoothStep(0f, 1f,
            MathHelper.Clamp((relativeAge + HandRaiseLeadTime) / (float)HandRaiseDuration, 0f, 1f));

        /// <summary>前冲前短暂后退，结束后立即前冲。</summary>
        public static float BodyRetreatProgress(int relativeAge) => MathHelper.SmoothStep(0f, 1f,
            MathHelper.Clamp((relativeAge + BodyRetreatLeadTime) / (float)RetreatDuration, 0f, 1f));

        /// <summary>每次拍击独立完成后退、穿过观察者和上方回程；各次动作之间回到正常平面。</summary>
        public static float BodyStrikeDepth(int relativeAge)
        {
            if (relativeAge is < -BodyRetreatLeadTime or > (BodyFollowThroughDuration + BodyRecoverDuration))
                return 0f;
            if (relativeAge <= -BodyRushDuration)
                return BackgroundZ * BodyRetreatProgress(relativeAge);
            if (relativeAge <= BodyFollowThroughDuration)
                return MathHelper.Lerp(BackgroundZ, ForegroundZ, BodyStrikeMotion(relativeAge).Foreground);
            float recovery = MathHelper.SmoothStep(0f, 1f,
                (relativeAge - BodyFollowThroughDuration) / (float)BodyRecoverDuration);
            return MathHelper.Lerp(BodyExitZ, 0f, recovery);
        }

        /// <summary>以等面积半径和低差异角度铺满半圆，留下24像素边缘余量，避免跨越安全半场。</summary>
        public static Vector2 GetStarSpawnOffset(int index, float radius, int side)
        {
            const float Inset = 24f;
            const float GoldenRatioFraction = 0.61803398875f;
            float distance = Math.Max(0f, radius - Inset) * MathF.Sqrt((index + 0.5f) / StarCount);
            float angle = MathHelper.Pi * (index * GoldenRatioFraction % 1f) - MathHelper.PiOver2;
            return new Vector2(side * (Inset + Math.Max(0f, MathF.Cos(angle)) * distance), MathF.Sin(angle) * distance);
        }
        public static int GetStrikeIndex(int age)
        {
            for (int i = 0; i < StrikeCount - 1; i++)
                if (age < GetStrikeTime(i) + LaserDuration + HandRecoverDuration)
                    return i;
            return StrikeCount - 1;
        }

        /// <summary>本体冲刺的路径进度和前景权重；拍击后越过观察者，再沿上方弧线退回背景。权重可大于1。</summary>
        public static (float Advance, float Foreground) BodyStrikeMotion(int relativeAge)
        {
            if (relativeAge is < -BodyRushDuration or > (BodyFollowThroughDuration + BodyRecoverDuration))
                return (0f, 0f);
            if (relativeAge <= 0)
            {
                float progress = MathF.Pow((relativeAge + BodyRushDuration) / (float)BodyRushDuration, 3f);
                return (progress, progress);
            }

            // 冲刺末端的速度连续衔接指数阻尼，避免拍击帧突然刹停。
            float followThrough = 3f / BodyRushDuration * BodyFollowThroughDamping
                * (1f - MathF.Exp(-Math.Min(relativeAge, BodyFollowThroughDuration) / BodyFollowThroughDamping));
            float recovery = MathHelper.SmoothStep(0f, 1f,
                MathHelper.Clamp((relativeAge - BodyFollowThroughDuration) / (float)BodyRecoverDuration, 0f, 1f));
            // 在拍击帧保留前冲末速度，再连续减速至观察者之外，避免深度运动突然刹停。
            float exitPower = 3f * (BackgroundZ - ForegroundZ) * BodyFollowThroughDuration
                / (BodyRushDuration * (ForegroundZ - BodyExitZ));
            float exitProgress = 1f - MathF.Pow(1f - MathHelper.Clamp(relativeAge / (float)BodyFollowThroughDuration, 0f, 1f), exitPower);
            float exitWeight = (BackgroundZ - BodyExitZ) / (BackgroundZ - ForegroundZ);
            return ((1f + followThrough) * (1f - recovery),
                MathHelper.Lerp(1f, exitWeight, exitProgress) * (1f - recovery));
        }

        public static float HandAttackProgress(int age)
        {
            int relativeAge = age - GetStrikeTime(GetStrikeIndex(age));
            float raise = HandRaiseProgress(relativeAge);
            float recover = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((relativeAge - LaserDuration) / (float)HandRecoverDuration, 0f, 1f));
            return raise * (1f - recover);
        }

        public static float HandSlamProgress(int relativeAge) => MathF.Pow(
            MathHelper.Clamp((relativeAge + HandSlamDuration) / (float)HandSlamDuration, 0f, 1f), 3f);

        /// <summary>在前一次拍击伤害窗口结束后提示半场，与抬手同步增强；拍击时转为白色冲击。</summary>
        public static float HalfFieldOpacity(int relativeAge)
        {
            if (relativeAge is < -HalfFieldWarningDuration or >= LaserDuration)
                return 0f;
            return relativeAge < 0 ? 0.08f + 0.14f * HandRaiseProgress(relativeAge)
                : 0.45f * (1f - relativeAge / (float)LaserDuration);
        }

        public static bool LasersActive(int age)
        {
            for (int i = 0; i < StrikeCount; i++)
                if (age >= GetStrikeTime(i) && age < GetStrikeTime(i) + LaserDuration)
                    return true;
            return false;
        }

        public static float LaserWarning(int age)
        {
            for (int i = 0; i < StrikeCount; i++)
            {
                int remaining = GetStrikeTime(i) - age;
                if (remaining is > 0 and <= LaserWarningDuration)
                    return 1f - remaining / (float)LaserWarningDuration;
            }
            return 0f;
        }

        /// <summary>在伤害窗口前淡入、结束后淡出；实际碰撞仍精确锁定拍击帧。</summary>
        public static float LaserIntensity(int age)
        {
            float intensity = 0f;
            for (int i = 0; i < StrikeCount; i++)
            {
                int relativeAge = age - GetStrikeTime(i);
                float fadeIn = MathHelper.Clamp((relativeAge + LaserFadeInDuration) / (float)LaserFadeInDuration, 0f, 1f);
                float fadeOut = MathHelper.Clamp((LaserDuration + LaserFadeOutDuration - relativeAge) / (float)LaserFadeOutDuration, 0f, 1f);
                intensity = Math.Max(intensity, MathHelper.SmoothStep(0f, 1f, Math.Min(fadeIn, fadeOut)));
            }
            return intensity;
        }
    }

    /// <summary>机制3史莱姆化身配置；时间节点为音乐绝对帧数，60帧/秒。</summary>
    public static class Mechanic3Data
    {
        public const int StartMusicTime = LegendaryNightPhase2Arena.FirstStarburstMusicTime;
        public const int FanTime = 66 * 60 + 10;
        public const int Ring1Time = 69 * 60 + 20;
        public const int Ring2Time = 70 * 60 + 55;
        public const int JumpTime = 72 * 60 + 30;
        public const int JumpDuration = 90;
        public const int LandingTime = JumpTime + JumpDuration;
        public const int ReturnDuration = 90;
        public const int RainDuration = 20;
        public const int RainLifetime = 600;
        public const int RainLayers = 3;
        public const int FinalRainLayers = 9;
        public const float RainSpawnHeight = 800f;
        public const float RainLayerSpacing = 125f;
        public const float RainHalfWidth = 40f;
        public const float RainSpeed = 20f;
        public const int FanCount = 11;
        public const int RingCount = 24;
        public const int ChargeDuration = 90;
        public const float MountSpeed = 16f;
        public const float MountSpinSpeed = 0.12f;
        public const float JumpGravity = 1.6f;
    }

    /// <summary>机制4铂金币直冲；音乐绝对帧数。</summary>
    public static class Mechanic4Data
    {
        public const int StartMusicTime = LegendaryNightPhase2Arena.SecondStarburstMusicTime;
        public const int FirstChargeTime = 78 * 60 + 47;
        public const int ChargeInterval = 95;
        public const int ChargeCount = 4;
        public const int ChargeDuration = (ChargeInterval + 1) / 2;
        public const int DecelerationDuration = 8;
        public const int RetreatDuration = ChargeInterval - ChargeDuration;
        public const int FirstRetreatStart = FirstChargeTime - RetreatDuration;
        public const int ReturnStart = FirstChargeTime + (ChargeCount - 1) * ChargeInterval + ChargeDuration;
        public const int EndMusicTime = LegendaryNightPhase2Arena.ThirdStarburstMusicTime;
        public const float RetreatSpeed = 12f;
        public const float ChargeSpeed = 32f;
        public static int GetChargeTime(int index) => FirstChargeTime + index * ChargeInterval;
        public static float ChargeSpeedFactor(int age) => MathHelper.Clamp(
            (ChargeDuration - age - 1f) / DecelerationDuration, 0f, 1f);

        public static bool LasersActive(int time) => LaserAge(time) is >= 0 and < Mechanic2Data.LaserDuration;

        public static float LaserWarning(int time)
        {
            int age = LaserAge(time);
            return age is < 0 and >= -ChargeDuration ? 1f + age / (float)ChargeDuration : 0f;
        }

        public static float LaserIntensity(int time)
        {
            int age = LaserAge(time);
            if (age < -Mechanic2Data.LaserFadeInDuration)
                return 0f;
            float fadeIn = MathHelper.Clamp((age + Mechanic2Data.LaserFadeInDuration) / (float)Mechanic2Data.LaserFadeInDuration, 0f, 1f);
            float fadeOut = MathHelper.Clamp((Mechanic2Data.LaserDuration + Mechanic2Data.LaserFadeOutDuration - age)
                / (float)Mechanic2Data.LaserFadeOutDuration, 0f, 1f);
            return MathHelper.SmoothStep(0f, 1f, Math.Min(fadeIn, fadeOut));
        }

        public static int LaserAge(int time)
        {
            for (int i = 0; i < ChargeCount; i++)
            {
                int age = time - GetChargeTime(i) - ChargeDuration;
                if (age is >= -ChargeDuration and < Mechanic2Data.LaserDuration + Mechanic2Data.LaserFadeOutDuration)
                    return age;
            }
            return int.MinValue;
        }
    }

    /// <summary>机制5常态星星雨及连续星爆。</summary>
    public static class Mechanic5Data
    {
        public const int StartMusicTime = LegendaryNightPhase2Arena.ThirdStarburstMusicTime;
        public const int RainEnd = LegendaryNightPhase2Arena.FourthStarburstMusicTime - 30;
        public const int RainInterval = 20;
        public const int RainLifetime = Mechanic1Data.ProjectileLifetime;
        public const int StarburstInitialStarCount = 10;
        public const int StarburstStarCountIncrement = 2;
        public static int GetStarburstStarCount(int starburstIndex) => StarburstInitialStarCount + (starburstIndex - 4) * StarburstStarCountIncrement;

        public const int HandReappearanceDuration = 45;
        public const int HandAttackCount = 8;
        public const int HandStrikeDelay = 47;
        public const int HandWindupDuration = 24;
        public const int HandStrikeDuration = 32;
        public const int HandReturnDuration = 36;
        public const int EndMusicTime = LegendaryNightPhase2Arena.StarburstEndMusicTime + HandStrikeDelay + HandStrikeDuration + HandReturnDuration;
        public const float HandWindupDistance = 80f;
        public const float HandOvershoot = 48f;

        // 星爆采用0-based索引；第5至12次星爆各延后47帧开始前挥。
        public static int GetHandStrikeTime(int attack) => LegendaryNightPhase2Arena.GetStarburstTime(4 + attack) + HandStrikeDelay;

        public static int GetHandAttackIndex(int musicTime)
        {
            for (int attack = 0; attack < HandAttackCount; attack++)
            {
                int age = musicTime - GetHandStrikeTime(attack);
                if (age is >= -HandWindupDuration and < HandStrikeDuration + HandReturnDuration)
                    return attack;
            }
            return -1;
        }

        /// <summary>沿锁定玩家的方向挥到法阵外沿，并留出越过边缘的余量。</summary>
        public static Vector2 GetHandStrikeDestination(Vector2 origin, Vector2 target, Vector2 center, float radius)
        {
            Vector2 direction = (target - origin).SafeNormalize(Vector2.UnitY);
            Vector2 offset = origin - center;
            float projection = Vector2.Dot(offset, direction);
            float distance = -projection + MathF.Sqrt(Math.Max(0f, projection * projection + radius * radius - offset.LengthSquared()));
            return origin + direction * (Math.Max(0f, distance) + HandOvershoot);
        }
    }

    /// <summary>机制6；全部时间为音乐绝对帧数，半拍按设计向后47帧。</summary>
    public static class Mechanic6Data
    {
        public const int StartMusicTime = 113 * 60 + 35;
        public const int AbsorbMusicTime = 114 * 60 + 5;
        public const int ExpandMusicTime = 114 * 60 + 22;
        public const int ArenaTransitionDuration = 10;
        public const int EndMusicTime = 131 * 60 + 47;
        public const int HandThrowDuration = 12;
        public const int HandReturnDuration = 24;
        public const float SphereRadius = 240f;
        public const float ThrowSpeed = 18f;
        public static int ChargeTime(int index) => index switch
        {
            0 => 113 * 60 + 35,
            1 => 119 * 60 + 55,
            2 => 126 * 60 + 15,
            _ => 129 * 60 + 25
        };
        public static int ThrowTime(int index) => index switch
        {
            0 => 118 * 60 + 20,
            1 => 124 * 60 + 40,
            2 => 127 * 60 + 50,
            _ => 131 * 60
        };
        public static int ExplosionTime(int index) => index switch
        {
            0 => 119 * 60 + 55,
            1 => 126 * 60 + 15,
            2 => 128 * 60 + 37,
            _ => 131 * 60 + 47
        };
        public static int StarCount(int index) => index switch { 0 => 70, 1 => 60, _ => 40 };
        public static float SizeRatio(int index) => index switch { 0 => 1f, 1 => 0.9f, _ => 0.585f };
        public static int Cycle(int time) => time < ChargeTime(1) ? 0 : time < ChargeTime(2) ? 1 : time < ChargeTime(3) ? 2 : 3;
        public static float ChargeProgress(int time, int index) => MathHelper.Clamp(
            (time - ChargeTime(index)) / (float)(ThrowTime(index) - ChargeTime(index)), 0f, 1f);
        // Ares 的 LargeTeslaOrbBlast 使用三次方增长，开始时微小、后半程迅速凝聚。
        public static float Radius(int time, int index) => MathHelper.Lerp(2f, SphereRadius * SizeRatio(index),
            MathF.Pow(ChargeProgress(time, index), 3f));
        public static Vector2 SphereOffset(int time, int index) => new(0f, -180f - Radius(time, index));
        public static float AbsorbProgress(int time) => MathHelper.SmoothStep(0f, 1f,
            MathHelper.Clamp((time - AbsorbMusicTime) / (float)ArenaTransitionDuration, 0f, 1f));
        public static float ArenaScale(int time) => MathHelper.Lerp(1f, 1.5f, MathHelper.SmoothStep(0f, 1f,
            MathHelper.Clamp((time - ExpandMusicTime) / (float)ArenaTransitionDuration, 0f, 1f)));
    }

    /// <summary>机制7；全部时间为音乐绝对帧数。</summary>
    public static class Mechanic7Data
    {
        public const int StartMusicTime = 132 * 60 + 30;
        public const int ArenaFadeDuration = 30;
        public const int HandExtendDuration = 75;
        public const int RotationStart = 134 * 60 + 5;
        public const int GrowthDuration = 90;
        public const int ThrowTime = 135 * 60 + 40;
        public const int ReturnTime = 137 * 60 + 15;
        public const int RecoverTime = 138 * 60 + 50;
        public const int HandRecoverDuration = 30;
        public const int StarCount = 30;
        public const float HandRadius = 400f;
        public const float NetRadius = 200f;
        public const float HandAngularSpeed = MathHelper.TwoPi / 30f;
        public const float NetAngularSpeed = MathHelper.TwoPi / 30f;
        public const float StarScale = 0.2f;

        public static float Radius(int time) => NetRadius
            * MathHelper.Clamp((time - RotationStart) / (float)GrowthDuration, 0f, 1f)
            * (1f - MathHelper.Clamp((time - ReturnTime) / (float)(RecoverTime - ReturnTime), 0f, 1f));

        // 与 Meteor 相同，依次连接间隔一个顶点的五个顶点，并在线段上等分。
        public static Vector2 StarOffset(int index, float radius, float rotation)
        {
            float edgePosition = index * 5f / StarCount;
            int edge = (int)edgePosition;
            Vector2 start = (rotation + TOMathUtils.PiOver5 * 4f * edge).ToRotationVector2() * radius;
            Vector2 end = (rotation + TOMathUtils.PiOver5 * 4f * (edge + 1)).ToRotationVector2() * radius;
            return Vector2.Lerp(start, end, edgePosition - edge);
        }
    }
}
