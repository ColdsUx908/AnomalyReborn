namespace Transoceanic.Behaviors;

public abstract partial class GlobalItemBehavior : GlobalEntityBehavior<Item>
{
    /// <inheritdoc cref="GlobalItem.CanUseItem"/>
    public virtual bool CanUseItem(Item item, Player player) => true;

    /// <inheritdoc cref="GlobalItem.CanAutoReuseItem"/>
    public virtual bool? CanAutoReuseItem(Item item, Player player) => null;

    /// <inheritdoc cref="GlobalItem.CanConsumeBait"/>
    public virtual bool? CanConsumeBait(Player player, Item bait) => null;

    /// <inheritdoc cref="GlobalItem.CanResearch"/>
    public virtual bool CanResearch(Item item) => true;

    /// <inheritdoc cref="GlobalItem.CanChooseAmmo"/>
    public virtual bool? CanChooseAmmo(Item weapon, Item ammo, Player player) => null;

    /// <inheritdoc cref="GlobalItem.CanBeChosenAsAmmo"/>
    public virtual bool? CanBeChosenAsAmmo(Item ammo, Item weapon, Player player) => null;

    /// <inheritdoc cref="GlobalItem.CanConsumeAmmo"/>
    public virtual bool CanConsumeAmmo(Item weapon, Item ammo, Player player) => true;

    /// <inheritdoc cref="GlobalItem.CanBeConsumedAsAmmo"/>
    public virtual bool CanBeConsumedAsAmmo(Item ammo, Item weapon, Player player) => true;

    /// <inheritdoc cref="GlobalItem.CanShoot"/>
    public virtual bool CanShoot(Item item, Player player) => true;

    /// <inheritdoc cref="GlobalItem.CanCatchNPC"/>
    public virtual bool? CanCatchNPC(Item item, NPC target, Player player) => null;

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

    /// <inheritdoc cref="GlobalItem.CanStack"/>
    public virtual bool CanStack(Item destination, Item source) => true;

    /// <inheritdoc cref="GlobalItem.CanStackInWorld"/>
    public virtual bool CanStackInWorld(Item destination, Item source) => true;

    /// <inheritdoc cref="GlobalItem.CanReforge"/>
    public virtual bool CanReforge(Item item) => true;

    /// <inheritdoc cref="GlobalItem.DrawArmorColor"/>
    public virtual void DrawArmorColor(EquipType type, int slot, Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) { }

    /// <inheritdoc cref="GlobalItem.ArmorArmGlowMask"/>
    public virtual void ArmorArmGlowMask(int slot, Player drawPlayer, float shadow, ref int glowMask, ref Color color) { }

    /// <inheritdoc cref="GlobalItem.WingUpdate"/>
    public virtual bool WingUpdate(int wings, Player player, bool inUse) => false;

    /// <inheritdoc cref="GlobalItem.CanPickup"/>
    public virtual bool CanPickup(Item item, Player player) => true;

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
}
