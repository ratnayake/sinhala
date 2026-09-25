namespace SinhalaInput.Core.Transliteration;

/// <summary>
/// The Latin-to-Sinhala phonetic rule set (see docs/SINHALA-INPUT-TOOL-DESIGN.md §3.2).
/// Longer Latin keys must be checked before their shorter prefixes; <see cref="RuleTrie"/>
/// guarantees that ordering, so the order of the lists below is not itself significant.
/// </summary>
public static class RuleTable
{
    public static readonly IReadOnlyList<SyllableRule> IndependentVowels =
    [
        new("a", TokenKind.IndependentVowel, "අ"),
        new("aa", TokenKind.IndependentVowel, "ආ"),
        new("A", TokenKind.IndependentVowel, "ආ"),
        new("ae", TokenKind.IndependentVowel, "ඇ"),
        new("aae", TokenKind.IndependentVowel, "ඈ"),
        new("Ae", TokenKind.IndependentVowel, "ඈ"),
        new("i", TokenKind.IndependentVowel, "ඉ"),
        new("ii", TokenKind.IndependentVowel, "ඊ"),
        new("I", TokenKind.IndependentVowel, "ඊ"),
        new("u", TokenKind.IndependentVowel, "උ"),
        new("uu", TokenKind.IndependentVowel, "ඌ"),
        new("U", TokenKind.IndependentVowel, "ඌ"),
        new("e", TokenKind.IndependentVowel, "එ"),
        new("ee", TokenKind.IndependentVowel, "ඒ"),
        new("E", TokenKind.IndependentVowel, "ඒ"),
        new("ai", TokenKind.IndependentVowel, "ඓ"),
        new("o", TokenKind.IndependentVowel, "ඔ"),
        new("oo", TokenKind.IndependentVowel, "ඕ"),
        new("O", TokenKind.IndependentVowel, "ඕ"),
        new("au", TokenKind.IndependentVowel, "ඖ"),
        new("R", TokenKind.IndependentVowel, "ඍ"), // U+0D8D SINHALA LETTER IRUYANNA
        new("RR", TokenKind.IndependentVowel, "ඎ"), // U+0D8E SINHALA LETTER IRUUYANNA
    ];

    public static readonly IReadOnlyList<SyllableRule> DependentVowelSigns =
    [
        // "a" is intentionally absent: the inherent vowel needs no sign.
        new("aa", TokenKind.DependentVowelSign, "ා"), // ා
        new("A", TokenKind.DependentVowelSign, "ා"),
        new("ae", TokenKind.DependentVowelSign, "ැ"), // ැ
        new("aae", TokenKind.DependentVowelSign, "ෑ"), // ෑ
        new("Ae", TokenKind.DependentVowelSign, "ෑ"),
        new("i", TokenKind.DependentVowelSign, "ි"), // ි
        new("ii", TokenKind.DependentVowelSign, "ී"), // ී
        new("I", TokenKind.DependentVowelSign, "ී"),
        new("u", TokenKind.DependentVowelSign, "ු"), // ු
        new("uu", TokenKind.DependentVowelSign, "ූ"), // ූ
        new("U", TokenKind.DependentVowelSign, "ූ"),
        new("e", TokenKind.DependentVowelSign, "ෙ"), // ෙ
        new("ee", TokenKind.DependentVowelSign, "ේ"), // ේ
        new("E", TokenKind.DependentVowelSign, "ේ"),
        new("ai", TokenKind.DependentVowelSign, "ෛ"), // ෛ
        new("o", TokenKind.DependentVowelSign, "ො"), // ො
        new("oo", TokenKind.DependentVowelSign, "ෝ"), // ෝ
        new("O", TokenKind.DependentVowelSign, "ෝ"),
        new("au", TokenKind.DependentVowelSign, "ෞ"), // ෞ
        new("R", TokenKind.DependentVowelSign, "ෘ"), // U+0DD8 SINHALA VOWEL SIGN GAETTA-PILLA
        new("RR", TokenKind.DependentVowelSign, "ෲ"), // U+0DF2 SINHALA VOWEL SIGN DIGA GAETTA-PILLA
    ];

