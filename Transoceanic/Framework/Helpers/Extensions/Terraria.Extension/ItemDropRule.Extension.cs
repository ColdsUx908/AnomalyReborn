// Designed by ColdsUx

using Terraria.GameContent.ItemDropRules;

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(ItemDropRule)
    {
        /// <summary>
        /// 创建一个自定义条件的掉落规则。
        /// </summary>
        /// <param name="canDrop">判断是否允许掉落的条件委托。</param>
        /// <param name="canShowItemDropInUI">判断是否在 UI 中显示该掉落的条件委托。</param>
        /// <param name="getConditionDescription">获取条件描述的委托。</param>
        /// <param name="itemId">掉落物品的 ID。</param>
        /// <param name="chanceDenominator">掉落概率的分母，默认为 1。</param>
        /// <param name="minimumDropped">最小掉落数量，默认为 1。</param>
        /// <param name="maximumDropped">最大掉落数量，默认为 1。</param>
        /// <param name="chanceNumerator">掉落概率的分子，默认为 1。</param>
        /// <returns>配置好的自定义条件掉落规则。</returns>
        public static IItemDropRule ByCustomCondition(Func<DropAttemptInfo, bool> canDrop, Func<bool> canShowItemDropInUI, Func<string> getConditionDescription, int itemId, int chanceDenominator = 1, int minimumDropped = 1, int maximumDropped = 1, int chanceNumerator = 1) =>
            ItemDropRule.ByCondition(new CustomDropRuleCondition(canDrop, canShowItemDropInUI, getConditionDescription), itemId, chanceDenominator, minimumDropped, maximumDropped, chanceNumerator);
    }
}