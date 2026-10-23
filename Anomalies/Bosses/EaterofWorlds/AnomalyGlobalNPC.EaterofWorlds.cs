using Anomalies.Bosses.EaterofWorlds;

namespace Anomalies.Common;

public sealed partial class AnomalyGlobalNPC
{
    // The worm's segment graph is server-local entity state, not singleton Behavior state.
    internal List<EaterofWorldsSegmentContainer> EaterOfWorlds_Segments { get; private set; } = [];

    partial void CloneEaterofWorldsData(AnomalyGlobalNPC clone)
    {
        clone.EaterOfWorlds_Segments = [with(EaterOfWorlds_Segments)];
    }
}
