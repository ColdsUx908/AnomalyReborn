// Designed by ColdsUx

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(UnifiedRandom rand)
    {
        /// <summary>
        /// 返回一个 [0, 2π) 范围内的随机弧度值。
        /// </summary>
        /// <returns>随机弧度。</returns>
        public float NextRadian() => rand.NextFloat(MathHelper.TwoPi);

        /// <summary>
        /// 返回一个极坐标向量，长度固定为 <paramref name="length"/>，角度随机。
        /// </summary>
        /// <param name="length">向量的长度。</param>
        /// <returns>随机方向的极坐标向量。</returns>
        public PolarVector2 NextPolarVector2(float length) => new(length, rand.NextRadian());

        /// <summary>
        /// 返回一个极坐标向量，长度在指定范围内随机，角度随机。
        /// </summary>
        /// <param name="minLength">最小长度。</param>
        /// <param name="maxLength">最大长度。</param>
        /// <returns>随机长度和随机方向的极坐标向量。</returns>
        public PolarVector2 NextPolarVector2(float minLength, float maxLength) => new(rand.NextFloat(minLength, maxLength), rand.NextRadian());

        /// <summary>
        /// 根据给定的概率返回布尔值。
        /// </summary>
        /// <param name="probability">概率值（0~1）。</param>
        /// <returns>如果随机数小于概率则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        public bool NextProbability(float probability) => rand.NextFloat() < probability;
    }
}