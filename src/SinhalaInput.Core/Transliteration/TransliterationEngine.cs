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

    public TransliterationEngine()
        : this(RuleTable.Consonants, RuleTable.IndependentVowels, RuleTable.DependentVowelSigns)
    {
    }

    // Internal constructor seam for unit tests (and CandidateProvider's alternate-spelling
    // engines) that need a substituted rule set.
    internal TransliterationEngine(
        IEnumerable<SyllableRule> consonants,
        IEnumerable<SyllableRule> independentVowels,
        IEnumerable<SyllableRule> dependentVowelSigns)
    {
        _consonants = new RuleTrie(consonants);
        _independentVowels = new RuleTrie(independentVowels);
        _dependentVowelSigns = new RuleTrie(dependentVowelSigns);
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
            SyllableRule? consonantMatch = _consonants.FindLongestMatch(latinWord, i);

            if (pendingConsonantGlyph is not null
                && consonantMatch is { } candidate
                && TryGetConjunctTail(candidate.Latin, out string? tailGlyph)
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

                // Passthrough (punctuation/digits/unknown chars) never inserts a virama: a
                // pending consonant ahead of it simply keeps its inherent vowel.
                FlushPendingConsonant(result, ref pendingConsonantGlyph, appendVirama: false);
                result.Append(latinWord[i]);
                i++;
                continue;
            }

            switch (chosen.Value.Kind)
            {
                case TokenKind.Consonant:
                    // Superseding a still-pending consonant with no vowel in between is a
                    // consonant cluster: the first consonant's vowel is suppressed (virama).
                    FlushPendingConsonant(result, ref pendingConsonantGlyph, appendVirama: true);
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

        // A consonant with no vowel that follows anywhere else in the word is word-final: it
        // keeps its inherent vowel (matching Google's observed behaviour), never a virama —
        // a virama is only ever inserted when another consonant supersedes it (see the
        // Consonant case above) or as part of a rakāraṃśaya/yansaya conjunct.
        FlushPendingConsonant(result, ref pendingConsonantGlyph, appendVirama: false);
        return result.ToString();
    }

    private static void FlushPendingConsonant(StringBuilder result, ref string? pendingConsonantGlyph, bool appendVirama)
    {
        if (pendingConsonantGlyph is null)
        {
            return;
        }

        result.Append(pendingConsonantGlyph);
        if (appendVirama)
        {
            result.Append(RuleTable.Virama[0]);
        }

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

    private static bool TryGetConjunctTail(string consonantLatin, out string tailGlyph)
    {
        switch (consonantLatin)
        {
            case "r":
                tailGlyph = RuleTable.RakaransayaTail;
                return true;
            case "y":
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
