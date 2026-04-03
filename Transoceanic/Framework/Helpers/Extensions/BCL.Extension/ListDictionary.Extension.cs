namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension<TKey, TValue>(IDictionary<TKey, List<TValue>> dictionary)
    {
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