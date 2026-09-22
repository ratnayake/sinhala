using SinhalaInput.App.Settings;

namespace SinhalaInput.App;

/// <summary>
/// Owns the system tray icon (<see cref="System.Windows.Forms.NotifyIcon"/>) and its context menu:
/// Enable/Disable, Settings, Exit. This tool is tray-resident with no always-visible main window.
/// </summary>
public sealed class TrayApplicationContext : IDisposable
{
    private readonly System.Windows.Forms.NotifyIcon _notifyIcon;
    private readonly System.Windows.Forms.ToolStripMenuItem _toggleMenuItem;
    private readonly TypingSessionController _controller;
    private readonly Func<SettingsWindow> _settingsWindowFactory;

    public TrayApplicationContext(TypingSessionController controller, Func<SettingsWindow> settingsWindowFactory)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(settingsWindowFactory);

        _controller = controller;
        _settingsWindowFactory = settingsWindowFactory;

        _toggleMenuItem = new System.Windows.Forms.ToolStripMenuItem("Enabled", null, OnToggleClicked)
        {
            Checked = controller.IsEnabled,
        };

        var settingsMenuItem = new System.Windows.Forms.ToolStripMenuItem("Settings...", null, OnSettingsClicked);
        var exitMenuItem = new System.Windows.Forms.ToolStripMenuItem("Exit", null, OnExitClicked);

        var contextMenu = new System.Windows.Forms.ContextMenuStrip();
        contextMenu.Items.Add(_toggleMenuItem);
        contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        contextMenu.Items.Add(settingsMenuItem);
        contextMenu.Items.Add(exitMenuItem);

        _notifyIcon = new System.Windows.Forms.NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Text = "SinhalaInput",
            ContextMenuStrip = contextMenu,
            Visible = true,
        };

        _controller.EnabledChanged += OnControllerEnabledChanged;
    }

    private void OnToggleClicked(object? sender, EventArgs e) => _controller.ToggleEnabled();

    private void OnControllerEnabledChanged(object? sender, bool isEnabled) => _toggleMenuItem.Checked = isEnabled;

    private void OnSettingsClicked(object? sender, EventArgs e)
    {
        SettingsWindow window = _settingsWindowFactory();
        window.Show();
        window.Activate();
    }

    private void OnExitClicked(object? sender, EventArgs e) => System.Windows.Application.Current.Shutdown();

    public void Dispose()
    {
        _controller.EnabledChanged -= OnControllerEnabledChanged;
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }
}
