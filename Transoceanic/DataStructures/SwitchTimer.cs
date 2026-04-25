// Designed by ColdsUx

namespace Transoceanic.DataStructures;

/// <summary>
/// 表示一个基于开/关时间戳的整数计时器，用于计算并限制在指定范围内的经过时间或剩余时间。
/// </summary>
/// <remarks>
/// 该结构体通过两个时间戳（<see cref="LastOnTime"/> 和 <see cref="LastOffTime"/>）模拟一个开关状态。
/// 调用 <see cref="GetValue"/> 方法时，会根据最近一次有效状态计算时间差，并将结果强制限制在 [0, max] 区间内。
/// 实现了 <see cref="IEquatable{T}"/> 接口，比较基于两个时间戳字段。
/// </remarks>
public struct SwitchTimer : IEquatable<SwitchTimer>
{
    /// <summary>
    /// 最近一次“开启”事件的时间戳。
    /// </summary>
    public int LastOnTime;

    /// <summary>
    /// 最近一次“关闭”事件的时间戳。
    /// </summary>
    public int LastOffTime;

    /// <summary>
    /// 根据当前实际时间计算一个受边界限制的计时值。
    /// </summary>
    /// <param name="actualTime">当前的参考时间。</param>
    /// <param name="max">计时结果允许的最大值（上边界）。</param>
    /// <param name="equal">
    /// 当 <see cref="LastOnTime"/> 与 <see cref="LastOffTime"/> 相等时，是否视为“开启”状态。
    /// 默认为 <see langword="false"/>，此时相等视为“关闭”状态。
    /// </param>
    /// <returns>
    /// 若当前状态为“开启”（<see cref="LastOnTime"/> 严格大于 <see cref="LastOffTime"/>，
    /// 或 equal 为 <see langword="true"/> 且两者相等），返回自 <see cref="LastOnTime"/> 以来经过的时间，即 <c>actualTime - LastOnTime</c> 的夹取结果。
    /// 否则视为“关闭”状态，返回距离下一次可能开启所需的剩余时间（以循环方式计算），即 <c>max - actualTime + LastOffTime</c> 的夹取结果。
    /// 返回值始终在 [0, max] 范围内。
    /// </returns>
    /// <remarks>
    /// 该方法不修改任何字段，仅根据当前时间与存储的时间戳计算差值并限幅。
    /// </remarks>
    public readonly int GetValue(int actualTime, int max, bool equal = false) => Math.Clamp(
        LastOnTime > LastOffTime || (equal && LastOnTime == LastOffTime) ? actualTime - LastOnTime : max - actualTime + LastOffTime,
        0, max);

    public readonly bool Equals(SwitchTimer other) => LastOnTime == other.LastOnTime && LastOffTime == other.LastOffTime;
    public override readonly bool Equals(object obj) => obj is SwitchTimer other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(LastOnTime, LastOffTime);
    public static bool operator ==(SwitchTimer left, SwitchTimer right) => left.Equals(right);
    public static bool operator !=(SwitchTimer left, SwitchTimer right) => !(left == right);
}
