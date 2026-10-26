using Transoceanic.Behaviors.Dispatching;
using Transoceanic.Runtime.Reflection;

namespace Transoceanic.Behaviors;

public sealed class BehaviorLoader : IContentLoader
{
    internal static IEnumerable<IEntityBehavior> _AllBehaviors;

    void IContentLoader.PostSetupContent()
    {
        _AllBehaviors = TOReflectionUtils.GetTypeInstancesDerivedFrom<IEntityBehavior>();

        PlayerBehaviorHandler.BehaviorSet.FillSet(_AllBehaviors.OfType<PlayerBehavior>());
        GlobalNPCBehaviorHandler.BehaviorSet.FillSet(_AllBehaviors.OfType<GlobalNPCBehavior>());
        GlobalProjectileBehaviorHandler.BehaviorSet.FillSet(_AllBehaviors.OfType<GlobalProjectileBehavior>());
        GlobalItemBehaviorHandler.BehaviorSet.FillSet(_AllBehaviors.OfType<GlobalItemBehavior>());
    }
    void IContentLoader.OnModUnload()
    {
        PlayerBehaviorHandler.BehaviorSet.Clear();
        GlobalNPCBehaviorHandler.BehaviorSet.Clear();
        GlobalProjectileBehaviorHandler.BehaviorSet.Clear();
        GlobalItemBehaviorHandler.BehaviorSet.Clear();

        _AllBehaviors = null;
    }
}
