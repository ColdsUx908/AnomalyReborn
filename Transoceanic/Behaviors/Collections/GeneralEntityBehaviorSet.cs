namespace Transoceanic.Behaviors.Collections;

/// <summary>
/// 表示一个通用的实体行为集合，行为类型约束为 <see cref="GeneralEntityBehavior{TEntity}"/>。
/// </summary>
/// <typeparam name="TEntity">行为所附加的实体类型，必须派生自 <see cref="Entity"/>。</typeparam>
/// <typeparam name="TBehavior">行为的具体类型，必须派生自 <see cref="GeneralEntityBehavior{TEntity}"/>。</typeparam>
public class GeneralEntityBehaviorSet<TEntity, TBehavior> : SimpleEntityBehaviorSet<TEntity, TBehavior>
    where TEntity : Entity
    where TBehavior : GeneralEntityBehavior<TEntity>;
