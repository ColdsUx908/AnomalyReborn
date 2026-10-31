namespace Transoceanic.Behaviors.Players.BuiltIn;

/// <summary>
/// 维护玩家当帧受伤标记与连续未受伤帧数。
/// </summary>
internal sealed class PlayerHurtTimeUpdate : PlayerBehavior
{
    /// <inheritdoc/>
    /// <remarks>
    /// 标记玩家在本帧受到伤害。
    /// </remarks>
    public override void OnHurt(Player.HurtInfo info) => OceanPlayer.IsHurt = true;

    /// <inheritdoc/>
    /// <remarks>
    /// 本帧受伤时重置未受伤计时并清除标记，否则增加一帧计时。
    /// </remarks>
    public override void PostUpdate()
    {
        if (OceanPlayer.IsHurt)
        {
            OceanPlayer.IsHurt = false;
            OceanPlayer.TimeWithoutHurt = 0;
        }
        else
            OceanPlayer.TimeWithoutHurt++;
    }
}
