namespace Anomalies.SpecialContents.Developer.EventideReunion;

public static class EventideReunionSunAssets
{
    public const string AssetPath = EventideReunionAssets.AssetPath + "Sun/";

    public static readonly SoundStyle SunSweep = new(AssetPath + "SunSweep") { MaxInstances = 0 };
    public static readonly SoundStyle SunDashHit = new(AssetPath + "SunDashHit") { MaxInstances = 0 };
    public static readonly SoundStyle SunFollowUpSlash = new(AssetPath + "SunFollowUpSlash") { MaxInstances = 0 };
}
