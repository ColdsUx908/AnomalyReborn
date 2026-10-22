namespace Anomalies.SpecialContents.Developer.EventideReunion;

public static class EventideReunionSounds
{
    public const string AssetPath = EventideReunionHoldout.AssetPath + "NormalAttack/";

    public static readonly SoundStyle Thrust = new(AssetPath + "Thrust") { MaxInstances = 0 };
    public static readonly SoundStyle SweepUpward = new(AssetPath + "SweepUpward") { MaxInstances = 0 };
    public static readonly SoundStyle DownwardSlash = new(AssetPath + "DownwardSlash") { MaxInstances = 0 };
}
