using SinhalaInput.Core.Transliteration;

namespace SinhalaInput.Core.Candidates;

/// <summary>
/// Default <see cref="ICandidateProvider"/>.
/// </summary>
/// <remarks>
/// v1 baseline: returns only the engine's single deterministic candidate and does not yet
/// learn from selections. Ranked alternates and a persisted user dictionary are tracked
/// follow-up work (design doc §5) — implement them here, backed by a new
/// <c>UserDictionary</c> type in this folder, and cover both with unit tests before
/// removing this remark.
/// </remarks>
public sealed class CandidateProvider(ITransliterationEngine engine) : ICandidateProvider
{
    private readonly ITransliterationEngine _engine = engine ?? throw new ArgumentNullException(nameof(engine));

    public IReadOnlyList<string> GetCandidates(string latinWord)
    {
        ArgumentNullException.ThrowIfNull(latinWord);
        return [_engine.Transliterate(latinWord)];
    }

    public void LearnSelection(string latinWord, string chosenSinhala)
    {
        ArgumentNullException.ThrowIfNull(latinWord);
        ArgumentNullException.ThrowIfNull(chosenSinhala);
        // No-op until the user dictionary lands.
    }
}
