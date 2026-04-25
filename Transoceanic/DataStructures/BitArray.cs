// Designed by ColdsUx

namespace Transoceanic.DataStructures;

/// <summary>
/// 表示一个固定长度为 32 位的位数组，以 <see cref="int"/> 作为底层存储。
/// </summary>
public struct BitArray32 : IEquatable<BitArray32>
{
    private int _value;

    /// <summary>
    /// 使用指定的 32 位整数值初始化 <see cref="BitArray32"/> 的新实例。
    /// </summary>
    /// <param name="value">用于初始化位数组的整数值。</param>
    public BitArray32(int value) => _value = value;

    /// <summary>
    /// 初始化 <see cref="BitArray32"/> 的新实例，并将所有位设置为 0。
    /// </summary>
    public BitArray32() : this(0) { }

    /// <summary>
    /// 获取或设置指定索引处的位值。
    /// </summary>
    /// <param name="index">位的从零开始的索引，必须在 0 到 31 之间。</param>
    /// <returns>如果指定位为 1，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public bool this[int index]
    {
        readonly get => TOMathUtils.BitOperation.GetBit(_value, index);
        set => TOMathUtils.BitOperation.SetBit(ref _value, index, value);
    }

    /// <summary>
    /// 获取或设置指定索引处的位值，支持从末尾开始计数的索引。
    /// </summary>
    /// <param name="index">位的索引，可使用 <c>^n</c> 形式表示从末尾算起的偏移。</param>
    /// <returns>如果指定位为 1，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public bool this[Index index]
    {
        readonly get => this[index.GetOffset(32)];
        set => this[index.GetOffset(32)] = value;
    }

    /// <summary>
    /// 指示当前实例是否等于同一类型的另一个实例。
    /// </summary>
    /// <param name="other">要与当前实例进行比较的 <see cref="BitArray32"/>。</param>
    /// <returns>如果两个实例的内部值相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public readonly bool Equals(BitArray32 other) => _value == other._value;

    /// <summary>
    /// 指示当前实例是否等于指定对象。
    /// </summary>
    /// <param name="obj">要与当前实例进行比较的对象。</param>
    /// <returns>如果 <paramref name="obj"/> 是 <see cref="BitArray32"/> 且其内部值与当前实例相同，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public override readonly bool Equals(object obj) => obj is BitArray32 other && Equals(other);

    public override readonly int GetHashCode() => _value.GetHashCode();
    public static bool operator ==(BitArray32 left, BitArray32 right) => left.Equals(right);
    public static bool operator !=(BitArray32 left, BitArray32 right) => !(left == right);

    /// <summary>
    /// 返回当前位数组的字符串表示形式，以 32 位二进制格式显示。
    /// </summary>
    /// <returns>形如 "BitArray32 { 32位二进制字符串 }" 的字符串。</returns>
    public override readonly string ToString() => $"BitArray32 {{ {Convert.ToString(_value, 2).PadLeft(32, '0')} }}";
}

/// <summary>
/// 表示一个固定长度为 64 位的位数组，以 <see cref="long"/> 作为底层存储。
/// </summary>
public struct BitArray64 : IEquatable<BitArray64>
{
    private long _value;

    /// <summary>
    /// 使用指定的 64 位整数值初始化 <see cref="BitArray64"/> 的新实例。
    /// </summary>
    /// <param name="value">用于初始化位数组的整数值。</param>
    public BitArray64(int value) => _value = value;

    /// <summary>
    /// 初始化 <see cref="BitArray64"/> 的新实例，并将所有位设置为 0。
    /// </summary>
    public BitArray64() : this(0) { }

    /// <summary>
    /// 获取或设置指定索引处的位值。
    /// </summary>
    /// <param name="index">位的从零开始的索引，必须在 0 到 63 之间。</param>
    /// <returns>如果指定位为 1，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public bool this[int index]
    {
        readonly get => TOMathUtils.BitOperation.GetBit(_value, index);
        set => TOMathUtils.BitOperation.SetBit(ref _value, index, value);
    }

    /// <summary>
    /// 获取或设置指定索引处的位值，支持从末尾开始计数的索引。
    /// </summary>
    /// <param name="index">位的索引，可使用 <c>^n</c> 形式表示从末尾算起的偏移。</param>
    /// <returns>如果指定位为 1，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public bool this[Index index]
    {
        readonly get => this[index.GetOffset(64)];
        set => this[index.GetOffset(64)] = value;
    }

    /// <summary>
    /// 指示当前实例是否等于同一类型的另一个实例。
    /// </summary>
    /// <param name="other">要与当前实例进行比较的 <see cref="BitArray64"/>。</param>
    /// <returns>如果两个实例的内部值相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public readonly bool Equals(BitArray64 other) => _value == other._value;

    /// <summary>
    /// 指示当前实例是否等于指定对象。
    /// </summary>
    /// <param name="obj">要与当前实例进行比较的对象。</param>
    /// <returns>如果 <paramref name="obj"/> 是 <see cref="BitArray64"/> 且其内部值与当前实例相同，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public override readonly bool Equals(object obj) => obj is BitArray64 other && Equals(other);

    public override readonly int GetHashCode() => _value.GetHashCode();
    public static bool operator ==(BitArray64 left, BitArray64 right) => left.Equals(right);
    public static bool operator !=(BitArray64 left, BitArray64 right) => !(left == right);

    /// <summary>
    /// 返回当前位数组的字符串表示形式，以 64 位二进制格式显示。
    /// </summary>
    /// <returns>形如 "BitArray64 { 64位二进制字符串 }" 的字符串。</returns>
    public override readonly string ToString() => $"BitArray64 {{ {Convert.ToString(_value, 2).PadLeft(64, '0')} }}";
}

