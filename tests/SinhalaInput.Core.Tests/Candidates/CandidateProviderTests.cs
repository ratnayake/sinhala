using SinhalaInput.Core.Candidates;
using SinhalaInput.Core.Transliteration;

namespace SinhalaInput.Core.Tests.Candidates;

public class CandidateProviderTests
{
    private readonly TransliterationEngine _engine = new();

    [Fact]
    public void GetCandidates_AlwaysReturnsThePrimaryEngineOutputFirst()
    {
        var provider = new CandidateProvider(_engine, new FakeUserDictionaryStore());

        IReadOnlyList<string> candidates = provider.GetCandidates("mama");

        Assert.Equal("මම", candidates[0]);
    }

    [Fact]
    public void GetCandidates_ForAWordContainingTheAmbiguousToken_IncludesTheDentalAlternate()
    {
        var provider = new CandidateProvider(_engine, new FakeUserDictionaryStore());

        // "mata" (to me): default "t" -> retroflex ට gives "මට"; the documented
        // ambiguous-token alternate "t" -> ත gives "මත".
        IReadOnlyList<string> candidates = provider.GetCandidates("mata");

        Assert.Equal(["මට", "මත"], candidates);
    }

    [Fact]
    public void GetCandidates_ForAWordWithNoAmbiguousToken_ReturnsOnlyOneCandidate()
    {
        var provider = new CandidateProvider(_engine, new FakeUserDictionaryStore());

        IReadOnlyList<string> candidates = provider.GetCandidates("mama");

        Assert.Single(candidates);
    }

    [Fact]
    public void GetCandidates_WhenTheUserPreviouslyChoseAnAlternate_RanksItFirst()
    {
        var store = new FakeUserDictionaryStore();
        store.SetPreferredCandidate("mata", "මත");
        var provider = new CandidateProvider(_engine, store);

        IReadOnlyList<string> candidates = provider.GetCandidates("mata");

        Assert.Equal(["මත", "මට"], candidates);
    }

    [Fact]
    public void GetCandidates_WhenTheUsersLearnedChoiceIsNotAGeneratedAlternate_StillPromotesIt()
    {
        var store = new FakeUserDictionaryStore();
        store.SetPreferredCandidate("mama", "මාමා"); // e.g. a hypothetical future rule-set alternate.
        var provider = new CandidateProvider(_engine, store);

        IReadOnlyList<string> candidates = provider.GetCandidates("mama");

        Assert.Equal("මාමා", candidates[0]);
        Assert.Contains("මම", candidates);
    }

    [Fact]
    public void LearnSelection_RecordsTheChoiceInTheUserDictionary()
    {
        var store = new FakeUserDictionaryStore();
        var provider = new CandidateProvider(_engine, store);

        provider.LearnSelection("mata", "මත");

        Assert.Equal("මත", store.TryGetPreferredCandidate("mata"));
    }

    [Fact]
    public void Constructor_NullPrimaryEngine_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new CandidateProvider(null!, new FakeUserDictionaryStore()));
    }

    [Fact]
    public void Constructor_NullUserDictionary_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new CandidateProvider(_engine, null!));
    }

    private sealed class FakeUserDictionaryStore : IUserDictionaryStore
    {
        private readonly Dictionary<string, string> _entries = [];

        public string? TryGetPreferredCandidate(string latinWord) => _entries.GetValueOrDefault(latinWord);

        public void SetPreferredCandidate(string latinWord, string sinhalaCandidate) => _entries[latinWord] = sinhalaCandidate;
    }
}
