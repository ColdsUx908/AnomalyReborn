namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(MemberInfo member)
    {
        public T Attribute<T>(bool inherit = true) where T : Attribute => member.GetCustomAttributes(typeof(T), inherit).AsValueEnumerable().OfType<T>().FirstOrDefault();

        public bool HasAttribute<T>(bool inherit = true) where T : Attribute => member.IsDefined(typeof(T), inherit);

        public bool TryGetAttribute<T>([NotNullWhen(true)] out T attribute, bool inherit = true) where T : Attribute => (attribute = member.Attribute<T>(inherit)) is not null;
    }
}
