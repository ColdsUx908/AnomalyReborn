namespace Anomalies.Bosses.Skeletron.LegendaryNight;

internal static class LegendStarburstCircleAssets
{
    private const string TexturePath = "Anomalies/Bosses/Skeletron/LegendaryNight/Attacks/Starburst/Textures/";
    [LoadTexture(TexturePath + "AoeDisc")]
    private static Asset<Texture2D> _ExplosionDisc;
    [LoadTexture(TexturePath + "GlowDot")]
    private static Asset<Texture2D> _GlowDot;
    [LoadTexture(TexturePath + "ShockShell")]
    private static Asset<Texture2D> _ShockShell;
    [LoadTexture(TexturePath + "ShotLine")]
    private static Asset<Texture2D> _ShotLine;
    [LoadTexture(TexturePath + "SparkTail")]
    private static Asset<Texture2D> _SparkTail;
    [LoadTexture(TexturePath + "Star4")]
    private static Asset<Texture2D> _Star4;
    [LoadTexture(TexturePath + "StarFlare")]
    private static Asset<Texture2D> _StarFlare;
    [LoadTexture(TexturePath + "HShotA")]
    private static Asset<Texture2D> _WarningFlare;

    public static bool IsLoaded => !Main.dedServ && _ExplosionDisc?.IsLoaded == true && _GlowDot?.IsLoaded == true
        && _WarningFlare?.IsLoaded == true && _ShockShell?.IsLoaded == true && _ShotLine?.IsLoaded == true
        && _SparkTail?.IsLoaded == true && _Star4?.IsLoaded == true && _StarFlare?.IsLoaded == true;

    public static LegendStarburstCircleVisuals.Textures Textures => new(_GlowDot.Value, _Star4.Value,
        _StarFlare.Value, _ShotLine.Value, _ExplosionDisc.Value, _ShockShell.Value, _WarningFlare.Value, _SparkTail.Value);
}
