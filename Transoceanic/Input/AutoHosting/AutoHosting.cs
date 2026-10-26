using Microsoft.Xna.Framework.Input;
using Terraria.Audio;
using Terraria.GameInput;

namespace Transoceanic.Input.AutoHosting;

/// <summary>
/// Owns one local automated interaction. Call <see cref="End"/> explicitly when
/// every stage finishes. Conflicting physical input or Escape cancels the interaction before
/// simulated input is applied, and invokes the supplied interruption callback.
/// </summary>
public sealed class AutoHosting
{
    private static AutoHosting _Current;
    private static readonly HashSet<Keys> _InterruptedKeys = [];
    private Action _Interrupted;
    private KeyboardState _PreviousKeyboard;
    private MouseState _PreviousMouse;
    private bool _ControlsMouse;
    private Vector2 _OriginalMouse;
    private Vector2 _MousePosition;
    private bool _ControlsChat;
    private string _PreviousChatText;
    private string _ExpectedChatText;

    public bool Active { get; private set; }
    public static bool IsHosting => _Current?.Active == true;
    public bool HasChatInput => Active && _ControlsChat && Main.drawingPlayerChat && Main.chatText == _ExpectedChatText;
    public bool HasMouseControl => Active && _ControlsMouse;

    /// <summary>Starts a session; keys already held at startup may be released normally.</summary>
    public bool TryStart(Action interrupted)
    {
        if (Active)
            return ValidateInput();
        if (IsHosting || Main.dedServ || Main.gameMenu || !Main.hasFocus ||
            Main.LocalPlayer.dead || !Main.LocalPlayer.active)
            return false;
        KeyboardState keyboard = Keyboard.GetState();
        // A key which interrupted the previous run must not restart it again
        // through ProcessTriggers in that very same input update.
        _InterruptedKeys.RemoveWhere(key => keyboard.IsKeyUp(key));
        if (_InterruptedKeys.Count > 0)
            return false;
        _PreviousKeyboard = keyboard;
        _PreviousMouse = Mouse.GetState();
        _Interrupted = interrupted;
        _Current = this;
        Active = true;
        return true;
    }

    /// <summary>Polls hardware state, never the mouse/keyboard fields being simulated.</summary>
    public bool ValidateInput()
    {
        if (!Active)
            return false;
        MouseState mouse = Mouse.GetState();
        KeyboardState keyboard = Keyboard.GetState();
        bool changed = AutoHostingInput.HasUserInput(_PreviousMouse, mouse, _PreviousKeyboard, keyboard,
            _ControlsMouse, _ControlsChat);
        _PreviousMouse = mouse;
        _PreviousKeyboard = keyboard;
        if (changed || !Main.hasFocus || Main.gameMenu || Main.LocalPlayer.dead || !Main.LocalPlayer.active ||
            Main.editSign || Main.editChest || Main.ingameOptionsWindow || Main.mapFullscreen ||
            (_ControlsChat && !HasChatInput))
        {
            Interrupt();
            return false;
        }
        return true;
    }

    public bool TryBeginChatInput()
    {
        if (!ValidateInput() || Main.drawingPlayerChat || Main.CurrentInputTextTakerOverride != null)
            return false;
        _PreviousChatText = Main.chatText;
        Main.OpenPlayerChat();
        if (!Main.drawingPlayerChat)
            return false;
        _ControlsChat = true;
        Main.chatText = _ExpectedChatText = "";
        SoundEngine.PlaySound(SoundID.MenuOpen);
        return true;
    }