/// <summary>
/// 表示一个固定长度为 128 位的位数组，以 <see cref="Int128"/> 作为底层存储。
/// </summary>
public struct BitArray128 : IEquatable<BitArray128>
{
    private Int128 _value;

    /// <summary>
    /// 使用指定的 128 位整数值初始化 <see cref="BitArray128"/> 的新实例。
    /// </summary>
    /// <param name="value">用于初始化位数组的 <see cref="Int128"/> 值。</param>
    public BitArray128(Int128 value) => _value = value;

    /// <summary>
    /// 使用由高 64 位和低 64 位组成的值初始化 <see cref="BitArray128"/> 的新实例。
    /// </summary>
    /// <param name="upper">高 64 位部分。</param>
    /// <param name="lower">低 64 位部分。</param>
    public BitArray128(ulong upper, ulong lower) : this(new Int128(upper, lower)) { }

    /// <summary>
    /// 初始化 <see cref="BitArray128"/> 的新实例，并将所有位设置为 0。
    /// </summary>
    public BitArray128() : this(Int128.Zero) { }

    /// <summary>
    /// 获取或设置指定索引处的位值。
    /// </summary>
    /// <param name="index">位的从零开始的索引，必须在 0 到 127 之间。</param>
    /// <returns>如果指定位为 1，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public bool this[int index]
    {
        readonly get => TOMathUtils.BitOperation.GetBit(_value, index);
        set => TOMathUtils.BitOperation.SetBit(ref _value, index, value);
    }

    /// <summary>
    /// 获取或设置指定索引处的位值，支持从末尾开始计数的索引。
    /// </summary>
    /// <param name="index">位的索引，可使用 <c>^n</c> 形式表示从末尾算起的偏移。</param>
    /// <returns>如果指定位为 1，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public bool this[Index index]
    {
        readonly get => this[index.GetOffset(128)];
        set => this[index.GetOffset(128)] = value;
    }

    /// <summary>
    /// 指示当前实例是否等于同一类型的另一个实例。
    /// </summary>
    /// <param name="other">要与当前实例进行比较的 <see cref="BitArray128"/>。</param>
    /// <returns>如果两个实例的内部值相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public readonly bool Equals(BitArray128 other) => _value == other._value;

    /// <summary>
    /// 指示当前实例是否等于指定对象。
    /// </summary>
    /// <param name="obj">要与当前实例进行比较的对象。</param>
    /// <returns>如果 <paramref name="obj"/> 是 <see cref="BitArray128"/> 且其内部值与当前实例相同，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public override readonly bool Equals(object obj) => obj is BitArray128 other && Equals(other);

    public override readonly int GetHashCode() => _value.GetHashCode();
    public static bool operator ==(BitArray128 left, BitArray128 right) => left.Equals(right);
    public static bool operator !=(BitArray128 left, BitArray128 right) => !(left == right);

    /// <summary>
    /// 返回当前位数组的字符串表示形式，以两组 64 位二进制格式显示。
    /// </summary>
    /// <returns>形如 "BitArray128 { 高64位二进制 低64位二进制 }" 的字符串。</returns>
    public override readonly string ToString() => $"BitArray128 {{ {Convert.ToString((long)(_value >> 64), 2).PadLeft(64, '0')} {Convert.ToString((long)_value, 2).PadLeft(64, '0')} }}";
}

/// <summary>
/// 表示一个长度可变的位数组，以 <see cref="uint"/> 数组作为底层存储。
/// </summary>
public class BitArray
{
    private readonly uint[] _value;

    /// <summary>
    /// 获取位数组的总位数。
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// 获取存储当前位数组所需的 32 位无符号整数的个数。
    /// </summary>
    private int ArrayLength => (Length + 31) / 32;

    /// <summary>
    /// 初始化 <see cref="BitArray"/> 类的新实例，该实例具有指定的长度，且所有位初始化为 <see langword="false"/>。
    /// </summary>
    /// <param name="length">位数组的长度，必须为正数。</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> 小于或等于 0。</exception>
    public BitArray(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
        Length = length;
        _value = new uint[ArrayLength];
    }

    /// <summary>
    /// 使用现有的 <see cref="uint"/> 数组初始化 <see cref="BitArray"/> 类的新实例。
    /// </summary>
    /// <param name="value">包含位数据的无符号整数数组。数组的每个元素提供 32 位，数组长度将决定总位数（<c>value.Length * 32</c>）。</param>
    public BitArray(uint[] value) : this(value.Length * 32) => _value = value;

    /// <summary>
    /// 获取或设置指定索引处的位值。
    /// </summary>
    /// <param name="index">位的从零开始的索引。</param>
    /// <returns>如果指定位为 1，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> 小于 0 或大于等于 <see cref="Length"/>。</exception>
    public bool this[int index]
    {
        get
        {
            if (index < 0 || index >= Length)
                throw new IndexOutOfRangeException();

            int arrayIndex = index / 32;
            int bitIndex = index % 32;
            return TOMathUtils.BitOperation.GetBit(_value[arrayIndex], bitIndex);
        }
        set
        {
            if (index < 0 || index >= Length)
                throw new IndexOutOfRangeException();

            int arrayIndex = index / 32;
            int bitIndex = index % 32;
            TOMathUtils.BitOperation.SetBit(ref _value[arrayIndex], bitIndex, value);
        }
    }

    /// <summary>
    /// 返回当前位数组的字符串表示形式，从高位到低位显示每一位（左侧为最高位）。
    /// </summary>
    /// <returns>形如 "BitArray { 二进制字符串 }" 的字符串。</returns>
    public override string ToString()
    {
        StringBuilder builder = new(Length);

        for (int i = Length - 1; i >= 0; i--)
            builder.Append(this[i] ? '1' : '0');

        return $"BitArray {{ {builder} }}";
    }
}