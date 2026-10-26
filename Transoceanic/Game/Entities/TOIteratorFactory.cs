namespace Transoceanic.Game.Entities;

/// <summary>
/// 提供创建针对 Terraria 各类实体（NPC、玩家、射弹等）的迭代器的工厂方法。
/// </summary>
/// <remarks>
/// <para>该工厂类封装了从 Terraria 主数组（如 <c>Main.npc</c>）获取 <see cref="ReadOnlySpan{T}"/> 的逻辑，
/// 并预置了常见的筛选组合（例如仅活动实体）。使用这些方法可以简化迭代器实例的构造，
/// 使代码更加清晰易读。</para>
/// <para>所有返回的迭代器均为 <see langword="ref struct"/> 类型，需直接在方法作用域内使用，
/// 无法作为类成员存储。</para>
/// </remarks>
public static class TOIteratorFactory
{
    /// <summary>
    /// 获取包含当前游戏世界中所有 NPC 的只读范围。
    /// </summary>
    public static ReadOnlySpan<NPC> NPCSpan => Main.npc.AsSpan(0, Main.maxNPCs);

    /// <summary>
    /// 获取包含当前游戏世界中所有射弹的只读范围。
    /// </summary>
    public static ReadOnlySpan<Projectile> ProjectileSpan => Main.projectile.AsSpan(0, Main.maxProjectiles);

    /// <summary>
    /// 获取包含当前游戏世界中所有玩家的只读范围。
    /// 在单人模式下仅包含索引 0 的玩家；在多人模式下包含全部 <see cref="Main.maxPlayers"/> 个玩家槽位。
    /// </summary>
    public static ReadOnlySpan<Player> PlayerSpan => Main.player.AsSpan(0, Main.netMode != NetmodeID.SinglePlayer ? Main.maxPlayers : 1);

    /// <summary>
    /// 获取包含当前游戏世界中所有物品的只读范围。
    /// </summary>
    public static ReadOnlySpan<Item> ItemSpan => Main.item.AsSpan(0, Main.maxItems);

    /// <summary>
    /// 获取包含当前游戏世界中所有尘埃粒子的只读范围。
    /// </summary>
    public static ReadOnlySpan<Dust> DustSpan => Main.dust.AsSpan(0, Main.maxDust);

    /// <summary>
    /// 创建一个新的 NPC 迭代器，允许自定义筛选条件。
    /// </summary>
    /// <param name="match">用于筛选 NPC 的谓词委托。</param>
    /// <returns>一个 <see cref="TOIterator{NPC}"/> 实例，可迭代满足条件的 NPC。</returns>
    public static TOIterator<NPC> NewNPCIterator(Func<NPC, bool> match) => new(NPCSpan, match);

    /// <summary>
    /// 创建一个带排除列表的 NPC 迭代器，允许自定义筛选条件并排除指定实例。
    /// </summary>
    /// <param name="match">用于筛选 NPC 的谓词委托。</param>
    /// <param name="exclusions">应被忽略的 NPC 实例的哈希集合。</param>
    /// <returns>一个 <see cref="TOExclusiveIterator{NPC}"/> 实例，可迭代满足条件且未被排除的 NPC。</returns>
    public static TOExclusiveIterator<NPC> NewNPCIterator(Func<NPC, bool> match, params HashSet<NPC> exclusions) => new(NPCSpan, match, exclusions);

    /// <summary>
    /// 创建一个仅迭代活动 NPC 的迭代器，并允许附加自定义筛选条件。
    /// </summary>
    /// <param name="match">附加的筛选谓词，将在 NPC 活动状态检查后应用。</param>
    /// <returns>一个 <see cref="TOIterator{NPC}"/> 实例，可迭代活动且满足条件的 NPC。</returns>
    public static TOIterator<NPC> NewActiveNPCIterator(Func<NPC, bool> match) => new(NPCSpan, n => n.active && match(n));

