namespace Anomalies.Bosses.Skeletron.LegendaryNight;

internal static class LegendStarburstCircleAssets
{
    private const string Path = "Anomalies/Bosses/Skeletron/LegendaryNight/Attacks/Starburst/Textures/";
    [LoadTexture(Path + "AoeDisc")]
    private static Asset<Texture2D> _AoeDisc;
    [LoadTexture(Path + "GlowDot")]
    private static Asset<Texture2D> _GlowDot;
    [LoadTexture(Path + "HShotA")]
    private static Asset<Texture2D> _HShotA;
    [LoadTexture(Path + "ShockShell")]
    private static Asset<Texture2D> _ShockShell;
    [LoadTexture(Path + "ShotLine")]
    private static Asset<Texture2D> _ShotLine;
    [LoadTexture(Path + "SparkTail")]
    private static Asset<Texture2D> _SparkTail;
    [LoadTexture(Path + "Star4")]
    private static Asset<Texture2D> _Star4;
    [LoadTexture(Path + "StarFlare")]
    private static Asset<Texture2D> _StarFlare;

    public static bool IsLoaded => !Main.dedServ && _AoeDisc?.IsLoaded == true && _GlowDot?.IsLoaded == true
        && _HShotA?.IsLoaded == true && _ShockShell?.IsLoaded == true && _ShotLine?.IsLoaded == true
        && _SparkTail?.IsLoaded == true && _Star4?.IsLoaded == true && _StarFlare?.IsLoaded == true;

    public static LegendStarburstCircleVisuals.Textures Textures => new(_GlowDot.Value, _Star4.Value,
        _StarFlare.Value, _ShotLine.Value, _AoeDisc.Value, _ShockShell.Value, _HShotA.Value, _SparkTail.Value);
}
