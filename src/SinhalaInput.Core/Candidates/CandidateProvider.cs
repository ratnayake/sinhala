using SinhalaInput.Core.Transliteration;

namespace SinhalaInput.Core.Candidates;

/// <summary>
/// Default <see cref="ICandidateProvider"/>: ranks the primary engine output ahead of
/// alternates generated from <see cref="AmbiguousTokenSubstitutions"/>, then promotes whatever
/// the user previously chose for this exact word to the front (design doc §5).
/// </summary>
public sealed class CandidateProvider : ICandidateProvider
{
    private readonly ITransliterationEngine _primaryEngine;
    private readonly IUserDictionaryStore _userDictionary;
    private readonly IReadOnlyList<ITransliterationEngine> _alternateEngines;

    public CandidateProvider(ITransliterationEngine primaryEngine, IUserDictionaryStore userDictionary)
    {
        ArgumentNullException.ThrowIfNull(primaryEngine);
        ArgumentNullException.ThrowIfNull(userDictionary);

        _primaryEngine = primaryEngine;
        _userDictionary = userDictionary;
        _alternateEngines = AmbiguousTokenSubstitutions.BuildAlternateConsonantRuleSets()
            .Select(consonants => (ITransliterationEngine)new TransliterationEngine(
                consonants, RuleTable.IndependentVowels, RuleTable.DependentVowelSigns))
            .ToArray();
    }

    public IReadOnlyList<string> GetCandidates(string latinWord)
    {
        ArgumentNullException.ThrowIfNull(latinWord);

        var candidates = new List<string> { _primaryEngine.Transliterate(latinWord) };
        foreach (ITransliterationEngine alternateEngine in _alternateEngines)
        {
            string candidate = alternateEngine.Transliterate(latinWord);
            if (!candidates.Contains(candidate))
            {
                candidates.Add(candidate);
            }
        }

        string? learned = _userDictionary.TryGetPreferredCandidate(latinWord);
        if (learned is not null)
        {
            // The user's past choice always ranks first, even if it doesn't match any
            // candidate the current rule set would generate (e.g. the rule set changed since).
            candidates.Remove(learned);
            candidates.Insert(0, learned);
        }

        return candidates;
    }

    public void LearnSelection(string latinWord, string chosenSinhala)
    {
        ArgumentNullException.ThrowIfNull(latinWord);
        ArgumentNullException.ThrowIfNull(chosenSinhala);
        _userDictionary.SetPreferredCandidate(latinWord, chosenSinhala);
    }
}
