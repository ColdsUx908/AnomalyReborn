namespace Transoceanic.Game.Entities;

/// <summary>
/// 提供一系列预定义的常用谓词委托，用于快速构建迭代器的筛选条件。
/// </summary>
/// <remarks>
/// 该类中的静态只读字段封装了对 Terraria 实体状态的常用检查逻辑，
/// 可以直接作为参数传递给迭代器工厂方法或作为附加条件使用，避免重复编写简单的 lambda 表达式。
/// </remarks>
public static class IteratorMatches
{
    /// <summary>
    /// 检查物品是否处于活动状态（即可被玩家看见并拾取）。
    /// </summary>
    public static readonly Func<Item, bool> ItemIsActive = i => i.active;

    /// <summary>
    /// 检查玩家是否处于活动状态。
    /// </summary>
    public static readonly Func<Player, bool> PlayerIsActive = p => p.active;

    /// <summary>
    /// 检查玩家是否存活（生命值大于 0 且未被标记为死亡）。
    /// </summary>
    public static readonly Func<Player, bool> PlayerIsAlive = p => p.Alive;

    /// <summary>
    /// 检查玩家是否开启了 PvP 模式。
    /// </summary>
    public static readonly Func<Player, bool> PlayerIsPVP = p => p.IsPvP;

    /// <summary>
    /// 检查射弹是否处于活动状态。
    /// </summary>
    public static readonly Func<Projectile, bool> ProjectileIsActive = p => p.active;

    /// <summary>
    /// 检查 NPC 是否处于活动状态。
    /// </summary>
    public static readonly Func<NPC, bool> NPCIsActive = n => n.active;

    /// <summary>
    /// 检查 NPC 是否为敌对生物。
    /// </summary>
    public static readonly Func<NPC, bool> NPCIsEnemy = n => n.IsEnemy;

    /// <summary>
    /// 检查 NPC 是否为 Boss 级别的敌对生物。
    /// </summary>
    public static readonly Func<NPC, bool> NPCIsBossEnemy = n => n.IsBossEnemy;
}
