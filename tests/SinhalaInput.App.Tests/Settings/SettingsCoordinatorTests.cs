using SinhalaInput.App.Settings;

namespace SinhalaInput.App.Tests.Settings;

public sealed class SettingsCoordinatorTests
{
    private readonly ControllerHarness _harness = new();
    private readonly InMemorySettingsStore _store = new();
    private readonly FakeStartupRegistration _startup = new();

    private SettingsCoordinator CreateCoordinator() =>
        new(_harness.Controller, _store, _startup, scheduleSave: save => save());

    [Fact]
    public void Initialize_FirstRun_KeepsEnabledRegistersStartupAndWritesDefaults()
    {
        using SettingsCoordinator coordinator = CreateCoordinator();

        coordinator.Initialize();

        Assert.True(_harness.Controller.IsEnabled);
        Assert.True(_startup.Enabled);
        Assert.True(coordinator.StartWithWindows);
        Assert.Equal(new UserSettings(), _store.Saved);
    }

    [Fact]
    public void Initialize_SavedDisabled_DisablesControllerOnce()
    {
        _store.Saved = new UserSettings { IsEnabled = false };
        using SettingsCoordinator coordinator = CreateCoordinator();
        int toggles = 0;
        _harness.Controller.EnabledChanged += (_, _) => toggles++;

        coordinator.Initialize();
        coordinator.Initialize();

        Assert.False(_harness.Controller.IsEnabled);
        Assert.Equal(1, toggles);
    }

    [Fact]
    public void Initialize_SavedStartWithWindowsOff_UnregistersStartup()
    {
        _store.Saved = new UserSettings { StartWithWindows = false };
        _startup.Enabled = true;
        using SettingsCoordinator coordinator = CreateCoordinator();

        coordinator.Initialize();

        Assert.False(_startup.Enabled);
        Assert.False(coordinator.StartWithWindows);
    }

    [Fact]
    public void ControllerToggle_SavesNewEnabledState()
    {
        using SettingsCoordinator coordinator = CreateCoordinator();
        coordinator.Initialize();

        _harness.Controller.ToggleEnabled();
        Assert.False(_store.Saved!.IsEnabled);

        _harness.Controller.ToggleEnabled();
        Assert.True(_store.Saved!.IsEnabled);
    }

    [Fact]
    public void SetStartWithWindows_UpdatesRegistrationAndSaves()
    {
        using SettingsCoordinator coordinator = CreateCoordinator();
        coordinator.Initialize();

        coordinator.SetStartWithWindows(false);

        Assert.False(_startup.Enabled);
        Assert.False(_store.Saved!.StartWithWindows);
        Assert.False(coordinator.StartWithWindows);
    }

    [Fact]
    public void ManagedByWindows_NeverTouchesRegistration()
    {
        _startup.IsManagedByWindows = true;
        _store.Saved = new UserSettings { StartWithWindows = false };
        using SettingsCoordinator coordinator = CreateCoordinator();

        coordinator.Initialize();

        Assert.Equal(0, _startup.SetEnabledCalls);
        Assert.True(coordinator.IsStartupManagedByWindows);
    }

    [Fact]
    public void RegistrationFailure_DoesNotPreventStartup()
    {
        _startup.ThrowOnSet = true;
        using SettingsCoordinator coordinator = CreateCoordinator();

        coordinator.Initialize();

        Assert.NotNull(_store.Saved);
    }

    [Fact]
    public void Dispose_StopsTrackingController()
    {
        SettingsCoordinator coordinator = CreateCoordinator();
        coordinator.Initialize();
        coordinator.Dispose();
        int savesAfterDispose = _store.SaveCount;

        _harness.Controller.ToggleEnabled();

        Assert.Equal(savesAfterDispose, _store.SaveCount);
    }

    private sealed class InMemorySettingsStore : ISettingsStore
    {
        public UserSettings? Saved { get; set; }

        public int SaveCount { get; private set; }

        public UserSettings Load() => Saved ?? new UserSettings();

        public void Save(UserSettings settings)
        {
            Saved = settings;
            SaveCount++;
        }
    }

    private sealed class FakeStartupRegistration : IStartupRegistration
    {
        public bool Enabled { get; set; }

        public bool ThrowOnSet { get; set; }

        public int SetEnabledCalls { get; private set; }

        public bool IsManagedByWindows { get; set; }

        public void SetEnabled(bool enabled)
        {
            SetEnabledCalls++;
            if (ThrowOnSet)
            {
                throw new UnauthorizedAccessException("policy");
            }

            Enabled = enabled;
        }
    }
}
