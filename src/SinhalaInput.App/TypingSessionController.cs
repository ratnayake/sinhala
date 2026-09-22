using System.Text;
using SinhalaInput.Core.Candidates;
using SinhalaInput.Core.Transliteration;
using SinhalaInput.Platform.Windows.Caret;
using SinhalaInput.Platform.Windows.Focus;
using SinhalaInput.Platform.Windows.Hooking;
using SinhalaInput.Platform.Windows.Input;

namespace SinhalaInput.App;

/// <summary>
/// Drives the word-boundary/editing state machine from design doc §4: buffers the Latin word
/// being typed, replaces it on screen with Sinhala at each commit trigger, and reports candidate
/// popup state so the UI layer can render it.
/// </summary>
/// <remarks>
/// Depends only on the platform/engine interfaces (never their concrete implementations), so it
/// is fully testable against Moq mocks — see <c>TypingSessionControllerTests</c>.
///
/// Known v1 gap: <see cref="IKeyboardHook"/> only reports key events, not mouse clicks or
/// cross-window focus changes, so the design doc §4 last row ("caret moved by mouse click ...
/// or focus change to a different control/window") cannot be fully implemented here. As a
/// partial mitigation, any key this controller does not otherwise recognize (e.g. function
/// keys, Alt+Tab's Tab-with-Alt-held) force-commits a pending buffer rather than leaving it
/// in limbo, and disabling the tool (Ctrl+Space) also force-commits. A true fix needs the
/// platform layer to additionally surface focus-change/mouse-click notifications.
/// </remarks>
public sealed class TypingSessionController : IDisposable
{
    private const int VkBack = 0x08;
    private const int VkTab = 0x09;
    private const int VkReturn = 0x0D;
    private const int VkShift = 0x10;
    private const int VkControl = 0x11;
    private const int VkMenu = 0x12;
    private const int VkEscape = 0x1B;
    private const int VkSpace = 0x20;
    private const int VkPageUp = 0x21;
    private const int VkPageDown = 0x22;
    private const int VkLeft = 0x25;
    private const int VkUp = 0x26;
    private const int VkRight = 0x27;
    private const int VkDown = 0x28;
    private const int Vk0 = 0x30;
    private const int Vk9 = 0x39;
    private const int VkA = 0x41;
    private const int VkZ = 0x5A;
    private const int VkLWin = 0x5B;
    private const int VkRWin = 0x5C;
    private const int VkLShift = 0xA0;
    private const int VkRShift = 0xA1;
    private const int VkLControl = 0xA2;
    private const int VkRControl = 0xA3;
    private const int VkLMenu = 0xA4;
    private const int VkRMenu = 0xA5;
    private const int VkOem1 = 0xBA; // ;:
    private const int VkOemPlus = 0xBB; // =+
    private const int VkOemComma = 0xBC; // ,<
    private const int VkOemMinus = 0xBD; // -_
    private const int VkOemPeriod = 0xBE; // .>
    private const int VkOem2 = 0xBF; // /?
    private const int VkOem3 = 0xC0; // `~
    private const int VkOem4 = 0xDB; // [{
    private const int VkOem5 = 0xDC; // \|
    private const int VkOem6 = 0xDD; // ]}
    private const int VkOem7 = 0xDE; // '"

    private readonly IKeyboardHook _keyboardHook;
    private readonly ITextInjector _textInjector;
    private readonly ICaretLocator _caretLocator;
    private readonly ITransliterationEngine _transliterationEngine;
    private readonly ICandidateProvider _candidateProvider;
    private readonly IPasswordFieldDetector _passwordFieldDetector;

    private readonly StringBuilder _buffer = new();
    private readonly HashSet<int> _pendingKeyUpSuppressions = [];

    private bool _enabled = true;
    private bool _controlDown;
    private bool _shiftDown;

    private string _primaryPreview = string.Empty;
    private IReadOnlyList<string> _candidates = [];
    private int _selectedCandidateIndex;

    public TypingSessionController(
        IKeyboardHook keyboardHook,
        ITextInjector textInjector,
        ICaretLocator caretLocator,
        ITransliterationEngine transliterationEngine,
        ICandidateProvider candidateProvider,
        IPasswordFieldDetector passwordFieldDetector)
    {
        ArgumentNullException.ThrowIfNull(keyboardHook);
        ArgumentNullException.ThrowIfNull(textInjector);
        ArgumentNullException.ThrowIfNull(caretLocator);
        ArgumentNullException.ThrowIfNull(transliterationEngine);
        ArgumentNullException.ThrowIfNull(candidateProvider);
        ArgumentNullException.ThrowIfNull(passwordFieldDetector);

        _keyboardHook = keyboardHook;
        _textInjector = textInjector;
        _caretLocator = caretLocator;
        _transliterationEngine = transliterationEngine;
        _candidateProvider = candidateProvider;
        _passwordFieldDetector = passwordFieldDetector;

        _keyboardHook.KeyIntercepted += OnKeyIntercepted;
    }

