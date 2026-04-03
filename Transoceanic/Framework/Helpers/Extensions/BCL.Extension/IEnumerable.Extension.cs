namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension<T>(IEnumerable<T>)
    {
        public static IEnumerable<T> operator |(IEnumerable<T> left, IEnumerable<T> right) => left.Union(right);

        public static IEnumerable<T> operator &(IEnumerable<T> left, IEnumerable<T> right) => left.Intersect(right);
    }
}
