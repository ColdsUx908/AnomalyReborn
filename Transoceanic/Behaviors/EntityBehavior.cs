using Transoceanic.Behaviors.Collections;
using Transoceanic.Core.ModIntegration;

namespace Transoceanic.Behaviors;

/// <summary>
/// 实体行为的抽象基类，为所有行为提供默认实现，并维护对当前关联实体实例的引用。
/// </summary>
/// <remarks>
/// 行为对象按类型单例存储，不能用实例字段或自动属性保存实体状态；应将状态存入实体的 AI 或 Global 实例。
/// 正在分发的回调会锁定绑定引用，明确禁止同一单例跨实体调用；不能长期缓存行为引用。
/// 如确需在回调内处理其他实体，应显式构造新的具体 Behavior 实例并绑定目标实体，自行完成所需初始化。
/// </remarks>
/// <typeparam name="TEntity">行为所附加的实体类型，必须派生自 <see cref="Entity"/>。</typeparam>
public abstract class EntityBehavior<TEntity> : RegisteredContent<IEntityBehavior>, IEntityBehavior where TEntity : Entity
{
    private int _EntityLockCount;

    /// <summary>
    /// 获取或设置当前行为绑定的实体实例。
    /// 框架在分发行为时负责绑定此引用；分发期间若已由 <see cref="LockEntity"/> 锁定，则禁止改绑到另一个实体。
    /// </summary>
    /// <remarks>
    /// 正常情况下，此属性仅在通过 <see cref="SimpleEntityBehaviorSet{TEntity, TBehavior}.Enumerate(TEntity, string)"/> 等方法获取行为实例时被赋值。
    /// <br/>如果直接实例化行为类，则需要手动为此属性赋值，否则在行为方法中访问此属性会导致 <see cref="NullReferenceException"/> 异常。
    /// <br/>直接查询后调用行为时，用 LockEntity 包围调用；运行期间向本属性赋予其他引用会抛出异常。
    /// </remarks>
    public TEntity Entity
    {
        get;
        set
        {
            if (_EntityLockCount > 0 && !ReferenceEquals(field, value))
                throw new InvalidOperationException("Cross-entity calls on a singleton Behavior are forbidden. Construct a new concrete Behavior instance, initialize it, and bind the target entity instead.");
            field = value;
        }
    }

    /// <summary>
    /// 锁定当前绑定引用，直到回调结束；实体自身的数据仍可正常修改。不提供跨线程同步。
    /// </summary>
    public EntityLock LockEntity() => new(this);

    /// <summary>
    /// 禁止作用域内切换到其他实体的绑定锁；用 using 释放，不可复制，也不是线程互斥锁。
    /// </summary>
    public ref struct EntityLock
    {
        private EntityBehavior<TEntity> _Owner;

        /// <summary>
        /// 为行为枚举器持有对应实体的遍历锁。
        /// 枚举结束或中止时必须调用 <see cref="Dispose"/>，以解除当前实体引用的绑定保护。
        /// </summary>
        internal EntityLock(EntityBehavior<TEntity> owner)
        {
            _Owner = owner;
            owner._EntityLockCount++;
        }

        /// <summary>
        /// 释放本作用域持有的实体绑定锁；重复释放无效，不应复制作用域或跨线程使用。
        /// </summary>
        public void Dispose()
        {
            if (_Owner is null)
                return;
            _Owner._EntityLockCount--;
            _Owner = null;
        }
    }

    /// <summary>
    /// 获取行为的优先级。默认值为 0。数值越大，处理顺序越靠前。
    /// </summary>
    public virtual decimal Priority => 0m;

    /// <summary>
    /// 获取一个值，指示当前行为是否应该执行处理逻辑。默认返回 <see langword="true"/>。
    /// </summary>
    public virtual bool ShouldProcess => true;

    /// <inheritdoc/>
    protected override void ClearReferences()
    {
        _EntityLockCount = 0;
        Entity = null;
    }

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
