namespace Transoceanic.DataStructures.Geometry;

public struct Ellipse : IEquatable<Ellipse>
{
    public Vector2 Center;
    public float A;
    public float B;
    public float Rotation;

    public Ellipse(Vector2 center, float a, float b, float rotation)
    {
        Center = center;
        A = a;
        B = b;
        Rotation = TOMathUtils.NormalizeWithPeriod(rotation);
    }

    public readonly bool Equals(Ellipse other) => Center == other.Center && A == other.A && B == other.B && Rotation == other.Rotation;
    public override readonly bool Equals(object obj) => obj is Ellipse other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(Center, A, B, Rotation);
    public static bool operator ==(Ellipse left, Ellipse right) => left.Equals(right);
    public static bool operator !=(Ellipse left, Ellipse right) => !left.Equals(right);
}