// Developed by ColdsUx

namespace Anomalies.Assets;

public static class AnomalySounds
{
    public const string SoundPathPrefix = "Anomalies/Assets/Sounds/";

    public static readonly SoundStyle AromalyActivate = new(SoundPathPrefix + "AromalyActivate") { Volume = 0.6f };
    public static readonly SoundStyle MetalPipeFalling = new(SoundPathPrefix + "MetalPipeFalling");
}
