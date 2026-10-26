namespace Transoceanic.Behaviors;

public abstract class TOGlobalItemBehavior : GlobalItemBehavior
{
    public sealed override TOMain Mod => TOMain.Instance;
}
