using SinhalaInput.App.Settings;

namespace SinhalaInput.App.Tests.Settings;

public sealed class JsonSettingsStoreTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "SinhalaInput.Tests", Guid.NewGuid().ToString("N"));

    private string FilePath => Path.Combine(_directory, "nested", "settings.json");

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public void Load_MissingFile_ReturnsDefaults()
    {
        var store = new JsonSettingsStore(FilePath);

        UserSettings settings = store.Load();

        Assert.True(settings.IsEnabled);
        Assert.True(settings.StartWithWindows);
    }

    [Fact]
    public void Save_ThenLoad_RoundTrips()
    {
        var saved = new UserSettings { IsEnabled = false, StartWithWindows = false };

        new JsonSettingsStore(FilePath).Save(saved);
        UserSettings loaded = new JsonSettingsStore(FilePath).Load();

        Assert.Equal(saved, loaded);
        Assert.False(File.Exists(FilePath + ".tmp"));
    }

    [Theory]
    [InlineData("{ this is not json")]
    [InlineData("[1, 2, 3]")]
    [InlineData("")]
    [InlineData("null")]
    public void Load_CorruptFile_ReturnsDefaults(string contents)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, contents);

        UserSettings settings = new JsonSettingsStore(FilePath).Load();

        Assert.Equal(new UserSettings(), settings);
    }

    [Fact]
    public void Load_FileMissingAProperty_UsesDefaultForIt()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, """{ "IsEnabled": false }""");

        UserSettings settings = new JsonSettingsStore(FilePath).Load();

        Assert.False(settings.IsEnabled);
        Assert.True(settings.StartWithWindows);
    }

    [Fact]
    public void Save_OverwritesCorruptFile()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, "garbage");
        var store = new JsonSettingsStore(FilePath);

        store.Save(new UserSettings { IsEnabled = false });

        Assert.False(store.Load().IsEnabled);
    }
}