    /// <summary>
    /// 创建一个带排除列表的活动 NPC 迭代器，允许自定义筛选条件并排除指定实例。
    /// </summary>
    /// <param name="match">附加的筛选谓词，将在 NPC 活动状态检查后应用。</param>
    /// <param name="exclusions">应被忽略的 NPC 实例的哈希集合。</param>
    /// <returns>一个 <see cref="TOExclusiveIterator{NPC}"/> 实例，可迭代活动、满足条件且未被排除的 NPC。</returns>
    public static TOExclusiveIterator<NPC> NewActiveNPCIterator(Func<NPC, bool> match, params HashSet<NPC> exclusions) => new(NPCSpan, n => n.active && match(n), exclusions);

    /// <summary>
    /// 创建一个新的射弹迭代器，允许自定义筛选条件。
    /// </summary>
    /// <param name="match">用于筛选射弹的谓词委托。</param>
    /// <returns>一个 <see cref="TOIterator{Projectile}"/> 实例，可迭代满足条件的射弹。</returns>
    public static TOIterator<Projectile> NewProjectileIterator(Func<Projectile, bool> match) => new(ProjectileSpan, match);

    /// <summary>
    /// 创建一个带排除列表的射弹迭代器，允许自定义筛选条件并排除指定实例。
    /// </summary>
    /// <param name="match">用于筛选射弹的谓词委托。</param>
    /// <param name="exclusions">应被忽略的射弹实例的哈希集合。</param>
    /// <returns>一个 <see cref="TOExclusiveIterator{Projectile}"/> 实例，可迭代满足条件且未被排除的射弹。</returns>
    public static TOExclusiveIterator<Projectile> NewProjectileIterator(Func<Projectile, bool> match, params HashSet<Projectile> exclusions) => new(ProjectileSpan, match, exclusions);

    /// <summary>
    /// 创建一个仅迭代活动射弹的迭代器，并允许附加自定义筛选条件。
    /// </summary>
    /// <param name="match">附加的筛选谓词，将在射弹活动状态检查后应用。</param>
    /// <returns>一个 <see cref="TOIterator{Projectile}"/> 实例，可迭代活动且满足条件的射弹。</returns>
    public static TOIterator<Projectile> NewActiveProjectileIterator(Func<Projectile, bool> match) => new(ProjectileSpan, p => p.active && match(p));

    /// <summary>
    /// 创建一个带排除列表的活动射弹迭代器，允许自定义筛选条件并排除指定实例。
    /// </summary>
    /// <param name="match">附加的筛选谓词，将在射弹活动状态检查后应用。</param>
    /// <param name="exclusions">应被忽略的射弹实例的哈希集合。</param>
    /// <returns>一个 <see cref="TOExclusiveIterator{Projectile}"/> 实例，可迭代活动、满足条件且未被排除的射弹。</returns>
    public static TOExclusiveIterator<Projectile> NewActiveProjectileIterator(Func<Projectile, bool> match, params HashSet<Projectile> exclusions) => new(ProjectileSpan, p => p.active && match(p), exclusions);

    /// <summary>
    /// 创建一个新的玩家迭代器，允许自定义筛选条件。
    /// </summary>
    /// <param name="match">用于筛选玩家的谓词委托。</param>
    /// <returns>一个 <see cref="TOIterator{Player}"/> 实例，可迭代满足条件的玩家。</returns>
    public static TOIterator<Player> NewPlayerIterator(Func<Player, bool> match) => new(PlayerSpan, match);

    /// <summary>
    /// 创建一个带排除列表的玩家迭代器，允许自定义筛选条件并排除指定实例。
    /// </summary>
    /// <param name="match">用于筛选玩家的谓词委托。</param>
    /// <param name="exclusions">应被忽略的玩家实例的哈希集合。</param>
    /// <returns>一个 <see cref="TOExclusiveIterator{Player}"/> 实例，可迭代满足条件且未被排除的玩家。</returns>
    public static TOExclusiveIterator<Player> NewPlayerIterator(Func<Player, bool> match, params HashSet<Player> exclusions) => new(PlayerSpan, match, exclusions);

    /// <summary>
    /// 创建一个仅迭代活动玩家的迭代器，并允许附加自定义筛选条件。
    /// </summary>
    /// <param name="match">附加的筛选谓词，将在玩家活动状态检查后应用。</param>
    /// <returns>一个 <see cref="TOIterator{Player}"/> 实例，可迭代活动且满足条件的玩家。</returns>
    public static TOIterator<Player> NewActivePlayerIterator(Func<Player, bool> match) => new(PlayerSpan, p => p.active && match(p));

