// Developed by ColdsUx

namespace Transoceanic.Framework.Helpers.Utilities;

/// <summary>
/// 提供与网络通信相关的便捷工具方法，主要针对 AI 数据的变化收集、传输与重放。
/// </summary>
public static class TONetUtils
{
    /// <summary>
    /// 向 <see cref="BinaryWriter"/> 写入一个 <see cref="Union32"/> 数组中所有被标记为已更改的 AI 数据。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 数组的结构为：前 <c>array.Length - tailCount</c> 个元素是真正的 32 位 AI 数据槽；
    /// 末尾 <paramref name="tailCount"/> 个元素是改动位标记（<see cref="BitArray32"/>）。
    /// 每个改动位标记固定管理最多 32 个数据槽：
    /// 第 0 个改动位对应索引 [0, 31] 的数据槽，
    /// 第 1 个改动位对应索引 [32, 63]，依此类推。
    /// 若某组对应的数据槽数量不足 32（到达数组末尾），则以实际数量为准；
    /// 若某组对应的起始索引已超出数据槽总数，则该组对应的数据槽数量为 0，不会发送任何数据。
    /// </para>
    /// <para>
    /// 本方法会遍历所有改动位，收集发生变化的槽位索引与当前值，将它们写入写入器；
    /// 成功写入所有数据后，才会将末尾的所有改动位重置为零，避免在写入过程中发生异常时丢失改动信息。
    /// </para>
    /// </remarks>
    /// <param name="writer">目标二进制写入器。</param>
    /// <param name="array">
    /// 包含数据槽和末尾改动位标记的数组。长度必须大于 <paramref name="tailCount"/>，
    /// 总长度 = 数据槽个数 + <paramref name="tailCount"/>。
    /// </param>
    /// <param name="tailCount">
    /// 末尾的改动位数量。必须大于 0。每一个 <see cref="BitArray32"/> 固定管理 32 个数据槽。
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="tailCount"/> 小于等于 0 时抛出。
    /// </exception>
    /// <exception cref="ObjectDisposedException">写入器已关闭。</exception>
    /// <exception cref="IOException">发生 I/O 错误。</exception>
    public static void WriteChangedAI32(BinaryWriter writer, Union32[] array, int tailCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(tailCount, 0);

        int dataLength = array.Length - tailCount;
        const int groupSize = 32; // 每个 BitArray32 固定管理 32 个数据槽

        List<(int index, float value)> toSend = [];

        for (int g = 0; g < tailCount; g++)
        {
            int tailIndex = dataLength + g;
            BitArray32 changedBits = array[tailIndex].bits;
            int baseIndex = g * groupSize;

            // 计算当前组实际管理的数据槽数量，可能为 0
            int actualCount = Math.Min(groupSize, Math.Max(0, dataLength - baseIndex));

            for (int i = 0; i < actualCount; i++)
            {
                if (changedBits[i])
                    toSend.Add((baseIndex + i, array[baseIndex + i].f));
            }
        }

        // 先写入所有待同步数据
        writer.Write(toSend.Count);
        foreach ((int index, float value) in toSend)
        {
            writer.Write(index);
            writer.Write(value);
        }

        // 成功写入后统一重置所有改动位
        for (int g = 0; g < tailCount; g++)
            array[dataLength + g].bits = default;
    }