    /// <summary>Changes the real local chat input without submitting a command.</summary>
    public bool TrySetChatInput(string text, bool playTypingSound = true)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (!ValidateInput() || !HasChatInput)
            return false;
        bool changed = Main.chatText != text;
        Main.chatText = _ExpectedChatText = text;
        if (changed && playTypingSound)
            SoundEngine.PlaySound(SoundID.MenuTick);
        return true;
    }

    public void EndChatInput()
    {
        if (!_ControlsChat)
            return;
        _ControlsChat = false;
        if (Main.chatText != _ExpectedChatText)
            return;
        Main.chatText = _PreviousChatText;
        if (Main.drawingPlayerChat)
            Main.ClosePlayerChat();
    }

    public bool TryBeginMouseControl()
    {
        if (!ValidateInput())
            return false;
        if (!_ControlsMouse)
        {
            _OriginalMouse = _MousePosition = new Vector2(PlayerInput.MouseX, PlayerInput.MouseY);
            _ControlsMouse = true;
        }
        ApplyMouse();
        return true;
    }

    /// <summary>Interpolates and applies a position in physical screen coordinates.</summary>
    public void MoveMouse(Vector2 from, Vector2 to, float progress)
    {
        if (!ValidateInput() || !_ControlsMouse)
            return;
        _MousePosition = Vector2.Lerp(from, to, Math.Clamp(progress, 0f, 1f));
        ApplyMouse();
    }

    public void EndMouseControl()
    {
        if (!_ControlsMouse)
            return;
        _ControlsMouse = false;
        SetMousePosition(_OriginalMouse);
    }

    private static void SetMousePosition(Vector2 position)
    {
        PlayerInput.MouseX = Main.mouseX = (int)Math.Clamp(position.X, 0, Math.Max(0, PlayerInput.RealScreenWidth - 1));
        PlayerInput.MouseY = Main.mouseY = (int)Math.Clamp(position.Y, 0, Math.Max(0, PlayerInput.RealScreenHeight - 1));
        PlayerInput.CacheMousePositionForZoom();
    }

    private void ApplyMouse()
    {
        if (!Active || !_ControlsMouse || !Main.hasFocus)
            return;
        SetMousePosition(_MousePosition);
        // Never move the OS cursor. Suppress synthetic slot interactions only
        // after hardware input has been checked for an immediate interruption.
        ButtonState released = ButtonState.Released;
        PlayerInput.MouseInfoOld = PlayerInput.MouseInfo;
        PlayerInput.MouseInfo = new MouseState(PlayerInput.MouseX, PlayerInput.MouseY,
            PlayerInput.MouseInfo.ScrollWheelValue, released, released, released, released, released);
        PlayerInput.MouseKeys.Clear();
        PlayerInput.Triggers.Current.MouseLeft = false;
        PlayerInput.Triggers.Current.MouseRight = false;
        PlayerInput.Triggers.Current.MouseMiddle = false;
        PlayerInput.Triggers.Current.MouseXButton1 = false;
        PlayerInput.Triggers.Current.MouseXButton2 = false;
        Main.mouseLeft = Main.mouseRight = false;
        Main.mouseMiddle = Main.mouseXButton1 = Main.mouseXButton2 = false;
        Main.mouseLeftRelease = Main.mouseRightRelease = true;
        Main.LocalPlayer.controlUseItem = Main.LocalPlayer.controlUseTile = false;
    }

    /// <summary>Explicitly completes hosting, restores the cursor, and closes owned chat input.</summary>
    public void End()
    {
        if (!Active)
            return;
        Active = false;
        if (ReferenceEquals(_Current, this))
            _Current = null;
        EndChatInput();
        EndMouseControl();
        _Interrupted = null;
    }

    private void Interrupt()
    {
        if (!Active)
            return;
        Action callback = _Interrupted;
        _InterruptedKeys.Clear();
        _InterruptedKeys.UnionWith(Keyboard.GetState().GetPressedKeys());
        // A physical interruption must keep the user's actual cursor position,
        // rather than snapping it back to the start of the presentation.
        if (_ControlsMouse)
        {
            _ControlsMouse = false;
            MouseState mouse = Mouse.GetState();
            SetMousePosition(new Vector2(mouse.X, mouse.Y));
        }
        End();
        callback?.Invoke();
    }

    internal static void CheckCurrentInput()
    {
        if (_Current != null)
            _Current.ValidateInput();
        else if (_InterruptedKeys.Count > 0)
        {
            KeyboardState keyboard = Keyboard.GetState();
            _InterruptedKeys.RemoveWhere(key => keyboard.IsKeyUp(key));
        }
    }
    internal static void ApplyCurrentMouse() => _Current?.ApplyMouse();
    internal static void InterruptCurrent() => _Current?.Interrupt();
}
