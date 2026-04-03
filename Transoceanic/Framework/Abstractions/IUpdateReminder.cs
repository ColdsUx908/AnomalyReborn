namespace Transoceanic.Framework.Abstractions;

internal interface IUpdateReminder
{
    internal abstract Action RegisterUpdateReminder();
}

public interface IExternalUpdateReminder
{
    public abstract Action RegisterUpdateReminder();
}
