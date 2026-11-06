using static Anomalies.Bosses.Skeletron.LegendaryNight.LegendaryNightStarburstParameters;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>跨机制星爆的纯时间查询；音乐节点由各 MechanicNTimeline 持有，相对窗口由参数类提供。</summary>
public static class LegendaryNightStarburstTimeline
{
    /// <summary>查询当前预警、伤害或淡出窗口的星爆编号；窗口外返回 -1。</summary>
    public static int GetStarburstIndex(int musicTime)
    {
        // 最后一次允许完整结算伤害及淡出，不在1:52截断。
        if (musicTime < SkeletronLegendHead.Mechanic3Timeline.StartMusicTime - StarburstWarningDuration
            || musicTime >= GetStarburstTime(StarburstCount - 1) + StarburstDamageDuration + StarburstFadeDuration)
            return -1;
        for (int i = StarburstCount - 1; i >= 0; i--)
            if (musicTime >= GetStarburstTime(i) - StarburstWarningDuration)
                return i;
        return -1;
    }
    /// <summary>返回指定零基星爆编号的音乐绝对帧；前四次采用独立节点，之后按快速间隔排列。</summary>
    public static int GetStarburstTime(int index) => index switch
    {
        0 => SkeletronLegendHead.Mechanic3Timeline.StartMusicTime,
        1 => SkeletronLegendHead.Mechanic4Timeline.StartMusicTime,
        2 => SkeletronLegendHead.Mechanic5Timeline.StartMusicTime,
        3 => SkeletronLegendHead.Mechanic5Timeline.FourthStarburstMusicTime,
        _ => SkeletronLegendHead.Mechanic5Timeline.FifthStarburstMusicTime + (index - 4) * RapidStarburstInterval
    };
    /// <summary>按下一次星爆蓄光和上一次星爆余辉计算场地亮度，非激活状态返回零。</summary>
    public static float GetGlow(int time, bool active)
    {
        if (!active
            || time >= GetStarburstTime(StarburstCount - 1) + StarburstColorRecoveryDuration)
            return 0f;
        int next = 0;
        while (next < StarburstCount - 1 && time > GetStarburstTime(next))
            next++;
        int nextTime = GetStarburstTime(next);
        // 快速连续星爆缩短蓄光，留出恢复原色的间隔。
        int chargeDuration = next >= 4 ? Math.Min(StarburstChargeDuration, RapidStarburstInterval / 3)
            : StarburstChargeDuration;
        float charge = nextTime >= time
            ? MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(1f - (nextTime - time) / (float)chargeDuration, 0f, 1f))
            : 0f;
        int previous = time > nextTime ? next : next - 1;
        float recovery = previous >= 0
            ? 1f - MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(
                (time - GetStarburstTime(previous)) / (float)StarburstColorRecoveryDuration, 0f, 1f))
            : 0f;
        return Math.Max(charge, recovery);
    }
}