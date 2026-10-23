using Anomalies.Bosses.BrainofCthulhu;

namespace Anomalies.Common;

public sealed partial class AnomalySynchronization
{
    static partial void ApplyBrainIllusionHit(int illusionIndex, int playerIndex) => BrainIllusionHit.Apply(illusionIndex, playerIndex);
}
