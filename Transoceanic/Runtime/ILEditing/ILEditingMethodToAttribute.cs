namespace Transoceanic.Runtime.ILEditing;

/// <summary>
/// 标记一个不在 IL 编辑类中的方法，指明其要编辑 IL 的目标源类型。
/// </summary>
/// <remarks>
/// 自动扫描使用 IL_方法名__参数类型 标记选择重载；例如 IL_Update__Player。
/// 可连续追加 __ 类型标记以消除歧义；无法唯一定位时会明确报错。
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ILEditingMethodToAttribute : Attribute
{
    /// <summary>
    /// 目标源类型。
    /// </summary>
    public readonly Type SourceType;

    /// <summary>
    /// 初始化 <see cref="ILEditingMethodToAttribute"/> 的新实例。
    /// </summary>
    /// <param name="targetType">目标源类型。</param>
    public ILEditingMethodToAttribute(Type targetType) => SourceType = targetType ?? throw new ArgumentNullException(nameof(targetType));
}
