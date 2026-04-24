using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI;
using Terraria.GameInput;

namespace Transoceanic.Framework.Abstractions;

#region Base
/// <summary>
/// 标识某个继承自 <see cref="EntityBehavior{TEntity}"/> 的类是关键类。
/// <br/>在 <see cref="SimpleEntityBehaviorSet{TEntity, TBehavior}.Initialize(IEnumerable{TBehavior})"/> 逻辑中，具有此特性的类会无条件捕获几乎所有方法。
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class CriticalBehaviorAttribute : Attribute;

/// <summary>
/// 定义实体行为必须实现的核心功能，包括所属 Mod、优先级、处理条件以及生命周期管理。
/// </summary>
public interface IEntityBehavior : ILoadable, IContentLoader
{
    /// <summary>
    /// 获取行为所属的 Mod 实例。
    /// </summary>
    public abstract Mod Mod { get; }

    /// <summary>
    /// 优先级，越大越先应用。
    /// <br/>设计规范：对于需优先应用的行为，建议设置为正值；对于需最后应用的行为，建议设置为负值。
    /// </summary>
    public abstract decimal Priority { get; }

    /// <summary>
    /// 获取一个值，指示当前行为是否应该执行处理逻辑。
    /// <br/>当通过 <see cref="SimpleEntityBehaviorSet{TEntity, TBehavior}"/> 遍历行为时，仅返回此属性为 <see langword="true"/> 的实例。
    /// </summary>
    public abstract bool ShouldProcess { get; }

    /// <summary>
    /// 在 Mod 加载的静态初始化阶段调用，用于设置静态默认值。
    /// <br/>等效于 <see cref="ModType.SetStaticDefaults"/>。
    /// </summary>
    public abstract void SetStaticDefaults();
}

/// <summary>
/// 实体行为的抽象基类，为所有行为提供默认实现，并维护对当前关联实体实例的引用。
/// </summary>
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
    public TEntity _entity;

    /// <summary>
    /// 获取行为所属的 Mod 实例。
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

    /// <inheritdoc cref="IEntityBehavior.SetStaticDefaults"/>
    public virtual void SetStaticDefaults() { }

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
    /// 显式实现 <see cref="IContentLoader.PostSetupContent"/>，将调用转至虚方法 <see cref="PostSetupContent"/>。
    /// </summary>
    void IContentLoader.PostSetupContent() => PostSetupContent();

    /// <summary>
    /// 在所有内容加载完成后调用，可重写以执行跨内容的初始化或调整。
    /// </summary>
    public virtual void PostSetupContent() { }

    /// <summary>
    /// 显式实现 <see cref="IContentLoader.OnModUnload"/>，将调用转至虚方法 <see cref="UnModUnload"/>。
    /// </summary>
    void IContentLoader.OnModUnload() => UnModUnload();

    /// <summary>
    /// 在当前 Mod 卸载时调用，可重写以清理与其他 Mod 相关的持久化数据。
    /// </summary>
    public virtual void UnModUnload() { }
}

/// <summary>
/// 通用实体行为的基类，不附加特定于全局或单一实体的功能。
/// 用于标识可应用于任意实体的普通行为类型。
/// </summary>
/// <typeparam name="TEntity">行为所附加的实体类型。</typeparam>
public abstract class GeneralEntityBehavior<TEntity> : EntityBehavior<TEntity> where TEntity : Entity;

/// <summary>
/// 全局实体行为的基类，为所有使用该行为的实体提供统一的 <see cref="SetDefaults(TEntity)"/> 默认值设置点。
/// 通常与全局行为集合 <see cref="GlobalEntityBehaviorSet{TEntity, TBehavior}"/> 配合使用。
/// </summary>
/// <typeparam name="TEntity">行为所附加的实体类型。</typeparam>
public abstract class GlobalEntityBehavior<TEntity> : GeneralEntityBehavior<TEntity> where TEntity : Entity
{
    /// <summary>
    /// 用于设置实体的默认值。当实体初次创建或重置时，该行为会调用此方法。
    /// </summary>
    /// <param name="entity">要设置默认值的实体实例。</param>
    /// <remarks>等效于 <see cref="GlobalType{TEntity, TGlobal}.SetDefaults(TEntity)"/> 的行为。</remarks>
    public virtual void SetDefaults(TEntity entity) { }
}

/// <summary>
/// 单一实体行为的基类，通过 <see cref="ApplyingType"/> 指定该行为所作用的特定实体类型。
/// 在 <see cref="SingleEntityBehaviorSet{TEntity, TBehavior}"/> 中按实体类型进行分组和检索。
/// </summary>
/// <typeparam name="TEntity">行为所附加的实体类型。</typeparam>
public abstract class SingleEntityBehavior<TEntity> : EntityBehavior<TEntity> where TEntity : Entity
{
    /// <summary>
    /// 获取此行为所应用的实体类型标识（通常为实体类型的唯一 ID）。
    /// </summary>
    public abstract int ApplyingType { get; }

    /// <summary>
    /// 为此行为设定的实体设置默认值。该方法仅在实体匹配 <see cref="ApplyingType"/> 时被调用。
    /// </summary>
    /// <remarks>等效于 <see cref="GlobalType{TEntity, TGlobal}.SetDefaults(TEntity)"/> 的作用，但只针对特定实体类型。</remarks>
    public virtual void SetDefaults() { }
}
#endregion Base

#region General Behavior
public abstract class PlayerBehavior : GeneralEntityBehavior<Player>
{
    public Player Player => _entity;

    public TOPlayer OceanPlayer { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _entity.Ocean; }

    #region 虚成员
    /// <inheritdoc cref="ModPlayer.Initialize"/>
    public virtual void Initialize() { }

    /// <inheritdoc cref="ModPlayer.ResetEffects"/>
    public virtual void ResetEffects() { }

    /// <inheritdoc cref="ModPlayer.ResetInfoAccessories"/>
    public virtual void ResetInfoAccessories() { }

    /// <inheritdoc cref="ModPlayer.RefreshInfoAccessoriesFromTeamPlayers"/>
    public virtual void RefreshInfoAccessoriesFromTeamPlayers(Player otherPlayer) { }

    /// <inheritdoc cref="ModPlayer.ModifyMaxStats"/>
    public virtual void ModifyMaxStats(out StatModifier health, out StatModifier mana)
    {
        health = StatModifier.Default;
        mana = StatModifier.Default;
    }

    /// <inheritdoc cref="ModPlayer.UpdateDead"/>
    public virtual void UpdateDead() { }

    /// <inheritdoc cref="ModPlayer.PreSaveCustomData"/>
    public virtual void PreSaveCustomData() { }

    /// <inheritdoc cref="ModPlayer.SaveData"/>
    public virtual void SaveData(TagCompound tag) { }

    /// <inheritdoc cref="ModPlayer.LoadData"/>
    public virtual void LoadData(TagCompound tag) { }

    /// <inheritdoc cref="ModPlayer.PreSavePlayer"/>
    public virtual void PreSavePlayer() { }

    /// <inheritdoc cref="ModPlayer.PostSavePlayer"/>
    public virtual void PostSavePlayer() { }

    /// <inheritdoc cref="ModPlayer.CopyClientState"/>
    public virtual void CopyClientState(ModPlayer targetCopy) { }

    /// <inheritdoc cref="ModPlayer.SyncPlayer"/>
    public virtual void SyncPlayer(int toWho, int fromWho, bool newPlayer) { }

    /// <inheritdoc cref="ModPlayer.SendClientChanges"/>
    public virtual void SendClientChanges(ModPlayer clientPlayer) { }

    /// <inheritdoc cref="ModPlayer.UpdateBadLifeRegen"/>
    public virtual void UpdateBadLifeRegen() { }

    /// <inheritdoc cref="ModPlayer.UpdateLifeRegen"/>
    public virtual void UpdateLifeRegen() { }

    /// <inheritdoc cref="ModPlayer.NaturalLifeRegen"/>
    public virtual void NaturalLifeRegen(ref float regen) { }

    /// <inheritdoc cref="ModPlayer.UpdateAutopause"/>
    public virtual void UpdateAutopause() { }

    /// <inheritdoc cref="ModPlayer.PreUpdate"/>
    public virtual void PreUpdate() { }

    /// <inheritdoc cref="ModPlayer.ProcessTriggers"/>
    public virtual void ProcessTriggers(TriggersSet triggersSet) { }

    /// <inheritdoc cref="ModPlayer.ArmorSetBonusActivated"/>
    public virtual void ArmorSetBonusActivated() { }

    /// <inheritdoc cref="ModPlayer.ArmorSetBonusHeld"/>
    public virtual void ArmorSetBonusHeld(int holdTime) { }

    /// <inheritdoc cref="ModPlayer.SetControls"/>
    public virtual void SetControls() { }

    /// <inheritdoc cref="ModPlayer.PreUpdateBuffs"/>
    public virtual void PreUpdateBuffs() { }

    /// <inheritdoc cref="ModPlayer.PostUpdateBuffs"/>
    public virtual void PostUpdateBuffs() { }

    /// <inheritdoc cref="ModPlayer.UpdateEquips"/>
    public virtual void UpdateEquips() { }

    /// <inheritdoc cref="ModPlayer.PostUpdateEquips"/>
    public virtual void PostUpdateEquips() { }

    /// <inheritdoc cref="ModPlayer.UpdateVisibleAccessories"/>
    public virtual void UpdateVisibleAccessories() { }

    /// <inheritdoc cref="ModPlayer.UpdateVisibleVanityAccessories"/>
    public virtual void UpdateVisibleVanityAccessories() { }

    /// <inheritdoc cref="ModPlayer.UpdateDyes"/>
    public virtual void UpdateDyes() { }

    /// <inheritdoc cref="ModPlayer.PostUpdateMiscEffects"/>
    public virtual void PostUpdateMiscEffects() { }

    /// <inheritdoc cref="ModPlayer.PostUpdateRunSpeeds"/>
    public virtual void PostUpdateRunSpeeds() { }

    /// <inheritdoc cref="ModPlayer.PreUpdateMovement"/>
    public virtual void PreUpdateMovement() { }

    /// <inheritdoc cref="ModPlayer.PostUpdate"/>
    public virtual void PostUpdate() { }

    /// <inheritdoc cref="ModPlayer.ModifyExtraJumpDurationMultiplier"/>
    public virtual void ModifyExtraJumpDurationMultiplier(ExtraJump jump, ref float duration) { }

    /// <inheritdoc cref="ModPlayer.CanStartExtraJump"/>
    public virtual bool CanStartExtraJump(ExtraJump jump) => true;

    /// <inheritdoc cref="ModPlayer.OnExtraJumpStarted"/>
    public virtual void OnExtraJumpStarted(ExtraJump jump, ref bool playSound) { }

    /// <inheritdoc cref="ModPlayer.OnExtraJumpEnded"/>
    public virtual void OnExtraJumpEnded(ExtraJump jump) { }

    /// <inheritdoc cref="ModPlayer.OnExtraJumpRefreshed"/>
    public virtual void OnExtraJumpRefreshed(ExtraJump jump) { }

    /// <inheritdoc cref="ModPlayer.ExtraJumpVisuals"/>
    public virtual void ExtraJumpVisuals(ExtraJump jump) { }

    /// <inheritdoc cref="ModPlayer.CanShowExtraJumpVisuals"/>
    public virtual bool CanShowExtraJumpVisuals(ExtraJump jump) => true;

    /// <inheritdoc cref="ModPlayer.OnExtraJumpCleared"/>
    public virtual void OnExtraJumpCleared(ExtraJump jump) { }

    /// <inheritdoc cref="ModPlayer.FrameEffects"/>
    public virtual void FrameEffects() { }

