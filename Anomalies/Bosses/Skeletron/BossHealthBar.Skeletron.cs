using Anomalies.Bosses.Skeletron;

namespace Anomalies.Visuals.BossBar;

public partial class BossHealthBar
{
    static partial void FilterSkeletronHand(NPC boss, NPC npc, ref bool include)
    {
        if (boss.type == NPCID.SkeletronHead && SkeletronHead.GetInstance(boss).ShouldProcess)
            include = SkeletronHead.IsLivingHand(boss, npc);
    }
}
