using Transoceanic.Behaviors.Collections;

namespace Transoceanic.Behaviors;

/// <summary>
/// 实体行为的抽象基类，为所有行为提供默认实现，并维护对当前关联实体实例的引用。
/// </summary>
/// <remarks>
/// 行为对象按类型单例存储，不能用实例字段或自动属性保存实体状态；应将状态存入实体的 AI 或 Global 实例。
/// 获取另一个实体的同类型行为会重新绑定当前上下文，因此不能长期缓存行为引用。
/// </remarks>
/// <typeparam name="TEntity">行为所附加的实体类型，必须派生自 <see cref="Entity"/>。</typeparam>
public abstract class EntityBehavior<TEntity> : IEntityBehavior where TEntity : Entity
{
    /// <summary>
    /// 存储当前行为所连接的实体实例。
    /// </summary>
    /// <remarks>
    /// 注意：正常情况下，此字段仅在通过 <see cref="SimpleEntityBehaviorSet{TEntity, TBehavior}.Enumerate(TEntity, string)"/> 等方法获取行为实例时被赋值。
    /// <br/>如果直接实例化行为类，则需要手动为此字段赋值，否则在行为方法中访问此字段会导致 <see cref="NullReferenceException"/> 异常。
    /// <br/>如果确需要在外部对此字段赋值，请务必谨慎。
    /// </remarks>
    public TEntity _Entity;

    /// <summary>
    /// 获取行为所属的 <see cref="global::Terraria.ModLoader.Mod"/> 实例。
    /// </summary>
    public abstract Mod Mod { get; }

    /// <summary>
    /// 获取行为的优先级。默认值为 0。数值越大，处理顺序越靠前。
    /// </summary>
    public virtual decimal Priority => 0m;

    /// <summary>
    /// 获取一个值，指示当前行为是否应该执行处理逻辑。默认返回 <see langword="true"/>。
    /// </summary>
    public virtual bool ShouldProcess => true;

    /// <summary>
    /// 显式实现 <see cref="ILoadable.Load(Mod)"/>，将调用转至虚方法 <see cref="Load(Mod)"/>。
    /// </summary>
    void ILoadable.Load(Mod mod) => Load(mod);

    /// <summary>
    /// 在 Mod 加载时调用，可重写以执行行为的初始化逻辑。
    /// </summary>
    /// <param name="mod">当前行为所属的 Mod 实例。</param>
    public virtual void Load(Mod mod) { }

    /// <summary>
    /// 显式实现 <see cref="ILoadable.Unload"/>，将调用转至虚方法 <see cref="Unload"/>。
    /// </summary>
    void ILoadable.Unload() => Unload();

    /// <summary>
    /// 在 Mod 卸载时调用，可重写以释放资源或执行清理逻辑。
    /// </summary>
    public virtual void Unload() { }

    /// <summary>
    /// 将调用转至虚方法 <see cref="PostSetupContent"/>。
    /// </summary>
    void IContentLoader.PostSetupContent() => PostSetupContent();

    /// <summary>
    /// 在所有内容加载完成后调用，可重写以执行跨内容的初始化或调整。
    /// </summary>
    public virtual void PostSetupContent() { }

    /// <summary>
    /// 将调用转至虚方法 <see cref="UnModUnload"/>。
    /// </summary>
    void IContentLoader.OnModUnload() => UnModUnload();

    /// <summary>
    /// 在当前 Mod 卸载时调用，可重写以清理与其他 Mod 相关的持久化数据。
    /// </summary>
    public virtual void UnModUnload() { }
}
