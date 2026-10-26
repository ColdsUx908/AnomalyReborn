namespace Transoceanic.Extensions.Terraria;

public static partial class TOTerrariaExtensions
{
    private static float NextWeightedFloat(float maximum) => Main.rand.NextFloat(0f, maximum);

    /// <summary>使用游戏随机源按权重抽取一项。</summary>
    public static T Pick<T>(this WeightedBag<T> bag) => bag.Pick(NextWeightedFloat);

    /// <summary>使用游戏随机源尝试按权重抽取一项。</summary>
    public static bool TryPick<T>(this WeightedBag<T> bag, out T item) => bag.TryPick(NextWeightedFloat, out item);

    /// <summary>使用游戏随机源重复抽取，允许重复项。</summary>
    public static IEnumerable<T> PickMultiple<T>(this WeightedBag<T> bag, int count) => bag.PickMultiple(count, NextWeightedFloat);

    /// <summary>使用游戏随机源不放回抽取。</summary>
    public static IEnumerable<T> PickDistinct<T>(this WeightedBag<T> bag, int count) => bag.PickDistinct(count, NextWeightedFloat);

    /// <summary>使用游戏随机源将极坐标向量旋转指定范围内的随机角度。</summary>
    public static PolarVector2 RotatedByRandom(this PolarVector2 vector, float maxOffset) => vector.RotatedBy(Main.rand.NextFloat(-maxOffset, maxOffset));
}
