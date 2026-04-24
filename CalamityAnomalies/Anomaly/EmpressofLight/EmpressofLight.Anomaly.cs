namespace CalamityAnomalies.Anomaly.EmpressofLight;

public sealed partial class EmpressofLight_Anomaly : AnomalyNPCBehavior
{
    public override bool ShouldProcess => Main.zenithWorld; //暂时仅在天顶世界启用

    public override int ApplyingType => NPCID.HallowBoss;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior method) => method switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        _ => true,
    };

    public override bool PreAI()
    {
        if (Main.zenithWorld)
            return LegacyCalamityAI();

        return true;
    }
}
