namespace Transoceanic.Behaviors.Players.BuiltIn;

/// <summary>
/// 累计玩家进入当前世界后的更新帧数。
/// </summary>
internal sealed class PlayerGameTimeUpdate : PlayerBehavior
{
    /// <inheritdoc/>
    /// <remarks>
    /// 将当前玩家的游戏计时增加一帧。
    /// </remarks>
    public override void PreUpdate() => OceanPlayer.GameTime++;

    /// <inheritdoc/>
    /// <remarks>
    /// 进入世界时将玩家游戏计时归零。
    /// </remarks>
    public override void OnEnterWorld() => OceanPlayer.GameTime = 0;
}
