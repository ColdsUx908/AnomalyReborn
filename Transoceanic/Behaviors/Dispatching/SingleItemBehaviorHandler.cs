using Transoceanic.Behaviors.Collections;

namespace Transoceanic.Behaviors.Dispatching;

[CriticalBehavior]
public abstract partial class SingleItemBehaviorHandler<TItemBehavior> : GlobalItemBehavior where TItemBehavior : SingleItemBehavior
{
    protected abstract SingleEntityBehaviorSet<Item, TItemBehavior> BehaviorSet { get; }

    public virtual bool TryGetBehavior(Item item, out TItemBehavior itemBehavior, [CallerMemberName] string methodName = null) => BehaviorSet.TryGetBehavior(item, methodName, out itemBehavior);

    public override void AddRecipes()
    {
        foreach (SimpleEntityBehaviorSet<Item, TItemBehavior> simpleSet in BehaviorSet._Data.Values)
        {
            foreach (TItemBehavior itemBehavior in simpleSet.Enumerate())
                itemBehavior.AddRecipes();
        }
    }

    public override void NetSend(Item item, BinaryWriter writer)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.NetSend(writer);
    }

    public override void NetReceive(Item item, BinaryReader reader)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.NetReceive(reader);
    }
}
