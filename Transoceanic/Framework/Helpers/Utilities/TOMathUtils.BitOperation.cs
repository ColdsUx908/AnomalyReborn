// Developed by ColdsUx

namespace Transoceanic.Framework.Helpers;

public static partial class TOMathUtils
{
    /// <summary>
    /// 提供针对多种整数类型的位操作（获取与设置指定位的值）。
    /// </summary>
    public static class BitOperation
    {
        /// <summary>
        /// 获取有符号 8 位整数在指定位索引的位值。
        /// </summary>
        /// <param name="number">目标数值。</param>
        /// <param name="bitIndex">位索引，必须介于 0 到 7 之间。</param>
        /// <returns>若该位为 1 则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="bitIndex"/> 不在 [0, 7] 范围内时抛出。</exception>
        public static bool GetBit(sbyte number, int bitIndex)
        {
            if (bitIndex is < 0 or >= 8)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 7].");
            return (number & (1 << bitIndex)) != 0;
        }

        /// <summary>
        /// 获取无符号 8 位整数在指定位索引的位值。
        /// </summary>
        /// <inheritdoc cref="GetBit(sbyte, int)"/>
        public static bool GetBit(byte number, int bitIndex)
        {
            if (bitIndex is < 0 or >= 8)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 7].");
            return (number & (1 << bitIndex)) != 0;
        }

        /// <summary>
        /// 获取有符号 16 位整数在指定位索引的位值。
        /// </summary>
        /// <param name="number">目标数值。</param>
        /// <param name="bitIndex">位索引，必须介于 0 到 15 之间。</param>
        /// <returns>若该位为 1 则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="bitIndex"/> 不在 [0, 15] 范围内时抛出。</exception>
        public static bool GetBit(short number, int bitIndex)
        {
            if (bitIndex is < 0 or >= 16)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 15].");
            return (number & (1 << bitIndex)) != 0;
        }

        /// <summary>
        /// 获取无符号 16 位整数在指定位索引的位值。
        /// </summary>
        /// <inheritdoc cref="GetBit(short, int)"/>
        public static bool GetBit(ushort number, int bitIndex)
        {
            if (bitIndex is < 0 or >= 16)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 15].");
            return (number & (1 << bitIndex)) != 0;
        }

        /// <summary>
        /// 获取有符号 32 位整数在指定位索引的位值。
        /// </summary>
        /// <param name="number">目标数值。</param>
        /// <param name="bitIndex">位索引，必须介于 0 到 31 之间。</param>
        /// <returns>若该位为 1 则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="bitIndex"/> 不在 [0, 31] 范围内时抛出。</exception>
        public static bool GetBit(int number, int bitIndex)
        {
            if (bitIndex is < 0 or >= 32)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 31].");
            return (number & (1 << bitIndex)) != 0;
        }

        /// <summary>
        /// 获取无符号 32 位整数在指定位索引的位值。
        /// </summary>
        /// <inheritdoc cref="GetBit(int, int)"/>
        public static bool GetBit(uint number, int bitIndex)
        {
            if (bitIndex is < 0 or >= 32)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 31].");
            return (number & (1u << bitIndex)) != 0;
        }

        /// <summary>
        /// 获取有符号 64 位整数在指定位索引的位值。
        /// </summary>
        /// <param name="number">目标数值。</param>
        /// <param name="bitIndex">位索引，必须介于 0 到 63 之间。</param>
        /// <returns>若该位为 1 则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="bitIndex"/> 不在 [0, 63] 范围内时抛出。</exception>
        public static bool GetBit(long number, int bitIndex)
        {
            if (bitIndex is < 0 or >= 64)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 63].");
            return (number & (1L << bitIndex)) != 0;
        }

        /// <summary>
        /// 获取无符号 64 位整数在指定位索引的位值。
        /// </summary>
        /// <inheritdoc cref="GetBit(long, int)"/>
        public static bool GetBit(ulong number, int bitIndex)
        {
            if (bitIndex is < 0 or >= 64)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 63].");
            return (number & (1ul << bitIndex)) != 0;
        }

        /// <summary>
        /// 获取有符号 128 位整数 (<see cref="Int128"/>) 在指定位索引的位值。
        /// </summary>
        /// <param name="number">目标数值。</param>
        /// <param name="bitIndex">位索引，必须介于 0 到 127 之间。</param>
        /// <returns>若该位为 1 则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="bitIndex"/> 不在 [0, 127] 范围内时抛出。</exception>
        public static bool GetBit(Int128 number, int bitIndex)
        {
            if (bitIndex is < 0 or >= 128)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 127].");
            return (number & (Int128.One << bitIndex)) != 0;
        }

        /// <summary>
        /// 获取无符号 128 位整数 (<see cref="UInt128"/>) 在指定位索引的位值。
        /// </summary>
        /// <inheritdoc cref="GetBit(Int128, int)"/>
        public static bool GetBit(UInt128 number, int bitIndex)
        {
            if (bitIndex is < 0 or >= 128)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 127].");
            return (number & (UInt128.One << bitIndex)) != 0;
        }

        /// <summary>
        /// 将有符号 8 位整数的指定位设置为指定值。
        /// </summary>
        /// <param name="number">目标数值的引用。</param>
        /// <param name="bitIndex">位索引，必须介于 0 到 7 之间。</param>
        /// <param name="value">若为 <see langword="true"/> 则置位为 1，否则置为 0。</param>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="bitIndex"/> 不在 [0, 7] 范围内时抛出。</exception>
        public static void SetBit(ref sbyte number, int bitIndex, bool value)
        {
            if (bitIndex is < 0 or >= 8)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 7].");
            number = (sbyte)((number & ~(1 << bitIndex)) | (value.ToInt() << bitIndex));
        }

        /// <summary>
        /// 将无符号 8 位整数的指定位设置为指定值。
        /// </summary>
        /// <inheritdoc cref="SetBit(ref sbyte, int, bool)"/>
        public static void SetBit(ref byte number, int bitIndex, bool value)
        {
            if (bitIndex is < 0 or >= 8)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 7].");
            number = (byte)((number & ~(1 << bitIndex)) | (value.ToInt() << bitIndex));
        }

        /// <summary>
        /// 将有符号 16 位整数的指定位设置为指定值。
        /// </summary>
        /// <param name="number">目标数值的引用。</param>
        /// <param name="bitIndex">位索引，必须介于 0 到 15 之间。</param>
        /// <param name="value">若为 <see langword="true"/> 则置位为 1，否则置为 0。</param>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="bitIndex"/> 不在 [0, 15] 范围内时抛出。</exception>
        public static void SetBit(ref short number, int bitIndex, bool value)
        {
            if (bitIndex is < 0 or >= 16)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 15].");
            number = (short)((number & ~(1 << bitIndex)) | (value.ToInt() << bitIndex));
        }

        /// <summary>
        /// 将无符号 16 位整数的指定位设置为指定值。
        /// </summary>
        /// <inheritdoc cref="SetBit(ref short, int, bool)"/>
        public static void SetBit(ref ushort number, int bitIndex, bool value)
        {
            if (bitIndex is < 0 or >= 16)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 15].");
            number = (ushort)((number & ~(1 << bitIndex)) | (value.ToInt() << bitIndex));
        }

        /// <summary>
        /// 将有符号 32 位整数的指定位设置为指定值。
        /// </summary>
        /// <param name="number">目标数值的引用。</param>
        /// <param name="bitIndex">位索引，必须介于 0 到 31 之间。</param>
        /// <param name="value">若为 <see langword="true"/> 则置位为 1，否则置为 0。</param>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="bitIndex"/> 不在 [0, 31] 范围内时抛出。</exception>
        public static void SetBit(ref int number, int bitIndex, bool value)
        {
            if (bitIndex is < 0 or >= 32)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 31].");
            number = (number & ~(1 << bitIndex)) | (value.ToInt() << bitIndex);
        }

        /// <summary>
        /// 将无符号 32 位整数的指定位设置为指定值。
        /// </summary>
        /// <inheritdoc cref="SetBit(ref int, int, bool)"/>
        public static void SetBit(ref uint number, int bitIndex, bool value)
        {
            if (bitIndex is < 0 or >= 32)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 31].");
            number = (number & ~(1u << bitIndex)) | ((uint)value.ToInt() << bitIndex);
        }

        /// <summary>
        /// 将有符号 64 位整数的指定位设置为指定值。
        /// </summary>
        /// <param name="number">目标数值的引用。</param>
        /// <param name="bitIndex">位索引，必须介于 0 到 63 之间。</param>
        /// <param name="value">若为 <see langword="true"/> 则置位为 1，否则置为 0。</param>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="bitIndex"/> 不在 [0, 63] 范围内时抛出。</exception>
        public static void SetBit(ref long number, int bitIndex, bool value)
        {
            if (bitIndex is < 0 or >= 64)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 63].");
            number = (number & ~(1L << bitIndex)) | ((long)value.ToInt() << bitIndex);
        }

        /// <summary>
        /// 将无符号 64 位整数的指定位设置为指定值。
        /// </summary>
        /// <inheritdoc cref="SetBit(ref long, int, bool)"/>
        public static void SetBit(ref ulong number, int bitIndex, bool value)
        {
            if (bitIndex is < 0 or >= 64)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 63].");
            number = (number & ~(1ul << bitIndex)) | ((ulong)value.ToInt() << bitIndex);
        }

        /// <summary>
        /// 将有符号 128 位整数的指定位设置为指定值。
        /// </summary>
        /// <param name="number">目标数值的引用。</param>
        /// <param name="bitIndex">位索引，必须介于 0 到 127 之间。</param>
        /// <param name="value">若为 <see langword="true"/> 则置位为 1，否则置为 0。</param>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="bitIndex"/> 不在 [0, 127] 范围内时抛出。</exception>
        public static void SetBit(ref Int128 number, int bitIndex, bool value)
        {
            if (bitIndex is < 0 or >= 128)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 127].");
            number = (number & ~(Int128.One << bitIndex)) | ((Int128)value.ToInt() << bitIndex);
        }

        /// <summary>
        /// 将无符号 128 位整数的指定位设置为指定值。
        /// </summary>
        /// <inheritdoc cref="SetBit(ref Int128, int, bool)"/>
        public static void SetBit(ref UInt128 number, int bitIndex, bool value)
        {
            if (bitIndex is < 0 or >= 128)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 127].");
            number = (number & ~(UInt128.One << bitIndex)) | ((UInt128)value.ToInt() << bitIndex);
        }
    }
}