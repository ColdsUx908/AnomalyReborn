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
        /// 检测两个实现了相互碰撞接口的对象之间的碰撞。
        /// </summary>
        /// <typeparam name="T1">第一个对象的类型，需实现 <see cref="ICollidable{T1, T2}"/>。</typeparam>
        /// <typeparam name="T2">第二个对象的类型，需实现 <see cref="ICollidable{T2, T1}"/>。</typeparam>
        /// <param name="a">第一个对象。</param>
        /// <param name="b">第二个对象。</param>
        /// <returns>若发生碰撞则为 <see langword="true"/>，否则为 <see langword="false"/>。</returns>
        public static bool Collides<T1, T2>(T1 a, T2 b)
            where T1 : ICollidable<T1, T2>
            where T2 : ICollidable<T2, T1>
            => a.Collides(b);

        /// <summary>
        /// 检测轴对齐矩形与圆的碰撞。
        /// </summary>
        /// <param name="a">轴对齐矩形。</param>
        /// <param name="b">圆。</param>
        /// <returns>若矩形与圆相交则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        public static bool FloatRectanglevCircleCollision(FloatRectangle a, Circle b)
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
        public static bool RotatedRectanglevCircleCollision(RotatedRectangle a, Circle b)
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
        public static bool FloatRectanglevRingCollision(FloatRectangle a, Ring b)
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
        public static bool RotatedRectanglevFloatRectangleCollision(RotatedRectangle a, FloatRectangle b)
        {
            (Vector2 aPoint1, Vector2 aPoint2, Vector2 aPoint3, Vector2 aPoint4) = a.Vertices;
            ReadOnlySpan<Vector2> aPoints = [aPoint1, aPoint2, aPoint3, aPoint4];
            ReadOnlySpan<Vector2> bPoints = [b.TopLeft, b.TopRight, b.BottomLeft, b.BottomRight];

            (float sinA, float cosA) = MathF.SinCos(a.Rotation);
            if (!OverlapOnAxis(new Vector2(cosA, sinA), aPoints, bPoints))
                return false;
            if (!OverlapOnAxis(new Vector2(-sinA, cosA), aPoints, bPoints))
                return false;

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
        #endregion 碰撞
    }
}