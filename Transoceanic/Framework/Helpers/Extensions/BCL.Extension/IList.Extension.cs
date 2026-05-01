// Developed by ColdsUx

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension<T>(IList<T> values)
    {
        /// <summary>
        /// 尝试获取列表中指定索引处的元素。
        /// </summary>
        /// <param name="index">要获取元素的从零开始的索引。</param>
        /// <param name="value">
        /// 当此方法返回时，如果索引有效，则包含位于指定索引处的元素；否则包含 <typeparamref name="T"/> 的默认值。
        /// 此参数以未初始化状态传递。
        /// </param>
        /// <returns>
        /// 如果索引有效（即大于等于 0 且小于列表元素数），则为 <see langword="true"/>；
        /// 否则为 <see langword="false"/>。
        /// </returns>
        public bool TryGetValue(int index, out T value)
        {
            if (index >= 0 && index < values.Count)
            {
                value = values[index];
                return true;
            }
            value = default;
            return false;
        }
    }
}