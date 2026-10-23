namespace Anomalies.SpecialContents.Developer.EventideReunion;

public static class EventideReunionAssets
{
    public const string AssetPath = "Anomalies/SpecialContents/Developer/EventideReunion/";

    public const string ItemPath = AssetPath + "EventideReunion";

    [LoadTexture(ItemPath)]
    private static Asset<Texture2D> _Item;
    public static Texture2D Item => _Item.Value;

    public const string WeaponPath = AssetPath + "EventideReunion_NoStreamer";

    [LoadTexture(WeaponPath)]
    private static Asset<Texture2D> _Weapon;
    public static Texture2D Weapon => _Weapon.Value;

    public const string SpearTrailPath = AssetPath + "SpearTrail";

    [LoadTexture(SpearTrailPath)]
    private static Asset<Texture2D> _SpearTrail;
    public static Texture2D SpearTrail => _SpearTrail.Value;
}
