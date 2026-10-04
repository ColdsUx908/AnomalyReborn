namespace Anomalies.GameContents.AnomalyItems.HoneyedWildbloom;

public sealed class HoneyedWildbloomBuff : ModBuff
{
    public override LocalizedText DisplayName => ModContent.GetModItem<HoneyedWildbloom>()?.DisplayName;
    public override LocalizedText Description => Language.GetText(AnomalySharedData.ModLocalizationPrefix + LocalizationCategory + ".BuffDescription");

    public override string LocalizationCategory => "GameContents.AnomalyItems.HoneyedWildbloom";

    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
        Main.vanityPet[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.buffTime[buffIndex] = 18000;
        bool petProjectileNotSpawned = player.ownedProjectileCounts[ModContent.ProjectileType<FriendlyBeePet>()] <= 0;
        if (petProjectileNotSpawned && player.whoAmI == Main.myPlayer)
            Projectile.NewProjectileAction<FriendlyBeePet>(player.GetSource_Buff(buffIndex), player.Center, Vector2.Zero, 0, 0f);
    }
}
