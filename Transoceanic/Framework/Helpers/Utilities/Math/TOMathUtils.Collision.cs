using Transoceanic.DataStructures.Geometry;

namespace Transoceanic.Framework.Helpers;

public static partial class TOMathUtils
{
    /// <summary>
    /// 提供用于碰撞检测的实用方法。
    /// </summary>
    public static class Collision
    {
        /// <summary>
        /// 获取用于液体碰撞检测的调整后的矩形碰撞箱。
        /// 该矩形的宽度被限制为不超过 10 像素，高度为原始高度的一半，并且以原始矩形的中心为中心。
        /// </summary>
        /// <param name="Position">原始矩形的左上角位置。</param>
        /// <param name="Width">原始矩形的宽度。</param>
        /// <param name="Height">原始矩形的高度。</param>
        /// <returns>调整后的矩形碰撞箱。</returns>
        public static Rectangle GetAdjustedHitboxForWetCollision(Vector2 Position, int Width, int Height)
        {
            Vector2 center = new(Position.X + Width / 2f, Position.Y + Height / 2f);
            int newWidth = Math.Min(Width, 10);
            int newHeight = Height / 2;
            Rectangle newHitbox = Rectangle.FromCenter(center, newWidth, newHeight);
            return newHitbox;
        }

        /// <summary>
        /// 检测实现了 <see cref="ICollidableWithRectangle"/> 接口的对象与指定矩形的碰撞。
        /// </summary>
        /// <typeparam name="T">实现了 <see cref="ICollidableWithRectangle"/> 的类型。</typeparam>
        /// <param name="a">可碰撞对象。</param>
        /// <param name="targetHitbox">目标矩形碰撞箱。</param>
        /// <returns>若发生碰撞则为 <see langword="true"/>，否则为 <see langword="false"/>。</returns>
        public static bool Collides<T>(T a, Rectangle targetHitbox) where T : ICollidableWithRectangle => a.Collides(targetHitbox);

        /// <summary>
        /// 检测轴对齐矩形与圆的碰撞。
        /// </summary>
        /// <param name="a">轴对齐矩形。</param>
        /// <param name="b">圆。</param>
        /// <returns>若矩形与圆相交则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        public static bool FloatRectangleVCircleCollision(FloatRectangle a, Circle b)
        {
            float distance = Geometry.MinDistanceSquaredFromTo(a, b.Center);
            return distance <= b.Radius * b.Radius;
        }

        /// <summary>
        /// 检测旋转矩形与圆的碰撞。
        /// </summary>
        /// <param name="a">旋转矩形。</param>
        /// <param name="b">圆。</param>
        /// <returns>若旋转矩形与圆相交则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        public static bool RotatedRectangleVCircleCollision(RotatedRectangle a, Circle b)
        {
            Vector2 newCenter = b.Center.RotatedBy(-a.Rotation, a.Center);
            float distanceSquared = Geometry.MinDistanceSquaredFromTo(a.Source, newCenter);
            return distanceSquared <= b.Radius * b.Radius;
        }

        /// <summary>
        /// 检测轴对齐矩形与圆环的碰撞。
        /// </summary>
        /// <param name="a">轴对齐矩形。</param>
        /// <param name="b">圆环（具有内外半径）。</param>
        /// <returns>若矩形与圆环区域相交则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        public static bool FloatRectangleVAnnulusCollision(FloatRectangle a, Annulus b)
        {
            float minDistanceSquared = Geometry.MinDistanceSquaredFromTo(a, b.Center);
            if (minDistanceSquared > b.OuterRadius * b.OuterRadius)
                return false;

            float maxDistanceSquared = Geometry.MaxDistanceSquaredFromTo(a, b.Center);
            if (maxDistanceSquared < b.InnerRadius * b.InnerRadius)
                return false;

            return true;
        }

        /// <summary>
        /// 检测旋转矩形与轴对齐矩形的碰撞（基于分离轴定理）。
        /// </summary>
        /// <param name="a">旋转矩形。</param>
        /// <param name="b">轴对齐矩形。</param>
        /// <returns>若两者相交则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        public static bool RotatedRectangleVFloatRectangleCollision(RotatedRectangle a, FloatRectangle b)
        {
            //包围盒检测
            if (!a.AABB.Collides(b))
                return false;

            (Vector2 aPoint1, Vector2 aPoint2, Vector2 aPoint3, Vector2 aPoint4) = a.Vertices;
            ReadOnlySpan<Vector2> aPoints = [aPoint1, aPoint2, aPoint3, aPoint4];
            ReadOnlySpan<Vector2> bPoints = [b.TopLeft, b.TopRight, b.BottomLeft, b.BottomRight];

            (float sinA, float cosA) = MathF.SinCos(a.Rotation);
            if (!Geometry.OverlapOnAxis(new Vector2(cosA, sinA), aPoints, bPoints))
                return false;
            if (!Geometry.OverlapOnAxis(new Vector2(-sinA, cosA), aPoints, bPoints))
                return false;

            if (!Geometry.OverlapOnFloatRectangleAxis(aPoints, b))
                return false;

            return true;
        }

        /// <summary>
        /// 检测平行四边形与轴对齐矩形的碰撞（基于分离轴定理）。
        /// </summary>
        /// <param name="a">平行四边形。</param>
        /// <param name="b">轴对齐矩形。</param>
        /// <returns>若两者相交则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        public static bool ParallelogramVFloatRectangleCollision(Parallelogram a, FloatRectangle b)
        {
            (Vector2 aPoint1, Vector2 aPoint2, Vector2 aPoint3, Vector2 aPoint4) = a.Vertices;
            ReadOnlySpan<Vector2> aPoints = [aPoint1, aPoint2, aPoint3, aPoint4];
            ReadOnlySpan<Vector2> bPoints = [b.TopLeft, b.TopRight, b.BottomLeft, b.BottomRight];

            (LineSegment aSide1, LineSegment aSide2, _, _) = a.Sides;
            Vector2 aSideVector1 = aSide1.Vector;
            Vector2 aSideVector2 = aSide2.Vector;

            if (!Geometry.OverlapOnAxis(new Vector2(-aSideVector1.Y, aSideVector1.X), aPoints, bPoints))
                return false;
            if (!Geometry.OverlapOnAxis(new Vector2(-aSideVector2.Y, aSideVector2.X), aPoints, bPoints))
                return false;

            if (!Geometry.OverlapOnFloatRectangleAxis(aPoints, b))
                return false;

            return true;
        }

        /// <summary>
        /// 检测平行四边形与圆的碰撞。
        /// </summary>
        /// <param name="a">平行四边形。</param>
        /// <param name="b">圆。</param>
        /// <returns>若平行四边形与圆相交则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        public static bool ParallelogramVCircleCollision(Parallelogram a, Circle b)
        {
            Vector2 center = b.Center;
            if (a.ContainsPoint(center)) //圆心在平行四边形内
                return true;

            (LineSegment c, LineSegment d, LineSegment e, LineSegment f) = a.Sides;
            float radiusSquared = b.Radius * b.Radius;
            return c.DistanceToPointSquared(center) <= radiusSquared || d.DistanceToPointSquared(center) <= radiusSquared || e.DistanceToPointSquared(center) <= radiusSquared || f.DistanceToPointSquared(center) <= radiusSquared;
        }
    }
}
