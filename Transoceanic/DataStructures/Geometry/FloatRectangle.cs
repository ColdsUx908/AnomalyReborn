// Developed by ColdsUx

namespace Transoceanic.DataStructures.Geometry;

/// <summary>
/// 表示一个轴对齐的矩形，其位置和尺寸使用浮点数表示。
/// </summary>
public struct FloatRectangle : IEquatable<FloatRectangle>,
    ICollidableWithRectangle,
    ICollidable<FloatRectangle, FloatRectangle>,
    ICollidable<FloatRectangle, Circle>,
    ICollidable<FloatRectangle, RotatedRectangle>,
    ICollidable<FloatRectangle, Ring>
{
    /// <summary>
    /// 矩形左上角的位置坐标。
    /// </summary>
    public Vector2 Position;

    /// <summary>
    /// 矩形的宽度。
    /// </summary>
    public float Width;

    /// <summary>
    /// 矩形的高度。
    /// </summary>
    public float Height;

    /// <summary>
    /// 获取矩形左边缘的 X 坐标。
    /// </summary>
    public readonly float Left => Position.X;

    /// <summary>
    /// 获取矩形右边缘的 X 坐标。
    /// </summary>
    public readonly float Right => Position.X + Width;

    /// <summary>
    /// 获取矩形上边缘的 Y 坐标。
    /// </summary>
    public readonly float Top => Position.Y;

    /// <summary>
    /// 获取矩形下边缘的 Y 坐标。
    /// </summary>
    public readonly float Bottom => Position.Y + Height;

    /// <summary>
    /// 获取矩形的中心点坐标。
    /// </summary>
    public readonly Vector2 Center => new(Position.X + Width / 2, Position.Y + Height / 2);

    /// <summary>
    /// 获取矩形左上角的坐标。
    /// </summary>
    public readonly Vector2 TopLeft => Position;
    /// <summary>
    /// 获取矩形右上角的坐标。
    /// </summary>
    public readonly Vector2 TopRight => new(Position.X + Width, Position.Y);
    /// <summary>
    /// 获取矩形左下角的坐标。
    /// </summary>
    public readonly Vector2 BottomLeft => new(Position.X, Position.Y + Height);
    /// <summary>
    /// 获取矩形右下角的坐标。
    /// </summary>
    public readonly Vector2 BottomRight => new(Position.X + Width, Position.Y + Height);

    /// <summary>
    /// 使用指定的位置和尺寸初始化 <see cref="FloatRectangle"/> 结构的新实例。
    /// </summary>
    /// <param name="position">矩形左上角的位置。</param>
    /// <param name="width">矩形的宽度。</param>
    /// <param name="height">矩形的高度。</param>
    public FloatRectangle(Vector2 position, float width, float height)
    {
        Position = position;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// 使用指定的坐标分量和尺寸初始化 <see cref="FloatRectangle"/> 结构的新实例。
    /// </summary>
    /// <param name="x">矩形左上角的 X 坐标。</param>
    /// <param name="y">矩形左上角的 Y 坐标。</param>
    /// <param name="width">矩形的宽度。</param>
    /// <param name="height">矩形的高度。</param>
    public FloatRectangle(float x, float y, float width, float height) : this(new Vector2(x, y), width, height) { }

    /// <summary>
    /// 使用指定的位置和尺寸向量初始化 <see cref="FloatRectangle"/> 结构的新实例。
    /// </summary>
    /// <param name="position">矩形左上角的位置。</param>
    /// <param name="size">包含宽度 (X) 和高度 (Y) 的向量。</param>
    public FloatRectangle(Vector2 position, Vector2 size) : this(position, size.X, size.Y) { }

    /// <summary>
    /// 根据中心点位置和尺寸创建一个新的 <see cref="FloatRectangle"/> 实例。
    /// </summary>
    /// <param name="center">矩形中心点的坐标。</param>
    /// <param name="width">矩形的宽度。</param>
    /// <param name="height">矩形的高度。</param>
    /// <returns>根据中心点生成的 <see cref="FloatRectangle"/>。</returns>
    public static FloatRectangle FromCenter(Vector2 center, float width, float height) => new(new Vector2(center.X - width / 2, center.Y - height / 2), width, height);

    /// <summary>
    /// 根据一个内部点及其到矩形四边的距离创建一个新的 <see cref="FloatRectangle"/> 实例。
    /// </summary>
    /// <param name="point">矩形内部的一个点。</param>
    /// <param name="left">点到矩形左边缘的距离。</param>
    /// <param name="right">点到矩形右边缘的距离。</param>
    /// <param name="top">点到矩形上边缘的距离。</param>
    /// <param name="bottom">点到矩形下边缘的距离。</param>
    /// <returns>根据内部点生成的 <see cref="FloatRectangle"/>。</returns>
    public static FloatRectangle FromInnerPoint(Vector2 point, float left, float right, float top, float bottom) => new(new Vector2(point.X - left, point.Y - top), left + right, top + bottom);

    public readonly bool Equals(FloatRectangle other) => Position == other.Position && Width == other.Width && Height == other.Height;
    public override readonly bool Equals(object obj) => obj is FloatRectangle other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(Position, Width, Height);
    public static bool operator ==(FloatRectangle left, FloatRectangle right) => left.Equals(right);
    public static bool operator !=(FloatRectangle left, FloatRectangle right) => !(left == right);

    /// <summary>
    /// 返回当前矩形的字符串表示形式。
    /// </summary>
    /// <returns>
    /// 一个格式为 <c>"FloatRectangle { Position: {X:0 Y:0}, Width:100, Height:50 }"</c> 的字符串，
    /// 其中 <c>Position</c> 使用 <see cref="Vector2"/> 的默认格式输出，
    /// <c>Width</c> 和 <c>Height</c> 输出为数值形式。
    /// </returns>
    public override readonly string ToString() => $"FloatRectangle {{ Position: {Position}, Width: {Width}, Height: {Height} }}";

    /// <summary>
    /// 定义从 <see cref="Rectangle"/> 到 <see cref="FloatRectangle"/> 的隐式转换。
    /// </summary>
    /// <param name="rect">要转换的 <see cref="Rectangle"/>。</param>
    public static implicit operator FloatRectangle(Rectangle rect) => new(rect.X, rect.Y, rect.Width, rect.Height);

    /// <summary>
    /// 判断指定的点是否位于当前矩形内部（包含边界）。
    /// </summary>
    /// <param name="point">要测试的点坐标。</param>
    /// <returns>如果点在矩形内（含边界），则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public readonly bool Contains(Vector2 point) => point.X >= Left && point.X <= Right && point.Y >= Top && point.Y <= Bottom;

    public readonly bool Collides(Rectangle other) => Collides((FloatRectangle)other);
    public readonly bool Collides(FloatRectangle other) => Left < other.Right && Right > other.Left && Top < other.Bottom && Bottom > other.Top;
    public readonly bool Collides(Circle other) => TOMathUtils.Geometry.FloatRectanglevCircleCollision(this, other);
    public readonly bool Collides(RotatedRectangle other) => TOMathUtils.Geometry.RotatedRectanglevFloatRectangleCollision(other, this);
    public readonly bool Collides(Ring other) => TOMathUtils.Geometry.FloatRectanglevRingCollision(this, other);
}