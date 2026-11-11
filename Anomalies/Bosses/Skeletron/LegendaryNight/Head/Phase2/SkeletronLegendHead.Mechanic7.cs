namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class SkeletronLegendHead
{
    /// <summary>机制 7 的音乐绝对帧节点；从转场音乐开始累计，单位为游戏帧。</summary>
    public static class Mechanic7Timeline
    {
        /// <summary>本机制开始的音乐绝对帧（包含此帧）。</summary>
        public const int StartMusicTime = Mechanic6Timeline.CompanionImpactMusicTime;
        /// <summary>终幕结算的音乐绝对帧；到达后沿用终幕冲击波与死亡处理，不切换新机制。</summary>
        public const int EndMusicTime = FinalMusicTime;
        /// <summary>变形开始的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int TransformationStartMusicTime = 153 * 60 + 47;
        /// <summary>元素圆开始的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int CircleStartMusicTime = 156 * 60 + 10;
        /// <summary>元素开始的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int ElementStartMusicTime = CircleStartMusicTime + 95;
        /// <summary>攻击开始的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int AttackStartMusicTime = ElementStartMusicTime + 95;
        /// <summary>终幕陨星开始的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int FinalMeteorStartMusicTime = 181 * 60 + 30;
        /// <summary>星体消散开始的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int StarDissipationStartMusicTime = 181 * 60 + 30;
        /// <summary>元素淡出开始的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int ElementFadeOutStartMusicTime = 184 * 60 + 40;
        /// <summary>解除死亡锁定的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int DeathUnlockMusicTime = 184 * 60 + 40;
        /// <summary>终幕的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int FinalMusicTime = 187 * 60 + 50; // 11270，3:07:50
        /// <summary>首次激光预警的音乐绝对帧，从转场音乐开始累计。</summary>
        public const int FirstLaserWarningMusicTime = AttackStartMusicTime - Mechanic6Parameters.LaserWarningDuration;
    }

    /// <summary>机制 7 的攻击、运动和表现参数及纯计算；时长单位为游戏帧。</summary>
    public static class Mechanic7Parameters
    {
        /// <summary>变形持续时间，单位为游戏帧。</summary>
        public const int TransformationDuration = 143;
        /// <summary>终幕缩放。</summary>
        public const float FinalScale = 2f;
        /// <summary>渐隐持续时间，单位为游戏帧。</summary>
        public const int FadeDuration = 60;
        /// <summary>加速持续时间，单位为游戏帧。</summary>
        public const int AccelerationDuration = 120;
        /// <summary>最大速度倍率。</summary>
        public const float MaximumSpeedMultiplier = 7.5f;
        /// <summary>元素弹幕速度，以世界像素和游戏帧计量。</summary>
        public const float ElementProjectileSpeed = 3.5f;
        /// <summary>激光数量。</summary>
        public const int LaserCount = 8;
        /// <summary>激光波次数量。</summary>
        public const int LaserWaveCount = 8;
        /// <summary>元素波次数量。</summary>
        public const int ElementWaveCount = 43;
        // 各调用点独立定义，数值相同也不互相引用，便于单独调整时间轴。
        /// <summary>星体消散持续时间，单位为游戏帧。</summary>
        public const int StarDissipationDuration = 285;
        /// <summary>陨星下落持续时间，单位为游戏帧。</summary>
        public const int MeteorFallDuration = Mechanic7Timeline.FinalMusicTime - Mechanic7Timeline.FinalMeteorStartMusicTime;
        /// <summary>终幕冲击波半径比例。</summary>
        public const float FinalShockwaveRadiusRatio = 2f;
        /// <summary>激光间隔，单位为游戏帧。</summary>
        public const int LaserInterval = 190;
        // 延续橙星外层的暖橙色，保留光束现有的白热核心。
        /// <summary>激光颜色。</summary>
        public static readonly Color LaserColor = new(255, 132, 42);
        /// <summary>判断当前音乐帧是否生成激光。</summary>
        public static bool ShouldSpawnLaser(int musicTime) => musicTime >= Mechanic7Timeline.FirstLaserWarningMusicTime
            && musicTime < Mechanic7Timeline.FirstLaserWarningMusicTime + LaserWaveCount * LaserInterval
            && (musicTime - Mechanic7Timeline.FirstLaserWarningMusicTime) % LaserInterval == 0;

        // 对线性速度乘数积分，避免加速起点或终点出现角度跳变。
        /// <summary>计算公转累计时间。</summary>
        public static float GetIntegratedOrbitTime(int musicTime)
        {
            float age = Math.Max(0, musicTime - Mechanic7Timeline.CircleStartMusicTime);
            float acceleratedAge = Math.Max(0, musicTime - Mechanic7Timeline.AttackStartMusicTime);
            float rampAge = Math.Min(acceleratedAge, AccelerationDuration);
            return age + (MaximumSpeedMultiplier - 1f) * rampAge * rampAge / (2f * AccelerationDuration)
                + (MaximumSpeedMultiplier - 1f) * Math.Max(0, acceleratedAge - AccelerationDuration);
        }

        /// <summary>判断发射。</summary>
        public static bool ShouldFireElementWave(int musicTime)
        {
            int age = musicTime - Mechanic7Timeline.AttackStartMusicTime;
            // 每95帧均分3轮，整数帧偏移0、31、63；第43轮（最后一轮）落在攻击开始后的第1330帧。
            return age >= 0 && age <= (ElementWaveCount - 1) * 95 / 3
                && age % 95 is 0 or 31 or 63;
        }
        /// <summary>元素圆半径，单位为世界像素。</summary>
        public const float CircleRadius = 100f;
        // 独立公转角速度；编号奇数顺时针，偶数逆时针。
        /// <summary>公转角速度。</summary>
        public static ReadOnlySpan<float> RollingSpeeds => [0.002f, 0.003f, 0.0033f, 0.0025f, 0.0023f, 0.0037f, 0.0027f, 0.0035f];
        /// <summary>计算元素圆透明度。</summary>
        public static float CircleOpacity(int musicTime) => MathHelper.Clamp((musicTime - Mechanic7Timeline.CircleStartMusicTime) / (float)FadeDuration, 0f, 1f)
            * (1f - MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((musicTime - Mechanic7Timeline.ElementFadeOutStartMusicTime) / (float)FadeDuration, 0f, 1f)));
        /// <summary>计算指定元素开始显现的音乐绝对帧；index 为从零开始的元素编号。</summary>
        public static int GetElementStartMusicTime(int index) => Mechanic7Timeline.ElementStartMusicTime + index * 6;

        // 按编号错开起显，所有元素在60帧淡入窗口末端完成淡入。
        /// <summary>计算元素透明度。</summary>
        public static float ElementOpacity(int musicTime, int index) => MathHelper.Clamp(
            (musicTime - GetElementStartMusicTime(index)) / (FadeDuration - index * 6f), 0f, 1f) * (musicTime < Mechanic7Timeline.ElementFadeOutStartMusicTime ? 1f : 0f);
        /// <summary>计算元素颜色。</summary>
        public static Color ElementColor(int index) => (index / 2) switch
        {
            0 => LegendaryNightHandler.BoneColor,
            1 => LegendaryNightHandler.SlimeColor,
            2 => LegendaryNightHandler.StarColor,
            _ => LegendaryNightHandler.PlatinumColor,
        };

        /// <summary>计算星体消散。</summary>
        public static float StarDissipation(int musicTime) => MathHelper.Clamp(
            (musicTime - Mechanic7Timeline.StarDissipationStartMusicTime) / (float)StarDissipationDuration, 0f, 1f);
        /// <summary>计算变形进度。</summary>
        public static float TransformationProgress(int musicTime) => MathHelper.SmoothStep(0f, 1f,
            MathHelper.Clamp((musicTime - Mechanic7Timeline.TransformationStartMusicTime) / (float)TransformationDuration, 0f, 1f));
        /// <summary>计算星体缩放。</summary>
        public static float StarScale(int musicTime) => MathHelper.Lerp(1f, FinalScale, TransformationProgress(musicTime));
    }

    private void Mechanic7AI()
    {
        #region 主体回中与终幕事件

        AnomalyNPC.Z = 0f;
        NPC.rotation = 0f;
        NPC.velocity = Phase2Arena is LegendaryNightPhase2Arena arena ? arena.Projectile.Center - NPC.Center : Vector2.Zero;
        int time = Phase2MusicElapsedTime;
        if (time == Mechanic7Timeline.FinalMeteorStartMusicTime && TONetworkEnvironment.NotClient)
            Projectile.NewProjectileAction<LegendFinalMeteor>(SourceAI, NPC.Top + new Vector2(0f, 8f), Vector2.Zero,
                0, 0f, action: p =>
                {
                    p.ai[0] = NPC.whoAmI + 1;
                    p.ai[1] = (Phase2Arena?.Radius ?? LegendaryNightPhase2Arena.TargetingRadius) * Mechanic7Parameters.FinalShockwaveRadiusRatio;
                    p.scale = NPC.scale;
                    p.netUpdate = true;
                });
        if (time == Mechanic7Timeline.DeathUnlockMusicTime && TONetworkEnvironment.NotClient)
            NPC.netUpdate = true;

        #endregion

        #region 恒星转变步骤同步

        // LocalPhase：0等待恒星转变，1变色膨胀，2保持橙星。
        int localPhase = time switch
        {
            < Mechanic7Timeline.TransformationStartMusicTime => 0,
            < Mechanic7Timeline.TransformationStartMusicTime + Mechanic7Parameters.TransformationDuration => 1,
            _ => 2
        };
        if (TONetworkEnvironment.NotClient && CurrentLocalPhase != localPhase)
            ChangePhase2State(Phase.Phase2_7, localPhase);

        #endregion
    }
}