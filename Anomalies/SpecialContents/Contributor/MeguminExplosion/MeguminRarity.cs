namespace Anomalies.SpecialContents.Contributor.MeguminExplosion;

public sealed class MeguminRarity : ModRarity
{
    public override bool IsLoadingEnabled(Mod mod) => AnomalySpecialContentToggler.MeguminExplosion;

    public override Color RarityColor => Color.Lerp(new Color(244, 61, 67), new Color(255, 214, 112),
        MathF.Sin(Main.GlobalTimeWrappedHourly * 2f) * 0.5f + 0.5f);

    public static Color LoreColor => Color.Lerp(new Color(215, 149, 255), new Color(255, 211, 196),
        MathF.Sin(Main.GlobalTimeWrappedHourly * 1.4f + 1.2f) * 0.5f + 0.5f);

    // Reforging keeps the staff's own rarity and name gradient.
    public override int GetPrefixedRarity(int offset, float valueMult) => Type;
}