    /// <inheritdoc cref="ModPlayer.ImmuneTo"/>
    public virtual bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable) => false;

    /// <inheritdoc cref="ModPlayer.FreeDodge"/>
    public virtual bool FreeDodge(Player.HurtInfo info) => false;

    /// <inheritdoc cref="ModPlayer.ConsumableDodge"/>
    public virtual bool ConsumableDodge(Player.HurtInfo info) => false;

    /// <inheritdoc cref="ModPlayer.ModifyHurt"/>
    public virtual void ModifyHurt(ref Player.HurtModifiers modifiers) { }

    /// <inheritdoc cref="ModPlayer.OnHurt"/>
    public virtual void OnHurt(Player.HurtInfo info) { }

    /// <inheritdoc cref="ModPlayer.PostHurt"/>
    public virtual void PostHurt(Player.HurtInfo info) { }

    /// <inheritdoc cref="ModPlayer.PreKill"/>
    public virtual bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource) => true;

    /// <inheritdoc cref="ModPlayer.Kill"/>
    public virtual void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) { }

    /// <inheritdoc cref="ModPlayer.PreModifyLuck"/>
    public virtual bool PreModifyLuck(ref float luck) => true;

    /// <inheritdoc cref="ModPlayer.ModifyLuck"/>
    public virtual void ModifyLuck(ref float luck) { }

    /// <inheritdoc cref="ModPlayer.PreItemCheck"/>
    public virtual bool PreItemCheck() => true;

    /// <inheritdoc cref="ModPlayer.PostItemCheck"/>
    public virtual void PostItemCheck() { }

    /// <inheritdoc cref="ModPlayer.UseTimeMultiplier"/>
    public virtual float UseTimeMultiplier(Item item) => 1f;

    /// <inheritdoc cref="ModPlayer.UseAnimationMultiplier"/>
    public virtual float UseAnimationMultiplier(Item item) => 1f;

    /// <inheritdoc cref="ModPlayer.UseSpeedMultiplier"/>
    public virtual float UseSpeedMultiplier(Item item) => 1f;

    /// <inheritdoc cref="ModPlayer.GetHealLife"/>
    public virtual void GetHealLife(Item item, bool quickHeal, ref int healValue) { }

    /// <inheritdoc cref="ModPlayer.GetHealMana"/>
    public virtual void GetHealMana(Item item, bool quickHeal, ref int healValue) { }

    /// <inheritdoc cref="ModPlayer.ModifyManaCost"/>
    public virtual void ModifyManaCost(Item item, ref float reduce, ref float mult) { }

    /// <inheritdoc cref="ModPlayer.OnMissingMana"/>
    public virtual void OnMissingMana(Item item, int neededMana) { }

    /// <inheritdoc cref="ModPlayer.OnConsumeMana"/>
    public virtual void OnConsumeMana(Item item, int manaConsumed) { }

    /// <inheritdoc cref="ModPlayer.ModifyWeaponDamage"/>
    public virtual void ModifyWeaponDamage(Item item, ref StatModifier damage) { }

    /// <inheritdoc cref="ModPlayer.ModifyWeaponKnockback"/>
    public virtual void ModifyWeaponKnockback(Item item, ref StatModifier knockback) { }

    /// <inheritdoc cref="ModPlayer.ModifyWeaponCrit"/>
    public virtual void ModifyWeaponCrit(Item item, ref float crit) { }

    /// <inheritdoc cref="ModPlayer.CanConsumeAmmo"/>
    public virtual bool CanConsumeAmmo(Item weapon, Item ammo) => true;

    /// <inheritdoc cref="ModPlayer.OnConsumeAmmo"/>
    public virtual void OnConsumeAmmo(Item weapon, Item ammo) { }

    /// <inheritdoc cref="ModPlayer.CanShoot"/>
    public virtual bool CanShoot(Item item) => true;

    /// <inheritdoc cref="ModPlayer.ModifyShootStats"/>
    public virtual void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) { }

    /// <inheritdoc cref="ModPlayer.Shoot"/>
    public virtual bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => true;

    /// <inheritdoc cref="ModPlayer.MeleeEffects"/>
    public virtual void MeleeEffects(Item item, Rectangle hitbox) { }

    /// <inheritdoc cref="ModPlayer.EmitEnchantmentVisualsAt"/>
    public virtual void EmitEnchantmentVisualsAt(Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight) { }

    /// <inheritdoc cref="ModPlayer.CanCatchNPC"/>
    public virtual bool? CanCatchNPC(NPC target, Item item) => null;

    /// <inheritdoc cref="ModPlayer.OnCatchNPC"/>
    public virtual void OnCatchNPC(NPC npc, Item item, bool failed) { }

    /// <inheritdoc cref="ModPlayer.ModifyItemScale"/>
    public virtual void ModifyItemScale(Item item, ref float scale) { }

    /// <inheritdoc cref="ModPlayer.OnHitAnything"/>
    public virtual void OnHitAnything(float x, float y, Entity victim) { }

    /// <inheritdoc cref="ModPlayer.CanHitNPC"/>
    [Obsolete("Poor performance.", true)]
    public virtual bool CanHitNPC(NPC target) => true;

    /// <inheritdoc cref="ModPlayer.CanMeleeAttackCollideWithNPC"/>
    [Obsolete("Poor performance.", true)]
    public virtual bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, NPC target) => null;

    /// <inheritdoc cref="ModPlayer.ModifyHitNPC"/>
    public virtual void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="ModPlayer.OnHitNPC"/>
    public virtual void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) { }

    /// <inheritdoc cref="ModPlayer.CanHitNPCWithItem"/>
    [Obsolete("Poor performance.", true)]
    public virtual bool? CanHitNPCWithItem(Item item, NPC target) => null;

    /// <inheritdoc cref="ModPlayer.ModifyHitNPCWithItem"/>
    public virtual void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="ModPlayer.OnHitNPCWithItem"/>
    public virtual void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) { }

    /// <inheritdoc cref="ModPlayer.CanHitNPCWithProj"/>
    [Obsolete("Poor performance.", true)]
    public virtual bool? CanHitNPCWithProj(Projectile proj, NPC target) => null;

    /// <inheritdoc cref="ModPlayer.ModifyHitNPCWithProj"/>
    public virtual void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="ModPlayer.OnHitNPCWithProj"/>
    public virtual void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) { }

    /// <inheritdoc cref="ModPlayer.CanHitPvp"/>
    [Obsolete("Poor performance.", true)]
    public virtual bool CanHitPvp(Item item, Player target) => true;

    /// <inheritdoc cref="ModPlayer.CanHitPvpWithProj"/>
    [Obsolete("Poor performance.", true)]
    public virtual bool CanHitPvpWithProj(Projectile proj, Player target) => true;

    /// <inheritdoc cref="ModPlayer.CanBeHitByNPC"/>
    [Obsolete("Poor performance.", true)]
    public virtual bool CanBeHitByNPC(NPC npc, ref int cooldownSlot) => true;

    /// <inheritdoc cref="ModPlayer.ModifyHitByNPC"/>
    public virtual void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers) { }

    /// <inheritdoc cref="ModPlayer.OnHitByNPC"/>
    public virtual void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) { }

    /// <inheritdoc cref="ModPlayer.CanBeHitByProjectile"/>
    [Obsolete("Poor performance.", true)]
    public virtual bool CanBeHitByProjectile(Projectile proj) => true;

    /// <inheritdoc cref="ModPlayer.ModifyHitByProjectile"/>
    public virtual void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers) { }

    /// <inheritdoc cref="ModPlayer.OnHitByProjectile"/>
    public virtual void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo) { }

    /// <inheritdoc cref="ModPlayer.ModifyFishingAttempt"/>
    public virtual void ModifyFishingAttempt(ref FishingAttempt attempt) { }

    /// <inheritdoc cref="ModPlayer.CatchFish"/>
    public virtual void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) { }

    /// <inheritdoc cref="ModPlayer.ModifyCaughtFish"/>
    public virtual void ModifyCaughtFish(Item fish) { }

    /// <inheritdoc cref="ModPlayer.CanConsumeBait"/>
    public virtual bool? CanConsumeBait(Item bait) => null;

    /// <inheritdoc cref="ModPlayer.GetFishingLevel"/>
    public virtual void GetFishingLevel(Item fishingRod, Item bait, ref float fishingLevel) { }

    /// <inheritdoc cref="ModPlayer.AnglerQuestReward"/>
    public virtual void AnglerQuestReward(float rareMultiplier, List<Item> rewardItems) { }

    /// <inheritdoc cref="ModPlayer.GetDyeTraderReward"/>
    public virtual void GetDyeTraderReward(List<int> rewardPool) { }

    /// <inheritdoc cref="ModPlayer.DrawEffects"/>
    public virtual void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) { }

    /// <inheritdoc cref="ModPlayer.ModifyDrawInfo"/>
    public virtual void ModifyDrawInfo(ref PlayerDrawSet drawInfo) { }

    /// <inheritdoc cref="ModPlayer.ModifyDrawLayerOrdering"/>
    public virtual void ModifyDrawLayerOrdering(IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position> positions) { }

    /// <inheritdoc cref="ModPlayer.HideDrawLayers"/>
    public virtual void HideDrawLayers(PlayerDrawSet drawInfo) { }

    /// <inheritdoc cref="ModPlayer.ModifyScreenPosition"/>
    public virtual void ModifyScreenPosition() { }

    /// <inheritdoc cref="ModPlayer.ModifyZoom"/>
    public virtual void ModifyZoom(ref float zoom) { }

    /// <inheritdoc cref="ModPlayer.PlayerConnect"/>
    public virtual void PlayerConnect() { }

    /// <inheritdoc cref="ModPlayer.PlayerDisconnect"/>
    public virtual void PlayerDisconnect() { }

    /// <inheritdoc cref="ModPlayer.OnEnterWorld"/>
    public virtual void OnEnterWorld() { }

    /// <inheritdoc cref="ModPlayer.OnRespawn"/>
    public virtual void OnRespawn() { }

    /// <inheritdoc cref="ModPlayer.ShiftClickSlot"/>
    public virtual bool ShiftClickSlot(Item[] inventory, int context, int slot) => false;

    /// <inheritdoc cref="ModPlayer.HoverSlot"/>
    public virtual bool HoverSlot(Item[] inventory, int context, int slot) => false;

    /// <inheritdoc cref="ModPlayer.PostSellItem"/>
    public virtual void PostSellItem(NPC vendor, Item[] shopInventory, Item item) { }

    /// <inheritdoc cref="ModPlayer.CanSellItem"/>
    public virtual bool CanSellItem(NPC vendor, Item[] shopInventory, Item item) => true;

    /// <inheritdoc cref="ModPlayer.PostBuyItem"/>
    public virtual void PostBuyItem(NPC vendor, Item[] shopInventory, Item item) { }

    /// <inheritdoc cref="ModPlayer.CanBuyItem"/>
    public virtual bool CanBuyItem(NPC vendor, Item[] shopInventory, Item item) => true;

    /// <inheritdoc cref="ModPlayer.CanUseItem"/>
    public virtual bool CanUseItem(Item item) => true;

    /// <inheritdoc cref="ModPlayer.CanAutoReuseItem"/>
    public virtual bool? CanAutoReuseItem(Item item) => null;

    /// <inheritdoc cref="ModPlayer.ModifyNurseHeal"/>
    public virtual bool ModifyNurseHeal(NPC nurse, ref int health, ref bool removeDebuffs, ref string chatText) => true;

    /// <inheritdoc cref="ModPlayer.ModifyNursePrice"/>
    public virtual void ModifyNursePrice(NPC nurse, int health, bool removeDebuffs, ref int price) { }

    /// <inheritdoc cref="ModPlayer.PostNurseHeal"/>
    public virtual void PostNurseHeal(NPC nurse, int health, bool removeDebuffs, int price) { }

    /// <inheritdoc cref="ModPlayer.AddStartingItems"/>
    public virtual IEnumerable<Item> AddStartingItems(bool mediumCoreDeath) => [];

    /// <inheritdoc cref="ModPlayer.ModifyStartingInventory"/>
    public virtual void ModifyStartingInventory(IReadOnlyDictionary<string, List<Item>> itemsByMod, bool mediumCoreDeath) { }

    /// <inheritdoc cref="ModPlayer.AddMaterialsForCrafting"/>
    public virtual IEnumerable<Item> AddMaterialsForCrafting(out ModPlayer.ItemConsumedCallback itemConsumedCallback)
    {
        itemConsumedCallback = null;
        return null;
    }

    /// <inheritdoc cref="ModPlayer.OnPickup"/>
    public virtual bool OnPickup(Item item) => true;

    /// <inheritdoc cref="ModPlayer.CanBeTeleportedTo"/>
    public virtual bool CanBeTeleportedTo(Vector2 teleportPosition, string context) => true;

    /// <inheritdoc cref="ModPlayer.OnEquipmentLoadoutSwitched"/>
    public virtual void OnEquipmentLoadoutSwitched(int oldLoadoutIndex, int loadoutIndex) { }
    #endregion 虚成员
}

public abstract class GlobalNPCBehavior : GlobalEntityBehavior<NPC>
{
    /// <inheritdoc cref="GlobalNPC.SetDefaultsFromNetId"/>
    public virtual void SetDefaultsFromNetId(NPC npc) { }

    /// <inheritdoc cref="GlobalNPC.OnSpawn"/>
    public virtual void OnSpawn(NPC npc, IEntitySource source) { }

    /// <inheritdoc cref="GlobalNPC.ApplyDifficultyAndPlayerScaling"/>
    public virtual void ApplyDifficultyAndPlayerScaling(NPC npc, int numPlayers, float balance, float bossAdjustment) { }

