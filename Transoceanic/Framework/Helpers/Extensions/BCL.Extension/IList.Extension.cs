namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension<T>(IList<T> values)
    {
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