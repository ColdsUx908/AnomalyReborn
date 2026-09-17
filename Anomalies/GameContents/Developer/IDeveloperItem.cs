namespace Anomalies.GameContents.Developer;

public interface IDeveloperItem
{
    public static void AddDeveloperItemIdentifier(Mod mod, List<TooltipLine> tooltips, int index) => tooltips.Insert(index, new TooltipLine(mod, "Tooltip_DeveloperItemIdentifier", Language.GetTextValue(AnomalySharedData.ModLocalizationPrefix + "GameContents.DeveloperItemIdentifier")) { OverrideColor = AnomalySharedData.UltraIdentifierColor });

    public static void AddDeveloperItemOwnerIdentifier(Mod mod, List<TooltipLine> tooltips, int index, string ownerName) => tooltips.Insert(index, new TooltipLine(mod, "Tooltip_DeveloperItemOwnerIdentifier", Language.GetTextValue(AnomalySharedData.ModLocalizationPrefix + "GameContents.DeveloperItemOwnerIdentifier", ownerName)) { OverrideColor = AnomalySharedData.UltraIdentifierColor });
}
