namespace Anomalies.SpecialContents.Contributor;

internal interface IContributorItem
{
    public static void AddContributorItemIdentifier(Mod mod, List<TooltipLine> tooltips, int index) => tooltips.Insert(index, new TooltipLine(mod, "Tooltip_ContributorItemIdentifier", Language.GetTextValue(AnomalySharedData.ModLocalizationPrefix + "SpecialContents.ContributorItemIdentifier")) { OverrideColor = TOSharedData.CelestialColor });
    public static void AddContributorItemOwnerIdentifier(Mod mod, List<TooltipLine> tooltips, int index, string owner) => tooltips.Insert(index, new TooltipLine(mod, "Tooltip_ContributorItemOwnerIdentifier", Language.GetTextValue(AnomalySharedData.ModLocalizationPrefix + "SpecialContents.ContributorItemOwnerIdentifier", owner)) { OverrideColor = TOSharedData.CelestialColor });
}