    /// <inheritdoc cref="GlobalNPC.SetBestiary"/>
    public virtual void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry) { }

    /// <inheritdoc cref="GlobalNPC.ModifyTypeName"/>
    public virtual void ModifyTypeName(NPC npc, ref string typeName) { }

    /// <inheritdoc cref="GlobalNPC.ModifyHoverBoundingBox"/>
    public virtual void ModifyHoverBoundingBox(NPC npc, ref Rectangle boundingBox) { }

    /// <inheritdoc cref="GlobalNPC.PreHoverInteract(NPC, bool)"/>
    public virtual bool PreHoverInteract(NPC npc, bool mouseInteracts) => true;

    /// <inheritdoc cref="GlobalNPC.ModifyTownNPCProfile"/>
    public virtual ITownNPCProfile ModifyTownNPCProfile(NPC npc) => null;

    /// <inheritdoc cref="GlobalNPC.ModifyNPCNameList"/>
    public virtual void ModifyNPCNameList(NPC npc, List<string> nameList) { }

    /// <inheritdoc cref="GlobalNPC.ResetEffects"/>
    public virtual void ResetEffects(NPC npc) { }

    /// <inheritdoc cref="GlobalNPC.PreAI"/>
    public virtual bool PreAI(NPC npc) => true;

    /// <inheritdoc cref="GlobalNPC.AI"/>
    public virtual void AI(NPC npc) { }

    /// <inheritdoc cref="GlobalNPC.PostAI"/>
    public virtual void PostAI(NPC npc) { }

    /// <inheritdoc cref="GlobalNPC.SendExtraAI"/>
    public virtual void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter) { }

    /// <inheritdoc cref="GlobalNPC.ReceiveExtraAI"/>
    public virtual void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader) { }

    /// <inheritdoc cref="GlobalNPC.FindFrame"/>
    public virtual void FindFrame(NPC npc, int frameHeight) { }

    /// <inheritdoc cref="GlobalNPC.HitEffect"/>
    public virtual void HitEffect(NPC npc, NPC.HitInfo hit) { }

    /// <inheritdoc cref="GlobalNPC.UpdateLifeRegen"/>
    public virtual void UpdateLifeRegen(NPC npc, ref int damage) { }

    /// <inheritdoc cref="GlobalNPC.CheckActive"/>
    public virtual bool CheckActive(NPC npc) => true;

    /// <inheritdoc cref="GlobalNPC.CheckDead"/>
    public virtual bool CheckDead(NPC npc) => true;

    /// <inheritdoc cref="GlobalNPC.SpecialOnKill"/>
    public virtual bool SpecialOnKill(NPC npc) => false;

    /// <inheritdoc cref="GlobalNPC.PreKill"/>
    public virtual bool PreKill(NPC npc) => true;

    /// <inheritdoc cref="GlobalNPC.OnKill"/>
    public virtual void OnKill(NPC npc) { }

    /// <inheritdoc cref="GlobalNPC.CanFallThroughPlatforms"/>
    public virtual bool? CanFallThroughPlatforms(NPC npc) => null;

    /// <inheritdoc cref="GlobalNPC.CanBeCaughtBy"/>
    public virtual bool? CanBeCaughtBy(NPC npc, Item item, Player player) => null;

    /// <inheritdoc cref="GlobalNPC.OnCaughtBy"/>
    public virtual void OnCaughtBy(NPC npc, Player player, Item item, bool failed) { }

    /// <inheritdoc cref="GlobalNPC.ModifyNPCLoot"/>
    public virtual void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) { }

    /// <inheritdoc cref="GlobalNPC.ModifyGlobalLoot"/>
    public virtual void ModifyGlobalLoot(GlobalLoot globalLoot) { }

    /// <inheritdoc cref="GlobalNPC.CanHitPlayer"/>
    [Obsolete("Poor performance.", true)]
    public virtual bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) => true;

    /// <inheritdoc cref="GlobalNPC.ModifyHitPlayer"/>
    public virtual void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers) { }

    /// <inheritdoc cref="GlobalNPC.OnHitPlayer"/>
    public virtual void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo) { }

    /// <inheritdoc cref="GlobalNPC.CanHitNPC"/>
    [Obsolete("Poor performance.", true)]
    public virtual bool CanHitNPC(NPC npc, NPC target) => true;

    /// <inheritdoc cref="GlobalNPC.CanBeHitByNPC"/>
    [Obsolete("Poor performance.", true)]
    public virtual bool CanBeHitByNPC(NPC npc, NPC attacker) => true;

    /// <inheritdoc cref="GlobalNPC.ModifyHitNPC"/>
    public virtual void ModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="GlobalNPC.OnHitNPC"/>
    public virtual void OnHitNPC(NPC npc, NPC target, NPC.HitInfo hit) { }

    /// <inheritdoc cref="GlobalNPC.CanBeHitByItem"/>
    [Obsolete("Poor performance.", true)]
    public virtual bool? CanBeHitByItem(NPC npc, Player player, Item item) => null;

    /// <inheritdoc cref="GlobalNPC.CanCollideWithPlayerMeleeAttack"/>
    [Obsolete("Poor performance.", true)]
    public virtual bool? CanCollideWithPlayerMeleeAttack(NPC npc, Player player, Item item, Rectangle meleeAttackHitbox) => null;

    /// <inheritdoc cref="GlobalNPC.ModifyHitByItem"/>
    public virtual void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="GlobalNPC.OnHitByItem"/>
    public virtual void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone) { }

    /// <inheritdoc cref="GlobalNPC.CanBeHitByProjectile"/>
    [Obsolete("Poor performance.", true)]
    public virtual bool? CanBeHitByProjectile(NPC npc, Projectile projectile) => null;

    /// <inheritdoc cref="GlobalNPC.ModifyHitByProjectile"/>
    public virtual void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="GlobalNPC.OnHitByProjectile"/>
    public virtual void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone) { }

    /// <inheritdoc cref="GlobalNPC.ModifyIncomingHit"/>
    public virtual void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="GlobalNPC.BossHeadSlot"/>
    public virtual void BossHeadSlot(NPC npc, ref int index) { }

    /// <inheritdoc cref="GlobalNPC.BossHeadRotation"/>
    public virtual void BossHeadRotation(NPC npc, ref float rotation) { }

    /// <inheritdoc cref="GlobalNPC.BossHeadSpriteEffects"/>
    public virtual void BossHeadSpriteEffects(NPC npc, ref SpriteEffects spriteEffects) { }

    /// <inheritdoc cref="GlobalNPC.GetAlpha"/>
    public virtual Color? GetAlpha(NPC npc, Color drawColor) => null;

    /// <inheritdoc cref="GlobalNPC.DrawEffects"/>
    public virtual void DrawEffects(NPC npc, ref Color drawColor) { }

    /// <inheritdoc cref="GlobalNPC.PreDraw"/>
    public virtual bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => true;

    /// <inheritdoc cref="GlobalNPC.PostDraw"/>
    public virtual void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }

    /// <inheritdoc cref="GlobalNPC.DrawBehind"/>
    public virtual void DrawBehind(NPC npc, int index) { }

    /// <inheritdoc cref="GlobalNPC.DrawHealthBar"/>
    public virtual bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position) => null;

    /// <inheritdoc cref="GlobalNPC.EditSpawnRate"/>
    public virtual void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) { }

    /// <inheritdoc cref="GlobalNPC.EditSpawnRange"/>
    public virtual void EditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY, ref int safeRangeX, ref int safeRangeY) { }

    /// <inheritdoc cref="GlobalNPC.EditSpawnPool"/>
    public virtual void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) { }

    /// <inheritdoc cref="GlobalNPC.SpawnNPC"/>
    public virtual void SpawnNPC(int npc, int tileX, int tileY) { }

    /// <inheritdoc cref="GlobalNPC.CanChat"/>
    public virtual bool? CanChat(NPC npc) => null;

    /// <inheritdoc cref="GlobalNPC.GetChat"/>
    public virtual void GetChat(NPC npc, ref string chat) { }

    /// <inheritdoc cref="GlobalNPC.PreChatButtonClicked"/>
    public virtual bool PreChatButtonClicked(NPC npc, bool firstButton) => true;

    /// <inheritdoc cref="GlobalNPC.OnChatButtonClicked"/>
    public virtual void OnChatButtonClicked(NPC npc, bool firstButton) { }

    /// <inheritdoc cref="GlobalNPC.ModifyShop"/>
    public virtual void ModifyShop(NPCShop shop) { }

    /// <inheritdoc cref="GlobalNPC.ModifyActiveShop"/>
    public virtual void ModifyActiveShop(NPC npc, string shopName, Item[] items) { }

    /// <inheritdoc cref="GlobalNPC.SetupTravelShop"/>
    public virtual void SetupTravelShop(int[] shop, ref int nextSlot) { }

    /// <inheritdoc cref="GlobalNPC.CanGoToStatue"/>
    public virtual bool? CanGoToStatue(NPC npc, bool toKingStatue) => null;

    /// <inheritdoc cref="GlobalNPC.OnGoToStatue"/>
    public virtual void OnGoToStatue(NPC npc, bool toKingStatue) { }

    /// <inheritdoc cref="GlobalNPC.BuffTownNPC"/>
    public virtual void BuffTownNPC(ref float damageMult, ref int defense) { }

    /// <inheritdoc cref="GlobalNPC.ModifyDeathMessage"/>
    public virtual bool ModifyDeathMessage(NPC npc, ref NetworkText customText, ref Color color) => true;

    /// <inheritdoc cref="GlobalNPC.TownNPCAttackStrength"/>
    public virtual void TownNPCAttackStrength(NPC npc, ref int damage, ref float knockback) { }

    /// <inheritdoc cref="GlobalNPC.TownNPCAttackCooldown"/>
    public virtual void TownNPCAttackCooldown(NPC npc, ref int cooldown, ref int randExtraCooldown) { }

    /// <inheritdoc cref="GlobalNPC.TownNPCAttackProj"/>
    public virtual void TownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay) { }

    /// <inheritdoc cref="GlobalNPC.TownNPCAttackProjSpeed"/>
    public virtual void TownNPCAttackProjSpeed(NPC npc, ref float multiplier, ref float gravityCorrection, ref float randomOffset) { }

    /// <inheritdoc cref="GlobalNPC.TownNPCAttackShoot"/>
    public virtual void TownNPCAttackShoot(NPC npc, ref bool inBetweenShots) { }

    /// <inheritdoc cref="GlobalNPC.TownNPCAttackMagic"/>
    public virtual void TownNPCAttackMagic(NPC npc, ref float auraLightMultiplier) { }

    /// <inheritdoc cref="GlobalNPC.TownNPCAttackSwing"/>
    public virtual void TownNPCAttackSwing(NPC npc, ref int itemWidth, ref int itemHeight) { }

    /// <inheritdoc cref="GlobalNPC.DrawTownAttackGun"/>
    public virtual void DrawTownAttackGun(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset) { }

    /// <inheritdoc cref="GlobalNPC.DrawTownAttackSwing"/>
    public virtual void DrawTownAttackSwing(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset) { }

    /// <inheritdoc cref="GlobalNPC.ModifyCollisionData"/>
    public virtual bool ModifyCollisionData(NPC npc, Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) => true;

    /// <inheritdoc cref="GlobalNPC.NeedSaving"/>
    public virtual bool NeedSaving(NPC npc) => false;

    /// <inheritdoc cref="GlobalNPC.SaveData"/>
    public virtual void SaveData(NPC npc, TagCompound tag) { }

    /// <inheritdoc cref="GlobalNPC.LoadData"/>
    public virtual void LoadData(NPC npc, TagCompound tag) { }

    /// <inheritdoc cref="GlobalNPC.PickEmote"/>
    public virtual int? PickEmote(NPC npc, Player closestPlayer, List<int> emoteList, WorldUIAnchor otherAnchor) => null;

    /// <inheritdoc cref="GlobalNPC.ChatBubblePosition"/>
    public virtual void ChatBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects) { }

    /// <inheritdoc cref="GlobalNPC.PartyHatPosition"/>
    public virtual void PartyHatPosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects) { }

    /// <inheritdoc cref="GlobalNPC.EmoteBubblePosition"/>
    public virtual void EmoteBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects) { }
}

public abstract class GlobalProjectileBehavior : GlobalEntityBehavior<Projectile>
{
    /// <inheritdoc cref="GlobalProjectile.OnSpawn"/>
    public virtual void OnSpawn(Projectile projectile, IEntitySource source) { }

    /// <inheritdoc cref="GlobalProjectile.PreAI"/>
    public virtual bool PreAI(Projectile projectile) => true;

    /// <inheritdoc cref="GlobalProjectile.AI"/>
    public virtual void AI(Projectile projectile) { }

    /// <inheritdoc cref="GlobalProjectile.PostAI"/>
    public virtual void PostAI(Projectile projectile) { }

    /// <inheritdoc cref="GlobalProjectile.SendExtraAI"/>
    public virtual void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) { }

    /// <inheritdoc cref="GlobalProjectile.ReceiveExtraAI"/>
    public virtual void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) { }

    /// <inheritdoc cref="GlobalProjectile.ShouldUpdatePosition"/>
    public virtual bool ShouldUpdatePosition(Projectile projectile) => true;

    /// <inheritdoc cref="GlobalProjectile.TileCollideStyle"/>
    public virtual bool TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => true;

    /// <inheritdoc cref="GlobalProjectile.OnTileCollide"/>
    public virtual bool OnTileCollide(Projectile projectile, Vector2 oldVelocity) => true;

    /// <inheritdoc cref="GlobalProjectile.PreKill"/>
    public virtual bool PreKill(Projectile projectile, int timeLeft) => true;

    /// <inheritdoc cref="GlobalProjectile.OnKill"/>
    public virtual void OnKill(Projectile projectile, int timeLeft) { }

    /// <inheritdoc cref="GlobalProjectile.CanCutTiles"/>
    public virtual bool? CanCutTiles(Projectile projectile) => null;

    /// <inheritdoc cref="GlobalProjectile.CutTiles"/>
    public virtual void CutTiles(Projectile projectile) { }

    /// <inheritdoc cref="GlobalProjectile.CanDamage"/>
    public virtual bool? CanDamage(Projectile projectile) => null;

    /// <inheritdoc cref="GlobalProjectile.MinionContactDamage"/>
    public virtual bool MinionContactDamage(Projectile projectile) => false;

    /// <inheritdoc cref="GlobalProjectile.ModifyDamageHitbox"/>
    public virtual void ModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox) { }

    /// <inheritdoc cref="GlobalProjectile.CanHitNPC"/>
    [Obsolete("Poor performance.", true)]
    public virtual bool? CanHitNPC(Projectile projectile, NPC target) => null;

    /// <inheritdoc cref="GlobalProjectile.ModifyHitNPC"/>
    public virtual void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="GlobalProjectile.OnHitNPC"/>
    public virtual void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

    /// <inheritdoc cref="GlobalProjectile.CanHitPvp"/>
    [Obsolete("Poor performance.", true)]
    public virtual bool CanHitPvp(Projectile projectile, Player target) => true;

    /// <inheritdoc cref="GlobalProjectile.CanHitPlayer"/>
    [Obsolete("Poor performance.", true)]
    public virtual bool CanHitPlayer(Projectile projectile, Player target) => true;

    /// <inheritdoc cref="GlobalProjectile.ModifyHitPlayer"/>
    public virtual void ModifyHitPlayer(Projectile projectile, Player target, ref Player.HurtModifiers modifiers) { }

    /// <inheritdoc cref="GlobalProjectile.OnHitPlayer"/>
    public virtual void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info) { }

    /// <inheritdoc cref="GlobalProjectile.Colliding"/>
    [Obsolete("Poor performance.", true)]
    public virtual bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox) => null;

    /// <inheritdoc cref="GlobalProjectile.GetAlpha"/>
    public virtual Color? GetAlpha(Projectile projectile, Color lightColor) => null;

    /// <inheritdoc cref="GlobalProjectile.PreDrawExtras"/>
    public virtual bool PreDrawExtras(Projectile projectile) => true;

    /// <inheritdoc cref="GlobalProjectile.PreDraw"/>
    public virtual bool PreDraw(Projectile projectile, ref Color lightColor) => true;

    /// <inheritdoc cref="GlobalProjectile.PostDraw"/>
    public virtual void PostDraw(Projectile projectile, Color lightColor) { }

    /// <inheritdoc cref="GlobalProjectile.DrawBehind"/>
    public virtual void DrawBehind(Projectile projectile, int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) { }

    /// <inheritdoc cref="GlobalProjectile.CanUseGrapple"/>
    public virtual bool? CanUseGrapple(int type, Player player) => null;

    /// <inheritdoc cref="GlobalProjectile.UseGrapple"/>
    public virtual void UseGrapple(Player player, ref int type) { }

    /// <inheritdoc cref="GlobalProjectile.NumGrappleHooks"/>
    public virtual void NumGrappleHooks(Projectile projectile, Player player, ref int numHooks) { }

    /// <inheritdoc cref="GlobalProjectile.GrappleRetreatSpeed"/>
    public virtual void GrappleRetreatSpeed(Projectile projectile, Player player, ref float speed) { }

    /// <inheritdoc cref="GlobalProjectile.GrapplePullSpeed"/>
    public virtual void GrapplePullSpeed(Projectile projectile, Player player, ref float speed) { }

    /// <inheritdoc cref="GlobalProjectile.GrappleTargetPoint"/>
    public virtual void GrappleTargetPoint(Projectile projectile, Player player, ref float grappleX, ref float grappleY) { }

    /// <inheritdoc cref="GlobalProjectile.GrappleCanLatchOnTo"/>
    public virtual bool? GrappleCanLatchOnTo(Projectile projectile, Player player, int x, int y) => null;

    /// <inheritdoc cref="GlobalProjectile.PrepareBombToBlow"/>
    public virtual void PrepareBombToBlow(Projectile projectile) { }

    /// <inheritdoc cref="GlobalProjectile.EmitEnchantmentVisualsAt"/>
    public virtual void EmitEnchantmentVisualsAt(Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight) { }
}

public abstract class GlobalItemBehavior : GlobalEntityBehavior<Item>
{
    /// <inheritdoc cref="GlobalItem.OnCreated"/>
    public virtual void OnCreated(Item item, ItemCreationContext context) { }

    /// <inheritdoc cref="GlobalItem.OnSpawn"/>
    public virtual void OnSpawn(Item item, IEntitySource source) { }

    /// <inheritdoc cref="GlobalItem.ChoosePrefix"/>
    public virtual int ChoosePrefix(Item item, UnifiedRandom rand) => -1;

    /// <inheritdoc cref="GlobalItem.PrefixChance"/>
    public virtual bool? PrefixChance(Item item, int pre, UnifiedRandom rand) => null;

    /// <inheritdoc cref="GlobalItem.AllowPrefix"/>
    public virtual bool AllowPrefix(Item item, int pre) => true;

    /// <inheritdoc cref="GlobalItem.CanUseItem"/>
    public virtual bool CanUseItem(Item item, Player player) => true;

    /// <inheritdoc cref="GlobalItem.CanAutoReuseItem"/>
    public virtual bool? CanAutoReuseItem(Item item, Player player) => null;

    /// <inheritdoc cref="GlobalItem.UseStyle"/>
    public virtual void UseStyle(Item item, Player player, Rectangle heldItemFrame) { }

    /// <inheritdoc cref="GlobalItem.HoldStyle"/>
    public virtual void HoldStyle(Item item, Player player, Rectangle heldItemFrame) { }

    /// <inheritdoc cref="GlobalItem.HoldItem"/>
    public virtual void HoldItem(Item item, Player player) { }

    /// <inheritdoc cref="GlobalItem.UseTimeMultiplier"/>
    public virtual float UseTimeMultiplier(Item item, Player player) => 1f;

    /// <inheritdoc cref="GlobalItem.UseAnimationMultiplier"/>
    public virtual float UseAnimationMultiplier(Item item, Player player) => 1f;

    /// <inheritdoc cref="GlobalItem.UseSpeedMultiplier"/>
    public virtual float UseSpeedMultiplier(Item item, Player player) => 1f;

