using Microsoft.Win32;
using SinhalaInput.App.Settings;

namespace SinhalaInput.App.Tests.Settings;

/// <summary>
/// Exercises the real registry, but under a throwaway HKCU subkey (never the actual Run key),
/// which is deleted afterwards.
/// </summary>
public sealed class RunKeyStartupRegistrationTests : IDisposable
{
    private const string ValueName = "SinhalaInput";
    private const string ExePath = @"C:\Program Files\SinhalaInput\SinhalaInput.App.exe";

    private const string ParentKeyPath = @"Software\SinhalaInput.Tests";

    private readonly string _keyPath = $@"{ParentKeyPath}\{Guid.NewGuid():N}";

    public void Dispose()
    {
        Registry.CurrentUser.DeleteSubKeyTree(_keyPath, throwOnMissingSubKey: false);

        using RegistryKey? parent = Registry.CurrentUser.OpenSubKey(ParentKeyPath);
        if (parent is not null && parent.SubKeyCount == 0 && parent.ValueCount == 0)
        {
            Registry.CurrentUser.DeleteSubKey(ParentKeyPath, throwOnMissingSubKey: false);
        }
    }

    private object? ReadValue()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(_keyPath);
        return key?.GetValue(ValueName);
    }

    [Fact]
    public void SetEnabled_True_WritesQuotedExePath()
    {
        var registration = new RunKeyStartupRegistration(_keyPath, ValueName, ExePath);

        registration.SetEnabled(true);

        Assert.Equal($"\"{ExePath}\"", ReadValue());
        Assert.True(registration.IsEnabled());
        Assert.False(registration.IsManagedByWindows);
    }

    [Fact]
    public void SetEnabled_False_RemovesValue()
    {
        var registration = new RunKeyStartupRegistration(_keyPath, ValueName, ExePath);
        registration.SetEnabled(true);

        registration.SetEnabled(false);

        Assert.Null(ReadValue());
        Assert.False(registration.IsEnabled());
    }

    [Fact]
    public void SetEnabled_False_WhenKeyMissing_DoesNotThrow()
    {
        var registration = new RunKeyStartupRegistration(_keyPath, ValueName, ExePath);

        registration.SetEnabled(false);

        Assert.False(registration.IsEnabled());
    }

    [Fact]
    public void SetEnabled_True_RepointsValueWhenExeMoved()
    {
        new RunKeyStartupRegistration(_keyPath, ValueName, @"C:\old\SinhalaInput.App.exe").SetEnabled(true);
        var moved = new RunKeyStartupRegistration(_keyPath, ValueName, ExePath);

        Assert.False(moved.IsEnabled());
        moved.SetEnabled(true);

        Assert.Equal($"\"{ExePath}\"", ReadValue());
    }
}
