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
  `AttachThreadInput`, `ClientToScreen`, UI Automation's `AutomationElement.FocusedElement`/
  `TextPattern`, and `GetCursorPos` only produce meaningful results against a real foreground
  window with real OS-level focus; there is no meaningful fake for "the shape of a third-party
  app's caret state" that would exercise anything beyond what the (untestable) P/Invoke/UIA
  calls themselves do. This was verified manually on a real Windows 11 desktop while doing the
  Edge fix (see "Findings from real-desktop verification" below); re-run that check by hand
  before every release rather than relying on the (necessarily absent) automated coverage.
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

## Caret location: what's automatic now vs. still manual

`Win32CaretLocator` chains four strategies: `GetGUIThreadInfo`, then `GetCaretPos`/
`ClientToScreen` with `AttachThreadInput`, then UI Automation's `TextPattern.GetBoundingRectangles`
(falling back to the focused element's own bounding rectangle), then `GetCursorPos` as a
last-resort anchor near the mouse. This closes the gap that used to leave the candidate popup
with no anchor (and therefore invisible, wherever it last was) for apps that expose no Win32
caret at all — Chromium (Edge/Chrome/CEF) being the most common case, but manual testing while
fixing this also found that the current, MSIX-packaged Windows 11 Notepad app does not reliably
expose a caret via `GetGUIThreadInfo` either (see findings below) — the popup now still gets a
usable anchor position for both.

Still left to manual smoke testing, because it depends on live OS UI state with no meaningful
fake: whether each of the four strategies actually fires and returns a *sane* position for a
given real target app. In particular:

- `GetCaretPos`'s cross-process `AttachThreadInput` result cannot be trusted purely by its
  boolean return value — it was observed to return `true` with a plausible-but-wrong value, and
  in another case `true` with an exact `(0, 0)` default, for apps that have no real Win32 caret
  at all. It is still tried before the more expensive UI Automation strategy (matching the design
  doc's ordering), so a future contributor changing this file should be aware `GetCaretPos`
  "succeeding" does not by itself prove the position is meaningful.
- Whether UI Automation's `TextPattern` is actually implemented (vs. just the element's raw
  `BoundingRectangle` fallback) varies by app/control and cannot be enumerated exhaustively;
  spot-check any newly-important target application.

### Findings from real-desktop verification (done for the Edge popup-invisible fix)

Verified interactively against real, focused windows on a Windows 11 desktop (using a throwaway
xUnit harness invoking `Win32CaretLocator`'s private strategy methods via reflection, since
`SetForegroundWindow` from a background process is silently denied by Windows' foreground-lock
rules unless the calling thread first attaches input to the actual current foreground thread —
the same `AttachThreadInput` trick `TryGetFromCaretPos` already uses for a different reason):

- A genuine, focused, same-thread classic Win32 `EDIT` control: `GetGUIThreadInfo` and
  `GetCaretPos` both succeed with the exact expected position — confirms the two original
  strategies are correctly implemented when a real Win32 caret exists.
- A real Microsoft Edge window, focused on an actual `<input>` element in webpage content
  (not the address bar): `GetGUIThreadInfo` fails (no caret found, as expected — Chromium
  renders its own text), `GetCaretPos` returns `true` with `(0, 0)` (a meaningless default, not
  a real position), and the new UI Automation strategy succeeds with a plausible on-screen
  position matching the input field's actual location. This is the direct fix for the reported
  bug.
- The real, currently-open Windows 11 Notepad window on this desktop: `GetGUIThreadInfo` also
  fails here. `GetCaretPos` and the UI Automation strategy both return a real (not default)
  screen position on the user's actual secondary monitor (negative Y — the monitor is stacked
  above the primary), so the overall chained lookup succeeds either way, but this shows modern
  Notepad is not the clean "legacy caret always works" baseline it once was.
- `GetCursorPos` (the final fallback) always succeeds trivially, as expected.

Not verified: the WPF `CandidateWindow` popup's actual on-screen rendering position when
anchored by each strategy — no screenshot/vision tooling was available for this verification, so
only the underlying `ICaretLocator` data was confirmed, not the popup's final pixel placement.
