namespace Anomalies.GameContents.AnomalyItems.HoneyedWildbloom;

public sealed class HoneyedWildbloom : AnomalyModItem
{
    public override string LocalizationCategory => "GameContents.AnomalyItems";

    public override void SetDefaults()
    {
        Item.DefaultToVanitypet(ModContent.ProjectileType<FriendlyBeePet>(), ModContent.BuffType<HoneyedWildbloomBuff>());
        Item.rare = ModContent.RarityType<AnomalyRarity>();
        Item.value = AnomalyRarity.AnomalyPrice;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => false;

    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            player.AddBuff(Item.buffType, 3600, true);
    }
}
