using static Anomalies.Bosses.Skeletron.LegendaryNight.LegendaryNightStarburstTimeline;
using static Anomalies.Bosses.Skeletron.LegendaryNight.LegendaryNightStarburstParameters;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class LegendaryNightPhase2Arena
{
    // 接口以每帧弧度定义；速度恒定的区段可直接预测交汇位置。
    // 慢法阵从首次拍击到首次星爆恰好公转一整圈，拍击时的位置就是首次爆点。
    // 首次交汇后按音乐节点分段积分，预测与实际公转共用计算。
    /// <summary>慢速公转周期，单位为游戏帧。</summary>
    public const int SlowOrbitPeriod = SkeletronLegendHead.Mechanic3Timeline.StartMusicTime - SkeletronLegendHead.Mechanic2Timeline.FirstStrikeMusicTime;
    /// <summary>慢速公转速度，以世界像素和游戏帧计量。</summary>
    public static float SlowOrbitSpeed => MathHelper.TwoPi / SlowOrbitPeriod;
    public float FirstStarburstAngle => Projectile.ai[2] - 1f;
    public Vector2 FirstStarburstPosition => Projectile.Center
        + FirstStarburstAngle.ToRotationVector2() * (MainCircleRadius + SmallCircleRadius);
    // 每段均从上一个星爆的累计角度积分，调速不会重置位置。
    // 两圈的相对角速度由相邻交汇时间约束；可独立调整各段慢圈速度。
    public static float GetSlowOrbitSpeed(int interval) => interval switch
    {
        0 => SlowOrbitSpeed,
        1 => SlowOrbitSpeed,
        _ => SlowOrbitSpeed
    };

    // 第四至第五次间先保持旧速，再以SmoothStep加速；积分恰好相差一圈。
    // 加速结束后两圈分别每95帧顺时针转150°、510°，角度与角速度均连续。
    /// <summary>快速慢速公转速度，以世界像素和游戏帧计量。</summary>
    public static float RapidSlowOrbitSpeed => MathHelper.ToRadians(150f) / RapidStarburstInterval;
    /// <summary>过渡持续时间。</summary>
    private static float TransitionDuration => SkeletronLegendHead.Mechanic5Timeline.FifthStarburstMusicTime - SkeletronLegendHead.Mechanic5Timeline.FourthStarburstMusicTime;
    /// <summary>初始相对速度，以世界像素和游戏帧计量。</summary>
    private static float InitialRelativeSpeed => MathHelper.TwoPi / (SkeletronLegendHead.Mechanic5Timeline.FourthStarburstMusicTime - SkeletronLegendHead.Mechanic5Timeline.StartMusicTime);
    /// <summary>快速段相对速度，以世界像素和游戏帧计量。</summary>
    private static float RapidRelativeSpeed => MathHelper.TwoPi / RapidStarburstInterval;
    /// <summary>加速延迟。</summary>
    private static float AccelerationDelay =>
        ((InitialRelativeSpeed + RapidRelativeSpeed) * 0.5f * TransitionDuration - MathHelper.TwoPi)
        / ((RapidRelativeSpeed - InitialRelativeSpeed) * 0.5f);
    private static float IntegrateTransition(float initialSpeed, float finalSpeed, float age)
    {
        float duration = TransitionDuration - AccelerationDelay;
        float progress = MathHelper.Clamp((age - AccelerationDelay) / duration, 0f, 1f);
        // SmoothStep(u) = 3u² - 2u³，其原函数为u³ - u⁴/2。
        float integral = duration * (progress * progress * progress - 0.5f * MathF.Pow(progress, 4f));
        return initialSpeed * age + (finalSpeed - initialSpeed) * integral;
    }

    // 恢复段对SmoothStep角速度解析积分；结束后接初始匀速，始终保持角度连续。
    private static float IntegrateOrbitRecovery(float initialSpeed, float finalSpeed, int age)
    {
        int elapsed = Math.Clamp(age, 0, OrbitRecoveryDuration);
        float progress = elapsed / (float)OrbitRecoveryDuration;
        float integral = OrbitRecoveryDuration * (progress * progress * progress - 0.5f * MathF.Pow(progress, 4f));
        return initialSpeed * elapsed + (finalSpeed - initialSpeed) * integral
            + Math.Max(0, age - OrbitRecoveryDuration) * finalSpeed;
    }
    public static float GetRelativeOrbitTurns(int musicTime)
    {
        if (musicTime <= SkeletronLegendHead.Mechanic3Timeline.StartMusicTime)
            return (musicTime - SkeletronLegendHead.Mechanic3Timeline.StartMusicTime) / (float)StarburstInterval;
        if (musicTime <= SkeletronLegendHead.Mechanic4Timeline.StartMusicTime)
            return (musicTime - SkeletronLegendHead.Mechanic3Timeline.StartMusicTime) / (float)(SkeletronLegendHead.Mechanic4Timeline.StartMusicTime - SkeletronLegendHead.Mechanic3Timeline.StartMusicTime);
        if (musicTime <= SkeletronLegendHead.Mechanic5Timeline.StartMusicTime)
            return 1f + (musicTime - SkeletronLegendHead.Mechanic4Timeline.StartMusicTime) / (float)(SkeletronLegendHead.Mechanic5Timeline.StartMusicTime - SkeletronLegendHead.Mechanic4Timeline.StartMusicTime);
        if (musicTime <= SkeletronLegendHead.Mechanic5Timeline.FourthStarburstMusicTime)
            return 2f + (musicTime - SkeletronLegendHead.Mechanic5Timeline.StartMusicTime) / (float)(SkeletronLegendHead.Mechanic5Timeline.FourthStarburstMusicTime - SkeletronLegendHead.Mechanic5Timeline.StartMusicTime);
        if (musicTime <= SkeletronLegendHead.Mechanic5Timeline.FifthStarburstMusicTime)
            return 3f + IntegrateTransition(InitialRelativeSpeed, RapidRelativeSpeed,
                musicTime - SkeletronLegendHead.Mechanic5Timeline.FourthStarburstMusicTime) / MathHelper.TwoPi;
        return 4f + (Math.Min(musicTime, SkeletronLegendHead.Mechanic5Timeline.OrbitRecoveryStartMusicTime) - SkeletronLegendHead.Mechanic5Timeline.FifthStarburstMusicTime) / (float)RapidStarburstInterval
            + IntegrateOrbitRecovery(RapidRelativeSpeed, MathHelper.TwoPi / StarburstInterval,
                musicTime - SkeletronLegendHead.Mechanic5Timeline.OrbitRecoveryStartMusicTime) / MathHelper.TwoPi;
    }
    public static float GetOrbitSpeed(int index, int musicTime)
    {
        float slow = musicTime < SkeletronLegendHead.Mechanic4Timeline.StartMusicTime ? GetSlowOrbitSpeed(0)
            : musicTime < SkeletronLegendHead.Mechanic5Timeline.StartMusicTime ? GetSlowOrbitSpeed(1) : GetSlowOrbitSpeed(2);
        float relative = musicTime < SkeletronLegendHead.Mechanic3Timeline.StartMusicTime ? MathHelper.TwoPi / StarburstInterval
            : musicTime < SkeletronLegendHead.Mechanic4Timeline.StartMusicTime ? MathHelper.TwoPi / (SkeletronLegendHead.Mechanic4Timeline.StartMusicTime - SkeletronLegendHead.Mechanic3Timeline.StartMusicTime)
            : musicTime < SkeletronLegendHead.Mechanic5Timeline.StartMusicTime ? MathHelper.TwoPi / (SkeletronLegendHead.Mechanic5Timeline.StartMusicTime - SkeletronLegendHead.Mechanic4Timeline.StartMusicTime)
            : InitialRelativeSpeed;
        if (musicTime >= SkeletronLegendHead.Mechanic5Timeline.OrbitRecoveryStartMusicTime)
        {
            float progress = MathHelper.Clamp((musicTime - SkeletronLegendHead.Mechanic5Timeline.OrbitRecoveryStartMusicTime) / (float)OrbitRecoveryDuration, 0f, 1f);
            slow = MathHelper.SmoothStep(RapidSlowOrbitSpeed, SlowOrbitSpeed, progress);
            relative = MathHelper.SmoothStep(RapidRelativeSpeed, MathHelper.TwoPi / StarburstInterval, progress);
        }
        else if (musicTime >= SkeletronLegendHead.Mechanic5Timeline.FifthStarburstMusicTime)
        {
            slow = RapidSlowOrbitSpeed;
            relative = RapidRelativeSpeed;
        }
        else if (musicTime > SkeletronLegendHead.Mechanic5Timeline.FourthStarburstMusicTime)
        {
            float progress = MathHelper.Clamp((musicTime - SkeletronLegendHead.Mechanic5Timeline.FourthStarburstMusicTime - AccelerationDelay)
                / (TransitionDuration - AccelerationDelay), 0f, 1f);
            slow = MathHelper.SmoothStep(GetSlowOrbitSpeed(2), RapidSlowOrbitSpeed, progress);
            relative = MathHelper.SmoothStep(InitialRelativeSpeed, RapidRelativeSpeed, progress);
        }
        return slow + (index == 2 ? relative : 0f);
    }

    public float GetOrbitAngle(int index, int musicTime)
    {
        int first = SkeletronLegendHead.Mechanic3Timeline.StartMusicTime;
        int second = SkeletronLegendHead.Mechanic4Timeline.StartMusicTime;
        int third = SkeletronLegendHead.Mechanic5Timeline.StartMusicTime;
        int fourth = SkeletronLegendHead.Mechanic5Timeline.FourthStarburstMusicTime;
        int fifth = SkeletronLegendHead.Mechanic5Timeline.FifthStarburstMusicTime;
        float slowAngle = (Math.Min(musicTime, first) - SkeletronLegendHead.Mechanic2Timeline.FirstStrikeMusicTime) * SlowOrbitSpeed
            + Math.Clamp(musicTime - first, 0, second - first) * GetSlowOrbitSpeed(0)
            + Math.Clamp(musicTime - second, 0, third - second) * GetSlowOrbitSpeed(1)
            + Math.Clamp(musicTime - third, 0, fourth - third) * GetSlowOrbitSpeed(2)
            + IntegrateTransition(GetSlowOrbitSpeed(2), RapidSlowOrbitSpeed, Math.Clamp(musicTime - fourth, 0, fifth - fourth))
            + Math.Clamp(musicTime - fifth, 0, SkeletronLegendHead.Mechanic5Timeline.OrbitRecoveryStartMusicTime - fifth) * RapidSlowOrbitSpeed
            + IntegrateOrbitRecovery(RapidSlowOrbitSpeed, SlowOrbitSpeed, musicTime - SkeletronLegendHead.Mechanic5Timeline.OrbitRecoveryStartMusicTime);
        return FirstStarburstAngle + slowAngle
            + (index == 2 ? MathHelper.TwoPi * GetRelativeOrbitTurns(musicTime) : 0f);
    }

    public Vector2 PredictStarburstPosition(int index) => index == 0 ? FirstStarburstPosition
        : Projectile.Center + GetOrbitAngle(1, GetStarburstTime(index)).ToRotationVector2()
            * (MainCircleRadius + SmallCircleRadius);
    public float StarburstGlow => GetGlow(MasterBehavior?.Phase2MusicElapsedTime ?? 0, MasterBehavior is { StarburstActive: true });
}