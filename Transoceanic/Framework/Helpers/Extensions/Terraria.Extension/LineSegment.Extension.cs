// Developed by ColdsUx

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(LineSegment segment)
    {
        /// <summary>
        /// 获取线段的方向向量（从起点指向终点）。
        /// </summary>
        public Vector2 Vector => segment.End - segment.Start;

        /// <summary>
        /// 获取线段的长度。
        /// </summary>
        public float Length => Vector2.Distance(segment.Start, segment.End);

        /// <summary>
        /// 获取线段的旋转角度（弧度），即方向向量的角度。
        /// </summary>
        public float Rotation => segment.Vector.ToRotation();

        /// <summary>
        /// 获取线段的单位方向向量。
        /// </summary>
        public Vector2 Direction => segment.Vector.SafeNormalize();

        /// <summary>
        /// 获取线段的中点坐标。
        /// </summary>
        public Vector2 Midpoint => (segment.Start + segment.End) / 2;

        /// <summary>
        /// 计算点到线段的最短距离。
        /// </summary>
        /// <param name="point">给定的点。</param>
        /// <returns>点到线段的最短距离。</returns>
        public float DistanceToPoint(Vector2 point)
        {
            Vector2 vector = segment.Vector;
            Vector2 toPoint = point - segment.Start;

            float lengthSquared = vector.LengthSquared();

            // 线段退化为点，直接返回点到该点的距离
            if (lengthSquared == 0f)
                return Vector2.Distance(point, segment.Start);

            // 计算投影参数 t，并限制在 [0,1] 范围内
            float t = Math.Clamp(Vector2.Dot(toPoint, vector) / lengthSquared, 0f, 1f);

            Vector2 closest = segment.Start + t * vector;
            return Vector2.Distance(point, closest);
        }

        /// <summary>
        /// 计算点到线段的最短距离的平方。
        /// </summary>
        /// <param name="point">给定的点。</param>
        /// <returns>点到线段的最短距离的平方。</returns>
        public float DistanceToPointSquared(Vector2 point)
        {
            Vector2 vector = segment.Vector;
            Vector2 toPoint = point - segment.Start;

            float lengthSquared = vector.LengthSquared();

            // 线段退化为点，直接返回点到该点的距离
            if (lengthSquared == 0f)
                return Vector2.DistanceSquared(point, segment.Start);

            // 计算投影参数 t，并限制在 [0,1] 范围内
            float t = Math.Clamp(Vector2.Dot(toPoint, vector) / lengthSquared, 0f, 1f);

            Vector2 closest = segment.Start + t * vector;
            return Vector2.DistanceSquared(point, closest);
        }
    }

    extension(LineSegment)
    {
        /// <summary>
        /// 判断两条线段是否相交（包括端点接触）。
        /// </summary>
        /// <param name="a">第一条线段。</param>
        /// <param name="b">第二条线段。</param>
        /// <returns>如果两条线段相交（包括端点在另一条线段上），则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        public static bool Intersects(LineSegment a, LineSegment b)
        {
            Vector2 p = a.Start;
            Vector2 q = b.Start;
            Vector2 r = a.Vector;
            Vector2 s = b.Vector;

            //计算叉积
            float rxs = Vector2.Cross(r, s);
            float qpxr = Vector2.Cross(q - p, r);

            //平行时
            if (rxs == 0)
            {
                //如果共线，检查是否重叠
                if (qpxr == 0)
                {
                    //检查投影是否重叠
                    float t0 = Vector2.Dot(q - p, r) / Vector2.Dot(r, r);
                    float t1 = t0 + Vector2.Dot(s, r) / Vector2.Dot(r, r);

                    TOMathUtils.NormalizeMinMax(ref t0, ref t1);

                    //检查是否有重叠部分
                    if (t0 <= 1 && t1 >= 0)
                        return t0 < 1 && t1 > 0;
                }

                return false;
            }

            float t = Vector2.Cross(q - p, s) / rxs;
            float u = Vector2.Cross(q - p, r) / rxs;
            if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
                return true;

            return false;
        }
    }
}