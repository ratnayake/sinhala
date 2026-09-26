using Microsoft.Win32;

namespace SinhalaInput.App.Settings;

/// <summary>
/// Unpackaged start-at-sign-in via a per-user <c>Run</c> registry value, which needs no admin
/// rights. The key path and value name are injectable so tests can use a throwaway location.
/// </summary>
public sealed class RunKeyStartupRegistration : IStartupRegistration
{
    public const string DefaultRunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    public const string DefaultValueName = "EasyAkuru";

    /// <summary>The value name written by releases from before the app was renamed to EasyAkuru.</summary>
    public const string LegacyValueName = "SinhalaInput";

    private readonly string _runKeyPath;
    private readonly string _valueName;
    private readonly string _command;
    private readonly string[] _legacyValueNames;

    /// <param name="legacyValueNames">
    /// Value names from older releases that are removed whenever the registration is applied, so an
    /// upgraded install doesn't launch twice at sign-in.
    /// </param>
    public RunKeyStartupRegistration(
        string runKeyPath,
        string valueName,
        string executablePath,
        IEnumerable<string>? legacyValueNames = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(runKeyPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(valueName);
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
        _runKeyPath = runKeyPath;
        _valueName = valueName;
        _command = $"\"{executablePath}\"";
        _legacyValueNames = legacyValueNames?
            .Where(name => !string.IsNullOrWhiteSpace(name) && !string.Equals(name, valueName, StringComparison.OrdinalIgnoreCase))
            .ToArray() ?? [];
    }

    public bool IsManagedByWindows => false;

    /// <summary>True only when the value exists and points at this executable.</summary>
    public bool IsEnabled()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(_runKeyPath);
        return key?.GetValue(_valueName) is string command
            && string.Equals(command, _command, StringComparison.OrdinalIgnoreCase);
    }

    public void SetEnabled(bool enabled)
    {
        if (enabled)
        {
            if (!IsEnabled())
            {
                using RegistryKey key = Registry.CurrentUser.CreateSubKey(_runKeyPath);
                key.SetValue(_valueName, _command, RegistryValueKind.String);
            }

            DeleteValues(_legacyValueNames);
        }
        else
        {
            DeleteValues([_valueName, .. _legacyValueNames]);
        }
    }

    private void DeleteValues(string[] valueNames)
    {
        if (valueNames.Length == 0)
        {
            return;
        }

        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(_runKeyPath, writable: true);
        if (key is null)
        {
            return;
        }

        foreach (string valueName in valueNames)
        {
            key.DeleteValue(valueName, throwOnMissingValue: false);
        }
    }
}

/// <summary>
/// Packaged (MSIX) start-at-sign-in is declared by the manifest's <c>desktop:StartupTask</c>
/// and controlled by the user in Windows Settings, so there is nothing for the app to write.
/// </summary>
public sealed class PackagedStartupRegistration : IStartupRegistration
{
    public bool IsManagedByWindows => true;

    public void SetEnabled(bool enabled)
    {
    }
}
