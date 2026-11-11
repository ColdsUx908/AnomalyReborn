using Terraria;

namespace AnomaliesMusic.Scenes;

public sealed class CrimsonMusicScene : BiomeMusicScene
{
    protected override string TrackName => "ForsakenBloodland";

    protected override bool IsEnabled(AnomaliesMusicConfig config) => config.ForsakenBloodland;

    protected override bool IsBiomeActive(Player player) => player.ZoneCrimson &&
        (player.ZoneOverworldHeight || player.ZoneDirtLayerHeight || player.ZoneRockLayerHeight);
}
