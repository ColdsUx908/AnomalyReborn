namespace Transoceanic.Framework.ExternalAttributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class PublicizeAttribute : Attribute
{
    public Type TargetType { get; }
    public PublicizeAttribute(Type targetType) => TargetType = targetType;
}