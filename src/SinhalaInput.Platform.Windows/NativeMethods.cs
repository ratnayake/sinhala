using System.Runtime.InteropServices;

[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32)]

namespace SinhalaInput.Platform.Windows;

/// <summary>
/// Centralised, source-generated P/Invoke signatures for every Win32 API this assembly calls
/// (<c>user32.dll</c>/<c>kernel32.dll</c> hooking/input/caret functions). Keeping every native
/// declaration in one partial class makes the unsafe/native surface area easy to audit
/// (design doc §8.1). Only APIs actually consumed elsewhere in this assembly are declared here.
/// </summary>
internal static partial class NativeMethods
{
    // -- MSIX package identity (Packaging/PackageIdentity.cs) ---------------------------------

    internal const int APPMODEL_ERROR_NO_PACKAGE = 15700;

    [LibraryImport("kernel32.dll")]
    internal static partial int GetCurrentPackageFullName(ref uint packageFullNameLength, nint packageFullName);

    // -- WH_KEYBOARD_LL hook (Hooking/LowLevelKeyboardHook.cs) --------------------------------

    internal const int WH_KEYBOARD_LL = 13;
    internal const int HC_ACTION = 0;

    internal const int WM_KEYDOWN = 0x0100;
    internal const int WM_KEYUP = 0x0101;
    internal const int WM_SYSKEYDOWN = 0x0104;
    internal const int WM_SYSKEYUP = 0x0105;

    /// <summary>Matches the native <c>HOOKPROC</c> signature for <c>WH_KEYBOARD_LL</c>.</summary>
    internal delegate nint LowLevelKeyboardProc(int nCode, nint wParam, nint lParam);

    [StructLayout(LayoutKind.Sequential)]
    internal struct KBDLLHOOKSTRUCT
    {
        internal uint vkCode;
        internal uint scanCode;
        internal uint flags;
        internal uint time;
        internal nint dwExtraInfo;
    }

    [LibraryImport("user32.dll", EntryPoint = "SetWindowsHookExW", SetLastError = true)]
    internal static partial nint SetWindowsHookEx(int idHook, nint lpfn, nint hMod, uint dwThreadId);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool UnhookWindowsHookEx(nint hhk);

    [LibraryImport("user32.dll")]
    internal static partial nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

    // -- SendInput text injection (Input/SendInputTextInjector.cs) ---------------------------

    internal const ushort VK_BACK = 0x08;

    internal const uint INPUT_KEYBOARD = 1;
    internal const uint KEYEVENTF_KEYUP = 0x0002;
    internal const uint KEYEVENTF_UNICODE = 0x0004;
    internal const uint KEYEVENTF_SCANCODE = 0x0008;

