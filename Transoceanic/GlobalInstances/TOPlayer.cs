using Transoceanic.Commands;

namespace Transoceanic.GlobalInstances;

/// <summary>
/// 保存每个玩家的计时、翼时倍率、命令与本地镜头状态。
/// </summary>
public sealed class TOPlayer : ModPlayer
{
    /// <summary>
    /// 当前玩家保存的命令调用信息。
    /// </summary>
    public CommandCallInfo CommandCallInfo { get; internal set; }

    /// <summary>
    /// 玩家进入世界后累计的更新帧数。
    /// </summary>
    public int GameTime { get; internal set; }

    /// <summary>
    /// 本帧受伤标记，由伤害回调设置，玩家更新结束时消费并清除。
    /// </summary>
    public bool IsHurt;

    /// <summary>
    /// 自上次伤害以来的玩家更新帧数，受伤帧重置为零。
    /// </summary>
    public int TimeWithoutHurt;

    /// <summary>
    /// 提升玩家翅膀飞行时间的乘区。
    /// <br/>每个索引独立计算。
    /// </summary>
    public AddableFloat[] WingTimeMaxMultipliers = new AddableFloat[5];

    /// <summary>
    /// 客户端镜头聚焦的世界像素坐标；null 表示没有聚焦目标，不参与同步。
    /// </summary>
    public Vector2? ScreenFocusCenter;

    /// <summary>
    /// 屏幕位置更改的插值参数，限制在区间 [0, 1] 内。
    /// <br/>值越大，表示屏幕的真实中心将越靠近 <see cref="ScreenFocusCenter"/>。
    /// <br/>当 <see cref="ScreenFocusCenter"/> 不为 <see langword="null"/> 时每帧递减 0.1，否则自动设为 0。
    /// </summary>
    public float ScreenFocusInterpolant
    {
        get;
        set => field = Math.Clamp(value, 0f, 1f);
    }

    /// <summary>
    /// 当此值大于0时，<see cref="ScreenFocusInterpolant"/> 将不会更新。
    /// 每帧递减。
    /// </summary>
    public int ScreenFocusHoldInPlaceTime
    {
        get;
        set => field = Math.Max(value, 0);
    }

    /// <summary>
    /// 客户端屏幕震动的像素幅度，赋值限制为非负；由摄像机行为逐帧衰减，不参与同步。
    /// </summary>
    public float CurrentScreenShakePower
    {
        get;
        set => field = Math.Max(value, 0f);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 复制玩家框架数据，独立克隆翼时倍率数组，避免玩家实例共享可变容器。
    /// </remarks>
    public override ModPlayer Clone(Player newEntity)
    {
        TOPlayer clone = (TOPlayer)base.Clone(newEntity);

        clone.CommandCallInfo = CommandCallInfo;
        clone.GameTime = GameTime;
        clone.IsHurt = IsHurt;
        clone.TimeWithoutHurt = TimeWithoutHurt;
        clone.WingTimeMaxMultipliers = (AddableFloat[])WingTimeMaxMultipliers.Clone();
        clone.ScreenFocusCenter = ScreenFocusCenter;
        clone.ScreenFocusInterpolant = ScreenFocusInterpolant;
        clone.ScreenFocusHoldInPlaceTime = ScreenFocusHoldInPlaceTime;
        clone.CurrentScreenShakePower = CurrentScreenShakePower;

        return clone;
    }
}
