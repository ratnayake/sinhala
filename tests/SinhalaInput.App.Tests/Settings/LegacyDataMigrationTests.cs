using SinhalaInput.App.Settings;

namespace SinhalaInput.App.Tests.Settings;

public sealed class LegacyDataMigrationTests : IDisposable
{
    private readonly string _baseDirectory = Path.Combine(Path.GetTempPath(), "SinhalaInput.Tests", Guid.NewGuid().ToString("N"));

    private string LegacyDirectory => Path.Combine(_baseDirectory, "SinhalaInput");

    private string NewDirectory => Path.Combine(_baseDirectory, "EasyAkuru");

    public void Dispose()
    {
        if (Directory.Exists(_baseDirectory))
        {
            Directory.Delete(_baseDirectory, recursive: true);
        }
    }

    private void WriteLegacy(string fileName, string content)
    {
        Directory.CreateDirectory(LegacyDirectory);
        File.WriteAllText(Path.Combine(LegacyDirectory, fileName), content);
    }

    [Fact]
    public void Migrate_CopiesLegacyFilesWhenNewOnesAreMissing_AndKeepsTheOldFolder()
    {
        WriteLegacy("settings.json", "{\"IsEnabled\":false}");
        WriteLegacy("user-dictionary.json", "{\"amma\":\"අම්මා\"}");

        IReadOnlyList<string> copied = LegacyDataMigration.Migrate(_baseDirectory);

        Assert.Equal(["settings.json", "user-dictionary.json"], copied);
        Assert.Equal("{\"IsEnabled\":false}", File.ReadAllText(Path.Combine(NewDirectory, "settings.json")));
        Assert.Equal("{\"amma\":\"අම්මා\"}", File.ReadAllText(Path.Combine(NewDirectory, "user-dictionary.json")));
        Assert.True(File.Exists(Path.Combine(LegacyDirectory, "settings.json")));
        Assert.True(File.Exists(Path.Combine(LegacyDirectory, "user-dictionary.json")));
    }

    [Fact]
    public void Migrate_DoesNotOverwriteExistingNewFiles()
    {
        WriteLegacy("settings.json", "legacy");
        WriteLegacy("user-dictionary.json", "legacy-dictionary");
        Directory.CreateDirectory(NewDirectory);
        File.WriteAllText(Path.Combine(NewDirectory, "settings.json"), "current");

        IReadOnlyList<string> copied = LegacyDataMigration.Migrate(_baseDirectory);

        Assert.Equal(["user-dictionary.json"], copied);
        Assert.Equal("current", File.ReadAllText(Path.Combine(NewDirectory, "settings.json")));
        Assert.Equal("legacy-dictionary", File.ReadAllText(Path.Combine(NewDirectory, "user-dictionary.json")));
    }

    [Fact]
    public void Migrate_OnlyCopiesLegacyFilesThatExist()
    {
        WriteLegacy("settings.json", "legacy");

        IReadOnlyList<string> copied = LegacyDataMigration.Migrate(_baseDirectory);

        Assert.Equal(["settings.json"], copied);
        Assert.False(File.Exists(Path.Combine(NewDirectory, "user-dictionary.json")));
    }

    [Fact]
    public void Migrate_NoLegacyFolder_IsNoOpAndCreatesNothing()
    {
        Directory.CreateDirectory(_baseDirectory);

        IReadOnlyList<string> copied = LegacyDataMigration.Migrate(_baseDirectory);

        Assert.Empty(copied);
        Assert.False(Directory.Exists(NewDirectory));
    }

    [Fact]
    public void Migrate_RunTwice_SecondRunCopiesNothing()
    {
        WriteLegacy("settings.json", "legacy");
        LegacyDataMigration.Migrate(_baseDirectory);
        File.WriteAllText(Path.Combine(NewDirectory, "settings.json"), "changed after migration");

        IReadOnlyList<string> copied = LegacyDataMigration.Migrate(_baseDirectory);

        Assert.Empty(copied);
        Assert.Equal("changed after migration", File.ReadAllText(Path.Combine(NewDirectory, "settings.json")));
    }

    [Fact]
    public void Migrate_BlankBaseDirectory_Throws()
    {
        Assert.Throws<ArgumentException>(() => LegacyDataMigration.Migrate(" "));
    }
}
