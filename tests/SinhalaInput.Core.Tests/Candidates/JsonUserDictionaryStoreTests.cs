using SinhalaInput.Core.Candidates;

namespace SinhalaInput.Core.Tests.Candidates;

public class JsonUserDictionaryStoreTests : IDisposable
{
    private readonly string _filePath = Path.Combine(Path.GetTempPath(), $"sinhala-input-tests-{Guid.NewGuid():N}.json");

    [Fact]
    public void TryGetPreferredCandidate_WhenFileDoesNotExist_ReturnsNull()
    {
        var store = new JsonUserDictionaryStore(_filePath);

        Assert.Null(store.TryGetPreferredCandidate("mata"));
    }

    [Fact]
    public void SetPreferredCandidate_ThenGet_RoundTripsTheValue()
    {
        var store = new JsonUserDictionaryStore(_filePath);

        store.SetPreferredCandidate("mata", "මත");

        Assert.Equal("මත", store.TryGetPreferredCandidate("mata"));
    }

    [Fact]
    public void SetPreferredCandidate_PersistsAcrossStoreInstances()
    {
        new JsonUserDictionaryStore(_filePath).SetPreferredCandidate("mata", "මත");

        var reloaded = new JsonUserDictionaryStore(_filePath);

        Assert.Equal("මත", reloaded.TryGetPreferredCandidate("mata"));
    }

    [Fact]
    public void SetPreferredCandidate_OverwritesAPreviousChoiceForTheSameWord()
    {
        var store = new JsonUserDictionaryStore(_filePath);
        store.SetPreferredCandidate("mata", "මට");

        store.SetPreferredCandidate("mata", "මත");

        Assert.Equal("මත", store.TryGetPreferredCandidate("mata"));
    }

    [Fact]
    public void SetPreferredCandidate_CreatesTheContainingDirectoryIfMissing()
    {
        string nestedPath = Path.Combine(Path.GetTempPath(), $"sinhala-input-tests-{Guid.NewGuid():N}", "nested", "user-dictionary.json");
        var store = new JsonUserDictionaryStore(nestedPath);

        store.SetPreferredCandidate("mata", "මත");

        Assert.True(File.Exists(nestedPath));
        Directory.Delete(Path.GetDirectoryName(Path.GetDirectoryName(nestedPath))!, recursive: true);
    }

    [Fact]
    public void Constructor_NullOrWhitespaceFilePath_Throws()
    {
        Assert.Throws<ArgumentException>(() => new JsonUserDictionaryStore(" "));
    }

    [Fact]
    public void GetDefaultFilePath_ReturnsAPathUnderApplicationDataForEasyAkuru()
    {
        string path = JsonUserDictionaryStore.GetDefaultFilePath();

        Assert.Contains("EasyAkuru", path);
        Assert.EndsWith("user-dictionary.json", path);
    }

    public void Dispose()
    {
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }

        GC.SuppressFinalize(this);
    }
}
