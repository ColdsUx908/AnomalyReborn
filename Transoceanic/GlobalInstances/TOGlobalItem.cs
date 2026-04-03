using Transoceanic.DataStructures;

namespace Transoceanic.GlobalInstances;

public class TOGlobalItem : GlobalItem
{
    public override bool InstancePerEntity => true;

    public override GlobalItem Clone(Item from, Item to)
    {
        TOGlobalItem clone = (TOGlobalItem)base.Clone(from, to);

        clone.Equip = Equip;
        clone.Equip_Timer = Equip_Timer;

        return clone;
    }

    internal GuaranteedBoolean Equip;
    internal SmoothInt Equip_Timer;

    /// <summary>
    /// 获取一个物品的装备时长。
    /// </summary>
    /// <param name="max"></param>
    /// <returns>装备时长。
    /// <br/>在物品装备时，返回值从0逐渐增加至max；未装备时，从max逐渐减少至0。
    /// </returns>
    public int GetEquippedTimer(int max) => Equip_Timer.GetValue(TOSharedData.GameTimer.TotalTicks, max);
}
