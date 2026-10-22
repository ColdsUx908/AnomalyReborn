using Anomalies.GameContents.Base;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed class SkeletronLegendScene : BaseMusicSceneEffect
{
    public override int MusicModMusic => AnomaliesMusicHandler.CurseoftheDungeon;
    public override int VanillaMusic => MusicID.Boss1;

    public override bool CheckSingleNPC(NPC npc) => npc.type == NPCID.SkeletronHead && npc.Anomaly.CustomAIType == 1 && !SkeletronLegendHead.GetInstance(npc).Phase2MusicActive;
}

public sealed class SkeletronLegendScene2 : BaseMusicSceneEffect
{
    public override int MusicModMusic => AnomaliesMusicHandler.TornfromtheHeavens;
    public override int VanillaMusic => MusicID.EmpressOfLight;

    public override bool IsSceneEffectActive(Player player)
    {
        bool active = base.IsSceneEffectActive(player);
        // 直接使用完整淡入系数，实际音量仍遵循玩家的音乐音量设置。

        int music = Music;
        if (active && !Main.dedServ && music > 0 && music < Main.musicFade.Length)
            Main.musicFade[music] = 1f;
        return active;
    }

    public override bool CheckSingleNPC(NPC npc) => npc.type == NPCID.SkeletronHead && npc.Anomaly.CustomAIType == 1 && SkeletronLegendHead.GetInstance(npc).Phase2MusicActive;
}
