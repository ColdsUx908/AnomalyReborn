// Developed by ColdsUx

namespace CalamityAnomalies.Assets;

public static class CASounds
{
    public const string SoundPathPrefix = "CalamityAnomalies/Assets/Sounds/";

    public static readonly SoundStyle AromalyActivate = new(SoundPathPrefix + "AromalyActivate") { Volume = 0.6f };
    public static readonly SoundStyle MetalPipeFalling = new(SoundPathPrefix + "MetalPipeFalling");
}
