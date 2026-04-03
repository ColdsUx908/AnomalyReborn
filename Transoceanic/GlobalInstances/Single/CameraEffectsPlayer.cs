namespace Transoceanic.GlobalInstances;

public class CameraEffectsPlayer : TOPlayerBehavior
{
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