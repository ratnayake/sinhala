using System.IO;
using System.Text.Json;

namespace SinhalaInput.App.Settings;

/// <summary>
/// Default <see cref="ISettingsStore"/>: a small JSON file. Like
/// <see cref="SinhalaInput.Core.Candidates.JsonUserDictionaryStore"/>, the path is supplied by
/// the caller so tests can use a throwaway file; production uses <see cref="GetDefaultFilePath"/>.
/// </summary>
public sealed class JsonSettingsStore : ISettingsStore
{
    private static readonly JsonSerializerOptions s_serializerOptions = new() { WriteIndented = true };

    private readonly string _filePath;
    private readonly Lock _gate = new();

    public JsonSettingsStore(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = filePath;
    }

    /// <summary>
    /// The per-user file location used in production (<c>%AppData%/SinhalaInput/settings.json</c>).
    /// </summary>
    public static string GetDefaultFilePath() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SinhalaInput", "settings.json");

    public UserSettings Load()
    {
        lock (_gate)
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    return new UserSettings();
                }

                string json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new UserSettings();
                }

                return JsonSerializer.Deserialize<UserSettings>(json, s_serializerOptions) ?? new UserSettings();
            }
            catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
            {
                // A broken settings file must never stop the input tool from starting; the next
                // save overwrites it with a valid one.
                return new UserSettings();
            }
        }
    }

    public void Save(UserSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        lock (_gate)
        {
            string? directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Write-then-replace so a crash mid-write can't leave a truncated file behind.
            string tempPath = _filePath + ".tmp";
            File.WriteAllText(tempPath, JsonSerializer.Serialize(settings, s_serializerOptions));
            File.Move(tempPath, _filePath, overwrite: true);
        }
    }
}
