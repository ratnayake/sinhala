using System.Diagnostics;
using Moq;
using SinhalaInput.Platform.Windows.Caret;

namespace SinhalaInput.App.Tests;

/// <summary>
/// Covers how <see cref="TypingSessionController"/> resolves the popup anchor: never on the
/// hook-callback thread, and never applying a stale lookup to a later word.
/// </summary>
public class CaretAnchorTests
{
    private delegate bool TryGetCaret(out ScreenPoint position);

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    [Fact]
    public void ResolvedAnchor_IsIncludedInOpenPopupState()
    {
        var harness = CreateHarness(resolveCaretInline: true);
        SetupCaret(harness, (out ScreenPoint p) =>
        {
            p = new ScreenPoint(100, 200);
            return true;
        });

        var states = new List<CandidatePopupState>();
        harness.Controller.PopupStateChanged += (_, state) => states.Add(state);

        harness.Type("ma");

        Assert.True(states[^1].IsOpen);
        Assert.Equal(new ScreenPoint(100, 200), states[^1].Anchor);
    }

    [Fact]
    public void NewWord_DoesNotInheritPreviousWordsAnchor()
    {
        var harness = CreateHarness(resolveCaretInline: true);
        SetupCaret(harness, (out ScreenPoint p) =>
        {
            p = new ScreenPoint(100, 200);
            return true;
        });

        var states = new List<CandidatePopupState>();
        harness.Controller.PopupStateChanged += (_, state) => states.Add(state);

        harness.Type("ma");
        harness.PressKey(VirtualKeys.Space);

        SetupCaret(harness, (out ScreenPoint p) =>
        {
            p = default;
            return false;
        });
        harness.Type("k");

        Assert.True(states[^1].IsOpen);
        Assert.Null(states[^1].Anchor);
    }

    [Fact]
    public void MovingCandidateSelection_DoesNotLocateCaretAgain()
    {
        var harness = CreateHarness(resolveCaretInline: true);
        harness.Candidates
            .Setup(c => c.GetCandidates(It.IsAny<string>()))
            .Returns(["a", "b"]);
        int lookups = 0;
        SetupCaret(harness, (out ScreenPoint p) =>
        {
            lookups++;
            p = new ScreenPoint(1, 2);
            return true;
        });

        harness.Type("ma");
        harness.PressKey(VirtualKeys.Down);

        Assert.Equal(2, lookups);
    }

    [Fact]
    public async Task SlowCaretLocator_DoesNotBlockTheHookCallback()
    {
        using var release = new ManualResetEventSlim();
        var harness = CreateHarness(resolveCaretInline: false);
        SetupCaret(harness, (out ScreenPoint p) =>
        {
            release.Wait(Timeout);
            p = new ScreenPoint(300, 400);
            return true;
        });

        var anchored = new TaskCompletionSource<CandidatePopupState>(TaskCreationOptions.RunContinuationsAsynchronously);
        var states = new List<CandidatePopupState>();
        harness.Controller.PopupStateChanged += (_, state) =>
        {
            lock (states)
            {
                states.Add(state);
            }

            if (state.Anchor is not null)
            {
                anchored.TrySetResult(state);
            }
        };

        var stopwatch = Stopwatch.StartNew();
        harness.Type("mama");
        stopwatch.Stop();

        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(1), $"Typing blocked for {stopwatch.Elapsed}.");
        lock (states)
        {
            Assert.True(states[^1].IsOpen);
        }

        release.Set();

        CandidatePopupState anchoredState = await anchored.Task.WaitAsync(Timeout);
        Assert.Equal(new ScreenPoint(300, 400), anchoredState.Anchor);
        harness.Controller.Dispose();
    }

    [Fact]
    public void AnchorResolvedAfterCommit_IsDiscarded()
    {
        using var lookupStarted = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        using var lookupFinished = new ManualResetEventSlim();
        var harness = CreateHarness(resolveCaretInline: false);
        SetupCaret(harness, (out ScreenPoint p) =>
        {
            lookupStarted.Set();
            release.Wait(Timeout);
            p = new ScreenPoint(300, 400);
            return true;
        });

        var states = new List<CandidatePopupState>();
        harness.Controller.PopupStateChanged += (_, state) =>
        {
            lock (states)
            {
                states.Add(state);
            }
        };

        harness.Type("m");
        Assert.True(lookupStarted.Wait(Timeout));
        harness.PressKey(VirtualKeys.Space);
        release.Set();

        // Dispose joins the resolver thread, so its callback has run (or been dropped) by now.
        harness.Controller.Dispose();

        lock (states)
        {
            Assert.False(states[^1].IsOpen);
            Assert.DoesNotContain(states, s => s.Anchor is not null);
        }
    }

    private static ControllerHarness CreateHarness(bool resolveCaretInline)
    {
        var harness = new ControllerHarness(resolveCaretInline);
        harness.Engine.Setup(e => e.Transliterate(It.IsAny<string>())).Returns("x");
        return harness;
    }

    private static void SetupCaret(ControllerHarness harness, TryGetCaret implementation) =>
        harness.Caret
            .Setup(c => c.TryGetCaretScreenPosition(out It.Ref<ScreenPoint>.IsAny))
            .Returns(implementation);
}
