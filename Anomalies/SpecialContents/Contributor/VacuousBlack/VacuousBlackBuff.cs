using Anomalies.BaseContents;

namespace Anomalies.SpecialContents.Contributor.VacuousBlack;

public sealed class VacuousBlackBuff : BaseSummonBuff<VacuousBlackMinion>
{
    public override bool IsLoadingEnabled(Mod mod) => AnomalySpecialContentToggler.VacuousBlack;

    public override ref bool MinionBool => ref AnomalyPlayer.Minion_VacuousBlack;

    public override string LocalizationCategory => "SpecialContents.Contributor";

    public override LocalizedText DisplayName => ModContent.GetModItem<VacuousBlack>()?.DisplayName;
}
