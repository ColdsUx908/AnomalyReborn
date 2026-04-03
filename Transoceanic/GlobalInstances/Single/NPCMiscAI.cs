namespace Transoceanic.GlobalInstances.Single;

public sealed class NPCMiscAI : TOGlobalNPCBehavior
{
    public override bool PreAI(NPC npc)
    {
        return true;
    }

    public override void PostAI(NPC npc)
    {
        if (npc.AlwaysRotating)
            npc.VelocityToRotation(npc.RotationOffset);
    }
}
