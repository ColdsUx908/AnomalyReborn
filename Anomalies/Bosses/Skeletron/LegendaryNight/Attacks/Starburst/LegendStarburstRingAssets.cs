using Anomalies.Assets.Effects;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

internal static class LegendStarburstRingAssets
{
    private const string TexturePath = "Anomalies/Bosses/Skeletron/LegendaryNight/Attacks/Starburst/Textures/";
    [LoadTexture(TexturePath + "RingMarkGradient")]
    private static Asset<Texture2D> _RingMarkGradient;

    public static bool IsLoaded => !Main.dedServ && AnomalyClientConfig.Instance.EnableShaders
        && LegendStarburstCircleAssets.IsLoaded && _RingMarkGradient?.IsLoaded == true
        && AnomalyEffects.Overlays.LegendStarburstRingMark?.Asset.IsLoaded == true;

    public static LegendStarburstRingVisuals.Textures Textures
    {
        get
        {
            LegendStarburstCircleVisuals.Textures shared = LegendStarburstCircleAssets.Textures;
            return new(_RingMarkGradient.Value, shared.ShotLine, shared.GlowDot, shared.Star4, shared.StarFlare);
        }
    }
}
