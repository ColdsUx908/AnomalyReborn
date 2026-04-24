namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    /// <summary>
    /// 为 <see cref="Union32"/> 提供泛型值设置扩展方法，允许将任意 4 字节非托管类型写入联合体内存。
    /// </summary>
    /// <typeparam name="T">要写入的非托管类型，其大小必须为 4 字节。</typeparam>
    /// <param name="union">要操作的 <see cref="Union32"/> 实例的引用。</param>
    /// <exception cref="InvalidOperationException">
    /// 当 <typeparamref name="T"/> 的大小不为 4 字节时抛出。
    /// </exception>
    extension<T>(ref Union32 union) where T : unmanaged
    {
        /// <summary>
        /// 将指定值 <paramref name="value"/> 按位写入当前 <see cref="Union32"/> 联合体，覆盖原有数据。
        /// </summary>
        /// <param name="value">要写入的 <typeparamref name="T"/> 类型值。</param>
        public void SetValue(T value)
        {
            if (Unsafe.SizeOf<T>() != 4)
                throw new InvalidOperationException($"Type {typeof(T)} must be 4 bytes in size.");

            union.f = Unsafe.As<T, float>(ref value);
        }
    }

    /// <summary>
    /// 为 <see cref="Union64"/> 提供泛型值设置扩展方法，允许将任意 8 字节非托管类型写入联合体内存。
    /// </summary>
    /// <typeparam name="T">要写入的非托管类型，其大小必须为 8 字节。</typeparam>
    /// <param name="union">要操作的 <see cref="Union64"/> 实例的引用。</param>
    /// <exception cref="InvalidOperationException">
    /// 当 <typeparamref name="T"/> 的大小不为 8 字节时抛出。
    /// </exception>
    extension<T>(ref Union64 union) where T : unmanaged
    {
        /// <summary>
        /// 将指定值 <paramref name="value"/> 按位写入当前 <see cref="Union64"/> 联合体，覆盖原有数据。
        /// </summary>
        /// <param name="value">要写入的 <typeparamref name="T"/> 类型值。</param>
        public void SetValue(T value)
        {
            if (Unsafe.SizeOf<T>() != 8)
                throw new InvalidOperationException($"Type {typeof(T)} must be 8 bytes in size.");

            union.d = Unsafe.As<T, double>(ref value);
        }
    }
}

