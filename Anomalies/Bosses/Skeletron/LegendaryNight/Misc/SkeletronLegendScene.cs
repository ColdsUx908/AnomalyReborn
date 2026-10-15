using Anomalies.GameContents.Base;

namespace Anomalies.Bosses.Skeletron.LegendaryNight.Misc;

public sealed class SkeletronLegendScene : BaseMusicSceneEffect
{
    public override int MusicModMusic => AnomaliesMusicHandler.CurseoftheDungeon;
    public override int VanillaMusic => MusicID.Boss1;

    public override bool CheckSingleNPC(NPC npc) => npc.type == NPCID.SkeletronHead && npc.Anomaly.CustomAIType == 1 && !SkeletronLegendHead.GetInstance(npc).Phase2;
}

public sealed class SkeletronLegendScene2 : BaseMusicSceneEffect
{
    public override int MusicModMusic => AnomaliesMusicHandler.TornfromtheHeavens;
    public override int VanillaMusic => MusicID.EmpressOfLight;

    public override bool CheckSingleNPC(NPC npc) => npc.type == NPCID.SkeletronHead && npc.Anomaly.CustomAIType == 1 && SkeletronLegendHead.GetInstance(npc).Phase2;
}