using Anomalies.Bosses.EaterofWorlds;

int checks = 0;
void Require(bool condition, string message)
{
    checks++;
    if (!condition) throw new InvalidOperationException(message);
}
foreach (int count in new[] { 60, 90 })
{
    var original = EaterofWorldsTopology.Create(count, 1);
    for (int cycle = 0; cycle < 20; cycle++)
    {
        var links = EaterofWorldsTopology.Create(count, 3);
        var visited = new HashSet<int>();
        int headCount = 0;
        for (int i = 0; i < count; i++)
        {
            if (links[i].Previous >= 0) continue;
            headCount++;
            int traversed = 0;
            for (int index = i; index >= 0; index = links[index].Next)
            {
                Require(visited.Add(index), "A local chain contains a cycle or duplicate segment");
                Require(links[index].Head == i, "A local chain crosses its owner boundary");
                if (links[index].Next >= 0)
                    Require(links[links[index].Next].Previous == index, "Forward/back links disagree");
                traversed++;
            }
            Require(traversed == count / 3, "Unequal split");
        }
        Require(headCount == 3 && visited.Count == count, "Lost segment or incorrect head count");
        Require(EaterofWorldsTopology.Create(count, 1).SequenceEqual(original), "Merge changed global ordering");
    }
}
foreach (var input in new[] { (0, 1), (1, 1), (60, 0), (60, -1), (60, 7), (60, 60), (60, 61) })
    Require(EaterofWorldsTopology.Create(input.Item1, input.Item2).Length == 0, "Invalid topology accepted");
Require(EaterofWorldsTopology.Create(2, 1).Length == 2, "Minimum head/tail pair rejected");
Console.WriteLine($"Passed {checks} topology assertions (60/90 segments, repeated split/merge, invalid requests).");
