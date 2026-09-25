using System.Reflection;

namespace SinhalaInput.App;

/// <summary>The text shown in the About window, kept separate from the XAML so it can be unit tested.</summary>
public sealed record AboutInfo(string ProductName, string Version, string Author, string Description)
{
    public const string AuthorName = "Isuru Ratnayake";

    public static AboutInfo Current { get; } = FromAssembly(typeof(AboutInfo).Assembly);

    public string VersionText => $"Version {Version}";

    public string AuthorText => $"Author: {Author}";

    public static AboutInfo FromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        return new AboutInfo(
            ProductName: "SinhalaInput",
            Version: GetDisplayVersion(assembly),
            Author: AuthorName,
            Description: "Type Sinhala anywhere in Windows using Latin (Singlish) keystrokes.");
    }

    private static string GetDisplayVersion(Assembly assembly)
    {
        // The SDK appends "+<commit sha>" to the informational version; that is noise for end users.
        string? informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(informational))
        {
            int metadataStart = informational.IndexOf('+', StringComparison.Ordinal);
            return metadataStart >= 0 ? informational[..metadataStart] : informational;
        }

        return assembly.GetName().Version?.ToString(3) ?? "unknown";
    }
}
