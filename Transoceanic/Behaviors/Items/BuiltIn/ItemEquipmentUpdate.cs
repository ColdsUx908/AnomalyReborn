using Transoceanic.Game.Timing;
using Transoceanic.GlobalInstances;
namespace Transoceanic.Behaviors.Items.BuiltIn;

public sealed class ItemEquipmentUpdate : TOGlobalItemBehavior
{
    public override decimal Priority => 500m;

    public override void UpdateEquip(Item item, Player player)
    {
        TOGlobalItem oceanItem = item.Ocean;
        oceanItem.Equip.Value = true;
        if (oceanItem.EquipTimer.LastOnTime <= oceanItem.EquipTimer.LastOffTime)
            oceanItem.EquipTimer.LastOnTime = TOGameTime.GameTimer.TotalTicks;
    }

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        TOGlobalItem oceanItem = item.Ocean;
        if (!Main.gamePaused && Main.hasFocus && !oceanItem.Equip && oceanItem.EquipTimer.LastOffTime < oceanItem.EquipTimer.LastOnTime)
            oceanItem.EquipTimer.LastOffTime = TOGameTime.GameTimer.TotalTicks;
        oceanItem.Equip.Value = false;
    }
}

