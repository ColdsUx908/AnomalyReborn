using Transoceanic.Behaviors.Collections;

namespace Transoceanic.Behaviors;

/// <summary>
/// 定义实体行为必须实现的核心功能，包括所属 Mod、优先级、处理条件以及生命周期管理。
/// </summary>
public interface IEntityBehavior : ILoadable, IContentLoader
{
    /// <summary>
    /// 获取行为所属的 Mod 实例。
    /// </summary>
    public abstract Mod Mod { get; }

    /// <summary>
    /// 优先级，越大越先应用。
    /// <br/>设计规范：对于需优先应用的行为，建议设置为正值；对于需最后应用的行为，建议设置为负值。
    /// </summary>
    public abstract decimal Priority { get; }

    /// <summary>
    /// 获取一个值，指示当前行为是否应该执行处理逻辑。
    /// <br/>当通过 <see cref="SimpleEntityBehaviorSet{TEntity, TBehavior}"/> 遍历行为时，仅返回此属性为 <see langword="true"/> 的实例。
    /// </summary>
    public abstract bool ShouldProcess { get; }
}
