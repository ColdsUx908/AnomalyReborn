namespace Anomalies.Common;

public sealed partial class AnomalyPlayer
{
    partial void CloneExtraData(AnomalyPlayer clone)
    {
        clone.Coldheart_Phase = Coldheart_Phase;
        clone.Coldheart_SubPhase = Coldheart_SubPhase;
        clone.Debuff_DimensionalRend = Debuff_DimensionalRend;
        clone.Progression_DownedBoss = Progression_DownedBoss.Clone();
        clone.Visual_DarknessIntensity = Visual_DarknessIntensity;
        clone.EventideReunion_RightWasDown = EventideReunion_RightWasDown;
        clone.EventideReunion_SunCooldown = EventideReunion_SunCooldown;
        clone.EventideReunion_SamsaraCooldown = EventideReunion_SamsaraCooldown;
        clone.EventideReunion_DeathCooldown = EventideReunion_DeathCooldown;
        clone.EventideReunion_DeathProtection = EventideReunion_DeathProtection;
        clone.EventideReunion_SunFallProtection = EventideReunion_SunFallProtection;
        clone.EventideReunion_ResonanceMask = EventideReunion_ResonanceMask;
        clone.EventideReunion_ResonanceTime = EventideReunion_ResonanceTime;
        clone.LegendaryNight_VirtualBoneWings = LegendaryNight_VirtualBoneWings?.Clone();
        clone.LegendaryNight_StarGodBlessingActive = LegendaryNight_StarGodBlessingActive;
        clone.LegendaryNight_StarGodBlessingOpacity = LegendaryNight_StarGodBlessingOpacity;
    }
}
