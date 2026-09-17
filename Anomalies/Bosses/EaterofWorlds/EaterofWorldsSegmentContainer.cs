namespace Anomalies.Bosses.EaterofWorlds;

public sealed class EaterofWorldsSegmentContainer
{
    public NPC NPC { get; }
    public IEaterofWorldsSegment SegmentBehavior { get; }

    public EaterofWorldsSegmentContainer(NPC npc)
    {
        NPC = npc;
        SegmentBehavior = EaterofWorldsHandler.TryGetSegment(npc);
    }

    public EaterofWorldsSegmentContainer(NPC npc, IEaterofWorldsSegment segmentBehavior)
    {
        NPC = npc;
        SegmentBehavior = segmentBehavior;
    }

    public void Deconstruct(out NPC npc, out IEaterofWorldsSegment segmentBehavior)
    {
        npc = NPC;
        segmentBehavior = SegmentBehavior;
    }
}
