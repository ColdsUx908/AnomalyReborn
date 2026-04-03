namespace Transoceanic.DataStructures.Geometry;

public struct FloatRectangle : IEquatable<FloatRectangle>,
    ICollidableWithRectangle,
    ICollidable<FloatRectangle, FloatRectangle>,
    ICollidable<FloatRectangle, Circle>,
    ICollidable<FloatRectangle, RotatedRectangle>,
    ICollidable<FloatRectangle, Ring>
{
    public Vector2 Position;
    public float Width;
    public float Height;

    public readonly float Left => Position.X;
    public readonly float Right => Position.X + Width;
    public readonly float Top => Position.Y;
    public readonly float Bottom => Position.Y + Height;

    public readonly Vector2 Center => new(Position.X + Width / 2, Position.Y + Height / 2);

    public readonly Vector2 TopLeft => Position;
    public readonly Vector2 TopRight => new(Position.X + Width, Position.Y);
    public readonly Vector2 BottomLeft => new(Position.X, Position.Y + Height);
    public readonly Vector2 BottomRight => new(Position.X + Width, Position.Y + Height);

    public FloatRectangle(Vector2 position, float width, float height)
    {
        Position = position;
        Width = width;
        Height = height;
    }

    public FloatRectangle(float x, float y, float width, float height) : this(new Vector2(x, y), width, height) { }

    public FloatRectangle(Vector2 position, Vector2 size) : this(position, size.X, size.Y) { }

    public static FloatRectangle FromCenter(Vector2 center, float width, float height) => new(new Vector2(center.X - width / 2, center.Y - height / 2), width, height);
    public static FloatRectangle FromInnerPoint(Vector2 point, float left, float right, float top, float bottom) => new(new Vector2(point.X - left, point.Y - top), left + right, top + bottom);

    public readonly bool Equals(FloatRectangle other) => Position == other.Position && Width == other.Width && Height == other.Height;
    public override readonly bool Equals(object obj) => obj is FloatRectangle other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(Position, Width, Height);
    public static bool operator ==(FloatRectangle left, FloatRectangle right) => left.Equals(right);
    public static bool operator !=(FloatRectangle left, FloatRectangle right) => !(left == right);

    public override readonly string ToString() => $"FloatRectangle {{ Position: {Position}, Width: {Width}, Height: {Height} }}";

    public static implicit operator FloatRectangle(Rectangle rect) => new(rect.X, rect.Y, rect.Width, rect.Height);

    public readonly bool Contains(Vector2 point) => point.X >= Left && point.X <= Right && point.Y >= Top && point.Y <= Bottom;

    public readonly bool Collides(Rectangle other) => Collides((FloatRectangle)other);
    public readonly bool Collides(FloatRectangle other) => Left < other.Right && Right > other.Left && Top < other.Bottom && Bottom > other.Top;
    public readonly bool Collides(Circle other) => TOMathUtils.Geometry.FloatRectanglevCircleCollision(this, other);
    public readonly bool Collides(RotatedRectangle other) => TOMathUtils.Geometry.RotatedRectanglevFloatRectangleCollision(other, this);
    public readonly bool Collides(Ring other) => TOMathUtils.Geometry.FloatRectanglevRingCollision(this, other);
}
