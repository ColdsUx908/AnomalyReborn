namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(LineSegment line)
    {
        /// <summary>
        /// 获取线段的方向向量（从起点指向终点）。
        /// </summary>
        public Vector2 Value => line.End - line.Start;

        /// <summary>
        /// 获取线段的长度。
        /// </summary>
        public float Length => Vector2.Distance(line.Start, line.End);

        /// <summary>
        /// 获取线段的旋转角度（弧度），即方向向量的角度。
        /// </summary>
        public float Rotation => line.Value.ToRotation();

        /// <summary>
        /// 获取线段的单位方向向量。
        /// </summary>
        public Vector2 Direction => line.Value.SafeNormalize();

        /// <summary>
        /// 获取线段的中点坐标。
        /// </summary>
        public Vector2 Midpoint => (line.Start + line.End) / 2;
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
            Vector2 r = a.Value;
            Vector2 s = b.Value;

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