using Transoceanic.Behaviors.Collections;
using Transoceanic.Game.Networking;

namespace Transoceanic.Behaviors.Dispatching;

public sealed partial class PlayerBehaviorHandler : ModPlayer
{
    public static readonly GeneralEntityBehaviorSet<Player, PlayerBehavior> BehaviorSet = new();

    public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
    {
        health = StatModifier.Default;
        mana = StatModifier.Default;

        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
        {
            behavior.ModifyMaxStats(out StatModifier newHealth, out StatModifier newMana);
            health = health.CombineWith(newHealth);
            mana = mana.CombineWith(newMana);
        }
    }

    public override IEnumerable<Item> AddMaterialsForCrafting(out ItemConsumedCallback itemConsumedCallback)
    {
        itemConsumedCallback = null;
        List<Item> allItems = [];
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
        {
            IEnumerable<Item> items = behavior.AddMaterialsForCrafting(out ItemConsumedCallback callback);
            if (items is not null)
                allItems.AddRange(items);
            itemConsumedCallback += callback;
        }
        return allItems;
    }

    public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
    {
        List<Item> allItems = [];
        foreach (PlayerBehavior behavior in BehaviorSet.Enumerate(Player))
        {
            IEnumerable<Item> items = behavior.AddStartingItems(mediumCoreDeath);
            if (items is not null)
                allItems.AddRange(items);
        }
        return allItems;
    }

    public override void CopyClientState(ModPlayer targetCopy)
    {
        if (TONetworkEnvironment.SyncEnabled)
        {
            //foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            //    behavior.CopyClientState(targetCopy);
        }
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        if (TONetworkEnvironment.SyncEnabled)
        {
            //foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            //    behavior.SyncPlayer(toWho, fromWho, newPlayer);
        }
    }

    public override void SendClientChanges(ModPlayer clientPlayer)
    {
        if (TONetworkEnvironment.SyncEnabled)
        {
            //foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            //    behavior.SendClientChanges(clientPlayer);
        }
    }
}
