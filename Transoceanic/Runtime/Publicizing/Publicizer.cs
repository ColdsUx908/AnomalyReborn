namespace Transoceanic.Runtime.Publicizing;

/// <summary>
/// 持有源实例的非公开成员访问包装器基类，由源生成器补充访问成员。
/// </summary>
public abstract class InstancedPublicizer(object Source)
{
    /// <summary>
    /// 此访问包装器持有的源对象引用。
    /// </summary>
    public object Source { get; } = Source;
}
