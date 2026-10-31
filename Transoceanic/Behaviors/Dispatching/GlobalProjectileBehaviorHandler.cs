using Transoceanic.Behaviors.Collections;

namespace Transoceanic.Behaviors.Dispatching;

/// <summary>
/// 将 tModLoader 实体回调转发到适用行为，按行为集合维护优先级、方法分组和实体绑定。
/// </summary>
internal sealed partial class GlobalProjectileBehaviorHandler : GlobalProjectile
{
    /// <inheritdoc/>
    /// <remarks>
    /// 返回 true，为每个实体分配独立的框架实例数据。
    /// </remarks>
    public override bool InstancePerEntity => true;

    /// <summary>
    /// 获取当前 Handler 用于查询和分发的行为集合；由框架在内容加载和卸载时填充、清理。
    /// </summary>
    internal static readonly GlobalEntityBehaviorSet<Projectile, GlobalProjectileBehavior> _BehaviorSet = new();
}
