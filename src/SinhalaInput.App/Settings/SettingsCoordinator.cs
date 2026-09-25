using System.Diagnostics;
using System.IO;

namespace SinhalaInput.App.Settings;

/// <summary>
/// Owns the persisted <see cref="UserSettings"/>: applies them to the running app at startup,
/// keeps them in sync with the controller's on/off state, and drives start-at-sign-in.
/// </summary>
public sealed class SettingsCoordinator : IDisposable
{
    private readonly TypingSessionController _controller;
    private readonly ISettingsStore _store;
    private readonly IStartupRegistration _startupRegistration;
    private readonly Action<Action> _scheduleSave;
    private readonly Lock _gate = new();
    private readonly Lock _saveGate = new();
    private UserSettings _settings = new();
    private bool _initialized;

    /// <param name="scheduleSave">
    /// Runs a save off the calling thread. <see cref="TypingSessionController.EnabledChanged"/>
    /// fires from inside the low-level keyboard hook callback, which Windows silently unhooks
    /// if it is slow, so disk I/O must not happen inline. Defaults to the thread pool.
    /// </param>
    public SettingsCoordinator(
        TypingSessionController controller,
        ISettingsStore store,
        IStartupRegistration startupRegistration,
        Action<Action>? scheduleSave = null)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(startupRegistration);
        _controller = controller;
        _store = store;
        _startupRegistration = startupRegistration;
        _scheduleSave = scheduleSave ?? (save => ThreadPool.QueueUserWorkItem(_ => save()));
    }

    public bool StartWithWindows
    {
        get
        {
            lock (_gate)
            {
                return _settings.StartWithWindows;
            }
        }
    }

    public bool IsStartupManagedByWindows => _startupRegistration.IsManagedByWindows;

    public void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        UserSettings loaded = _store.Load();
        lock (_gate)
        {
            _settings = loaded;
        }

        if (!loaded.IsEnabled && _controller.IsEnabled)
        {
            _controller.ToggleEnabled();
        }

        _controller.EnabledChanged += OnControllerEnabledChanged;

        // Re-applied every launch so the Run value follows the exe if it has moved.
        TryApplyStartupRegistration(loaded.StartWithWindows);
        SaveNow();
    }

    public void SetStartWithWindows(bool enabled)
    {
        lock (_gate)
        {
            if (_settings.StartWithWindows == enabled)
            {
                return;
            }

            _settings = _settings with { StartWithWindows = enabled };
        }

        TryApplyStartupRegistration(enabled);
        _scheduleSave(SaveNow);
    }

    public void Dispose()
    {
        if (!_initialized)
        {
            return;
        }

        _initialized = false;
        _controller.EnabledChanged -= OnControllerEnabledChanged;
        SaveNow();
    }

    private void OnControllerEnabledChanged(object? sender, bool isEnabled)
    {
        lock (_gate)
        {
            _settings = _settings with { IsEnabled = isEnabled };
        }

        _scheduleSave(SaveNow);
    }

    private void TryApplyStartupRegistration(bool enabled)
    {
        if (_startupRegistration.IsManagedByWindows)
        {
            return;
        }

        try
        {
            _startupRegistration.SetEnabled(enabled);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
        {
            // Policy-locked registries shouldn't stop the tool; it just won't auto-start.
            Debug.WriteLine($"SinhalaInput: could not update start-at-sign-in registration: {ex}");
        }
    }

    private void SaveNow()
    {
        // _saveGate serialises saves and each takes its snapshot only once it holds it, so the
        // last save to run writes the newest state. _gate is never held during I/O because the
        // hook callback takes it (via OnControllerEnabledChanged).
        lock (_saveGate)
        {
            UserSettings snapshot;
            lock (_gate)
            {
                snapshot = _settings;
            }

            try
            {
                _store.Save(snapshot);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Debug.WriteLine($"SinhalaInput: could not save settings: {ex}");
            }
        }
    }
}
