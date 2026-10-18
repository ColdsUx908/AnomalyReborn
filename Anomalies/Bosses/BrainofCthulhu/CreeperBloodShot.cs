namespace Anomalies.Bosses.BrainofCthulhu;

public sealed class CreeperBloodShot : BrainBloodShot
{
    public override bool IsNautilusShot => false;
    public override LocalizedText DisplayName => ModContent.GetInstance<BrainBloodShot>().DisplayName;
}
