namespace Anomalies.SpecialContents.Developer.EventideReunion;

internal static class EventideReunionRuneAssets
{
    public const string AssetPath = EventideReunionAssets.AssetPath + "Rune/";
    public const string SunPath = AssetPath + "Sun";

    [LoadTexture(SunPath)]
    public static Asset<Texture2D> Sun;
    [LoadTexture(AssetPath + "Samsara")]
    public static Asset<Texture2D> Samsara;
    [LoadTexture(AssetPath + "Life")]
    public static Asset<Texture2D> Life;
    [LoadTexture(AssetPath + "Death")]
    public static Asset<Texture2D> Death;
    [LoadTexture(AssetPath + "Sacredness")]
    public static Asset<Texture2D> Sacredness;
    [LoadTexture(AssetPath + "Star")]
    public static Asset<Texture2D> Star;

    public static Texture2D GetTexture(EventideReunionAspect aspect) => (aspect switch
    {
        EventideReunionAspect.Sun => Sun,
        EventideReunionAspect.Samsara => Samsara,
        EventideReunionAspect.Life => Life,
        EventideReunionAspect.Death => Death,
        EventideReunionAspect.Sacredness => Sacredness,
        EventideReunionAspect.Star => Star,
        _ => throw new ArgumentOutOfRangeException(nameof(aspect), aspect, null)
    }).Value;
}
