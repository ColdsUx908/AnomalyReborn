using Transoceanic.DataStructures.Geometry;

namespace Transoceanic.Framework.Helpers;

public static partial class TOMathUtils
{
    public static class Geometry
    {
        public static float MinDistanceSquaredFromTo(FloatRectangle rectangle, Vector2 point)
        {
            float deltaX = Math.Clamp(point.X, rectangle.Left, rectangle.Right) - point.X;
            float deltaY = Math.Clamp(point.Y, rectangle.Top, rectangle.Bottom) - point.Y;
            return deltaX * deltaX + deltaY * deltaY;
        }

        public static float MaxDistanceSquaredFromTo(FloatRectangle rectangle, Vector2 point)
        {
            float deltaX = Math.Max(Math.Abs(point.X - rectangle.Left), Math.Abs(point.X - rectangle.Right));
            float deltaY = Math.Max(Math.Abs(point.Y - rectangle.Top), Math.Abs(point.Y - rectangle.Bottom));
            return deltaX * deltaX + deltaY * deltaY;
        }

        public static float MinDistanceFromTo(FloatRectangle rectangle, Vector2 point) => MathF.Sqrt(MinDistanceSquaredFromTo(rectangle, point));
        public static float MaxDistanceFromTo(FloatRectangle rectangle, Vector2 point) => MathF.Sqrt(MaxDistanceSquaredFromTo(rectangle, point));

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
        public static bool Collides<T>(T a, Rectangle targetHitbox) where T : ICollidableWithRectangle => a.Collides(targetHitbox);

        public static bool Collides<T1, T2>(T1 a, T2 b)
            where T1 : ICollidable<T1, T2>
            where T2 : ICollidable<T2, T1>
            => a.Collides(b);

        public static bool FloatRectanglevCircleCollision(FloatRectangle a, Circle b)
        {
            float distance = MinDistanceSquaredFromTo(a, b.Center);
            return distance <= b.Radius * b.Radius;
        }

        public static bool RotatedRectanglevCircleCollision(RotatedRectangle a, Circle b)
        {
            Vector2 newCenter = b.Center.RotatedBy(-a.Rotation, a.Center);
            float distanceSquared = MinDistanceSquaredFromTo(a.Source, newCenter);
            return distanceSquared <= b.Radius * b.Radius;
        }

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