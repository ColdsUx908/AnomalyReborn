using Anomalies.Bosses.BrainofCthulhu;

namespace Anomalies.Common;

public sealed partial class AnomalyGlobalNPC
{
    // 克脑的客户端绘制数据，不参与网络快照。
    public List<BrainOfCthulhuAfterImage> BrainOfCthulhu_AfterImages = [];
    public Rectangle BrainOfCthulhu_Frame = new(0, 0, 198, 180);

    partial void CloneBrainofCthulhuData(AnomalyGlobalNPC clone)
    {
        clone.BrainOfCthulhu_AfterImages = [];
        clone.BrainOfCthulhu_Frame = BrainOfCthulhu_Frame;
    }
}
