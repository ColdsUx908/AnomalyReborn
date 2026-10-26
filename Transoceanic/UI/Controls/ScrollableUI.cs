using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;

namespace Transoceanic.UI.Controls;

public class ScrollableUI : UIElement
{
    public UIScrollbar ScrollBar { get; private set; }
    public ScrollContent Content { get; private set; }
    private float _Sensitivity;
    private Color _BackColor;
    public ScrollableUI(UIScrollbar scrollBar, float sensitivity = 0.2f)
    {
        SetScrollBar(scrollBar);
        _Sensitivity = sensitivity;
        OverflowHidden = true;
        Content = new ScrollContent();
        _BackColor = new Color(63, 82, 151) * 0.6f;
    }
    public override void OnInitialize()
    {
        //SetContent(Content);
        base.OnInitialize();
    }
    public void SetScrollBar(UIScrollbar scrollBar)
    {
        ScrollBar = scrollBar;
    }
    /// <summary>
    /// Set the height of the content before calling
    /// </summary>
    /// <param name="content">Not <see cref="null"/></param>
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
    public override void ScrollWheel(UIScrollWheelEvent evt)
    {
        base.ScrollWheel(evt);
        if (ScrollBar != null)
            ScrollBar.ViewPosition -= _Sensitivity * evt.ScrollWheelValue;
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        //Color drawColor = new Color(63, 82, 151) * 0.7f;
        Rectangle rect = GetDimensions().ToRectangle();
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, _BackColor);
        base.DrawSelf(spriteBatch);
    }
    public void RearrangeContent()
    {
        if (Content != null)
        {
            Content.Width.Set(0, 1f);
            Content.Recalculate();
            Content.Rearrange();
            Content.Recalculate();
            if (ScrollBar != null)
                ScrollBar.SetView(Math.Max(0, GetInnerDimensions().Height), Math.Max(0, Content.Height.Pixels));
        }
    }
}