    /// <inheritdoc cref="GlobalItem.GetHealLife"/>
    public virtual void GetHealLife(Item item, Player player, bool quickHeal, ref int healValue) { }

    /// <inheritdoc cref="GlobalItem.GetHealMana"/>
    public virtual void GetHealMana(Item item, Player player, bool quickHeal, ref int healValue) { }

    /// <inheritdoc cref="GlobalItem.ModifyManaCost"/>
    public virtual void ModifyManaCost(Item item, Player player, ref float reduce, ref float mult) { }

    /// <inheritdoc cref="GlobalItem.OnMissingMana"/>
    public virtual void OnMissingMana(Item item, Player player, int neededMana) { }

    /// <inheritdoc cref="GlobalItem.OnConsumeMana"/>
    public virtual void OnConsumeMana(Item item, Player player, int manaConsumed) { }

    /// <inheritdoc cref="GlobalItem.ModifyPotionDelay"/>
    public virtual void ModifyPotionDelay(Item item, Player player, ref int baseDelay) { }

    /// <inheritdoc cref="GlobalItem.ApplyPotionDelay"/>
    public virtual bool ApplyPotionDelay(Item item, Player player, int potionDelay) => true;

    /// <inheritdoc cref="GlobalItem.ModifyWeaponDamage"/>
    public virtual void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) { }

    /// <inheritdoc cref="GlobalItem.ModifyResearchSorting"/>
    public virtual void ModifyResearchSorting(Item item, ref ContentSamples.CreativeHelper.ItemGroup itemGroup) { }

    /// <inheritdoc cref="GlobalItem.CanConsumeBait"/>
    public virtual bool? CanConsumeBait(Player player, Item bait) => null;

    /// <inheritdoc cref="GlobalItem.CanResearch"/>
    public virtual bool CanResearch(Item item) => true;

    /// <inheritdoc cref="GlobalItem.OnResearched"/>
    public virtual void OnResearched(Item item, bool fullyResearched) { }

    /// <inheritdoc cref="GlobalItem.ModifyWeaponKnockback"/>
    public virtual void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback) { }

    /// <inheritdoc cref="GlobalItem.ModifyWeaponCrit"/>
    public virtual void ModifyWeaponCrit(Item item, Player player, ref float crit) { }

    /// <inheritdoc cref="GlobalItem.NeedsAmmo"/>
    public virtual bool NeedsAmmo(Item item, Player player) => true;

    /// <inheritdoc cref="GlobalItem.PickAmmo"/>
    public virtual void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback) { }

    /// <inheritdoc cref="GlobalItem.CanChooseAmmo"/>
    public virtual bool? CanChooseAmmo(Item weapon, Item ammo, Player player) => null;

    /// <inheritdoc cref="GlobalItem.CanBeChosenAsAmmo"/>
    public virtual bool? CanBeChosenAsAmmo(Item ammo, Item weapon, Player player) => null;

    /// <inheritdoc cref="GlobalItem.CanConsumeAmmo"/>
    public virtual bool CanConsumeAmmo(Item weapon, Item ammo, Player player) => true;

    /// <inheritdoc cref="GlobalItem.CanBeConsumedAsAmmo"/>
    public virtual bool CanBeConsumedAsAmmo(Item ammo, Item weapon, Player player) => true;

    /// <inheritdoc cref="GlobalItem.OnConsumeAmmo"/>
    public virtual void OnConsumeAmmo(Item weapon, Item ammo, Player player) { }

    /// <inheritdoc cref="GlobalItem.OnConsumedAsAmmo"/>
    public virtual void OnConsumedAsAmmo(Item ammo, Item weapon, Player player) { }

    /// <inheritdoc cref="GlobalItem.CanShoot"/>
    public virtual bool CanShoot(Item item, Player player) => true;

    /// <inheritdoc cref="GlobalItem.ModifyShootStats"/>
    public virtual void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) { }

    /// <inheritdoc cref="GlobalItem.Shoot"/>
    public virtual bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => true;

    /// <inheritdoc cref="GlobalItem.UseItemHitbox"/>
    public virtual void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox) { }

    /// <inheritdoc cref="GlobalItem.MeleeEffects"/>
    public virtual void MeleeEffects(Item item, Player player, Rectangle hitbox) { }

    /// <inheritdoc cref="GlobalItem.CanCatchNPC"/>
    public virtual bool? CanCatchNPC(Item item, NPC target, Player player) => null;

    /// <inheritdoc cref="GlobalItem.OnCatchNPC"/>
    public virtual void OnCatchNPC(Item item, NPC npc, Player player, bool failed) { }

    /// <inheritdoc cref="GlobalItem.ModifyItemScale"/>
    public virtual void ModifyItemScale(Item item, Player player, ref float scale) { }

    /// <inheritdoc cref="GlobalItem.CanHitNPC"/>
    [Obsolete("Poor performance.", true)]
    public virtual bool? CanHitNPC(Item item, Player player, NPC target) => null;

    /// <inheritdoc cref="GlobalItem.CanMeleeAttackCollideWithNPC"/>
    [Obsolete("Poor performance.", true)]
    public virtual bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, Player player, NPC target) => null;

    /// <inheritdoc cref="GlobalItem.ModifyHitNPC"/>
    public virtual void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="GlobalItem.OnHitNPC"/>
    public virtual void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone) { }

    /// <inheritdoc cref="GlobalItem.CanHitPvp"/>
    [Obsolete("Poor performance.", true)]
    public virtual bool CanHitPvp(Item item, Player player, Player target) => true;

    /// <inheritdoc cref="GlobalItem.ModifyHitPvp"/>
    public virtual void ModifyHitPvp(Item item, Player player, Player target, ref Player.HurtModifiers modifiers) { }

    /// <inheritdoc cref="GlobalItem.OnHitPvp"/>
    public virtual void OnHitPvp(Item item, Player player, Player target, Player.HurtInfo hurtInfo) { }

    /// <inheritdoc cref="GlobalItem.UseItem"/>
    public virtual bool? UseItem(Item item, Player player) => null;

    /// <inheritdoc cref="GlobalItem.UseAnimation"/>
    public virtual void UseAnimation(Item item, Player player) { }

    /// <inheritdoc cref="GlobalItem.ConsumeItem"/>
    public virtual bool ConsumeItem(Item item, Player player) => true;

    /// <inheritdoc cref="GlobalItem.OnConsumeItem"/>
    public virtual void OnConsumeItem(Item item, Player player) { }

    /// <inheritdoc cref="GlobalItem.UseItemFrame"/>
    public virtual void UseItemFrame(Item item, Player player) { }

    /// <inheritdoc cref="GlobalItem.HoldItemFrame"/>
    public virtual void HoldItemFrame(Item item, Player player) { }

    /// <inheritdoc cref="GlobalItem.AltFunctionUse"/>
    public virtual bool AltFunctionUse(Item item, Player player) => false;

    /// <inheritdoc cref="GlobalItem.UpdateInventory"/>
    public virtual void UpdateInventory(Item item, Player player) { }

    /// <inheritdoc cref="GlobalItem.UpdateInfoAccessory"/>
    public virtual void UpdateInfoAccessory(Item item, Player player) { }

    /// <inheritdoc cref="GlobalItem.UpdateEquip"/>
    public virtual void UpdateEquip(Item item, Player player) { }

    /// <inheritdoc cref="GlobalItem.UpdateAccessory"/>
    public virtual void UpdateAccessory(Item item, Player player, bool hideVisual) { }

    /// <inheritdoc cref="GlobalItem.UpdateVanity"/>
    public virtual void UpdateVanity(Item item, Player player) { }

    /// <inheritdoc cref="GlobalItem.UpdateVisibleAccessory"/>
    public virtual void UpdateVisibleAccessory(Item item, Player player, bool hideVisual) { }

    /// <inheritdoc cref="GlobalItem.UpdateItemDye"/>
    public virtual void UpdateItemDye(Item item, Player player, int dye, bool hideVisual) { }

    /// <inheritdoc cref="GlobalItem.IsArmorSet"/>
    public virtual string IsArmorSet(Item head, Item body, Item legs) => "";

    /// <inheritdoc cref="GlobalItem.UpdateArmorSet"/>
    public virtual void UpdateArmorSet(Player player, string set) { }

    /// <inheritdoc cref="GlobalItem.IsVanitySet"/>
    public virtual string IsVanitySet(int head, int body, int legs)
    {
        int headItemType = 0;
        if (head >= 0)
            headItemType = Item.headType[head];

        Item headItem = ContentSamples.ItemsByType[headItemType];

        int bodyItemType = 0;
        if (body >= 0)
            bodyItemType = Item.bodyType[body];

        Item bodyItem = ContentSamples.ItemsByType[bodyItemType];

        int legsItemType = 0;
        if (legs >= 0)
            legsItemType = Item.legType[legs];

        Item legItem = ContentSamples.ItemsByType[legsItemType];

        return IsArmorSet(headItem, bodyItem, legItem);
    }

    /// <inheritdoc cref="GlobalItem.PreUpdateVanitySet"/>
    public virtual void PreUpdateVanitySet(Player player, string set) { }

    /// <inheritdoc cref="GlobalItem.UpdateVanitySet"/>
    public virtual void UpdateVanitySet(Player player, string set) { }

    /// <inheritdoc cref="GlobalItem.ArmorSetShadows"/>
    public virtual void ArmorSetShadows(Player player, string set) { }

    /// <inheritdoc cref="GlobalItem.SetMatch"/>
    public virtual void SetMatch(int armorSlot, int type, bool male, ref int equipSlot, ref bool robes) { }

    /// <inheritdoc cref="GlobalItem.CanRightClick"/>
    public virtual bool CanRightClick(Item item) => false;

    /// <inheritdoc cref="GlobalItem.RightClick"/>
    public virtual void RightClick(Item item, Player player) { }

    /// <inheritdoc cref="GlobalItem.ModifyItemLoot"/>
    public virtual void ModifyItemLoot(Item item, ItemLoot itemLoot) { }

    /// <inheritdoc cref="GlobalItem.CanStack"/>
    public virtual bool CanStack(Item destination, Item source) => true;

    /// <inheritdoc cref="GlobalItem.CanStackInWorld"/>
    public virtual bool CanStackInWorld(Item destination, Item source) => true;

    /// <inheritdoc cref="GlobalItem.OnStack"/>
    public virtual void OnStack(Item destination, Item source, int numToTransfer) { }

    /// <inheritdoc cref="GlobalItem.SplitStack"/>
    public virtual void SplitStack(Item destination, Item source, int numToTransfer) { }

    /// <inheritdoc cref="GlobalItem.ReforgePrice"/>
    public virtual bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount) => true;

    /// <inheritdoc cref="GlobalItem.CanReforge"/>
    public virtual bool CanReforge(Item item) => true;

    /// <inheritdoc cref="GlobalItem.PreReforge"/>
    public virtual void PreReforge(Item item) { }

    /// <inheritdoc cref="GlobalItem.PostReforge"/>
    public virtual void PostReforge(Item item) { }

    /// <inheritdoc cref="GlobalItem.DrawArmorColor"/>
    public virtual void DrawArmorColor(EquipType type, int slot, Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) { }

    /// <inheritdoc cref="GlobalItem.ArmorArmGlowMask"/>
    public virtual void ArmorArmGlowMask(int slot, Player drawPlayer, float shadow, ref int glowMask, ref Color color) { }

    /// <inheritdoc cref="GlobalItem.VerticalWingSpeeds"/>
    public virtual void VerticalWingSpeeds(Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) { }

    /// <inheritdoc cref="GlobalItem.HorizontalWingSpeeds"/>
    public virtual void HorizontalWingSpeeds(Item item, Player player, ref float speed, ref float acceleration) { }

    /// <inheritdoc cref="GlobalItem.WingUpdate"/>
    public virtual bool WingUpdate(int wings, Player player, bool inUse) => false;

    /// <inheritdoc cref="GlobalItem.Update"/>
    public virtual void Update(Item item, ref float gravity, ref float maxFallSpeed) { }

    /// <inheritdoc cref="GlobalItem.PostUpdate"/>
    public virtual void PostUpdate(Item item) { }

    /// <inheritdoc cref="GlobalItem.GrabRange"/>
    public virtual void GrabRange(Item item, Player player, ref int grabRange) { }

    /// <inheritdoc cref="GlobalItem.GrabStyle"/>
    public virtual bool GrabStyle(Item item, Player player) => false;

    /// <inheritdoc cref="GlobalItem.CanPickup"/>
    public virtual bool CanPickup(Item item, Player player) => true;

    /// <inheritdoc cref="GlobalItem.OnPickup"/>
    public virtual bool OnPickup(Item item, Player player) => true;

    /// <inheritdoc cref="GlobalItem.ItemSpace"/>
    public virtual bool ItemSpace(Item item, Player player) => false;

    /// <inheritdoc cref="GlobalItem.GetAlpha"/>
    public virtual Color? GetAlpha(Item item, Color lightColor) => null;

    /// <inheritdoc cref="GlobalItem.PreDrawInWorld"/>
    public virtual bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) => true;

    /// <inheritdoc cref="GlobalItem.PostDrawInWorld"/>
    public virtual void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) { }

    /// <inheritdoc cref="GlobalItem.PreDrawInInventory"/>
    public virtual bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) => true;

    /// <inheritdoc cref="GlobalItem.PostDrawInInventory"/>
    public virtual void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) { }

    /// <inheritdoc cref="GlobalItem.HoldoutOffset"/>
    public virtual Vector2? HoldoutOffset(int type) => null;

    /// <inheritdoc cref="GlobalItem.HoldoutOrigin"/>
    public virtual Vector2? HoldoutOrigin(int type) => null;

    /// <inheritdoc cref="GlobalItem.CanEquipAccessory"/>
    public virtual bool CanEquipAccessory(Item item, Player player, int slot, bool modded) => true;

    /// <inheritdoc cref="GlobalItem.CanAccessoryBeEquippedWith"/>
    public virtual bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player) => true;

    /// <inheritdoc cref="GlobalItem.ExtractinatorUse"/>
    public virtual void ExtractinatorUse(int extractType, int extractinatorBlockType, ref int resultType, ref int resultStack) { }

    /// <inheritdoc cref="GlobalItem.CaughtFishStack"/>
    public virtual void CaughtFishStack(int type, ref int stack) { }

    /// <inheritdoc cref="GlobalItem.IsAnglerQuestAvailable"/>
    public virtual bool IsAnglerQuestAvailable(int type) => true;

    /// <inheritdoc cref="GlobalItem.AnglerChat"/>
    public virtual void AnglerChat(int type, ref string chat, ref string catchLocation) { }

    /// <inheritdoc cref="GlobalItem.AddRecipes"/>
    public virtual void AddRecipes() { }

    /// <inheritdoc cref="GlobalItem.PreDrawTooltip"/>
    public virtual bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y) => true;

    /// <inheritdoc cref="GlobalItem.PostDrawTooltip"/>
    public virtual void PostDrawTooltip(Item item, ReadOnlyCollection<DrawableTooltipLine> lines) { }

    /// <inheritdoc cref="GlobalItem.PreDrawTooltipLine"/>
    public virtual bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset) => true;

    /// <inheritdoc cref="GlobalItem.PostDrawTooltipLine"/>
    public virtual void PostDrawTooltipLine(Item item, DrawableTooltipLine line) { }

    /// <inheritdoc cref="GlobalItem.ModifyTooltips"/>
    public virtual void ModifyTooltips(Item item, List<TooltipLine> tooltips) { }

    /// <inheritdoc cref="GlobalItem.SaveData"/>
    public virtual void SaveData(Item item, TagCompound tag) { }

    /// <inheritdoc cref="GlobalItem.LoadData"/>
    public virtual void LoadData(Item item, TagCompound tag) { }

    /// <inheritdoc cref="GlobalItem.NetSend"/>
    public virtual void NetSend(Item item, BinaryWriter writer) { }

    /// <inheritdoc cref="GlobalItem.NetReceive"/>
    public virtual void NetReceive(Item item, BinaryReader reader) { }
}
#endregion General Behavior

