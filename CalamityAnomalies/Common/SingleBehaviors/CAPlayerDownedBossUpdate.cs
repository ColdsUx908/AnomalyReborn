using CalamityMod.NPCs.Abyss;

namespace CalamityAnomalies.Common.SingleBehaviors;

public sealed class CAPlayerDownedBossUpdate_Player : CAPlayerBehavior
{
    public override void SaveData(TagCompound tag)
    {
        AnomalyPlayer.DownedBossCalamity.SaveData(tag, "PlayerDownedBossCalamity");
        AnomalyPlayer.DownedBossAnomaly.SaveData(tag, "PlayerDownedBossAnomaly");
    }

    public override void LoadData(TagCompound tag)
    {
        AnomalyPlayer.DownedBossCalamity.LoadData(tag, "PlayerDownedBossCalamity");
        AnomalyPlayer.DownedBossAnomaly.LoadData(tag, "PlayerDownedBossAnomaly");
    }
}

public sealed class CAPlayerDownedBossUpdate_GlobalNPC : CAGlobalNPCBehavior
{
    public override void OnKill(NPC npc)
    {
        if (npc.ModNPC is EidolonWyrmHead && !DownedBossSystem_Bridge.downedPrimordialWyrm)
            DownedBossSystem_Bridge.downedPrimordialWyrm = true;

        foreach (Player player in Player.ActivePlayers)
            player.Anomaly.DownedBossCalamity.BossesOnKill(npc);
    }
}

public sealed class CAPlayerDownedBossUpdate_System : ModSystem
{
    public override void PostUpdateNPCs()
    {
        foreach (Player player in Player.ActivePlayers)
            player.Anomaly.DownedBossCalamity.WorldPolluted();
    }
}