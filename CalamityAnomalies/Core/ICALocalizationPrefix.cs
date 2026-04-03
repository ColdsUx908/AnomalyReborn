namespace CalamityAnomalies.Core;

public enum CAGamePhase
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

public interface ICALocalizationPrefix : ILocalizationPrefix
{
    public abstract CAGamePhase Phase { get; }
    public abstract string LocalizationName { get; }

    string ILocalizationPrefix.LocalizationPrefix => CASharedData.TweakLocalizationPrefix + Phase switch
    {
        CAGamePhase.Beginning => "1.1.",
        CAGamePhase.PostEvil1 => "1.2.",
        CAGamePhase.PostEvil2 => "1.3.",
        CAGamePhase.PostSkeletron => "1.4.",
        CAGamePhase.Hardmode => "2.1.",
        CAGamePhase.PostMechanics => "2.2.",
        CAGamePhase.PostPlantera => "3.1.",
        CAGamePhase.PostGolem => "3.2.",
        CAGamePhase.PostCultist => "3.3.",
        CAGamePhase.PostMoonlord => "4.1.",
        CAGamePhase.PostProvidence => "4.2.",
        CAGamePhase.PostPolterghast => "4.3.",
        CAGamePhase.PostDoG => "5.1.",
        CAGamePhase.PostYharon => "5.2.",
        CAGamePhase.Focus => "6.",
        _ => ""
    } + LocalizationName;
}

