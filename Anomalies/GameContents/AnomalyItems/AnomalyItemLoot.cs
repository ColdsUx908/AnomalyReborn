using Anomalies.Bosses.Skeletron.LegendaryNight;
using Terraria.GameContent.ItemDropRules;

namespace Anomalies.GameContents.AnomalyItems;

public sealed class AnomalyItemLoot : AnomalyGlobalNPCBehavior
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

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        switch (npc.type)
        {
            case NPCID.QueenBee:
                npcLoot.Add(ItemDropRule.ByCondition(AnomalyItemLootRule.Instance, ModContent.ItemType<HoneyedWildbloom.HoneyedWildbloom>()));
                break;
            case NPCID.SkeletronHead:
                npcLoot.Add(ItemDropRule.ByCondition(AnomalyUltramundaneItemLootRule.Instance, ModContent.ItemType<FallenStarEssence>()));
                break;
        }
    }
}
