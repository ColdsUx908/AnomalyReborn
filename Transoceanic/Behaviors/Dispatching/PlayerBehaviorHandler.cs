using Transoceanic.Behaviors.Collections;

namespace Transoceanic.Behaviors.Dispatching;

/// <summary>
/// 将 tModLoader 实体回调转发到适用行为，按行为集合维护优先级、方法分组和实体绑定。
/// </summary>
internal sealed partial class PlayerBehaviorHandler : ModPlayer
{
    /// <summary>
    /// 获取当前 Handler 用于查询和分发的行为集合；由框架在内容加载和卸载时填充、清理。
    /// </summary>
    internal static readonly GeneralEntityBehaviorSet<Player, PlayerBehavior> _BehaviorSet = new();

    /// <inheritdoc/>
    /// <remarks>
    /// 从默认修正开始，以 CombineWith 依次合并所有适用行为的生命和魔力修正。
    /// </remarks>
    public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
    {
        health = StatModifier.Default;
        mana = StatModifier.Default;

        foreach (PlayerBehavior behavior in _BehaviorSet.Enumerate(Player))
        {
            behavior.ModifyMaxStats(out StatModifier newHealth, out StatModifier newMana);
            health = health.CombineWith(newHealth);
            mana = mana.CombineWith(newMana);
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 合并适用行为提供的非 null 合成材料集合，并按遍历顺序组合材料消耗回调。
    /// </remarks>
    public override IEnumerable<Item> AddMaterialsForCrafting(out ItemConsumedCallback itemConsumedCallback)
    {
        itemConsumedCallback = null;
        List<Item> allItems = [];
        foreach (PlayerBehavior behavior in _BehaviorSet.Enumerate(Player))
        {
            IEnumerable<Item> items = behavior.AddMaterialsForCrafting(out ItemConsumedCallback callback);
            if (items is not null)
                allItems.AddRange(items);
            itemConsumedCallback += callback;
        }
        return allItems;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 合并适用行为提供的非 null 初始物品集合，将中核死亡标志原样传给各行为。
    /// </remarks>
    public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
    {
        List<Item> allItems = [];
        foreach (PlayerBehavior behavior in _BehaviorSet.Enumerate(Player))
        {
            IEnumerable<Item> items = behavior.AddStartingItems(mediumCoreDeath);
            if (items is not null)
                allItems.AddRange(items);
        }
        return allItems;
    }

}
