#if DEBUG
using Anomalies.Bosses.Skeletron.SkeletronLegend;
using Anomalies.GameContents;

namespace Anomalies.SpecialContents.Developer;

/// <summary>
/// Anomaly测试物品。
/// </summary>
public sealed class TestItem : AnomalyModItem, IDeveloperItem
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
        Item.rare = ModContent.RarityType<Celestial>();
        Item.value = Celestial.CelestialPrice;
        Item.shoot = ModContent.ProjectileType<FallenStar>();
        Item.shootSpeed = 15f;
    }

    public override bool CanUseItem(Player player)
    {
        return true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        Projectile.NewProjectileAction(source, position, velocity, type, damage, knockback, action: p =>
        {
            p.scale = 0.1f;
        });
        return false;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        AnomalyItemTooltipModifier modifier = new(Item, tooltips);
        if (!modifier.TryGet(null, "Tooltip0", out int index, out _))
            return;

        IDeveloperItem.AddDeveloperItemIdentifier(Mod, tooltips, index);
    }
}
#endif
