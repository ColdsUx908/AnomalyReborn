namespace Transoceanic.Behaviors;

/// <summary>
/// 面向全部弹幕的行为基类，由全局弹幕调度器筛选并调用。
/// </summary>
public abstract partial class GlobalProjectileBehavior : GlobalEntityBehavior<Projectile>
{
    /// <inheritdoc cref="GlobalProjectile.CanCutTiles"/>
    public virtual bool? CanCutTiles(Projectile projectile) => null;

    /// <inheritdoc cref="GlobalProjectile.CanDamage"/>
    public virtual bool? CanDamage(Projectile projectile) => null;

    /// <inheritdoc cref="GlobalProjectile.CanUseGrapple"/>
    public virtual bool? CanUseGrapple(int type, Player player) => null;

    /// <inheritdoc cref="GlobalProjectile.UseGrapple"/>
    public virtual void UseGrapple(Player player, ref int type) { }
}
