namespace SinhalaInput.Core.Candidates;

/// <summary>
/// Produces ranked Sinhala transliteration candidates for a Latin word, and learns from the
/// user's picks so a previously chosen alternate is ranked first next time
/// (design doc §5, "Candidate generation").
/// </summary>
public interface ICandidateProvider
{
    /// <summary>
    /// Returns the ranked candidates for <paramref name="latinWord"/>, most likely first.
    /// Always returns at least one candidate (the engine's primary transliteration).
    /// </summary>
    IReadOnlyList<string> GetCandidates(string latinWord);

    /// <summary>Records that the user picked <paramref name="chosenSinhala"/> for <paramref name="latinWord"/>.</summary>
    void LearnSelection(string latinWord, string chosenSinhala);
}
