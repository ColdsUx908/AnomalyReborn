// Developed by ColdsUx

namespace Transoceanic.DataStructures;

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
    /// <param name="weight">权重，必须大于等于 0。</param>
    /// <exception cref="ArgumentOutOfRangeException">权重为负数时抛出。</exception>
    public WeightedItem(T item, float weight)
    {
        Item = item;
        ArgumentOutOfRangeException.ThrowIfNegative(weight);
        Weight = weight;
    }

    /// <summary>
    /// 指示当前对象是否等于同一类型的另一个对象。
    /// </summary>
    /// <param name="other">要与当前对象进行比较的对象。</param>
    /// <returns>如果两个对象的项相等且权重相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public bool Equals(WeightedItem<T> other) => EqualityComparer<T>.Default.Equals(Item, other.Item) && Weight.Equals(other.Weight);

    /// <summary>
    /// 指示此实例是否等于指定的对象。
    /// </summary>
    /// <param name="obj">要与当前实例进行比较的对象。</param>
    /// <returns>如果 <paramref name="obj"/> 是 <see cref="WeightedItem{T}"/> 且项和权重均相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public override bool Equals(object obj) => obj is WeightedItem<T> other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Item, Weight);

    public static bool operator ==(WeightedItem<T> left, WeightedItem<T> right) => left.Equals(right);
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

    public static bool operator <(WeightedItem<T> left, WeightedItem<T> right) => left.CompareTo(right) < 0;
    public static bool operator >(WeightedItem<T> left, WeightedItem<T> right) => left.CompareTo(right) > 0;
    public static bool operator <=(WeightedItem<T> left, WeightedItem<T> right) => left.CompareTo(right) <= 0;
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

    /// <summary>
    /// 返回表示当前对象的字符串。
    /// </summary>
    /// <returns>包含项类型、项值和权重的字符串。</returns>
    public override string ToString() => $"WeightedItem<{typeof(T).Name}>: Item = {Item}, Weight = {Weight}";
}

/// <summary>
/// 表示一个加权随机抽取集合（摸彩袋），其中的每个元素都关联一个正数权重，
/// 抽取时元素被选中的概率与其权重成正比。
/// </summary>
/// <typeparam name="T">袋中元素的类型。</typeparam>
public class WeightedBag<T>
{
    private readonly List<WeightedItem<T>> _items = [];
    private float _totalWeight;
    /// <summary>
    /// 标记是否需要重新计算总权重。
    /// </summary>
    private bool _isDirty = true;

    /// <summary>
    /// 获取当前袋中元素的数量。
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// 获取袋中所有元素的总权重。
    /// </summary>
    public float TotalWeight
    {
        get
        {
            if (_isDirty)
            {
                _totalWeight = 0f;
                foreach (WeightedItem<T> item in _items)
                    _totalWeight += item.Weight;
                _isDirty = false;
            }

            return _totalWeight;
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
    /// <param name="weight">元素的权重，必须为正数。</param>
    /// <exception cref="ArgumentException">权重小于或等于 0 时抛出。</exception>
    public void Add(T item, float weight)
    {
        if (weight <= 0)
            throw new ArgumentException("权重必须为正数", nameof(weight));

        _items.Add(new WeightedItem<T>(item, weight));
        _isDirty = true;
    }

    /// <summary>
    /// 向袋中添加一个已封装好的加权项。
    /// </summary>
    /// <param name="weightedItem">要添加的加权项。</param>
    public void Add(WeightedItem<T> weightedItem)
    {
        _items.Add(weightedItem);
        _isDirty = true;
    }

    /// <summary>
    /// 向袋中批量添加多个加权项。
    /// </summary>
    /// <param name="items">要添加的加权项集合。</param>
    public void AddRange(IEnumerable<WeightedItem<T>> items)
    {
        _items.AddRange(items);
        _isDirty = true;
    }

    /// <summary>
    /// 清空摸彩袋，移除所有元素，并将总权重重置为 0。
    /// </summary>
    public void Clear()
    {
        _items.Clear();
        _totalWeight = 0;
        _isDirty = false;
    }

    /// <summary>
    /// 从袋中根据权重随机抽取一个元素。
    /// </summary>
    /// <returns>抽取到的元素；如果袋为空，则返回 <typeparamref name="T"/> 的默认值。</returns>
    public T Pick()
    {
        if (_items.Count == 0)
            return default;

        if (_items.Count == 1)
            return _items[0].Item;

        float totalWeight = TotalWeight;

        float randomValue = Main.rand.NextFloat(0f, totalWeight);

        //遍历元素，找到随机数落入的区间
        float cumulativeWeight = 0f;
        foreach (WeightedItem<T> weightedItem in _items)
        {
            cumulativeWeight += weightedItem.Weight;
            if (randomValue < cumulativeWeight)
                return weightedItem.Item;
        }

        //理论上不应执行到这里，但为了安全，返回最后一个元素
        return _items[^1].Item;
    }

    /// <summary>
    /// 尝试从袋中根据权重随机抽取一个元素。
    /// </summary>
    /// <param name="item">当此方法返回时，如果抽取成功，则包含抽取到的元素；否则为 <typeparamref name="T"/> 的默认值。</param>
    /// <returns>如果袋不为空且成功抽取，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public bool TryPick(out T item)
    {
        if (_items.Count == 0)
        {
            item = default;
            return false;
        }

        item = Pick();
        return true;
    }

    /// <summary>
    /// 从袋中根据权重随机抽取指定数量的元素（允许重复抽取同一元素）。
    /// </summary>
    /// <param name="count">要抽取的元素数量。</param>
    /// <returns>一个包含抽取结果的序列。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> 小于或等于 0 时抛出。</exception>
    public IEnumerable<T> PickMultiple(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        for (int i = 0; i < count; i++)
            yield return Pick();
    }

    /// <summary>
    /// 从袋中根据权重随机抽取指定数量的不同元素（不放回抽取）。
    /// </summary>
    /// <param name="count">要抽取的元素数量。</param>
    /// <returns>一个包含抽取结果的序列，其中的元素互不相同。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> 小于或等于 0，或大于袋中元素总数时抛出。</exception>
    public IEnumerable<T> PickDistinct(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, Count);

        List<(WeightedItem<T> item, int index)> itemsWithIndices = [.. _items.Select((item, index) => (item, index))];
        List<T> results = [];
        float totalWeight = itemsWithIndices.Sum(x => x.item.Weight);

        for (int i = 0; i < count; i++)
        {
            float randomValue = Main.rand.NextFloat(0f, totalWeight);

            float cumulativeWeight = 0f;
            for (int j = 0; j < itemsWithIndices.Count; j++)
            {
                cumulativeWeight += itemsWithIndices[j].item.Weight;
                if (randomValue < cumulativeWeight)
                {
                    WeightedItem<T> item = itemsWithIndices[j].item;
                    results.Add(item.Item);
                    totalWeight -= item.Weight;
                    itemsWithIndices.RemoveAt(j);
                    break;
                }
            }
        }
        return results;
    }

    /// <summary>
    /// 返回表示当前 <see cref="WeightedBag{T}"/> 对象的字符串。
    /// </summary>
    /// <returns>包含元素类型、元素数量和总权重的字符串。</returns>
    public override string ToString() => $"WeightedBag<{typeof(T).Name}> {{ Count = {Count}, TotalWeight = {TotalWeight:F2} }}";
}