#region Transoceanic General Behavior
public abstract class TOPlayerBehavior : PlayerBehavior
{
    public sealed override TOMain Mod => TOMain.Instance;
}

public abstract class TOGlobalNPCBehavior : GlobalNPCBehavior
{
    public sealed override TOMain Mod => TOMain.Instance;
}

public abstract class TOGlobalProjectileBehavior : GlobalProjectileBehavior
{
    public sealed override TOMain Mod => TOMain.Instance;
}

public abstract class TOGlobalItemBehavior : GlobalItemBehavior
{
    public sealed override TOMain Mod => TOMain.Instance;
}
#endregion Transoceanic General Behavior

#region Single Behavior
public abstract class SingleNPCBehavior : SingleEntityBehavior<NPC>
{
    public NPC NPC => _entity;

    public TOGlobalNPC OceanNPC { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _entity.Ocean; }

    public Player Target => NPC.PlayerTarget;

    public int Timer1
    {
        get => NPC.Timer1;
        set => NPC.Timer1 = value;
    }
    public int Timer2
    {
        get => NPC.Timer2;
        set => NPC.Timer2 = value;
    }
    public int Timer3
    {
        get => NPC.Timer3;
        set => NPC.Timer3 = value;
    }
    public int Timer4
    {
        get => NPC.Timer4;
        set => NPC.Timer4 = value;
    }
    public int Timer5
    {
        get => NPC.Timer5;
        set => NPC.Timer5 = value;
    }
    public float Timer6
    {
        get => NPC.Timer6;
        set => NPC.Timer6 = value;
    }
    public float Timer7
    {
        get => NPC.Timer7;
        set => NPC.Timer7 = value;
    }
    public float Timer8
    {
        get => NPC.Timer8;
        set => NPC.Timer8 = value;
    }

    public Union32 AI_Union_0
    {
        get => (Union32)NPC.ai[0];
        set => NPC.ai[0] = value.f;
    }
    public Union32 AI_Union_1
    {
        get => (Union32)NPC.ai[1];
        set => NPC.ai[1] = value.f;
    }
    public Union32 AI_Union_2
    {
        get => (Union32)NPC.ai[2];
        set => NPC.ai[2] = value.f;
    }
    public Union32 AI_Union_3
    {
        get => (Union32)NPC.ai[3];
        set => NPC.ai[3] = value.f;
    }
    public Union32 LocalAI_Union_0
    {
        get => (Union32)NPC.localAI[0];
        set => NPC.localAI[0] = value.f;
    }
    public Union32 LocalAI_Union_1
    {
        get => (Union32)NPC.localAI[1];
        set => NPC.localAI[1] = value.f;
    }
    public Union32 LocalAI_Union_2
    {
        get => (Union32)NPC.localAI[2];
        set => NPC.localAI[2] = value.f;
    }
    public Union32 LocalAI_Union_3
    {
        get => (Union32)NPC.localAI[3];
        set => NPC.localAI[3] = value.f;
    }

    public IEntitySource SourceAI => NPC.GetSource_FromAI();

    #region 虚成员
    #region Defaults
    /// <inheritdoc cref="GlobalNPC.SetDefaultsFromNetId"/>
    public virtual void SetDefaultsFromNetId() { }

    /// <inheritdoc cref="GlobalNPC.ApplyDifficultyAndPlayerScaling"/>
    public virtual void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) { }

    /// <inheritdoc cref="GlobalNPC.SetBestiary"/>
    public virtual void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) { }

    /// <inheritdoc cref="GlobalNPC.ModifyTypeName"/>
    public virtual void ModifyTypeName(ref string typeName) { }

    /// <inheritdoc cref="GlobalNPC.ModifyHoverBoundingBox"/>
    public virtual void ModifyHoverBoundingBox(ref Rectangle boundingBox) { }

    /// <inheritdoc cref="GlobalNPC.PreHoverInteract"/>
    public virtual bool PreHoverInteract(bool mouseIntersects) => true;

    /// <inheritdoc cref="GlobalNPC.ModifyTownNPCProfile"/>
    public virtual ITownNPCProfile ModifyTownNPCProfile() => null;

    /// <inheritdoc cref="GlobalNPC.ModifyNPCNameList"/>
    public virtual void ModifyNPCNameList(List<string> nameList) { }

    /// <inheritdoc cref="GlobalNPC.ResetEffects"/>
    public virtual void ResetEffects() { }
    #endregion Defaults

    #region Lifetime
    /// <inheritdoc cref="GlobalNPC.OnSpawn"/>
    public virtual void OnSpawn(IEntitySource source) { }

    /// <inheritdoc cref="GlobalNPC.CheckActive"/>
    public virtual bool CheckActive() => true;

    /// <inheritdoc cref="GlobalNPC.CheckDead"/>
    public virtual bool CheckDead() => true;

    /// <inheritdoc cref="GlobalNPC.SpecialOnKill"/>
    public virtual bool SpecialOnKill() => false;

    /// <inheritdoc cref="GlobalNPC.PreKill"/>
    public virtual bool PreKill() => true;

    /// <inheritdoc cref="GlobalNPC.OnKill"/>
    public virtual void OnKill() { }

    /// <inheritdoc cref="GlobalNPC.ModifyNPCLoot"/>
    public virtual void ModifyNPCLoot(NPCLoot npcLoot) { }
    #endregion Lifetime

    #region AI
    /// <inheritdoc cref="GlobalNPC.PreAI"/>
    public virtual bool PreAI() => true;

    /// <inheritdoc cref="GlobalNPC.AI"/>
    public virtual void AI() { }

    /// <inheritdoc cref="GlobalNPC.PostAI"/>
    public virtual void PostAI() { }
    #endregion AI

    #region Draw
    /// <inheritdoc cref="GlobalNPC.FindFrame"/>
    public virtual void FindFrame(int frameHeight) { }

    /// <inheritdoc cref="GlobalNPC.GetAlpha"/>
    public virtual Color? GetAlpha(Color drawColor) => null;

    /// <inheritdoc cref="GlobalNPC.DrawEffects"/>
    public virtual void DrawEffects(ref Color drawColor) { }

    /// <inheritdoc cref="GlobalNPC.PreDraw"/>
    public virtual bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => true;

    /// <inheritdoc cref="GlobalNPC.PostDraw"/>
    public virtual void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }

    /// <inheritdoc cref="GlobalNPC.DrawBehind"/>
    public virtual void DrawBehind(int index) { }

    /// <inheritdoc cref="GlobalNPC.BossHeadSlot"/>
    public virtual void BossHeadSlot(ref int index) { }

    /// <inheritdoc cref="GlobalNPC.BossHeadRotation"/>
    public virtual void BossHeadRotation(ref float rotation) { }

    /// <inheritdoc cref="GlobalNPC.BossHeadSpriteEffects"/>
    public virtual void BossHeadSpriteEffects(ref SpriteEffects spriteEffects) { }

    /// <inheritdoc cref="GlobalNPC.DrawHealthBar"/>
    public virtual bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => null;
    #endregion Draw

    #region Hit
    /// <inheritdoc cref="GlobalNPC.HitEffect"/>
    public virtual void HitEffect(NPC.HitInfo hit) { }

    /// <inheritdoc cref="GlobalNPC.CanBeHitByItem"/>
    public virtual bool? CanBeHitByItem(Player player, Item item) => null;

    /// <inheritdoc cref="GlobalNPC.CanCollideWithPlayerMeleeAttack"/>
    public virtual bool? CanCollideWithPlayerMeleeAttack(Player player, Item item, Rectangle meleeAttackHitbox) => null;

    /// <inheritdoc cref="GlobalNPC.ModifyHitByItem"/>
    public virtual void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="GlobalNPC.OnHitByItem"/>
    public virtual void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) { }

    /// <inheritdoc cref="GlobalNPC.CanBeHitByProjectile"/>
    public virtual bool? CanBeHitByProjectile(Projectile projectile) => null;

    /// <inheritdoc cref="GlobalNPC.ModifyHitByProjectile"/>
    public virtual void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="GlobalNPC.OnHitByProjectile"/>
    public virtual void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) { }

    /// <inheritdoc cref="GlobalNPC.CanBeHitByNPC"/>
    public virtual bool CanBeHitByNPC(NPC attacker) => true;

    /// <inheritdoc cref="GlobalNPC.ModifyIncomingHit"/>
    public virtual void ModifyIncomingHit(ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="GlobalNPC.CanHitPlayer"/>
    public virtual bool CanHitPlayer(Player target, ref int cooldownSlot) => true;

    /// <inheritdoc cref="GlobalNPC.ModifyHitPlayer"/>
    public virtual void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) { }

    /// <inheritdoc cref="GlobalNPC.OnHitPlayer"/>
    public virtual void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) { }

    /// <inheritdoc cref="GlobalNPC.CanHitNPC"/>
    public virtual bool CanHitNPC(NPC target) => true;

    /// <inheritdoc cref="GlobalNPC.ModifyHitNPC"/>
    public virtual void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="GlobalNPC.OnHitNPC"/>
    public virtual void OnHitNPC(NPC target, NPC.HitInfo hit) { }

    /// <inheritdoc cref="GlobalNPC.ModifyCollisionData"/>
    public virtual bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) => true;
    #endregion Hit

    #region SpecialEffects
    /// <inheritdoc cref="GlobalNPC.UpdateLifeRegen"/>
    public virtual void UpdateLifeRegen(ref int damage) { }

    /// <inheritdoc cref="GlobalNPC.CanFallThroughPlatforms"/>
    public virtual bool? CanFallThroughPlatforms() => null;

    /// <inheritdoc cref="GlobalNPC.CanBeCaughtBy"/>
    public virtual bool? CanBeCaughtBy(Item item, Player player) => null;

    /// <inheritdoc cref="GlobalNPC.OnCaughtBy"/>
    public virtual void OnCaughtBy(Player player, Item item, bool failed) { }

    /// <inheritdoc cref="GlobalNPC.CanChat"/>
    public virtual bool? CanChat() => null;

    /// <inheritdoc cref="GlobalNPC.GetChat"/>
    public virtual void GetChat(ref string chat) { }

    /// <inheritdoc cref="GlobalNPC.PreChatButtonClicked"/>
    public virtual bool PreChatButtonClicked(bool firstButton) => true;

    /// <inheritdoc cref="GlobalNPC.OnChatButtonClicked"/>
    public virtual void OnChatButtonClicked(bool firstButton) { }

    /// <inheritdoc cref="GlobalNPC.ModifyActiveShop"/>
    public virtual void ModifyActiveShop(string shopName, Item[] items) { }

    /// <inheritdoc cref="GlobalNPC.CanGoToStatue"/>
    public virtual bool? CanGoToStatue(bool toKingStatue) => null;

    /// <inheritdoc cref="GlobalNPC.OnGoToStatue"/>
    public virtual void OnGoToStatue(bool toKingStatue) { }

    /// <inheritdoc cref="GlobalNPC.ModifyDeathMessage"/>
    public virtual bool ModifyDeathMessage(ref NetworkText customText, ref Color color) => true;

    /// <inheritdoc cref="GlobalNPC.TownNPCAttackStrength"/>
    public virtual void TownNPCAttackStrength(ref int damage, ref float knockback) { }

    /// <inheritdoc cref="GlobalNPC.TownNPCAttackCooldown"/>
    public virtual void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown) { }

    /// <inheritdoc cref="GlobalNPC.TownNPCAttackProj"/>
    public virtual void TownNPCAttackProj(ref int projType, ref int attackDelay) { }

    /// <inheritdoc cref="GlobalNPC.TownNPCAttackProjSpeed"/>
    public virtual void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset) { }

    /// <inheritdoc cref="GlobalNPC.TownNPCAttackShoot"/>
    public virtual void TownNPCAttackShoot(ref bool inBetweenShots) { }

    /// <inheritdoc cref="GlobalNPC.TownNPCAttackMagic"/>
    public virtual void TownNPCAttackMagic(ref float auraLightMultiplier) { }

    /// <inheritdoc cref="GlobalNPC.TownNPCAttackSwing"/>
    public virtual void TownNPCAttackSwing(ref int itemWidth, ref int itemHeight) { }

    /// <inheritdoc cref="GlobalNPC.DrawTownAttackGun"/>
    public virtual void DrawTownAttackGun(ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset) { }

    /// <inheritdoc cref="GlobalNPC.DrawTownAttackSwing"/>
    public virtual void DrawTownAttackSwing(ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset) { }

    /// <inheritdoc cref="GlobalNPC.NeedSaving"/>
    public virtual bool NeedSaving() => false;

    /// <inheritdoc cref="GlobalNPC.SaveData"/>
    public virtual void SaveData(TagCompound tag) { }

    /// <inheritdoc cref="GlobalNPC.LoadData"/>
    public virtual void LoadData(TagCompound tag) { }

    /// <inheritdoc cref="GlobalNPC.PickEmote"/>
    public virtual int? PickEmote(Player closestPlayer, List<int> emoteList, WorldUIAnchor otherAnchor) => null;

    /// <inheritdoc cref="GlobalNPC.ChatBubblePosition"/>
    public virtual void ChatBubblePosition(ref Vector2 position, ref SpriteEffects spriteEffects) { }

    /// <inheritdoc cref="GlobalNPC.PartyHatPosition"/>
    public virtual void PartyHatPosition(ref Vector2 position, ref SpriteEffects spriteEffects) { }

    /// <inheritdoc cref="GlobalNPC.EmoteBubblePosition"/>
    public virtual void EmoteBubblePosition(ref Vector2 position, ref SpriteEffects spriteEffects) { }
    #endregion SpecialEffects
    #endregion 虚成员
}

public abstract class SingleProjectileBehavior : SingleEntityBehavior<Projectile>
{
    public Projectile Projectile => _entity;

    public TOGlobalProjectile OceanProjectile { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _entity.Ocean; }

    public Player Owner => Projectile.Owner;

    public int Timer1
    {
        get => Projectile.Timer1;
        set => Projectile.Timer1 = value;
    }
    public int Timer2
    {
        get => Projectile.Timer2;
        set => Projectile.Timer2 = value;
    }
    public int Timer3
    {
        get => Projectile.Timer3;
        set => Projectile.Timer3 = value;
    }
    public float Timer4
    {
        get => Projectile.Timer4;
        set => Projectile.Timer4 = value;
    }
    public float Timer5
    {
        get => Projectile.Timer5;
        set => Projectile.Timer5 = value;
    }

