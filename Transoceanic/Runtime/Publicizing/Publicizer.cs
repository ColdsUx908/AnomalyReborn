namespace Transoceanic.Runtime.Publicizing;

public abstract class InstancedPublicizer(object Source)
{
    public object Source { get; } = Source;
}
