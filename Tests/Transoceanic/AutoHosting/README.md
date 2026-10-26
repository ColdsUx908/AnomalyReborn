Run from the repository root:

```powershell
dotnet run --project Tests/Transoceanic/AutoHosting/AutoHosting.Tests.csproj
```

Uses the repository's existing `../tModLoader.targets` to locate the installed
tModLoader and FNA assemblies. Override `TmlTargets` if that file is elsewhere.
The checks exercise the actual input detector in `AutoHostingInput.cs` without
starting the game: idle input, the initial hotkey being held/released, subsequent
key presses, modifiers, mouse motion, wheel input, and all five mouse buttons,
under mouse-only, keyboard-only, combined, and unowned input states.

In-game verification:

1. Start the mod list alone, inventory alone, and the combined presentation.
   The status must appear at the upper center for the entire active session.
2. During inventory-only hosting, keyboard input (including presentation
   hotkeys and opening manual chat) must not cancel the mouse stage. Mouse
   movement, clicks, and wheel input must cancel it. During mod-list-only
   hosting, mouse input must not cancel it; keyboard input must cancel it
   throughout typing, scrolling, and the final hold. With both stages owning
   input, either conflicting input cancels the entire session. Escape always
   cancels, including when no input is owned between stages. No partial
   command should be submitted on interruption.
3. Release the interrupting key and launch again. It must not restart during
   the key event that interrupted it, but must work on the next fresh press.
4. Let both stages finish. The status must disappear, the chat input must close,
   and normal mouse/keyboard control must remain available. Existing chat
   history, including the mod list, must remain intact.
5. Alt-tab, open a menu, die, or leave the world during hosting. The session
   must end and must not resume when returning.
