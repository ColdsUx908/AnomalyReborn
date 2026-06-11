// Developed by ColdsUx

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