    public Union32 AI_Union_0
    {
        get => (Union32)Projectile.ai[0];
        set => Projectile.ai[0] = value.f;
    }
    public Union32 AI_Union_1
    {
        get => (Union32)Projectile.ai[0];
        set => Projectile.ai[0] = value.f;
    }
    public Union32 AI_Union_2
    {
        get => (Union32)Projectile.ai[0];
        set => Projectile.ai[0] = value.f;
    }
    public Union32 LocalAI_Union_0
    {
        get => (Union32)Projectile.localAI[0];
        set => Projectile.localAI[0] = value.f;
    }
    public Union32 LocalAI_Union_1
    {
        get => (Union32)Projectile.localAI[1];
        set => Projectile.localAI[1] = value.f;
    }
    public Union32 LocalAI_Union_2
    {
        get => (Union32)Projectile.localAI[2];
        set => Projectile.localAI[2] = value.f;
    }

    public IEntitySource SourceAI => Projectile.GetSource_FromAI();

    #region 虚成员
    #region Lifetime
    /// <inheritdoc cref="GlobalProjectile.OnSpawn"/>
    public virtual void OnSpawn(IEntitySource source) { }

    /// <inheritdoc cref="GlobalProjectile.TileCollideStyle"/>
	public virtual bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => true;

    /// <inheritdoc cref="GlobalProjectile.OnTileCollide"/>
    public virtual bool OnTileCollide(Vector2 oldVelocity) => true;

    /// <inheritdoc cref="GlobalProjectile.PreKill"/>
    public virtual bool PreKill(int timeLeft) => true;

    /// <inheritdoc cref="GlobalProjectile.OnKill"/>
    public virtual void OnKill(int timeLeft) { }
    #endregion Lifetime

    #region AI
    /// <inheritdoc cref="GlobalProjectile.PreAI"/>
    public virtual bool PreAI() => true;

    /// <inheritdoc cref="GlobalProjectile.AI"/>
    public virtual void AI() { }

    /// <inheritdoc cref="GlobalProjectile.PostAI"/>
    public virtual void PostAI() { }

    /// <inheritdoc cref="GlobalProjectile.ShouldUpdatePosition"/>
    public virtual bool ShouldUpdatePosition() => true;
    #endregion AI

    #region Hit
    /// <inheritdoc cref="GlobalProjectile.CanCutTiles"/>
    public virtual bool? CanCutTiles() => null;

    /// <inheritdoc cref="GlobalProjectile.CutTiles"/>
    public virtual void CutTiles() { }

    /// <inheritdoc cref="GlobalProjectile.CanDamage"/>
    public virtual bool? CanDamage() => null;

    /// <inheritdoc cref="GlobalProjectile.MinionContactDamage"/>
    public virtual bool MinionContactDamage() => false;

    /// <inheritdoc cref="GlobalProjectile.ModifyDamageHitbox"/>
    public virtual void ModifyDamageHitbox(ref Rectangle hitbox) { }

    /// <inheritdoc cref="GlobalProjectile.CanHitNPC"/>
    public virtual bool? CanHitNPC(NPC target) => null;

    /// <inheritdoc cref="GlobalProjectile.ModifyHitNPC"/>
    public virtual void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="GlobalProjectile.OnHitNPC"/>
    public virtual void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) { }

    /// <inheritdoc cref="GlobalProjectile.CanHitPvp"/>
    public virtual bool CanHitPvp(Player target) => true;

    /// <inheritdoc cref="GlobalProjectile.CanHitPlayer"/>
    public virtual bool CanHitPlayer(Player target) => true;

    /// <inheritdoc cref="GlobalProjectile.ModifyHitPlayer"/>
    public virtual void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) { }

    /// <inheritdoc cref="GlobalProjectile.OnHitPlayer"/>
    public virtual void OnHitPlayer(Player target, Player.HurtInfo info) { }

    /// <inheritdoc cref="GlobalProjectile.Colliding"/>
    public virtual bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => null;
    #endregion Hit

    #region Draw
    /// <inheritdoc cref="GlobalProjectile.GetAlpha"/>
    public virtual Color? GetAlpha(Color lightColor) => null;

    /// <inheritdoc cref="GlobalProjectile.PreDrawExtras"/>
    public virtual bool PreDrawExtras() => true;

    /// <inheritdoc cref="GlobalProjectile.PreDraw"/>
    public virtual bool PreDraw(ref Color lightColor) => true;

    /// <inheritdoc cref="GlobalProjectile.PostDraw"/>
    public virtual void PostDraw(Color lightColor) { }

    /// <inheritdoc cref="GlobalProjectile.DrawBehind"/>
    public virtual void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) { }
    #endregion Draw

    #region SpecialEffects
    /// <inheritdoc cref="GlobalProjectile.NumGrappleHooks"/>
    public virtual void NumGrappleHooks(Player player, ref int numHooks) { }

    /// <inheritdoc cref="GlobalProjectile.GrappleRetreatSpeed"/>
    public virtual void GrappleRetreatSpeed(Player player, ref float speed) { }

    /// <inheritdoc cref="GlobalProjectile.GrapplePullSpeed"/>
    public virtual void GrapplePullSpeed(Player player, ref float speed) { }

    /// <inheritdoc cref="GlobalProjectile.GrappleTargetPoint"/>
    public virtual void GrappleTargetPoint(Player player, ref float grappleX, ref float grappleY) { }

    /// <inheritdoc cref="GlobalProjectile.GrappleCanLatchOnTo"/>
    public virtual bool? GrappleCanLatchOnTo(Player player, int x, int y) => null;

    /// <inheritdoc cref="GlobalProjectile.PrepareBombToBlow"/>
    public virtual void PrepareBombToBlow() { }

    /// <inheritdoc cref="GlobalProjectile.EmitEnchantmentVisualsAt"/>
    public virtual void EmitEnchantmentVisualsAt(Vector2 boxPosition, int boxWidth, int boxHeight) { }
    #endregion SpecialEffects
    #endregion 虚成员
}

public abstract class SingleItemBehavior : SingleEntityBehavior<Item>
{
    public Item Item => _entity;

    public TOGlobalItem OceanItem { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _entity.Ocean; }

    #region 虚成员
    #region Defaults
    /// <inheritdoc cref="GlobalItem.AddRecipes"/>
    public virtual void AddRecipes() { }
    #endregion Defaults

    #region Lifetime
    /// <inheritdoc cref="GlobalItem.OnCreated"/>
    public virtual void OnCreated(ItemCreationContext context) { }

    /// <inheritdoc cref="GlobalItem.OnSpawn"/>
    public virtual void OnSpawn(IEntitySource source) { }

    /// <inheritdoc cref="GlobalItem.Update"/>
    public virtual void Update(ref float gravity, ref float maxFallSpeed) { }

    /// <inheritdoc cref="GlobalItem.PostUpdate"/>
    public virtual void PostUpdate() { }

    /// <inheritdoc cref="GlobalItem.GrabRange"/>
    public virtual void GrabRange(Player player, ref int grabRange) { }

    /// <inheritdoc cref="GlobalItem.GrabStyle"/>
    public virtual bool GrabStyle(Player player) => false;

    /// <inheritdoc cref="GlobalItem.CanPickup"/>
    public virtual bool CanPickup(Player player) => true;

    /// <inheritdoc cref="GlobalItem.OnPickup"/>
    public virtual bool OnPickup(Player player) => true;

    /// <inheritdoc cref="GlobalItem.ItemSpace"/>
    public virtual bool ItemSpace(Player player) => false;
    #endregion Lifetime

    #region Update
    /// <inheritdoc cref="GlobalItem.UpdateInventory"/>
    public virtual void UpdateInventory(Player player) { }

    /// <inheritdoc cref="GlobalItem.UpdateInfoAccessory"/>
    public virtual void UpdateInfoAccessory(Player player) { }

    /// <inheritdoc cref="GlobalItem.UpdateEquip"/>
    public virtual void UpdateEquip(Player player) { }

    /// <inheritdoc cref="GlobalItem.UpdateAccessory"/>
    public virtual void UpdateAccessory(Player player, bool hideVisual) { }

    /// <inheritdoc cref="GlobalItem.UpdateVanity"/>
    public virtual void UpdateVanity(Player player) { }

    /// <inheritdoc cref="GlobalItem.UpdateVisibleAccessory"/>
    public virtual void UpdateVisibleAccessory(Player player, bool hideVisual) { }

    /// <inheritdoc cref="GlobalItem.UpdateItemDye"/>
    public virtual void UpdateItemDye(Player player, int dye, bool hideVisual) { }
    #endregion Update

    #region Draw
    /// <inheritdoc cref="GlobalItem.GetAlpha"/>
    public virtual Color? GetAlpha(Color lightColor) => null;

    /// <inheritdoc cref="GlobalItem.PreDrawInWorld"/>
    public virtual bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) => true;

    /// <inheritdoc cref="GlobalItem.PostDrawInWorld"/>
    public virtual void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) { }

    /// <inheritdoc cref="GlobalItem.PreDrawInInventory"/>
    public virtual bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) => true;

    /// <inheritdoc cref="GlobalItem.PostDrawInInventory"/>
    public virtual void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) { }
    #endregion Draw

    #region Prefix
    /// <inheritdoc cref="GlobalItem.ChoosePrefix"/>
    public virtual int ChoosePrefix(UnifiedRandom rand) => -1;

    /// <inheritdoc cref="GlobalItem.PrefixChance"/>
    public virtual bool? PrefixChance(int pre, UnifiedRandom rand) => null;

    /// <inheritdoc cref="GlobalItem.AllowPrefix"/>
    public virtual bool AllowPrefix(int pre) => true;

    /// <inheritdoc cref="GlobalItem.ReforgePrice"/>
    public virtual bool ReforgePrice(ref int reforgePrice, ref bool canApplyDiscount) => true;

    /// <inheritdoc cref="GlobalItem.CanReforge"/>
    public virtual bool CanReforge() => true;

    /// <inheritdoc cref="GlobalItem.PreReforge"/>
    public virtual void PreReforge() { }

    /// <inheritdoc cref="GlobalItem.PostReforge"/>
    public virtual void PostReforge() { }
    #endregion Prefix

    #region Use
    /// <inheritdoc cref="GlobalItem.AltFunctionUse"/>
    public virtual bool AltFunctionUse(Player player) => false;

    /// <inheritdoc cref="GlobalItem.CanUseItem"/>
    public virtual bool CanUseItem(Player player) => true;

    /// <inheritdoc cref="GlobalItem.CanAutoReuseItem"/>
    public virtual bool? CanAutoReuseItem(Player player) => null;

    /// <inheritdoc cref="GlobalItem.UseStyle"/>
    public virtual void UseStyle(Player player, Rectangle heldItemFrame) { }

    /// <inheritdoc cref="GlobalItem.HoldStyle"/>
    public virtual void HoldStyle(Player player, Rectangle heldItemFrame) { }

    /// <inheritdoc cref="GlobalItem.HoldItem"/>
    public virtual void HoldItem(Player player) { }

    /// <inheritdoc cref="GlobalItem.UseTimeMultiplier"/>
    public virtual float UseTimeMultiplier(Player player) => 1f;

    /// <inheritdoc cref="GlobalItem.UseAnimationMultiplier"/>
    public virtual float UseAnimationMultiplier(Player player) => 1f;

    /// <inheritdoc cref="GlobalItem.UseSpeedMultiplier"/>
    public virtual float UseSpeedMultiplier(Player player) => 1f;

    /// <inheritdoc cref="GlobalItem.UseItem"/>
    public virtual bool? UseItem(Player player) => null;

    /// <inheritdoc cref="GlobalItem.UseAnimation"/>
    public virtual void UseAnimation(Player player) { }

    /// <inheritdoc cref="GlobalItem.CanShoot"/>
    public virtual bool CanShoot(Player player) => true;

    /// <inheritdoc cref="GlobalItem.Shoot"/>
    public virtual bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => true;

    /// <inheritdoc cref="GlobalItem.CanRightClick"/>
    public virtual bool CanRightClick() => false;

    /// <inheritdoc cref="GlobalItem.RightClick"/>
    public virtual void RightClick(Player player) { }
    #endregion Use

    #region ModifyStats
    /// <inheritdoc cref="GlobalItem.ModifyWeaponDamage"/>
    public virtual void ModifyWeaponDamage(Player player, ref StatModifier damage) { }

    /// <inheritdoc cref="GlobalItem.ModifyWeaponKnockback"/>
    public virtual void ModifyWeaponKnockback(Player player, ref StatModifier knockback) { }

    /// <inheritdoc cref="GlobalItem.ModifyWeaponCrit"/>
    public virtual void ModifyWeaponCrit(Player player, ref float crit) { }

    /// <inheritdoc cref="GlobalItem.ModifyShootStats"/>
    public virtual void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) { }

    /// <inheritdoc cref="GlobalItem.ModifyItemScale"/>
    public virtual void ModifyItemScale(Player player, ref float scale) { }

    /// <inheritdoc cref="GlobalItem.ModifyItemLoot"/>
    public virtual void ModifyItemLoot(ItemLoot itemLoot) { }
    #endregion ModifyStats

    #region Hit
    /// <inheritdoc cref="GlobalItem.CanHitNPC"/>
    public virtual bool? CanHitNPC(Player player, NPC target) => null;

    /// <inheritdoc cref="GlobalItem.CanMeleeAttackCollideWithNPC"/>
    public virtual bool? CanMeleeAttackCollideWithNPC(Rectangle meleeAttackHitbox, Player player, NPC target) => null;

    /// <inheritdoc cref="GlobalItem.ModifyHitNPC"/>
    public virtual void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="GlobalItem.OnHitNPC"/>
    public virtual void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) { }

    /// <inheritdoc cref="GlobalItem.CanHitPvp"/>
    public virtual bool CanHitPvp(Player player, Player target) => true;

    /// <inheritdoc cref="GlobalItem.ModifyHitPvp"/>
    public virtual void ModifyHitPvp(Player player, Player target, ref Player.HurtModifiers modifiers) { }

    /// <inheritdoc cref="GlobalItem.OnHitPvp"/>
    public virtual void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo) { }
    #endregion Hit

    #region SpecialEffects
    /// <inheritdoc cref="GlobalItem.GetHealLife"/>
    public virtual void GetHealLife(Player player, bool quickHeal, ref int healValue) { }

    /// <inheritdoc cref="GlobalItem.GetHealMana"/>
    public virtual void GetHealMana(Player player, bool quickHeal, ref int healValue) { }

    /// <inheritdoc cref="GlobalItem.ModifyManaCost"/>
    public virtual void ModifyManaCost(Player player, ref float reduce, ref float mult) { }

    /// <inheritdoc cref="GlobalItem.OnMissingMana"/>
    public virtual void OnMissingMana(Player player, int neededMana) { }

    /// <inheritdoc cref="GlobalItem.OnConsumeMana"/>
    public virtual void OnConsumeMana(Player player, int manaConsumed) { }

    /// <inheritdoc cref="GlobalItem.ModifyPotionDelay"/>
    public virtual void ModifyPotionDelay(Player player, ref int baseDelay) { }

    /// <inheritdoc cref="GlobalItem.ApplyPotionDelay"/>
    public virtual bool ApplyPotionDelay(Player player, int potionDelay) => true;

    /// <inheritdoc cref="GlobalItem.CanResearch"/>
    public virtual bool CanResearch() => true;

    /// <inheritdoc cref="GlobalItem.OnResearched"/>
    public virtual void OnResearched(bool fullyResearched) { }

    /// <inheritdoc cref="GlobalItem.ModifyResearchSorting"/>
    public virtual void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) { }

    /// <inheritdoc cref="GlobalItem.NeedsAmmo"/>
    public virtual bool NeedsAmmo(Player player) => true;

    /// <inheritdoc cref="GlobalItem.UseItemHitbox"/>
    public virtual void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) { }

    /// <inheritdoc cref="GlobalItem.MeleeEffects"/>
    public virtual void MeleeEffects(Player player, Rectangle hitbox) { }

    /// <inheritdoc cref="GlobalItem.CanCatchNPC"/>
    public virtual bool? CanCatchNPC(NPC target, Player player) => null;

    /// <inheritdoc cref="GlobalItem.OnCatchNPC"/>
    public virtual void OnCatchNPC(NPC npc, Player player, bool failed) { }

    /// <inheritdoc cref="GlobalItem.ConsumeItem"/>
    public virtual bool ConsumeItem(Player player) => true;

    /// <inheritdoc cref="GlobalItem.OnConsumeItem"/>
    public virtual void OnConsumeItem(Player player) { }

    /// <inheritdoc cref="GlobalItem.UseItemFrame"/>
    public virtual void UseItemFrame(Player player) { }

    /// <inheritdoc cref="GlobalItem.HoldItemFrame"/>
    public virtual void HoldItemFrame(Player player) { }

    /// <inheritdoc cref="GlobalItem.VerticalWingSpeeds"/>
    public virtual void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) { }

    /// <inheritdoc cref="GlobalItem.HorizontalWingSpeeds"/>
    public virtual void HorizontalWingSpeeds(Player player, ref float speed, ref float acceleration) { }

    /// <inheritdoc cref="GlobalItem.CanEquipAccessory"/>
    public virtual bool CanEquipAccessory(Player player, int slot, bool modded) => true;
    #endregion SpecialEffects

    #region Tooltip
    /// <inheritdoc cref="GlobalItem.PreDrawTooltip"/>
    public virtual bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y) => true;

    /// <inheritdoc cref="GlobalItem.PostDrawTooltip"/>
    public virtual void PostDrawTooltip(ReadOnlyCollection<DrawableTooltipLine> lines) { }

    /// <inheritdoc cref="GlobalItem.PreDrawTooltipLine"/>
    public virtual bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset) => true;

    /// <inheritdoc cref="GlobalItem.PostDrawTooltipLine"/>
    public virtual void PostDrawTooltipLine(DrawableTooltipLine line) { }

    /// <inheritdoc cref="GlobalItem.ModifyTooltips"/>
    public virtual void ModifyTooltips(List<TooltipLine> tooltips) { }
    #endregion Tooltip

    #region WorldSaving
    /// <inheritdoc cref="GlobalItem.SaveData"/>
    public virtual void SaveData(TagCompound tag) { }

    /// <inheritdoc cref="GlobalItem.LoadData"/>
    public virtual void LoadData(TagCompound tag) { }
    #endregion WorldSaving

    #region Net
    /// <inheritdoc cref="GlobalItem.NetSend"/>
    public virtual void NetSend(BinaryWriter writer) { }

    /// <inheritdoc cref="GlobalItem.NetReceive"/>
    public virtual void NetReceive(BinaryReader reader) { }
    #endregion Net
    #endregion 虚成员
}
#endregion Single Behavior

