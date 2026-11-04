namespace Anomalies.Bosses.DukeFishron;

public sealed class DetonatingBubble : AnomalyNPCBehavior<DetonatingBubble>
{
    public override bool ShouldProcess => base.ShouldProcess && AnomalyBossChangeToggler.DukeFishron;

    public override int ApplyingType => NPCID.DetonatingBubble;

    public override bool PreAI()
    {
        if (Ultra)
            NPC.dontTakeDamage = true;

        return true;
    }
}

