namespace Transoceanic.GlobalInstances;

public sealed class TOPlayer : ModPlayer, IContentLoader
{
    public CommandCallInfo CommandCallInfo { get; internal set; }

    public int GameTime { get; internal set; }

    public bool IsHurt;

    public int TimeWithoutHurt;

    /// <summary>
    /// 提升玩家翅膀飞行时间的乘区。
    /// <br/>每个索引独立计算。
    /// </summary>
    public AddableFloat[] WingTimeMaxMultipliers = new AddableFloat[5];

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

    public float CurrentScreenShakePower
    {
        get;
        set => field = Math.Max(value, 0f);
    }

    public override ModPlayer Clone(Player newEntity)
    {
        TOPlayer clone = (TOPlayer)base.Clone(newEntity);

        clone.CommandCallInfo = CommandCallInfo;
        clone.GameTime = GameTime;
        clone.IsHurt = IsHurt;
        clone.TimeWithoutHurt = TimeWithoutHurt;
        Array.Copy(WingTimeMaxMultipliers, clone.WingTimeMaxMultipliers, WingTimeMaxMultipliers.Length);
        clone.ScreenFocusCenter = ScreenFocusCenter;
        clone.ScreenFocusInterpolant = ScreenFocusInterpolant;
        clone.ScreenFocusHoldInPlaceTime = ScreenFocusHoldInPlaceTime;
        clone.CurrentScreenShakePower = CurrentScreenShakePower;

        return clone;
    }
}