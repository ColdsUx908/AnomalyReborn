namespace Anomalies.Bosses.Skeletron;

public sealed class SkeletronHand : AnomalyNPCBehavior<SkeletronHand>
{
    public override int ApplyingType => NPCID.SkeletronHand;

    public override bool ShouldProcess => /*base.ShouldProcess && AnomalyNPC.CustomAIType != 1;*/ false;
}
