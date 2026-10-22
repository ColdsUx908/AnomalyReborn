namespace Anomalies.SpecialContents.Developer.EventideReunion;

public static class EventideReunionSunSounds
{
    public const string SunAssetPath = EventideReunionHoldout.AssetPath + "Sun/";

    public static readonly SoundStyle SunSweep = new(SunAssetPath + "SunSweep") { MaxInstances = 0 };
    public static readonly SoundStyle SunDashHit = new(SunAssetPath + "SunDashHit") { MaxInstances = 0 };
    public static readonly SoundStyle SunFollowUpSlash = new(SunAssetPath + "SunFollowUpSlash") { MaxInstances = 0 };
}
