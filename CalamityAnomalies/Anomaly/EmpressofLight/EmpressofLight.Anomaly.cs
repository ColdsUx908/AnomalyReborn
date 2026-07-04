// Developed by ColdsUx

namespace CalamityAnomalies.Anomaly.EmpressofLight;

public sealed partial class EmpressofLight_Anomaly : AnomalyNPCBehavior<EmpressofLight_Anomaly>
{
    public override bool ShouldProcess => base.ShouldProcess && Aroma; //暂时仅在天顶世界启用

    public override int ApplyingType => NPCID.HallowBoss;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        _ => true,
    };

    public override bool PreAI()
    {
        if (Aroma)
            return EmpressOfLightResurrection.AI(NPC, CalamityNPC);

        return true;
    }
}
