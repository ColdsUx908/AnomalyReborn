using Transoceanic.Behaviors.Collections;

namespace Transoceanic.Behaviors.Dispatching;

public sealed partial class GlobalProjectileBehaviorHandler : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    public static readonly GlobalEntityBehaviorSet<Projectile, GlobalProjectileBehavior> BehaviorSet = new();
}
