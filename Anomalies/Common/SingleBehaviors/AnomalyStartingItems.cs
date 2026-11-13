#if ANOMALY_MODE || SPECIAL_CONTENTS
#if ANOMALY_MODE
using Anomalies.AnomalyMode;
#endif
#if SPECIAL_CONTENTS
using Anomalies.SpecialContents.Contributor.ImmaculateWhite;
using Anomalies.SpecialContents.Contributor.VacuousBlack;
#endif

namespace Anomalies.Common.SingleBehaviors;

public sealed class AnomalyStartingItems : AnomalyPlayerBehavior
{
    public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
    {
        List<Item> result = [];

#if ANOMALY_MODE
        result.Add(Item.CreateItem<HeavenrendCrown>()); //用于在游戏初期即能开启异象模式

#endif

#if SPECIAL_CONTENTS
        if (Player.name == "人间小天使")
        {
            if (ModContent.GetModItem<ImmaculateWhite>() is not null)
                result.Add(Item.CreateItem<ImmaculateWhite>()); //纯白
            if (ModContent.GetModItem<VacuousBlack>() is not null)
                result.Add(Item.CreateItem<VacuousBlack>()); //纯黑
        }

#endif
        return result;
    }
}
#endif
