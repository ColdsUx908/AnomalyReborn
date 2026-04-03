namespace Transoceanic.DataStructures;

public readonly struct WeightedItem<T> : IEquatable<WeightedItem<T>>, IComparable<WeightedItem<T>>, IComparable
{
    public readonly T Item;

    /// <summary>
    /// 权重。
    /// </summary>
    public readonly float Weight;

    public WeightedItem(T item, float weight)
    {
        Item = item;
        ArgumentOutOfRangeException.ThrowIfNegative(weight);
        Weight = weight;
    }

    public bool Equals(WeightedItem<T> other) => EqualityComparer<T>.Default.Equals(Item, other.Item) && Weight.Equals(other.Weight);
    public override bool Equals(object obj) => obj is WeightedItem<T> other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Item, Weight);
    public static bool operator ==(WeightedItem<T> left, WeightedItem<T> right) => left.Equals(right);
    public static bool operator !=(WeightedItem<T> left, WeightedItem<T> right) => !left.Equals(right);

    /// <summary>
    /// 基于权重进行比较。
    /// </summary>
    public int CompareTo(WeightedItem<T> other) => Weight.CompareTo(other.Weight);
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

    public void Deconstruct(out T item, out float weight)
    {
        item = Item;
        weight = Weight;
    }

    public override string ToString() => $"WeightedItem<{typeof(T).Name}>: Item = {Item}, Weight = {Weight}";
}

public class WeightedBag<T>
{
    private readonly List<WeightedItem<T>> _items = [];
    private float _totalWeight;
    /// <summary>
    /// 标记是否需要重新计算总权重。
    /// </summary>
    private bool _isDirty = true;

    /// <summary>
    /// 当前袋中元素数量。
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// 所有元素的总权重。
    /// </summary>
    public float TotalWeight
    {
        get
        {
            if (_isDirty)
            {
                _totalWeight = 0f;
                foreach (var item in _items)
                {
                    _totalWeight += item.Weight;
                }
                _isDirty = false;
            }

            return _totalWeight;
        }
    }

    public WeightedBag() { }

    public WeightedBag(IEnumerable<WeightedItem<T>> items) => AddRange(items);

    public void Add(T item, float weight)
    {
        if (weight <= 0)
            throw new ArgumentException("权重必须为正数", nameof(weight));

        _items.Add(new WeightedItem<T>(item, weight));
        _isDirty = true;
    }

    public void Add(WeightedItem<T> weightedItem)
    {
        _items.Add(weightedItem);
        _isDirty = true;
    }

    public void AddRange(IEnumerable<WeightedItem<T>> items)
    {
        _items.AddRange(items);
        _isDirty = true;
    }

    /// <summary>
    /// 清空摸彩袋。
    /// </summary>
    public void Clear()
    {
        _items.Clear();
        _totalWeight = 0;
        _isDirty = false;
    }

    /// <summary>
    /// 从袋中随机抽取一个元素（加权随机）。
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
        foreach (var weightedItem in _items)
        {
            cumulativeWeight += weightedItem.Weight;
            if (randomValue < cumulativeWeight)
                return weightedItem.Item;
        }

        //理论上不应执行到这里，但为了安全，返回最后一个元素
        return _items[^1].Item;
    }

    /// <summary>
    /// 尝试从袋中随机抽取一个元素。
    /// </summary>
    /// <param name="item">抽取到的元素</param>
    /// <returns>是否成功抽取</returns>
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
    /// 从袋中随机抽取多个元素（允许重复）。
    /// </summary>
    public IEnumerable<T> PickMultiple(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        for (int i = 0; i < count; i++)
            yield return Pick();
    }

    /// <summary>
    /// 从袋中随机抽取多个元素（不重复）。
    /// </summary>
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

    public override string ToString() => $"WeightedBag<{typeof(T).Name}> {{ Count = {Count}, TotalWeight = {TotalWeight:F2} }}";
}