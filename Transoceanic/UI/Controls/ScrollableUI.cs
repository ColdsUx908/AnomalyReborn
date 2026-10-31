using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;

namespace Transoceanic.UI.Controls;

/// <summary>
/// 裁剪溢出内容并通过滚动条控制垂直偏移的 UI 容器。
/// </summary>
public class ScrollableUI : UIElement
{
    /// <summary>
    /// 驱动内容垂直偏移的滚动条，可为 null。
    /// </summary>
    public UIScrollbar ScrollBar { get; private set; }
    /// <summary>
    /// 容器当前显示的滚动内容，由 SetContent 替换。
    /// </summary>
    public ScrollContent Content { get; private set; }
    private float _Sensitivity;
    private Color _BackColor;
    /// <summary>
    /// 创建裁剪溢出内容的滚动容器，保存滚动条与灵敏度；内容须通过 SetContent 挂接。
    /// </summary>
    public ScrollableUI(UIScrollbar scrollBar, float sensitivity = 0.2f)
    {
        SetScrollBar(scrollBar);
        _Sensitivity = sensitivity;
        OverflowHidden = true;
        Content = new ScrollContent();
        _BackColor = new Color(63, 82, 151) * 0.6f;
    }
    /// <inheritdoc/>
    public override void OnInitialize()
    {
        //SetContent(Content);
        base.OnInitialize();
    }
    /// <summary>
    /// 替换滚动条引用；传入 null 时停用滚动条驱动的位移。
    /// </summary>
    public void SetScrollBar(UIScrollbar scrollBar)
    {
        ScrollBar = scrollBar;
    }
    /// <summary>
    /// 替换并挂接滚动内容，更新滚动范围、布局及可见性回调；调用前须设置内容高度。
    /// </summary>
    /// <param name="content">新内容；为 null 或已是本容器子元素时不执行操作。</param>
    public void SetContent(ScrollContent content)
    {
        if (content == null || HasChild(content))
            return;
        Content?.OnSetInvisible();
        if (Content?.Parent == this) RemoveChild(Content);
        Content = content;
        Content.Left.Set(0, 0);
        Content.Width.Set(0, 1f);
        if (ScrollBar != null)
        {
            ScrollBar.SetView(Math.Max(0, GetInnerDimensions().Height), Math.Max(0, Content.Height.Pixels));
            Content.Top.Set(-ScrollBar.ViewPosition, 0);
        }
        else
            Content.Top.Set(0, 0);
        Append(Content);
        RearrangeContent();
        Content.Recalculate();
        Content.OnSetVisible();
    }
    /// <inheritdoc/>
    /// <remarks>
    /// 悬停时阻止原版滚轮操作，并根据滚动条视图位置更新内容偏移。
    /// </remarks>
    public override void Update(GameTime gameTime)
    {
        if (IsMouseHovering)
            PlayerInput.LockVanillaMouseScroll("Transoceanic: ScrollableUI");
        if (Content != null && ScrollBar != null)
        {
            if (Content.Top.Pixels != -ScrollBar.ViewPosition)
            {
                Content.Top.Set(-ScrollBar.ViewPosition, 0);
                Content.Recalculate();
            }
        }
        base.Update(gameTime);
    }
    /// <inheritdoc/>
    /// <remarks>
    /// 将滚轮增量乘以灵敏度后反向累加到滚动条视图位置。
    /// </remarks>
    public override void ScrollWheel(UIScrollWheelEvent evt)
    {
        base.ScrollWheel(evt);
        ScrollBar?.ViewPosition -= _Sensitivity * evt.ScrollWheelValue;
    }
    /// <inheritdoc/>
    /// <remarks>
    /// 绘制容器背景，再执行基类自身绘制。
    /// </remarks>
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        //Color drawColor = new Color(63, 82, 151) * 0.7f;
        Rectangle rect = GetDimensions().ToRectangle();
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, _BackColor);
        base.DrawSelf(spriteBatch);
    }
    /// <summary>
    /// 按容器宽度重算并排列内容，再用可视高度与内容高度更新滚动条范围。
    /// </summary>
    public void RearrangeContent()
    {
        if (Content != null)
        {
            Content.Width.Set(0, 1f);
            Content.Recalculate();
            Content.Rearrange();
            Content.Recalculate();
            ScrollBar?.SetView(Math.Max(0, GetInnerDimensions().Height), Math.Max(0, Content.Height.Pixels));
        }
    }
}
