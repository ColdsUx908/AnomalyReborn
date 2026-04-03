namespace Transoceanic.DataStructures.Geometry;

public struct Ring : IEquatable<Ring>,
    ICollidableWithRectangle,
    ICollidable<Ring, FloatRectangle>
{
    public Vector2 Center;
    public float InnerRadius;
    public float OuterRadius;

    public Ring(Vector2 center, float innerRadius, float outerRadius)
    {
        Center = center;

        if (innerRadius < 0f || innerRadius > outerRadius)
            throw new ArgumentException("Inner radius must be non-negative and less than outer radius.", $"{nameof(innerRadius)}, {nameof(outerRadius)}");

        InnerRadius = innerRadius;
        OuterRadius = outerRadius;
    }

    public Ring(float x, float y, float innerRadius, float outerRadius) : this(new Vector2(x, y), innerRadius, outerRadius) { }

    public readonly bool Equals(Ring other) => Center == other.Center && InnerRadius == other.InnerRadius && OuterRadius == other.OuterRadius;
    public override readonly bool Equals(object obj) => obj is Ring other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(Center, InnerRadius, OuterRadius);
    public static bool operator ==(Ring left, Ring right) => left.Equals(right);
    public static bool operator !=(Ring left, Ring right) => !(left == right);

    public override readonly string ToString() => $"Ring {{ Center: {Center}, InnerRadius: {InnerRadius}, OuterRadius: {OuterRadius} }}";

    public readonly bool CircleContains(FloatRectangle rectangle, bool intersect, bool useOuterRadius) => (intersect ? TOMathUtils.Geometry.MinDistanceFromTo(rectangle, Center) : TOMathUtils.Geometry.MaxDistanceFromTo(rectangle, Center)) < (useOuterRadius ? OuterRadius : InnerRadius);

    public readonly bool Collides(Rectangle other) => TOMathUtils.Geometry.FloatRectanglevRingCollision((FloatRectangle)other, this);
    public readonly bool Collides(FloatRectangle other) => TOMathUtils.Geometry.FloatRectanglevRingCollision(other, this);
}