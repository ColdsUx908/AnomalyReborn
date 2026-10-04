using Anomalies.GameContents.Base;

namespace Anomalies.SpecialContents.Contributor.VacuousBlack;

public sealed class VacuousBlackBuff : BaseSummonBuff<VacuousBlackMinion>
{
    public override ref bool MinionBool => ref AnomalyPlayer.Minion_VacuousBlack;

    public override string LocalizationCategory => "SpecialContents.Contributor";

    public override LocalizedText DisplayName => ModContent.GetModItem<VacuousBlack>()?.DisplayName;
}

