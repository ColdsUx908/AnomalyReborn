namespace Anomalies.SpecialContents.Developer.EventideReunion;

public static class EventideReunionDeathAssets
{
    public const string AssetPath = EventideReunionAssets.AssetPath + "Death/";

    public const string StarfieldPath = AssetPath + "Starfield";

    [LoadTexture(StarfieldPath)]
    private static Asset<Texture2D> _Starfield;
    public static Texture2D Starfield => _Starfield.Value;
}
