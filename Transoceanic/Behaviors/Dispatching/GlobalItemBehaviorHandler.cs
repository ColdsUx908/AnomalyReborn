using Transoceanic.Behaviors.Collections;

namespace Transoceanic.Behaviors.Dispatching;

public sealed partial class GlobalItemBehaviorHandler : GlobalItem
{
    public override bool InstancePerEntity => true;

    public static readonly GlobalEntityBehaviorSet<Item, GlobalItemBehavior> BehaviorSet = new();

    public override int ChoosePrefix(Item item, UnifiedRandom rand)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
        {
            int result = behavior.ChoosePrefix(item, rand);
            if (result != -1)
                return result;
        }
        return -1;
    }

    public override string IsArmorSet(Item head, Item body, Item legs)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
        {
            string result = behavior.IsArmorSet(head, body, legs);
            if (result != "")
                return result;
        }
        return "";
    }

    public override string IsVanitySet(int head, int body, int legs)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.Enumerate())
        {
            string result = behavior.IsVanitySet(head, body, legs);
            if (result != "")
                return result;
        }
        return "";
    }

    /*
    public override void NetSend(Item item, BinaryWriter writer)
    {
        if (TONetworkEnvironment.SyncEnabled)
        {
            //foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            //    behavior.NetSend(item, writer);
        }
    }

    public override void NetReceive(Item item, BinaryReader reader)
    {
        if (TONetworkEnvironment.SyncEnabled)
        {
            //foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            //    behavior.NetReceive(item, reader);
        }
    }
    */
}
