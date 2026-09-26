using System.Diagnostics;
using System.IO;

namespace SinhalaInput.App.Settings;

/// <summary>
/// Carries per-user data over from the folder used before the app was renamed to EasyAkuru.
/// Files are copied, never moved, so downgrading to an older release still finds its data.
/// </summary>
public static class LegacyDataMigration
{
    public const string DataFolderName = "EasyAkuru";

    public const string LegacyDataFolderName = "SinhalaInput";

    public static IReadOnlyList<string> FileNames { get; } = ["settings.json", "user-dictionary.json"];

    /// <summary>
    /// Copies each known data file from <c>&lt;baseDirectory&gt;\SinhalaInput</c> to
    /// <c>&lt;baseDirectory&gt;\EasyAkuru</c> unless the new file already exists. Must run before
    /// the stores load. Failures are swallowed: the app then just starts with defaults.
    /// </summary>
    /// <param name="baseDirectory">The folder both data folders live in (production: <c>%AppData%</c>).</param>
    /// <returns>The file names that were copied.</returns>
    public static IReadOnlyList<string> Migrate(string baseDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseDirectory);

        string legacyDirectory = Path.Combine(baseDirectory, LegacyDataFolderName);
        if (!Directory.Exists(legacyDirectory))
        {
            return [];
        }

        string newDirectory = Path.Combine(baseDirectory, DataFolderName);
        var copied = new List<string>();
        foreach (string fileName in FileNames)
        {
            string source = Path.Combine(legacyDirectory, fileName);
            string destination = Path.Combine(newDirectory, fileName);
            if (!File.Exists(source) || File.Exists(destination))
            {
                continue;
            }

            try
            {
                Directory.CreateDirectory(newDirectory);
                File.Copy(source, destination, overwrite: false);
                copied.Add(fileName);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Debug.WriteLine($"EasyAkuru: could not migrate {source}: {ex}");
            }
        }

        return copied;
    }
}
