namespace Transoceanic.DataStructures;

public struct BitArray32 : IEquatable<BitArray32>
{
    private int _value;

    public BitArray32(int value) => _value = value;
    public BitArray32() : this(0) { }

    public bool this[int index]
    {
        readonly get => TOMathUtils.BitOperation.GetBit(_value, index);
        set => TOMathUtils.BitOperation.SetBit(ref _value, index, value);
    }

    public bool this[Index index]
    {
        readonly get => this[index.GetOffset(32)];
        set => this[index.GetOffset(32)] = value;
    }

    public readonly bool Equals(BitArray32 other) => _value == other._value;
    public override readonly bool Equals(object obj) => obj is BitArray32 other && Equals(other);
    public override readonly int GetHashCode() => _value.GetHashCode();
    public static bool operator ==(BitArray32 left, BitArray32 right) => left.Equals(right);
    public static bool operator !=(BitArray32 left, BitArray32 right) => !(left == right);

    public override readonly string ToString() => $"BitArray32 {{ {Convert.ToString(_value, 2).PadLeft(32, '0')} }}";
}

public struct BitArray64 : IEquatable<BitArray64>
{
    private long _value;

    public BitArray64(int value) => _value = value;
    public BitArray64() : this(0) { }

    public bool this[int index]
    {
        readonly get => TOMathUtils.BitOperation.GetBit(_value, index);
        set => TOMathUtils.BitOperation.SetBit(ref _value, index, value);
    }

    public bool this[Index index]
    {
        readonly get => this[index.GetOffset(64)];
        set => this[index.GetOffset(64)] = value;
    }

    public readonly bool Equals(BitArray64 other) => _value == other._value;
    public override readonly bool Equals(object obj) => obj is BitArray64 other && Equals(other);
    public override readonly int GetHashCode() => _value.GetHashCode();
    public static bool operator ==(BitArray64 left, BitArray64 right) => left.Equals(right);
    public static bool operator !=(BitArray64 left, BitArray64 right) => !(left == right);

    public override readonly string ToString() => $"BitArray64 {{ {Convert.ToString(_value, 2).PadLeft(64, '0')} }}";
}

public struct BitArray128 : IEquatable<BitArray128>
{
    private Int128 _value;

    public BitArray128(Int128 value) => _value = value;
    public BitArray128(ulong upper, ulong lower) : this(new Int128(upper, lower)) { }
    public BitArray128() : this(Int128.Zero) { }

    public bool this[int index]
    {
        readonly get => TOMathUtils.BitOperation.GetBit(_value, index);
        set => TOMathUtils.BitOperation.SetBit(ref _value, index, value);
    }

    public bool this[Index index]
    {
        readonly get => this[index.GetOffset(128)];
        set => this[index.GetOffset(128)] = value;
    }

    public readonly bool Equals(BitArray128 other) => _value == other._value;
    public override readonly bool Equals(object obj) => obj is BitArray32 other && Equals(other);
    public override readonly int GetHashCode() => _value.GetHashCode();
    public static bool operator ==(BitArray128 left, BitArray128 right) => left.Equals(right);
    public static bool operator !=(BitArray128 left, BitArray128 right) => !(left == right);

    public override readonly string ToString() => $"BitArray128 {{ {Convert.ToString((long)(_value >> 64), 2).PadLeft(64, '0')} {Convert.ToString((long)_value, 2).PadLeft(64, '0')} }}";
}

public class BitArray
{
    private readonly uint[] _value;
    public int Length { get; }
    private int ArrayLength => (Length + 31) / 32;

    public BitArray(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
        Length = length;
        _value = new uint[ArrayLength];
    }

    public BitArray(uint[] value) => _value = value;

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

    public override string ToString()
    {
        StringBuilder builder = new();
        for (int i = _value.Length - 1; i >= 0; i--)
        {
            builder.Append(Convert.ToString(_value[i], 2).PadLeft(32, '0'));
            if (i != 0)
                builder.Append(' ');
        }
        return $"BitArray {{ {builder} }}";
    }
}