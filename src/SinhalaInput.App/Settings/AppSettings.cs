namespace SinhalaInput.App.Settings;

/// <summary>
/// Static, v1-only display value for the toggle hotkey. It is not user-remappable yet
/// (design doc §12 roadmap tracks hotkey remapping as v1.1 work); the actual on/off state
/// lives on <see cref="TypingSessionController"/>, which is the single source of truth, and is
/// persisted across launches by <see cref="SettingsCoordinator"/>.
/// </summary>
public static class AppSettings
{
    public const string ToggleHotkeyDisplay = "Ctrl+Space";
}
