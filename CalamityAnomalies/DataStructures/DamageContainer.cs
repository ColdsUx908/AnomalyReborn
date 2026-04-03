namespace CalamityAnomalies.DataStructures;

/// <summary>
/// 用于存储弹幕数据的结构，内含自动调整逻辑。
/// </summary>
/// <param name="NormalDamage">普通模式伤害，须为2的倍数。</param>
/// <param name="ExpertDamage">专家模式伤害，须为4的倍数。</param>
/// <param name="MasterDamage">大师模式伤害，须为6的倍数。</param>
/// <param name="LegendaryDamage">传奇模式伤害，须为6的倍数。</param>
/// <param name="AnomalyDamage">异象模式伤害，须为6的倍数。</param>
/// <param name="AnomalyUltramundaneDamage">异象超凡伤害，须为6的倍数。</param>
public readonly record struct ProjectileDamageContainer(
    int NormalDamage, int ExpertDamage, int MasterDamage, int LegendaryDamage,
    int AnomalyDamage, int AnomalyUltramundaneDamage)
{
    public const float ExpertContactVanillaMultiplier = 2f;
    public const float MasterContactVanillaMultiplier = 3f;
    public const float NormalProjectileVanillaMultiplier = 2f;
    public const float ExpertProjectileVanillaMultiplier = 4f;
    public const float MasterProjectileVanillaMultiplier = 6f;

    public int Value
    {
        get
        {
            float damageAdjustment = Main.masterMode ? MasterProjectileVanillaMultiplier : Main.expertMode ? ExpertProjectileVanillaMultiplier : NormalProjectileVanillaMultiplier;
            float expectedDamage =
                CASharedData.AnomalyUltramundane ? AnomalyUltramundaneDamage
                : CASharedData.Anomaly ? AnomalyDamage
                : TOSharedData.LegendaryMode ? LegendaryDamage
                : Main.masterMode ? MasterDamage
                : Main.expertMode ? ExpertDamage
                : NormalDamage;

            return (int)MathF.Round(expectedDamage / damageAdjustment);
        }
    }
}