    /// <summary>Raised whenever the candidate popup should change what it displays.</summary>
    public event EventHandler<CandidatePopupState>? PopupStateChanged;

    /// <summary>Raised whenever Ctrl+Space toggles the tool on or off.</summary>
    public event EventHandler<bool>? EnabledChanged;

    /// <summary>Whether the tool is currently transliterating (toggled via Ctrl+Space).</summary>
    public bool IsEnabled => _enabled;

    /// <summary>Toggles the tool on/off, the same action Ctrl+Space performs (e.g. from the tray menu).</summary>
    public void ToggleEnabled()
    {
        _enabled = !_enabled;

        if (!_enabled)
        {
            CommitBuffer();
        }

        EnabledChanged?.Invoke(this, _enabled);
    }

    public void Dispose()
    {
        _keyboardHook.KeyIntercepted -= OnKeyIntercepted;
    }

    private void OnKeyIntercepted(object? sender, KeyInterceptedEventArgs e)
    {
        UpdateModifierState(e);

        if (!e.IsKeyDown)
        {
            if (_pendingKeyUpSuppressions.Remove(e.VirtualKeyCode))
            {
                e.Handled = true;
            }

            return;
        }

        if (e.VirtualKeyCode == VkSpace && _controlDown)
        {
            ToggleEnabled();
            Suppress(e);
            return;
        }

        if (!_enabled)
        {
            return;
        }

        HandleKeyDown(e);
    }

    private void HandleKeyDown(KeyInterceptedEventArgs e)
    {
        int vk = e.VirtualKeyCode;

        if (IsModifierKey(vk))
        {
            return;
        }

        if (_passwordFieldDetector.IsFocusedControlPasswordField())
        {
            // Never buffer, preview, transliterate, inject, or learn from a password field
            // (design doc §10). Discard rather than commit anything already buffered — the
            // word may have been typed partly before focus moved here — and let every key
            // pass through untouched so the field's own masking is the only thing on screen.
            if (_buffer.Length > 0)
            {
                ResetBuffer();
            }

            return;
        }

        if (IsVerticalNavigationKey(vk))
        {
            if (_buffer.Length > 0)
            {
                MoveCandidateSelection(vk is VkUp or VkPageUp ? -1 : 1, e);
            }

            return;
        }

        if (vk is VkLeft or VkRight)
        {
            if (_buffer.Length > 0)
            {
                CommitBuffer();
            }

            return;
        }

        if (IsDigitRow(vk))
        {
            HandleDigit(vk, e);
            return;
        }

        if (vk == VkBack)
        {
            HandleBackspace();
            return;
        }

        if (vk == VkEscape)
        {
            HandleEscape(e);
            return;
        }

        if (IsCommitTriggerKey(vk))
        {
            CommitBuffer();
            return;
        }

        if (IsLetterKey(vk))
        {
            AppendLetter(vk);
            return;
        }

        // Any other key (function keys, Home/End, Alt-held combos, etc.): force-commit a
        // pending word rather than leaving it in limbo, per design doc §4's closing principle.
        if (_buffer.Length > 0)
        {
            CommitBuffer();
        }
    }

    private void HandleDigit(int vk, KeyInterceptedEventArgs e)
    {
        int digit = vk - Vk0;

        if (_buffer.Length > 0 && !_shiftDown && digit is >= 1 and <= 9)
        {
            SelectCandidate(digit, e);
            return;
        }

        if (_buffer.Length > 0)
        {
            // Shift+digit is a punctuation symbol (e.g. Shift+1 = "!"): treat it as a commit
            // trigger like other punctuation, then let it pass through as usual.
            CommitBuffer();
        }
    }

    private void HandleBackspace()
    {
        if (_buffer.Length == 0)
        {
            // Nothing buffered: pass through untouched so it deletes whatever precedes the caret.
            return;
        }

        _buffer.Length--;
        UpdatePopup();

        // Left un-suppressed deliberately: the raw Latin word is still on screen (design doc §1
        // point 2 — the target application's text is untouched until commit), so letting
        // Backspace pass through keeps the on-screen text and this buffer in sync.
    }