    [StructLayout(LayoutKind.Sequential)]
    internal struct KEYBDINPUT
    {
        internal ushort wVk;
        internal ushort wScan;
        internal uint dwFlags;
        internal uint time;
        internal nint dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MOUSEINPUT
    {
        internal int dx;
        internal int dy;
        internal uint mouseData;
        internal uint dwFlags;
        internal uint time;
        internal nint dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct HARDWAREINPUT
    {
        internal uint uMsg;
        internal ushort wParamL;
        internal ushort wParamH;
    }

    /// <summary>
    /// The native <c>INPUT</c> union. <see cref="MOUSEINPUT"/> and <see cref="HARDWAREINPUT"/>
    /// are never populated by this assembly (only keyboard events are injected) but must stay
    /// declared so the union's automatic layout matches the OS's actual <c>INPUT</c> size —
    /// <c>SendInput</c> rejects the whole call if <c>cbSize</c> does not match that native size.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct InputUnion
    {
        [FieldOffset(0)]
        internal MOUSEINPUT mi;

        [FieldOffset(0)]
        internal KEYBDINPUT ki;

        [FieldOffset(0)]
        internal HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct INPUT
    {
        internal uint type;
        internal InputUnion u;
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    internal static partial uint SendInput(uint nInputs, [In] INPUT[] pInputs, int cbSize);

    // -- Caret location (Caret/Win32CaretLocator.cs) ------------------------------------------

    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        internal int Left;
        internal int Top;
        internal int Right;
        internal int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT
    {
        internal int X;
        internal int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct GUITHREADINFO
    {
        internal uint cbSize;
        internal uint flags;
        internal nint hwndActive;
        internal nint hwndFocus;
        internal nint hwndCapture;
        internal nint hwndMenuOwner;
        internal nint hwndMoveSize;
        internal nint hwndCaret;
        internal RECT rcCaret;
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO lpgui);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetCaretPos(out POINT lpPoint);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool ClientToScreen(nint hWnd, ref POINT lpPoint);

    [LibraryImport("user32.dll")]
    internal static partial nint GetForegroundWindow();

    [LibraryImport("user32.dll")]
    internal static partial uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);

    /// <summary>
    /// Last-resort caret-location fallback: always succeeds regardless of which application or
    /// control has focus, so it is used when none of the other strategies can locate anything.
    /// </summary>
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetCursorPos(out POINT lpPoint);

    // -- Password-field detection (Focus/Win32PasswordFieldDetector.cs) -----------------------

    internal const int GWL_STYLE = -16;
    internal const int ES_PASSWORD = 0x0020;

    /// <summary>
    /// <c>GWL_STYLE</c> is always a 32-bit <c>LONG</c> even on 64-bit Windows (unlike pointer-sized
    /// indices such as <c>GWLP_WNDPROC</c>), so the plain, non-pointer-width <c>GetWindowLong</c>
    /// entry point is correct here on both x86 and x64.
    /// </summary>
    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)]
    internal static partial int GetWindowLong(nint hWnd, int nIndex);

    /// <summary>
    /// Not requested explicitly by the design doc's API list, but required for the
    /// <c>GetCaretPos</c> fallback to work at all: <c>GetCaretPos</c> only returns a caret
    /// belonging to a window owned by the *calling* thread's message queue, so observing the
    /// caret of the (foreground) window of another process requires temporarily attaching
    /// input queues via <c>AttachThreadInput</c>. <c>GetGUIThreadInfo</c> (the primary
    /// strategy) needs no such attachment, which is why it is tried first.
    /// </summary>
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool AttachThreadInput(uint idAttach, uint idAttachTo, [MarshalAs(UnmanagedType.Bool)] bool fAttach);

    [LibraryImport("kernel32.dll")]
    internal static partial uint GetCurrentThreadId();

    [LibraryImport("user32.dll", EntryPoint = "GetClassNameW", SetLastError = true)]
    internal static unsafe partial int GetClassName(nint hWnd, char* lpClassName, int nMaxCount);

    internal const uint OBJID_CARET = 0xFFFFFFF8;

    /// <summary>
    /// MSAA's <c>OBJID_CARET</c> object. Chromium (Chrome/Edge/Electron) exposes its real text
    /// caret here (for screen magnifiers) even though it never creates a Win32 caret.
    /// </summary>
    [LibraryImport("oleacc.dll")]
    internal static partial int AccessibleObjectFromWindow(nint hwnd, uint dwId, in Guid riid, out nint ppvObject);

    internal static readonly Guid IID_IAccessible = new("618736E0-3C3D-11CF-810C-00AA00389B71");

    // -- DPI + popup placement (Dpi/*, Windowing/PopupPlacement.cs) ----------------------------

    internal static readonly nint DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4;

    [LibraryImport("user32.dll")]
    internal static partial nint SetThreadDpiAwarenessContext(nint dpiContext);

    internal const uint MONITOR_DEFAULTTONEAREST = 2;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MONITORINFO
    {
        internal uint cbSize;
        internal RECT rcMonitor;
        internal RECT rcWork;
        internal uint dwFlags;
    }

    [LibraryImport("user32.dll")]
    internal static partial nint MonitorFromPoint(POINT pt, uint dwFlags);

    [LibraryImport("user32.dll", EntryPoint = "GetMonitorInfoW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetMonitorInfo(nint hMonitor, ref MONITORINFO lpmi);

    internal const int MDT_EFFECTIVE_DPI = 0;

    [LibraryImport("shcore.dll")]
    internal static partial int GetDpiForMonitor(nint hmonitor, int dpiType, out uint dpiX, out uint dpiY);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetWindowRect(nint hWnd, out RECT lpRect);

    internal static readonly nint HWND_TOPMOST = -1;
    internal const uint SWP_NOSIZE = 0x0001;
    internal const uint SWP_NOACTIVATE = 0x0010;

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
}
