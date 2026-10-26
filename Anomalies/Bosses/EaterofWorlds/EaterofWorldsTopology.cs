namespace Anomalies.Bosses.EaterofWorlds;

// Immutable topology plan: global order survives every split; local edges never cross worm boundaries.
internal static class EaterofWorldsTopology
{
    internal readonly record struct Link(int Head, int Previous, int Next);

    internal static Link[] Create(int segmentCount, int wormCount)
    {
        if (wormCount <= 0 || segmentCount < 2 || segmentCount % wormCount != 0 || segmentCount / wormCount < 2)
            return [];
        int length = segmentCount / wormCount;
        Link[] links = new Link[segmentCount];
        for (int i = 0; i < segmentCount; i++)
        {
            int offset = i % length;
            links[i] = new(i - offset, offset == 0 ? -1 : i - 1, offset == length - 1 ? -1 : i + 1);
        }
        return links;
    }
}
