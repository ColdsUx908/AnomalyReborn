namespace Transoceanic.Runtime.Detours;

//该类不使用源生成器
/// <summary>
/// 为 <see cref="GlobalType{TEntity, TGlobal}"/> 的实例化、克隆和初始化回调提供通用 Detour 扩展点。
/// 派生类可覆盖对应的 <c>Detour_</c> 方法；默认实现会继续调用原始回调。
/// </summary>
public abstract class GlobalTypeDetour<TEntity, TGlobal, TGlobalType> : ModTypeDetour<TGlobalType>
    where TEntity : IEntityWithGlobals<TGlobal>
    where TGlobal : GlobalType<TEntity, TGlobal>
    where TGlobalType : TGlobal
{
    // get_IsCloneable
    /// <summary>
    /// 保存原始 <see cref="GlobalType{TEntity, TGlobal}.IsCloneable"/> getter 的调用入口。
    /// 对应 Detour 可调用此委托继续执行原始克隆能力判定。
    /// </summary>
    public delegate bool Orig_get_IsCloneable(TGlobalType self);
    /// <inheritdoc cref="GlobalType{TEntity, TGlobal}.IsCloneable"/>
    public virtual bool Detour_get_IsCloneable(Orig_get_IsCloneable orig, TGlobalType self) => orig(self);

    // get_CloneNewInstances
    /// <summary>
    /// 保存原始 <see cref="GlobalType{TEntity, TGlobal}.CloneNewInstances"/> getter 的调用入口。
    /// 对应 Detour 可调用此委托继续执行原始新实例克隆策略。
    /// </summary>
    public delegate bool Orig_get_CloneNewInstances(TGlobalType self);
    /// <inheritdoc cref="GlobalType{TEntity, TGlobal}.CloneNewInstances"/>
    public virtual bool Detour_get_CloneNewInstances(Orig_get_CloneNewInstances orig, TGlobalType self) => orig(self);

    // AppliesToEntity
    /// <summary>
    /// 保存原始 <see cref="GlobalType{TEntity, TGlobal}.AppliesToEntity"/> 回调的调用入口。
    /// 委托接收目标实体及延迟实例化标记，并返回该全局类型是否适用于实体。
    /// </summary>
    public delegate bool Orig_AppliesToEntity(TGlobalType self, TEntity entity, bool lateInstantiation);
    /// <inheritdoc cref="GlobalType{TEntity, TGlobal}.AppliesToEntity"/>
    public virtual bool Detour_AppliesToEntity(Orig_AppliesToEntity orig, TGlobalType self, TEntity entity, bool lateInstantiation) => orig(self, entity, lateInstantiation);

    // SetDefaults
    /// <summary>
    /// 保存原始 <see cref="GlobalType{TEntity, TGlobal}.SetDefaults"/> 回调的调用入口。
    /// 委托接收待初始化的全局实例和所属实体。
    /// </summary>
    public delegate void Orig_SetDefaults(TGlobalType self, TEntity entity);
    /// <inheritdoc cref="GlobalType{TEntity, TGlobal}.SetDefaults"/>
    public virtual void Detour_SetDefaults(Orig_SetDefaults orig, TGlobalType self, TEntity entity) => orig(self, entity);

#nullable enable
    // Clone
    /// <summary>
    /// 保存原始 <see cref="GlobalType{TEntity, TGlobal}.Clone"/> 回调的调用入口。
    /// 委托接收来源实体（可能为空）及目标实体，并返回克隆后的全局实例。
    /// </summary>
    public delegate TGlobal Orig_Clone(TGlobalType self, TEntity? from, TEntity to);
    /// <inheritdoc cref="GlobalType{TEntity, TGlobal}.Clone"/>
    public virtual TGlobal Detour_Clone(Orig_Clone orig, TGlobalType self, TEntity? from, TEntity to) => orig(self, from, to);

    // NewInstance
    /// <summary>
    /// 保存原始 <see cref="GlobalType{TEntity, TGlobal}.NewInstance"/> 回调的调用入口。
    /// 委托按目标实体创建实例；原实现允许返回空值表示不创建全局实例。
    /// </summary>
    public delegate TGlobal? Orig_NewInstance(TGlobalType self, TEntity target);
    /// <inheritdoc cref="GlobalType{TEntity, TGlobal}.NewInstance"/>
    public virtual TGlobal? Detour_NewInstance(Orig_NewInstance orig, TGlobalType self, TEntity target) => orig(self, target);
#nullable restore

    /// <inheritdoc/>
    /// <remarks>
    /// 先应用基类 Detour，再注册本类型覆盖的克隆、适用性、默认值和实例创建回调。
    /// 应用顺序与各个原始委托的签名对应。
    /// </remarks>
    public override void ApplyDetour()
    {
        base.ApplyDetour();
        ApplySingleDetour(Detour_get_IsCloneable);
        ApplySingleDetour(Detour_get_CloneNewInstances);
        ApplySingleDetour(Detour_AppliesToEntity);
        ApplySingleDetour(Detour_SetDefaults);
        ApplySingleDetour(Detour_Clone);
        ApplySingleDetour(Detour_NewInstance);
    }
}
