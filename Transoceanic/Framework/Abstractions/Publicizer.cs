namespace Transoceanic.Framework.Abstractions;

public abstract class InstancedPublicizer(object Source)
{
    public object Source { get; } = Source;
}