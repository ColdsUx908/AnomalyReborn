namespace Transoceanic.Framework.Helpers;

public static class TONPCUtils
{
    public static bool IsDefeatingTwins(NPC npc) => npc.type switch
    {
        NPCID.Retinazer => !NPC.ActiveNPCs.Any(n => n.type == NPCID.Spazmatism),
        NPCID.Spazmatism => !NPC.ActiveNPCs.Any(n => n.type == NPCID.Retinazer),
        _ => false
    };

    public static bool DownedMechBossAll => NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3;
}
