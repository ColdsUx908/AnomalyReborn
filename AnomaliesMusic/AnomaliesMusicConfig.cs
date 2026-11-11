using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace AnomaliesMusic;

public sealed class AnomaliesMusicConfig : ModConfig
{
    public static AnomaliesMusicConfig Instance { get; private set; }

    public override ConfigScope Mode => ConfigScope.ClientSide;

    public override void OnLoaded() => Instance = this;

    [Header("Theme")]

    [DefaultValue(true)]
    public bool PathwaytoReverie;

    [Header("Environment")]

    [DefaultValue(true)]
    public bool EchoesoftheAzure;

    [DefaultValue(true)]
    public bool ForsakenBloodland;

    [DefaultValue(true)]
    public bool DivineLament;
}
