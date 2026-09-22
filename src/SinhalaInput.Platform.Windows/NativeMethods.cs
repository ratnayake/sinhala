namespace SinhalaInput.Platform.Windows;

/// <summary>
/// Centralised, source-generated P/Invoke signatures for every Win32 API this assembly calls
/// (<c>user32.dll</c> hooking/input/caret functions). Keeping every native declaration in one
/// partial class makes the unsafe/native surface area easy to audit (design doc §8.1).
/// </summary>
/// <remarks>
/// v1 placeholder: add <c>[LibraryImport]</c> declarations here for
/// <c>SetWindowsHookEx</c>/<c>UnhookWindowsHookEx</c>/<c>CallNextHookEx</c> (keyboard hook),
/// <c>SendInput</c> and its <c>INPUT</c>/<c>KEYBDINPUT</c> structs (text injection), and
/// <c>GetGUIThreadInfo</c>/<c>GetCaretPos</c>/<c>ClientToScreen</c> (caret location), per
/// current .NET interop guidance (source-generated P/Invoke, not the legacy
/// <c>[DllImport]</c>).
/// </remarks>
internal static partial class NativeMethods
{
}
