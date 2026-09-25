namespace SinhalaInput.Platform.Windows.Caret;

/// <summary>
/// Pure plausibility checks that let <see cref="Win32CaretLocator"/> reject the degenerate
/// "success" results some strategies report, so the chain falls through to a strategy that
/// actually knows where the caret is.
/// </summary>
internal static class CaretGeometry
{
    /// <summary>
    /// <c>GetCaretPos</c> returns <see langword="true"/> with client point <c>(0, 0)</c> for
    /// windows that own no real Win32 caret (observed live in Chromium/Edge). A genuine caret at
    /// exactly the client origin is effectively impossible (edit controls always have a margin),
    /// so that point is treated as "no caret".
    /// </summary>
    internal static bool IsPlausibleClientCaretPoint(int x, int y) => x != 0 || y != 0;

    /// <summary>
    /// A real text caret always has a height (its width may legitimately be 0 or 1). An all-zero
    /// or non-finite rectangle is what providers report when they have no caret to describe.
    /// </summary>
    internal static bool IsPlausibleCaretRect(double left, double top, double width, double height) =>
        double.IsFinite(left) && double.IsFinite(top) && double.IsFinite(width) && double.IsFinite(height)
        && width >= 0 && height > 0;

    /// <summary>A focused element's own bounds must have a real area to be worth anchoring to.</summary>
    internal static bool IsPlausibleElementRect(double left, double top, double width, double height) =>
        IsPlausibleCaretRect(left, top, width, height) && width > 0;
}
