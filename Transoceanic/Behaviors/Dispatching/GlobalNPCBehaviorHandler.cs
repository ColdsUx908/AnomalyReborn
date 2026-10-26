using Transoceanic.Behaviors.Collections;

namespace Transoceanic.Behaviors.Dispatching;

public sealed partial class GlobalNPCBehaviorHandler : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public static readonly GlobalEntityBehaviorSet<NPC, GlobalNPCBehavior> BehaviorSet = new();
}
