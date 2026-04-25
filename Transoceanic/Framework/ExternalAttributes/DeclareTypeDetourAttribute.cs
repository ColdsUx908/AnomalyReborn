// Designed by ColdsUx

namespace Transoceanic.Framework.ExternalAttributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DeclareTypeDetourAttribute : Attribute
{
    public Type TargetType { get; }

    public DeclareTypeDetourAttribute(Type targetType) => TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
}