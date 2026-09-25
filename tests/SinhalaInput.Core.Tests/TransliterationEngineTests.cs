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

    // Google/Helakuru convention: a consonant with no vowel after it takes the hal mark (virama);
    // the inherent vowel has to be typed explicitly as "a" (see the "ka" case above).
    [Theory]
    [InlineData("k", "ක්")]
    [InlineData("kh", "ඛ්")]
    [InlineData("g", "ග්")]
    [InlineData("gh", "ඝ්")]
    [InlineData("nG", "ඞ්")]
    [InlineData("c", "ච්")]
    [InlineData("ch", "ච්")]
    [InlineData("chh", "ඡ්")]
    [InlineData("j", "ජ්")]
    [InlineData("jh", "ඣ්")]
    [InlineData("nY", "ඤ්")]
    [InlineData("t", "ට්")] // bare "t" defaults to the retroflex sound.
    [InlineData("T", "ට්")] // capitalisation convention: explicit retroflex.
    [InlineData("th", "ත්")] // dental, spelled with the digraph.
    [InlineData("thh", "ථ්")] // dental aspirate, mirroring ch/chh.
    [InlineData("d", "ද්")] // bare "d" defaults to dental.
    [InlineData("dh", "ධ්")]
    [InlineData("D", "ඩ්")] // capitalisation convention: explicit retroflex.
    [InlineData("N", "ණ්")]
    [InlineData("n", "න්")]
    [InlineData("p", "ප්")]
    [InlineData("ph", "ඵ්")]
    [InlineData("f", "ෆ්")]
    [InlineData("b", "බ්")]
    [InlineData("bh", "භ්")]
    [InlineData("m", "ම්")]
    [InlineData("y", "ය්")]
    [InlineData("r", "ර්")]
    [InlineData("l", "ල්")]
    [InlineData("L", "ළ්")]
    [InlineData("v", "ව්")]
    [InlineData("w", "ව්")]
    [InlineData("Sh", "ෂ්")]
    [InlineData("sh", "ශ්")]
    [InlineData("s", "ස්")]
    [InlineData("h", "හ්")]
    [InlineData("z", "ස්")] // no native /z/ (design doc §3.2): mapped to ස.
    public void Transliterate_BareConsonant_TakesVirama(string latin, string expected)
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
    [InlineData("kr", "ක්ර්")] // "r" at the absolute end of the word has no following vowel:
    [InlineData("ky", "ක්ය්")] // plain consonant cluster (virama), not a ZWJ conjunct.
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

    // Regression coverage for the retroflex-shorthand/gemination collision (design doc §3.2):
    // RuleTable.Consonants used to also register "tt"/"dd"/"nn"/"ll"/"ss" as alternate spellings
    // of the retroflex letters T/D/N/L/Sh. Because RuleTrie.FindLongestMatch always prefers the
    // longer key, that shorthand always won over the *far* more common pattern it collided with:
    // a doubled consonant simply meaning gemination (hal kirīma + a repeat of the same
    // consonant), which the cluster logic in TransliterationEngine.Transliterate already
    // handles correctly for two *distinct* consonants (see the theory above). Removing the
    // shorthand lets that same cluster logic apply uniformly to a repeated letter too, with no
    // special-casing: each doubled letter here now resolves to <consonant>් + <consonant>්
    // (the second one word-final, so it takes its own virama too).
    [Theory]
    [InlineData("tt", "ට්ට්")] // "t" defaults to retroflex ට even bare, so this one is unchanged in glyph.
    [InlineData("dd", "ද්ද්")] // "d" (dental), not the retroflex ඩ the old shorthand produced.
    [InlineData("nn", "න්න්")] // "n" (dental), not the retroflex ණ the old shorthand produced.
    [InlineData("ll", "ල්ල්")] // "l" (plain), not the retroflex ළ the old shorthand produced.
    [InlineData("ss", "ස්ස්")] // "s" (plain), not the retroflex ෂ the old shorthand produced.
    public void Transliterate_DoubledConsonant_IsGeminationNotRetroflexShorthand(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }

    // Realistic colloquial words exercising the same gemination pattern end-to-end (not just the
    // bare doubled letter in isolation), across several different consonants, so this whole class
    // of bug is guarded against recurring for any consonant, not only the two originally reported.
    [Theory]
    [InlineData("malli", "මල්ලි")] // reported bug: used to produce "මළි" (ළ = retroflex l).
    [InlineData("akka", "අක්ක")] // "elder brother" (colloquial) — kk cluster.
    [InlineData("appa", "අප්ප")] // "father" (colloquial) — pp cluster.
    [InlineData("vatta", "වට්ට")] // tt cluster — used to collide with the T/tt retroflex shorthand.
    [InlineData("vissa", "විස්ස")] // "twenty" — ss cluster, used to collide with the Sh/ss shorthand.
    [InlineData("adda", "අද්ද")] // constructed word — dd cluster, used to collide with the D/dd shorthand.
    public void Transliterate_RealisticWords_WithConsonantGemination(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }

    // Reported bug: "enne" used to produce "එණෙ" (nn misread as the retroflex ණ instead of an
    // n+n cluster). The consonant cluster (න්න) is now unambiguously fixed. The word ends in a
    // vowel, so the word-final hal rule (see Transliterate_WordFinalConsonant_TakesVirama) does
    // not apply. Vowel length is a separate, harder question: the literal spelling "enne" has a
    // single final "e", which by this engine's own short/long vowel convention (bare letter = short, doubled/capitalised
    // = long — see the "vyaparaya" precedent above) deterministically yields the *short* vowel
    // sign ෙ, giving "එන්නෙ". The colloquially "correct" spelling of this word actually carries a
    // long final vowel (එන්නේ, spelled "ennee"/"ennE" in this scheme) — recovering that from the
    // bare "enne" spelling would require inferring vowel length the engine has no dictionary to
    // infer from, exactly like "vyaparaya" vs. "vyaapaaraya". This is judged out of scope for this
    // fix (see AmbiguousTokenSubstitutions, which targets single-token consonant ambiguity, not
    // word-final vowel length — extending it to offer "එන්නේ" as a candidate alternate would need
    // a position-sensitive substitution the mechanism doesn't support today); this test documents
    // the current, deterministic behaviour so it isn't silently swept under the rug.
    [Fact]
    public void Transliterate_Enne_FixesClusterButKeepsLiteralShortFinalVowel()
    {
        Assert.Equal("එන්නෙ", _engine.Transliterate("enne"));
    }

    // Google/Helakuru convention: a word-final consonant takes the hal mark (virama). Dead-consonant
    // endings like these are ~30-40% of words in real news text, so the inherent vowel is the one
    // that has to be spelled out ("kana" -> කන), not the virama.
    [Theory]
    [InlineData("kan", "කන්")]
    [InlineData("man", "මන්")]
    [InlineData("gaman", "ගමන්")]
    [InlineData("visin", "විසින්")]
    [InlineData("ekak", "එකක්")]
    [InlineData("eth", "එත්")]
    [InlineData("nam", "නම්")]
    [InlineData("kana", "කන")]
    public void Transliterate_WordFinalConsonant_TakesVirama(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }

    [Theory]
    [InlineData("ayubowan", "අයුබොවන්")]
    [InlineData("aayuboowan", "ආයුබෝවන්")]
    public void Transliterate_RealisticGreetingWords(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }

    [Theory]
    [InlineData("kata,mama!123", "කට,මම!123")]
    [InlineData("mama2024", "මම2024")]
    [InlineData("visin,", "විසින්,")] // a passthrough character ends the syllable like word end does.
    [InlineData("ekak.", "එකක්.")]
    [InlineData("nam2", "නම්2")]
    public void Transliterate_PassesThroughPunctuationAndDigitsMixedIntoAWord(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }

    // Anusvara (ං, U+0D82) attaches to the end of an already-vowelled syllable rather than
    // replacing a vowel (design doc §3.2 "Anusvara"). Capital "M" is its trigger because plain
    // "n" before a consonant already means a full consonant cluster with virama (see
    // Transliterate_ConsonantCluster_InsertsVirama's "anda" -> අන්ද below), so anusvara needs
    // its own unambiguous spelling that cannot collide with that existing meaning.
    [Theory]
    [InlineData("laMkaa", "ලංකා")] // Lanka.
    [InlineData("siMhala", "සිංහල")] // Sinhala -- the language's own name.
    [InlineData("beMgaala", "බෙංගාල")] // Bengal.
    public void Transliterate_Anusvara_AttachesToPrecedingSyllable(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }

    // Regression: plain "n" immediately before another consonant must still produce an
    // ordinary consonant cluster (virama), completely unaffected by the new "M" anusvara
    // trigger introduced above.
    [Fact]
    public void Transliterate_Anusvara_DoesNotAffectPlainNConsonantClusters()
    {
        Assert.Equal("අන්ද", _engine.Transliterate("anda"));
    }

    // Regression: sanity-check a couple of ordinary words untouched by the anusvara addition,
    // beyond relying on the full suite alone.
    [Theory]
    [InlineData("mama", "මම")]
    [InlineData("api", "අපි")]
    public void Transliterate_Anusvara_DoesNotAffectOrdinaryWords(string latin, string expected)
    {
        Assert.Equal(expected, _engine.Transliterate(latin));
    }
}
