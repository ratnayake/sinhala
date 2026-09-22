using Moq;

namespace SinhalaInput.App.Tests;

/// <summary>
/// Drives <see cref="TypingSessionController"/> (the design doc §4 state machine) against Moq
/// mocks of all five interfaces it depends on — never a real hook or real Win32 calls.
/// </summary>
public sealed class TypingSessionControllerTests
{
    [Fact]
    public void Space_CommitsBufferedWord_ThenPassesSpaceThrough()
    {
        var harness = new ControllerHarness();
        harness.Engine.Setup(e => e.Transliterate("mama")).Returns("මම");

        harness.Type("mama");
        harness.Injector.VerifyNoOtherCalls();

        var spaceKeyDown = harness.KeyDown(VirtualKeys.Space);

        harness.Injector.Verify(i => i.ReplaceTypedText(4, "මම"), Times.Once);
        harness.Injector.VerifyNoOtherCalls();
        Assert.False(spaceKeyDown.Handled);
    }

    [Fact]
    public void Enter_And_Punctuation_AlsoCommitBufferedWord()
    {
        var harness = new ControllerHarness();
        harness.Engine.Setup(e => e.Transliterate("api")).Returns("අපි");

        harness.Type("api");
        var enterKeyDown = harness.KeyDown(VirtualKeys.Return);

        harness.Injector.Verify(i => i.ReplaceTypedText(3, "අපි"), Times.Once);
        Assert.False(enterKeyDown.Handled);

        harness.Engine.Setup(e => e.Transliterate("kko")).Returns("X");
        harness.Type("kko");
        var periodKeyDown = harness.KeyDown(VirtualKeys.OemPeriod);

        harness.Injector.Verify(i => i.ReplaceTypedText(3, "X"), Times.Once);
        Assert.False(periodKeyDown.Handled);
    }

    [Fact]
    public void Backspace_EditsTheBuffer_WithoutCallingTextInjector()
    {
        var harness = new ControllerHarness();
        harness.Engine.Setup(e => e.Transliterate("mam")).Returns("PARTIAL");
        harness.Engine.Setup(e => e.Transliterate("mama")).Returns("මම");

        harness.Type("mama");
        var backspaceKeyDown = harness.KeyDown(VirtualKeys.Back);
        harness.KeyUp(VirtualKeys.Back);

        // Popping the buffer must not touch already-on-screen text: no injector call yet, and
        // the key passes through so the target app's own raw-Latin text shrinks in step.
        harness.Injector.VerifyNoOtherCalls();
        Assert.False(backspaceKeyDown.Handled);

        harness.KeyDown(VirtualKeys.Space);

        harness.Injector.Verify(i => i.ReplaceTypedText(3, "PARTIAL"), Times.Once);
        harness.Injector.VerifyNoOtherCalls();
    }

    [Fact]
    public void Backspace_WithEmptyBuffer_PassesThroughUntouched()
    {
        var harness = new ControllerHarness();

        var backspaceKeyDown = harness.KeyDown(VirtualKeys.Back);

        Assert.False(backspaceKeyDown.Handled);
        harness.Injector.VerifyNoOtherCalls();
    }

    [Fact]
    public void Digit_WhilePopupOpen_SelectsCandidate_AndLearnsSelection()
    {
        var harness = new ControllerHarness();
        harness.Engine.Setup(e => e.Transliterate("kara")).Returns("PRIMARY");
        harness.Candidates
            .Setup(c => c.GetCandidates("kara"))
            .Returns(["PRIMARY", "ALT-2", "ALT-3"]);

        harness.Type("kara");
        var digitKeyDown = harness.KeyDown(VirtualKeys.Digit(2));

        harness.Injector.Verify(i => i.ReplaceTypedText(4, "ALT-2"), Times.Once);
        harness.Candidates.Verify(c => c.LearnSelection("kara", "ALT-2"), Times.Once);
        Assert.True(digitKeyDown.Handled);

        // Buffer must have been reset: typing a fresh word now commits only that word.
        harness.Engine.Setup(e => e.Transliterate("hi")).Returns("HI");
        harness.Type("hi");
        harness.KeyDown(VirtualKeys.Space);
        harness.Injector.Verify(i => i.ReplaceTypedText(2, "HI"), Times.Once);
    }

    [Fact]
    public void Digit_WithEmptyBuffer_PassesThroughAsOrdinaryDigit()
    {
        var harness = new ControllerHarness();

        var digitKeyDown = harness.KeyDown(VirtualKeys.Digit(5));

        Assert.False(digitKeyDown.Handled);
        harness.Injector.VerifyNoOtherCalls();
    }

    [Fact]
    public void ArrowKeys_WhilePopupOpen_MoveCandidateSelection_AndCommitSelectedOne()
    {
        var harness = new ControllerHarness();
        harness.Engine.Setup(e => e.Transliterate("ta")).Returns("PRIMARY");
        harness.Candidates
            .Setup(c => c.GetCandidates("ta"))
            .Returns(["PRIMARY", "ALT"]);

        harness.Type("ta");
        var downArrow = harness.KeyDown(VirtualKeys.Down);
        Assert.True(downArrow.Handled);

        harness.KeyDown(VirtualKeys.Return);

        harness.Injector.Verify(i => i.ReplaceTypedText(2, "ALT"), Times.Once);
    }

