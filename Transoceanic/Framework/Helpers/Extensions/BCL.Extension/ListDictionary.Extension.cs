// Developed by ColdsUx

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension<TKey, TValue>(IDictionary<TKey, List<TValue>> dictionary)
    {
        /// <summary>
        /// 向字典中的指定键对应的列表中添加一个值。
        /// <br/>如果键不存在，则创建新的列表并添加值；如果键对应的值为 <see langword="null"/>，则重新初始化列表并添加值。
        /// </summary>
        /// <param name="key">要添加值的键。</param>
        /// <param name="value">要添加到列表中的值。</param>
        public void AddBetter(TKey key, TValue value)
        {
            if (dictionary.TryGetValue(key, out List<TValue> list))
            {
                if (list is null)
                    dictionary[key] = [value];
                else
                    list.Add(value);
            }
            else
                dictionary[key] = [value];
        }
    }
}