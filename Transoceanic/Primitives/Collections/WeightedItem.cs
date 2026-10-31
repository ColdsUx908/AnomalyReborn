namespace Transoceanic.Primitives.Collections;

/// <summary>
/// 表示一个带有权重的项，权重用于加权随机选择。
/// </summary>
/// <typeparam name="T">项的类型。</typeparam>
public readonly struct WeightedItem<T> : IEquatable<WeightedItem<T>>, IComparable<WeightedItem<T>>, IComparable
{
    /// <summary>
    /// 项本身。
    /// </summary>
    public readonly T Item;

    /// <summary>
    /// 项的权重，用于加权随机选择。必须为非负数。
    /// </summary>
    public readonly float Weight;

    /// <summary>
    /// 使用指定的项和权重初始化 <see cref="WeightedItem{T}"/> 结构的新实例。
    /// </summary>
    /// <param name="item">项。</param>
    /// <param name="weight">权重，必须为有限非负数。</param>
    /// <exception cref="ArgumentOutOfRangeException">权重为负数、NaN 或无穷大时抛出。</exception>
    public WeightedItem(T item, float weight)
    {
        Item = item;
        if (!float.IsFinite(weight) || weight < 0f)
            throw new ArgumentOutOfRangeException(nameof(weight), weight, "Weight must be finite and nonnegative.");
        Weight = weight;
    }

    /// <summary>
    /// 指示当前对象是否等于同一类型的另一个对象。
    /// </summary>
    /// <param name="other">要与当前对象进行比较的对象。</param>
    /// <returns>如果两个对象的项相等且权重相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public bool Equals(WeightedItem<T> other) => EqualityComparer<T>.Default.Equals(Item, other.Item) && Weight.Equals(other.Weight);

    /// <inheritdoc/>
    /// <remarks>
    /// 指示此实例是否等于指定的对象。
    /// </remarks>
    /// <param name="obj">要与当前实例进行比较的对象。</param>
    /// <returns>如果 <paramref name="obj"/> 是 <see cref="WeightedItem{T}"/> 且项和权重均相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public override bool Equals(object obj) => obj is WeightedItem<T> other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Item, Weight);

    /// <summary>
    /// 按 Equals 比较 Item、Weight，相等时返回 true。
    /// </summary>
    public static bool operator ==(WeightedItem<T> left, WeightedItem<T> right) => left.Equals(right);
    /// <summary>
    /// 返回相等运算符结果的逻辑取反。
    /// </summary>
    public static bool operator !=(WeightedItem<T> left, WeightedItem<T> right) => !left.Equals(right);

    /// <summary>
    /// 基于权重比较当前实例与另一个 <see cref="WeightedItem{T}"/>。
    /// </summary>
    /// <param name="other">要比较的另一个加权项。</param>
    /// <returns>一个值，指示相对顺序。</returns>
    public int CompareTo(WeightedItem<T> other) => Weight.CompareTo(other.Weight);

    /// <summary>
    /// 将当前实例与另一个对象进行比较，并返回一个值，指示当前实例在排序顺序中是位于另一个对象之前、之后还是与其出现在同一位置。
    /// </summary>
    /// <param name="obj">要比较的对象，或为 <see langword="null"/>。</param>
    /// <returns>一个值，指示相对顺序。</returns>
    /// <exception cref="ArgumentException"><paramref name="obj"/> 不是 <see cref="WeightedItem{T}"/> 类型时抛出。</exception>
    public int CompareTo(object obj)
    {
        if (obj is null)
            return 1;
        if (obj is WeightedItem<T> other)
            return CompareTo(other);
        throw new ArgumentException($"Object must be of type {nameof(WeightedItem<>)}");
    }

    /// <summary>
    /// 按权重比较左项是否小于右项。
    /// </summary>
    public static bool operator <(WeightedItem<T> left, WeightedItem<T> right) => left.CompareTo(right) < 0;
    /// <summary>
    /// 按权重比较左项是否大于右项。
    /// </summary>
    public static bool operator >(WeightedItem<T> left, WeightedItem<T> right) => left.CompareTo(right) > 0;
    /// <summary>
    /// 按权重比较左项是否小于或等于右项。
    /// </summary>
    public static bool operator <=(WeightedItem<T> left, WeightedItem<T> right) => left.CompareTo(right) <= 0;
    /// <summary>
    /// 按权重比较左项是否大于或等于右项。
    /// </summary>
    public static bool operator >=(WeightedItem<T> left, WeightedItem<T> right) => left.CompareTo(right) >= 0;

    /// <summary>
    /// 将当前加权项解构为项和权重。
    /// </summary>
    /// <param name="item">当此方法返回时，包含项的值。</param>
    /// <param name="weight">当此方法返回时，包含权重的值。</param>
    public void Deconstruct(out T item, out float weight)
    {
        item = Item;
        weight = Weight;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 返回表示当前对象的字符串。
    /// </remarks>
    /// <returns>包含项类型、项值和权重的字符串。</returns>
    public override string ToString() => $"WeightedItem<{typeof(T).Name}>: Item = {Item}, Weight = {Weight}";
}

/// <summary>
/// 表示一个加权随机抽取集合（摸彩袋），其中的每个元素都关联一个有限非负权重，零权重项不会被抽取，
/// 抽取时元素被选中的概率与其权重成正比。
/// </summary>
/// <typeparam name="T">袋中元素的类型。</typeparam>
public class WeightedBag<T>
{
    private readonly List<WeightedItem<T>> _Items = [];
    private float _TotalWeight;

    /// <summary>
    /// 标记是否需要重新计算总权重。
    /// </summary>
    private bool _IsDirty;

    /// <summary>
    /// 获取当前袋中元素的数量。
    /// </summary>
    public int Count => _Items.Count;

    /// <summary>
    /// 获取袋中所有元素的总权重。
    /// </summary>
    public float TotalWeight
    {
        get
        {
            if (_IsDirty)
            {
                _TotalWeight = (float)_Items.Sum(item => (double)item.Weight);
                _IsDirty = false;
            }

            return _TotalWeight;
        }
    }

    /// <summary>
    /// 初始化 <see cref="WeightedBag{T}"/> 类的新实例，该实例为空。
    /// </summary>
    public WeightedBag() { }

    /// <summary>
    /// 初始化 <see cref="WeightedBag{T}"/> 类的新实例，并用指定的加权项集合填充。
    /// </summary>
    /// <param name="items">要添加到袋中的加权项集合。</param>
    public WeightedBag(IEnumerable<WeightedItem<T>> items) => AddRange(items);

    /// <summary>
    /// 向袋中添加一个带有指定权重的元素。
    /// </summary>
    /// <param name="item">要添加的元素。</param>
    /// <param name="weight">元素的权重，必须为有限非负数。</param>
    /// <exception cref="ArgumentException">权重无效或累计总权重溢出时抛出。</exception>
    public void Add(T item, float weight) => Add(new WeightedItem<T>(item, weight));

    /// <summary>
    /// 添加有限非负权重项；零权重项仅存储，不参与抽取。
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">权重无效或累计总权重超出 float 的有限范围。</exception>
    public void Add(WeightedItem<T> weightedItem) => AddRange([weightedItem]);

    /// <summary>
    /// 先验证整批数据与累计总权重，再追加所有项；失败时不改变集合。
    /// </summary>
    /// <exception cref="ArgumentNullException">集合为空引用。</exception>
    /// <exception cref="ArgumentOutOfRangeException">权重无效或累计总权重超出 float 的有限范围。</exception>
    public void AddRange(IEnumerable<WeightedItem<T>> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        WeightedItem<T>[] candidates = items.ToArray();
        double total = _Items.Sum(item => (double)item.Weight);
        foreach (WeightedItem<T> item in candidates)
        {
            if (!float.IsFinite(item.Weight) || item.Weight < 0f)
                throw new ArgumentOutOfRangeException(nameof(items), "Weights must be finite and nonnegative.");
            total += item.Weight;
        }
        if (total > float.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(items), "Total weight must remain finite.");
        _Items.AddRange(candidates);
        _IsDirty = true;
    }

    /// <summary>
    /// 清空摸彩袋，移除所有元素，并将总权重重置为 0。
    /// </summary>
    public void Clear()
    {
        _Items.Clear();
        _TotalWeight = 0;
        _IsDirty = false;
    }

    /// <summary>
    /// 从袋中根据权重随机抽取一个元素。
    /// </summary>
    /// <param name="nextFloat">随机采样函数，接收总权重并返回 [0, 总权重) 内的数值。</param>
    /// <returns>抽取到的元素。空袋或总权重为零时抛出 InvalidOperationException。</returns>
    /// <exception cref="InvalidOperationException">空袋或总权重为零。</exception>
    /// <exception cref="ArgumentNullException">采样函数为空。</exception>
    /// <exception cref="ArgumentOutOfRangeException">采样值不在有限的 [0, 总权重) 范围内。</exception>
    public T Pick(Func<float, float> nextFloat)
    {
        ArgumentNullException.ThrowIfNull(nextFloat);
        EnsurePositiveTotal(TotalWeight);
        if (_Items.Count == 1)
            return _Items[0].Item;
        return _Items[SelectIndex(_Items, nextFloat)].Item;
    }

    private static void EnsurePositiveTotal(float total)
    {
        if (!float.IsFinite(total) || total <= 0f)
            throw new InvalidOperationException("Sampling requires a finite positive total weight.");
    }

    private static int SelectIndex(List<WeightedItem<T>> items, Func<float, float> nextFloat)
    {
        double total = items.Sum(item => (double)item.Weight);
        float maximum = (float)total;
        EnsurePositiveTotal(maximum);
        float sample = nextFloat(maximum);
        if (!float.IsFinite(sample) || sample < 0f || sample >= maximum)
            throw new ArgumentOutOfRangeException(nameof(nextFloat), sample, "Sample must be finite and in [0, totalWeight).");

        double cumulative = 0d;
        int lastPositive = -1;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].Weight == 0f)
                continue;
            lastPositive = i;
            cumulative += items[i].Weight;
            if (sample < cumulative)
                return i;
        }
        // float 总权重向上舍入时，尾部舍入区间归入最后一个正权重项。
        return lastPositive;
    }

    /// <summary>
    /// 尝试从袋中根据权重随机抽取一个元素。
    /// </summary>
    /// <param name="nextFloat">随机采样函数，接收总权重并返回 [0, 总权重) 内的数值。</param>
    /// <param name="item">当此方法返回时，如果抽取成功，则包含抽取到的元素；否则为 <typeparamref name="T"/> 的默认值。</param>
    /// <returns>空袋返回 false；非空袋成功抽取返回 true。非空且总权重为零时仍抛出异常。</returns>
    public bool TryPick(Func<float, float> nextFloat, out T item)
    {
        if (_Items.Count == 0)
        {
            item = default;
            return false;
        }

        item = Pick(nextFloat);
        return true;
    }

    /// <summary>
    /// 从袋中根据权重随机抽取指定数量的元素（允许重复抽取同一元素）。
    /// </summary>
    /// <param name="count">要抽取的元素数量。</param>
    /// <param name="nextFloat">随机采样函数，接收总权重并返回 [0, 总权重) 内的数值。</param>
    /// <returns>一个包含抽取结果的序列。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> 小于或等于 0 时抛出。</exception>
    public IEnumerable<T> PickMultiple(int count, Func<float, float> nextFloat)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        for (int i = 0; i < count; i++)
            yield return Pick(nextFloat);
    }

    /// <summary>
    /// 从袋中根据权重随机抽取指定数量的不同元素（不放回抽取）。
    /// </summary>
    /// <param name="count">要抽取的元素数量。</param>
    /// <param name="nextFloat">随机采样函数，接收剩余总权重并返回 [0, 剩余总权重) 内的数值。</param>
    /// <returns>按条目不放回的结果；不同条目仍可能具有相同的值，零权重项不会入选。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> 小于或等于 0，或大于正权重条目数时抛出。</exception>
    public IEnumerable<T> PickDistinct(int count, Func<float, float> nextFloat)
    {
        ArgumentNullException.ThrowIfNull(nextFloat);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
        EnsurePositiveTotal(TotalWeight);
        List<WeightedItem<T>> candidates = [.. _Items.Where(item => item.Weight > 0f)];
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, candidates.Count);
        List<T> results = [];
        for (int i = 0; i < count; i++)
        {
            int index = SelectIndex(candidates, nextFloat);
            results.Add(candidates[index].Item);
            candidates.RemoveAt(index);
        }
        return results;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 返回表示当前 <see cref="WeightedBag{T}"/> 对象的字符串。
    /// </remarks>
    /// <returns>包含元素类型、元素数量和总权重的字符串。</returns>
    public override string ToString() => $"WeightedBag<{typeof(T).Name}> {{ Count = {Count}, TotalWeight = {TotalWeight:F2} }}";
}
