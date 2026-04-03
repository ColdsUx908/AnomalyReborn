namespace Transoceanic.DataStructures;

/// <summary>
/// 对象迭代器。
/// <para/>示例：
/// <code>
/// foreach (NPC kingSlime in TOIteratorFactory.NewActiveIterator(n => n.type = NPCID.KingSlime))
/// {
///     //代码
/// }
/// </code>
/// </summary>
public readonly ref struct TOIterator<T> where T : class
{
    public static Func<T, bool> MatchAll => _ => true;
    public static Func<T, bool> MatchNone => _ => false;
    public static TOIterator<T> Empty => new([], MatchNone);

    private readonly ReadOnlySpan<T> _span;
    private readonly Func<T, bool> _match;

    public TOIterator(ReadOnlySpan<T> span, Func<T, bool> match)
    {
        _span = span;
        _match = match;
    }

    public Enumerator GetEnumerator() => new(this);

    public ref struct Enumerator
    {
        private ReadOnlySpan<T>.Enumerator _enumerator;
        private readonly Func<T, bool> _match;

        public Enumerator(TOIterator<T> iterator)
        {
            _enumerator = iterator._span.GetEnumerator();
            _match = iterator._match;
        }

        public ref readonly T Current => ref _enumerator.Current;

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

    public bool Any()
    {
        foreach (T _ in this)
            return true;
        return false;
    }

    public bool Any(Func<T, bool> furtherMatch)
    {
        foreach (T data in this)
        {
            if (furtherMatch(data))
                return true;
        }
        return false;
    }

    public bool All(Func<T, bool> furtherMatch)
    {
        foreach (T data in this)
        {
            if (!furtherMatch(data))
                return false;
        }
        return true;
    }

    public int Count()
    {
        int count = 0;
        foreach (T _ in this)
            count++;
        return count;
    }

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

    public List<T> ToList() => [.. this];

    public T[] ToArray() => [.. this];

    public bool TryGetFirst(out T found) => (found = this[0]) is not null;

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
/// 含排除对象的对象迭代器。
/// <para/>示例：
/// <code>
/// public override void AI(NPC npc) //GlobalNPC类的重写方法
/// {
///     foreach (NPC kingSlime in TOIteratorFactory.NewActiveNPCIterator(n => n.type == NPCID.KingSlime), npc)
///     {
///         //代码
///     }
/// }
/// </code>
/// </summary>
public readonly ref struct TOExclusiveIterator<T> where T : class
{
    public static Func<T, bool> MatchAll => _ => true;
    public static Func<T, bool> MatchNone => _ => false;
    public static TOExclusiveIterator<T> Empty => new([], MatchNone, new HashSet<T>());

    private readonly ReadOnlySpan<T> _span;
    private readonly Func<T, bool> _match;
    private readonly HashSet<T> _exclusions;

    public TOExclusiveIterator(ReadOnlySpan<T> span, Func<T, bool> match, HashSet<T> exceptions)
    {
        _span = span;
        _match = match;
        _exclusions = exceptions;
    }

    public TOExclusiveIterator(ReadOnlySpan<T> span, Func<T, bool> match, params ReadOnlySpan<T> exceptions)
    {
        _span = span;
        _match = match;
        _exclusions = [.. exceptions];
    }

    public Enumerator GetEnumerator() => new(this);

    public ref struct Enumerator
    {
        private ReadOnlySpan<T>.Enumerator _enumerator;
        private readonly Func<T, bool> _match;
        private readonly HashSet<T> _exceptions;

        public Enumerator(TOExclusiveIterator<T> iterator)
        {
            _enumerator = iterator._span.GetEnumerator();
            _match = iterator._match;
            _exceptions = iterator._exclusions;
        }

        public ref readonly T Current => ref _enumerator.Current;

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

    public bool Any()
    {
        foreach (T _ in this)
            return true;
        return false;
    }

    public bool Any(Func<T, bool> furtherMatch)
    {
        foreach (T data in this)
        {
            if (furtherMatch(data))
                return true;
        }
        return false;
    }

    public bool All(Func<T, bool> furtherMatch)
    {
        foreach (T data in this)
        {
            if (!furtherMatch(data))
                return false;
        }
        return true;
    }

    public int Count()
    {
        int count = 0;
        foreach (T _ in this)
            count++;
        return count;
    }

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

    public List<T> ToList() => [.. this];

    public T[] ToArray() => [.. this];

    public bool TryGetFirst(out T found) => (found = this[0]) is not null;
}

/// <summary>
/// 对象迭代器工厂。
/// </summary>
public static class TOIteratorFactory
{
    public static ReadOnlySpan<NPC> NPCSpan => Main.npc.AsSpan(0, Main.maxNPCs);

    public static ReadOnlySpan<Projectile> ProjectileSpan => Main.projectile.AsSpan(0, Main.maxProjectiles);

    public static ReadOnlySpan<Player> PlayerSpan => Main.player.AsSpan(0, Main.netMode != NetmodeID.SinglePlayer ? Main.maxPlayers : 1);

    public static ReadOnlySpan<Item> ItemSpan => Main.item.AsSpan(0, Main.maxItems);

    public static ReadOnlySpan<Dust> DustSpan => Main.dust.AsSpan(0, Main.maxDust);


    public static TOIterator<NPC> NewNPCIterator(Func<NPC, bool> match) => new(NPCSpan, match);

    public static TOExclusiveIterator<NPC> NewNPCIterator(Func<NPC, bool> match, params HashSet<NPC> exclusions) => new(NPCSpan, match, exclusions);

    public static TOIterator<NPC> NewActiveNPCIterator(Func<NPC, bool> match) => new(NPCSpan, n => n.active && match(n));

    public static TOExclusiveIterator<NPC> NewActiveNPCIterator(Func<NPC, bool> match, params HashSet<NPC> exclusions) => new(NPCSpan, n => n.active && match(n), exclusions);

    public static TOIterator<Projectile> NewProjectileIterator(Func<Projectile, bool> match) => new(ProjectileSpan, match);

    public static TOExclusiveIterator<Projectile> NewProjectileIterator(Func<Projectile, bool> match, params HashSet<Projectile> exclusions) => new(ProjectileSpan, match, exclusions);

    public static TOIterator<Projectile> NewActiveProjectileIterator(Func<Projectile, bool> match) => new(ProjectileSpan, p => p.active && match(p));

    public static TOExclusiveIterator<Projectile> NewActiveProjectileIterator(Func<Projectile, bool> match, params HashSet<Projectile> exclusions) => new(ProjectileSpan, p => p.active && match(p), exclusions);

    public static TOIterator<Player> NewPlayerIterator(Func<Player, bool> match) => new(PlayerSpan, match);

    public static TOExclusiveIterator<Player> NewPlayerIterator(Func<Player, bool> match, params HashSet<Player> exclusions) => new(PlayerSpan, match, exclusions);

    public static TOIterator<Player> NewActivePlayerIterator(Func<Player, bool> match) => new(PlayerSpan, p => p.active && match(p));

    public static TOExclusiveIterator<Player> NewActivePlayerIterator(Func<Player, bool> match, params HashSet<Player> exclusions) => new(PlayerSpan, p => p.active && match(p), exclusions);

    public static TOIterator<Dust> NewDustIterator(Func<Dust, bool> match) => new(DustSpan, match);

    public static TOExclusiveIterator<Dust> NewDustIterator(Func<Dust, bool> match, params HashSet<Dust> exclusions) => new(DustSpan, match, exclusions);

    public static TOIterator<Item> NewItemIterator(Func<Item, bool> match) => new(ItemSpan, match);

    public static TOExclusiveIterator<Item> NewItemIterator(Func<Item, bool> match, params HashSet<Item> exclusions) => new(ItemSpan, match, exclusions);

    public static TOIterator<Item> NewActiveItemIterator(Func<Item, bool> match) => new(ItemSpan, i => i.active && match(i));

    public static TOExclusiveIterator<Item> NewActiveItemIterator(Func<Item, bool> match, params HashSet<Item> exclusions) => new(ItemSpan, i => i.active && match(i), exclusions);
}

public static class IteratorMatches
{
    public static readonly Func<Item, bool> Item_IsActive = i => i.active;
    public static readonly Func<Player, bool> Player_IsActive = p => p.active;
    public static readonly Func<Player, bool> Player_IsAlive = p => p.Alive;
    public static readonly Func<Player, bool> Player_IsPVP = p => p.IsPvP;
    public static readonly Func<Projectile, bool> Projectile_IsActive = p => p.active;
    public static readonly Func<NPC, bool> NPC_IsActive = n => n.active;
    public static readonly Func<NPC, bool> NPC_IsEnemy = n => n.IsEnemy;
    public static readonly Func<NPC, bool> NPC_IsBossEnemy = n => n.IsBossEnemy;
}