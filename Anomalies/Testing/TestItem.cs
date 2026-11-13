#if DEBUG

namespace Anomalies.Testing;

/// <summary>
/// Anomaly测试物品。
/// </summary>
public sealed partial class TestItem : AnomalyModItem
{
    public override string Texture => TOAssetUtils.FormatVanillaItemTexturePath(ItemID.IronBroadsword);

    public override string LocalizationCategory => "SpecialContents.Developer";

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.damage = 10;
        Item.DamageType = DamageClass.Default;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.autoReuse = true;
        Item.UseSound = SoundID.Item1;
        Item.knockBack = 5f;
        Item.noMelee = true;
        Item.rare = ItemRarityID.Purple;
        Item.shootSpeed = 15f;
    }

    public override bool CanResearch() => false;

    public override bool AltFunctionUse(Player player) => true;

    public override bool CanUseItem(Player player)
    {
        bool available = false;
        return available;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        return false;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        AnomalyItemTooltipModifier modifier = new(Item, tooltips);
        if (!modifier.TryGet(null, "Tooltip0", out int index, out _))
            return;

        tooltips.Insert(index, new TooltipLine(Mod, "Tooltip_DeveloperItemIdentifier",
            Language.GetTextValue(AnomalySharedData.ModLocalizationPrefix + "SpecialContents.DeveloperItemIdentifier"))
        { OverrideColor = AnomalySharedData.AnomalyUltramundaneColor });
    }
}
#endif
