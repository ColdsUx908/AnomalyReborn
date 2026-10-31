using Transoceanic.Behaviors.Collections;

namespace Transoceanic.Behaviors.Dispatching;

/// <summary>
/// 将 tModLoader 实体回调转发到适用行为，按行为集合维护优先级、方法分组和实体绑定。
/// </summary>
[CriticalBehavior]
public abstract partial class SingleNPCBehaviorHandler<TNPCBehavior> : GlobalNPCBehavior where TNPCBehavior : SingleNPCBehavior
{
    /// <summary>
    /// 获取当前 Handler 用于查询和分发的行为集合；由框架在内容加载和卸载时填充、清理。
    /// </summary>
    protected abstract SingleEntityBehaviorSet<NPC, TNPCBehavior> BehaviorSet { get; }

    /// <summary>
    /// 按实体、方法名和优先级查找首个满足 ShouldProcess 的行为并绑定实体；未命中返回 false。调用返回的共享行为时须立即使用 LockEntity 作用域，不可长期缓存。
    /// </summary>
    public virtual bool TryGetBehavior(NPC npc, out TNPCBehavior npcBehavior, [CallerMemberName] string methodName = null) => BehaviorSet.TryGetBehavior(npc, methodName, out npcBehavior);
}
