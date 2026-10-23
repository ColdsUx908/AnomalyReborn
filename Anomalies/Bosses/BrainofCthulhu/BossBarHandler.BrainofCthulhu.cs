using Anomalies.Bosses.BrainofCthulhu;

namespace Anomalies.Visuals.BossBar;

public partial class BossBarHandler
{
    static partial void RegisterBrainExclusions()
    {
        ExclusionList.Add(ModContent.NPCType<FalseBrain>());
        ExclusionList.Add(ModContent.NPCType<BrainIllusion>());
    }
}
