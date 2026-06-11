// Developed by ColdsUx

using Transoceanic.DataStructures.Geometry;

namespace Transoceanic.Framework.Helpers;

public static partial class TOMathUtils
{
    /// <summary>
    /// 提供二维几何计算工具，包括距离测量与碰撞检测。
    /// </summary>
    public static class Geometry
    {
        /// <summary>
        /// 计算点 <paramref name="point"/> 到轴对齐矩形 <paramref name="rectangle"/> 的最短距离的平方。
        /// </summary>
        /// <param name="rectangle">轴对齐矩形。</param>
        /// <param name="point">目标点。</param>
        /// <returns>点到矩形边界（或内部）最短距离的平方。若点在矩形内部则返回 0。</returns>
        public static float MinDistanceSquaredFromTo(FloatRectangle rectangle, Vector2 point)
        {
            float deltaX = Math.Clamp(point.X, rectangle.Left, rectangle.Right) - point.X;
            float deltaY = Math.Clamp(point.Y, rectangle.Top, rectangle.Bottom) - point.Y;
            return deltaX * deltaX + deltaY * deltaY;
        }

        /// <summary>
        /// 计算点 <paramref name="point"/> 到轴对齐矩形 <paramref name="rectangle"/> 的最远距离的平方（即点到矩形最远顶点的距离）。
        /// </summary>
        /// <param name="rectangle">轴对齐矩形。</param>
        /// <param name="point">目标点。</param>
        /// <returns>点到矩形最远顶点距离的平方。</returns>
        public static float MaxDistanceSquaredFromTo(FloatRectangle rectangle, Vector2 point)
        {
            float deltaX = Math.Max(Math.Abs(point.X - rectangle.Left), Math.Abs(point.X - rectangle.Right));
            float deltaY = Math.Max(Math.Abs(point.Y - rectangle.Top), Math.Abs(point.Y - rectangle.Bottom));
            return deltaX * deltaX + deltaY * deltaY;
        }

        /// <summary>
        /// 计算点 <paramref name="point"/> 到轴对齐矩形 <paramref name="rectangle"/> 的最短距离。
        /// </summary>
        /// <param name="rectangle">轴对齐矩形。</param>
        /// <param name="point">目标点。</param>
        /// <returns>点到矩形边界（或内部）的最短距离。若点在矩形内部则返回 0。</returns>
        public static float MinDistanceFromTo(FloatRectangle rectangle, Vector2 point) => MathF.Sqrt(MinDistanceSquaredFromTo(rectangle, point));

        /// <summary>
        /// 计算点 <paramref name="point"/> 到轴对齐矩形 <paramref name="rectangle"/> 的最远距离。
        /// </summary>
        /// <param name="rectangle">轴对齐矩形。</param>
        /// <param name="point">目标点。</param>
        /// <returns>点到矩形最远顶点的距离。</returns>
        public static float MaxDistanceFromTo(FloatRectangle rectangle, Vector2 point) => MathF.Sqrt(MaxDistanceSquaredFromTo(rectangle, point));

        /// <summary>
        /// 在指定分离轴上进行投影重叠检测，用于分离轴定理（SAT）。
        /// </summary>
        /// <param name="axis">分离轴方向向量（无需归一化，但应保持一致）。</param>
        /// <param name="aPoints">形状 A 的顶点集合（只读跨度）。</param>
        /// <param name="bPoints">形状 B 的顶点集合（只读跨度）。</param>
        /// <returns>若两个形状在给定轴上的投影区间存在重叠，则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
        public static bool OverlapOnAxis(Vector2 axis, ReadOnlySpan<Vector2> aPoints, ReadOnlySpan<Vector2> bPoints)
        {
            float aMin = float.MaxValue, aMax = float.MinValue;
            foreach (Vector2 point in aPoints)
            {
                float projection = Vector2.Dot(point, axis);
                aMin = Math.Min(aMin, projection);
                aMax = Math.Max(aMax, projection);
            }
            float bMin = float.MaxValue, bMax = float.MinValue;
            foreach (Vector2 point in bPoints)
            {
                float projection = Vector2.Dot(point, axis);
                bMin = Math.Min(bMin, projection);
                bMax = Math.Max(bMax, projection);
            }
            return aMax >= bMin && bMax >= aMin;
        }

