using Moq;
using SinhalaInput.Core.Candidates;
using SinhalaInput.Core.Transliteration;
using SinhalaInput.Platform.Windows.Caret;
using SinhalaInput.Platform.Windows.Focus;
using SinhalaInput.Platform.Windows.Hooking;
using SinhalaInput.Platform.Windows.Input;

namespace SinhalaInput.App.Tests;

/// <summary>
/// Wires a <see cref="TypingSessionController"/> to Moq mocks of all six interfaces it depends
/// on, and drives it by raising <see cref="IKeyboardHook.KeyIntercepted"/> the same way a real
/// hook would.
/// </summary>
internal sealed class ControllerHarness
{
    public Mock<IKeyboardHook> Hook { get; } = new();
    public Mock<ITextInjector> Injector { get; } = new();
    public Mock<ICaretLocator> Caret { get; } = new();
    public Mock<ITransliterationEngine> Engine { get; } = new();
    public Mock<ICandidateProvider> Candidates { get; } = new();
    public Mock<IPasswordFieldDetector> PasswordFieldDetector { get; } = new();

    public TypingSessionController Controller { get; }

    /// <param name="resolveCaretInline">
    /// <see langword="true"/> (the default) resolves the caret synchronously so tests are
    /// deterministic; <see langword="false"/> uses the production background resolver thread.
    /// </param>
    public ControllerHarness(bool resolveCaretInline = true)
    {
        ScreenPoint ignoredPosition;
        Caret.Setup(c => c.TryGetCaretScreenPosition(out ignoredPosition)).Returns(false);

        // By default, the candidate list mirrors whatever the engine mock produces, matching
        // the real (v1 baseline) CandidateProvider's behaviour of wrapping the engine's output.
        Candidates
            .Setup(c => c.GetCandidates(It.IsAny<string>()))
            .Returns((string latin) => new List<string> { Engine.Object.Transliterate(latin) });

        // Default to "not a password field" so existing tests exercise the normal typing path;
        // tests that care about password-field behaviour override this explicitly.
        PasswordFieldDetector.Setup(p => p.IsFocusedControlPasswordField()).Returns(false);

        Controller = resolveCaretInline
            ? new TypingSessionController(
                Hook.Object,
                Injector.Object,
                Caret.Object,
                Engine.Object,
                Candidates.Object,
                PasswordFieldDetector.Object,
                resolveCaretInline: true)
            : new TypingSessionController(
                Hook.Object,
                Injector.Object,
                Caret.Object,
                Engine.Object,
                Candidates.Object,
                PasswordFieldDetector.Object);
    }

    public KeyInterceptedEventArgs KeyDown(int virtualKeyCode)
    {
        var args = new KeyInterceptedEventArgs(virtualKeyCode, isKeyDown: true);
        Hook.Raise(h => h.KeyIntercepted += null, Hook.Object, args);
        return args;
    }

    public KeyInterceptedEventArgs KeyUp(int virtualKeyCode)
    {
        var args = new KeyInterceptedEventArgs(virtualKeyCode, isKeyDown: false);
        Hook.Raise(h => h.KeyIntercepted += null, Hook.Object, args);
        return args;
    }

    public KeyInterceptedEventArgs PressKey(int virtualKeyCode)
    {
        KeyInterceptedEventArgs down = KeyDown(virtualKeyCode);
        KeyUp(virtualKeyCode);
        return down;
    }

    /// <summary>Presses each letter of <paramref name="lowerCaseLatinWord"/> as its own key event.</summary>
    public void Type(string lowerCaseLatinWord)
    {
        foreach (char letter in lowerCaseLatinWord)
        {
            PressKey(VirtualKeys.Letter(letter));
        }
    }

    public void PressCtrlSpace()
    {
        KeyDown(VirtualKeys.Control);
        PressKey(VirtualKeys.Space);
        KeyUp(VirtualKeys.Control);
    }
}
