namespace Anomalies.Bosses.BrainofCthulhu;

internal static class BrainBehaviorAccess
{
    public static bool ActiveBrain => (uint)NPC.crimsonBoss < Main.maxNPCs && Main.npc[NPC.crimsonBoss].active && Main.npc[NPC.crimsonBoss].type == NPCID.BrainofCthulhu && AnomalySharedData.Anomaly;

    public static BrainofCthulhu GetBrainBehavior(this NPC npc) => BrainofCthulhu.GetInstance(npc);
    public static CreeperAI GetCreeperBehavior(this NPC npc) => CreeperAI.GetInstance(npc);
    public static bool TryGetBrainBehavior(this NPC npc, out BrainofCthulhu behavior)
    {
        behavior = npc.GetBrainBehavior();
        return npc.active && npc.type == NPCID.BrainofCthulhu && behavior.ShouldProcess;
    }

    public static bool TryGetCreeperBehavior(this NPC npc, out CreeperAI behavior)
    {
        behavior = npc.GetCreeperBehavior();
        return npc.active && npc.type == NPCID.Creeper && behavior.ShouldProcess;
    }
}
