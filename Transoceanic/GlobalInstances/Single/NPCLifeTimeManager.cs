namespace Transoceanic.GlobalInstances.Single;

public sealed class NPCLifeTimeManager : TOGlobalNPCBehavior
{
    public override decimal Priority => 1000m;

    public override void SetDefaults(NPC npc)
    {
        npc.Master = null;
    }

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        npc.SpawnTime = TOSharedData.GameTimer.TotalTicks;
    }

    public override bool PreAI(NPC npc)
    {
        npc.ActiveTime++;
        return true;
    }
}
