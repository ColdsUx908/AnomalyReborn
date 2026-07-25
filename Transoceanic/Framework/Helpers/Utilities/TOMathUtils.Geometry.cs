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
    }
}