using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SinhalaInput.App.Settings;
using SinhalaInput.Core.Candidates;
using SinhalaInput.Core.Transliteration;
using SinhalaInput.Platform.Windows.Caret;
using SinhalaInput.Platform.Windows.Focus;
using SinhalaInput.Platform.Windows.Hooking;
using SinhalaInput.Platform.Windows.Input;

namespace SinhalaInput.App;

/// <summary>
/// Composition root. This tool is tray-resident with no always-visible main window, so startup
/// is driven from here (rather than <c>StartupUri</c>) once the DI container is built.
/// </summary>
[SuppressMessage(
    "Design",
    "CA1001:Types that own disposable fields should be disposable",
    Justification = "System.Windows.Application is not meant to be IDisposable; its disposable " +
        "fields are cleaned up deterministically in OnExit, which always runs on shutdown.")]
public partial class App : System.Windows.Application
{
    private IHost? _host;
    private TrayApplicationContext? _trayContext;
    private CandidateWindow? _candidateWindow;
    private TypingSessionController? _controller;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<ITransliterationEngine, TransliterationEngine>();
                services.AddSingleton<IUserDictionaryStore>(
                    _ => new JsonUserDictionaryStore(JsonUserDictionaryStore.GetDefaultFilePath()));
                services.AddSingleton<ICandidateProvider, CandidateProvider>();
                services.AddSingleton<IKeyboardHook, LowLevelKeyboardHook>();
                services.AddSingleton<ITextInjector, SendInputTextInjector>();
                services.AddSingleton<ICaretLocator, Win32CaretLocator>();
                services.AddSingleton<IPasswordFieldDetector, Win32PasswordFieldDetector>();
                services.AddSingleton<TypingSessionController>();
                services.AddSingleton<CandidateWindow>();
                services.AddTransient<SettingsWindow>();
                services.AddSingleton<ISettingsStore>(
                    _ => new JsonSettingsStore(JsonSettingsStore.GetDefaultFilePath()));
                services.AddSingleton<IStartupRegistration>(_ => SinhalaInput.Platform.Windows.Packaging.PackageIdentity.IsPackaged
                    ? new PackagedStartupRegistration()
                    : new RunKeyStartupRegistration(
                        RunKeyStartupRegistration.DefaultRunKeyPath,
                        RunKeyStartupRegistration.DefaultValueName,
                        Environment.ProcessPath ?? System.IO.Path.Combine(AppContext.BaseDirectory, "SinhalaInput.App.exe")));
                services.AddSingleton<SettingsCoordinator>();
            })
            .Build();

        _host.Start();
        _host.Services.GetRequiredService<SettingsCoordinator>().Initialize();

        _controller = _host.Services.GetRequiredService<TypingSessionController>();
        _candidateWindow = _host.Services.GetRequiredService<CandidateWindow>();
        _controller.PopupStateChanged += OnPopupStateChanged;

        _trayContext = new TrayApplicationContext(
            _controller,
            () => _host.Services.GetRequiredService<SettingsWindow>());

        _host.Services.GetRequiredService<IKeyboardHook>().Start();
    }

    private void OnPopupStateChanged(object? sender, CandidatePopupState state) =>
        Dispatcher.BeginInvoke(() => _candidateWindow?.Render(state));

    protected override void OnExit(ExitEventArgs e)
    {
        if (_controller is not null)
        {
            _controller.PopupStateChanged -= OnPopupStateChanged;
        }

        _trayContext?.Dispose();
        _candidateWindow?.Close();
        _controller?.Dispose();

        if (_host is not null)
        {
            _host.Services.GetRequiredService<IKeyboardHook>().Dispose();
            _host.Dispose();
        }

        base.OnExit(e);
    }
}