#region Single Behavior Handler
[CriticalBehavior]
public abstract class SingleNPCBehaviorHandler<TNPCBehavior> : GlobalNPCBehavior where TNPCBehavior : SingleNPCBehavior
{
    protected abstract SingleEntityBehaviorSet<NPC, TNPCBehavior> BehaviorSet { get; }

    public virtual bool TryGetBehavior(NPC npc, out TNPCBehavior npcBehavior, [CallerMemberName] string methodName = null) => BehaviorSet.TryGetBehavior(npc, methodName, out npcBehavior);

    #region Defaults
    public override void SetStaticDefaults()
    {
        foreach (SimpleEntityBehaviorSet<NPC, TNPCBehavior> simpleSet in BehaviorSet._data.Values)
        {
            foreach (TNPCBehavior npcBehavior in simpleSet.Enumerate())
                npcBehavior.SetStaticDefaults();
        }
    }

    public override void SetDefaults(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.SetDefaults();
    }

    public override void SetDefaultsFromNetId(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.SetDefaultsFromNetId();
    }

    public override void ApplyDifficultyAndPlayerScaling(NPC npc, int numPlayers, float balance, float bossAdjustment)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ApplyDifficultyAndPlayerScaling(numPlayers, balance, bossAdjustment);
    }

    public override void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.SetBestiary(database, bestiaryEntry);
    }

    public override void ModifyTypeName(NPC npc, ref string typeName)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ModifyTypeName(ref typeName);
    }

    public override void ModifyHoverBoundingBox(NPC npc, ref Rectangle boundingBox)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ModifyHoverBoundingBox(ref boundingBox);
    }

    public override bool PreHoverInteract(NPC npc, bool mouseIntersects)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (!npcBehavior.PreHoverInteract(mouseIntersects))
                return false;
        }
        return true;
    }

    public override ITownNPCProfile ModifyTownNPCProfile(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            return npcBehavior.ModifyTownNPCProfile();
        return null;
    }

    public override void ModifyNPCNameList(NPC npc, List<string> nameList)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ModifyNPCNameList(nameList);
    }

    public override void ResetEffects(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ResetEffects();
    }
    #endregion Defaults

    #region Lifetime
    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.OnSpawn(source);
    }

    public override bool CheckActive(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (!npcBehavior.CheckActive())
                return false;
        }
        return true;
    }

    public override bool CheckDead(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (!npcBehavior.CheckDead())
                return false;
        }
        return true;
    }

    public override bool SpecialOnKill(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (npcBehavior.SpecialOnKill())
                return true;
        }
        return false;
    }

    public override bool PreKill(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (!npcBehavior.PreKill())
                return false;
        }
        return true;
    }

    public override void OnKill(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.OnKill();
    }

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ModifyNPCLoot(npcLoot);
    }
    #endregion Lifetime

    #region AI
    public override bool PreAI(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (!npcBehavior.PreAI())
                return false;
        }
        return true;
    }

    public override void AI(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.AI();
    }

    public override void PostAI(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.PostAI();
    }
    #endregion AI

    #region Draw
    public override void FindFrame(NPC npc, int frameHeight)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.FindFrame(frameHeight);
    }

    public override Color? GetAlpha(NPC npc, Color drawColor)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            Color? result = npcBehavior.GetAlpha(drawColor);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void DrawEffects(NPC npc, ref Color drawColor)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.DrawEffects(ref drawColor);
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (!npcBehavior.PreDraw(spriteBatch, screenPos, drawColor))
                return false;
        }
        return true;
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.PostDraw(spriteBatch, screenPos, drawColor);
    }

    public override void DrawBehind(NPC npc, int index)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.DrawBehind(index);
    }

    public override void BossHeadSlot(NPC npc, ref int index)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.BossHeadSlot(ref index);
    }

    public override void BossHeadRotation(NPC npc, ref float rotation)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.BossHeadRotation(ref rotation);
    }

    public override void BossHeadSpriteEffects(NPC npc, ref SpriteEffects spriteEffects)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.BossHeadSpriteEffects(ref spriteEffects);
    }

    public override bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            bool? result = npcBehavior.DrawHealthBar(hbPosition, ref scale, ref position);
            if (result is not null)
                return result;
        }
        return null;
    }
    #endregion Draw

    #region Hit
    public override void HitEffect(NPC npc, NPC.HitInfo hit)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.HitEffect(hit);
    }

    [Obsolete("Poor performance.", true)]
    public override bool? CanBeHitByItem(NPC npc, Player player, Item item)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            bool? result = npcBehavior.CanBeHitByItem(player, item);
            if (result is not null)
                return result;
        }
        return null;
    }

    [Obsolete("Poor performance.", true)]
    public override bool? CanCollideWithPlayerMeleeAttack(NPC npc, Player player, Item item, Rectangle meleeAttackHitbox)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            bool? result = npcBehavior.CanCollideWithPlayerMeleeAttack(player, item, meleeAttackHitbox);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ModifyHitByItem(player, item, ref modifiers);
    }

    public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.OnHitByItem(player, item, hit, damageDone);
    }

    [Obsolete("Poor performance.", true)]
    public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            bool? result = npcBehavior.CanBeHitByProjectile(projectile);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ModifyHitByProjectile(projectile, ref modifiers);
    }

    public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.OnHitByProjectile(projectile, hit, damageDone);
    }

    [Obsolete("Poor performance.", true)]
    public override bool CanBeHitByNPC(NPC npc, NPC attacker)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (!npcBehavior.CanBeHitByNPC(attacker))
                return false;
        }
        return true;
    }

    public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ModifyIncomingHit(ref modifiers);
    }

    [Obsolete("Poor performance.", true)]
    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (!npcBehavior.CanHitPlayer(target, ref cooldownSlot))
                return false;
        }
        return true;
    }

    public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ModifyHitPlayer(target, ref modifiers);
    }

    public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.OnHitPlayer(target, hurtInfo);
    }

    [Obsolete("Poor performance.", true)]
    public override bool CanHitNPC(NPC npc, NPC target)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (!npcBehavior.CanHitNPC(target))
                return false;
        }
        return true;
    }

    public override void ModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ModifyHitNPC(target, ref modifiers);
    }

    public override void OnHitNPC(NPC npc, NPC target, NPC.HitInfo hit)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.OnHitNPC(target, hit);
    }

    public override bool ModifyCollisionData(NPC npc, Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (!npcBehavior.ModifyCollisionData(victimHitbox, ref immunityCooldownSlot, ref damageMultiplier, ref npcHitbox))
                return false;
        }
        return true;
    }
    #endregion Hit

    #region SpecialEffects
    public override void UpdateLifeRegen(NPC npc, ref int damage)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.UpdateLifeRegen(ref damage);
    }

    public override bool? CanFallThroughPlatforms(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            bool? result = npcBehavior.CanFallThroughPlatforms();
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool? CanBeCaughtBy(NPC npc, Item item, Player player)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            bool? result = npcBehavior.CanBeCaughtBy(item, player);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void OnCaughtBy(NPC npc, Player player, Item item, bool failed)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.OnCaughtBy(player, item, failed);
    }

    public override bool? CanChat(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            bool? result = npcBehavior.CanChat();
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void GetChat(NPC npc, ref string chat)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.GetChat(ref chat);
    }

    public override bool PreChatButtonClicked(NPC npc, bool firstButton)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (!npcBehavior.PreChatButtonClicked(firstButton))
                return false;
        }
        return true;
    }

    public override void OnChatButtonClicked(NPC npc, bool firstButton)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.OnChatButtonClicked(firstButton);
    }

    public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ModifyActiveShop(shopName, items);
    }

    public override bool? CanGoToStatue(NPC npc, bool toKingStatue)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            bool? result = npcBehavior.CanGoToStatue(toKingStatue);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void OnGoToStatue(NPC npc, bool toKingStatue)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.OnGoToStatue(toKingStatue);
    }

    public override bool ModifyDeathMessage(NPC npc, ref NetworkText customText, ref Color color)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (!npcBehavior.ModifyDeathMessage(ref customText, ref color))
                return false;
        }
        return true;
    }

    public override void TownNPCAttackStrength(NPC npc, ref int damage, ref float knockback)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.TownNPCAttackStrength(ref damage, ref knockback);
    }

    public override void TownNPCAttackCooldown(NPC npc, ref int cooldown, ref int randExtraCooldown)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.TownNPCAttackCooldown(ref cooldown, ref randExtraCooldown);
    }

    public override void TownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.TownNPCAttackProj(ref projType, ref attackDelay);
    }

    public override void TownNPCAttackProjSpeed(NPC npc, ref float multiplier, ref float gravityCorrection, ref float randomOffset)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.TownNPCAttackProjSpeed(ref multiplier, ref gravityCorrection, ref randomOffset);
    }

    public override void TownNPCAttackShoot(NPC npc, ref bool inBetweenShots)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.TownNPCAttackShoot(ref inBetweenShots);
    }

    public override void TownNPCAttackMagic(NPC npc, ref float auraLightMultiplier)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.TownNPCAttackMagic(ref auraLightMultiplier);
    }

    public override void TownNPCAttackSwing(NPC npc, ref int itemWidth, ref int itemHeight)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.TownNPCAttackSwing(ref itemWidth, ref itemHeight);
    }

    public override void DrawTownAttackGun(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.DrawTownAttackGun(ref item, ref itemFrame, ref scale, ref horizontalHoldoutOffset);
    }

    public override void DrawTownAttackSwing(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.DrawTownAttackSwing(ref item, ref itemFrame, ref itemSize, ref scale, ref offset);
    }

    public override int? PickEmote(NPC npc, Player closestPlayer, List<int> emoteList, WorldUIAnchor otherAnchor)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            int? result = npcBehavior.PickEmote(closestPlayer, emoteList, otherAnchor);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void ChatBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ChatBubblePosition(ref position, ref spriteEffects);
    }

    public override void PartyHatPosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.PartyHatPosition(ref position, ref spriteEffects);
    }

    public override void EmoteBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.EmoteBubblePosition(ref position, ref spriteEffects);
    }
    #endregion SpecialEffects

    #region WorldSaving
    public override bool NeedSaving(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (npcBehavior.NeedSaving())
                return true;
        }
        return false;
    }

    public override void SaveData(NPC npc, TagCompound tag)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.SaveData(tag);
    }

    public override void LoadData(NPC npc, TagCompound tag)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.LoadData(tag);
    }
    #endregion WorldSaving
}

[CriticalBehavior]
public abstract class SingleProjectileBehaviorHandler<TProjectileBehavior> : GlobalProjectileBehavior where TProjectileBehavior : SingleProjectileBehavior
{
    protected abstract SingleEntityBehaviorSet<Projectile, TProjectileBehavior> BehaviorSet { get; }

    public virtual bool TryGetBehavior(Projectile projectile, out TProjectileBehavior projectileBehavior, [CallerMemberName] string methodName = null) => BehaviorSet.TryGetBehavior(projectile, methodName, out projectileBehavior);

    #region Defaults
    public override void SetStaticDefaults()
    {
        foreach (SimpleEntityBehaviorSet<Projectile, TProjectileBehavior> simpleSet in BehaviorSet._data.Values)
        {
            foreach (TProjectileBehavior projectileBehavior in simpleSet.Enumerate())
                projectileBehavior.SetStaticDefaults();
        }
    }

