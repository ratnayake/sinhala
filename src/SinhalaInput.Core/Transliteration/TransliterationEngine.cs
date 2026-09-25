using System.Text;

namespace SinhalaInput.Core.Transliteration;

/// <summary>
/// Deterministic, syllable-oriented Latin-to-Sinhala transliteration engine.
/// See docs/SINHALA-INPUT-TOOL-DESIGN.md §3.3 for the algorithm this implements.
/// </summary>
public sealed class TransliterationEngine : ITransliterationEngine
{
    private readonly RuleTrie _consonants;
    private readonly RuleTrie _independentVowels;
    private readonly RuleTrie _dependentVowelSigns;
    private readonly RuleTrie _anusvara;

    public TransliterationEngine()
        : this(RuleTable.Consonants, RuleTable.IndependentVowels, RuleTable.DependentVowelSigns)
    {
    }

    // Internal constructor seam for unit tests (and CandidateProvider's alternate-spelling
    // engines) that need a substituted rule set. `anusvara` defaults to RuleTable.AnusvaraRules
    // (a compile-time-constant default isn't possible here since it's a static readonly list,
    // not a literal) so existing 3-argument call sites keep compiling unchanged.
    internal TransliterationEngine(
        IEnumerable<SyllableRule> consonants,
        IEnumerable<SyllableRule> independentVowels,
        IEnumerable<SyllableRule> dependentVowelSigns,
        IEnumerable<SyllableRule>? anusvara = null)
    {
        _consonants = new RuleTrie(consonants);
        _independentVowels = new RuleTrie(independentVowels);
        _dependentVowelSigns = new RuleTrie(dependentVowelSigns);
        _anusvara = new RuleTrie(anusvara ?? RuleTable.AnusvaraRules);
    }

