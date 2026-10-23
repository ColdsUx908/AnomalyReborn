namespace Anomalies.Common;

public sealed partial class AnomalyGlobalProjectile
{
    #region 独立实例数据
    #region 克苏鲁之脑
    // 弹幕在克脑幻象阶段开始前已存在时，不再触发真假脑命中判定。
    // 本地实体标记，保留原有不参与额外 AI 快照的行为；克隆时按值复制。
    public bool BrainOfCthulhu_IgnoreIllusions;
    #endregion

    #endregion
}
