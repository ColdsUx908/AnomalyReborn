using Anomalies.Bosses.Skeletron.LegendaryNight;

namespace Anomalies.Common;

public sealed partial class AnomalyGlobalNPC
{
    #region 独立实例数据

    // 仅客户端绘制使用，不同步，也不在单例 Behavior 上保存实体状态。
    internal SkeletronHandBloodDust Skeletron_HandBloodDust;
    // 本机已消费的三次半场拍击位图；音乐时钟回拨不能让同一次拍击再次结算，不同步。
    internal int Skeletron_LegendaryNightHalfFieldHits;

    #endregion

    partial void CloneSkeletronData(AnomalyGlobalNPC clone)
    {
        clone.Skeletron_HandBloodDust = null;
        clone.Skeletron_LegendaryNightHalfFieldHits = 0;
    }
}