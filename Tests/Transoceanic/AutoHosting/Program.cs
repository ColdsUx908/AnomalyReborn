using Transoceanic.Input.AutoHosting;
using System;
using Microsoft.Xna.Framework.Input;

int checks = 0;
void Check(bool condition, string message)
{
    if (!condition)
        throw new Exception(message);
    checks++;
}

MouseState MouseAt(int x = 100, int y = 200, int wheel = 0,
    ButtonState left = ButtonState.Released, ButtonState right = ButtonState.Released,
    ButtonState middle = ButtonState.Released, ButtonState x1 = ButtonState.Released, ButtonState x2 = ButtonState.Released)
    => new(x, y, wheel, left, middle, right, x1, x2);

var mouse = MouseAt();
var released = new KeyboardState();
var launching = new KeyboardState(Keys.L);
bool Input(MouseState nextMouse, KeyboardState previous, KeyboardState next)
    => AutoHostingInput.HasUserInput(mouse, nextMouse, previous, next, true, true);

Check(!Input(mouse, released, released), "Idle hardware interrupted hosting");
Check(!Input(mouse, launching, launching), "Holding the launch hotkey interrupted hosting");
Check(!Input(mouse, launching, released), "Releasing the launch hotkey interrupted hosting");
Check(Input(mouse, released, launching), "Pressing the launch key again must interrupt");
Check(Input(mouse, launching, new KeyboardState(Keys.L, Keys.A)), "An additional key must interrupt");
Check(Input(mouse, released, new KeyboardState(Keys.LeftShift)), "Modifiers must interrupt");
Check(Input(mouse, released, new KeyboardState(Keys.Escape)), "Escape must interrupt");
Check(Input(mouse, released, new KeyboardState(Keys.Enter)), "Enter must interrupt before command submission");
Check(Input(mouse, released, new KeyboardState(Keys.Up)), "Up must interrupt before native scrolling");
Check(Input(mouse, released, new KeyboardState(Keys.Back)), "Backspace must interrupt before editing");
Check(Input(MouseAt(x: 101), released, released), "Horizontal mouse movement must interrupt");
Check(Input(MouseAt(y: 201), released, released), "Vertical mouse movement must interrupt");
Check(Input(MouseAt(wheel: 120), released, released), "Mouse wheel must interrupt");
Check(Input(MouseAt(left: ButtonState.Pressed), released, released), "Left click must interrupt");
Check(Input(MouseAt(right: ButtonState.Pressed), released, released), "Right click must interrupt");
Check(Input(MouseAt(middle: ButtonState.Pressed), released, released), "Middle click must interrupt");
Check(Input(MouseAt(x1: ButtonState.Pressed), released, released), "Mouse button 4 must interrupt");
Check(Input(MouseAt(x2: ButtonState.Pressed), released, released), "Mouse button 5 must interrupt");
// The detector receives only physical snapshots: applying another simulated
// UI cursor position or chat string between these polls cannot cancel hosting.
for (int frame = 0; frame < 144; frame++)
    Check(!Input(mouse, released, released), "Unchanged hardware must remain hosted");

foreach (bool controlsMouse in new[] { false, true })
foreach (bool controlsKeyboard in new[] { false, true })
{
    bool Conflict(MouseState nextMouse, KeyboardState previous, KeyboardState next)
        => AutoHostingInput.HasUserInput(mouse, nextMouse, previous, next, controlsMouse, controlsKeyboard);
    Check(!Conflict(mouse, released, released), "Idle input must not conflict with any ownership combination");
    Check(Conflict(mouse, released, new KeyboardState(Keys.Escape)), "Escape must interrupt every ownership combination");
    Check(Conflict(mouse, new KeyboardState(Keys.Escape), new KeyboardState(Keys.Escape)), "Held Escape must still force interruption");
    Check(Conflict(mouse, released, new KeyboardState(Keys.W)) == controlsKeyboard, "Keyboard input must only conflict with keyboard ownership");
    Check(Conflict(mouse, released, new KeyboardState(Keys.P)) == controlsKeyboard, "Presentation hotkey must not cancel mouse-only hosting");
    Check(Conflict(mouse, released, new KeyboardState(Keys.Enter)) == controlsKeyboard, "Opening manual chat must not cancel mouse-only hosting");
    Check(Conflict(mouse, released, new KeyboardState(Keys.Up)) == controlsKeyboard, "Chat scrolling must keep keyboard ownership");
    Check(Conflict(mouse, released, new KeyboardState(Keys.LeftShift)) == controlsKeyboard, "Modifier input must follow keyboard ownership");
    Check(!Conflict(mouse, launching, released), "Releasing the launch key must not conflict");
    foreach (MouseState moved in new[] { MouseAt(x: 101), MouseAt(y: 201), MouseAt(wheel: 120),
        MouseAt(left: ButtonState.Pressed), MouseAt(right: ButtonState.Pressed), MouseAt(middle: ButtonState.Pressed),
        MouseAt(x1: ButtonState.Pressed), MouseAt(x2: ButtonState.Pressed) })
        Check(Conflict(moved, released, released) == controlsMouse, "Mouse input must only conflict with mouse ownership");
}
Console.WriteLine($"Passed {checks} input ownership and interruption checks.");
