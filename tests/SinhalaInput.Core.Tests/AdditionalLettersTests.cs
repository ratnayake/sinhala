using SinhalaInput.Core.Transliteration;

namespace SinhalaInput.Core.Tests;

// Every word here ends in a vowel so the expectations are independent of how the engine
// treats a word-final bare consonant.
public class AdditionalLettersTests
{
    private readonly TransliterationEngine _engine = new();

    [Theory]
    [InlineData("zga", "ඟ")]
    [InlineData("zja", "ඦ")]
    [InlineData("zDa", "ඬ")]
    [InlineData("zda", "ඳ")]
    [InlineData("zba", "ඹ")]
    [InlineData("sazdahaa", "සඳහා")]
    [InlineData("azba", "අඹ")]
    [InlineData("gazga", "ගඟ")]
    [InlineData("hazDa", "හඬ")]
    [InlineData("kazda", "කඳ")]
    public void Transliterate_SanyakaConsonants(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }

    [Theory]
    [InlineData("Tha", "ඨ")]
    [InlineData("Dha", "ඪ")]
    [InlineData("shreeShTha", "ශ්‍රේෂ්ඨ")]
    [InlineData("kaNTha", "කණ්ඨ")]
    [InlineData("guuDha", "ගූඪ")]
    public void Transliterate_RetroflexAspirates(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }

    [Theory]
    [InlineData("R", "ඍ")]
    [InlineData("RR", "ඎ")]
    [InlineData("RShi", "ඍෂි")]
    [InlineData("kRShi", "කෘෂි")]
    [InlineData("dRShTi", "දෘෂ්ටි")]
    [InlineData("mRgayaa", "මෘගයා")]
    [InlineData("kRR", "කෲ")]
    public void Transliterate_VocalicR(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }

    [Theory]
    [InlineData("jnya", "ඥ")]
    [InlineData("jnyaana", "ඥාන")]
    [InlineData("vijnyaana", "විඥාන")]
    public void Transliterate_Jnya(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }

    // Guards that the new multi-letter keys did not capture any spelling that already had a
    // meaning: each of these shares a prefix with one of the additions above.
    [Theory]
    [InlineData("tha", "ත")]
    [InlineData("dha", "ධ")]
    [InlineData("Ta", "ට")]
    [InlineData("Da", "ඩ")]
    [InlineData("TaDa", "ටඩ")]
    [InlineData("za", "ස")]
    [InlineData("zna", "ස්න")]
    [InlineData("ja", "ජ")]
    [InlineData("jna", "ජ්න")]
    [InlineData("rata", "රට")]
    [InlineData("krama", "ක්‍රම")]
    public void Transliterate_ExistingSpellingsUnchanged(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }
}
