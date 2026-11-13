using Anomalies.Bosses.Skeletron.LegendaryNight;
using Terraria.GameContent.ItemDropRules;

namespace Anomalies.Bosses.Skeletron;

public sealed class SkeletronLoot : AnomalyGlobalNPCBehavior
{
    // Also covers natural spawns, until the world's first Skeletron victory.
    public override bool PreKill(NPC npc) => NPC.downedBoss3 || !IsPostPlanteraDungeonEnemy(npc.type);

    private static bool IsPostPlanteraDungeonEnemy(int type) => type is
        NPCID.BlueArmoredBones or NPCID.BlueArmoredBonesMace or NPCID.BlueArmoredBonesNoPants or NPCID.BlueArmoredBonesSword
        or NPCID.RustyArmoredBonesAxe or NPCID.RustyArmoredBonesFlail or NPCID.RustyArmoredBonesSword or NPCID.RustyArmoredBonesSwordNoArmor
        or NPCID.HellArmoredBones or NPCID.HellArmoredBonesMace or NPCID.HellArmoredBonesSpikeShield or NPCID.HellArmoredBonesSword
        or NPCID.Necromancer or NPCID.NecromancerArmored or NPCID.RaggedCaster or NPCID.RaggedCasterOpenCoat
        or NPCID.DiabolistRed or NPCID.DiabolistWhite or NPCID.SkeletonCommando or NPCID.SkeletonSniper or NPCID.TacticalSkeleton
        or NPCID.BoneLee or NPCID.GiantCursedSkull or NPCID.DungeonSpirit or NPCID.Paladin;

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (npc.type == NPCID.SkeletronHead)
            npcLoot.Add(ItemDropRule.ByCondition(AnomalyLootConditions.AnomalyUltramundaneItemLootRule.Instance, ModContent.ItemType<FallenStarEssence>()));
    }
}
