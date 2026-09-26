using System.Text.Json;

namespace SinhalaInput.Core.Candidates;

/// <summary>
/// Default <see cref="IUserDictionaryStore"/>: a small JSON file mapping each learned Latin
/// word to the Sinhala rendering the user picked for it.
/// </summary>
/// <remarks>
/// The file path is always supplied by the caller rather than looked up internally, so this
/// type is unit-testable against a throwaway path without touching the real per-user profile.
/// Production callers should point it at <see cref="GetDefaultFilePath"/>.
/// </remarks>
public sealed class JsonUserDictionaryStore : IUserDictionaryStore
{
    private readonly string _filePath;
    private readonly Lock _gate = new();

    public JsonUserDictionaryStore(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = filePath;
    }

    /// <summary>
    /// The per-user file location used in production
    /// (<c>%AppData%/EasyAkuru/user-dictionary.json</c> on Windows). Unit tests should
    /// inject their own path instead of calling this.
    /// </summary>
    public static string GetDefaultFilePath() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasyAkuru", "user-dictionary.json");

    public string? TryGetPreferredCandidate(string latinWord)
    {
        ArgumentNullException.ThrowIfNull(latinWord);
        lock (_gate)
        {
            return Load().GetValueOrDefault(latinWord);
        }
    }

    public void SetPreferredCandidate(string latinWord, string sinhalaCandidate)
    {
        ArgumentNullException.ThrowIfNull(latinWord);
        ArgumentNullException.ThrowIfNull(sinhalaCandidate);
        lock (_gate)
        {
            Dictionary<string, string> entries = Load();
            entries[latinWord] = sinhalaCandidate;
            Save(entries);
        }
    }

    private Dictionary<string, string> Load()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        string json = File.ReadAllText(_filePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
    }

    private void Save(Dictionary<string, string> entries)
    {
        string? directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(_filePath, JsonSerializer.Serialize(entries));
    }
}
