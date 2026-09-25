using System.Windows;

namespace SinhalaInput.App.Settings;

/// <summary>
/// Settings UI: an enable/disable toggle mirroring <see cref="TypingSessionController"/>, the
/// start-at-sign-in preference, and a static display of the (not yet remappable) toggle hotkey.
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly TypingSessionController _controller;
    private readonly SettingsCoordinator _settings;
    private bool _suppressCheckBoxEvent;

    public SettingsWindow(TypingSessionController controller, SettingsCoordinator settings)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(settings);
        _controller = controller;
        _settings = settings;

        InitializeComponent();

        HotkeyText.Text = $"Toggle hotkey: {AppSettings.ToggleHotkeyDisplay} (not remappable in v1)";
        SetCheckBoxWithoutRaisingEvent(_controller.IsEnabled);

        if (_settings.IsStartupManagedByWindows)
        {
            // The packaged app can't read its StartupTask state without WinRT projections, so
            // rather than show a possibly wrong checkbox, point the user at where it lives.
            StartWithWindowsCheckBox.Visibility = Visibility.Collapsed;
            StartupManagedText.Visibility = Visibility.Visible;
        }
        else
        {
            StartWithWindowsCheckBox.IsChecked = _settings.StartWithWindows;
        }

        _controller.EnabledChanged += OnControllerEnabledChanged;
        Closed += (_, _) => _controller.EnabledChanged -= OnControllerEnabledChanged;
    }

    private void OnEnabledCheckBoxToggled(object sender, RoutedEventArgs e)
    {
        if (_suppressCheckBoxEvent)
        {
            return;
        }

        if (EnabledCheckBox.IsChecked == _controller.IsEnabled)
        {
            return;
        }

        _controller.ToggleEnabled();
    }

    private void OnStartWithWindowsCheckBoxToggled(object sender, RoutedEventArgs e)
    {
        // Also fires when the constructor sets the initial state; SetStartWithWindows ignores
        // an unchanged value, so that is harmless.
        if (_settings.IsStartupManagedByWindows)
        {
            return;
        }

        _settings.SetStartWithWindows(StartWithWindowsCheckBox.IsChecked == true);
    }

    private void OnControllerEnabledChanged(object? sender, bool isEnabled) =>
        SetCheckBoxWithoutRaisingEvent(isEnabled);

    private void SetCheckBoxWithoutRaisingEvent(bool isEnabled)
    {
        _suppressCheckBoxEvent = true;
        EnabledCheckBox.IsChecked = isEnabled;
        _suppressCheckBoxEvent = false;
    }
}
