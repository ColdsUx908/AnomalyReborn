namespace Transoceanic.Behaviors.Collections;

/// <summary>
/// 表示一个全局实体行为集合，与 <see cref="GeneralEntityBehaviorSet{TEntity, TBehavior}"/> 相同。
/// 此类可能用于标记具有全局作用域的行为集合。
/// </summary>
/// <typeparam name="TEntity">行为所附加的实体类型，必须派生自 <see cref="Entity"/>。</typeparam>
/// <typeparam name="TBehavior">行为的具体类型，必须派生自 <see cref="GeneralEntityBehavior{TEntity}"/>。</typeparam>
public class GlobalEntityBehaviorSet<TEntity, TBehavior> : GeneralEntityBehaviorSet<TEntity, TBehavior>
    where TEntity : Entity
    where TBehavior : GeneralEntityBehavior<TEntity>;
