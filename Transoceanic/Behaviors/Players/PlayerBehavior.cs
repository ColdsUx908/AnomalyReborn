using Terraria.GameInput;
using Transoceanic.GlobalInstances;

namespace Transoceanic.Behaviors;

public abstract class PlayerBehavior : GeneralEntityBehavior<Player>
{
    public Player Player { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _Entity; }

    public TOPlayer OceanPlayer { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _Entity.Ocean; }

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

    /// <inheritdoc cref="ModPlayer.ModifyHitNPC"/>
    public virtual void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="ModPlayer.OnHitNPC"/>
    public virtual void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) { }

    /// <inheritdoc cref="ModPlayer.ModifyHitNPCWithItem"/>
    public virtual void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="ModPlayer.OnHitNPCWithItem"/>
    public virtual void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) { }

    /// <inheritdoc cref="ModPlayer.ModifyHitNPCWithProj"/>
    public virtual void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="ModPlayer.OnHitNPCWithProj"/>
    public virtual void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) { }

    /// <inheritdoc cref="ModPlayer.ModifyHitByNPC"/>
    public virtual void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers) { }

    /// <inheritdoc cref="ModPlayer.OnHitByNPC"/>
    public virtual void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) { }

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

    /// <inheritdoc cref="ModPlayer.TransformDrawData"/>
    public virtual void TransformDrawData(ref PlayerDrawSet drawInfo) { }

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
