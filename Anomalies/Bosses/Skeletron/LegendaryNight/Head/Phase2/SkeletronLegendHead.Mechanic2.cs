namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class SkeletronLegendHead
{
    /// <summary>机制 2 的音乐绝对帧节点；从转场音乐开始累计，单位为游戏帧。</summary>
    public static class Mechanic2Timeline
    {
        /// <summary>本机制开始的音乐绝对帧（包含此帧）。</summary>
        public const int StartMusicTime = Mechanic1Timeline.EndMusicTime;
        /// <summary>本机制结束的音乐绝对帧（不包含此帧），与下一机制的开始帧衔接。</summary>
        public const int EndMusicTime = Mechanic3Timeline.StartMusicTime;
        /// <summary>场地制动开始的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int ArenaBrakeStartMusicTime = Mechanic1Timeline.PlatinumCoinEndMusicTime;
        /// <summary>场地制动结束的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int ArenaBrakeEndMusicTime = Mechanic1Timeline.StarRainEndMusicTime;
        /// <summary>首次拍击的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int FirstStrikeMusicTime = 52 * 60;
        /// <summary>第二次拍击的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int SecondStrikeMusicTime = 55 * 60 + 8;
        /// <summary>第三次拍击的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int ThirdStrikeMusicTime = 58 * 60 + 16;
        /// <summary>回归的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int ReturnMusicTime = 59 * 60 + 50;
    }

    /// <summary>机制 2 的攻击、运动和表现参数及纯计算；时长单位为游戏帧。</summary>
    public static class Mechanic2Parameters
    {
        #region 时长、运动与攻击参数

        /// <summary>场地制动持续时间，单位为游戏帧。</summary>
        public const int ArenaBrakeDuration = Mechanic2Timeline.ArenaBrakeEndMusicTime - Mechanic2Timeline.ArenaBrakeStartMusicTime;
        /// <summary>场地重新定位延迟，单位为游戏帧。</summary>
        public const int ArenaRepositionDelay = 15;
        /// <summary>场地重新定位制动持续时间，单位为游戏帧。</summary>
        public const int ArenaRepositionBrakeDuration = 30;
        /// <summary>场地重新定位高度，单位为世界像素。</summary>
        public const float ArenaRepositionHeight = 320f;
        /// <summary>场地重新定位速度，以世界像素和游戏帧计量。</summary>
        public const float ArenaRepositionSpeed = 40f;
        /// <summary>蓄力粒子最小存活时间，单位为游戏帧。</summary>
        public const int ChargeParticleMinLifetime = 60;
        /// <summary>蓄力粒子最大存活时间，单位为游戏帧。</summary>
        public const int ChargeParticleMaxLifetime = 72;
        /// <summary>蓄力粒子最小半径，单位为世界像素。</summary>
        public const float ChargeParticleMinRadius = 310f;
        /// <summary>蓄力粒子最大半径，单位为世界像素。</summary>
        public const float ChargeParticleMaxRadius = 380f;
        /// <summary>蓄力粒子范围增长量。</summary>
        public const float ChargeParticleRangeGrowth = 0.5f;
        /// <summary>蓄力粒子缩放增长量。</summary>
        public const float ChargeParticleScaleGrowth = 0.9f;
        /// <summary>场地离场持续时间，单位为游戏帧。</summary>
        public const int ArenaDepartureDuration = 60;
        /// <summary>手部入场持续时间，单位为游戏帧。</summary>
        public const int HandEntryDuration = 45;
        /// <summary>非拍击手下垂时相对本体的横向距离，给骨节保留自然展开空间，单位为世界像素。</summary>
        public const float PassingHandHorizontalOffset = 200f;
        /// <summary>非拍击手下垂时相对本体的高度，单位为世界像素；前冲时从玩家侧方掠过。</summary>
        public const float PassingHandVerticalOffset = 160f;
        /// <summary>第一次拍击相对机制二起点的帧数。</summary>
        public const int FirstStrikeAge = Mechanic2Timeline.FirstStrikeMusicTime - Mechanic2Timeline.StartMusicTime;
        /// <summary>最后一次拍击相对机制二起点的帧数。</summary>
        public const int LastStrikeAge = Mechanic2Timeline.ThirdStrikeMusicTime - Mechanic2Timeline.StartMusicTime;
        /// <summary>拍击数量。</summary>
        public const int StrikeCount = 3;
        /// <summary>拍击间隔，单位为游戏帧。</summary>
        public const int StrikeInterval = Mechanic2Timeline.SecondStrikeMusicTime - Mechanic2Timeline.FirstStrikeMusicTime;
        /// <summary>后撤持续时间，单位为游戏帧。</summary>
        public const int RetreatDuration = 60;
        /// <summary>手部抬起提前量，单位为游戏帧。</summary>
        public const int HandRaiseLeadTime = HalfFieldWarningDuration;
        /// <summary>手部抬起持续时间，单位为游戏帧。</summary>
        public const int HandRaiseDuration = HandRaiseLeadTime - BodyRushDuration;
        /// <summary>手部拍下持续时间，单位为游戏帧。</summary>
        public const int HandSlamDuration = BodyRushDuration;
        /// <summary>手部恢复持续时间，单位为游戏帧。</summary>
        public const int HandRecoverDuration = 30;
        /// <summary>主体前冲持续时间，单位为游戏帧。</summary>
        public const int BodyRushDuration = 24;
        /// <summary>主体后撤提前量，单位为游戏帧。</summary>
        public const int BodyRetreatLeadTime = RetreatDuration + BodyRushDuration;
        /// <summary>主体惯性前冲持续时间，单位为游戏帧。</summary>
        public const int BodyFollowThroughDuration = 12;
        /// <summary>主体恢复持续时间，单位为游戏帧。</summary>
        public const int BodyRecoverDuration = 60;
        /// <summary>主体惯性前冲阻尼系数。</summary>
        public const float BodyFollowThroughDamping = 2f;
        /// <summary>主体撞击高度，单位为世界像素。</summary>
        public const float BodyImpactHeight = 420f;
        /// <summary>主体头部间隙，单位为世界像素。</summary>
        public const float BodyHeadClearance = 8f;
        /// <summary>主体回归弧高，单位为世界像素。</summary>
        public const float BodyReturnArcHeight = 1200f;
        /// <summary>主体离场深度。</summary>
        public const float BodyExitZ = AnomalyDepthDrawSystem.MinimumZ - 250f;
        /// <summary>激光持续时间，单位为游戏帧。</summary>
        public const int LaserDuration = 24;
        // 下一次预警在前一次拍击伤害窗口结束后开始，避免连续拍击的提示重叠。
        /// <summary>半场预警持续时间，单位为游戏帧。</summary>
        public const int HalfFieldWarningDuration = StrikeInterval - LaserDuration;
        /// <summary>激光预警持续时间，单位为游戏帧。</summary>
        public const int LaserWarningDuration = HalfFieldWarningDuration;
        /// <summary>激光淡入持续时间，单位为游戏帧。</summary>
        public const int LaserFadeInDuration = 12;
        /// <summary>激光淡出持续时间，单位为游戏帧。</summary>
        public const int LaserFadeOutDuration = 18;
        /// <summary>雨幕持续时间，单位为游戏帧。</summary>
        public const int RainDuration = 80;
        /// <summary>回归动作相对机制二起点的帧数。</summary>
        public const int ReturnStartAge = LastStrikeAge + BodyFollowThroughDuration;
        /// <summary>回归持续时间，单位为游戏帧。</summary>
        public const int ReturnDuration = Mechanic2Timeline.ReturnMusicTime - Mechanic2Timeline.StartMusicTime - ReturnStartAge;
        /// <summary>机制二总持续帧数。</summary>
        public const int Duration = Mechanic2Timeline.EndMusicTime - Mechanic2Timeline.StartMusicTime;
        /// <summary>扇形星体数量。</summary>
        public const int FanStarCount = 15;
        /// <summary>扇形星体展开角。</summary>
        public const float FanStarSpread = TOMathUtils.PiOver3;
        /// <summary>扇形星体快速持续时间，单位为游戏帧。</summary>
        public const int FanStarFastDuration = 20;
        /// <summary>扇形星体减速持续时间，单位为游戏帧。</summary>
        public const int FanStarDecelerationDuration = 12;
        /// <summary>扇形星体快速半径比例。</summary>
        public const float FanStarFastRadiusRatio = 0.35f;

        /// <summary>星体数量。</summary>
        public const int StarCount = 300;
        /// <summary>星体拖尾长度。</summary>
        public const int StarTrailLength = 12;
        /// <summary>星体速度，以世界像素和游戏帧计量。</summary>
        public const float StarSpeed = 32f;
        /// <summary>背景深度。</summary>
        public const float BackgroundZ = 1800f;
        /// <summary>前景深度。</summary>
        public const float ForegroundZ = -350f;
        /// <summary>玩家上方高度。</summary>
        public const float HeightAbovePlayer = 650f;
        #endregion

        #region 时间窗口、轨迹与表现查询

        /// <summary>计算手部入场进度。</summary>
        public static float HandEntryProgress(int age) => MathHelper.SmoothStep(0f, 1f,
            MathHelper.Clamp(age / (float)HandEntryDuration, 0f, 1f));
        /// <summary>从入场实际偏移和初速度平滑停靠到待机点，保留两端位置和速度连续性。</summary>
        public static Vector2 GetHandEntryOffset(Vector2 origin, Vector2 initialVelocity, Vector2 destination, int age) =>
            Vector2.Hermite(origin, initialVelocity * HandEntryDuration, destination, Vector2.Zero,
                MathHelper.Clamp(age / (float)HandEntryDuration, 0f, 1f));

        /// <summary>非拍击手保持下垂并随预警移至本体侧下方，为骨节保留展开空间，恢复后回到待机位置。</summary>
        public static Vector2 GetPassingHandOffset(int side, Vector2 idleOffset, int age) =>
            Vector2.Lerp(idleOffset, new Vector2(side * PassingHandHorizontalOffset, PassingHandVerticalOffset), HandAttackProgress(age));

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

        /// <summary>计算指定拍击相对机制二起点的帧数；strike 为从零开始的拍击编号。</summary>
        public static int GetStrikeTime(int index) => FirstStrikeAge + index * StrikeInterval;

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

        /// <summary>以等面积半径和低差异角度铺满半圆，留下24像素边缘余量，避免跨越安全半场。</summary>
        public static Vector2 GetStarSpawnOffset(int index, float radius, int side)
        {
            const float Inset = 24f;
            const float GoldenRatioFraction = 0.61803398875f;
            float distance = Math.Max(0f, radius - Inset) * MathF.Sqrt((index + 0.5f) / StarCount);
            float angle = MathHelper.Pi * (index * GoldenRatioFraction % 1f) - MathHelper.PiOver2;
            return new Vector2(side * (Inset + Math.Max(0f, MathF.Cos(angle)) * distance), MathF.Sin(angle) * distance);
        }
        /// <summary>计算拍击索引。</summary>
        public static int GetStrikeIndex(int age)
        {
            for (int i = 0; i < StrikeCount - 1; i++)
                if (age < GetStrikeTime(i) + LaserDuration + HandRecoverDuration)
                    return i;
            return StrikeCount - 1;
        }

        /// <summary>查询主体后撤、冲刺或回收窗口内的拍击编号；窗口外返回 -1。主体回收比手部回收更长。</summary>
        public static int GetBodyStrikeIndex(int age)
        {
            for (int strike = 0; strike < StrikeCount; strike++)
            {
                int relativeAge = age - GetStrikeTime(strike);
                if (relativeAge is >= -BodyRetreatLeadTime and <= (BodyFollowThroughDuration + BodyRecoverDuration))
                    return strike;
            }
            return -1;
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

        /// <summary>计算手部攻击进度。</summary>
        public static float HandAttackProgress(int age)
        {
            int relativeAge = age - GetStrikeTime(GetStrikeIndex(age));
            float raise = HandRaiseProgress(relativeAge);
            float recover = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((relativeAge - LaserDuration) / (float)HandRecoverDuration, 0f, 1f));
            return raise * (1f - recover);
        }

        /// <summary>计算手部拍下进度。</summary>
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

        /// <summary>判断当前帧是否处于激光伤害窗口。</summary>
        public static bool LasersActive(int age)
        {
            for (int i = 0; i < StrikeCount; i++)
                if (age >= GetStrikeTime(i) && age < GetStrikeTime(i) + LaserDuration)
                    return true;
            return false;
        }

        /// <summary>计算激光预警。</summary>
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

        #endregion
    }

    // 顺序：服务器随机侧位图 → 本机单次伤害 → 回程/等待场地 → 主体轨迹 → 拍击攻击。
    private void Mechanic2AI()
    {
        int age = Mechanic2Age;
        LegendaryNightPhase2Arena arena = Phase2Arena;
        EnsureStrikePattern();
        if (Mechanic2HandPattern == 0)
        {
            // 阶段预测可能早于服务器的随机位图；未收到时既不猜测拍击侧，也不结算半场伤害。
            NPC.velocity = Vector2.Zero;
            return;
        }
        if (arena is not null)
            ApplyHalfFieldStrike(arena);
        if (age >= Mechanic2Parameters.ReturnStartAge && arena is not null)
        {
            // 回程也依赖最后一次服务器锁定，不能把旧拍击或已清空的向量解释为回程起点。
            if (EnsureStrikeLock(Mechanic2Parameters.StrikeCount - 1))
                ReturnToArenaCenter();
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
        Vector2 destination = new(center.X, Target.Center.Y - Mechanic2Parameters.HeightAbovePlayer);
        float departure = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(
            (age - LegendaryNightPhase2Arena.AppearanceDuration) / (float)Mechanic2Parameters.ArenaDepartureDuration, 0f, 1f));
        NPC.velocity = (destination - NPC.Center) * (0.12f * departure);
        UpdateMechanic2BodyStrike(age);
        NPC.rotation *= 0.85f;
        NPC.Face(Target);

        int impactStrike = Mechanic2Parameters.GetStrikeIndex(age);
        if (age == Mechanic2Parameters.GetStrikeTime(impactStrike))
        {
            if (!Main.dedServ)
                SoundEngine.PlaySound(SoundID.Item14);
            if (TONetworkEnvironment.NotClient)
            {
                SpawnLegendSlapStars(arena, Mechanic2StrikeSide(impactStrike), impactStrike);
                SpawnStrikeFanStars();
                NPC.netUpdate = true;
            }
        }

        void ApplyHalfFieldStrike(LegendaryNightPhase2Arena arena)
        {
            if (Main.dedServ || !Mechanic2Active || Mechanic2HandPattern == 0)
                return;

            int age = Mechanic2Age;
            int strike = Mechanic2Parameters.GetStrikeIndex(age);
            if (age != Mechanic2Parameters.GetStrikeTime(strike))
                return;

            int hitMask = 1 << strike;
            if ((AnomalyNPC.Skeletron_LegendaryNightHalfFieldHits & hitMask) != 0)
                return;
            AnomalyNPC.Skeletron_LegendaryNightHalfFieldHits |= hitMask;

            Player player = Main.LocalPlayer;
            if (new Circle(arena.Projectile.Center, arena.Radius).Collides(player.Hitbox)
                && (Mechanic2StrikeSide(strike) < 0 ? player.Hitbox.Left < arena.Projectile.Center.X : player.Hitbox.Right > arena.Projectile.Center.X))
                LegendaryNightDamage.HurtLocalPlayer(arena.Projectile, SlapDamage, true);
        }

        bool EnsureStrikeLock(int strike)
        {
            if (Mechanic2LockedStrike == strike + 1)
                return true;
            if (!TONetworkEnvironment.NotClient)
            {
                NPC.velocity = Vector2.Zero;
                AnomalyNPC.Depth_DrawAnchor = null;
                return false;
            }

            // AI64[2/3]：起点/锁定目标；AI32[8]：拍击编号 + 1。先写向量，再发布锁定编号。
            // 只有服务器锁定；客户端收到完整快照后按音乐年龄恢复，不使用本机玩家位置补写权威数据。
            Mechanic2StrikeOrigin = NPC.Center;
            Mechanic2StrikeTarget = Target.Center - Vector2.UnitY * Mechanic2Parameters.BodyImpactHeight;
            Mechanic2LockedStrike = strike + 1;
            NPC.netUpdate = true;
            return true;
        }

        void UpdateMechanic2BodyStrike(int age)
        {
            int strike = Mechanic2Parameters.GetBodyStrikeIndex(age);
            if (strike < 0 || !EnsureStrikeLock(strike))
                return;

            int relativeAge = age - Mechanic2Parameters.GetStrikeTime(strike);
            Vector2 origin = Mechanic2StrikeOrigin;
            Vector2 target = Mechanic2StrikeTarget;
            Vector2 retreatEnd = target - Vector2.UnitY * (Mechanic2Parameters.HeightAbovePlayer - Mechanic2Parameters.BodyImpactHeight);
            (float advance, _) = Mechanic2Parameters.BodyStrikeMotion(relativeAge);
            Vector2 position = relativeAge < -Mechanic2Parameters.BodyRushDuration
                ? Vector2.Lerp(origin, retreatEnd, Mechanic2Parameters.BodyRetreatProgress(relativeAge))
                : Vector2.Lerp(retreatEnd, target, advance);
            // 冲出观察者平面后，从上方绕回后景；前冲始终掠过锁定目标的头顶。
            if (relativeAge >= -Mechanic2Parameters.BodyRushDuration)
                position.Y = Math.Min(position.Y, target.Y);
            float recovery = MathHelper.Clamp((relativeAge - Mechanic2Parameters.BodyFollowThroughDuration)
                / (float)Mechanic2Parameters.BodyRecoverDuration, 0f, 1f);
            position.Y -= MathF.Sin(MathHelper.Pi * MathHelper.SmoothStep(0f, 1f, recovery)) * Mechanic2Parameters.BodyReturnArcHeight;
            // 按音乐时钟计算位置，以本帧位移作为速度，拍击帧准确抵达前景而不受追踪插值滞后影响。
            NPC.velocity = position - NPC.Center;
            AnomalyNPC.Z = Mechanic2Parameters.BodyStrikeDepth(relativeAge);
            // 以绘制底边为锚点反算投影偏移，避免负 Z 放大时底边离玩家越来越远。
            // 回程的前六帧仍在观察者之外，此时解除锚定，再从上方弧线重新入画。
            UpdateStrikeDrawAnchor(relativeAge, recovery);
        }

        void SpawnLegendSlapStars(LegendaryNightPhase2Arena arena, int side, int strike)
        {
            const int Count = Mechanic2Parameters.StarCount;
            float radius = arena.Radius;
            // 生成时立即铺满整个受击半圆，而不是从半圆上沿之外开始下落。
            for (int i = 0; i < Count; i++)
            {
                Vector2 position = arena.Projectile.Center + Mechanic2Parameters.GetStarSpawnOffset(i, radius, side);
                Projectile.NewProjectileAction<LegendSlapStar>(SourceAI, position, Vector2.UnitY * Mechanic2Parameters.StarSpeed, FallingStarDamage, 0f, action: p =>
                {
                    p.ai[0] = strike;
                    p.ai[1] = side;
                    p.ai[2] = NPC.whoAmI + 1;
                    p.netUpdate = true;
                });
            }
        }

        void SpawnStrikeFanStars()
        {
            Vector2 origin = arena.Projectile.Center;
            float targetAngle = (Target.Center - origin).SafeNormalize(Vector2.UnitY).ToRotation();
            for (int i = 0; i < Mechanic2Parameters.FanStarCount; i++)
            {
                float angle = targetAngle + Mechanic2Parameters.FanStarSpread
                    * (i / (float)(Mechanic2Parameters.FanStarCount - 1) - 0.5f);
                Vector2 velocity = angle.ToRotationVector2() * Mechanic2Parameters.GetFanStarSpeed(arena.Radius, 0);
                Projectile.NewProjectileAction<FallingStar>(SourceAI, origin, velocity, FallingStarDamage, 0f, action: p =>
                {
                    p.ai[1] = arena.Radius;
                    p.ai[2] = NPC.whoAmI + 1;
                    p.scale = 0.2f;
                    p.Resize(16, 16);
                    p.timeLeft = Mechanic2Parameters.StrikeInterval;
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
                SoundEngine.PlaySound(SoundID.Item9);
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

        void EnsureStrikePattern()
        {
            if (Mechanic2HandPattern == 0 && TONetworkEnvironment.NotClient)
            {
                int hand = Main.rand.Next(2);
                int pattern = hand | (1 << Mechanic2Parameters.StrikeCount);
                for (int strike = 1; strike < Mechanic2Parameters.StrikeCount; strike++)
                {
                    // 前两次同向时，第三次必定换向。
                    bool forceSwitch = strike == 2 && (pattern & 0b11) is 0 or 0b11;
                    if (forceSwitch || Main.rand.NextProbability(0.7f))
                        hand ^= 1;
                    pattern |= hand << strike;
                }
                Mechanic2HandPattern = pattern;
                Mechanic2LockedStrike = 0;
                NPC.netUpdate = true;
            }
        }

        void UpdateStrikeDrawAnchor(int relativeAge, float recovery)
        {
            float anchorWeight = MathHelper.SmoothStep(0f, 1f,
                MathHelper.Clamp((relativeAge + Mechanic2Parameters.BodyRushDuration)
                    / (float)Mechanic2Parameters.BodyRushDuration, 0f, 1f))
                * (1f - MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(recovery * 10f, 0f, 1f)));
            float halfWidth = NPC.frame.Width * NPC.scale * 0.5f;
            float halfHeight = NPC.frame.Height * NPC.scale * 0.5f;
            float drawRotation = NPC.rotation * 0.85f;
            float bottomExtent = Math.Abs(MathF.Sin(drawRotation)) * halfWidth
                + Math.Abs(MathF.Cos(drawRotation)) * halfHeight;
            float drawCenterOffset = NPC.height * 0.5f - halfHeight + 4f + NPC.gfxOffY;
            AnomalyNPC.Depth_DrawAnchor = (new Vector2(0f, drawCenterOffset + bottomExtent),
                Target.Top - Vector2.UnitY * Mechanic2Parameters.BodyHeadClearance, anchorWeight);
        }

        void ReturnToArenaCenter()
        {
            HideHands();
            // 第三次拍击穿过观察者后才隐藏手部；从屏外沿上方弧线回中。
            // 起点由已同步的拍击目标推导，中途加入也能恢复相同轨迹。
            Vector2 impact = Mechanic2StrikeTarget;
            Vector2 retreatEnd = impact - Vector2.UnitY * (Mechanic2Parameters.HeightAbovePlayer - Mechanic2Parameters.BodyImpactHeight);
            (float advance, _) = Mechanic2Parameters.BodyStrikeMotion(Mechanic2Parameters.BodyFollowThroughDuration);
            Vector2 origin = Vector2.Lerp(retreatEnd, impact, advance);
            origin.Y = Math.Min(origin.Y, impact.Y);
            float progress = MathHelper.Clamp((age - Mechanic2Parameters.ReturnStartAge)
                / (float)Mechanic2Parameters.ReturnDuration, 0f, 1f);
            float eased = MathHelper.SmoothStep(0f, 1f, progress);
            Vector2 position = Vector2.Lerp(origin, arena.Projectile.Center, eased);
            position.Y -= MathF.Sin(MathHelper.Pi * eased) * Mechanic2Parameters.BodyReturnArcHeight;
            NPC.velocity = position - NPC.Center;
            AnomalyNPC.Z = MathHelper.Lerp(Mechanic2Parameters.BodyExitZ, 0f, eased);
            NPC.rotation *= 0.85f;
            int remaining = Mechanic2Timeline.ReturnMusicTime - Phase2MusicElapsedTime;
            if (remaining == 0)
            {
                NPC.Center = arena.Projectile.Center;
                NPC.velocity = Vector2.Zero;
                SoundEngine.PlaySound(SoundID.ForceRoar);
                ReleaseSpellShockwave(Spell.None);
                // 外圈使用 CalRD BrimstoneGigaBlast 的算法 B；内圈使用 BrimstoneFireblast 的算法 A。
                SpawnRadialStars(8f, 8, 45f * MathHelper.PiOver2 * 0.01f);
                SpawnRadialStars(6f, 30, 12f * MathHelper.PiOver2 * 0.01f);
            }
        }
    }
}