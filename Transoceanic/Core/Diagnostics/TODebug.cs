using Transoceanic.Commands;

namespace Transoceanic.Core.Diagnostics;

/// <summary>
/// 提供框架调试开关、日志和调试输出工具。
/// </summary>
public static class TODebug
{
    /// <summary>
    /// 指示当前是否处于调试（DEBUG）模式。
    /// <br/>在 Debug 编译配置下默认为 <see langword="true"/>，否则默认为 <see langword="false"/>。
    /// <br/>可通过 <see cref="DebugModeCommand"/> 在游戏内动态调整。
    /// </summary>
    public static bool DEBUG { get; internal set; } =
#if DEBUG
    true;
#else
    false;
#endif

    /// <summary>
    /// 调试模式下的预设玩家名称，用于身份检查。
    /// </summary>
    private const string DEBUGPlayerName = "~ColdsUx";

    /// <summary>
    /// 判断给定的玩家是否为调试玩家。
    /// </summary>
    /// <param name="player">要检查的玩家实例。</param>
    /// <returns>当 <see cref="DEBUG"/> 为 <see langword="true"/> 且玩家名称等于 <see cref="DEBUGPlayerName"/> 时返回 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public static bool IsDEBUGPlayer(Player player) => DEBUG && player?.name == DEBUGPlayerName;

    /// <summary>
    /// 调试警告信息的颜色（橙色）。
    /// </summary>
    public static readonly Color TODebugWarnColor = Color.Orange;

    /// <summary>
    /// 调试错误信息的颜色（红色）。
    /// </summary>
    public static readonly Color TODebugErrorColor = new(0xFF, 0x00, 0x00);
}
