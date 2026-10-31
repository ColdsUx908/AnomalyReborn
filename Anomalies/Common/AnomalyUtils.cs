using System.Diagnostics.CodeAnalysis;

namespace Anomalies.Common;

public static partial class AnomalyUtils
{
    /// <summary>通过已进入 Mod.Load 的异象主实例记录 IL 修改失败。</summary>
    /// <param name="name">IL 修改名称。</param>
    /// <param name="reason">失败原因。</param>
    /// <param name="exception">是否记录错误并抛出异常。</param>
    public static void ILFailure(string name, string reason, [DoesNotReturnIf(true)] bool exception = false) =>
        ILFailure(AnomalyMain.Instance, name, reason, exception);

    /// <summary>通过显式所属模组记录 IL 修改失败，适用于早于 Mod.Load 的内容加载阶段。</summary>
    /// <param name="mod">由 tML 提供的所属模组。</param>
    /// <param name="name">IL 修改名称。</param>
    /// <param name="reason">失败原因。</param>
    /// <param name="exception">是否记录错误并抛出异常；false 时仅警告。</param>
    public static void ILFailure(Mod mod, string name, string reason, [DoesNotReturnIf(true)] bool exception = false)
    {
        ArgumentNullException.ThrowIfNull(mod);
        string message = $"""[CA IL Editing] IL edit "{name}" failed! {reason}""";
        if (exception)
        {
            mod.Logger.Error(message);
            throw new InvalidOperationException(message);
        }
        else
            mod.Logger.Warn(message);
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
