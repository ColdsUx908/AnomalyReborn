using Terraria.UI;

namespace Transoceanic.UI.Controls;

/// <summary>
/// 滚动容器的内容基类，提供显示、隐藏和重排回调。
/// </summary>
public class ScrollContent : UIElement
{
    /// <inheritdoc/>
    public override void Recalculate()
    {
        //Left.Set(0, 0);
        //Width = Parent.Width;
        base.Recalculate();
    }
    /// <inheritdoc/>
    /// <remarks>
    /// 将最大高度设为当前高度，再执行基类初始化。
    /// </remarks>
    public override void OnInitialize()
    {
        MaxHeight = Height;
        //if(Parent != null)
        //    Width = Parent.Width;
        base.OnInitialize();
    }
    /// <summary>
    /// 内容被显示后调用；默认不执行操作，供派生类初始化显示状态。
    /// </summary>
    public virtual void OnSetVisible()
    {

    }
    /// <summary>
    /// 内容被替换隐藏前调用；默认不执行操作，供派生类清理显示状态。
    /// </summary>
    public virtual void OnSetInvisible()
    {

    }
    /// <summary>
    /// 将最大高度设为当前高度；派生类可在此重新排列内容。
    /// </summary>
    public virtual void Rearrange()
    {
        MaxHeight = Height;
    }
}
