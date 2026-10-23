namespace Anomalies.Common;

public sealed partial class AnomalyPlayer
{
    partial void ResetExtraData()
    {
        Debuff_DimensionalRend = false;
        Minion_VacuousBlack = false;
        LegendaryNight_StarGodBlessingActive = false;
        LegendaryNight_StarGodBlessingOpacity = 0f;
        Visual_DarknessIntensity = MathHelper.Max(Visual_DarknessIntensity - 0.05f, 0);
    }
}
