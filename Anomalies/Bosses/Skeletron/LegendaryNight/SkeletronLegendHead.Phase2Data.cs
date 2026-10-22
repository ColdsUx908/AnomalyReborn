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
        public const int DemonShadowEnd = 42 * 60 + 30;
        public const int PlatinumCoinEnd = 44 * 60;
        public const int StarRainEnd = 45 * 60 + 30;
        public const int Phase2ArenaSound = 46 * 60 + 45;
        public const int End = 47 * 60 + 10;
        public const int AttackBeatDuration = StarShockwave - SlimeShockwave;
        public const int AttackIntervalsPerBeat = 8;
        public const int AttackAnimationDuration = (AttackBeatDuration + AttackIntervalsPerBeat - 1) / AttackIntervalsPerBeat;
        public const int PlatinumCoinInterval = 8;
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
        public const int FirstStrikeMusicTime = 53 * 60 + 30;
        public const int SecondStrikeMusicTime = 59 * 60 + 50;
        public const int ThirdStrikeMusicTime = 66 * 60 + 10;
        public const int Star1MusicTime = 56 * 60 + 45;
        public const int Star2MusicTime = 57 * 60 + 20;
        public const int Star3MusicTime = 63 * 60;
        public const int Star4MusicTime = 63 * 60 + 12;
        public const int Star5MusicTime = 63 * 60 + 24;
        public const int Star6MusicTime = 63 * 60 + 36;
        public const int Star7MusicTime = 67 * 60 + 48;
        public const int Star8MusicTime = 68 * 60;
        public const int Star9MusicTime = 68 * 60 + 12;
        public const int Star10MusicTime = 68 * 60 + 24;
        public const int Star11MusicTime = 68 * 60 + 48;
        public const int Star12MusicTime = 69 * 60 + 12;
        public const int Star13MusicTime = 69 * 60 + 24;
        public const int EndMusic = 72 * 60 + 30;
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
        public const int HalfFieldWarningDuration = 3 * 60;
        public const int LaserWarningDuration = HalfFieldWarningDuration;
        public const int LaserFadeInDuration = 12;
        public const int LaserFadeOutDuration = 18;
        public const int RainDuration = 80;
        public const int ReturnStart = LastStrike + RainDuration;
        public const int ReturnDuration = 60;
        public const int End = EndMusic - StartMusicTime;
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

        /// <summary>提前3秒提示半场，与抬手同步增强；拍击时转为白色冲击。</summary>
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

    /// <summary>
    /// 机制3配置；时间节点为音乐绝对帧数，60帧/秒。
    /// </summary>
    public static class Mechanic3Data
    {
        public const int StartMusicTime = 72 * 60 + 30;
        public const int SecondCircleMusicTime = 75 * 60 + 40;
        public const int CircleCount = 2;
        public const int StarCount = 30;
        public const int ExpansionDelay = 30;
        public const int ExpansionDuration = 150;
        public const float MaximumRadius = 1500f;
        public const float SecondCircleMaximumRadius = 160f;
        public const int SpawnParticleCount = 150;
        public const float ConnectionWidth = 2f;
        public const float ConnectionGlowWidth = 10f;
        public static readonly Color ConnectionColor = new(255, 80, 20);

        public static int GetStartMusicTime(int circleIndex) => circleIndex == 1 ? StartMusicTime : SecondCircleMusicTime;

        public static Vector2 GetStarPosition(LegendaryNightPhase2Arena arena, int index, int age, int circleIndex = 1)
        {
            (Vector2 center, float initialRadius, float rotation) = arena.GetMagicCircle(circleIndex);
            float progress = MathHelper.Clamp((age - ExpansionDelay) / (float)ExpansionDuration, 0f, 1f);
            float radius = MathHelper.Lerp(initialRadius, circleIndex == 2 ? SecondCircleMaximumRadius : MaximumRadius, progress);
            // 与陨石爆炸相同，按隔一个顶点的顺序连接，五条线各取6个点且不重复端点。
            float edgePosition = index * 5f / StarCount;
            int edge = (int)edgePosition;
            Vector2 start = (rotation + MathHelper.TwoPi * (edge * 2) / 5f).ToRotationVector2();
            Vector2 end = (rotation + MathHelper.TwoPi * (edge * 2 + 2) / 5f).ToRotationVector2();
            return center + Vector2.Lerp(start, end, edgePosition - edge) * radius;
        }
    }
}
