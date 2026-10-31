namespace Transoceanic.Behaviors.Projectiles.BuiltIn;

/// <summary>
/// 预留的弹幕生命周期行为，当前仅定义分发优先级，不提供生命周期回调。
/// </summary>
internal sealed class ProjectileLifeTimeManager : GlobalProjectileBehavior
{
    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>1000m</c>。</remarks>
    public override decimal Priority => 1000m;
}
