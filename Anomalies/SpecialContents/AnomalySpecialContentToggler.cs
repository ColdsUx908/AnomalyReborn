namespace Anomalies.SpecialContents;

public static class AnomalySpecialContentToggler
{
    /// <summary>
    /// 是否启用未完成的特殊内容。
    /// <br/>在Release模式中，玩家只能体验到已经完成的特殊内容。
    /// </summary>
    public const bool EnableUnfinishedContents =
#if DEBUG
        true;
#else
        false;
#endif

    //按首字母排序

    public static bool EventideReunion { get; private set; } = EnableUnfinishedContents;

    public static bool ImmaculateWhite { get; private set; } = true;

    public static bool VacuousBlack { get; private set; } = true;
}
