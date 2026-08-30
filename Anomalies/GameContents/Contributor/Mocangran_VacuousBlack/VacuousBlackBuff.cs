using Anomalies.GameContents.Base;

namespace Anomalies.GameContents.Contributor.Mocangran_VacuousBlack;

public sealed class VacuousBlackBuff : BaseSummonBuff<VacuousBlackMinion>
{
    public override ref bool MinionBool => ref AnomalyPlayer.Minion_VacuousBlack;

    public override string LocalizationCategory => "GameContents.Contributor";

    public override LocalizedText DisplayName => ModContent.GetModItem<VacuousBlack>()?.DisplayName;
}

