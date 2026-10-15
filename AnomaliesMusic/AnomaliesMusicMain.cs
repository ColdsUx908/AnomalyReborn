global using Terraria.ModLoader;

namespace AnomaliesMusic;

public sealed class AnomaliesMusicMain : Mod
{
    internal static AnomaliesMusicMain Instance { get; private set; }

    public override void Load()
    {
        Instance = this;
    }

    public override void Unload()
    {
        Instance = null;
    }
}
