using CalamityAnomalies.GameContents.Base;

namespace CalamityAnomalies.GameContents.Contributor.Mocangran_VacuousBlack;

public sealed class VacuousBlackBuff : CABaseSummonBuff<VacuousBlackMinion>
{
    protected override ref bool MinionBool => ref AnomalyPlayer.Minion_VacuousBlack;

    public override string LocalizationCategory => "GameContents.Contributor";

    public override LocalizedText DisplayName => ModContent.GetModItem<VacuousBlack>()?.DisplayName;
}
