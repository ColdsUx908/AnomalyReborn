namespace Anomalies.Bosses.EmpressofLight;

public sealed partial class EmpressofLight_Night : AnomalyNPCBehavior<EmpressofLight_Night>
{
    public override bool ShouldProcess => /* base.ShouldProcess && !Aroma && !Main.dayTime;*/ false; //暂不启用

    public override int ApplyingType => NPCID.HallowBoss;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        _ => true,
    };
}

