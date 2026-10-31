using Transoceanic.Behaviors.Dispatching;

namespace Transoceanic.Behaviors;

/// <summary>
/// 在内容设置完成后复用已注册的共享行为实例，填充各实体调度集合，并在卸载时清空。
/// </summary>
internal sealed class BehaviorLoader : IContentLoader
{
    /// <summary>
    /// 当前加载的行为实例集合；实例按类型共享，实体数据应保存在实体容器。
    /// </summary>
    internal static IEntityBehavior[] _AllBehaviors;

    void IContentLoader.PostSetupContent()
    {
        _AllBehaviors = TOLoaderHandler.GetLoaderInstances<IEntityBehavior>().ToArray();

        PlayerBehaviorHandler._BehaviorSet.FillSet(_AllBehaviors.OfType<PlayerBehavior>());
        GlobalNPCBehaviorHandler._BehaviorSet.FillSet(_AllBehaviors.OfType<GlobalNPCBehavior>());
        GlobalProjectileBehaviorHandler._BehaviorSet.FillSet(_AllBehaviors.OfType<GlobalProjectileBehavior>());
        GlobalItemBehaviorHandler._BehaviorSet.FillSet(_AllBehaviors.OfType<GlobalItemBehavior>());
    }
    void IContentLoader.OnModUnload()
    {
        PlayerBehaviorHandler._BehaviorSet.Clear();
        GlobalNPCBehaviorHandler._BehaviorSet.Clear();
        GlobalProjectileBehaviorHandler._BehaviorSet.Clear();
        GlobalItemBehaviorHandler._BehaviorSet.Clear();

        _AllBehaviors = null;
    }
}
