namespace Transoceanic.Extensions.System;

/// <summary>
/// 提供集合、反射、文本和参数校验等 System 类型扩展。
/// </summary>
public static partial class TOSystemExtensions
{
    extension<T>(IEnumerable<T>)
    {
        /// <summary>
        /// 求两个序列的并集。
        /// </summary>
        /// <param name="left">第一个序列。</param>
        /// <param name="right">第二个序列。</param>
        /// <returns>包含两个序列中所有非重复元素的序列。</returns>
        public static IEnumerable<T> operator |(IEnumerable<T> left, IEnumerable<T> right) => left.Union(right);

        /// <summary>
        /// 求两个序列的交集。
        /// </summary>
        /// <param name="left">第一个序列。</param>
        /// <param name="right">第二个序列。</param>
        /// <returns>包含两个序列中共有元素的序列。</returns>
        public static IEnumerable<T> operator &(IEnumerable<T> left, IEnumerable<T> right) => left.Intersect(right);
    }
}
