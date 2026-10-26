using MonoMod.Utils;
using Transoceanic.Runtime.Reflection;

namespace Transoceanic.Behaviors.Collections;

/// <summary>
/// 表示一个基于方法的实体行为集合，用于存储和检索实现特定覆写方法的 <typeparamref name="TBehavior"/> 实例。
/// 行为实例按方法名分组，并依照优先级（<see cref="EntityBehavior{TEntity}.Priority"/>）降序排列。
/// </summary>
/// <typeparam name="TEntity">行为所附加的实体类型，必须派生自 <see cref="Entity"/>。</typeparam>
/// <typeparam name="TBehavior">行为的具体类型，必须派生自 <see cref="EntityBehavior{TEntity}"/>。</typeparam>
public class SimpleEntityBehaviorSet<TEntity, TBehavior>
    where TEntity : Entity
    where TBehavior : EntityBehavior<TEntity>
{
    /// <summary>
    /// 提供对特定方法所关联行为的只读枚举器。
    /// 该结构体为 <see langword="ref struct"/>，确保不会被分配到托管堆上。
    /// </summary>
    public readonly ref struct BehaviorProcesser
    {
        /// <summary>
        /// 获取一个空的 <see cref="BehaviorProcesser"/> 实例，表示无行为可供处理。
        /// </summary>
        public static BehaviorProcesser Empty => new([]);

        private readonly Span<TBehavior> _Span;

        /// <summary>
        /// 初始化 <see cref="BehaviorProcesser"/> 结构的新实例。
        /// </summary>
        /// <param name="span">包含行为实例的只读跨度。</param>
        public BehaviorProcesser(Span<TBehavior> span) => _Span = span;

        /// <summary>
        /// 返回一个可用于遍历当前方法下满足处理条件（<see cref="EntityBehavior{TEntity}.ShouldProcess"/>）的行为的枚举器。
        /// </summary>
        /// <returns>一个 <see cref="Enumerator"/> 结构实例。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new(this);

        /// <summary>
        /// 对满足处理条件的行为进行迭代的枚举器结构。
        /// </summary>
        public ref struct Enumerator
        {
            private Span<TBehavior>.Enumerator _Enumerator;
            private TBehavior _Current;

            /// <summary>
            /// 获取枚举器当前位置的行为实例。
            /// </summary>
            public readonly TBehavior Current { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _Current; }

            /// <summary>
            /// 初始化 <see cref="Enumerator"/> 结构的新实例。
            /// </summary>
            /// <param name="processer">关联的 <see cref="BehaviorProcesser"/> 实例。</param>
            public Enumerator(BehaviorProcesser processer) => _Enumerator = processer._Span.GetEnumerator();

            /// <summary>
            /// 将枚举器推进到下一个满足处理条件的行为元素。
            /// </summary>
            /// <returns>如果成功移动到下一个有效元素，则为 <see langword="true"/>；如果已到达末尾，则为 <see langword="false"/>。</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                while (_Enumerator.MoveNext())
                {
                    TBehavior temp = _Enumerator.Current;
                    if (temp.ShouldProcess)
                    {
                        _Current = temp;
                        return true;
                    }
                }
                return false;
            }
        }
    }

    /// <summary>
    /// 提供对特定方法所关联行为并自动设置其所属实体的只读枚举器。
    /// 该结构体在迭代过程中会将当前实体引用赋给行为实例的 <c>_Entity</c> 字段。
    /// 由于性能原因，直接操作了内部字段，使用时请谨慎。
    /// </summary>
    public readonly ref struct ConnectedBehaviorProcesser
    {
        /// <summary>
        /// 获取一个空的 <see cref="ConnectedBehaviorProcesser"/> 实例，表示无行为可供处理。
        /// </summary>
        public static ConnectedBehaviorProcesser Empty => new([], null);

        private readonly Span<TBehavior> _Span;
        private readonly TEntity _Entity;

        /// <summary>
        /// 初始化 <see cref="ConnectedBehaviorProcesser"/> 结构的新实例。
        /// </summary>
        /// <param name="span">包含行为实例的只读跨度。</param>
        /// <param name="entity">要关联到行为的实体实例。</param>
        public ConnectedBehaviorProcesser(Span<TBehavior> span, TEntity entity)
        {
            _Span = span;
            _Entity = entity;
        }

        /// <summary>
        /// 返回一个可用于遍历当前方法下满足处理条件并自动关联实体的行为的枚举器。
        /// </summary>
        /// <returns>一个 <see cref="Enumerator"/> 结构实例。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new(this);

        /// <summary>
        /// 对满足处理条件并自动设置所属实体的行为进行迭代的枚举器结构。
        /// </summary>
        public ref struct Enumerator
        {
            private Span<TBehavior>.Enumerator _Enumerator;
            private readonly TEntity _Entity;
            private TBehavior _Current;

            /// <summary>
            /// 获取枚举器当前位置的行为实例。
            /// </summary>
            public readonly TBehavior Current { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _Current; }

            /// <summary>
            /// 初始化 <see cref="Enumerator"/> 结构的新实例。
            /// </summary>
            /// <param name="processer">关联的 <see cref="ConnectedBehaviorProcesser"/> 实例。</param>
            public Enumerator(ConnectedBehaviorProcesser processer)
            {
                _Enumerator = processer._Span.GetEnumerator();
                _Entity = processer._Entity;
            }

            /// <summary>
            /// 将枚举器推进到下一个满足处理条件的行为元素，并自动设置其所属实体。
            /// </summary>
            /// <returns>如果成功移动到下一个有效元素，则为 <see langword="true"/>；如果已到达末尾，则为 <see langword="false"/>。</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                while (_Enumerator.MoveNext())
                {
                    TBehavior temp = _Enumerator.Current;
                    temp._Entity = _Entity;
                    if (temp.ShouldProcess)
                    {
                        _Current = temp;
                        return true;
                    }
                }
                return false;
            }
        }
    }

    /// <summary>
    /// 提供对特定方法所关联且派生自指定类型 <typeparamref name="T"/> 的行为的只读枚举器。
    /// </summary>
    /// <typeparam name="T">期望的具体行为类型，必须继承自 <typeparamref name="TBehavior"/>。</typeparam>
    public readonly ref struct TypedBehaviorProcesser<T> where T : TBehavior
    {
        /// <summary>
        /// 获取一个空的 <see cref="TypedBehaviorProcesser{T}"/> 实例，表示无符合类型的行为可供处理。
        /// </summary>
        public static TypedBehaviorProcesser<T> Empty => new([]);

        private readonly Span<TBehavior> _Span;

        /// <summary>
        /// 初始化 <see cref="TypedBehaviorProcesser{T}"/> 结构的新实例。
        /// </summary>
        /// <param name="span">包含行为实例的只读跨度。</param>
        public TypedBehaviorProcesser(Span<TBehavior> span) => _Span = span;

        /// <summary>
        /// 返回一个可用于遍历当前方法下满足处理条件且类型为 <typeparamref name="T"/> 的行为的枚举器。
        /// </summary>
        /// <returns>一个 <see cref="Enumerator"/> 结构实例。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new(this);

        /// <summary>
        /// 对满足处理条件且类型为 <typeparamref name="T"/> 的行为进行迭代的枚举器结构。
        /// </summary>
        public ref struct Enumerator
        {
            private Span<TBehavior>.Enumerator _Enumerator;
            private T _Current;

            /// <summary>
            /// 获取枚举器当前位置的行为实例。
            /// </summary>
            public readonly T Current { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _Current; }

            /// <summary>
            /// 初始化 <see cref="Enumerator"/> 结构的新实例。
            /// </summary>
            /// <param name="processer">关联的 <see cref="TypedBehaviorProcesser{T}"/> 实例。</param>
            public Enumerator(TypedBehaviorProcesser<T> processer) => _Enumerator = processer._Span.GetEnumerator();

            /// <summary>
            /// 将枚举器推进到下一个满足处理条件且类型为 <typeparamref name="T"/> 的行为元素。
            /// </summary>
            /// <returns>如果成功移动到下一个有效元素，则为 <see langword="true"/>；如果已到达末尾，则为 <see langword="false"/>。</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                while (_Enumerator.MoveNext())
                {
                    T temp = _Enumerator.Current as T;
                    if (temp?.ShouldProcess == true)
                    {
                        _Current = temp;
                        return true;
                    }
                }
                return false;
            }
        }
    }

    /// <summary>
    /// 提供对特定方法所关联、派生自指定类型 <typeparamref name="T"/> 并自动设置其所属实体的行为的只读枚举器。
    /// 该结构体在迭代过程中会将当前实体引用赋给行为实例的 <c>_Entity</c> 字段。
    /// 由于性能原因，直接操作了内部字段，使用时请谨慎。
    /// </summary>
    /// <typeparam name="T">期望的具体行为类型，必须继承自 <typeparamref name="TBehavior"/>。</typeparam>
    public readonly ref struct TypedConnectedBehaviorProcesser<T> where T : TBehavior
    {
        /// <summary>
        /// 获取一个空的 <see cref="TypedConnectedBehaviorProcesser{T}"/> 实例，表示无符合类型的行为可供处理。
        /// </summary>
        public static TypedConnectedBehaviorProcesser<T> Empty => new([], null);

        private readonly Span<TBehavior> _Span;
        private readonly TEntity _Entity;

        /// <summary>
        /// 初始化 <see cref="TypedConnectedBehaviorProcesser{T}"/> 结构的新实例。
        /// </summary>
        /// <param name="span">包含行为实例的只读跨度。</param>
        /// <param name="entity">要关联到行为的实体实例。</param>
        public TypedConnectedBehaviorProcesser(Span<TBehavior> span, TEntity entity)
        {
            _Span = span;
            _Entity = entity;
        }

        /// <summary>
        /// 返回一个可用于遍历当前方法下满足处理条件、类型为 <typeparamref name="T"/> 并自动关联实体的行为的枚举器。
        /// </summary>
        /// <returns>一个 <see cref="Enumerator"/> 结构实例。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new(this);

        /// <summary>
        /// 对满足处理条件、类型为 <typeparamref name="T"/> 并自动设置所属实体的行为进行迭代的枚举器结构。
        /// </summary>
        public ref struct Enumerator
        {
            private Span<TBehavior>.Enumerator _Enumerator;
            private readonly TEntity _Entity;
            private T _Current;

            /// <summary>
            /// 获取枚举器当前位置的行为实例。
            /// </summary>
            public readonly T Current { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _Current; }

            /// <summary>
            /// 初始化 <see cref="Enumerator"/> 结构的新实例。
            /// </summary>
            /// <param name="processer">关联的 <see cref="TypedConnectedBehaviorProcesser{T}"/> 实例。</param>
            public Enumerator(TypedConnectedBehaviorProcesser<T> processer)
            {
                _Enumerator = processer._Span.GetEnumerator();
                _Entity = processer._Entity;
            }

            /// <summary>
            /// 将枚举器推进到下一个满足处理条件且类型为 <typeparamref name="T"/> 的行为元素，并自动设置其所属实体。
            /// </summary>
            /// <returns>如果成功移动到下一个有效元素，则为 <see langword="true"/>；如果已到达末尾，则为 <see langword="false"/>。</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                while (_Enumerator.MoveNext())
                {
                    T temp = _Enumerator.Current as T;
                    if (temp is not null)
                    {
                        temp._Entity = _Entity;
                        if (temp.ShouldProcess)
                        {
                            _Current = temp;
                            return true;
                        }
                    }
                }
                return false;
            }
        }
    }

    /// <summary>
    /// 内部存储结构，以方法名为键，存储按优先级降序排列的行为数组。
    /// </summary>
    protected internal readonly Dictionary<string, TBehavior[]> _Data = [];

    /// <summary>
    /// 清除集合中所有行为数据。
    /// </summary>
    public void Clear() => _Data.Clear();

    /// <summary>
    /// 使用指定的行为集合填充集合。行为将按覆写的方法名进行分组。
    /// </summary>
    /// <param name="behaviors">要添加的行为实例集合。</param>
    public void FillSet(IEnumerable<TBehavior> behaviors) => Initialize(behaviors);

    /// <summary>
    /// 通过反射自动发现并填充所有派生自 <typeparamref name="TBehavior"/> 的类型实例。
    /// </summary>
    public void FillSet() => Initialize(TOReflectionUtils.GetTypeInstancesDerivedFrom<TBehavior>());

    /// <summary>
    /// 从指定程序集中通过反射发现并填充所有派生自 <typeparamref name="TBehavior"/> 的类型实例。
    /// </summary>
    /// <param name="assemblyToSearch">要搜索的程序集。</param>
    public void FillSet(Assembly assemblyToSearch) => Initialize(TOReflectionUtils.GetTypeInstancesDerivedFrom<TBehavior>(assemblyToSearch));

    /// <summary>
    /// 填充集合，仅包含其所属 Mod 类型为 <typeparamref name="T"/> 的行为实例。
    /// </summary>
    /// <typeparam name="T">用于筛选的 Mod 类型。</typeparam>
    public void FillSet<T>() where T : Mod => Initialize(TOReflectionUtils.GetTypeInstancesDerivedFrom<TBehavior>().Where(b => b.Mod is T));

    /// <summary>
    /// 获取用于枚举当前调用者方法所关联的行为的处理器。
    /// </summary>
    /// <param name="methodName">由 <see cref="CallerMemberNameAttribute"/> 自动填充的调用方法名。</param>
    /// <returns>一个 <see cref="BehaviorProcesser"/> 实例，可用于 <see langword="foreach"/> 循环。</returns>
    public BehaviorProcesser Enumerate([CallerMemberName] string methodName = null) =>
        _Data.TryGetValue(methodName, out TBehavior[] behaviors) ? new(behaviors) : BehaviorProcesser.Empty;

    /// <summary>
    /// 获取用于枚举当前调用者方法所关联的行为并自动关联指定实体的处理器。
    /// </summary>
    /// <param name="entity">要关联的实体实例。</param>
    /// <param name="methodName">由 <see cref="CallerMemberNameAttribute"/> 自动填充的调用方法名。</param>
    /// <returns>一个 <see cref="ConnectedBehaviorProcesser"/> 实例，可用于 <see langword="foreach"/> 循环。</returns>
    public ConnectedBehaviorProcesser Enumerate(TEntity entity, [CallerMemberName] string methodName = null) =>
        _Data.TryGetValue(methodName, out TBehavior[] behaviors) ? new(behaviors, entity) : ConnectedBehaviorProcesser.Empty;

    /// <summary>
    /// 获取用于枚举当前调用者方法所关联且类型为 <typeparamref name="T"/> 的行为的处理器。
    /// </summary>
    /// <typeparam name="T">期望的具体行为类型。</typeparam>
    /// <param name="methodName">由 <see cref="CallerMemberNameAttribute"/> 自动填充的调用方法名。</param>
    /// <returns>一个 <see cref="TypedBehaviorProcesser{T}"/> 实例，可用于 <see langword="foreach"/> 循环。</returns>
    public TypedBehaviorProcesser<T> Enumerate<T>([CallerMemberName] string methodName = null) where T : TBehavior =>
        _Data.TryGetValue(methodName, out TBehavior[] behaviors) ? new(behaviors) : TypedBehaviorProcesser<T>.Empty;

    /// <summary>
    /// 获取用于枚举当前调用者方法所关联、类型为 <typeparamref name="T"/> 并自动关联指定实体的行为的处理器。
    /// </summary>
    /// <typeparam name="T">期望的具体行为类型。</typeparam>
    /// <param name="entity">要关联的实体实例。</param>
    /// <param name="methodName">由 <see cref="CallerMemberNameAttribute"/> 自动填充的调用方法名。</param>
    /// <returns>一个 <see cref="TypedConnectedBehaviorProcesser{T}"/> 实例，可用于 <see langword="foreach"/> 循环。</returns>
    public TypedConnectedBehaviorProcesser<T> Enumerate<T>(TEntity entity, [CallerMemberName] string methodName = null) where T : TBehavior =>
        _Data.TryGetValue(methodName, out TBehavior[] behaviors) ? new(behaviors, entity) : TypedConnectedBehaviorProcesser<T>.Empty;

    /// <summary>
    /// 按照优先级（<see cref="EntityBehavior{TEntity}.Priority"/>）降序查找当前调用者方法关联的第一个满足处理条件的行为实例。
    /// </summary>
    /// <param name="entity">要关联的实体实例，将在检查前赋值给行为的 <c>_Entity</c> 字段。</param>
    /// <param name="methodName">由 <see cref="CallerMemberNameAttribute"/> 自动填充的调用方法名。</param>
    /// <returns>第一个满足 <see cref="EntityBehavior{TEntity}.ShouldProcess"/> 条件的行为实例；如果不存在，则为 <see langword="null"/>。</returns>
    public TBehavior GetFirstOrDefault(TEntity entity, [CallerMemberName] string methodName = null)
    {
        if (_Data.TryGetValue(methodName, out TBehavior[] behaviors))
        {
            for (int i = 0; i < behaviors.Length; i++)
            {
                TBehavior behavior = behaviors[i];
                behavior._Entity = entity;
                if (behavior.ShouldProcess)
                    return behavior;
            }
        }
        return null;
    }

    /// <summary>
    /// 内部初始化方法，将行为集合按覆写的方法名分组，并按优先级降序存储。
    /// </summary>
    /// <param name="behaviors">要初始化的行为实例集合。</param>
    internal void Initialize(IEnumerable<TBehavior> behaviors)
    {
        Dictionary<string, List<TBehavior>> tempData = [];
        foreach ((TBehavior behavior, HashSet<string> behaviorMethods) in
            behaviors.Select(b =>
            {
                Type type = b.GetType();
                return (b, (type.HasAttribute<CriticalBehaviorAttribute>() ? type.GetMethodNamesExceptObject(TOReflectionUtils.UniversalBindingFlags) : type.GetOverrideMethodNames(TOReflectionUtils.UniversalBindingFlags)).ToHashSet());
            }))
        {
            foreach (string method in behaviorMethods)
                tempData.AddBetter(method, behavior);
        }
        _Data.AddRange(tempData.ToDictionary(kv => kv.Key, kv => kv.Value.Distinct().OrderByDescending(b => b.Priority).ToArray()));
    }
}
