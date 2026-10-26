using Transoceanic.Behaviors.Collections;

namespace Transoceanic.Behaviors.Dispatching;

[CriticalBehavior]
public abstract partial class SingleProjectileBehaviorHandler<TProjectileBehavior> : GlobalProjectileBehavior where TProjectileBehavior : SingleProjectileBehavior
{
    protected abstract SingleEntityBehaviorSet<Projectile, TProjectileBehavior> BehaviorSet { get; }

    public virtual bool TryGetBehavior(Projectile projectile, out TProjectileBehavior projectileBehavior, [CallerMemberName] string methodName = null) => BehaviorSet.TryGetBehavior(projectile, methodName, out projectileBehavior);
}
