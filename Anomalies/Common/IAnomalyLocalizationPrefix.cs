// Developed by ColdsUx

namespace Anomalies.Common;

public enum AnomalyGamePhase
{
    Beginning,
    PostEvil1,
    PostEvil2,
    PostSkeletron,
    Hardmode,
    PostMechanics,
    PostPlantera,
    PostGolem,
    PostCultist,
    PostMoonlord,
    PostProvidence,
    PostPolterghast,
    PostDoG,
    PostYharon,
    Focus,
}

public interface IAnomalyLocalizationPrefix : ILocalizationPrefix
{
    public abstract AnomalyGamePhase Phase { get; }
    public abstract string LocalizationName { get; }

    string ILocalizationPrefix.LocalizationPrefix => AnomalySharedData.TweakLocalizationPrefix + Phase switch
    {
        AnomalyGamePhase.Beginning => "1.1.",
        AnomalyGamePhase.PostEvil1 => "1.2.",
        AnomalyGamePhase.PostEvil2 => "1.3.",
        AnomalyGamePhase.PostSkeletron => "1.4.",
        AnomalyGamePhase.Hardmode => "2.1.",
        AnomalyGamePhase.PostMechanics => "2.2.",
        AnomalyGamePhase.PostPlantera => "3.1.",
        AnomalyGamePhase.PostGolem => "3.2.",
        AnomalyGamePhase.PostCultist => "3.3.",
        AnomalyGamePhase.PostMoonlord => "4.1.",
        AnomalyGamePhase.PostProvidence => "4.2.",
        AnomalyGamePhase.PostPolterghast => "4.3.",
        AnomalyGamePhase.PostDoG => "5.1.",
        AnomalyGamePhase.PostYharon => "5.2.",
        AnomalyGamePhase.Focus => "6.",
        _ => ""
    } + LocalizationName;
}

