using Terraria.Audio;
using Terraria.UI;

namespace Transoceanic.UI.Controls;

/// <summary>
/// 通过回调读写调用方状态的图标开关；状态的生命周期由调用方管理。
/// </summary>
public sealed class ToggleOption : UIElement
{
    private readonly Asset<Texture2D> _Icon;
    private readonly Func<bool> _GetValue;
    private readonly Action<bool> _SetValue;
    private readonly Func<string> _GetHoverText;
    private readonly Asset<Texture2D> _On = ModContent.Request<Texture2D>("Transoceanic/Assets/Textures/UI/On");
    private readonly Asset<Texture2D> _Off = ModContent.Request<Texture2D>("Transoceanic/Assets/Textures/UI/Off");

    /// <summary>
    /// 创建 32×32 开关控件，绑定图标、布尔值读写回调和悬停文本回调。
    /// </summary>
    public ToggleOption(Asset<Texture2D> icon, Func<bool> getValue, Action<bool> setValue, Func<string> getHoverText)
    {
        _Icon = icon;
        _GetValue = getValue;
        _SetValue = setValue;
        _GetHoverText = getHoverText;
        Width.Set(32, 0);
        Height.Set(32, 0);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 执行基类点击处理后反转绑定的布尔值，并播放菜单提示音。
    /// </remarks>
    public override void LeftClick(UIMouseEvent evt)
    {
        base.LeftClick(evt);
        _SetValue(!_GetValue());
        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 更新子元素；悬停时占用玩家鼠标界面输入并显示提示。
    /// </remarks>
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (IsMouseHovering)
        {
            Main.LocalPlayer.mouseInterface = true;
            Main.instance.MouseText(_GetHoverText());
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 绘制图标与开关状态；悬停时降低图标亮度。
    /// </remarks>
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        Rectangle rect = GetDimensions().ToRectangle();
        spriteBatch.Draw(_Icon.Value, rect, Color.White * (IsMouseHovering ? 0.5f : 1f));
        spriteBatch.Draw((_GetValue() ? _On : _Off).Value, rect, Color.White);
    }
}