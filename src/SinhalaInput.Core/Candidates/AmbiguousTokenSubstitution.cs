using SinhalaInput.Core.Transliteration;

namespace SinhalaInput.Core.Candidates;

/// <summary>
/// A documented ambiguous Latin consonant token, mapped to an alternate Sinhala glyph a user
/// might have meant instead of the engine's default (design doc §5).
/// </summary>
public readonly record struct AmbiguousTokenSubstitution(string Latin, string AlternateGlyph);

/// <summary>
/// The small, explicitly documented set of ambiguous-token substitutions used to generate
/// candidate alternates (design doc §5: "e.g. try <c>t -&gt; ත</c> in addition to the default
/// <c>t -&gt; ට</c>"). This is not a general fuzzer: it targets specific known ambiguities in
/// the phonetic scheme where a bare Latin letter's default mapping and a plausible intended
/// alternate both correspond to real Sinhala consonants.
/// </summary>
public static class AmbiguousTokenSubstitutions
{
    public static readonly IReadOnlyList<AmbiguousTokenSubstitution> Entries =
    [
        // "t" defaults to the retroflex ට, but a user who dropped the "h" from the dental
        // digraph "th" also plausibly meant ත.
        new("t", "ත"),
    ];

    /// <summary>
    /// Builds one alternate consonant rule set per entry in <see cref="Entries"/>, each with
    /// exactly that entry's Latin token remapped to its alternate glyph.
    /// </summary>
    internal static IEnumerable<IReadOnlyList<SyllableRule>> BuildAlternateConsonantRuleSets()
    {
        foreach (AmbiguousTokenSubstitution substitution in Entries)
        {
            yield return RuleTable.Consonants
                .Select(rule => rule.Latin == substitution.Latin
                    ? rule with { Glyph = substitution.AlternateGlyph }
                    : rule)
                .ToArray();
        }
    }
}