    /// <summary>
    /// 创建一个带排除列表的活动玩家迭代器，允许自定义筛选条件并排除指定实例。
    /// </summary>
    /// <param name="match">附加的筛选谓词，将在玩家活动状态检查后应用。</param>
    /// <param name="exclusions">应被忽略的玩家实例的哈希集合。</param>
    /// <returns>一个 <see cref="TOExclusiveIterator{Player}"/> 实例，可迭代活动、满足条件且未被排除的玩家。</returns>
    public static TOExclusiveIterator<Player> NewActivePlayerIterator(Func<Player, bool> match, params HashSet<Player> exclusions) => new(PlayerSpan, p => p.active && match(p), exclusions);

    /// <summary>
    /// 创建一个新的尘埃粒子迭代器，允许自定义筛选条件。
    /// </summary>
    /// <param name="match">用于筛选尘埃粒子的谓词委托。</param>
    /// <returns>一个 <see cref="TOIterator{Dust}"/> 实例，可迭代满足条件的尘埃粒子。</returns>
    public static TOIterator<Dust> NewDustIterator(Func<Dust, bool> match) => new(DustSpan, match);

    /// <summary>
    /// 创建一个带排除列表的尘埃粒子迭代器，允许自定义筛选条件并排除指定实例。
    /// </summary>
    /// <param name="match">用于筛选尘埃粒子的谓词委托。</param>
    /// <param name="exclusions">应被忽略的尘埃粒子实例的哈希集合。</param>
    /// <returns>一个 <see cref="TOExclusiveIterator{Dust}"/> 实例，可迭代满足条件且未被排除的尘埃粒子。</returns>
    public static TOExclusiveIterator<Dust> NewDustIterator(Func<Dust, bool> match, params HashSet<Dust> exclusions) => new(DustSpan, match, exclusions);

    /// <summary>
    /// 创建一个新的物品迭代器，允许自定义筛选条件。
    /// </summary>
    /// <param name="match">用于筛选物品的谓词委托。</param>
    /// <returns>一个 <see cref="TOIterator{Item}"/> 实例，可迭代满足条件的物品。</returns>
    public static TOIterator<Item> NewItemIterator(Func<Item, bool> match) => new(ItemSpan, match);

    /// <summary>
    /// 创建一个带排除列表的物品迭代器，允许自定义筛选条件并排除指定实例。
    /// </summary>
    /// <param name="match">用于筛选物品的谓词委托。</param>
    /// <param name="exclusions">应被忽略的物品实例的哈希集合。</param>
    /// <returns>一个 <see cref="TOExclusiveIterator{Item}"/> 实例，可迭代满足条件且未被排除的物品。</returns>
    public static TOExclusiveIterator<Item> NewItemIterator(Func<Item, bool> match, params HashSet<Item> exclusions) => new(ItemSpan, match, exclusions);

    /// <summary>
    /// 创建一个仅迭代活动物品（存在于世界中且可被拾取）的迭代器，并允许附加自定义筛选条件。
    /// </summary>
    /// <param name="match">附加的筛选谓词，将在物品活动状态检查后应用。</param>
    /// <returns>一个 <see cref="TOIterator{Item}"/> 实例，可迭代活动且满足条件的物品。</returns>
    public static TOIterator<Item> NewActiveItemIterator(Func<Item, bool> match) => new(ItemSpan, i => i.active && match(i));

    /// <summary>
    /// 创建一个带排除列表的活动物品迭代器，允许自定义筛选条件并排除指定实例。
    /// </summary>
    /// <param name="match">附加的筛选谓词，将在物品活动状态检查后应用。</param>
    /// <param name="exclusions">应被忽略的物品实例的哈希集合。</param>
    /// <returns>一个 <see cref="TOExclusiveIterator{Item}"/> 实例，可迭代活动、满足条件且未被排除的物品。</returns>
    public static TOExclusiveIterator<Item> NewActiveItemIterator(Func<Item, bool> match, params HashSet<Item> exclusions) => new(ItemSpan, i => i.active && match(i), exclusions);
}

