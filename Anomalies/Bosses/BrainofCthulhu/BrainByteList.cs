using System.Collections;

namespace Anomalies.Bosses.BrainofCthulhu;

// Packed bytes keep attack queues and multiplayer target history in the normal AI snapshot.
internal static class BrainPackedData
{
    public static byte Read(NPC npc, int offset)
    {
        AnomalyGlobalNPC data = npc.GetGlobalNPC<AnomalyGlobalNPC>();
        return offset < 176 ? (byte)((uint)data.AnomalyAI32[20 + offset / 4].I >> (offset % 4 * 8)) : (byte)((ulong)data.AnomalyAI64[1 + (offset - 176) / 8].L >> ((offset - 176) % 8 * 8));
    }

    public static void Write(NPC npc, int offset, byte value)
    {
        AnomalyGlobalNPC data = npc.GetGlobalNPC<AnomalyGlobalNPC>();
        if (offset < 176)
        {
            int slot = 20 + offset / 4, shift = offset % 4 * 8;
            uint old = (uint)data.AnomalyAI32[slot].I;
            data.AnomalyAI32[slot].I = (int)((old & ~(255u << shift)) | ((uint)value << shift));
            data.AIChanged32[slot] = true;
        }
        else
        {
            int slot = 1 + (offset - 176) / 8, shift = (offset - 176) % 8 * 8;
            ulong old = (ulong)data.AnomalyAI64[slot].L;
            data.AnomalyAI64[slot].L = (long)((old & ~(255ul << shift)) | ((ulong)value << shift));
            data.AIChanged64[slot] = true;
        }
    }
}

public readonly struct BrainByteList<T>(NPC npc, int offset, int capacity, int countSlot) : IEnumerable<T> where T : struct, IConvertible
{
    public int Count => npc.GetGlobalNPC<AnomalyGlobalNPC>().AnomalyAI32[countSlot].I;

    private void SetCount(int value)
    {
        AnomalyGlobalNPC data = npc.GetGlobalNPC<AnomalyGlobalNPC>();
        data.AnomalyAI32[countSlot].I = value;
        data.AIChanged32[countSlot] = true;
    }

    public T this[int index]
    {
        get
        {
            if ((uint)index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            byte value = BrainPackedData.Read(npc, offset + index);
            return typeof(T).IsEnum ? (T)Enum.ToObject(typeof(T), value) : (T)(object)value;
        }
    }

    public void Add(T value)
    {
        if (Count >= capacity)
            throw new InvalidOperationException("Brain attack queue capacity exceeded.");
        BrainPackedData.Write(npc, offset + Count, value.ToByte(null));
        SetCount(Count + 1);
    }

    public void Clear() => SetCount(0);
    public void Replace(IEnumerable<T> values)
    {
        Clear();
        foreach (T value in values)
            Add(value);
    }

    public bool Contains(T value) => this.Any(v => EqualityComparer<T>.Default.Equals(v, value));
    public void Remove(T value)
    {
        for (int i = 0; i < Count; i++)
            if (EqualityComparer<T>.Default.Equals(this[i], value))
            {
                RemoveAt(i);
                return;
            }
    }

    public void RemoveAt(int index)
    {
        if ((uint)index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        for (int i = index; i < Count - 1; i++)
            BrainPackedData.Write(npc, offset + i, BrainPackedData.Read(npc, offset + i + 1));
        SetCount(Count - 1);
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
            yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public readonly struct BrainTargetSet(NPC npc) : IEnumerable<int>
{
    public int Count => this.Count(_ => true);

    public bool Contains(int value) => (uint)value < 256 && (BrainPackedData.Read(npc, 264 + value / 8) & (1 << (value % 8))) != 0;
    public void Add(int value)
    {
        if ((uint)value >= Main.maxPlayers)
            return;
        int offset = 264 + value / 8;
        BrainPackedData.Write(npc, offset, (byte)(BrainPackedData.Read(npc, offset) | (1 << (value % 8))));
    }

    public void Clear()
    {
        for (int i = 0; i < 32; i++)
            BrainPackedData.Write(npc, 264 + i, 0);
    }

    public IEnumerator<int> GetEnumerator()
    {
        for (int i = 0; i < Main.maxPlayers; i++)
            if (Contains(i))
                yield return i;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
