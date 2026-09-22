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

    [Theory]
    [InlineData("a", "අ")]
    [InlineData("aa", "ආ")]
    [InlineData("A", "ආ")]
    [InlineData("ae", "ඇ")]
    [InlineData("aae", "ඈ")]
    [InlineData("Ae", "ඈ")]
    [InlineData("i", "ඉ")]
    [InlineData("ii", "ඊ")]
    [InlineData("I", "ඊ")]
    [InlineData("u", "උ")]
    [InlineData("uu", "ඌ")]
    [InlineData("U", "ඌ")]
    [InlineData("e", "එ")]
    [InlineData("ee", "ඒ")]
    [InlineData("E", "ඒ")]
    [InlineData("ai", "ඓ")]
    [InlineData("o", "ඔ")]
    [InlineData("oo", "ඕ")]
    [InlineData("O", "ඕ")]
    [InlineData("au", "ඖ")]
    public void Transliterate_IndependentVowel_AtWordStart(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }

    [Theory]
    [InlineData("ka", "ක")] // "a" is the inherent vowel: no dependent sign is appended.
    [InlineData("kaa", "කා")]
    [InlineData("kA", "කා")]
    [InlineData("kae", "කැ")]
    [InlineData("kaae", "කෑ")]
    [InlineData("kAe", "කෑ")]
    [InlineData("ki", "කි")]
    [InlineData("kii", "කී")]
    [InlineData("kI", "කී")]
    [InlineData("ku", "කු")]
    [InlineData("kuu", "කූ")]
    [InlineData("kU", "කූ")]
    [InlineData("ke", "කෙ")]
    [InlineData("kee", "කේ")]
    [InlineData("kE", "කේ")]
    [InlineData("kai", "කෛ")]
    [InlineData("ko", "කො")]
    [InlineData("koo", "කෝ")]
    [InlineData("kO", "කෝ")]
    [InlineData("kau", "කෞ")]
    public void Transliterate_DependentVowelSign_AfterConsonant(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }

    [Theory]
    [InlineData("k", "ක")]
    [InlineData("kh", "ඛ")]
    [InlineData("g", "ග")]
    [InlineData("gh", "ඝ")]
    [InlineData("ng", "ඞ")]
    [InlineData("c", "ච")]
    [InlineData("ch", "ච")]
    [InlineData("chh", "ඡ")]
    [InlineData("j", "ජ")]
    [InlineData("jh", "ඣ")]
    [InlineData("ny", "ඤ")]
    [InlineData("t", "ට")] // bare "t" defaults to the retroflex sound.
    [InlineData("T", "ට")] // capitalisation convention: explicit retroflex.
    [InlineData("tt", "ට")] // doubled-letter alternative to capitalisation.
    [InlineData("th", "ත")] // dental, spelled with the digraph.
    [InlineData("d", "ද")] // bare "d" defaults to dental.
    [InlineData("dh", "ධ")]
    [InlineData("D", "ඩ")] // capitalisation convention: explicit retroflex.
    [InlineData("dd", "ඩ")] // doubled-letter alternative.
    [InlineData("N", "ණ")]
    [InlineData("nn", "ණ")]
    [InlineData("n", "න")]
    [InlineData("p", "ප")]
    [InlineData("ph", "ඵ")]
    [InlineData("f", "ෆ")]
    [InlineData("b", "බ")]
    [InlineData("bh", "භ")]
    [InlineData("m", "ම")]
    [InlineData("y", "ය")]
    [InlineData("r", "ර")]
    [InlineData("l", "ල")]
    [InlineData("L", "ළ")]
    [InlineData("ll", "ළ")]
    [InlineData("v", "ව")]
    [InlineData("w", "ව")]
    [InlineData("Sh", "ෂ")]
    [InlineData("ss", "ෂ")]
    [InlineData("sh", "ශ")]
    [InlineData("s", "ස")]
    [InlineData("h", "හ")]
    [InlineData("z", "ස")] // no native /z/ (design doc §3.2): mapped to ස.
    public void Transliterate_Consonant_KeepsInherentVowelWhenBare(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }

    [Theory]
    [InlineData("krama", "ක්‍රම")] // rakāraṃśaya: no vowel lengthening needed for this word.
    [InlineData("vyaapaaraya", "ව්‍යාපාරය")] // yansaya, reproduces the design doc's example exactly.
    public void Transliterate_RakaransayaAndYansayaConjuncts(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }

    // The design doc's own "vyaparaya -> ව්‍යාපාරය" example (§3.2) is a real-world Sinhala
    // word (ව්‍යාපාරය, "business") whose true pronunciation has two long "aa" vowels
    // ("vyaapaaraya"). A deterministic, no-dictionary engine cannot infer that length from
    // the shorter casual spelling "vyaparaya" alone (bare "a" is unambiguously the *short*
    // inherent vowel everywhere else in this rule set, e.g. "mama" -> "මම" not "මාමා") —
    // recovering the doc's exact glyphs requires the fully-lengthened spelling, tested above
    // as "vyaapaaraya". This test locks in what the engine deterministically produces for
    // the literal, casually-spelled Latin word: the yansaya conjunct still fires correctly,
    // just with short vowels throughout instead of the dictionary-only "correct" spelling.
    [Fact]
    public void Transliterate_YansayaConjunct_WithLiteralShortVowelSpelling()
    {
        Assert.Equal("ව්‍යපරය", _engine.Transliterate("vyaparaya"));
    }

    [Theory]
    [InlineData("kr", "ක්ර")] // "r" at the absolute end of the word has no following vowel:
    [InlineData("ky", "ක්ය")] // plain consonant cluster (virama), not a ZWJ conjunct.
    public void Transliterate_RakaransayaYansaya_RequiresFollowingVowel(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }

    [Theory]
    [InlineData("anda", "අන්ද")]
    [InlineData("kanda", "කන්ද")]
    [InlineData("chandra", "චන්ද්‍ර")] // combines a plain cluster (nd) and a rakāraṃśaya (dra).
    public void Transliterate_ConsonantCluster_InsertsVirama(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }

    // See the remark on FlushPendingConsonant: a trailing consonant with nothing after it
    // keeps its inherent vowel rather than getting a virama, which only ever appears when
    // another consonant (or a conjunct) immediately supersedes the pending one.
    [Theory]
    [InlineData("kan", "කන")]
    [InlineData("man", "මන")]
    public void Transliterate_WordFinalConsonant_KeepsInherentVowel(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }

    [Theory]
    [InlineData("ayubowan", "අයුබොවන")]
    [InlineData("aayuboowan", "ආයුබෝවන")]
    public void Transliterate_RealisticGreetingWords(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }

    [Theory]
    [InlineData("kata,mama!123", "කට,මම!123")]
    [InlineData("mama2024", "මම2024")]
    public void Transliterate_PassesThroughPunctuationAndDigitsMixedIntoAWord(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }
}
