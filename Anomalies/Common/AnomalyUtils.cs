// Developed by ColdsUx

using System.Diagnostics.CodeAnalysis;

namespace Anomalies.Common;

public static class AnomalyUtils
{
    public static void ILFailure(string name, string reason, [DoesNotReturnIf(true)] bool exception = false)
    {
        string message = $"""[CA IL Editing] IL edit "{name}" failed! {reason}""";
        AnomalyMain.Instance.Logger.Warn(message);
        if (exception)
            throw new InvalidOperationException(message);
    }

    public static TooltipLine CreateNewTooltipLine(int num, string text) => new(AnomalyMain.Instance, $"Tooltip{num}", text);

    public static TooltipLine CreateNewTooltipLine(int num, string text, Color color) => new(AnomalyMain.Instance, $"Tooltip{num}", text) { OverrideColor = color };

    public static TooltipLine CreateNewTooltipLine(int num, Action<TooltipLine> action)
    {
        TooltipLine newLine = new(AnomalyMain.Instance, $"Tooltip{num}", "");
        action?.Invoke(newLine);
        return newLine;
    }
}
