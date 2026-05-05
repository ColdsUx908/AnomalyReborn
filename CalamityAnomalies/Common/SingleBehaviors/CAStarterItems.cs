using CalamityAnomalies.GameContents.Contributor.Mocangran_ImmaculateWhite;
using CalamityMod.Items.TreasureBags.MiscGrabBags;
using Terraria.GameContent.ItemDropRules;

namespace CalamityAnomalies.Common.SingleBehaviors;

public sealed class CAStarterItems : CAGlobalItemBehavior
{
    public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
    {
        if (item.ModItem is not StarterBag)
            return;

        Player player = Main.LocalPlayer;
        if (player.name == "人间小天使") //纯白
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<ImmaculateWhite>()));
    }
}
