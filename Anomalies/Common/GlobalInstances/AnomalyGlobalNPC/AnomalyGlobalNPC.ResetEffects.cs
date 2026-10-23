namespace Anomalies.Common;

public sealed partial class AnomalyGlobalNPC
{
    partial void ResetExtraData()
    {
        General_SpecialFlags = default;
        Depth_DrawAnchor = null;
    }
}