    public static readonly IReadOnlyList<SyllableRule> Consonants =
    [
        new("k", TokenKind.Consonant, "ක"),
        new("kh", TokenKind.Consonant, "ඛ"),
        new("g", TokenKind.Consonant, "ග"),
        new("gh", TokenKind.Consonant, "ඝ"),
        // "nG"/"nY" rather than "ng"/"ny": plain n+g and n+y are ordinary clusters (බල්ලන්ගේ,
        // සාමාන්‍ය) and far more common than ඞ/ඤ, so they must keep the regular cluster reading.
        new("nG", TokenKind.Consonant, "ඞ"),
        new("ch", TokenKind.Consonant, "ච"),
        new("c", TokenKind.Consonant, "ච"),
        new("chh", TokenKind.Consonant, "ඡ"),
        new("j", TokenKind.Consonant, "ජ"),
        new("jh", TokenKind.Consonant, "ඣ"),
        new("nY", TokenKind.Consonant, "ඤ"),
        new("jny", TokenKind.Consonant, "ඥ"), // U+0DA5; kept as "jny" (not "jnY") because ජ්න්‍ය never occurs.
        new("T", TokenKind.Consonant, "ට"),
        new("t", TokenKind.Consonant, "ට"),
        new("th", TokenKind.Consonant, "ත"),
        new("thh", TokenKind.Consonant, "ථ"), // U+0DAE; mirrors ch/chh. The ත්හ cluster it supersedes never occurs.
        // "Th"/"Dh" deliberately supersede the former T+h / D+h cluster readings (ට්හ / ඩ්හ),
        // which essentially never occur in Sinhala, while ඨ/ඪ appear in common tatsama words.
        new("Th", TokenKind.Consonant, "ඨ"), // U+0DA8
        new("d", TokenKind.Consonant, "ද"),
        new("dh", TokenKind.Consonant, "ධ"),
        new("D", TokenKind.Consonant, "ඩ"),
        new("Dh", TokenKind.Consonant, "ඪ"), // U+0DAA
        new("N", TokenKind.Consonant, "ණ"),
        new("n", TokenKind.Consonant, "න"),
        new("p", TokenKind.Consonant, "ප"),
        new("ph", TokenKind.Consonant, "ඵ"),
        new("f", TokenKind.Consonant, "ෆ"),
        new("b", TokenKind.Consonant, "බ"),
        new("bh", TokenKind.Consonant, "භ"),
        new("m", TokenKind.Consonant, "ම"),
        new("y", TokenKind.Consonant, "ය"),
        new("r", TokenKind.Consonant, "ර"),
        new("l", TokenKind.Consonant, "ල"),
        new("L", TokenKind.Consonant, "ළ"),
        new("v", TokenKind.Consonant, "ව"),
        new("w", TokenKind.Consonant, "ව"),
        new("Sh", TokenKind.Consonant, "ෂ"),
        new("sh", TokenKind.Consonant, "ශ"),
        new("s", TokenKind.Consonant, "ස"),
        new("h", TokenKind.Consonant, "හ"),
        new("z", TokenKind.Consonant, "ස"), // no native /z/ in Sinhala; §3.2 maps it to ස.
        // Sanyaka (prenasalized) consonants. "z" is free as a prefix because it has no sound of
        // its own in Sinhala, and "n"+stop is already taken by the ordinary cluster (අන්ද).
        new("zg", TokenKind.Consonant, "ඟ"), // U+0D9F
        new("zj", TokenKind.Consonant, "ඦ"), // U+0DA6
        new("zD", TokenKind.Consonant, "ඬ"), // U+0DAC
        new("zd", TokenKind.Consonant, "ඳ"), // U+0DB3
        new("zb", TokenKind.Consonant, "ඹ"), // U+0DB9
    ];

    public const string Virama = "්"; // ් — SINHALA SIGN AL-LAKUNA
    public const string ZeroWidthJoiner = "‍";
    public const string RakaransayaTail = "ර"; // conjunct 'r' glyph used after virama+ZWJ
    public const string YansayaTail = "ය"; // conjunct 'y' glyph used after virama+ZWJ
    public const string Anusvara = "ං"; // ං — U+0D82 SINHALA SIGN ANUSVARAYA

    // Emits nothing; it only ends the current syllable, exactly as a passthrough character
    // would. Needed where the default reading is a real word too and only a dictionary could
    // choose: කල්යාම ("kalqyaama", not the yansaya of කල්‍යාණ) and hiatus like වගඋත්තරකරු
    // ("vagaquththarakaru", not the ෞ sign). "q" because the typing session only buffers
    // letters and "q" has no Sinhala reading; "x" is left free for a future ක්ෂ.
    public const char SyllableBreak = 'q';

    // Anusvara attaches to the end of an already-vowelled syllable rather than replacing a
    // vowel, so it is matched by its own trie (TransliterationEngine._anusvara), checked
    // unconditionally each iteration instead of only when a consonant is pending like the
    // dependent vowel signs above. Capital "M" was chosen as the trigger, following this rule
    // set's existing capitalisation convention for otherwise-ambiguous sounds (T/D/N/L/Sh,
    // A/I/U/E/O): plain "n" before a consonant already means a full consonant cluster with
    // virama (e.g. "anda" -> අන්ද), so anusvara needs a trigger that cannot collide with it.
    public static readonly IReadOnlyList<SyllableRule> AnusvaraRules =
    [
        new("M", TokenKind.Anusvara, Anusvara),
    ];

    public static IEnumerable<SyllableRule> All =>
        Consonants.Concat(IndependentVowels).Concat(DependentVowelSigns).Concat(AnusvaraRules);
}
