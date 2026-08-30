using MonoMod.Cil;

namespace Anomalies.Common.SingleBehaviors;

public sealed class PlayerBaseStatBoosts : AnomalyPlayerBehavior, IContentLoader
{
    public override decimal Priority => -10m; //在最后应用

    public override void UpdateEquips()
    {
        if (AnomalyClientConfig.Instance.FasterTilePlacement)
        {
            Player.tileSpeed += 0.5f;
            Player.wallSpeed += 0.5f;
        }
    }

    public override void PostUpdateMiscEffects()
    {
        if (AnomalyClientConfig.Instance.FasterFall)
        {
            // Allow the player to double their gravity (but NOT max fall speed!) by holding the down button while in midair.
            bool holdingDown = Player.controlDown && !Player.controlJump;
            bool controlsEnabled = Player.ControlsEnabled();
            bool notInLiquid = !Player.wet;
            bool notOnRope = !Player.pulley && Player.ropeCount == 0;
            bool notGrappling = Player.grappling[0] == -1;
            bool airborne = Player.velocity.Y != 0;
            if (holdingDown && Player.ControlsEnabled() && notInLiquid && notOnRope && notGrappling && airborne) //Player cannot further increase their ridiculous gravity during a Gravistar Slam
            {
                Player.velocity.Y += Player.gravity * Player.gravDir; //下落加速度提高100￥
                if (Player.velocity.Y * Player.gravDir > Player.maxFallSpeed)
                    Player.velocity.Y = Player.maxFallSpeed * Player.gravDir;
            }
        }

        if (AnomalyClientConfig.Instance.FasterRopeClimbSpeed)
        {
            if (Player.pulley)
            {
                int xPos = (int)(Player.position.X + (float)(Player.width / 2)) / 16;
                int yPos = (int)(Player.position.Y - 16f) / 16;
                int yPos2 = (int)(Player.position.Y - 8f) / 16;
                bool ropeAbove = true;
                bool onRope = false;
                if (WorldGen.IsRope(xPos, yPos2 - 1) || WorldGen.IsRope(xPos, yPos2 + 1))
                    onRope = true;

                if (!WorldGen.IsRope(xPos, yPos))
                {
                    ropeAbove = false;
                    if (Player.velocity.Y < 0f)
                        Player.velocity.Y = 0f;
                }

                if (onRope)
                {
                    if (Player.controlUp && ropeAbove)
                    {
                        // Base multiplier is 0.7f
                        // Add an additional multiplier of the same value to make it decelerate much faster
                        if (Player.velocity.Y > 0f)
                            Player.velocity.Y *= 0.7f;

                        // Base acceleration values are 0.2f and 0.02f
                        // New acceleration values are 0.2f + 0.2f (0.4f) before hitting -3f velocity and 0.02f + 0.18f (0.2f) after hitting -6f velocity
                        if (Player.velocity.Y > -3f)
                            Player.velocity.Y -= 0.2f;
                        else
                            Player.velocity.Y -= 0.18f;

                        if (Player.velocity.Y < -8f)
                            Player.velocity.Y = -8f;
                    }
                    else if (Player.controlDown)
                    {
                        // Base multiplier is 0.7f
                        // Add an additional multiplier of the same value to make it decelerate much faster
                        if (Player.velocity.Y < 0f)
                            Player.velocity.Y *= 0.7f;

                        // Base acceleration values are 0.2f and 0.1f
                        // New acceleration values are 0.2f + 0.4f (0.6f) before hitting 3f velocity and 0.1f + 0.2f (0.3f) after hitting 3f velocity
                        if (Player.velocity.Y < 3f)
                            Player.velocity.Y += 0.4f;
                        else
                            Player.velocity.Y += 0.2f;

                        if (Player.velocity.Y > Player.maxFallSpeed)
                            Player.velocity.Y = Player.maxFallSpeed;
                    }
                    else if (Math.Abs(Player.velocity.Y) > 0f)
                    {
                        // Base multiplier is 0.7f
                        // Add an additional multiplier of the same value to make it decelerate much faster
                        Player.velocity.Y *= 0.7f;
                        if (Math.Abs(Player.velocity.Y) < 0.1f)
                            Player.velocity.Y = 0f;
                    }
                }
            }
        }

        if (AnomalyClientConfig.Instance.FasterBaseSpeed)
            Player.moveSpeed *= 1.5f;
    }

    void IContentLoader.PostSetupContent()
    {
        IL_Player.Update += IL_Player_Update;
    }

    private void IL_Player_Update(ILContext il)
    {
        // When the relevant config is enabled: Increases the player's base jump speed by 13.97% (allows you to jump 7 blocks)

        const float VanillaBaseJumpSpeed = 5.01f;
        const float ConfigBoostedBaseJumpSpeed = 5.71f;

        // Increase the base jump height of the player to make early game less of a slog.
        ILCursor cursor = new ILCursor(il);

        // The jumpSpeed variable is set to this specific value before anything else occurs.
        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcR4(VanillaBaseJumpSpeed)))
        {
            AnomalyUtils.ILFailure("Base Jump Height Buff", "Could not locate the jump height variable.");
            return;
        }
        cursor.Remove();

        // Increase by 10% if the higher jump speed is enabled.
        cursor.EmitDelegate(() => AnomalyClientConfig.Instance.FasterJumpSpeed ? ConfigBoostedBaseJumpSpeed : VanillaBaseJumpSpeed);
    }
}

