using Transoceanic.Game.Timing;
using Transoceanic.GlobalInstances;
namespace Transoceanic.Behaviors.Items.BuiltIn;

/// <summary>
/// 通过装备与物品栏绘制回调维护物品的装备状态及切换计时。
/// </summary>
internal sealed class ItemEquipmentUpdate : GlobalItemBehavior
{
    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>500m</c>。</remarks>
    public override decimal Priority => 500m;

    /// <inheritdoc/>
    /// <remarks>
    /// 标记物品正在装备；首次由未装备转为装备时记录全局帧计时。
    /// </remarks>
    public override void UpdateEquip(Item item, Player player)
    {
        TOGlobalItem oceanItem = item.Ocean;
        oceanItem.Equip.Value = true;
        if (oceanItem.EquipTimer.LastOnTime <= oceanItem.EquipTimer.LastOffTime)
            oceanItem.EquipTimer.LastOnTime = TOGameTime.GameTimer.TotalTicks;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 在游戏正常运行且物品已脱下时记录脱装时间，随后清除当前装备标志。
    /// </remarks>
    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        TOGlobalItem oceanItem = item.Ocean;
        if (!Main.gamePaused && Main.hasFocus && !oceanItem.Equip && oceanItem.EquipTimer.LastOffTime < oceanItem.EquipTimer.LastOnTime)
            oceanItem.EquipTimer.LastOffTime = TOGameTime.GameTimer.TotalTicks;
        oceanItem.Equip.Value = false;
    }
}

