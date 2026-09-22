using System.Windows;

namespace SinhalaInput.App.Settings;

/// <summary>
/// Minimal v1 settings UI: an enable/disable toggle mirroring <see cref="TypingSessionController"/>
/// and a static display of the (not yet remappable) toggle hotkey.
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly TypingSessionController _controller;
    private bool _suppressCheckBoxEvent;

    public SettingsWindow(TypingSessionController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);
        _controller = controller;

        InitializeComponent();

        HotkeyText.Text = $"Toggle hotkey: {AppSettings.ToggleHotkeyDisplay} (not remappable in v1)";
        SetCheckBoxWithoutRaisingEvent(_controller.IsEnabled);

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

    private void OnControllerEnabledChanged(object? sender, bool isEnabled) =>
        SetCheckBoxWithoutRaisingEvent(isEnabled);

    private void SetCheckBoxWithoutRaisingEvent(bool isEnabled)
    {
        _suppressCheckBoxEvent = true;
        EnabledCheckBox.IsChecked = isEnabled;
        _suppressCheckBoxEvent = false;
    }
}
