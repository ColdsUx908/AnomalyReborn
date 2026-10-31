namespace Transoceanic.Behaviors.Collections;

/// <summary>
/// 表示一个针对单一实体类型（通过 <see cref="SingleEntityBehavior{TEntity}.ApplyingType"/> 区分）的行为集合。
/// 内部根据实体类型ID维护多个 <see cref="SimpleEntityBehaviorSet{TEntity, TBehavior}"/> 实例。
/// </summary>
/// <typeparam name="TEntity">行为所附加的实体类型，必须派生自 <see cref="Entity"/>。</typeparam>
/// <typeparam name="TBehavior">行为的具体类型，必须派生自 <see cref="SingleEntityBehavior{TEntity}"/>。</typeparam>
/// <remarks>仅对调用方自行拥有的集合使用填充和清理入口；框架登记集合通过 BehaviorQueries 查询。
/// 集合与共享行为绑定不提供线程安全保证，应在游戏相应主线程阶段使用。</remarks>
public class SingleEntityBehaviorSet<TEntity, TBehavior>
    where TEntity : Entity
    where TBehavior : SingleEntityBehavior<TEntity>
{
    /// <summary>
    /// 内部存储结构，以实体类型ID（<see cref="SingleEntityBehavior{TEntity}.ApplyingType"/>）为键，
    /// 值为对应实体类型的行为集合。
    /// </summary>
    private readonly Dictionary<int, SimpleEntityBehaviorSet<TEntity, TBehavior>> _Data = [];

    internal IEnumerable<SimpleEntityBehaviorSet<TEntity, TBehavior>> Sets => _Data.Values;

    /// <summary>
    /// 尝试获取指定实体和方法的第一个满足处理条件的行为实例。
    /// 查找会依据 <see cref="EntityBehavior{TEntity}.Priority"/> 降序进行。
    /// </summary>
    /// <param name="entity">要检查的实体实例，用于确定实体类型并设置行为的 <c>Entity</c> 属性。</param>
    /// <param name="methodName">要查找的方法名称。</param>
    /// <param name="behavior">当此方法返回 <see langword="true"/> 时，包含找到的行为实例；否则为 <see langword="null"/>。</param>
    /// <returns>如果找到满足条件的行为，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public bool TryGetBehavior(TEntity entity, string methodName, [NotNullWhen(true)] out TBehavior behavior) =>
        (behavior = entity is not null && _Data.TryGetValue(entity.EntityType, out SimpleEntityBehaviorSet<TEntity, TBehavior> set) ? set.GetFirstOrDefault(entity, methodName) : null) is not null;

    /// <summary>
    /// 通过反射自动发现并填充所有派生自 <typeparamref name="TBehavior"/> 的类型实例。
    /// </summary>
    public void FillSet() => Initialize(TOLoaderHandler.GetLoaderInstances<TBehavior>());

    /// <summary>
    /// 填充集合，仅包含其所属 Mod 类型为 <typeparamref name="T"/> 的行为实例。
    /// </summary>
    /// <typeparam name="T">用于筛选的 Mod 类型。</typeparam>
    public void FillSet<T>() where T : Mod => Initialize(TOLoaderHandler.GetLoaderInstances<TBehavior>().Where(b => b.Mod is T));

    /// <summary>
    /// 从指定程序集中通过反射发现并填充所有派生自 <typeparamref name="TBehavior"/> 的类型实例。
    /// </summary>
    /// <param name="assemblyToSearch">要搜索的程序集。</param>
    public void FillSet(Assembly assemblyToSearch) => Initialize(TOLoaderHandler.GetLoaderInstances<TBehavior>(assemblyToSearch));

    /// <summary>
    /// 内部初始化方法，将行为按其所应用的实体类型（<see cref="SingleEntityBehavior{TEntity}.ApplyingType"/>）分组，
    /// 并为每种类型构建对应的 <see cref="SimpleEntityBehaviorSet{TEntity, TBehavior}"/>。
    /// </summary>
    /// <param name="behaviors">要初始化的行为实例集合。</param>
    internal void Initialize(IEnumerable<TBehavior> behaviors)
    {
        foreach (IGrouping<int, TBehavior> group in behaviors.GroupBy(b => b.ApplyingType))
        {
            _Data[group.Key] = new();
            _Data[group.Key].Initialize(group);
        }
    }

    /// <summary>
    /// 清除所有实体类型的行为集合。
    /// </summary>
    public void Clear()
    {
        foreach (SimpleEntityBehaviorSet<TEntity, TBehavior> set in _Data.Values)
            set.Clear();
        _Data.Clear();
    }
}
