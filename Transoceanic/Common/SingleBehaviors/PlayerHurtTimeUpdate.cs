// Developed by ColdsUx

namespace Transoceanic.Common.SingleBehaviors;

public sealed class PlayerHurtTimeUpdate : TOPlayerBehavior
{
    public override void OnHurt(Player.HurtInfo info) => OceanPlayer.IsHurt = true;

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
