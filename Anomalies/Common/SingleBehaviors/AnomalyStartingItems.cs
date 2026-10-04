using Anomalies.AnomalyMode;
using Anomalies.SpecialContents.Contributor.ImmaculateWhite;
using Anomalies.SpecialContents.Contributor.VacuousBlack;

namespace Anomalies.Common.SingleBehaviors;

public sealed class AnomalyStartingItems : AnomalyPlayerBehavior
{
    public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
    {
        List<Item> result = [];

        result.Add(Item.CreateItem<HeavenrendCrown>()); //用于在游戏初期即能开启异象模式

        if (Player.name == "人间小天使")
        {
            result.Add(Item.CreateItem<ImmaculateWhite>()); //纯白
            result.Add(Item.CreateItem<VacuousBlack>()); //纯黑
        }

        return result;
    }
}

