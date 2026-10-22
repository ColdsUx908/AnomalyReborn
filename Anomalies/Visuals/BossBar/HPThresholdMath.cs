namespace Anomalies.Visuals.BossBar;

/// <summary>常规阈值所依据的血量来源。</summary>
public enum HPThresholdLifeSource
{
    NPC,
    Extra
}

/// <summary>将分段血量阈值映射到合并血条的位置。</summary>
public static class HPThresholdMath
{
    public static float GetPosition(HPThresholdLifeSource source, float ratio, long npcMaxLife, long totalMaxLife)
    {
        if (totalMaxLife <= 0 || npcMaxLife <= 0)
            return 0f;
        long bodyMax = System.Math.Min(npcMaxLife, totalMaxLife);
        long extraMax = totalMaxLife - bodyMax;
        ratio = System.Math.Clamp(ratio, 0f, 1f);
        if (source == HPThresholdLifeSource.Extra)
            return extraMax > 0 ? (bodyMax + extraMax * ratio) / totalMaxLife : 0f;
        return bodyMax * ratio / totalMaxLife;
    }

    public static float GetProgress(HPThresholdLifeSource source, long npcLife, long npcMaxLife, long extraLife, long totalMaxLife)
    {
        if (totalMaxLife <= 0)
            return 0f;
        return source == HPThresholdLifeSource.Extra
            ? (System.Math.Min(npcMaxLife, totalMaxLife) + (float)System.Math.Max(extraLife, 0)) / totalMaxLife
            : (float)System.Math.Max(npcLife, 0) / totalMaxLife;
    }
}
