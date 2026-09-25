namespace SinhalaInput.App.Settings;

/// <summary>
/// The user's persisted preferences. Property defaults double as first-run values and as the
/// fallback for any property missing from an older settings file.
/// </summary>
public sealed record UserSettings
{
    public bool IsEnabled { get; init; } = true;

    public bool StartWithWindows { get; init; } = true;
}
