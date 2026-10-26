using Transoceanic.Behaviors.Collections;

namespace Transoceanic.Behaviors;

/// <summary>
/// 标识某个继承自 <see cref="EntityBehavior{TEntity}"/> 的类是关键类。
/// <br/>在 <see cref="SimpleEntityBehaviorSet{TEntity, TBehavior}.Initialize(IEnumerable{TBehavior})"/> 逻辑中，具有此特性的类会无条件捕获几乎所有方法。
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class CriticalBehaviorAttribute : Attribute;
