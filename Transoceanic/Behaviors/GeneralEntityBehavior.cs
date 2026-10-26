namespace Transoceanic.Behaviors;

/// <summary>
/// 通用实体行为的基类，不附加特定于全局或单一实体的功能。
/// 用于标识可应用于任意实体的普通行为类型。
/// </summary>
/// <typeparam name="TEntity">行为所附加的实体类型。</typeparam>
public abstract class GeneralEntityBehavior<TEntity> : EntityBehavior<TEntity> where TEntity : Entity;
