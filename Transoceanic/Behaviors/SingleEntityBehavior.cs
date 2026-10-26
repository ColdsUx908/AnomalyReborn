using Transoceanic.Behaviors.Collections;

namespace Transoceanic.Behaviors;

/// <summary>
/// 单一实体行为的基类，通过 <see cref="ApplyingType"/> 指定该行为所作用的特定实体类型。
/// 在 <see cref="SingleEntityBehaviorSet{TEntity, TBehavior}"/> 中按实体类型进行分组和检索。
/// </summary>
/// <typeparam name="TEntity">行为所附加的实体类型。</typeparam>
public abstract class SingleEntityBehavior<TEntity> : EntityBehavior<TEntity> where TEntity : Entity
{
    /// <summary>
    /// 此行为所应用的实体 ID。
    /// </summary>
    public abstract int ApplyingType { get; }

    /// <inheritdoc cref="GlobalType{TEntity, TGlobal}.SetDefaults(TEntity)"/>
    public virtual void SetDefaults() { }
}