    /// <summary>
    /// 向 <see cref="BinaryWriter"/> 写入一个 <see cref="Union64"/> 数组中所有被标记为已更改的 AI 数据。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 数组的结构为：前 <c>array.Length - tailCount</c> 个元素是真正的 64 位 AI 数据槽；
    /// 末尾 <paramref name="tailCount"/> 个元素是改动位标记（<see cref="BitArray64"/>）。
    /// 每个改动位标记固定管理最多 64 个数据槽：
    /// 第 0 个改动位对应索引 [0, 63]，
    /// 第 1 个改动位对应索引 [64, 127]，依此类推。
    /// 若某组对应的数据槽数量不足 64（到达数组末尾），则以实际数量为准；
    /// 若某组对应的起始索引已超出数据槽总数，则该组对应的数据槽数量为 0，不会发送任何数据。
    /// </para>
    /// <para>
    /// 本方法会遍历所有改动位，收集发生变化的槽位索引与当前值，将它们写入写入器；
    /// 成功写入所有数据后，才会将末尾的所有改动位重置为零，避免在写入过程中发生异常时丢失改动信息。
    /// </para>
    /// </remarks>
    /// <param name="writer">目标二进制写入器。</param>
    /// <param name="array">
    /// 包含数据槽和末尾改动位标记的数组。长度必须大于 <paramref name="tailCount"/>。
    /// </param>
    /// <param name="tailCount">
    /// 末尾的改动位数量。必须大于 0。每个 <see cref="BitArray64"/> 固定管理 64 个数据槽。
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="tailCount"/> 小于等于 0 时抛出。
    /// </exception>
    /// <exception cref="ObjectDisposedException">写入器已关闭。</exception>
    /// <exception cref="IOException">发生 I/O 错误。</exception>
    public static void WriteChangedAI64(BinaryWriter writer, Union64[] array, int tailCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(tailCount, 0);

        int dataLength = array.Length - tailCount;
        const int groupSize = 64; // 每个 BitArray64 固定管理 64 个数据槽

        List<(int index, double value)> toSend = [];

        for (int g = 0; g < tailCount; g++)
        {
            int tailIndex = dataLength + g;
            BitArray64 changedBits = array[tailIndex].bits;
            int baseIndex = g * groupSize;

            int actualCount = Math.Min(groupSize, Math.Max(0, dataLength - baseIndex));

            for (int i = 0; i < actualCount; i++)
            {
                if (changedBits[i])
                    toSend.Add((baseIndex + i, array[baseIndex + i].d));
            }
        }

        // 先写入所有待同步数据
        writer.Write(toSend.Count);
        foreach ((int index, double value) in toSend)
        {
            writer.Write(index);
            writer.Write(value);
        }

        // 成功写入后统一重置所有改动位
        for (int g = 0; g < tailCount; g++)
            array[dataLength + g].bits = default;
    }

    /// <summary>
    /// 从 <see cref="BinaryReader"/> 读取一组 <see cref="Union32"/> 的 AI 数据变化，
    /// 并直接覆写目标数组中对应的数据槽。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 读取格式与 <see cref="WriteChangedAI32"/> 写入格式对应：首先读取一个 <see cref="int"/> 表示变化的条目数量，
    /// 然后依次读取每一对索引（<see cref="int"/>）和单精度浮点值（<see cref="float"/>）。
    /// 接收到的每个索引只要在数组有效范围内，就会直接覆盖对应槽位的浮点值。
    /// 该方法不会重置任何改动位，适用于接收端直接应用同步数据。
    /// </para>
    /// <para>
    /// 若数据流中包含越界索引，那些条目会被静默忽略，不会引发异常。
    /// </para>
    /// </remarks>
    /// <param name="reader">数据源读取器，其当前位置必须位于由 <see cref="WriteChangedAI32"/> 写入的序列开头。</param>
    /// <param name="array">要接收数据的目标 <see cref="Union32"/> 数组。</param>
    /// <exception cref="EndOfStreamException">读取器在期望的位置意外到达流末尾。</exception>
    /// <exception cref="ObjectDisposedException">读取器已关闭。</exception>
    /// <exception cref="IOException">发生 I/O 错误。</exception>
    public static void ReadChangedAI32(BinaryReader reader, Union32[] array)
    {
        int count = reader.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            int index = reader.ReadInt32();
            float value = reader.ReadSingle();
            if (index >= 0 && index < array.Length)
                array[index].f = value;
        }
    }

    /// <summary>
    /// 从 <see cref="BinaryReader"/> 读取一组 <see cref="Union64"/> 的 AI 数据变化，
    /// 并直接覆写目标数组中对应的数据槽。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 读取格式与 <see cref="WriteChangedAI64"/> 写入格式对应：首先读取条目数量（<see cref="int"/>），
    /// 然后读取每一条目的索引（<see cref="int"/>）和双精度浮点值（<see cref="double"/>）。
    /// 有效索引范围内的槽位会被直接覆盖。越界索引会被忽略。
    /// </para>
    /// </remarks>
    /// <param name="reader">数据源读取器，其当前位置必须位于由 <see cref="WriteChangedAI64"/> 写入的序列开头。</param>
    /// <param name="array">要接收数据的目标 <see cref="Union64"/> 数组。</param>
    /// <exception cref="EndOfStreamException">读取器在期望的位置意外到达流末尾。</exception>
    /// <exception cref="ObjectDisposedException">读取器已关闭。</exception>
    /// <exception cref="IOException">发生 I/O 错误。</exception>
    public static void ReadChangedAI64(BinaryReader reader, Union64[] array)
    {
        int count = reader.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            int index = reader.ReadInt32();
            double value = reader.ReadDouble();
            if (index >= 0 && index < array.Length)
                array[index].d = value;
        }
    }
}