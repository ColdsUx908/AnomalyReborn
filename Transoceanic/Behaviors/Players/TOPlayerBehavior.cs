namespace Transoceanic.Behaviors;

public abstract class TOPlayerBehavior : PlayerBehavior
{
    public sealed override TOMain Mod => TOMain.Instance;
}
