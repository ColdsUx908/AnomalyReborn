namespace Anomalies.Common.SingleBehaviors;

public sealed class AnomalyPlayerDownedBossUpdate_Player : AnomalyPlayerBehavior
{
    public override void SaveData(TagCompound tag)
    {
        AnomalyPlayer.DownedBoss.SaveData(tag, "PlayerDownedBoss");
    }

    public override void LoadData(TagCompound tag)
    {
        AnomalyPlayer.DownedBoss.LoadData(tag, "PlayerDownedBoss");
    }
}

public sealed class AnomalyPlayerDownedBossUpdate_GlobalNPC : AnomalyGlobalNPCBehavior
{
    public override void OnKill(NPC npc)
    {
    }
}

public sealed class AnomalyPlayerDownedBossUpdate_System : ModSystem
{
    public override void PostUpdateNPCs()
    {
        foreach (Player player in Player.ActivePlayers)
            player.Anomaly.DownedBoss.WorldPolluted();
    }
}
