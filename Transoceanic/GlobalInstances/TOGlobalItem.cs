using Transoceanic.Game.Timing;

namespace Transoceanic.GlobalInstances;

/// <summary>
/// 保存每个物品的装备状态与装备计时。
/// </summary>
public sealed class TOGlobalItem : GlobalItem
{
    /// <inheritdoc/>
    /// <remarks>
    /// 返回 true，为每个实体分配独立的框架实例数据。
    /// </remarks>
    public override bool InstancePerEntity => true;

    /// <inheritdoc/>
    /// <remarks>
    /// 复制物品框架数据，按值保留装备滞后状态与切换计时。
    /// </remarks>
    public override GlobalItem Clone(Item from, Item to)
    {
        TOGlobalItem clone = (TOGlobalItem)base.Clone(from, to);

        clone.Equip = Equip;
        clone.EquipTimer = EquipTimer;

        return clone;
    }

    /// <summary>
    /// 装备状态的滞后布尔值，由装备和物品栏绘制回调维护。
    /// </summary>
    internal HysteresisBoolean Equip;
    /// <summary>
    /// 记录装备和脱装时刻的计时器，使用全局游戏帧计时。
    /// </summary>
    internal SwitchTimer EquipTimer;

    /// <summary>
    /// 获取一个物品的装备时长。
    /// </summary>
    /// <param name="max">计时上限，单位为游戏更新帧；与全局游戏帧计时使用同一单位。</param>
    /// <returns>装备时长。
    /// <br/>在物品装备时，返回值从0逐渐增加至max；未装备时，从max逐渐减少至0。
    /// </returns>
    public long GetEquippedTimer(long max) => EquipTimer.GetValue(TOGameTime.GameTimer.TotalTicks, max);
}
