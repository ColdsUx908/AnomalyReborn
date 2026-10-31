using Transoceanic.GlobalInstances;

namespace Transoceanic.Behaviors;

/// <summary>
/// 按物品类型匹配的行为基类，回调前绑定当前物品。
/// 不支持 NetSend 和 NetReceive；物品网络数据由所属 ModItem 或 GlobalItem 实现。
/// </summary>
public abstract partial class SingleItemBehavior : SingleEntityBehavior<Item>
{
    /// <summary>
    /// 获取当前绑定实体的 Item 引用；行为共享此绑定，不应跨回调缓存。
    /// </summary>
    public Item Item { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Entity; }

    /// <summary>
    /// 获取当前绑定实体的 OceanItem 引用；行为共享此绑定，不应跨回调缓存。
    /// </summary>
    public TOGlobalItem OceanItem { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Entity.Ocean; }

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
