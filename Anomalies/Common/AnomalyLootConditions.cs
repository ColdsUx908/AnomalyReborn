#if ANOMALY_MODE
using Terraria.GameContent.ItemDropRules;

namespace Anomalies.Common;

/// <summary>异象难度掉落条件，供各内容模块独立使用。</summary>
public static class AnomalyLootConditions
{
    public sealed class AnomalyItemLootRule : IItemDropRuleCondition
    {
        public static readonly AnomalyItemLootRule Instance = new();

        public bool CanDrop(DropAttemptInfo info) => AnomalySharedData.Anomaly;
        public bool CanShowItemDropInUI() => true;
        public string GetConditionDescription() => Language.GetTextValue(AnomalySharedData.ModLocalizationPrefix + "GameContents.AnomalyItems.AnomalyLoot");

        private AnomalyItemLootRule() { }
    }

    public sealed class AnomalyUltramundaneItemLootRule : IItemDropRuleCondition
    {
        public static readonly AnomalyUltramundaneItemLootRule Instance = new();

        public bool CanDrop(DropAttemptInfo info) => Ultra;
        public bool CanShowItemDropInUI() => true;
        public string GetConditionDescription() => Language.GetTextValue(AnomalySharedData.ModLocalizationPrefix + "GameContents.AnomalyItems.AnomalyUltramundaneLoot");
        private AnomalyUltramundaneItemLootRule() { }
    }

}
#endif
