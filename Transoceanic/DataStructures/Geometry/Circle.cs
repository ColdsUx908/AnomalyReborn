namespace Transoceanic.DataStructures.Geometry;

public struct Circle : IEquatable<Circle>,
    ICollidableWithRectangle,
    ICollidable<Circle, Circle>,
    ICollidable<Circle, FloatRectangle>,
    ICollidable<Circle, RotatedRectangle>
{
    public Vector2 Center;
    public float Radius;

    public Circle(Vector2 center, float radius)
    {
        Center = center;
        Radius = radius;
    }

    public Circle(float x, float y, float radius) : this(new Vector2(x, y), radius) { }

    public readonly bool Equals(Circle other) => Center == other.Center && Radius == other.Radius;
    public override readonly bool Equals(object obj) => obj is Circle other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(Center, Radius);
    public static bool operator ==(Circle left, Circle right) => left.Equals(right);
    public static bool operator !=(Circle left, Circle right) => !(left == right);

    public override readonly string ToString() => $"Circle {{ Center: {Center}, Radius: {Radius} }}";

    public readonly bool Collides(Rectangle other) => Collides((FloatRectangle)other);
    public readonly bool Collides(Circle other) => Vector2.Distance(Center, other.Center) <= Radius + other.Radius;
    public readonly bool Collides(FloatRectangle other) => TOMathUtils.Geometry.FloatRectanglevCircleCollision(other, this);
    public readonly bool Collides(RotatedRectangle other) => TOMathUtils.Geometry.RotatedRectanglevCircleCollision(other, this);
}
