using Terraria.Audio;
using Terraria.UI;

namespace Transoceanic.UI.Controls;

/// <summary>An icon toggle whose state belongs to its consumer, not the UI lifetime.</summary>
public sealed class ToggleOption : UIElement
{
    private readonly Asset<Texture2D> _Icon;
    private readonly Func<bool> _GetValue;
    private readonly Action<bool> _SetValue;
    private readonly Func<string> _GetHoverText;
    private readonly Asset<Texture2D> _On = ModContent.Request<Texture2D>("Transoceanic/Assets/Textures/UI/On");
    private readonly Asset<Texture2D> _Off = ModContent.Request<Texture2D>("Transoceanic/Assets/Textures/UI/Off");

    public ToggleOption(Asset<Texture2D> icon, Func<bool> getValue, Action<bool> setValue, Func<string> getHoverText)
    {
        _Icon = icon;
        _GetValue = getValue;
        _SetValue = setValue;
        _GetHoverText = getHoverText;
        Width.Set(32, 0);
        Height.Set(32, 0);
    }

    public override void LeftClick(UIMouseEvent evt)
    {
        base.LeftClick(evt);
        _SetValue(!_GetValue());
        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (IsMouseHovering)
        {
            Main.LocalPlayer.mouseInterface = true;
            Main.instance.MouseText(_GetHoverText());
        }
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        Rectangle rect = GetDimensions().ToRectangle();
        spriteBatch.Draw(_Icon.Value, rect, Color.White * (IsMouseHovering ? 0.5f : 1f));
        spriteBatch.Draw((_GetValue() ? _On : _Off).Value, rect, Color.White);
    }
}