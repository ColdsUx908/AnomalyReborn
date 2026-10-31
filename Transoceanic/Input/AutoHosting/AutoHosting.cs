using Microsoft.Xna.Framework.Input;
using Terraria.Audio;
using Terraria.GameInput;

namespace Transoceanic.Input.AutoHosting;

/// <summary>
/// 管理一次本地输入托管会话，全部步骤完成后须显式调用 <see cref="End"/>。检测到冲突的真实输入或 Escape 时，在应用模拟输入前取消托管并调用中断回调。
/// </summary>
/// <remarks>仅托管本地输入，不提供服务器权限，也不会替代实体或玩家数据的网络同步。</remarks>
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

    /// <summary>
    /// 当前输入托管会话是否有效，由启动、结束与中断流程维护。
    /// </summary>
    public bool Active { get; private set; }
    /// <summary>
    /// 当前是否存在有效的输入托管会话。
    /// </summary>
    public static bool IsHosting => _Current?.Active == true;
    /// <summary>
    /// 托管有效且聊天框仍打开、文本仍为托管写入值时返回 true。
    /// </summary>
    public bool HasChatInput => Active && _ControlsChat && Main.drawingPlayerChat && Main.chatText == _ExpectedChatText;
    /// <summary>
    /// 会话有效且已接管游戏内鼠标时返回 true。
    /// </summary>
    public bool HasMouseControl => Active && _ControlsMouse;

    /// <summary>
    /// 尝试启动托管会话；启动前已经按住的按键可以正常松开，不因此中断托管。
    /// </summary>
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

    /// <summary>
    /// 检查真实硬件输入与游戏状态；无有效会话或托管被中断时返回 false，不读取模拟输入字段作为硬件状态。
    /// </summary>
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

    /// <summary>
    /// 本地托管有效且聊天框未被占用时打开聊天输入，保存原文本；成功返回 true。
    /// </summary>
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

    /// <summary>
    /// 尝试修改本地聊天框文本，不提交消息或命令。
    /// </summary>
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

    /// <summary>
    /// 释放聊天控制；仅当文本仍是托管写入值时恢复原文本并关闭聊天框。
    /// </summary>
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

    /// <summary>
    /// 验证输入后接管游戏内鼠标并保存起始位置；成功返回 true，不移动系统光标。
    /// </summary>
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

    /// <summary>
    /// 在托管有效且已接管鼠标时，按限制到 0 至 1 的进度插值并应用游戏内鼠标的屏幕像素坐标，不移动系统光标。
    /// </summary>
    public void MoveMouse(Vector2 from, Vector2 to, float progress)
    {
        if (!ValidateInput() || !_ControlsMouse)
            return;
        _MousePosition = Vector2.Lerp(from, to, Math.Clamp(progress, 0f, 1f));
        ApplyMouse();
    }

    /// <summary>
    /// 释放鼠标控制并恢复开始托管时的游戏内鼠标位置；未接管时不执行操作。
    /// </summary>
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

    /// <summary>
    /// 显式结束托管，恢复接管前的游戏内鼠标位置，并释放托管占用的聊天输入。
    /// </summary>
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

    /// <summary>
    /// 检查当前托管是否应被用户输入中断；无托管时清除已经松开的中断按键记录。
    /// </summary>
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
    /// <summary>
    /// 应用当前托管的游戏内鼠标位置并抑制合成点击，不移动系统光标。
    /// </summary>
    internal static void ApplyCurrentMouse() => _Current?.ApplyMouse();
    /// <summary>
    /// 中断当前托管，保留用户实际鼠标位置，在释放控制后调用中断回调。
    /// </summary>
    internal static void InterruptCurrent() => _Current?.Interrupt();
}
