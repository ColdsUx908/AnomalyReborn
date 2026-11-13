using Anomalies.Assets.Effects;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

internal static class LegendStarburstRingAssets
{
    private const string Path = "Anomalies/Bosses/Skeletron/LegendaryNight/Attacks/Starburst/Textures/";
    [LoadTexture(Path + "RingMarkGradient")]
    private static Asset<Texture2D> _RingMarkGradient;

    public static bool IsLoaded => !Main.dedServ && AnomalyClientConfig.Instance.EnableShaders
        && LegendStarburstCircleAssets.IsLoaded && _RingMarkGradient?.IsLoaded == true
        && AnomalyEffects.Overlays.LegendStarburstRingMark?.Asset.IsLoaded == true;

    public static LegendStarburstRingVisuals.Textures Textures => new(_RingMarkGradient.Value, LegendStarburstCircleAssets.Textures.ShotLine,
        LegendStarburstCircleAssets.Textures.GlowDot, LegendStarburstCircleAssets.Textures.Star4,
        LegendStarburstCircleAssets.Textures.StarFlare);
}
