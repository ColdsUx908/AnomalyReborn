namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(LineSegment line)
    {
        public Vector2 Value => line.End - line.Start;

        public float Length => Vector2.Distance(line.Start, line.End);

        public float Rotation => line.Value.ToRotation();

        public Vector2 Direction => line.Value.SafeNormalize();

        public Vector2 Midpoint => (line.Start + line.End) / 2;
    }

    extension(LineSegment)
    {
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