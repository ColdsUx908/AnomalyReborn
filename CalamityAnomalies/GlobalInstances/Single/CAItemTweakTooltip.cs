namespace CalamityAnomalies.GlobalInstances.Single;

public sealed class CAItemTweakTooltip : CAGlobalItemBehavior, ILocalizationPrefix
{
    public string LocalizationPrefix => CASharedData.TweakLocalizationPrefix;

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (CASharedData.TweakedItems[item.type] && tooltips.TryFindVanillaTooltipByName("ItemName", out int index, out _))
            tooltips.Insert(index + 1, new TooltipLine(Mod, "TweakIdentifier", this.GetTextValue("TweakIdentifier")) { OverrideColor = TOSharedData.CelestialColor });
    }
}