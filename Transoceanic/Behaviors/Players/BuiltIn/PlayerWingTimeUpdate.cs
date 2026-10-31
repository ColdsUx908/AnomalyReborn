namespace Transoceanic.Behaviors.Players.BuiltIn;

/// <summary>
/// 每帧重置并应用玩家最大翼时的分区倍率。
/// </summary>
internal sealed class PlayerWingTimeUpdate : PlayerBehavior
{
    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>-1000m</c>。</remarks>
    public override decimal Priority => -1000m;

    /// <inheritdoc/>
    /// <remarks>
    /// 将所有最大翼时附加倍率槽重置为零，供本帧装备和效果重新累加。
    /// </remarks>
    public override void ResetEffects()
    {
        for (int i = 0; i < OceanPlayer.WingTimeMaxMultipliers.Length; i++)
            OceanPlayer.WingTimeMaxMultipliers[i] = AddableFloat.Zero;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 最大翼时为正时连乘各槽的 1 + 附加倍率，应用到最大翼时并转换为整数。
    /// </remarks>
    public override void PostUpdateMiscEffects()
    {
        if (Player.wingTimeMax > 0)
        {
            float multiplier = 1f;
            foreach (AddableFloat wingTimeMaxMultiplier in OceanPlayer.WingTimeMaxMultipliers)
                multiplier *= 1f + wingTimeMaxMultiplier.Value;
            Player.wingTimeMax = (int)(Player.wingTimeMax * multiplier);
        }
    }
}
