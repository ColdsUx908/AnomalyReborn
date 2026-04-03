using Terraria.GameContent.ItemDropRules;
using Transoceanic.DataStructures;

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(ItemDropRule)
    {
        public static IItemDropRule ByCustomCondition(Func<DropAttemptInfo, bool> canDrop, Func<bool> canShowItemDropInUI, Func<string> getConditionDescription, int itemId, int chanceDenominator = 1, int minimumDropped = 1, int maximumDropped = 1, int chanceNumerator = 1) =>
            ItemDropRule.ByCondition(new CustomDropRuleCondition(canDrop, canShowItemDropInUI, getConditionDescription), itemId, chanceDenominator, minimumDropped, maximumDropped, chanceNumerator);
    }
}