    public override void SetDefaults(Projectile projectile)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.SetDefaults();
    }
    #endregion Defaults

    #region Lifetime
    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.OnSpawn(source);
    }

    public override bool TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac))
                return false;
        }
        return true;
    }

    public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.OnTileCollide(oldVelocity))
                return false;
        }
        return true;
    }

    public override bool PreKill(Projectile projectile, int timeLeft)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.PreKill(timeLeft))
                return false;
        }
        return true;
    }

    public override void OnKill(Projectile projectile, int timeLeft)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.OnKill(timeLeft);
    }
    #endregion Lifetime

    #region AI
    public override bool PreAI(Projectile projectile)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.PreAI())
                return false;
        }
        return true;
    }

    public override void AI(Projectile projectile)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.AI();
    }

    public override void PostAI(Projectile projectile)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.PostAI();
    }

    public override bool ShouldUpdatePosition(Projectile projectile)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.ShouldUpdatePosition())
                return false;
        }
        return true;
    }
    #endregion AI

    #region Draw
    public override Color? GetAlpha(Projectile projectile, Color lightColor)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            Color? result = projectileBehavior.GetAlpha(lightColor);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool PreDrawExtras(Projectile projectile)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.PreDrawExtras())
                return false;
        }
        return true;
    }

    public override bool PreDraw(Projectile projectile, ref Color lightColor)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.PreDraw(ref lightColor))
                return false;
        }
        return true;
    }

    public override void PostDraw(Projectile projectile, Color lightColor)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.PostDraw(lightColor);
    }

    public override void DrawBehind(Projectile projectile, int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
    }
    #endregion Draw

    #region Hit
    public override bool? CanCutTiles(Projectile projectile)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            bool? result = projectileBehavior.CanCutTiles();
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void CutTiles(Projectile projectile)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.CutTiles();
    }

    public override bool? CanDamage(Projectile projectile)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            bool? result = projectileBehavior.CanDamage();
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool MinionContactDamage(Projectile projectile)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.MinionContactDamage())
                return false;
        }
        return false;
    }

    public override void ModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.ModifyDamageHitbox(ref hitbox);
    }

    [Obsolete("Poor performance.", true)]
    public override bool? CanHitNPC(Projectile projectile, NPC target)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            bool? result = projectileBehavior.CanHitNPC(target);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.ModifyHitNPC(target, ref modifiers);
    }

    public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.OnHitNPC(target, hit, damageDone);
    }

    [Obsolete("Poor performance.", true)]
    public override bool CanHitPvp(Projectile projectile, Player target)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.CanHitPvp(target))
                return false;
        }
        return true;
    }

    [Obsolete("Poor performance.", true)]
    public override bool CanHitPlayer(Projectile projectile, Player target)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.CanHitPlayer(target))
                return false;
        }
        return true;
    }

    public override void ModifyHitPlayer(Projectile projectile, Player target, ref Player.HurtModifiers modifiers)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.ModifyHitPlayer(target, ref modifiers);
    }

    public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.OnHitPlayer(target, info);
    }

    [Obsolete("Poor performance.", true)]
    public override bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            bool? result = projectileBehavior.Colliding(projHitbox, targetHitbox);
            if (result is not null)
                return result;
        }
        return null;
    }
    #endregion Hit

    #region SpecialEffects
    public override void NumGrappleHooks(Projectile projectile, Player player, ref int numHooks)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.NumGrappleHooks(player, ref numHooks);
    }

    public override void GrappleRetreatSpeed(Projectile projectile, Player player, ref float speed)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.GrappleRetreatSpeed(player, ref speed);
    }

    public override void GrapplePullSpeed(Projectile projectile, Player player, ref float speed)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.GrapplePullSpeed(player, ref speed);
    }

    public override void GrappleTargetPoint(Projectile projectile, Player player, ref float grappleX, ref float grappleY)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.GrappleTargetPoint(player, ref grappleX, ref grappleY);
    }

    public override bool? GrappleCanLatchOnTo(Projectile projectile, Player player, int x, int y)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            bool? result = projectileBehavior.GrappleCanLatchOnTo(player, x, y);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void PrepareBombToBlow(Projectile projectile)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.PrepareBombToBlow();
    }

    public override void EmitEnchantmentVisualsAt(Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.EmitEnchantmentVisualsAt(boxPosition, boxWidth, boxHeight);
    }
    #endregion SpecialEffects
}

[CriticalBehavior]
public abstract class SingleItemBehaviorHandler<TItemBehavior> : GlobalItemBehavior where TItemBehavior : SingleItemBehavior
{
    protected abstract SingleEntityBehaviorSet<Item, TItemBehavior> BehaviorSet { get; }

    public virtual bool TryGetBehavior(Item item, out TItemBehavior itemBehavior, [CallerMemberName] string methodName = null) => BehaviorSet.TryGetBehavior(item, methodName, out itemBehavior);

    #region Defaults
    public override void SetStaticDefaults()
    {
        foreach (SimpleEntityBehaviorSet<Item, TItemBehavior> simpleSet in BehaviorSet._data.Values)
        {
            foreach (TItemBehavior itemBehavior in simpleSet.Enumerate())
                itemBehavior.SetStaticDefaults();
        }
    }

    public override void SetDefaults(Item item)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.SetDefaults();
    }

    public override void AddRecipes()
    {
        foreach (SimpleEntityBehaviorSet<Item, TItemBehavior> simpleSet in BehaviorSet._data.Values)
        {
            foreach (TItemBehavior itemBehavior in simpleSet.Enumerate())
                itemBehavior.AddRecipes();
        }
    }
    #endregion Defaults

    #region Lifetime
    public override void OnCreated(Item item, ItemCreationContext context)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.OnCreated(context);
    }

    public override void OnSpawn(Item item, IEntitySource source)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.OnSpawn(source);
    }

    public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.Update(ref gravity, ref maxFallSpeed);
    }

    public override void PostUpdate(Item item)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.PostUpdate();
    }

    public override void GrabRange(Item item, Player player, ref int grabRange)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.GrabRange(player, ref grabRange);
    }

    public override bool GrabStyle(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.GrabStyle(player))
                return false;
        }
        return false;
    }

    public override bool CanPickup(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.CanPickup(player))
                return false;
        }
        return true;
    }

    public override bool OnPickup(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.OnPickup(player))
                return false;
        }
        return true;
    }

    public override bool ItemSpace(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.ItemSpace(player))
                return false;
        }
        return false;
    }
    #endregion Lifetime

    #region Update
    public override void UpdateInventory(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.UpdateInventory(player);
    }

    public override void UpdateInfoAccessory(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.UpdateInfoAccessory(player);
    }

    public override void UpdateEquip(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.UpdateEquip(player);
    }

    public override void UpdateAccessory(Item item, Player player, bool hideVisual)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.UpdateAccessory(player, hideVisual);
    }

    public override void UpdateVanity(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.UpdateVanity(player);
    }

    public override void UpdateVisibleAccessory(Item item, Player player, bool hideVisual)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.UpdateVisibleAccessory(player, hideVisual);
    }

    public override void UpdateItemDye(Item item, Player player, int dye, bool hideVisual)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.UpdateItemDye(player, dye, hideVisual);
    }
    #endregion Update

    #region Draw
    public override Color? GetAlpha(Item item, Color lightColor)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            Color? result = itemBehavior.GetAlpha(lightColor);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI))
                return false;
        }
        return true;
    }

    public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.PostDrawInWorld(spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
    }

    public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale))
                return false;
        }
        return true;
    }

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.PostDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    }
    #endregion Draw

    #region Prefix
    public override int ChoosePrefix(Item item, UnifiedRandom rand)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            int result = itemBehavior.ChoosePrefix(rand);
            if (result != -1)
                return result;
        }
        return -1;
    }

    public override bool? PrefixChance(Item item, int pre, UnifiedRandom rand)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            bool? result = itemBehavior.PrefixChance(pre, rand);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool AllowPrefix(Item item, int pre)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.AllowPrefix(pre))
                return false;
        }
        return true;
    }

    public override bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.ReforgePrice(ref reforgePrice, ref canApplyDiscount))
                return false;
        }
        return true;
    }

    public override bool CanReforge(Item item)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.CanReforge())
                return false;
        }
        return true;
    }

    public override void PreReforge(Item item)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.PreReforge();
    }

    public override void PostReforge(Item item)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.PostReforge();
    }
    #endregion Prefix

    #region Use
    public override bool AltFunctionUse(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (itemBehavior.AltFunctionUse(player))
                return true;
        }
        return false;
    }

    public override bool CanUseItem(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.CanUseItem(player))
                return false;
        }
        return true;
    }

    public override bool? CanAutoReuseItem(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            bool? result = itemBehavior.CanAutoReuseItem(player);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void UseStyle(Item item, Player player, Rectangle heldItemFrame)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.UseStyle(player, heldItemFrame);
    }

    public override void HoldStyle(Item item, Player player, Rectangle heldItemFrame)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.HoldStyle(player, heldItemFrame);
    }

    public override void HoldItem(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.HoldItem(player);
    }

    public override float UseTimeMultiplier(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            float result = itemBehavior.UseTimeMultiplier(player);
            if (result != 1f)
                return result;
        }
        return 1f;
    }

    public override float UseAnimationMultiplier(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            float result = itemBehavior.UseAnimationMultiplier(player);
            if (result != 1f)
                return result;
        }
        return 1f;
    }

    public override float UseSpeedMultiplier(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            float result = itemBehavior.UseSpeedMultiplier(player);
            if (result != 1f)
                return result;
        }
        return 1f;
    }

    public override bool? UseItem(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            bool? result = itemBehavior.UseItem(player);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void UseAnimation(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.UseAnimation(player);
    }

    public override bool CanShoot(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.CanShoot(player))
                return false;
        }
        return true;
    }

    public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.Shoot(player, source, position, velocity, type, damage, knockback))
                return false;
        }
        return true;
    }

    public override bool CanRightClick(Item item)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.CanRightClick())
                return false;
        }
        return false;
    }

    public override void RightClick(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.RightClick(player);
    }
    #endregion Use

    #region ModifyStats
    public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.ModifyWeaponDamage(player, ref damage);
    }

    public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.ModifyWeaponKnockback(player, ref knockback);
    }

    public override void ModifyWeaponCrit(Item item, Player player, ref float crit)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.ModifyWeaponCrit(player, ref crit);
    }

    public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.ModifyShootStats(player, ref position, ref velocity, ref type, ref damage, ref knockback);
    }

    public override void ModifyItemScale(Item item, Player player, ref float scale)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.ModifyItemScale(player, ref scale);
    }

    public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.ModifyItemLoot(itemLoot);
    }
    #endregion ModifyStats

    #region Hit
    [Obsolete("Poor performance.", true)]
    public override bool? CanHitNPC(Item item, Player player, NPC target)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            bool? result = itemBehavior.CanHitNPC(player, target);
            if (result is not null)
                return result;
        }
        return null;
    }

    [Obsolete("Poor performance.", true)]
    public override bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, Player player, NPC target)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            bool? result = itemBehavior.CanMeleeAttackCollideWithNPC(meleeAttackHitbox, player, target);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.ModifyHitNPC(player, target, ref modifiers);
    }

    public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.OnHitNPC(player, target, hit, damageDone);
    }

    [Obsolete("Poor performance.", true)]
    public override bool CanHitPvp(Item item, Player player, Player target)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            bool result = itemBehavior.CanHitPvp(player, target);
            if (!result)
                return false;
        }
        return true;
    }

    public override void ModifyHitPvp(Item item, Player player, Player target, ref Player.HurtModifiers modifiers)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.ModifyHitPvp(player, target, ref modifiers);
    }

    public override void OnHitPvp(Item item, Player player, Player target, Player.HurtInfo hurtInfo)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.OnHitPvp(player, target, hurtInfo);
    }
    #endregion Hit

    #region SpecialEffects
    public override void GetHealLife(Item item, Player player, bool quickHeal, ref int healValue)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.GetHealLife(player, quickHeal, ref healValue);
    }

    public override void GetHealMana(Item item, Player player, bool quickHeal, ref int healValue)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.GetHealMana(player, quickHeal, ref healValue);
    }

    public override void ModifyManaCost(Item item, Player player, ref float reduce, ref float mult)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.ModifyManaCost(player, ref reduce, ref mult);
    }

    public override void OnMissingMana(Item item, Player player, int neededMana)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.OnMissingMana(player, neededMana);
    }

    public override void OnConsumeMana(Item item, Player player, int manaConsumed)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.OnConsumeMana(player, manaConsumed);
    }

    public override void ModifyPotionDelay(Item item, Player player, ref int baseDelay)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.ModifyPotionDelay(player, ref baseDelay);
    }

    public override bool ApplyPotionDelay(Item item, Player player, int potionDelay)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.ApplyPotionDelay(player, potionDelay))
                return false;
        }
        return true;
    }

    public override bool CanResearch(Item item)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.CanResearch())
                return false;
        }
        return true;
    }

    public override void OnResearched(Item item, bool fullyResearched)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.OnResearched(fullyResearched);
    }

    public override void ModifyResearchSorting(Item item, ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.ModifyResearchSorting(ref itemGroup);
    }

    public override bool NeedsAmmo(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.NeedsAmmo(player))
                return false;
        }
        return true;
    }

    public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.UseItemHitbox(player, ref hitbox, ref noHitbox);
    }

    public override void MeleeEffects(Item item, Player player, Rectangle hitbox)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.MeleeEffects(player, hitbox);
    }

    public override bool? CanCatchNPC(Item item, NPC target, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            bool? result = itemBehavior.CanCatchNPC(target, player);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void OnCatchNPC(Item item, NPC npc, Player player, bool failed)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.OnCatchNPC(npc, player, failed);
    }

    public override bool ConsumeItem(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.ConsumeItem(player))
                return false;
        }
        return true;
    }

    public override void OnConsumeItem(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.OnConsumeItem(player);
    }

    public override void UseItemFrame(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.UseItemFrame(player);
    }

    public override void HoldItemFrame(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.HoldItemFrame(player);
    }

    public override void VerticalWingSpeeds(Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.VerticalWingSpeeds(player, ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier, ref maxAscentMultiplier, ref constantAscend);
    }

    public override void HorizontalWingSpeeds(Item item, Player player, ref float speed, ref float acceleration)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.HorizontalWingSpeeds(player, ref speed, ref acceleration);
    }

    public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.CanEquipAccessory(player, slot, modded))
                return false;
        }
        return true;
    }
    #endregion SpecialEffects

    #region Tooltip
    public override bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.PreDrawTooltip(lines, ref x, ref y))
                return false;
        }
        return true;
    }

    public override void PostDrawTooltip(Item item, ReadOnlyCollection<DrawableTooltipLine> lines)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.PostDrawTooltip(lines);
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.PreDrawTooltipLine(line, ref yOffset))
                return false;
        }
        return true;
    }

    public override void PostDrawTooltipLine(Item item, DrawableTooltipLine line)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.PostDrawTooltipLine(line);
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.ModifyTooltips(tooltips);
    }
    #endregion Tooltip

    #region WorldSaving
    public override void SaveData(Item item, TagCompound tag)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.SaveData(tag);
    }

    public override void LoadData(Item item, TagCompound tag)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.LoadData(tag);
    }
    #endregion WorldSaving

    #region Net
    public override void NetSend(Item item, BinaryWriter writer)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.NetSend(writer);
    }

    public override void NetReceive(Item item, BinaryReader reader)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.NetReceive(reader);
    }
    #endregion Net
}

#endregion Single Behavior Handler