using System.Runtime.InteropServices;

namespace SinhalaInput.Platform.Windows.Focus;

/// <summary>
/// <see cref="IPasswordFieldDetector"/> implemented by reading the focused control's
/// <c>ES_PASSWORD</c> window style via <c>GetGUIThreadInfo</c> (design doc §10).
/// </summary>
/// <remarks>
/// This recognizes classic Win32 <c>Edit</c>-style password controls (used by most native
/// Win32/WinForms/WPF apps and many browser form fields rendered through the OS's native edit
/// control). It will not recognize password inputs implemented entirely with custom drawing or
/// a non-native accessibility tree — a UI Automation <c>IsPasswordProperty</c> fallback would
/// close that gap and is tracked as v1.1 follow-up work, matching the same trade-off already
/// made for <see cref="Caret.Win32CaretLocator"/>'s UI Automation fallback.
/// </remarks>
public sealed class Win32PasswordFieldDetector : IPasswordFieldDetector
{
    public bool IsFocusedControlPasswordField()
    {
        nint foregroundWindow = NativeMethods.GetForegroundWindow();
        if (foregroundWindow == nint.Zero)
        {
            return false;
        }

        uint threadId = NativeMethods.GetWindowThreadProcessId(foregroundWindow, out _);

        var info = default(NativeMethods.GUITHREADINFO);
        info.cbSize = (uint)Marshal.SizeOf<NativeMethods.GUITHREADINFO>();

        if (!NativeMethods.GetGUIThreadInfo(threadId, ref info) || info.hwndFocus == nint.Zero)
        {
            return false;
        }

        int style = NativeMethods.GetWindowLong(info.hwndFocus, NativeMethods.GWL_STYLE);
        return (style & NativeMethods.ES_PASSWORD) != 0;
    }
}
