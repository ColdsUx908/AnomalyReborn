using Transoceanic.Behaviors.Collections;

namespace Transoceanic.Behaviors;

/// <summary>
/// 全局实体行为的基类，为所有使用该行为的实体提供统一的 <see cref="SetDefaults(TEntity)"/> 默认值设置点。
/// 通常与全局行为集合 <see cref="GlobalEntityBehaviorSet{TEntity, TBehavior}"/> 配合使用。
/// </summary>
/// <typeparam name="TEntity">行为所附加的实体类型。</typeparam>
public abstract class GlobalEntityBehavior<TEntity> : GeneralEntityBehavior<TEntity> where TEntity : Entity
{
    /// <inheritdoc cref="GlobalType{TEntity, TGlobal}.SetDefaults(TEntity)"/>
    public virtual void SetDefaults(TEntity entity) { }
}
