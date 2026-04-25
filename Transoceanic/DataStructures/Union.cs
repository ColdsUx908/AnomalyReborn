// Designed by ColdsUx

namespace Transoceanic.DataStructures;

//含扩展

/// <summary>
/// 表示一个 32 位（4 字节）的联合体结构，允许以多种类型（float, int, 字节片段等）安全地访问同一内存区域。
/// </summary>
/// <remarks>
/// 该结构通过 <see cref="System.Runtime.InteropServices.LayoutKind.Explicit"/> 使所有字段共享相同的内存起始地址。
/// 静态构造函数会在运行时验证结构大小是否为 4 字节，若不符则抛出异常，确保平台兼容性。
/// </remarks>
[StructLayout(LayoutKind.Explicit)]
public struct Union32
{
    /// <summary>
    /// 静态构造函数。运行时验证结构体大小必须为 4 字节，否则抛出 <see cref="InvalidOperationException"/>。
    /// </summary>
    /// <exception cref="InvalidOperationException">当结构体大小不为 4 字节时引发。</exception>
    static Union32()
    {
        if (Unsafe.SizeOf<Union32>() != 4)
            throw new InvalidOperationException("Union32 must be 4 bytes in size.");
    }

    /// <summary>32 位单精度浮点数视图。</summary>
    [FieldOffset(0)] public float f;
    /// <summary>32 位有符号整数视图。</summary>
    [FieldOffset(0)] public int i;
    /// <summary>低 16 位有符号短整数视图。</summary>
    [FieldOffset(0)] public short short0;
    /// <summary>高 16 位有符号短整数视图。</summary>
    [FieldOffset(2)] public short short1;
    /// <summary>最低有效字节（字节 0）。</summary>
    [FieldOffset(0)] public byte byte0;
    /// <summary>字节 1。</summary>
    [FieldOffset(1)] public byte byte1;
    /// <summary>字节 2。</summary>
    [FieldOffset(2)] public byte byte2;
    /// <summary>最高有效字节（字节 3）。</summary>
    [FieldOffset(3)] public byte byte3;
    /// <summary>32 位位域视图，允许按位操作。</summary>
    [FieldOffset(0)] public BitArray32 bits;

    /// <summary>
    /// 使用指定的单精度浮点数值初始化 <see cref="Union32"/> 的新实例。
    /// </summary>
    /// <param name="f">初始化的浮点数值。</param>
    public Union32(float f) => this.f = f;

    /// <summary>
    /// 将当前联合体中的内存重新解释为指定 4 字节大小的非托管类型。
    /// </summary>
    /// <typeparam name="T">目标非托管类型，其大小必须为 4 字节。</typeparam>
    /// <returns>重新解释后的 <typeparamref name="T"/> 类型值。</returns>
    /// <exception cref="InvalidOperationException">
    /// 当 <typeparamref name="T"/> 的大小不为 4 字节时抛出。
    /// </exception>
    public T GetValue<T>() where T : unmanaged
    {
        if (Unsafe.SizeOf<T>() != 4)
            throw new InvalidOperationException($"Type {typeof(T)} must be 4 bytes in size.");

        return Unsafe.As<float, T>(ref f);
    }

    /// <summary>从 <see cref="float"/> 显式转换为 <see cref="Union32"/>。</summary>
    public static explicit operator Union32(float f) => new(f);
    /// <summary>从 <see cref="int"/> 显式转换为 <see cref="Union32"/>。</summary>
    public static explicit operator Union32(int i) => new() { i = i };
    /// <summary>从 <see cref="BitArray32"/> 显式转换为 <see cref="Union32"/>。</summary>
    public static explicit operator Union32(BitArray32 bits) => new() { bits = bits };
}

/// <summary>
/// 表示一个 64 位（8 字节）的联合体结构，允许以多种类型（double, long, 位域等）安全地访问同一内存区域。
/// </summary>
/// <remarks>
/// 该结构通过 <see cref="System.Runtime.InteropServices.LayoutKind.Explicit"/> 使所有字段共享相同的内存起始地址。
/// 静态构造函数会在运行时验证结构大小是否为 8 字节，若不符则抛出异常，确保平台兼容性。
/// </remarks>
[StructLayout(LayoutKind.Explicit)]
public struct Union64
{
    /// <summary>
    /// 静态构造函数。运行时验证结构体大小必须为 8 字节，否则抛出 <see cref="InvalidOperationException"/>。
    /// </summary>
    /// <exception cref="InvalidOperationException">当结构体大小不为 8 字节时引发。</exception>
    static Union64()
    {
        if (Unsafe.SizeOf<Union64>() != 8)
            throw new InvalidOperationException("Union64 must be 8 bytes in size.");
    }

    /// <summary>64 位双精度浮点数视图。</summary>
    [FieldOffset(0)] public double d;
    /// <summary>64 位有符号长整数视图。</summary>
    [FieldOffset(0)] public long l;
    /// <summary>64 位位域视图，允许按位操作。</summary>
    [FieldOffset(0)] public BitArray64 bits;

    /// <summary>
    /// 使用指定的双精度浮点数值初始化 <see cref="Union64"/> 的新实例。
    /// </summary>
    /// <param name="d">初始化的双精度浮点数值。</param>
    public Union64(double d) => this.d = d;

    /// <summary>
    /// 将当前联合体中的内存重新解释为指定 8 字节大小的非托管类型。
    /// </summary>
    /// <typeparam name="T">目标非托管类型，其大小必须为 8 字节。</typeparam>
    /// <returns>重新解释后的 <typeparamref name="T"/> 类型值。</returns>
    /// <exception cref="InvalidOperationException">
    /// 当 <typeparamref name="T"/> 的大小不为 8 字节时抛出。
    /// </exception>
    public T GetValue<T>() where T : unmanaged
    {
        if (Unsafe.SizeOf<T>() != 8)
            throw new InvalidOperationException($"Type {typeof(T)} must be 8 bytes in size.");

        return Unsafe.As<double, T>(ref d);
    }

    /// <summary>从 <see cref="double"/> 显式转换为 <see cref="Union64"/>。</summary>
    public static explicit operator Union64(double d) => new(d);
    /// <summary>从 <see cref="long"/> 显式转换为 <see cref="Union64"/>。</summary>
    public static explicit operator Union64(long l) => new() { l = l };
    /// <summary>从 <see cref="BitArray64"/> 显式转换为 <see cref="Union64"/>。</summary>
    public static explicit operator Union64(BitArray64 bits) => new() { bits = bits };
}