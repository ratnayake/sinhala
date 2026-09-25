using SinhalaInput.Platform.Windows.Caret;

namespace SinhalaInput.App;

/// <summary>
/// Runs <see cref="ICaretLocator"/> lookups off the keyboard-hook thread, on a dedicated
/// background MTA thread (UI Automation client calls must not run on an STA that is also pumping
/// the hook). Latest request wins: while a lookup is in flight, newer requests replace any
/// queued one, so rapid typing never builds up a backlog of stale lookups.
/// </summary>
internal sealed class CaretAnchorResolver : IDisposable
{
    private static readonly TimeSpan KeystrokeSettleDelay = TimeSpan.FromMilliseconds(25);

    private readonly ICaretLocator _caretLocator;
    private readonly bool _runInline;
    private readonly object _gate = new();
    private readonly AutoResetEvent _requestAvailable = new(initialState: false);
    private readonly Thread? _worker;

    private Action<ScreenPoint?>? _pendingCallback;
    private volatile bool _disposed;

    /// <param name="caretLocator">The locator to call.</param>
    /// <param name="runInline">
    /// Test-only: run each lookup synchronously on the requesting thread, so controller tests
    /// stay deterministic.
    /// </param>
    internal CaretAnchorResolver(ICaretLocator caretLocator, bool runInline)
    {
        ArgumentNullException.ThrowIfNull(caretLocator);

        _caretLocator = caretLocator;
        _runInline = runInline;

        if (!runInline)
        {
            _worker = new Thread(WorkerLoop)
            {
                IsBackground = true,
                Name = "SinhalaInput caret locator",
            };
            _worker.SetApartmentState(ApartmentState.MTA);
            _worker.Start();
        }
    }

    /// <summary>
    /// Requests a caret lookup; <paramref name="onResolved"/> receives the position (or
    /// <see langword="null"/> if none was found) on the worker thread, unless a newer request
    /// superseded it before it started.
    /// </summary>
    public void Request(Action<ScreenPoint?> onResolved)
    {
        ArgumentNullException.ThrowIfNull(onResolved);

        if (_disposed)
        {
            return;
        }

        if (_runInline)
        {
            onResolved(Locate());
            return;
        }

        lock (_gate)
        {
            _pendingCallback = onResolved;
        }

        _requestAvailable.Set();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_worker is not null)
        {
            _requestAvailable.Set();

            // A lookup blocked in a hung target application must not hold up process exit;
            // the worker is a background thread, so abandoning it is safe.
            if (_worker.Join(TimeSpan.FromMilliseconds(500)))
            {
                _requestAvailable.Dispose();
            }
        }
        else
        {
            _requestAvailable.Dispose();
        }
    }

    private void WorkerLoop()
    {
        while (true)
        {
            _requestAvailable.WaitOne();

            if (_disposed)
            {
                return;
            }

            // The hook sees each key before the target application does, so an immediate lookup
            // reports the caret as it was *before* the letter that triggered it. A short settle
            // delay lets the application process the key first, and also coalesces bursts of
            // fast typing into a single lookup.
            Thread.Sleep(KeystrokeSettleDelay);

            Action<ScreenPoint?>? callback;
            lock (_gate)
            {
                callback = _pendingCallback;
                _pendingCallback = null;
            }

            callback?.Invoke(Locate());
        }
    }

    private ScreenPoint? Locate()
    {
        try
        {
            return _caretLocator.TryGetCaretScreenPosition(out ScreenPoint position) ? position : null;
        }
#pragma warning disable CA1031 // A failed lookup must never take down the locator thread (or the hook).
        catch (Exception)
#pragma warning restore CA1031
        {
            return null;
        }
    }
}
