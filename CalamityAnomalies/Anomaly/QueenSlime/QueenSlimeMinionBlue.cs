// Developed by ColdsUx

namespace CalamityAnomalies.Anomaly.QueenSlime;

public sealed class QueenSlimeMinionBlue : AnomalyNPCBehavior<QueenSlimeMinionBlue>
{
    public override int ApplyingType => NPCID.QueenSlimeMinionBlue;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.GetAlpha => false,
        _ => true,
    };

    public override Color? GetAlpha(Color drawColor) => drawColor;
}

public sealed class QueenSlimeMinionPink_Anomaly : AnomalyNPCBehavior<QueenSlimeMinionPink_Anomaly>
{
    public override int ApplyingType => NPCID.QueenSlimeMinionPink;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.GetAlpha => false,
        _ => true,
    };

    public override Color? GetAlpha(Color drawColor) => drawColor;
}

public sealed class QueenSlimeMinionPurple_Anomaly : AnomalyNPCBehavior<QueenSlimeMinionPurple_Anomaly>
{
    public override int ApplyingType => NPCID.QueenSlimeMinionPurple;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.GetAlpha => false,
        _ => true,
    };

    public override Color? GetAlpha(Color drawColor) => drawColor;
}
