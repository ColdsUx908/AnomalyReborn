using Anomalies.GameContents.Drinks;

namespace Anomalies.Common.SingleBehaviors;

public sealed class TravelShopHandler : AnomalyGlobalNPCBehavior
{
    public override void SetupTravelShop(int[] shop, ref int nextSlot)
    {
        if (nextSlot < shop.Length && Main.rand.NextBool(10))
            shop[nextSlot++] = ModContent.ItemType<BubbleTea>();

        if (nextSlot < shop.Length && Main.rand.NextBool(10))
            shop[nextSlot++] = ModContent.ItemType<PlumGreenTea>();

        if (nextSlot < shop.Length && Main.rand.NextBool(10))
            shop[nextSlot++] = ModContent.ItemType<RaspberryBlackTea>();
    }
}
