// Developed by ColdsUx

using Transoceanic.DataStructures.Geometry;

namespace Transoceanic.Framework.Abstractions;

/// <summary>
/// 提供在世界坐标系与指定平面坐标系之间进行转换的能力。
/// </summary>
/// <typeparam name="TSelf">实现此接口的几何类型。</typeparam>
public interface ICoordinateTransformable<TSelf> where TSelf : ICoordinateTransformable<TSelf>
{
    /// <summary>
    /// 将当前对象（位于世界坐标系）转换到指定的局部坐标系中。
    /// </summary>
    /// <param name="localSystem">目标局部坐标系。</param>
    /// <returns>在 <paramref name="localSystem"/> 坐标系下表示的几何对象。</returns>
    public abstract TSelf WorldToLocal(PlaneCoordinateSystem localSystem);

    /// <summary>
    /// 将当前对象（位于指定的局部坐标系）转换到世界坐标系。
    /// </summary>
    /// <param name="localSystem">当前对象所在的局部坐标系。</param>
    /// <returns>在世界坐标系中表示的几何对象。</returns>
    public abstract TSelf LocalToWorld(PlaneCoordinateSystem localSystem);
}