// Developed by ColdsUx

namespace Transoceanic.Common.SingleBehaviors;

public sealed class PlayerGameTimeUpdate : TOPlayerBehavior
{
    public override void PreUpdate() => OceanPlayer.GameTime++;

    public override void OnEnterWorld() => OceanPlayer.GameTime = 0;
}
