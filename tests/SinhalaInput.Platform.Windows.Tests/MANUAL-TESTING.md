# SinhalaInput.Platform.Windows — what unit tests deliberately don't cover

Most of this project is P/Invoke against the real OS. The automated tests here cover every
piece of *pure* logic that can be extracted from that P/Invoke surface (input-sequence
construction, hook message/struct decoding, dispose/guard lifecycle). The following is
left to manual smoke testing on purpose, rather than faked with a test that doesn't
actually exercise the real behaviour:

- **`LowLevelKeyboardHook.Start()` actually installing a hook.** Calling
  `SetWindowsHookEx(WH_KEYBOARD_LL, ...)` installs a real system-wide hook for the process
  running the test. Doing that from a unit test run is invasive (it affects the whole
  session for the duration of the test process) and doesn't prove much beyond "the P/Invoke
  call didn't throw" — the behaviour that actually matters (keystrokes from other
  applications reaching `KeyIntercepted`, the hook surviving reasonable load, the callback's
  latency budget) can only be observed by running the built app and typing into a real
  target window.
- **`SendInputTextInjector.ReplaceTypedText` actually replacing text in a focused window.**
  `SendInput`'s effect depends entirely on which application currently has focus and how it
  handles synthetic `KEYEVENTF_UNICODE` events. The pure "which `INPUT[]` do we build"
  logic is covered by `Input/SendInputSequenceBuilderTests.cs`; whether Notepad/Word/a
  browser/VS Code actually renders the result correctly is a manual check.
- **`Win32CaretLocator` against real windows.** `GetGUIThreadInfo`, `GetCaretPos`,
  `AttachThreadInput`, and `ClientToScreen` only produce meaningful results against a real
  foreground window with a real caret; there is no meaningful fake for "the shape of a
  third-party app's caret state" that would exercise anything beyond what the (untestable)
  P/Invoke calls themselves do.
- **The "double `Start()`" guard.** The `ObjectDisposedException` and no-op `Stop()`
  lifecycle guards that don't require an installed hook *are* covered
  (`Hooking/LowLevelKeyboardHookLifecycleTests.cs`); the `InvalidOperationException` path for
  calling `Start()` a second time is not, because verifying it requires a first `Start()` to
  have actually succeeded (i.e. a real hook installed).

## Manual smoke-test checklist (re-run before every release, per design doc §9)

1. Notepad — type Latin text, confirm `IKeyboardHook`/`ITextInjector` wiring (once composed
   by the App layer) can observe keystrokes and inject replacement text atomically.
2. A WPF/WinForms app's text box.
3. Chrome address bar and a page text field (Chromium input handling differs from native
   Win32 edit controls).
4. VS Code's editor and its command palette / find box (Electron).
5. A password field — confirm no keystrokes are logged anywhere and behaviour degrades
   safely (see design doc §10; the App layer owns the actual password-field detection using
   `ICaretLocator`/window-class inspection, this project only supplies the primitives).

## Known v1 gap

`ICaretLocator` implements only the two required Win32 strategies (`GetGUIThreadInfo`, then
`GetCaretPos`/`ClientToScreen` with `AttachThreadInput`). The UI Automation
`TextPattern.GetBoundingRectangles` fallback for controls with no Win32 caret (some
Chromium/UWP surfaces) is not implemented — see the remarks on `Win32CaretLocator` for the
rationale (new dependency, materially more complex lookup, deferred to v1.1).
