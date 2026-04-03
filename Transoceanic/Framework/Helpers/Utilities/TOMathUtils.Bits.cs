namespace Transoceanic.Framework.Helpers;

public static partial class TOMathUtils
{
    /// <summary>
    /// 提供位操作方法的类。
    /// </summary>
    public static class BitOperation
    {
        /// <summary>
        /// 获取传入数值在指定位索引的位值（以 <see langword="bool"/> 值表示）。
        /// </summary>
        /// <param name="number">传入数值。</param>
        /// <param name="bitIndex">位索引。须为 [0, 7] 范围内的整数。</param>
        /// <returns>传入数值在指定位索引的位值是否为 <c>1</c>。</returns>
        public static bool GetBit(sbyte number, int bitIndex)
        {
            if (bitIndex is < 0 or >= 8)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 7].");
            return (number & (1 << bitIndex)) != 0;
        }

        /// <summary>
        /// 获取传入数值在指定位索引的位值（以 <see langword="bool"/> 值表示）。
        /// </summary>
        /// <param name="number">传入数值。</param>
        /// <param name="bitIndex">位索引。须为 [0, 7] 范围内的整数。</param>
        /// <returns>传入数值在指定位索引的位值是否为 <c>1</c>。</returns>
        public static bool GetBit(byte number, int bitIndex)
        {
            if (bitIndex is < 0 or >= 8)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 7].");
            return (number & (1 << bitIndex)) != 0;
        }

        /// <summary>
        /// 获取传入数值在指定位索引的位值（以 <see langword="bool"/> 值表示）。
        /// </summary>
        /// <param name="number">传入数值。</param>
        /// <param name="bitIndex">位索引。须为 [0, 15] 范围内的整数。</param>
        /// <returns>传入数值在指定位索引的位值是否为 <c>1</c>。</returns>
        public static bool GetBit(short number, int bitIndex)
        {
            if (bitIndex is < 0 or >= 16)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 15].");
            return (number & (1 << bitIndex)) != 0;
        }

        /// <summary>
        /// 获取传入数值在指定位索引的位值（以 <see langword="bool"/> 值表示）。
        /// </summary>
        /// <param name="number">传入数值。</param>
        /// <param name="bitIndex">位索引。须为 [0, 15] 范围内的整数。</param>
        /// <returns>传入数值在指定位索引的位值是否为 <c>1</c>。</returns>
        public static bool GetBit(ushort number, int bitIndex)
        {
            if (bitIndex is < 0 or >= 16)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 15].");
            return (number & (1 << bitIndex)) != 0;
        }

        /// <summary>
        /// 获取传入数值在指定位索引的位值（以 <see langword="bool"/> 值表示）。
        /// </summary>
        /// <param name="number">传入数值。</param>
        /// <param name="bitIndex">位索引。须为 [0, 31] 范围内的整数。</param>
        /// <returns>传入数值在指定位索引的位值是否为 <c>1</c>。</returns>
        public static bool GetBit(int number, int bitIndex)
        {
            if (bitIndex is < 0 or >= 32)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 31].");
            return (number & (1 << bitIndex)) != 0;
        }

        /// <summary>
        /// 获取传入数值在指定位索引的位值（以 <see langword="bool"/> 值表示）。
        /// </summary>
        /// <param name="number">传入数值。</param>
        /// <param name="bitIndex">位索引。须为 [0, 31] 范围内的整数。</param>
        /// <returns>传入数值在指定位索引的位值是否为 <c>1</c>。</returns>
        public static bool GetBit(uint number, int bitIndex)
        {
            if (bitIndex is < 0 or >= 32)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 31].");
            return (number & (1u << bitIndex)) != 0;
        }

        /// <summary>
        /// 获取传入数值在指定位索引的位值（以 <see langword="bool"/> 值表示）。
        /// </summary>
        /// <param name="number">传入数值。</param>
        /// <param name="bitIndex">位索引。须为 [0, 63] 范围内的整数。</param>
        /// <returns>传入数值在指定位索引的位值是否为 <c>1</c>。</returns>
        public static bool GetBit(long number, int bitIndex)
        {
            if (bitIndex is < 0 or >= 64)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 63].");
            return (number & (1L << bitIndex)) != 0;
        }

        /// <summary>
        /// 获取传入数值在指定位索引的位值（以 <see langword="bool"/> 值表示）。
        /// </summary>
        /// <param name="number">传入数值。</param>
        /// <param name="bitIndex">位索引。须为 [0, 63] 范围内的整数。</param>
        /// <returns>传入数值在指定位索引的位值是否为 <c>1</c>。</returns>
        public static bool GetBit(ulong number, int bitIndex)
        {
            if (bitIndex is < 0 or >= 64)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 63].");
            return (number & (1ul << bitIndex)) != 0;
        }

        /// <summary>
        /// 获取传入数值在指定位索引的位值（以 <see langword="bool"/> 值表示）。
        /// </summary>
        /// <param name="number">传入数值。</param>
        /// <param name="bitIndex">位索引。须为 [0, 127] 范围内的整数。</param>
        /// <returns>传入数值在指定位索引的位值是否为 <c>1</c>。</returns>
        public static bool GetBit(Int128 number, int bitIndex)
        {
            if (bitIndex is < 0 or >= 128)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 127].");
            return (number & (Int128.One << bitIndex)) != 0;
        }

        /// <summary>
        /// 获取传入数值在指定位索引的位值（以 <see langword="bool"/> 值表示）。
        /// </summary>
        /// <param name="number">传入数值。</param>
        /// <param name="bitIndex">位索引。须为 [0, 127] 范围内的整数。</param>
        /// <returns>传入数值在指定位索引的位值是否为 <c>1</c>。</returns>
        public static bool GetBit(UInt128 number, int bitIndex)
        {
            if (bitIndex is < 0 or >= 128)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 127].");
            return (number & (UInt128.One << bitIndex)) != 0;
        }

        /// <summary>
        /// 将传入数值在指定位索引的位设置为指定值，并直接修改原数值。
        /// </summary>
        /// <param name="number">传入数值（引用）。</param>
        /// <param name="bitIndex">位索引。须为 [0, 7] 范围内的整数。</param>
        /// <param name="value">要设置的位值。<see langword="true"/> 表示将该位设置为 <c>1</c>，<see langword="false"/> 表示将该位设置为 <c>0</c>。</param>
        public static void SetBit(ref sbyte number, int bitIndex, bool value)
        {
            if (bitIndex is < 0 or >= 8)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 7].");
            number = (sbyte)((number & ~(1 << bitIndex)) | (value.ToInt() << bitIndex));
        }

        /// <summary>
        /// 将传入数值在指定位索引的位设置为指定值，并直接修改原数值。
        /// </summary>
        /// <param name="number">传入数值（引用）。</param>
        /// <param name="bitIndex">位索引。须为 [0, 7] 范围内的整数。</param>
        /// <param name="value">要设置的位值。<see langword="true"/> 表示将该位设置为 <c>1</c>，<see langword="false"/> 表示将该位设置为 <c>0</c>。</param>
        public static void SetBit(ref byte number, int bitIndex, bool value)
        {
            if (bitIndex is < 0 or >= 8)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 7].");
            number = (byte)((number & ~(1 << bitIndex)) | (value.ToInt() << bitIndex));
        }

        /// <summary>
        /// 将传入数值在指定位索引的位设置为指定值，并直接修改原数值。
        /// </summary>
        /// <param name="number">传入数值（引用）。</param>
        /// <param name="bitIndex">位索引。须为 [0, 15] 范围内的整数。</param>
        /// <param name="value">要设置的位值。<see langword="true"/> 表示将该位设置为 <c>1</c>，<see langword="false"/> 表示将该位设置为 <c>0</c>。</param>
        public static void SetBit(ref short number, int bitIndex, bool value)
        {
            if (bitIndex is < 0 or >= 16)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 15].");
            number = (short)((number & ~(1 << bitIndex)) | (value.ToInt() << bitIndex));
        }

        /// <summary>
        /// 将传入数值在指定位索引的位设置为指定值，并直接修改原数值。
        /// </summary>
        /// <param name="number">传入数值（引用）。</param>
        /// <param name="bitIndex">位索引。须为 [0, 15] 范围内的整数。</param>
        /// <param name="value">要设置的位值。<see langword="true"/> 表示将该位设置为 <c>1</c>，<see langword="false"/> 表示将该位设置为 <c>0</c>。</param>
        public static void SetBit(ref ushort number, int bitIndex, bool value)
        {
            if (bitIndex is < 0 or >= 16)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 15].");
            number = (ushort)((number & ~(1 << bitIndex)) | (value.ToInt() << bitIndex));
        }

        /// <summary>
        /// 将传入数值在指定位索引的位设置为指定值，并直接修改原数值。
        /// </summary>
        /// <param name="number">传入数值（引用）。</param>
        /// <param name="bitIndex">位索引。须为 [0, 31] 范围内的整数。</param>
        /// <param name="value">要设置的位值。<see langword="true"/> 表示将该位设置为 <c>1</c>，<see langword="false"/> 表示将该位设置为 <c>0</c>。</param>
        public static void SetBit(ref int number, int bitIndex, bool value)
        {
            if (bitIndex is < 0 or >= 32)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 31].");
            number = (number & ~(1 << bitIndex)) | (value.ToInt() << bitIndex);
        }

        /// <summary>
        /// 将传入数值在指定位索引的位设置为指定值，并直接修改原数值。
        /// </summary>
        /// <param name="number">传入数值（引用）。</param>
        /// <param name="bitIndex">位索引。须为 [0, 31] 范围内的整数。</param>
        /// <param name="value">要设置的位值。<see langword="true"/> 表示将该位设置为 <c>1</c>，<see langword="false"/> 表示将该位设置为 <c>0</c>。</param>
        public static void SetBit(ref uint number, int bitIndex, bool value)
        {
            if (bitIndex is < 0 or >= 32)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 31].");
            number = (number & ~(1u << bitIndex)) | ((uint)value.ToInt() << bitIndex);
        }

        /// <summary>
        /// 将传入数值在指定位索引的位设置为指定值，并直接修改原数值。
        /// </summary>
        /// <param name="number">传入数值（引用）。</param>
        /// <param name="bitIndex">位索引。须为 [0, 63] 范围内的整数。</param>
        /// <param name="value">要设置的位值。<see langword="true"/> 表示将该位设置为 <c>1</c>，<see langword="false"/> 表示将该位设置为 <c>0</c>。</param>
        public static void SetBit(ref long number, int bitIndex, bool value)
        {
            if (bitIndex is < 0 or >= 64)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 63].");
            number = (number & ~(1L << bitIndex)) | ((long)value.ToInt() << bitIndex);
        }

        /// <summary>
        /// 将传入数值在指定位索引的位设置为指定值，并直接修改原数值。
        /// </summary>
        /// <param name="number">传入数值（引用）。</param>
        /// <param name="bitIndex">位索引。须为 [0, 63] 范围内的整数。</param>
        /// <param name="value">要设置的位值。<see langword="true"/> 表示将该位设置为 <c>1</c>，<see langword="false"/> 表示将该位设置为 <c>0</c>。</param>
        public static void SetBit(ref ulong number, int bitIndex, bool value)
        {
            if (bitIndex is < 0 or >= 64)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 63].");
            number = (number & ~(1ul << bitIndex)) | ((ulong)value.ToInt() << bitIndex);
        }

        /// <summary>
        /// 将传入数值在指定位索引的位设置为指定值，并直接修改原数值。
        /// </summary>
        /// <param name="number">传入数值（引用）。</param>
        /// <param name="bitIndex">位索引。须为 [0, 127] 范围内的整数。</param>
        /// <param name="value">要设置的位值。<see langword="true"/> 表示将该位设置为 <c>1</c>，<see langword="false"/> 表示将该位设置为 <c>0</c>。</param>
        public static void SetBit(ref Int128 number, int bitIndex, bool value)
        {
            if (bitIndex is < 0 or >= 128)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 127].");
            number = (number & ~(Int128.One << bitIndex)) | ((Int128)value.ToInt() << bitIndex);
        }

        /// <summary>
        /// 将传入数值在指定位索引的位设置为指定值，并直接修改原数值。
        /// </summary>
        /// <param name="number">传入数值（引用）。</param>
        /// <param name="bitIndex">位索引。须为 [0, 127] 范围内的整数。</param>
        /// <param name="value">要设置的位值。<see langword="true"/> 表示将该位设置为 <c>1</c>，<see langword="false"/> 表示将该位设置为 <c>0</c>。</param>
        public static void SetBit(ref UInt128 number, int bitIndex, bool value)
        {
            if (bitIndex is < 0 or >= 128)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be in the range [0, 127].");
            number = (number & ~(UInt128.One << bitIndex)) | ((UInt128)value.ToInt() << bitIndex);
        }
    }
}