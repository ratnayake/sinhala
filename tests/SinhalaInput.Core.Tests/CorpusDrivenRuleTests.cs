using SinhalaInput.Core.Transliteration;

namespace SinhalaInput.Core.Tests;

// Rules added after round-tripping the Lankadeepa corpus (roundtrip-test/lankadeepa/CHECKLIST.md).
public class CorpusDrivenRuleTests
{
    private readonly TransliterationEngine _engine = new();

    [Theory]
    [InlineData("thha", "ථ")]
    [InlineData("sthhaanaya", "ස්ථානය")]
    [InlineData("aarthhika", "ආර්ථික")]
    [InlineData("avasthhaa", "අවස්ථා")]
    [InlineData("prathhama", "ප්‍රථම")]
    [InlineData("vyavasthhaa", "ව්‍යවස්ථා")]
    [InlineData("madhyasthhaana", "මධ්‍යස්ථාන")]
    public void Transliterate_DentalAspirateTha(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }

    [Theory]
    [InlineData("ballangee", "බල්ලන්ගේ")]
    [InlineData("lengathuma", "ලෙන්ගතුම")]
    [InlineData("thamangee", "තමන්ගේ")]
    [InlineData("saamaanya", "සාමාන්‍ය")]
    [InlineData("nyaaya", "න්‍යාය")]
    [InlineData("nGa", "ඞ")]
    [InlineData("nYaaNa", "ඤාණ")]
    [InlineData("jnyaana", "ඥාන")]
    public void Transliterate_NPlusGOrYIsAnOrdinaryCluster(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }

    [Theory]
    [InlineData("kaarya", "කාර්ය")]
    [InlineData("aachaarya", "ආචාර්ය")]
    [InlineData("suurya", "සූර්ය")]
    [InlineData("kaaryasaadhaka", "කාර්යසාධක")]
    [InlineData("vyaapaaraya", "ව්‍යාපාරය")]
    [InlineData("krama", "ක්‍රම")]
    public void Transliterate_RaPlusYaHasNoYansaya(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }

    [Theory]
    [InlineData("kalqyaama", "කල්යාම")]
    [InlineData("kalyaaNa", "කල්‍යාණ")]
    [InlineData("bavathqya", "බවත්ය")]
    [InlineData("avasanqya", "අවසන්ය")]
    [InlineData("vagaquththarakaru", "වගඋත්තරකරු")]
    [InlineData("vagauththarakaru", "වගෞත්තරකරු")]
    [InlineData("kaqi", "කඉ")]
    [InlineData("kqa", "ක්අ")]
    [InlineData("kq", "ක්")]
    [InlineData("q", "")]
    public void Transliterate_SyllableBreakEndsTheSyllableAndEmitsNothing(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }

    [Theory]
    [InlineData("25000k", "25000ක්")]
    [InlineData("2k", "2ක්")]
    public void Transliterate_DigitsPassThroughBeforeSinhala(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }
}
