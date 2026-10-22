using Transoceanic.Framework.Abstractions;

namespace Transoceanic.DataStructures.Geometry;

/// <summary>表示包含两条径向边和圆弧边界的二维扇形。</summary>
public struct Sector : IEquatable<Sector>, ICollidableWithRectangle
{
    /// <summary>扇形的圆心。</summary>
    public Vector2 Center;
    /// <summary>扇形的半径。</summary>
    public float Radius;
    /// <summary>中轴相对 X 正方向的旋转角，单位为弧度。</summary>
    public float Rotation;
    /// <summary>张角的一半，取值范围为 [0, π]，单位为弧度。</summary>
    public float HalfAngle;

    /// <summary>使用圆心、半径、中轴旋转角和半张角构造扇形。</summary>
    public Sector(Vector2 center, float radius, float rotation, float halfAngle)
    {
        if (!float.IsFinite(radius) || radius < 0f)
            throw new ArgumentOutOfRangeException(nameof(radius));
        if (!float.IsFinite(rotation))
            throw new ArgumentOutOfRangeException(nameof(rotation));
        if (!float.IsFinite(halfAngle) || halfAngle < 0f || halfAngle > MathF.PI)
            throw new ArgumentOutOfRangeException(nameof(halfAngle));
        Center = center;
        Radius = radius;
        Rotation = rotation;
        HalfAngle = halfAngle;
    }

    /// <summary>使用圆心坐标分量、半径、中轴旋转角和半张角构造扇形。</summary>
    public Sector(float x, float y, float radius, float rotation, float halfAngle)
        : this(new Vector2(x, y), radius, rotation, halfAngle) { }

    /// <summary>扇形中轴的单位方向。</summary>
    public readonly Vector2 Direction => new(MathF.Cos(Rotation), MathF.Sin(Rotation));

    public readonly bool Equals(Sector other) => Center == other.Center && Radius == other.Radius && Rotation == other.Rotation && HalfAngle == other.HalfAngle;
    public override readonly bool Equals(object obj) => obj is Sector other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(Center, Radius, Rotation, HalfAngle);
    public static bool operator ==(Sector left, Sector right) => left.Equals(right);
    public static bool operator !=(Sector left, Sector right) => !(left == right);
    public override readonly string ToString() => $"Sector {{ Center: {Center}, Radius: {Radius}, Rotation: {Rotation}, HalfAngle: {HalfAngle} }}";

    /// <summary>判断点是否位于扇形内，包含边界。</summary>
    public readonly bool Contains(Vector2 point) => ContainsOffset(point - Center, Radius);

    private readonly bool ContainsOffset(Vector2 offset, float radius)
    {
        if (offset.LengthSquared() > radius * radius)
            return false;
        if (offset.LengthSquared() == 0f)
            return true;
        return Vector2.Dot(Vector2.Normalize(offset), Direction) >= MathF.Cos(HalfAngle);
    }

    public readonly bool Collides(Rectangle hitbox)
    {
        Vector2 center = Center;
        float radius = Radius;
        // 两条径向边和圆弧共同组成扇形。圆弧与 AABB 四边的交点也检查，
        // 避免只测中心或角点时漏掉穿过扇形的细长目标。
        Vector2 min = new(hitbox.Left, hitbox.Top);
        Vector2 max = new(hitbox.Right, hitbox.Bottom);
        if (center.X >= min.X && center.X <= max.X && center.Y >= min.Y && center.Y <= max.Y
            || ContainsOffset(Vector2.Clamp(center, min, max) - center, radius))
            return true;
        Span<Vector2> corners = stackalloc Vector2[4]
        {
            min, new Vector2(max.X, min.Y), max, new Vector2(min.X, max.Y)
        };
        for (int i = 0; i < corners.Length; i++)
        {
            if (ContainsOffset(corners[i] - center, radius))
                return true;
        }
        for (int side = -1; side <= 1; side += 2)
        {
            Vector2 end = center + Vector2.Transform(Direction, Matrix.CreateRotationZ(HalfAngle * side)) * radius;
            if (SegmentIntersects(center, end, min, max))
                return true;
        }
        for (int i = 0; i < corners.Length; i++)
        {
            Vector2 start = corners[i] - center;
            Vector2 edge = corners[(i + 1) % corners.Length] - corners[i];
            float a = edge.LengthSquared();
            if (a <= 0f)
                continue;
            float b = 2f * Vector2.Dot(start, edge);
            float c = start.LengthSquared() - radius * radius;
            float discriminant = b * b - 4f * a * c;
            if (discriminant < 0f)
                continue;
            float root = MathF.Sqrt(discriminant);
            for (int sign = -1; sign <= 1; sign += 2)
            {
                float t = (-b + sign * root) / (2f * a);
                if (t >= 0f && t <= 1f && ContainsOffset(start + edge * t, radius + 0.01f))
                    return true;
            }
        }
        return false;
    }

    private static bool SegmentIntersects(Vector2 start, Vector2 end, Vector2 min, Vector2 max)
    {
        Vector2 delta = end - start;
        float enter = 0f;
        float exit = 1f;
        return Clip(-delta.X, start.X - min.X) && Clip(delta.X, max.X - start.X)
            && Clip(-delta.Y, start.Y - min.Y) && Clip(delta.Y, max.Y - start.Y);

        bool Clip(float p, float q)
        {
            if (Math.Abs(p) < 0.00001f)
                return q >= 0f;
            float t = q / p;
            if (p < 0f)
                enter = Math.Max(enter, t);
            else
                exit = Math.Min(exit, t);
            return enter <= exit;
        }
    }
}
