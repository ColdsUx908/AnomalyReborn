// Developed by ColdsUx

namespace Anomalies.Common.SingleBehaviors;

public sealed class AnomalyItemTweakTooltip : AnomalyGlobalItemBehavior, ILocalizationPrefix
{
    public string LocalizationPrefix => AnomalySharedData.TweakLocalizationPrefix;

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if ( AnomalySharedData.TweakedItems[item.type] && tooltips.TryFindVanillaTooltipByName("ItemName", out int index, out _))
            tooltips.Insert(index + 1, new TooltipLine(Mod, "TweakIdentifier", this.GetTextValue("TweakIdentifier")) { OverrideColor = TOSharedData.CelestialColor });
    }
}