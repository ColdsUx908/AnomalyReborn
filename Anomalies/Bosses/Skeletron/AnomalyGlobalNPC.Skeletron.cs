using Anomalies.Bosses.Skeletron.LegendaryNight;

namespace Anomalies.Common;

public sealed partial class AnomalyGlobalNPC
{
    // 仅客户端绘制使用，不同步，也不在单例 Behavior 上保存实体状态。
    internal SkeletronHandBloodDust Skeletron_HandBloodDust;

    partial void CloneSkeletronData(AnomalyGlobalNPC clone)
    {
        clone.Skeletron_HandBloodDust = null;
    }
}
