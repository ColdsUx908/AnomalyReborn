namespace Anomalies.Bosses.EaterofWorlds;

public sealed class EaterofWorldsSegmentContainer
{
    public NPC NPC { get; }
    // Resolve after type conversion so the container never retains a body behavior for a new head.
    public IEaterofWorldsSegment SegmentBehavior => EaterofWorldsHandler.TryGetSegment(NPC);

    public EaterofWorldsSegmentContainer(NPC npc)
    {
        NPC = npc;
    }

    public EaterofWorldsSegmentContainer(NPC npc, IEaterofWorldsSegment segmentBehavior)
    {
        NPC = npc;
    }

    public void Deconstruct(out NPC npc, out IEaterofWorldsSegment segmentBehavior)
    {
        npc = NPC;
        segmentBehavior = SegmentBehavior;
    }
}
