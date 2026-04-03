namespace Transoceanic.DataStructures;

//含扩展

[StructLayout(LayoutKind.Explicit)]
public struct Union32
{
    static Union32()
    {
        if (Unsafe.SizeOf<Union32>() != 4)
            throw new InvalidOperationException("Union32 must be 4 bytes in size.");
    }

    [FieldOffset(0)] public float f;
    [FieldOffset(0)] public int i;
    [FieldOffset(0)] public short short0;
    [FieldOffset(2)] public short short1;
    [FieldOffset(0)] public byte byte0;
    [FieldOffset(1)] public byte byte1;
    [FieldOffset(2)] public byte byte2;
    [FieldOffset(3)] public byte byte3;
    [FieldOffset(0)] public BitArray32 bits;

    public Union32(float f) => this.f = f;

    public T GetValue<T>() where T : unmanaged
    {
        if (Unsafe.SizeOf<T>() != 4)
            throw new InvalidOperationException($"Type {typeof(T)} must be 4 bytes in size.");

        return Unsafe.As<float, T>(ref f);
    }

    public static explicit operator Union32(float f) => new(f);
    public static explicit operator Union32(int i) => new() { i = i };
    public static explicit operator Union32(BitArray32 bits) => new() { bits = bits };
}

[StructLayout(LayoutKind.Explicit)]
public struct Union64
{
    static Union64()
    {
        if (Unsafe.SizeOf<Union64>() != 8)
            throw new InvalidOperationException("Union64 must be 8 bytes in size.");
    }

    [FieldOffset(0)] public double d;
    [FieldOffset(0)] public long l;
    [FieldOffset(0)] public BitArray64 bits;

    public Union64(double d) => this.d = d;

    public T GetValue<T>() where T : unmanaged
    {
        if (Unsafe.SizeOf<T>() != 8)
            throw new InvalidOperationException($"Type {typeof(T)} must be 8 bytes in size.");

        return Unsafe.As<double, T>(ref d);
    }

    public static explicit operator Union64(double d) => new(d);
    public static explicit operator Union64(long l) => new() { l = l };
    public static explicit operator Union64(BitArray64 bits) => new() { bits = bits };
}