        /// <summary>
        /// 在轴对齐矩形 <paramref name="b"/> 的世界坐标轴（X 轴与 Y 轴）上进行投影重叠检测。
        /// <br/>此方法验证给定形状的顶点集合在水平方向和垂直方向上的投影区间是否均与矩形自身的区间相交，通常作为分离轴定理（SAT）的一部分，用于判断任意多边形（或点集）与轴对齐矩形之间是否存在碰撞。
        /// </summary>
        /// <param name="aPoints">
        /// 待检测形状所有顶点的只读跨度。顶点排列顺序任意，但必须包含形状的所有极值点，
        /// 以确保投影区间计算准确。
        /// </param>
        /// <param name="b">用于重叠检测的目标轴对齐矩形。</param>
        /// <returns>
        /// 如果 <paramref name="aPoints"/> 在 X 轴和 Y 轴上的投影区间均与矩形 <paramref name="b"/> 的对应区间存在重叠，则返回 <see langword="true"/>；否则返回 <see langword="false"/>。
        /// </returns>
        public static bool OverlapOnFloatRectangleAxis(ReadOnlySpan<Vector2> aPoints, FloatRectangle b)
        {
            float aMin = float.MaxValue, aMax = float.MinValue;
            foreach (Vector2 point in aPoints)
            {
                float projection = point.X;
                aMin = Math.Min(aMin, projection);
                aMax = Math.Max(aMax, projection);
            }
            if (aMax < b.Left || b.Right < aMin)
                return false;

            aMin = float.MaxValue;
            aMax = float.MinValue;
            foreach (Vector2 point in aPoints)
            {
                float projection = point.Y;
                aMin = Math.Min(aMin, projection);
                aMax = Math.Max(aMax, projection);
            }
            if (aMax < b.Top || b.Bottom < aMin)
                return false;

            return true;
        }

        #region 碰撞
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
            float distance = MinDistanceSquaredFromTo(a, b.Center);
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
            float distanceSquared = MinDistanceSquaredFromTo(a.Source, newCenter);
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
            float minDistanceSquared = MinDistanceSquaredFromTo(a, b.Center);
            if (minDistanceSquared > b.OuterRadius * b.OuterRadius)
                return false;

            float maxDistanceSquared = MaxDistanceSquaredFromTo(a, b.Center);
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
            (Vector2 aPoint1, Vector2 aPoint2, Vector2 aPoint3, Vector2 aPoint4) = a.Vertices;
            ReadOnlySpan<Vector2> aPoints = [aPoint1, aPoint2, aPoint3, aPoint4];
            ReadOnlySpan<Vector2> bPoints = [b.TopLeft, b.TopRight, b.BottomLeft, b.BottomRight];

            (float sinA, float cosA) = MathF.SinCos(a.Rotation);
            if (!OverlapOnAxis(new Vector2(cosA, sinA), aPoints, bPoints))
                return false;
            if (!OverlapOnAxis(new Vector2(-sinA, cosA), aPoints, bPoints))
                return false;

            if (!OverlapOnFloatRectangleAxis(aPoints, b))
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

            if (!OverlapOnAxis(new Vector2(-aSideVector1.Y, aSideVector1.X), aPoints, bPoints))
                return false;
            if (!OverlapOnAxis(new Vector2(-aSideVector2.Y, aSideVector2.X), aPoints, bPoints))
                return false;

            if (!OverlapOnFloatRectangleAxis(aPoints, b))
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
        #endregion 碰撞
    }
}