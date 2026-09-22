using System.Text;

namespace SinhalaInput.Core.Transliteration;

/// <summary>
/// Deterministic, syllable-oriented Latin-to-Sinhala transliteration engine.
/// See docs/SINHALA-INPUT-TOOL-DESIGN.md §3.3 for the algorithm this implements.
/// </summary>
/// <remarks>
/// This is the v1 baseline: it covers the core consonant/vowel algorithm. It does not yet
/// special-case consonant-cluster viramas beyond a single trailing consonant, nor the
/// rakāraṃśaya/yansaya conjuncts (design doc §3.2 "Special conjuncts") — those are tracked
/// as follow-up work, to be covered by <c>WordList.csv</c> golden tests before being marked done.
/// </remarks>
public sealed class TransliterationEngine : ITransliterationEngine
{
    private readonly RuleTrie _consonants;
    private readonly RuleTrie _independentVowels;
    private readonly RuleTrie _dependentVowelSigns;

    public TransliterationEngine()
        : this(RuleTable.Consonants, RuleTable.IndependentVowels, RuleTable.DependentVowelSigns)
    {
    }

    // Internal constructor seam for unit tests that need a reduced rule set.
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
            SyllableRule? vowelMatch = pendingConsonantGlyph is not null
                ? _dependentVowelSigns.FindLongestMatch(latinWord, i)
                : _independentVowels.FindLongestMatch(latinWord, i);

            SyllableRule? chosen = PickLongerMatch(consonantMatch, vowelMatch);

            if (chosen is null)
            {
                // A bare 'a' right after a consonant is the *inherent* vowel: it has no
                // dependent sign of its own (RuleTable.DependentVowelSigns has no "a" entry
                // by design), so it must be consumed silently rather than treated as an
                // unmatched passthrough character.
                if (pendingConsonantGlyph is not null && latinWord[i] == 'a')
                {
                    i++;
                    continue;
                }

                FlushPendingConsonant(result, ref pendingConsonantGlyph);
                result.Append(latinWord[i]); // passthrough: punctuation, digits, unknown chars
                i++;
                continue;
            }

            switch (chosen.Value.Kind)
            {
                case TokenKind.Consonant:
                    FlushPendingConsonant(result, ref pendingConsonantGlyph); // consonant cluster -> virama
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
                    // Reserved for rakāraṃśaya/yansaya handling; not yet produced by any rule table.
                    break;
            }

            i += chosen.Value.Latin.Length;
        }

        FlushPendingConsonant(result, ref pendingConsonantGlyph);
        return result.ToString();
    }

    private static void FlushPendingConsonant(StringBuilder result, ref string? pendingConsonantGlyph)
    {
        if (pendingConsonantGlyph is null)
        {
            return;
        }

        // A consonant with no vowel that follows is either word-final (keeps its inherent
        // vowel, matching Google's observed behaviour) or is about to be superseded by the
        // next consonant, which appends the virama itself — see the Consonant case above.
        result.Append(pendingConsonantGlyph);
        pendingConsonantGlyph = null;
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
