#define TEST_DEV

#if TEST_DEV
using CalamityAnomalies.GameContents;
using CalamityAnomalies.Publicizers.CalamityMod;

namespace CalamityAnomalies.DeveloperContents;

/// <summary>
/// CA测试物品。
/// </summary>
public sealed class TestItem : CAModItem
{
    public override string Texture => TOAssetUtils.FormatVanillaItemTexturePath(ItemID.IronBroadsword);

    public override string LocalizationCategory => "DeveloperContents";

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.damage = 10;
        Item.DamageType = AverageDamageClass_Publicizer.Instance;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.autoReuse = true;
        Item.UseSound = SoundID.Item1;
        Item.knockBack = 5f;
        Item.noMelee = true;
        Item.rare = ModContent.RarityType<Celestial>();
        Item.value = Celestial.CelestialPrice;
        Item.shoot = ProjectileID.PurificationPowder;
        Item.shootSpeed = 12f;
        CalamityItem.devItem = true;
    }

    public override bool CanUseItem(Player player)
    {
        return true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        return false;
    }
}
#endif