    public string Transliterate(string latinWord)
    {
        ArgumentNullException.ThrowIfNull(latinWord);
        if (latinWord.Length == 0)
        {
            return string.Empty;
        }

        var result = new StringBuilder(latinWord.Length * 2);
        string? pendingConsonantGlyph = null;
        int i = 0;

        while (i < latinWord.Length)
        {
            // Anusvara (ං) attaches to the end of a syllable that already has its vowel
            // (inherent or explicit) rather than replacing one, so — unlike the dependent
            // vowel signs below, which only apply while a consonant is pending — it must be
            // recognised on every iteration regardless of pendingConsonantGlyph's state.
            if (_anusvara.FindLongestMatch(latinWord, i) is { } anusvaraMatch)
            {
                if (pendingConsonantGlyph is not null)
                {
                    result.Append(pendingConsonantGlyph).Append(RuleTable.Anusvara[0]);
                    pendingConsonantGlyph = null;
                }
                else
                {
                    result.Append(RuleTable.Anusvara[0]);
                }

                i += anusvaraMatch.Latin.Length;
                continue;
            }

            if (latinWord[i] == RuleTable.SyllableBreak)
            {
                FlushPendingConsonantWithVirama(result, ref pendingConsonantGlyph);
                i++;
                continue;
            }

            SyllableRule? consonantMatch = _consonants.FindLongestMatch(latinWord, i);

            if (pendingConsonantGlyph is not null
                && consonantMatch is { } candidate
                && TryGetConjunctTail(pendingConsonantGlyph, candidate.Latin, out string? tailGlyph)
                && IsFollowedByVowel(latinWord, i + candidate.Latin.Length))
            {
                // Rakāraṃśaya / yansaya (design doc §3.2 "Special conjuncts"): the head
                // consonant gives up its own vowel to virama + ZWJ + the conjunct tail, and
                // the tail itself becomes the new pending consonant so the vowel that follows
                // attaches to *it* via the ordinary vowel-sign handling below (e.g. "krama"'s
                // "ra" still needs to resolve its own inherent/explicit vowel).
                result.Append(pendingConsonantGlyph).Append(RuleTable.Virama[0]).Append(RuleTable.ZeroWidthJoiner[0]);
                pendingConsonantGlyph = tailGlyph;
                i += candidate.Latin.Length;
                continue;
            }

            SyllableRule? vowelMatch = pendingConsonantGlyph is not null
                ? _dependentVowelSigns.FindLongestMatch(latinWord, i)
                : _independentVowels.FindLongestMatch(latinWord, i);

            SyllableRule? chosen = PickLongerMatch(consonantMatch, vowelMatch);

            if (chosen is null)
            {
                // A bare 'a' right after a consonant is the *inherent* vowel: it has no
                // dependent sign of its own (RuleTable.DependentVowelSigns has no "a" entry
                // by design), so it finalizes the pending consonant with no sign appended,
                // rather than being treated as an unmatched passthrough character.
                if (pendingConsonantGlyph is not null && latinWord[i] == 'a')
                {
                    result.Append(pendingConsonantGlyph);
                    pendingConsonantGlyph = null;
                    i++;
                    continue;
                }

                // A passthrough character (punctuation/digits/unknown) ends the syllable just
                // like the end of the word does, so a pending consonant gets its virama here too.
                FlushPendingConsonantWithVirama(result, ref pendingConsonantGlyph);
                result.Append(latinWord[i]);
                i++;
                continue;
            }

            switch (chosen.Value.Kind)
            {
                case TokenKind.Consonant:
                    // Superseding a still-pending consonant with no vowel in between is a
                    // consonant cluster: the first consonant's vowel is suppressed (virama).
                    FlushPendingConsonantWithVirama(result, ref pendingConsonantGlyph);
                    pendingConsonantGlyph = chosen.Value.Glyph;
                    break;

                case TokenKind.DependentVowelSign:
                    result.Append(pendingConsonantGlyph).Append(chosen.Value.Glyph);
                    pendingConsonantGlyph = null;
                    break;

                case TokenKind.IndependentVowel:
                    result.Append(chosen.Value.Glyph);
                    break;

                case TokenKind.ConjunctMarker:
                    // Reserved: no rule table currently carries this kind. Conjuncts are
                    // produced by the rakāraṃśaya/yansaya check above instead.
                    break;
            }

            i += chosen.Value.Latin.Length;
        }

        // Google/Helakuru convention: a consonant not followed by a vowel always takes the
        // virama, word-final included (visin -> විසින්); the inherent vowel must be typed as "a".
        FlushPendingConsonantWithVirama(result, ref pendingConsonantGlyph);
        return result.ToString();
    }

    private static void FlushPendingConsonantWithVirama(StringBuilder result, ref string? pendingConsonantGlyph)
    {
        if (pendingConsonantGlyph is null)
        {
            return;
        }

        result.Append(pendingConsonantGlyph).Append(RuleTable.Virama[0]);
        pendingConsonantGlyph = null;
    }

    private bool IsFollowedByVowel(ReadOnlySpan<char> text, int position)
    {
        if (position >= text.Length)
        {
            return false;
        }

        // The inherent 'a' has no entry of its own in DependentVowelSigns (see above), so it
        // must be checked for explicitly alongside an actual dependent-vowel-sign match.
        return text[position] == 'a' || _dependentVowelSigns.FindLongestMatch(text, position) is not null;
    }

    private static bool TryGetConjunctTail(string headGlyph, string consonantLatin, out string tailGlyph)
    {
        switch (consonantLatin)
        {
            case "r":
                tailGlyph = RuleTable.RakaransayaTail;
                return true;
            // Standard orthography writes ර + ය as a plain cluster (කාර්ය, ආචාර්ය, සූර්ය): the
            // head takes the repaya form and the ය is never joined into a yansaya.
            case "y" when headGlyph != RuleTable.RakaransayaTail:
                tailGlyph = RuleTable.YansayaTail;
                return true;
            default:
                tailGlyph = string.Empty;
                return false;
        }
    }

    private static SyllableRule? PickLongerMatch(SyllableRule? a, SyllableRule? b)
    {
        if (a is null)
        {
            return b;
        }

        if (b is null)
        {
            return a;
        }

        return a.Value.Latin.Length >= b.Value.Latin.Length ? a : b;
    }
}
