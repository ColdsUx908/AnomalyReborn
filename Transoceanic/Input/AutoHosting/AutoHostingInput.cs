using Microsoft.Xna.Framework.Input;

namespace Transoceanic.Input.AutoHosting;

/// <summary>
/// 比较硬件输入快照，判断是否中断当前输入托管。
/// </summary>
internal static class AutoHostingInput
{
    /// <summary>
    /// 比较前后硬件输入，判断按键、鼠标按钮或托管范围内的输入变化是否应中断托管。
    /// </summary>
    internal static bool HasUserInput(MouseState previousMouse, MouseState mouse, KeyboardState previousKeyboard, KeyboardState keyboard,
        bool controlsMouse, bool controlsKeyboard)
    {
        // Escape is always available, even between stages with no owned input.
        if (keyboard.IsKeyDown(Keys.Escape))
            return true;
        // FNA's MouseState equality does not include the extra mouse buttons.
        if (controlsMouse && (mouse.X != previousMouse.X || mouse.Y != previousMouse.Y ||
            mouse.ScrollWheelValue != previousMouse.ScrollWheelValue ||
            mouse.LeftButton != previousMouse.LeftButton || mouse.RightButton != previousMouse.RightButton ||
            mouse.MiddleButton != previousMouse.MiddleButton ||
            mouse.XButton1 != previousMouse.XButton1 || mouse.XButton2 != previousMouse.XButton2))
            return true;
        if (!controlsKeyboard)
            return false;
        foreach (Keys key in keyboard.GetPressedKeys())
            if (previousKeyboard.IsKeyUp(key))
                return true;
        return false;
    }
}
