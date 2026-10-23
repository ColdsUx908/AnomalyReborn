namespace Anomalies.Common;

public sealed partial class AnomalyGlobalProjectile
{
    partial void CloneExtraData(AnomalyGlobalProjectile clone)
    {
        clone.BrainOfCthulhu_IgnoreIllusions = BrainOfCthulhu_IgnoreIllusions;
    }
}
