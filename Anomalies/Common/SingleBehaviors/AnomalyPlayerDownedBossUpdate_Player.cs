namespace Anomalies.Common.SingleBehaviors;

public sealed class AnomalyPlayerDownedBossUpdate_Player : AnomalyPlayerBehavior
{
    public override void SaveData(TagCompound tag)
    {
        AnomalyPlayer.Progression_DownedBoss.SaveData(tag, "PlayerDownedBoss");
    }

    public override void LoadData(TagCompound tag)
    {
        AnomalyPlayer.Progression_DownedBoss.LoadData(tag, "PlayerDownedBoss");
    }
}

public sealed class AnomalyPlayerDownedBossUpdate_GlobalNPC : AnomalyGlobalNPCBehavior
{
    public override void OnKill(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        bool eaterOfWorlds = npc.type is NPCID.EaterofWorldsHead or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail;
        if (eaterOfWorlds)
        {
            // 分裂后的任意一段都可能成为最后一段，不能只检查头部。
            foreach (NPC other in Main.ActiveNPCs)
                if (other.whoAmI != npc.whoAmI && other.life > 0 &&
                    other.type is NPCID.EaterofWorldsHead or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail)
                    return;
        }
        else if (npc.type is not (NPCID.BrainofCthulhu or NPCID.DD2Betsy or NPCID.BloodNautilus))
            return;

        AnomalyPlayerDownedBossUpdate_System.RecordKill(npc.type);
        if (Main.netMode == NetmodeID.Server)
        {
            ModPacket packet = AnomalySynchronization.GetCAPacket();
            packet.Write(AnomalySynchronization.ID.PlayerDownedBossKill);
            packet.Write(npc.type);
            packet.Send();
        }
    }
}

public sealed class AnomalyPlayerDownedBossUpdate_System : ModSystem
{
    internal static void RecordKill(int npcType)
    {
        foreach (Player player in Player.ActivePlayers)
        {
            PlayerDownedBoss progression = player.Anomaly.Progression_DownedBoss;
            switch (npcType)
            {
                case NPCID.EaterofWorldsHead:
                case NPCID.EaterofWorldsBody:
                case NPCID.EaterofWorldsTail:
                    progression.EaterOfWorld = true;
                    break;
                case NPCID.BrainofCthulhu:
                    progression.BrainOfCthulhu = true;
                    break;
                case NPCID.DD2Betsy:
                    progression.Betsy = true;
                    break;
                case NPCID.BloodNautilus:
                    progression.Dreadnautilus = true;
                    break;
            }
            progression.UpdateDerivedProgression();
        }
    }

    public override void PostUpdateNPCs()
    {
        foreach (Player player in Player.ActivePlayers)
            player.Anomaly.Progression_DownedBoss.WorldPolluted();
    }
}
