// Developed by ColdsUx

namespace CalamityAnomalies.Anomaly.Bosses.DukeFishron;

public sealed class DetonatingBubble : AnomalyNPCBehavior<DetonatingBubble>
{
    public override int ApplyingType => NPCID.DetonatingBubble;

    public override bool PreAI()
    {
        if (Ultra)
            NPC.dontTakeDamage = true;

        return true;
    }
}
