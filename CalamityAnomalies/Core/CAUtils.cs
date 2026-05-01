// Developed by ColdsUx

using System.Diagnostics.CodeAnalysis;

namespace CalamityAnomalies.Core;

public static class CAUtils
{
    public static bool IsDefeatingLeviathan(NPC npc) => npc.LeviathanBoss && !TOIteratorFactory.NewActiveNPCIterator(n => n.LeviathanBoss, npc).Any();

    public static bool IsDefeatingProfanedGuardians(NPC npc) => npc.ProfanedGuardianBoss && !TOIteratorFactory.NewActiveNPCIterator(n => n.ProfanedGuardianBoss, npc).Any();

    public static bool IsDefeatingExoMechs(NPC npc) =>
        npc.Ares && !NPC.ActiveNPCs.Any(n => !n.ExoTwins && !n.Thanatos)
        || npc.ExoTwins && !NPC.ActiveNPCs.Any(n => !n.Ares && !n.Thanatos)
        || npc.ThanatosHead && !NPC.ActiveNPCs.Any(n => !n.ExoTwins && !n.Ares);

    public static void ILFailure(string name, string reason, [DoesNotReturnIf(true)] bool exception = false)
    {
        string message = $"""[CA IL Editing] IL edit "{name}" failed! {reason}""";
        CAMain.Instance.Logger.Warn(message);
        if (exception)
            throw new InvalidOperationException(message);
    }

    public static TooltipLine CreateNewTooltipLine(int num, string text) => new(CAMain.Instance, $"Tooltip{num}", text);

    public static TooltipLine CreateNewTooltipLine(int num, string text, Color color) => new(CAMain.Instance, $"Tooltip{num}", text) { OverrideColor = color };

    public static TooltipLine CreateNewTooltipLine(int num, Action<TooltipLine> action)
    {
        TooltipLine newLine = new(CAMain.Instance, $"Tooltip{num}", "");
        action?.Invoke(newLine);
        return newLine;
    }
}
