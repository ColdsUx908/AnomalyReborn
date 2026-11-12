namespace Anomalies.Bosses;

public static class AnomalyBossChangeToggler
{
    /// <summary>
    /// 是否启用未完成的Boss的修改。
    /// <br/>在Release模式中，玩家只能体验到已经完成的Boss。
    /// </summary>
    public const bool EnableUnfinishedBossChange =
#if DEBUG
        true;
#else
        false;
#endif

    //按首字母排序

    public static bool BrainofCthulhu { get; private set; } = EnableUnfinishedBossChange;

    public static bool DukeFishron { get; private set; } = EnableUnfinishedBossChange;

    public static bool EaterofWorlds { get; private set; } = EnableUnfinishedBossChange;

    public static bool EmpressofLight { get; private set; } = EnableUnfinishedBossChange;

    public static bool EyeofCthulhu { get; private set; } = true;

    public static bool KingSlime { get; private set; } = true;

    public static bool LegendaryNight { get; private set; } = true;

    public static bool QueenBee { get; private set; } = true;

    public static bool QueenSlime { get; private set; } = EnableUnfinishedBossChange;

    public static bool Skeletron { get; private set; } = true;
}
