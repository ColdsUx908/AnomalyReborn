using Transoceanic.Behaviors.Collections;
using Transoceanic.Behaviors.Dispatching;

namespace Transoceanic.Behaviors;

/// <summary>
/// 只读枚举框架登记的全局行为，不暴露调度集合的填充或清理权限。
/// </summary>
/// <remarks>保留优先级、方法分组、ShouldProcess 筛选和绑定锁语义。行为为共享实例，不可缓存或跨实体重入。查询与绑定不提供线程安全保证，须在游戏相应主线程阶段调用。</remarks>
public static class BehaviorQueries
{
    /// <summary>
    /// 按方法名枚举NPC的适用全局行为。
    /// </summary>
    /// <param name="methodName">业务回调名称，默认使用调用者方法名。</param>
    /// <returns>在 foreach 结束或中止时释放绑定锁的行为处理器。</returns>
    public static SimpleEntityBehaviorSet<NPC, GlobalNPCBehavior>.BehaviorProcesser EnumerateNPC([CallerMemberName] string methodName = null) =>
        GlobalNPCBehaviorHandler._BehaviorSet.Enumerate(methodName);

    /// <summary>
    /// 按方法名枚举NPC的适用全局行为，并在遍历时绑定指定实体。
    /// </summary>
    /// <param name="entity">遍历期间绑定的实体。</param>
    /// <param name="methodName">业务回调名称，默认使用调用者方法名。</param>
    /// <returns>在 foreach 结束或中止时释放绑定锁的行为处理器。</returns>
    public static SimpleEntityBehaviorSet<NPC, GlobalNPCBehavior>.ConnectedBehaviorProcesser EnumerateNPC(NPC entity, [CallerMemberName] string methodName = null) =>
        GlobalNPCBehaviorHandler._BehaviorSet.Enumerate(entity, methodName);

    /// <summary>
    /// 按方法名枚举NPC的适用全局行为。
    /// </summary>
    /// <typeparam name="T">筛选的行为类型。</typeparam>
    /// <param name="methodName">业务回调名称，默认使用调用者方法名。</param>
    /// <returns>在 foreach 结束或中止时释放绑定锁的行为处理器。</returns>
    public static SimpleEntityBehaviorSet<NPC, GlobalNPCBehavior>.TypedBehaviorProcesser<T> EnumerateNPC<T>([CallerMemberName] string methodName = null) where T : GlobalNPCBehavior =>
        GlobalNPCBehaviorHandler._BehaviorSet.Enumerate<T>(methodName);

    /// <summary>
    /// 按方法名枚举NPC的适用全局行为，并在遍历时绑定指定实体。
    /// </summary>
    /// <typeparam name="T">筛选的行为类型。</typeparam>
    /// <param name="entity">遍历期间绑定的实体。</param>
    /// <param name="methodName">业务回调名称，默认使用调用者方法名。</param>
    /// <returns>在 foreach 结束或中止时释放绑定锁的行为处理器。</returns>
    public static SimpleEntityBehaviorSet<NPC, GlobalNPCBehavior>.TypedConnectedBehaviorProcesser<T> EnumerateNPC<T>(NPC entity, [CallerMemberName] string methodName = null) where T : GlobalNPCBehavior =>
        GlobalNPCBehaviorHandler._BehaviorSet.Enumerate<T>(entity, methodName);

    /// <summary>
    /// 按方法名枚举Projectile的适用全局行为。
    /// </summary>
    /// <param name="methodName">业务回调名称，默认使用调用者方法名。</param>
    /// <returns>在 foreach 结束或中止时释放绑定锁的行为处理器。</returns>
    public static SimpleEntityBehaviorSet<Projectile, GlobalProjectileBehavior>.BehaviorProcesser EnumerateProjectile([CallerMemberName] string methodName = null) =>
        GlobalProjectileBehaviorHandler._BehaviorSet.Enumerate(methodName);

    /// <summary>
    /// 按方法名枚举Projectile的适用全局行为，并在遍历时绑定指定实体。
    /// </summary>
    /// <param name="entity">遍历期间绑定的实体。</param>
    /// <param name="methodName">业务回调名称，默认使用调用者方法名。</param>
    /// <returns>在 foreach 结束或中止时释放绑定锁的行为处理器。</returns>
    public static SimpleEntityBehaviorSet<Projectile, GlobalProjectileBehavior>.ConnectedBehaviorProcesser EnumerateProjectile(Projectile entity, [CallerMemberName] string methodName = null) =>
        GlobalProjectileBehaviorHandler._BehaviorSet.Enumerate(entity, methodName);

    /// <summary>
    /// 按方法名枚举Projectile的适用全局行为。
    /// </summary>
    /// <typeparam name="T">筛选的行为类型。</typeparam>
    /// <param name="methodName">业务回调名称，默认使用调用者方法名。</param>
    /// <returns>在 foreach 结束或中止时释放绑定锁的行为处理器。</returns>
    public static SimpleEntityBehaviorSet<Projectile, GlobalProjectileBehavior>.TypedBehaviorProcesser<T> EnumerateProjectile<T>([CallerMemberName] string methodName = null) where T : GlobalProjectileBehavior =>
        GlobalProjectileBehaviorHandler._BehaviorSet.Enumerate<T>(methodName);

    /// <summary>
    /// 按方法名枚举Projectile的适用全局行为，并在遍历时绑定指定实体。
    /// </summary>
    /// <typeparam name="T">筛选的行为类型。</typeparam>
    /// <param name="entity">遍历期间绑定的实体。</param>
    /// <param name="methodName">业务回调名称，默认使用调用者方法名。</param>
    /// <returns>在 foreach 结束或中止时释放绑定锁的行为处理器。</returns>
    public static SimpleEntityBehaviorSet<Projectile, GlobalProjectileBehavior>.TypedConnectedBehaviorProcesser<T> EnumerateProjectile<T>(Projectile entity, [CallerMemberName] string methodName = null) where T : GlobalProjectileBehavior =>
        GlobalProjectileBehaviorHandler._BehaviorSet.Enumerate<T>(entity, methodName);

