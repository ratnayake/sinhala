using Microsoft.Win32;
using SinhalaInput.App.Settings;

namespace SinhalaInput.App.Tests.Settings;

/// <summary>
/// Exercises the real registry, but under a throwaway HKCU subkey (never the actual Run key),
/// which is deleted afterwards.
/// </summary>
public sealed class RunKeyStartupRegistrationTests : IDisposable
{
    private const string ValueName = "EasyAkuru";
    private const string LegacyValueName = "SinhalaInput";
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

    private object? ReadValue(string valueName = ValueName)
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(_keyPath);
        return key?.GetValue(valueName);
    }

    private void WriteLegacyValue()
    {
        using RegistryKey key = Registry.CurrentUser.CreateSubKey(_keyPath);
        key.SetValue(LegacyValueName, @"""C:\old\SinhalaInput.App.exe""", RegistryValueKind.String);
    }

    private RunKeyStartupRegistration CreateWithLegacy() =>
        new(_keyPath, ValueName, ExePath, [LegacyValueName]);

    [Fact]
    public void Defaults_UseEasyAkuruValueAndSinhalaInputLegacyValue()
    {
        Assert.Equal("EasyAkuru", RunKeyStartupRegistration.DefaultValueName);
        Assert.Equal("SinhalaInput", RunKeyStartupRegistration.LegacyValueName);
    }

    [Fact]
    public void SetEnabled_True_RemovesLegacyValue()
    {
        WriteLegacyValue();

        CreateWithLegacy().SetEnabled(true);

        Assert.Equal($"\"{ExePath}\"", ReadValue());
        Assert.Null(ReadValue(LegacyValueName));
    }

    [Fact]
    public void SetEnabled_True_WhenAlreadyEnabled_StillRemovesLegacyValue()
    {
        CreateWithLegacy().SetEnabled(true);
        WriteLegacyValue();

        CreateWithLegacy().SetEnabled(true);

        Assert.Equal($"\"{ExePath}\"", ReadValue());
        Assert.Null(ReadValue(LegacyValueName));
    }

    [Fact]
    public void SetEnabled_False_RemovesCurrentAndLegacyValues()
    {
        var registration = CreateWithLegacy();
        registration.SetEnabled(true);
        WriteLegacyValue();

        registration.SetEnabled(false);

        Assert.Null(ReadValue());
        Assert.Null(ReadValue(LegacyValueName));
    }

    [Fact]
    public void SetEnabled_WithoutLegacyNames_LeavesOtherValuesAlone()
    {
        WriteLegacyValue();

        var registration = new RunKeyStartupRegistration(_keyPath, ValueName, ExePath);
        registration.SetEnabled(true);
        registration.SetEnabled(false);

        Assert.NotNull(ReadValue(LegacyValueName));
    }

    [Fact]
    public void Constructor_LegacyNameEqualToValueName_IsIgnored()
    {
        var registration = new RunKeyStartupRegistration(_keyPath, ValueName, ExePath, [ValueName]);

        registration.SetEnabled(true);

        Assert.True(registration.IsEnabled());
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
