namespace SinhalaInput.Platform.Windows.Focus;

/// <summary>
/// Best-effort detection of whether the currently focused control is a password/sensitive
/// entry field, so the typing session can disengage buffering entirely for it (design doc §10
/// "Cross-cutting concerns" — password fields).
/// </summary>
public interface IPasswordFieldDetector
{
    /// <summary>
    /// Returns <see langword="true"/> if the control currently holding keyboard focus appears
    /// to be a password field. This is inherently best-effort: it can only recognize controls
    /// that expose the classic Win32 <c>ES_PASSWORD</c> style, so custom-drawn or non-Win32
    /// (e.g. some Chromium/UWP) password inputs may not be detected.
    /// </summary>
    bool IsFocusedControlPasswordField();
}
