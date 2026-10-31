using Transoceanic.Behaviors.Collections;

namespace Transoceanic.Behaviors.Dispatching;

/// <summary>
/// 将 tModLoader 实体回调转发到适用行为，按行为集合维护优先级、方法分组和实体绑定。
/// </summary>
internal sealed partial class GlobalItemBehaviorHandler : GlobalItem
{
    /// <inheritdoc/>
    /// <remarks>
    /// 返回 true，为每个实体分配独立的框架实例数据。
    /// </remarks>
    public override bool InstancePerEntity => true;

    /// <summary>
    /// 获取当前 Handler 用于查询和分发的行为集合；由框架在内容加载和卸载时填充、清理。
    /// </summary>
    internal static readonly GlobalEntityBehaviorSet<Item, GlobalItemBehavior> _BehaviorSet = new();

    /// <inheritdoc/>
    /// <remarks>
    /// 按优先级返回首个非 -1 的行为前缀结果；全部行为不作决定时返回 -1。
    /// </remarks>
    public override int ChoosePrefix(Item item, UnifiedRandom rand)
    {
        foreach (GlobalItemBehavior behavior in _BehaviorSet.Enumerate())
        {
            int result = behavior.ChoosePrefix(item, rand);
            if (result != -1)
                return result;
        }
        return -1;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 按优先级返回首个非空字符串的护甲套装名称；均不匹配时返回空字符串。
    /// </remarks>
    public override string IsArmorSet(Item head, Item body, Item legs)
    {
        foreach (GlobalItemBehavior behavior in _BehaviorSet.Enumerate())
        {
            string result = behavior.IsArmorSet(head, body, legs);
            if (result != "")
                return result;
        }
        return "";
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 按优先级返回首个非空字符串的时装套装名称；均不匹配时返回空字符串。
    /// </remarks>
    public override string IsVanitySet(int head, int body, int legs)
    {
        foreach (GlobalItemBehavior behavior in _BehaviorSet.Enumerate())
        {
            string result = behavior.IsVanitySet(head, body, legs);
            if (result != "")
                return result;
        }
        return "";
    }
}