    /// <summary>
    /// 按方法名枚举Item的适用全局行为。
    /// </summary>
    /// <param name="methodName">业务回调名称，默认使用调用者方法名。</param>
    /// <returns>在 foreach 结束或中止时释放绑定锁的行为处理器。</returns>
    public static SimpleEntityBehaviorSet<Item, GlobalItemBehavior>.BehaviorProcesser EnumerateItem([CallerMemberName] string methodName = null) =>
        GlobalItemBehaviorHandler._BehaviorSet.Enumerate(methodName);

    /// <summary>
    /// 按方法名枚举Item的适用全局行为，并在遍历时绑定指定实体。
    /// </summary>
    /// <param name="entity">遍历期间绑定的实体。</param>
    /// <param name="methodName">业务回调名称，默认使用调用者方法名。</param>
    /// <returns>在 foreach 结束或中止时释放绑定锁的行为处理器。</returns>
    public static SimpleEntityBehaviorSet<Item, GlobalItemBehavior>.ConnectedBehaviorProcesser EnumerateItem(Item entity, [CallerMemberName] string methodName = null) =>
        GlobalItemBehaviorHandler._BehaviorSet.Enumerate(entity, methodName);

    /// <summary>
    /// 按方法名枚举Item的适用全局行为。
    /// </summary>
    /// <typeparam name="T">筛选的行为类型。</typeparam>
    /// <param name="methodName">业务回调名称，默认使用调用者方法名。</param>
    /// <returns>在 foreach 结束或中止时释放绑定锁的行为处理器。</returns>
    public static SimpleEntityBehaviorSet<Item, GlobalItemBehavior>.TypedBehaviorProcesser<T> EnumerateItem<T>([CallerMemberName] string methodName = null) where T : GlobalItemBehavior =>
        GlobalItemBehaviorHandler._BehaviorSet.Enumerate<T>(methodName);

    /// <summary>
    /// 按方法名枚举Item的适用全局行为，并在遍历时绑定指定实体。
    /// </summary>
    /// <typeparam name="T">筛选的行为类型。</typeparam>
    /// <param name="entity">遍历期间绑定的实体。</param>
    /// <param name="methodName">业务回调名称，默认使用调用者方法名。</param>
    /// <returns>在 foreach 结束或中止时释放绑定锁的行为处理器。</returns>
    public static SimpleEntityBehaviorSet<Item, GlobalItemBehavior>.TypedConnectedBehaviorProcesser<T> EnumerateItem<T>(Item entity, [CallerMemberName] string methodName = null) where T : GlobalItemBehavior =>
        GlobalItemBehaviorHandler._BehaviorSet.Enumerate<T>(entity, methodName);

    /// <summary>
    /// 按方法名枚举Player的适用全局行为。
    /// </summary>
    /// <param name="methodName">业务回调名称，默认使用调用者方法名。</param>
    /// <returns>在 foreach 结束或中止时释放绑定锁的行为处理器。</returns>
    public static SimpleEntityBehaviorSet<Player, PlayerBehavior>.BehaviorProcesser EnumeratePlayer([CallerMemberName] string methodName = null) =>
        PlayerBehaviorHandler._BehaviorSet.Enumerate(methodName);

    /// <summary>
    /// 按方法名枚举Player的适用全局行为，并在遍历时绑定指定实体。
    /// </summary>
    /// <param name="entity">遍历期间绑定的实体。</param>
    /// <param name="methodName">业务回调名称，默认使用调用者方法名。</param>
    /// <returns>在 foreach 结束或中止时释放绑定锁的行为处理器。</returns>
    public static SimpleEntityBehaviorSet<Player, PlayerBehavior>.ConnectedBehaviorProcesser EnumeratePlayer(Player entity, [CallerMemberName] string methodName = null) =>
        PlayerBehaviorHandler._BehaviorSet.Enumerate(entity, methodName);

    /// <summary>
    /// 按方法名枚举Player的适用全局行为。
    /// </summary>
    /// <typeparam name="T">筛选的行为类型。</typeparam>
    /// <param name="methodName">业务回调名称，默认使用调用者方法名。</param>
    /// <returns>在 foreach 结束或中止时释放绑定锁的行为处理器。</returns>
    public static SimpleEntityBehaviorSet<Player, PlayerBehavior>.TypedBehaviorProcesser<T> EnumeratePlayer<T>([CallerMemberName] string methodName = null) where T : PlayerBehavior =>
        PlayerBehaviorHandler._BehaviorSet.Enumerate<T>(methodName);

    /// <summary>
    /// 按方法名枚举Player的适用全局行为，并在遍历时绑定指定实体。
    /// </summary>
    /// <typeparam name="T">筛选的行为类型。</typeparam>
    /// <param name="entity">遍历期间绑定的实体。</param>
    /// <param name="methodName">业务回调名称，默认使用调用者方法名。</param>
    /// <returns>在 foreach 结束或中止时释放绑定锁的行为处理器。</returns>
    public static SimpleEntityBehaviorSet<Player, PlayerBehavior>.TypedConnectedBehaviorProcesser<T> EnumeratePlayer<T>(Player entity, [CallerMemberName] string methodName = null) where T : PlayerBehavior =>
        PlayerBehaviorHandler._BehaviorSet.Enumerate<T>(entity, methodName);

}
