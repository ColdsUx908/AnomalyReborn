namespace Transoceanic.DataStructures;

/// <summary>
/// 提供对一组指定类型对象的延迟、条件过滤迭代功能。
/// </summary>
/// <typeparam name="T">要迭代的对象类型，必须为引用类型（<see langword="class"/>）。</typeparam>
/// <remarks>
/// <para><see cref="TOIterator{T}"/> 是一个基于 <see cref="ReadOnlySpan{T}"/> 的轻量级迭代结构，
/// 主要用于在遍历大型集合（例如 Terraria 中的实体数组）时动态应用筛选条件，避免创建额外的临时列表。
/// 迭代过程中仅返回满足指定谓词的元素，且延迟执行，直至真正开始迭代。</para>
/// <para>该类型为 <see langword="ref struct"/>，确保其仅存在于栈上，不可被装箱或存储在堆中，
/// 从而保证了高性能和内存安全。适合在游戏更新循环等性能敏感场景下使用。</para>
/// <example>
/// <code>
/// foreach (NPC kingSlime in TOIteratorFactory.NewActiveIterator(n => n.type = NPCID.KingSlime))
/// {
///     //代码
/// }
/// </code>
/// </example>
/// </remarks>
public readonly ref struct TOIterator<T> where T : class
{
    /// <summary>
    /// 获取一个始终返回 <see langword="true"/> 的谓词，表示匹配所有元素。
    /// </summary>
    public static Func<T, bool> MatchAll => _ => true;

    /// <summary>
    /// 获取一个始终返回 <see langword="false"/> 的谓词，表示不匹配任何元素。
    /// </summary>
    public static Func<T, bool> MatchNone => _ => false;

    /// <summary>
    /// 获取一个空的 <see cref="TOIterator{T}"/> 实例，迭代该实例将不会产生任何元素。
    /// </summary>
    public static TOIterator<T> Empty => new([], MatchNone);

    private readonly ReadOnlySpan<T> _span;
    private readonly Func<T, bool> _match;

    /// <summary>
    /// 使用指定的源范围与筛选条件初始化 <see cref="TOIterator{T}"/> 的新实例。
    /// </summary>
    /// <param name="span">包含待迭代元素的源范围。</param>
    /// <param name="match">用于筛选元素的谓词委托。仅当该委托返回 <see langword="true"/> 时元素才会被迭代器返回。</param>
    public TOIterator(ReadOnlySpan<T> span, Func<T, bool> match)
    {
        _span = span;
        _match = match;
    }

    /// <summary>
    /// 返回一个可用于在 <see langword="foreach"/> 循环中迭代本实例的枚举器。
    /// </summary>
    /// <returns>一个 <see cref="Enumerator"/> 结构，用于遍历满足筛选条件的元素。</returns>
    public Enumerator GetEnumerator() => new(this);

    /// <summary>
    /// 为 <see cref="TOIterator{T}"/> 提供迭代逻辑的枚举器结构。
    /// </summary>
    public ref struct Enumerator
    {
        private ReadOnlySpan<T>.Enumerator _enumerator;
        private readonly Func<T, bool> _match;

        /// <summary>
        /// 初始化 <see cref="Enumerator"/> 的新实例。
        /// </summary>
        /// <param name="iterator">父级 <see cref="TOIterator{T}"/> 实例，包含源数据与筛选条件。</param>
        public Enumerator(TOIterator<T> iterator)
        {
            _enumerator = iterator._span.GetEnumerator();
            _match = iterator._match;
        }

        /// <summary>
        /// 获取当前迭代位置的元素（只读引用）。
        /// </summary>
        public ref readonly T Current => ref _enumerator.Current;

        /// <summary>
        /// 将枚举器推进到下一个满足筛选条件的元素。
        /// </summary>
        /// <returns>若成功推进到符合条件的元素，则为 <see langword="true"/>；若已到达源范围末尾，则为 <see langword="false"/>。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                if (_match(Current))
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// 获取迭代器中指定索引处的元素。
    /// </summary>
    /// <param name="index">要获取的元素的从零开始的索引，该索引相对于迭代器产生的序列顺序。</param>
    /// <returns>位于指定索引处的元素；若索引超出有效范围，则返回 <see langword="null"/>。</returns>
    /// <remarks>
    /// <para>索引器会遍历迭代器序列，效率为 O(n)，因此不适用于频繁访问。
    /// 若需要随机访问，请先调用 <see cref="ToList"/> 或 <see cref="ToArray"/> 方法转换为具体集合。</para>
    /// </remarks>
    private T this[int index]
    {
        get
        {
            int i = 0;
            foreach (T data in this)
            {
                if (i++ == index)
                    return data;
            }
            return null;
        }
    }

    /// <summary>
    /// 确定迭代器序列中是否包含任何元素。
    /// </summary>
    /// <returns>若序列中至少存在一个元素，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public bool Any()
    {
        foreach (T _ in this)
            return true;
        return false;
    }

    /// <summary>
    /// 确定迭代器序列中是否存在任何元素满足附加的指定条件。
    /// </summary>
    /// <param name="furtherMatch">附加的筛选谓词，用于对已通过主筛选的元素进行二次判断。</param>
    /// <returns>若存在至少一个元素同时满足主筛选条件与附加条件，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public bool Any(Func<T, bool> furtherMatch)
    {
        foreach (T data in this)
        {
            if (furtherMatch(data))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 确定迭代器序列中的所有元素是否都满足附加的指定条件。
    /// </summary>
    /// <param name="furtherMatch">附加的筛选谓词，用于检验每个已通过主筛选的元素。</param>
    /// <returns>若序列中的所有元素均满足附加条件（或序列为空），则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public bool All(Func<T, bool> furtherMatch)
    {
        foreach (T data in this)
        {
            if (!furtherMatch(data))
                return false;
        }
        return true;
    }

    /// <summary>
    /// 计算迭代器序列中的元素总数。
    /// </summary>
    /// <returns>序列中元素的数量。</returns>
    public int Count()
    {
        int count = 0;
        foreach (T _ in this)
            count++;
        return count;
    }

    /// <summary>
    /// 计算迭代器序列中同时满足附加指定条件的元素数量。
    /// </summary>
    /// <param name="furtherMatch">附加的筛选谓词，仅当元素满足此条件时才计入计数。</param>
    /// <returns>满足附加条件的元素数量。</returns>
    public int Count(Func<T, bool> furtherMatch)
    {
        int count = 0;
        foreach (T data in this)
        {
            if (furtherMatch(data))
                count++;
        }
        return count;
    }

    /// <summary>
    /// 将迭代器序列中的元素复制到新 <see cref="List{T}"/> 中。
    /// </summary>
    /// <returns>包含序列中所有元素的 <see cref="List{T}"/>。</returns>
    public List<T> ToList() => [.. this];

    /// <summary>
    /// 将迭代器序列中的元素复制到新数组中。
    /// </summary>
    /// <returns>包含序列中所有元素的数组。</returns>
    public T[] ToArray() => [.. this];

    /// <summary>
    /// 尝试获取序列中的第一个元素。
    /// </summary>
    /// <param name="found">当此方法返回时，包含序列中的第一个元素；若序列为空，则为 <see langword="null"/>。</param>
    /// <returns>若序列非空并成功获取元素，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public bool TryGetFirst(out T found) => (found = this[0]) is not null;

    /// <summary>
    /// 尝试获取序列中第一个同时满足附加指定条件的元素。
    /// </summary>
    /// <param name="furtherMatch">附加的筛选谓词，用于定位目标元素。</param>
    /// <param name="found">当此方法返回时，包含找到的第一个同时满足主筛选与附加条件的元素；若未找到，则为 <see langword="null"/>。</param>
    /// <returns>若找到符合条件的元素，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public bool TryGetFirst(Func<T, bool> furtherMatch, out T found)
    {
        foreach (T data in this)
        {
            if (furtherMatch(data))
            {
                found = data;
                return true;
            }
        }
        found = null;
        return false;
    }
}

/// <summary>
/// 提供带排除对象集合的延迟、条件过滤迭代功能。
/// </summary>
/// <typeparam name="T">要迭代的对象类型，必须为引用类型（<see langword="class"/>）。</typeparam>
/// <remarks>
/// <para><see cref="TOExclusiveIterator{T}"/> 在 <see cref="TOIterator{T}"/> 的基础上增加了排除列表功能，
/// 可以在迭代过程中自动跳过指定的对象实例。这对于需要遍历所有满足条件的实体，但故意忽略某些特定实例的场景非常有用
/// （例如，避免对触发当前逻辑的实体自身产生影响）。</para>
/// <para>该类型 <see langword="ref struct"/>，确保高效且安全的栈分配行为。</para>
/// <example>
/// <code>
/// public override void AI(NPC npc) //GlobalNPC类的重写方法
/// {
///     foreach (NPC kingSlime in TOIteratorFactory.NewActiveNPCIterator(n => n.type == NPCID.KingSlime), npc)
///     {
///         //代码
///     }
/// }
/// </code>
/// </example>
/// </remarks>
public readonly ref struct TOExclusiveIterator<T> where T : class
{
    /// <summary>
    /// 获取一个始终返回 <see langword="true"/> 的谓词，表示匹配所有元素。
    /// </summary>
    public static Func<T, bool> MatchAll => _ => true;

    /// <summary>
    /// 获取一个始终返回 <see langword="false"/> 的谓词，表示不匹配任何元素。
    /// </summary>
    public static Func<T, bool> MatchNone => _ => false;

    /// <summary>
    /// 获取一个空的 <see cref="TOExclusiveIterator{T}"/> 实例，迭代该实例将不会产生任何元素。
    /// </summary>
    public static TOExclusiveIterator<T> Empty => new([], MatchNone, new HashSet<T>());

    private readonly ReadOnlySpan<T> _span;
    private readonly Func<T, bool> _match;
    private readonly HashSet<T> _exclusions;

    /// <summary>
    /// 使用指定的源范围、筛选条件与排除集合初始化 <see cref="TOExclusiveIterator{T}"/> 的新实例。
    /// </summary>
    /// <param name="span">包含待迭代元素的源范围。</param>
    /// <param name="match">用于筛选元素的谓词委托。仅当该委托返回 <see langword="true"/> 时元素才会进入后续判断。</param>
    /// <param name="exceptions">包含应被忽略的对象实例的哈希集合。集合中的对象将不会出现在迭代结果中。</param>
    public TOExclusiveIterator(ReadOnlySpan<T> span, Func<T, bool> match, HashSet<T> exceptions)
    {
        _span = span;
        _match = match;
        _exclusions = exceptions;
    }

    /// <summary>
    /// 使用指定的源范围、筛选条件与排除对象列表初始化 <see cref="TOExclusiveIterator{T}"/> 的新实例。
    /// </summary>
    /// <param name="span">包含待迭代元素的源范围。</param>
    /// <param name="match">用于筛选元素的谓词委托。仅当该委托返回 <see langword="true"/> 时元素才会进入后续判断。</param>
    /// <param name="exceptions">应被忽略的对象实例列表。这些对象将不会出现在迭代结果中。</param>
    public TOExclusiveIterator(ReadOnlySpan<T> span, Func<T, bool> match, params ReadOnlySpan<T> exceptions)
    {
        _span = span;
        _match = match;
        _exclusions = [.. exceptions];
    }

    /// <summary>
    /// 返回一个可用于在 <see langword="foreach"/> 循环中迭代本实例的枚举器。
    /// </summary>
    /// <returns>一个 <see cref="Enumerator"/> 结构，用于遍历满足筛选条件且未被排除的元素。</returns>
    public Enumerator GetEnumerator() => new(this);

    /// <summary>
    /// 为 <see cref="TOExclusiveIterator{T}"/> 提供迭代逻辑的枚举器结构。
    /// </summary>
    public ref struct Enumerator
    {
        private ReadOnlySpan<T>.Enumerator _enumerator;
        private readonly Func<T, bool> _match;
        private readonly HashSet<T> _exceptions;

        /// <summary>
        /// 初始化 <see cref="Enumerator"/> 的新实例。
        /// </summary>
        /// <param name="iterator">父级 <see cref="TOExclusiveIterator{T}"/> 实例，包含源数据、筛选条件及排除列表。</param>
        public Enumerator(TOExclusiveIterator<T> iterator)
        {
            _enumerator = iterator._span.GetEnumerator();
            _match = iterator._match;
            _exceptions = iterator._exclusions;
        }

        /// <summary>
        /// 获取当前迭代位置的元素（只读引用）。
        /// </summary>
        public ref readonly T Current => ref _enumerator.Current;

        /// <summary>
        /// 将枚举器推进到下一个满足筛选条件且未被排除的元素。
        /// </summary>
        /// <returns>若成功推进到符合条件的元素，则为 <see langword="true"/>；若已到达源范围末尾，则为 <see langword="false"/>。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                if (_match(Current) && !_exceptions.Contains(Current))
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// 获取迭代器中指定索引处的元素。
    /// </summary>
    /// <param name="index">要获取的元素的从零开始的索引，该索引相对于迭代器产生的序列顺序。</param>
    /// <returns>位于指定索引处的元素；若索引超出有效范围，则返回 <see langword="null"/>。</returns>
    /// <remarks>
    /// 索引器会遍历整个迭代序列，效率为 O(n)。频繁访问时请考虑转换为集合。
    /// </remarks>
    private T this[int index]
    {
        get
        {
            int i = 0;
            foreach (T data in this)
            {
                if (i++ == index)
                    return data;
            }
            return null;
        }
    }

    /// <summary>
    /// 确定迭代器序列中是否包含任何元素（排除项已被忽略）。
    /// </summary>
    /// <returns>若序列中至少存在一个元素，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public bool Any()
    {
        foreach (T _ in this)
            return true;
        return false;
    }

    /// <summary>
    /// 确定迭代器序列中是否存在任何元素同时满足附加的指定条件。
    /// </summary>
    /// <param name="furtherMatch">附加的筛选谓词，用于对已通过主筛选且未被排除的元素进行二次判断。</param>
    /// <returns>若存在至少一个符合条件的元素，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public bool Any(Func<T, bool> furtherMatch)
    {
        foreach (T data in this)
        {
            if (furtherMatch(data))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 确定迭代器序列中的所有元素是否都满足附加的指定条件。
    /// </summary>
    /// <param name="furtherMatch">附加的筛选谓词，用于检验每个已通过主筛选且未被排除的元素。</param>
    /// <returns>若序列中的所有元素均满足附加条件（或序列为空），则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public bool All(Func<T, bool> furtherMatch)
    {
        foreach (T data in this)
        {
            if (!furtherMatch(data))
                return false;
        }
        return true;
    }

    /// <summary>
    /// 计算迭代器序列中未被排除的元素总数。
    /// </summary>
    /// <returns>序列中元素的数量。</returns>
    public int Count()
    {
        int count = 0;
        foreach (T _ in this)
            count++;
        return count;
    }

    /// <summary>
    /// 计算迭代器序列中同时满足附加指定条件的元素数量。
    /// </summary>
    /// <param name="furtherMatch">附加的筛选谓词，仅当元素满足此条件时才计入计数。</param>
    /// <returns>满足附加条件的元素数量。</returns>
    public int Count(Func<T, bool> furtherMatch)
    {
        int count = 0;
        foreach (T data in this)
        {
            if (furtherMatch(data))
                count++;
        }
        return count;
    }

    /// <summary>
    /// 将迭代器序列中的元素复制到新 <see cref="List{T}"/> 中。
    /// </summary>
    /// <returns>包含序列中所有元素的 <see cref="List{T}"/>。</returns>
    public List<T> ToList() => [.. this];

    /// <summary>
    /// 将迭代器序列中的元素复制到新数组中。
    /// </summary>
    /// <returns>包含序列中所有元素的数组。</returns>
    public T[] ToArray() => [.. this];

    /// <summary>
    /// 尝试获取序列中的第一个元素。
    /// </summary>
    /// <param name="found">当此方法返回时，包含序列中的第一个元素；若序列为空，则为 <see langword="null"/>。</param>
    /// <returns>若序列非空并成功获取元素，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public bool TryGetFirst(out T found) => (found = this[0]) is not null;
}

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
    public static readonly Func<Item, bool> Item_IsActive = i => i.active;

    /// <summary>
    /// 检查玩家是否处于活动状态。
    /// </summary>
    public static readonly Func<Player, bool> Player_IsActive = p => p.active;

    /// <summary>
    /// 检查玩家是否存活（生命值大于 0 且未被标记为死亡）。
    /// </summary>
    public static readonly Func<Player, bool> Player_IsAlive = p => p.Alive;

    /// <summary>
    /// 检查玩家是否开启了 PvP 模式。
    /// </summary>
    public static readonly Func<Player, bool> Player_IsPVP = p => p.IsPvP;

    /// <summary>
    /// 检查射弹是否处于活动状态。
    /// </summary>
    public static readonly Func<Projectile, bool> Projectile_IsActive = p => p.active;

    /// <summary>
    /// 检查 NPC 是否处于活动状态。
    /// </summary>
    public static readonly Func<NPC, bool> NPC_IsActive = n => n.active;

    /// <summary>
    /// 检查 NPC 是否为敌对生物。
    /// </summary>
    public static readonly Func<NPC, bool> NPC_IsEnemy = n => n.IsEnemy;

    /// <summary>
    /// 检查 NPC 是否为 Boss 级别的敌对生物。
    /// </summary>
    public static readonly Func<NPC, bool> NPC_IsBossEnemy = n => n.IsBossEnemy;
}