    private void HandleEscape(KeyInterceptedEventArgs e)
    {
        if (_buffer.Length == 0)
        {
            return;
        }

        // The on-screen text is still the raw Latin the user typed (nothing has been injected
        // for this word yet), so "reverting to raw Latin" only requires abandoning the buffer
        // and closing the popup — no ITextInjector call is needed.
        ResetBuffer();
        Suppress(e);
    }

    private void AppendLetter(int vk)
    {
        if (_controlDown)
        {
            // e.g. Ctrl+A/Ctrl+C shortcuts: not part of a Singlish word, pass through untouched.
            return;
        }

        char baseChar = (char)('a' + (vk - VkA));
        _buffer.Append(_shiftDown ? char.ToUpperInvariant(baseChar) : baseChar);
        UpdatePopup();

        // Left un-suppressed: the letter passes through so the target app shows the raw Latin
        // word while it is still being buffered (design doc §1 point 2).
    }

    private void CommitBuffer()
    {
        if (_buffer.Length == 0)
        {
            return;
        }

        string latin = _buffer.ToString();
        string sinhala = _candidates.Count > 0
            ? _candidates[_selectedCandidateIndex]
            : _transliterationEngine.Transliterate(latin);

        _textInjector.ReplaceTypedText(latin.Length, sinhala);
        ResetBuffer();
    }

    private void SelectCandidate(int oneBasedIndex, KeyInterceptedEventArgs e)
    {
        Suppress(e);

        if (oneBasedIndex > _candidates.Count)
        {
            return;
        }

        string latin = _buffer.ToString();
        string chosen = _candidates[oneBasedIndex - 1];

        _textInjector.ReplaceTypedText(latin.Length, chosen);
        _candidateProvider.LearnSelection(latin, chosen);
        ResetBuffer();
    }

    private void MoveCandidateSelection(int delta, KeyInterceptedEventArgs e)
    {
        Suppress(e);

        if (_candidates.Count == 0)
        {
            return;
        }

        _selectedCandidateIndex = Math.Clamp(_selectedCandidateIndex + delta, 0, _candidates.Count - 1);
        RaisePopupStateChanged(isOpen: true);
    }

    private void UpdatePopup()
    {
        if (_buffer.Length == 0)
        {
            ResetBuffer();
            return;
        }

        string latin = _buffer.ToString();
        _primaryPreview = _transliterationEngine.Transliterate(latin);
        _candidates = _candidateProvider.GetCandidates(latin);
        _selectedCandidateIndex = 0;
        RaisePopupStateChanged(isOpen: true);
    }

    private void ResetBuffer()
    {
        _buffer.Clear();
        _primaryPreview = string.Empty;
        _candidates = [];
        _selectedCandidateIndex = 0;
        RaisePopupStateChanged(isOpen: false);
    }

    private void RaisePopupStateChanged(bool isOpen)
    {
        ScreenPoint? anchor = null;
        if (isOpen && _caretLocator.TryGetCaretScreenPosition(out ScreenPoint position))
        {
            anchor = position;
        }

        PopupStateChanged?.Invoke(
            this,
            new CandidatePopupState(isOpen, _primaryPreview, _candidates, _selectedCandidateIndex, anchor));
    }

    private void UpdateModifierState(KeyInterceptedEventArgs e)
    {
        switch (e.VirtualKeyCode)
        {
            case VkControl or VkLControl or VkRControl:
                _controlDown = e.IsKeyDown;
                break;
            case VkShift or VkLShift or VkRShift:
                _shiftDown = e.IsKeyDown;
                break;
        }
    }

    private void Suppress(KeyInterceptedEventArgs e)
    {
        e.Handled = true;
        _pendingKeyUpSuppressions.Add(e.VirtualKeyCode);
    }

    private static bool IsModifierKey(int vk) =>
        vk is VkShift or VkControl or VkMenu
            or VkLShift or VkRShift or VkLControl or VkRControl or VkLMenu or VkRMenu
            or VkLWin or VkRWin;

    private static bool IsVerticalNavigationKey(int vk) => vk is VkUp or VkDown or VkPageUp or VkPageDown;

    private static bool IsDigitRow(int vk) => vk is >= Vk0 and <= Vk9;

    private static bool IsLetterKey(int vk) => vk is >= VkA and <= VkZ;

    private static bool IsCommitTriggerKey(int vk) =>
        vk is VkSpace or VkReturn or VkTab
            or VkOem1 or VkOemPlus or VkOemComma or VkOemMinus or VkOemPeriod or VkOem2
            or VkOem3 or VkOem4 or VkOem5 or VkOem6 or VkOem7;
}
