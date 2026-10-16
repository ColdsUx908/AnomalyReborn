namespace Anomalies.Bosses.EaterofWorlds;

public sealed class EaterofWorldsSegmentContainer
{
    public NPC NPC { get; }
    // Behavior is a singleton: reacquire it to bind the correct NPC on every access.
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
