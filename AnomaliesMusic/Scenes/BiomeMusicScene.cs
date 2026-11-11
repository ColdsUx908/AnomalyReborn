using Terraria;

namespace AnomaliesMusic.Scenes;

public abstract class BiomeMusicScene : ModSceneEffect
{
    protected abstract string TrackName { get; }

    public sealed override int Music => MusicLoader.GetMusicSlot(Mod, $"Music/{TrackName}");

    public sealed override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

    public sealed override bool IsSceneEffectActive(Player player) =>
        IsEnabled(AnomaliesMusicConfig.Instance) && IsBiomeActive(player);

    protected abstract bool IsEnabled(AnomaliesMusicConfig config);

    protected abstract bool IsBiomeActive(Player player);
}
