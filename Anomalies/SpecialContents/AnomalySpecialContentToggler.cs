namespace Anomalies.SpecialContents;

/// <summary>
/// 特殊内容的常量开关；修改后需要重新构建并打包模组。
/// 关闭时不加载对应物品及其专属弹幕、Buff。测试物品仍仅由 DEBUG 控制。
/// </summary>
public static class AnomalySpecialContentToggler
{
    // 按首字母排序。
    public const bool EventideReunion = true;

    public const bool ImmaculateWhite = true;

    public const bool VacuousBlack = true;
}
