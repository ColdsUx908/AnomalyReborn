namespace CalamityAnomalies.Anomaly.DukeFishron;

public sealed class DetonatingBubble_Anomaly : AnomalyNPCBehavior<DetonatingBubble_Anomaly>
{
    public override int ApplyingType => NPCID.DetonatingBubble;

    public override bool PreAI()
    {
        if (Ultra)
            NPC.dontTakeDamage = true;

        return true;
    }
}
