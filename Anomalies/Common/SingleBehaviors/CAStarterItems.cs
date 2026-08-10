// Developed by ColdsUx

using Anomalies.GameContents.Contributor.Mocangran_ImmaculateWhite;
using Terraria.GameContent.ItemDropRules;

namespace Anomalies.Common.SingleBehaviors;

public sealed class AnomalyStarterItems : AnomalyPlayerBehavior
{
    public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
    {
        List<Item> result = [];

        if (Player.name == "人间小天使") //纯白
            result.Add(Item.CreateItem<ImmaculateWhite>());

        return result;
    }
}
