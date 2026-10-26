using Transoceanic.GlobalInstances;

namespace Transoceanic.Behaviors;

public abstract partial class SingleItemBehavior : SingleEntityBehavior<Item>
{
    public Item Item { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _Entity; }

    public TOGlobalItem OceanItem { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _Entity.Ocean; }

    /// <inheritdoc cref="GlobalItem.AddRecipes"/>
    public virtual void AddRecipes() { }

    /// <inheritdoc cref="GlobalItem.CanPickup"/>
    public virtual bool CanPickup(Player player) => true;

    /// <inheritdoc cref="GlobalItem.CanReforge"/>
    public virtual bool CanReforge() => true;

    /// <inheritdoc cref="GlobalItem.CanUseItem"/>
    public virtual bool CanUseItem(Player player) => true;

    /// <inheritdoc cref="GlobalItem.CanAutoReuseItem"/>
    public virtual bool? CanAutoReuseItem(Player player) => null;

    /// <inheritdoc cref="GlobalItem.CanShoot"/>
    public virtual bool CanShoot(Player player) => true;

    /// <inheritdoc cref="GlobalItem.CanRightClick"/>
    public virtual bool CanRightClick() => false;

    /// <inheritdoc cref="GlobalItem.CanResearch"/>
    public virtual bool CanResearch() => true;

    /// <inheritdoc cref="GlobalItem.CanCatchNPC"/>
    public virtual bool? CanCatchNPC(NPC target, Player player) => null;

    /// <inheritdoc cref="GlobalItem.CanEquipAccessory"/>
    public virtual bool CanEquipAccessory(Player player, int slot, bool modded) => true;
}
