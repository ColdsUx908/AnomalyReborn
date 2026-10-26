using Transoceanic.Behaviors.Collections;

namespace Transoceanic.Behaviors.Dispatching;

[CriticalBehavior]
public abstract partial class SingleNPCBehaviorHandler<TNPCBehavior> : GlobalNPCBehavior where TNPCBehavior : SingleNPCBehavior
{
    protected abstract SingleEntityBehaviorSet<NPC, TNPCBehavior> BehaviorSet { get; }

    public virtual bool TryGetBehavior(NPC npc, out TNPCBehavior npcBehavior, [CallerMemberName] string methodName = null) => BehaviorSet.TryGetBehavior(npc, methodName, out npcBehavior);
}
