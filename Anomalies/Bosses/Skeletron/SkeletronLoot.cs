using Anomalies.Bosses.Skeletron.LegendaryNight;
using Anomalies.GameContents.AnomalyItems;
using Terraria.GameContent.ItemDropRules;

namespace Anomalies.Bosses.Skeletron;

public sealed class SkeletronLoot : AnomalyGlobalNPCBehavior
{
    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (npc.type == NPCID.SkeletronHead)
            npcLoot.Add(ItemDropRule.ByCondition(AnomalyItemLoot.AnomalyUltramundaneItemLootRule.Instance, ModContent.ItemType<FallenStarEssence>()));
    }
}
