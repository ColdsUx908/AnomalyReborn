namespace Anomalies.Common;

public static partial class AnomalyUtils
{
    /// <summary>撤销单人专家、大师与 bossAdjustment 倍率，保留异象 Boss 的多人 balance。</summary>
    public static void ApplyAnomalyBossDifficultyAndPlayerScaling(NPC npc, float bossAdjustment)
    {
        // 升级 tML 时须复核原版难度缩放时机与倍率。
        float vanillaExpertLifeMultiplier = TONPCUtils.GetVanillaExpertLifeMultiplier(npc.type);
        npc.lifeMax = (int)MathF.Round(npc.lifeMax / (vanillaExpertLifeMultiplier * 1.5f) / bossAdjustment / 10f) * 10;
    }
}
