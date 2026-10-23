namespace Anomalies.Common;

public sealed partial class AnomalyGlobalNPC
{
    partial void CloneBrainofCthulhuData(AnomalyGlobalNPC clone);

    partial void CloneEaterofWorldsData(AnomalyGlobalNPC clone);

    partial void CloneSkeletronData(AnomalyGlobalNPC clone);

    partial void CloneExtraData(AnomalyGlobalNPC clone)
    {
        clone.EventideReunion_DeathHitOwner = -1;
        clone.EventideReunion_DeathMarkTime = 0;
        clone.EventideReunion_DeathMarkProgress = 0f;
        clone.EventideReunion_DeathOwner = -1;
        clone.EventideReunion_SamsaraRecord = new();
        clone.EventideReunion_SamsaraOwner = -1;
        clone.EventideReunion_SamsaraVisualTime = 0;
        clone._Network_ProjectileReferenceCache = (ProjectileNetReference[])_Network_ProjectileReferenceCache.Clone();
        CloneBrainofCthulhuData(clone);
        CloneEaterofWorldsData(clone);
        CloneSkeletronData(clone);
        clone.Depth_DrawAnchor = null;
    }
}
