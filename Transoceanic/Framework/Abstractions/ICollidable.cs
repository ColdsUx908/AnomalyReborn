// Designed by ColdsUx

namespace Transoceanic.Framework.Abstractions;

/// <summary>
/// 表示可与 <see cref="Rectangle"/> 进行碰撞检测的类型。
/// </summary>
public interface ICollidableWithRectangle
{
    /// <summary>
    /// 判断当前对象是否与指定的 <see cref="Rectangle"/> 发生碰撞。
    /// </summary>
    /// <param name="other">要检测的矩形区域。</param>
    /// <returns>若发生碰撞则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
    public abstract bool Collides(Rectangle other);
}

/// <summary>
/// 提供双向类型安全的碰撞检测接口。
/// </summary>
/// <typeparam name="TSelf">实现该接口的具体类型，用于表示自身。</typeparam>
/// <typeparam name="TOther">与之进行碰撞检测的对方类型，该类型也必须实现本接口且将 <typeparamref name="TSelf"/> 作为对方类型。</typeparam>
public interface ICollidable<TSelf, TOther>
    where TSelf : ICollidable<TSelf, TOther>
    where TOther : ICollidable<TOther, TSelf>
{
    /// <summary>
    /// 判断当前对象是否与指定的其他可碰撞对象发生碰撞。
    /// </summary>
    /// <param name="other">要检测的对方对象。</param>
    /// <returns>若发生碰撞则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
    public abstract bool Collides(TOther other);
}