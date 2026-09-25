namespace SinhalaInput.App.Settings;

/// <summary>Registers (or unregisters) the app to launch when the user signs in to Windows.</summary>
public interface IStartupRegistration
{
    /// <summary>
    /// True when Windows itself owns the setting (the MSIX <c>StartupTask</c>, toggled under
    /// Settings › Apps › Startup), so <see cref="SetEnabled"/> has no effect and the app can't
    /// report the current state.
    /// </summary>
    bool IsManagedByWindows { get; }

    void SetEnabled(bool enabled);
}
