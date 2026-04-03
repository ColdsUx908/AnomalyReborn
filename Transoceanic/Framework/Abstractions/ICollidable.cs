namespace Transoceanic.Framework.Abstractions;

public interface ICollidableWithRectangle
{
    public abstract bool Collides(Rectangle other);
}

public interface ICollidable<TSelf, TOther>
    where TSelf : ICollidable<TSelf, TOther>
    where TOther : ICollidable<TOther, TSelf>
{
    public abstract bool Collides(TOther other);
}
