namespace Transoceanic.Runtime.Detours;

/// <summary>
/// 用于标记一个方法，使其在自动 Detour 应用逻辑中被忽略。
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class NotDetourMethodAttribute : Attribute;