    [Fact]
    public void Escape_RevertsToRawLatin_WithoutTouchingScreenText_AndClosesPopup()
    {
        var harness = new ControllerHarness();
        harness.Engine.Setup(e => e.Transliterate("test")).Returns("SHOULD-NOT-BE-USED");

        harness.Type("test");
        var escapeKeyDown = harness.KeyDown(VirtualKeys.Escape);

        harness.Injector.VerifyNoOtherCalls();
        Assert.True(escapeKeyDown.Handled);

        // The buffer must be fully cleared, not merely closed: a fresh word after Esc commits cleanly.
        harness.Engine.Setup(e => e.Transliterate("hi")).Returns("HI");
        harness.Type("hi");
        harness.KeyDown(VirtualKeys.Space);
        harness.Injector.Verify(i => i.ReplaceTypedText(2, "HI"), Times.Once);
        harness.Injector.VerifyNoOtherCalls();
    }

    [Fact]
    public void CtrlSpace_TogglesToolOff_ForceCommittingAnyPendingWord()
    {
        var harness = new ControllerHarness();
        harness.Engine.Setup(e => e.Transliterate("mama")).Returns("මම");

        harness.Type("mama");
        harness.PressCtrlSpace();

        harness.Injector.Verify(i => i.ReplaceTypedText(4, "මම"), Times.Once);
        Assert.False(harness.Controller.IsEnabled);
    }

    [Fact]
    public void CtrlSpace_WhileDisabled_LetsAllKeysPassThroughUntouched()
    {
        var harness = new ControllerHarness();
        harness.PressCtrlSpace();
        Assert.False(harness.Controller.IsEnabled);

        harness.Type("mama");
        var spaceKeyDown = harness.KeyDown(VirtualKeys.Space);

        harness.Injector.VerifyNoOtherCalls();
        Assert.False(spaceKeyDown.Handled);
    }

    [Fact]
    public void CtrlSpace_TogglesBackOn_ResumingNormalBehaviour()
    {
        var harness = new ControllerHarness();
        harness.PressCtrlSpace();
        harness.PressCtrlSpace();
        Assert.True(harness.Controller.IsEnabled);

        harness.Engine.Setup(e => e.Transliterate("mama")).Returns("මම");
        harness.Type("mama");
        harness.KeyDown(VirtualKeys.Space);

        harness.Injector.Verify(i => i.ReplaceTypedText(4, "මම"), Times.Once);
    }

    [Fact]
    public void PasswordField_LettersAndDigitsPassThrough_NeverBufferedOrInjectedOrLearned()
    {
        var harness = new ControllerHarness();
        harness.PasswordFieldDetector.Setup(p => p.IsFocusedControlPasswordField()).Returns(true);

        // A realistic password with a mid-word digit — this is exactly the sequence that, prior
        // to the password-field guard, would have caused HandleDigit to treat "2" as a candidate
        // selection and persist the "Winter" fragment via ICandidateProvider.LearnSelection.
        harness.Type("winter");
        var digitKeyDown = harness.KeyDown(VirtualKeys.Digit(2));
        var spaceKeyDown = harness.KeyDown(VirtualKeys.Space);

        harness.Injector.VerifyNoOtherCalls();
        harness.Candidates.Verify(c => c.LearnSelection(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        Assert.False(digitKeyDown.Handled);
        Assert.False(spaceKeyDown.Handled);
    }

    [Fact]
    public void PasswordField_NeverOpensCandidatePopup()
    {
        var harness = new ControllerHarness();
        harness.PasswordFieldDetector.Setup(p => p.IsFocusedControlPasswordField()).Returns(true);

        var states = new List<CandidatePopupState>();
        harness.Controller.PopupStateChanged += (_, state) => states.Add(state);

        harness.Type("secret");

        Assert.Empty(states);
    }

    [Fact]
    public void FocusMovesIntoPasswordFieldMidWord_DiscardsBufferedWord_WithoutCommitting()
    {
        var harness = new ControllerHarness();
        harness.Engine.Setup(e => e.Transliterate("partial")).Returns("SHOULD-NOT-BE-USED");

        harness.Type("partial");
        harness.Injector.VerifyNoOtherCalls();

        // Simulate focus having moved to a password field before the word was committed.
        harness.PasswordFieldDetector.Setup(p => p.IsFocusedControlPasswordField()).Returns(true);
        harness.KeyDown(VirtualKeys.Digit(1));
        harness.KeyDown(VirtualKeys.Space);

        harness.Injector.VerifyNoOtherCalls();
        harness.Candidates.Verify(c => c.LearnSelection(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void PopupStateChanged_OpensWhileTyping_AndClosesAfterCommit()
    {
        var harness = new ControllerHarness();
        harness.Engine.Setup(e => e.Transliterate("mama")).Returns("මම");

        var states = new List<CandidatePopupState>();
        harness.Controller.PopupStateChanged += (_, state) => states.Add(state);

        harness.Type("mama");
        Assert.True(states[^1].IsOpen);

        harness.KeyDown(VirtualKeys.Space);
        Assert.False(states[^1].IsOpen);
    }
}
