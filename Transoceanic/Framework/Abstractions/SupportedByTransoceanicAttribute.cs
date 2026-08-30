namespace Transoceanic.Framework.Abstractions;

/// <summary>
/// 标记一个模组程序集，表示该模组受 Transoceanic 框架支持。
/// <br/>与TO相关的反射工具仅考虑标记了该特性的模组，其他模组将被忽略。
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class SupportedByTransoceanicAttribute : Attribute;

