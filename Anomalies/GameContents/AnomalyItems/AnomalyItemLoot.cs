#if ANOMALY_MODE && GAME_CONTENTS
using Terraria.GameContent.ItemDropRules;

namespace Anomalies.GameContents.AnomalyItems;

public sealed class AnomalyItemLoot : AnomalyGlobalNPCBehavior
{
    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        switch (npc.type)
        {
            case NPCID.QueenBee:
                npcLoot.Add(ItemDropRule.ByCondition(AnomalyLootConditions.AnomalyItemLootRule.Instance, ModContent.ItemType<HoneyedWildbloom.HoneyedWildbloom>()));
                break;
        }
    }
}
#endif
