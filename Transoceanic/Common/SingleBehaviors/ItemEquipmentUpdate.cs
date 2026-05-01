// Developed by ColdsUx

namespace Transoceanic.Common.SingleBehaviors;

public sealed class ItemEquipmentUpdate : TOGlobalItemBehavior
{
    public override decimal Priority => 500m;

    public override void UpdateEquip(Item item, Player player)
    {
        TOGlobalItem oceanItem = item.Ocean;
        oceanItem.Equip.Value = true;
        if (oceanItem.Equip_Timer.LastOnTime <= oceanItem.Equip_Timer.LastOffTime)
            oceanItem.Equip_Timer.LastOnTime = TOSharedData.GameTimer.TotalTicks;
    }

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        TOGlobalItem oceanItem = item.Ocean;
        if (!Main.gamePaused && Main.hasFocus && !oceanItem.Equip && oceanItem.Equip_Timer.LastOffTime < oceanItem.Equip_Timer.LastOnTime)
            oceanItem.Equip_Timer.LastOffTime = TOSharedData.GameTimer.TotalTicks;
        oceanItem.Equip.Value = false;
    }
}
