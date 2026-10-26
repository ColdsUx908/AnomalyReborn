namespace Transoceanic.Runtime.Detours;

//该类不使用源生成器
public abstract class GlobalTypeDetour<TEntity, TGlobal, TGlobalType> : ModTypeDetour<TGlobalType>
    where TEntity : IEntityWithGlobals<TGlobal>
    where TGlobal : GlobalType<TEntity, TGlobal>
    where TGlobalType : TGlobal
{
    // get_IsCloneable
    public delegate bool Orig_get_IsCloneable(TGlobalType self);
    /// <inheritdoc cref="GlobalType{TEntity, TGlobal}.IsCloneable"/>
    public virtual bool Detour_get_IsCloneable(Orig_get_IsCloneable orig, TGlobalType self) => orig(self);

    // get_CloneNewInstances
    public delegate bool Orig_get_CloneNewInstances(TGlobalType self);
    /// <inheritdoc cref="GlobalType{TEntity, TGlobal}.CloneNewInstances"/>
    public virtual bool Detour_get_CloneNewInstances(Orig_get_CloneNewInstances orig, TGlobalType self) => orig(self);

    // AppliesToEntity
    public delegate bool Orig_AppliesToEntity(TGlobalType self, TEntity entity, bool lateInstantiation);
    /// <inheritdoc cref="GlobalType{TEntity, TGlobal}.AppliesToEntity"/>
    public virtual bool Detour_AppliesToEntity(Orig_AppliesToEntity orig, TGlobalType self, TEntity entity, bool lateInstantiation) => orig(self, entity, lateInstantiation);

    // SetDefaults
    public delegate void Orig_SetDefaults(TGlobalType self, TEntity entity);
    /// <inheritdoc cref="GlobalType{TEntity, TGlobal}.SetDefaults"/>
    public virtual void Detour_SetDefaults(Orig_SetDefaults orig, TGlobalType self, TEntity entity) => orig(self, entity);

#nullable enable
    // Clone
    public delegate TGlobal Orig_Clone(TGlobalType self, TEntity? from, TEntity to);
    /// <inheritdoc cref="GlobalType{TEntity, TGlobal}.Clone"/>
    public virtual TGlobal Detour_Clone(Orig_Clone orig, TGlobalType self, TEntity? from, TEntity to) => orig(self, from, to);

    // NewInstance
    public delegate TGlobal? Orig_NewInstance(TGlobalType self, TEntity target);
    /// <inheritdoc cref="GlobalType{TEntity, TGlobal}.NewInstance"/>
    public virtual TGlobal? Detour_NewInstance(Orig_NewInstance orig, TGlobalType self, TEntity target) => orig(self, target);
#nullable restore

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
