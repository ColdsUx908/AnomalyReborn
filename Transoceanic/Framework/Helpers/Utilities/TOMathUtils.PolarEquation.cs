using System.ComponentModel;

namespace Transoceanic.Framework.Helpers;

public static partial class TOMathUtils
{
    public static class PolarEquation
    {
        /// <param name="angle">周期：2π。</param>
        public static float RegularPolygon(float angle, int sideAmount)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(sideAmount, 3);

            float period = MathHelper.TwoPi / sideAmount;
            float halfCenter = MathHelper.Pi * (sideAmount - 2) / (sideAmount * 2);
            angle = NormalizeWithPeriod(angle, period);
            return MathF.Sin(halfCenter) / MathF.Sin(angle + halfCenter);
        }

        /// <param name="angle">周期：2π。</param>
        public static float Triangle(float angle)
        {
            const float period = PiOver3 * 2;
            const float halfCenter = PiOver6;
            angle = NormalizeWithPeriod(angle, period);
            return MathF.Sin(halfCenter) / MathF.Sin(angle + halfCenter);
        }

        /// <param name="angle">周期：2π。</param>
        public static float Square(float angle)
        {
            //针对正方形特化的快速算法

            const float period = MathHelper.PiOver2;
            angle = NormalizeWithPeriod(angle, period);
            (float sin, float cos) = MathF.SinCos(angle);
            return 1f / (sin + cos);
        }

        /// <param name="angle">周期：4π。</param>
        public static float Pentagram(float angle)
        {
            const float period = PiOver5 * 4;
            const float halfCenter = PiOver10;
            angle = NormalizeWithPeriod(angle, period);
            return MathF.Sin(halfCenter) / MathF.Sin(angle + halfCenter);
        }

        /// <param name="angle">周期：2π。</param>
        public static float PentagramEdge(float angle)
        {
            const float period = PiOver5 * 2;
            const float halfPeriod = PiOver5;
            const float halfCenter = PiOver10;
            angle = NormalizeWithPeriod(angle, period);
            if (angle > halfPeriod)
                angle = period - angle;
            return MathF.Sin(halfCenter) / MathF.Sin(angle + halfCenter);
        }

        /// <summary>
        /// 计算圆弧上一点的极径。该圆弧以原点为极点，极轴为x轴正半轴，端点位于 (-1,0) 和 (1,0)，向上凸起，拱高为 <paramref name="archHeight"/>。
        /// </summary>
        /// <param name="angle">周期：π。</param>
        /// <param name="archHeight">拱高，取值范围通常为 (0, 1]，决定圆弧的弯曲程度。<br/></param>
        /// <returns>对应极角的极径长度。</returns>
        public static float Arc(float angle, float archHeight)
        {
            angle = NormalizeWithPeriod(angle, MathHelper.Pi);
            archHeight = Math.Clamp(archHeight, 0f, 1f);

            if (archHeight == 0f) //退化为直线
                return Math.Abs(angle - MathHelper.PiOver2) / MathHelper.PiOver2;
            else
            {
                float verticalDistance = Arc_CalculateVerticalDistance(archHeight);
                float temp = verticalDistance * MathF.Sin(angle);
                return MathF.Sqrt(1f + temp * temp) - temp;
            }
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static float Arc_CalculateVerticalDistance(float archHeight) => (1f - archHeight * archHeight) / (2f * archHeight);

        /// <summary>
        /// 拉梅曲线，表示形如 <c>|x|^n + |y|^n = 1</c> 的曲线。
        /// </summary>
        /// <param name="angle">周期：2π。</param>
        public static float LameCurve(float angle, float exponent)
        {
            const float period = MathHelper.PiOver2;
            angle = NormalizeWithPeriod(angle, period);
            (float sin, float cos) = MathF.SinCos(angle);
            return 1f / (MathF.Pow(sin, exponent) + MathF.Pow(cos, exponent));
        }
    }
}