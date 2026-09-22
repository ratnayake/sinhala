namespace SinhalaInput.Core.Candidates;

/// <summary>
/// Persists the user's learned preference for how a Latin word should be rendered in Sinhala,
/// so a previously chosen alternate ranks first next time (design doc §5, §10 privacy note:
/// local-only, no telemetry, stores only already-committed words).
/// </summary>
public interface IUserDictionaryStore
{
    /// <summary>
    /// Returns the Sinhala rendering the user previously chose for <paramref name="latinWord"/>,
    /// or <see langword="null"/> if nothing is recorded for it.
    /// </summary>
    string? TryGetPreferredCandidate(string latinWord);

    /// <summary>Records that the user chose <paramref name="sinhalaCandidate"/> for <paramref name="latinWord"/>.</summary>
    void SetPreferredCandidate(string latinWord, string sinhalaCandidate);
}
