using Transoceanic.GlobalInstances;
namespace Transoceanic.Behaviors.Players.BuiltIn;

/// <summary>
/// 按玩家镜头状态应用本地聚焦与震屏。
/// </summary>
internal class PlayerCameraEffects : PlayerBehavior
{
    /// <inheritdoc/>
    /// <remarks>
    /// 递减镜头保持时间；保持结束后衰减聚焦插值和震屏强度。
    /// </remarks>
    public override void ResetEffects()
    {
        TOPlayer oceanPlayer = OceanPlayer;

        if (oceanPlayer.ScreenFocusHoldInPlaceTime > 0)
            oceanPlayer.ScreenFocusHoldInPlaceTime--;
        else
        {
            if (oceanPlayer.ScreenFocusCenter is null)
                oceanPlayer.ScreenFocusInterpolant = 0f;
            else
            {
                oceanPlayer.ScreenFocusInterpolant -= 0.1f;
                if (oceanPlayer.ScreenFocusInterpolant == 0f)
                    oceanPlayer.ScreenFocusCenter = null;
            }
            oceanPlayer.CurrentScreenShakePower -= 0.2f;
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 玩家存活时按聚焦插值调整屏幕中心，并叠加当前强度的随机圆周震屏偏移。
    /// </remarks>
    public override void ModifyScreenPosition()
    {
        if (Player.dead)
            return;

        TOPlayer oceanPlayer = OceanPlayer;

        if (oceanPlayer.ScreenFocusCenter.HasValue && oceanPlayer.ScreenFocusInterpolant > 0f)
            Main.ScreenCenter = Vector2.Lerp(Main.ScreenCenter, oceanPlayer.ScreenFocusCenter.Value, oceanPlayer.ScreenFocusInterpolant);

        if (oceanPlayer.CurrentScreenShakePower > 0f)
            Main.screenPosition += Main.rand.NextVector2CircularEdge(oceanPlayer.CurrentScreenShakePower, oceanPlayer.CurrentScreenShakePower);
    }
}
