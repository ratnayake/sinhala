using SinhalaInput.Core.Transliteration;

namespace SinhalaInput.Core.Tests;

public class TransliterationEngineTests
{
    private readonly TransliterationEngine _engine = new();

    [Theory]
    [InlineData("mama", "මම")]
    [InlineData("api", "අපි")]
    [InlineData("kohomada", "කොහොමද")]
    public void Transliterate_ProducesExpectedSinhala(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }
}
