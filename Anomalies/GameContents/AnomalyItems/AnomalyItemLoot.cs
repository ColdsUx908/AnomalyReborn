using Terraria.GameContent.ItemDropRules;

namespace Anomalies.GameContents.AnomalyItems;

public sealed class AnomalyItemLoot : AnomalyGlobalNPCBehavior
{
    public sealed class AnomalyItemLootRule : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info) => AnomalySharedData.Anomaly;
        public bool CanShowItemDropInUI() => true;
        public string GetConditionDescription() => Language.GetTextValue(AnomalySharedData.ModLocalizationPrefix + "GameContents.AnomalyItems.AnomalyLoot");

        public AnomalyItemLootRule() { }
    }

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        switch (npc.type)
        {
            case NPCID.QueenBee:
                npcLoot.Add(ItemDropRule.ByCondition(new AnomalyItemLootRule(), ModContent.ItemType<HoneyedWildbloom.HoneyedWildbloom>(), 1));
                break;
        }
    }
}
