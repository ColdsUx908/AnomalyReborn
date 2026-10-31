namespace Anomalies.Common;

// Capacity is local storage, not part of the sparse snapshot wire format.
internal static class AnomalyAICapacity
{
    internal static T[] Grow<T>(T[] array, int count, int tailCount)
    {
        int oldCount = array.Length - tailCount;
        if (count <= oldCount)
            return array;
        T[] expanded = new T[count + tailCount];
        Array.Copy(array, expanded, oldCount);
        Array.Copy(array, oldCount, expanded, count, tailCount);
        return expanded;
    }

    internal static Union32[] Read32(BinaryReader reader, Union32[] current, int tailCount, int maximum)
    {
        Union32[] snapshot = new Union32[maximum + tailCount];
        TONetUtils.ReadAI32Snapshot(reader, snapshot, tailCount);
        int count = current.Length - tailCount;
        for (int i = maximum - 1; i >= count; i--)
            if (snapshot[i].I != 0)
            {
                count = i + 1;
                break;
            }
        Union32[] result = new Union32[count + tailCount];
        Array.Copy(snapshot, result, count);
        return result;
    }

    internal static Union64[] Read64(BinaryReader reader, Union64[] current, int maximum)
    {
        Union64[] snapshot = new Union64[maximum + 1];
        TONetUtils.ReadAI64Snapshot(reader, snapshot, 1);
        int count = current.Length - 1;
        for (int i = maximum - 1; i >= count; i--)
            if (snapshot[i].L != 0)
            {
                count = i + 1;
                break;
            }
        Union64[] result = new Union64[count + 1];
        Array.Copy(snapshot, result, count);
        return result;
    }
}
