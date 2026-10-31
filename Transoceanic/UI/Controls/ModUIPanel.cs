using Terraria.GameContent.UI.Elements;
using Terraria.UI;
namespace Transoceanic.UI.Controls;

/// <summary>
/// 支持左键拖动、右下角缩放和布局事件的 UI 面板。
/// </summary>
public class ModUIPanel : UIPanel
{
    /// <summary>
    /// 是否允许从面板右下角缩放。
    /// </summary>
    protected bool Resizeable;
    /// <summary>
    /// 面板当前是否处于左键缩放状态。
    /// </summary>
    protected bool Resizing;
    private readonly int _MinResizeWidth;
    private readonly int _MinResizeHeight;

    /// <summary>
    /// 是否允许左键拖动面板。
    /// </summary>
    protected bool Draggable;
    /// <summary>
    /// 面板当前是否处于左键拖动状态。
    /// </summary>
    protected bool Dragging;

    /// <summary>
    /// 拖动或缩放开始时鼠标相对面板锚点的 UI 像素偏移。
    /// </summary>
    internal Vector2 Offset;

    /// <summary>
    /// 当框架或调用方触发 <c>OnRecalculate</c> 时通知已订阅者；订阅方可据此响应相应生命周期或状态变化。
    /// </summary>
    protected event ElementEvent OnRecalculate;
    /// <summary>
    /// 当框架或调用方触发 <c>OnDrag</c> 时通知已订阅者；订阅方可据此响应相应生命周期或状态变化。
    /// </summary>
    protected event ElementEvent OnDrag;
    /// <summary>
    /// 当框架或调用方触发 <c>OnResize</c> 时通知已订阅者；订阅方可据此响应相应生命周期或状态变化。
    /// </summary>
    protected event ElementEvent OnResize;

    /// <summary>
    /// 面板右下角缩放标记的纹理资源。
    /// </summary>
    internal Asset<Texture2D> ResizeIndicator;

    /// <summary>
    /// 创建可选拖动和缩放的面板，绑定鼠标事件，保存最小宽高（UI 像素）。
    /// </summary>
    public ModUIPanel(bool draggable = true, bool resizeable = false, int minResizeWidth = 160, int minResizeHeight = 90)
    {
        Draggable = draggable;
        Resizeable = resizeable;
        OnLeftMouseDown += DragStart;
        OnLeftMouseUp += DragEnd;
        _MinResizeWidth = minResizeWidth;
        _MinResizeHeight = minResizeHeight;
        ResizeIndicator = ModContent.Request<Texture2D>("Transoceanic/Assets/Textures/UI/Resize");
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 重算面板布局后触发 OnRecalculate。
    /// </remarks>
    public override void Recalculate()
    {
        base.Recalculate();
        OnRecalculate?.Invoke(this);
    }

    /// <summary>
    /// 面板右下角用于命中缩放操作的屏幕像素矩形。
    /// </summary>
    public Rectangle ResizeRectangle
    {
        get
        {
            CalculatedStyle innerDimensions = GetInnerDimensions();
            return new Rectangle((int)(innerDimensions.X + innerDimensions.Width - 12), (int)(innerDimensions.Y + innerDimensions.Height - 12), 12 + (int)PaddingRight, 12 + (int)PaddingBottom);
        }
    }

    // 可拖动/调整大小界面
    private void DragStart(UIMouseEvent evt, UIElement listeningElement)
    {
        //CalculatedStyle innerDimensions = GetInnerDimensions();
        if (evt.Target != this)
            return;
        if (Resizeable && ResizeRectangle.Contains(evt.MousePosition.ToPoint()))
        {
            CalculatedStyle dimensions = GetOuterDimensions();
            Offset = evt.MousePosition - new Vector2(dimensions.X + dimensions.Width, dimensions.Y + dimensions.Height);
            Resizing = true;
        }
        else if (Draggable)
        {
            Offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
            Dragging = true;
        }
    }

    // 可拖动/调整大小界面
    private void DragEnd(UIMouseEvent evt, UIElement listeningElement)
    {
        Dragging = false;
        Resizing = false;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 绘制面板；可缩放时绘制右下角标记，并在悬停或缩放时更新鼠标外观。
    /// </remarks>
    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        if (Resizeable)
        {
            CalculatedStyle dimensions = GetInnerDimensions();
            Vector2 position = dimensions.ToRectangle().BottomRight() - new Vector2(4f);
            Texture2D texture = ResizeIndicator.Value;

            spriteBatch.Draw(texture, position, BorderColor);

            if (ResizeRectangle.Contains(UserInterface.ActiveInstance.MousePosition.ToPoint()) || Resizing)
            {
                Main.cursorOverride = CursorOverrideID.GamepadDefaultCursor;
                Main.cursorColor = Color.SkyBlue;
            }
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 根据鼠标更新拖动或缩放，限制最小尺寸，重算布局并触发对应事件。
    /// </remarks>
    public override void Update(GameTime gameTime)
    {
        if (!Main.mouseLeft) { Dragging = false; Resizing = false; }
        base.Update(gameTime);

        if (IsMouseHovering)
        {
            Main.LocalPlayer.mouseInterface = true;
        }

        CalculatedStyle dimensions = GetOuterDimensions();

        if (Dragging)
        {
            Left.Set(UserInterface.ActiveInstance.MousePosition.X - Offset.X, 0f);
            Top.Set(UserInterface.ActiveInstance.MousePosition.Y - Offset.Y, 0f);
            Recalculate();
            OnDrag?.Invoke(this);
        }
        if (Resizing)
        {
            Width.Pixels = UserInterface.ActiveInstance.MousePosition.X - dimensions.X - Offset.X;
            Height.Pixels = UserInterface.ActiveInstance.MousePosition.Y - dimensions.Y - Offset.Y;
            // 限制
            if (Width.Pixels <= _MinResizeWidth)
            {
                Width.Pixels = _MinResizeWidth;
            }
            if (Height.Pixels <= _MinResizeHeight)
            {
                Height.Pixels = _MinResizeHeight;
            }
            OnResize?.Invoke(this);
            Recalculate();
        }
    }
}

