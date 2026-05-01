// Developed by ColdsUx

using System.ComponentModel;

namespace Transoceanic.Framework.Helpers;

public static partial class TOMathUtils
{
    /// <summary>
    /// 提供基于极坐标的曲线方程计算。
    /// </summary>
    public static class PolarEquation
    {
        /// <summary>
        /// 计算正多边形的极径方程。
        /// </summary>
        /// <param name="angle">极角，周期为 2π。</param>
        /// <param name="sideAmount">正多边形的边数，必须大于等于 3。</param>
        /// <returns>给定极角处的极径长度。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="sideAmount"/> 小于 3 时抛出。</exception>
        public static float RegularPolygon(float angle, int sideAmount)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(sideAmount, 3);

            float period = MathHelper.TwoPi / sideAmount;
            float halfCenter = MathHelper.Pi * (sideAmount - 2) / (sideAmount * 2);
            angle = NormalizeWithPeriod(angle, period);
            return MathF.Sin(halfCenter) / MathF.Sin(angle + halfCenter);
        }

        /// <summary>
        /// 计算正三角形（等边三角形）的极径方程。
        /// </summary>
        /// <param name="angle">极角，周期为 2π。</param>
        /// <returns>给定极角处的极径长度。</returns>
        public static float Triangle(float angle)
        {
            const float period = PiOver3 * 2;
            const float halfCenter = PiOver6;
            angle = NormalizeWithPeriod(angle, period);
            return MathF.Sin(halfCenter) / MathF.Sin(angle + halfCenter);
        }

        /// <summary>
        /// 计算正方形的极径方程（特化快速算法）。
        /// </summary>
        /// <param name="angle">极角，周期为 2π。</param>
        /// <returns>给定极角处的极径长度。</returns>
        public static float Square(float angle)
        {
            const float period = MathHelper.PiOver2;
            angle = NormalizeWithPeriod(angle, period);
            (float sin, float cos) = MathF.SinCos(angle);
            return 1f / (sin + cos);
        }

        /// <summary>
        /// 计算五角星（外轮廓）的极径方程。
        /// </summary>
        /// <param name="angle">极角，周期为 4π（覆盖整个星形轮廓）。</param>
        /// <returns>给定极角处的极径长度。</returns>
        public static float Pentagram(float angle)
        {
            const float period = PiOver5 * 4;
            const float halfCenter = PiOver10;
            angle = NormalizeWithPeriod(angle, period);
            return MathF.Sin(halfCenter) / MathF.Sin(angle + halfCenter);
        }

        /// <summary>
        /// 计算五角星边缘（仅外凸部分）的极径方程。
        /// </summary>
        /// <param name="angle">极角，周期为 2π。</param>
        /// <returns>给定极角处的极径长度。</returns>
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
        /// 计算圆弧的极径方程。该圆弧以原点为极点，极轴为 x 轴正半轴，端点位于 (-1, 0) 和 (1, 0)，向上凸起。
        /// </summary>
        /// <param name="angle">极角，周期为 π。</param>
        /// <param name="archHeight">拱高，取值范围通常为 (0, 1]，决定圆弧的弯曲程度。为 0 时退化为直线段。</param>
        /// <returns>给定极角处的极径长度。</returns>
        public static float Arc(float angle, float archHeight)
        {
            angle = NormalizeWithPeriod(angle, MathHelper.Pi);
            archHeight = Math.Clamp(archHeight, 0f, 1f);

            if (archHeight == 0f) // 退化为直线
                return Math.Abs(angle - MathHelper.PiOver2) / MathHelper.PiOver2;
            else
            {
                float verticalDistance = Arc_CalculateVerticalDistance(archHeight);
                float temp = verticalDistance * MathF.Sin(angle);
                return MathF.Sqrt(1f + temp * temp) - temp;
            }
        }

        /// <summary>
        /// 计算圆弧辅助参数：拱高对应的垂直距离。
        /// </summary>
        /// <param name="archHeight">拱高。</param>
        /// <returns>垂直距离。</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static float Arc_CalculateVerticalDistance(float archHeight) => (1f - archHeight * archHeight) / (2f * archHeight);

        /// <summary>
        /// 计算拉梅曲线（Lamé curve）的极径方程，曲线方程形式为 <c>|x|^n + |y|^n = 1</c>。
        /// </summary>
        /// <param name="angle">极角，周期为 2π。</param>
        /// <param name="exponent">指数 n，通常大于 0。n=2 时为圆，n=1 时为菱形，n→∞ 时趋近于正方形。</param>
        /// <returns>给定极角处的极径长度。</returns>
        public static float LameCurve(float angle, float exponent)
        {
            const float period = MathHelper.PiOver2;
            angle = NormalizeWithPeriod(angle, period);
            (float sin, float cos) = MathF.SinCos(angle);
            return 1f / (MathF.Pow(sin, exponent) + MathF.Pow(cos, exponent));
        }
    }
}