namespace Transoceanic.Behaviors;

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
