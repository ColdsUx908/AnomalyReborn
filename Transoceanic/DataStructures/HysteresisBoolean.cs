// Designed by ColdsUx

namespace Transoceanic.DataStructures;

/// <summary>
/// 表示一个具有滞后（防抖）特性的布尔值。
/// 内部使用一个 0~2 的整数计数器，连续设置为 <see langword="true"/> 会立即将值切换为 <see langword="true"/>，
/// 但需要连续两次设置为 <see langword="false"/> 才会切换回 <see langword="false"/>。
/// 适用于需要抗干扰或多次确认的场景。
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item/><description/>设置 <see langword="true"/> 时：内部计数器增加 2（上限 2），<see cref="Value"/> 立即变为 <see langword="true"/>。
/// <item/><description/>设置 <see langword="false"/> 时：内部计数器减少 1（下限 0），<see cref="Value"/> 仅在计数器归零后变为 <see langword="false"/>。
/// <item/><description/>因此，从 <see langword="false"/> 切换到 <see langword="true"/> 只需一次 <see langword="true"/> 设置，而从 <see langword="true"/> 切换回 <see langword="false"/> 需要两次连续的 <see langword="false"/> 设置。
/// </list>
/// <para>该类型实现了 <see cref="IEquatable{GuaranteedBoolean}"/>，相等比较基于内部计数器的值。</para>
/// <para>支持到 <see cref="bool"/> 的隐式转换，可直接用于条件判断。</para>
/// </remarks>
public struct HysteresisBoolean : IEquatable<HysteresisBoolean>
{
    private int _value;

    /// <summary>
    /// 使用指定的初始内部计数值初始化 <see cref="HysteresisBoolean"/> 结构。
    /// </summary>
    /// <param name="value">
    /// 内部计数器的初始值。建议使用 0（表示 <see langword="false"/>）或 2（表示 <see langword="true"/>），
    /// 其他值将影响后续属性设置的行为，但 <see cref="Value"/> 属性仅以 <c>> 0</c> 作为判断依据。
    /// </param>
    public HysteresisBoolean(int value) => _value = value;

    /// <summary>
    /// 获取或设置当前的布尔状态，带有滞后特性。
    /// </summary>
    /// <value>
    /// 获取时返回 <see langword="true"/> 当且仅当内部计数器大于 0。
    /// 设置时根据给定的布尔值调整内部计数器：
    /// <list type="bullet">
    /// <item><description>若设置为 <see langword="true"/>：计数器增加 2，但不超过 2。</description></item>
    /// <item><description>若设置为 <see langword="false"/>：计数器减少 1，但不低于 0。</description></item>
    /// </list>
    /// </value>
    public bool Value
    {
        readonly get => _value > 0;
        set => _value = Math.Clamp(_value + (value ? 2 : -1), 0, 2);
    }

    public static implicit operator bool(HysteresisBoolean guaranteedBoolean) => guaranteedBoolean.Value;

    public readonly bool Equals(HysteresisBoolean other) => _value == other._value;
    public override readonly bool Equals(object obj) => obj is HysteresisBoolean other && Equals(other);
    public static bool operator ==(HysteresisBoolean left, HysteresisBoolean right) => left.Equals(right);
    public static bool operator !=(HysteresisBoolean left, HysteresisBoolean right) => !(left == right);
    public override readonly int GetHashCode() => _value.GetHashCode();
}
