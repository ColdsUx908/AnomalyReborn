namespace Transoceanic.DataStructures.Geometry;

public struct RotatedRectangle : IEquatable<RotatedRectangle>,
    ICollidableWithRectangle,
    ICollidable<RotatedRectangle, RotatedRectangle>,
    ICollidable<RotatedRectangle, FloatRectangle>,
    ICollidable<RotatedRectangle, Circle>
{
    public FloatRectangle Source;
    public float Rotation;

    public RotatedRectangle(FloatRectangle source, float rotation)
    {
        Source = source;
        Rotation = TOMathUtils.NormalizeWithPeriod(rotation);
    }

    public RotatedRectangle(Vector2 center, float width, float height, float rotation) : this(Rectangle.FromCenter(center, width, height), rotation) { }

    public readonly Vector2 Center => Source.Center;

    public readonly Vector2 TopLeft => Source.TopLeft.RotatedBy(Rotation, Center);
    public readonly Vector2 TopRight => Source.TopRight.RotatedBy(Rotation, Center);
    public readonly Vector2 BottomLeft => Source.BottomLeft.RotatedBy(Rotation, Center);
    public readonly Vector2 BottomRight => Source.BottomRight.RotatedBy(Rotation, Center);

    public readonly LineSegment TopSide
    {
        get
        {
            Vector2 center = Center;
            (float sin, float cos) = MathF.SinCos(Rotation);
            Vector2 widthRotated = new Vector2(cos, sin) * (Source.Width / 2f);
            Vector2 heightRotated = new Vector2(-sin, cos) * (Source.Height / 2f);
            return new(center - widthRotated - heightRotated, center + widthRotated - heightRotated);
        }
    }

    public readonly LineSegment BottomSide
    {
        get
        {
            Vector2 center = Center;
            (float sin, float cos) = MathF.SinCos(Rotation);
            Vector2 widthRotated = new Vector2(cos, sin) * (Source.Width / 2f);
            Vector2 heightRotated = new Vector2(-sin, cos) * (Source.Height / 2f);
            return new(center - widthRotated + heightRotated, center + widthRotated + heightRotated);
        }
    }

    public readonly LineSegment LeftSide
    {
        get
        {
            Vector2 center = Center;
            (float sin, float cos) = MathF.SinCos(Rotation);
            Vector2 widthRotated = new Vector2(cos, sin) * (Source.Width / 2f);
            Vector2 heightRotated = new Vector2(-sin, cos) * (Source.Height / 2f);
            return new(center - widthRotated - heightRotated, center - widthRotated + heightRotated);
        }
    }

    public readonly LineSegment RightSide
    {
        get
        {
            Vector2 center = Center;
            (float sin, float cos) = MathF.SinCos(Rotation);
            Vector2 widthRotated = new Vector2(cos, sin) * (Source.Width / 2f);
            Vector2 heightRotated = new Vector2(-sin, cos) * (Source.Height / 2f);
            return new(center + widthRotated - heightRotated, center + widthRotated + heightRotated);
        }
    }

    public readonly (Vector2 TopLeft, Vector2 TopRight, Vector2 BottomLeft, Vector2 BottomRight) Vertices
    {
        get
        {
            Vector2 center = Center;
            (float sin, float cos) = MathF.SinCos(Rotation);
            Vector2 widthRotated = new Vector2(cos, sin) * (Source.Width / 2f);
            Vector2 heightRotated = new Vector2(-sin, cos) * (Source.Height / 2f);
            return (center - widthRotated - heightRotated, center + widthRotated - heightRotated, center - widthRotated + heightRotated, center + widthRotated + heightRotated);
        }
    }

    public readonly (LineSegment Top, LineSegment Bottom, LineSegment Left, LineSegment Right) Sides
    {
        get
        {
            (Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight) = Vertices;
            return (new LineSegment(topLeft, topRight), new LineSegment(bottomLeft, bottomRight), new LineSegment(topLeft, bottomLeft), new LineSegment(topRight, bottomRight));
        }
    }

    public readonly bool Contains(Vector2 point) => Source.Contains(Center + (point - Center).RotatedBy(-Rotation));

    public readonly bool Equals(RotatedRectangle other) => Source == other.Source && Rotation == other.Rotation;
    public override readonly bool Equals(object obj) => obj is RotatedRectangle other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(Source, Rotation);
    public static bool operator ==(RotatedRectangle left, RotatedRectangle right) => left.Equals(right);
    public static bool operator !=(RotatedRectangle left, RotatedRectangle right) => !(left == right);

    public override readonly string ToString() => $"RotatedRectangle {{ Source: {Source}, Rotation: {Rotation} }}";

    public static implicit operator RotatedRectangle(FloatRectangle rect) => new(rect, 0f);

    public readonly bool Collides(RotatedRectangle other)
    {
        //SAT检测
        (Vector2 aPoint1, Vector2 aPoint2, Vector2 aPoint3, Vector2 aPoint4) = Vertices;
        (Vector2 bPoint1, Vector2 bPoint2, Vector2 bPoint3, Vector2 bPoint4) = other.Vertices;
        ReadOnlySpan<Vector2> aPoints = [aPoint1, aPoint2, aPoint3, aPoint4];
        ReadOnlySpan<Vector2> bPoints = [bPoint1, bPoint2, bPoint3, bPoint4];

        (float sinA, float cosA) = MathF.SinCos(Rotation);
        if (!TOMathUtils.Geometry.OverlapOnAxis(new Vector2(cosA, sinA), aPoints, bPoints))
            return false;
        if (!TOMathUtils.Geometry.OverlapOnAxis(new Vector2(-sinA, cosA), aPoints, bPoints))
            return false;

        (float sinB, float cosB) = MathF.SinCos(other.Rotation);
        if (!TOMathUtils.Geometry.OverlapOnAxis(new Vector2(cosB, sinB), aPoints, bPoints))
            return false;
        if (!TOMathUtils.Geometry.OverlapOnAxis(new Vector2(-sinB, cosB), aPoints, bPoints))
            return false;

        return true;
    }

    public readonly bool Collides(Rectangle other) => Collides((FloatRectangle)other);
    public readonly bool Collides(FloatRectangle other) => TOMathUtils.Geometry.RotatedRectanglevFloatRectangleCollision(this, other);
    public readonly bool Collides(Circle other) => TOMathUtils.Geometry.RotatedRectanglevCircleCollision(this, other);
}
