using System.Text;
using SinhalaInput.Core.Transliteration;
using Xunit.Abstractions;

namespace SinhalaInput.Core.Tests;

/// <summary>
/// Round-trips every word of the Lankadeepa corpus (roundtrip-test/lankadeepa/*.tsv): the
/// Singlish spelling of each non-SKIP row must transliterate to exactly the Sinhala the site
/// published. See roundtrip-test/lankadeepa/CHECKLIST.md.
/// </summary>
public class CorpusRoundTripTests
{
    private const char Virama = '්';
    private const char ZeroWidthJoiner = '‍';

    private static readonly TransliterationEngine Engine = new();

    private readonly ITestOutputHelper _output;

    public CorpusRoundTripTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public sealed record CorpusRow(string File, int Line, string Sinhala, string Singlish, string Status);

    public static IEnumerable<object[]> Rows() =>
        LoadCorpus()
            .Where(row => row.Status != "SKIP")
            .Select(row => new object[] { $"{row.File}:{row.Line}", row.Singlish, row.Sinhala });

    [Theory]
    [MemberData(nameof(Rows))]
    public void Transliterate_ReproducesCorpusWord(string location, string singlish, string sinhala)
    {
        Assert.False(string.IsNullOrEmpty(singlish), $"{location}: empty Singlish");
        Assert.Equal(NormalizeExpected(sinhala), Engine.Transliterate(singlish));
    }

    [Fact]
    public void Corpus_PassRateSummary()
    {
        List<CorpusRow> corpus = LoadCorpus();
        CorpusRow[] tested = corpus.Where(row => row.Status != "SKIP").ToArray();
        CorpusRow[] failing = tested
            .Where(row => Engine.Transliterate(row.Singlish) != NormalizeExpected(row.Sinhala))
            .ToArray();

        int passed = tested.Length - failing.Length;
        _output.WriteLine(
            $"Corpus: {corpus.Count} rows, {corpus.Count - tested.Length} SKIP, {tested.Length} tested, "
            + $"{passed} pass ({100.0 * passed / tested.Length:F1}%)");
        foreach (CorpusRow row in failing)
        {
            _output.WriteLine($"FAIL {row.File}:{row.Line} {row.Sinhala} <- {row.Singlish} got {Engine.Transliterate(row.Singlish)}");
        }

        Assert.NotEmpty(tested);
    }

    [Theory]
    [InlineData("ඇඟලු‍ම්", "ඇඟලුම්")]
    [InlineData("ඉල්ලු‍ම", "ඉල්ලුම")]
    [InlineData("ක්‍රම", "ක්‍රම")]
    [InlineData("‍ම", "ම")]
    public void NormalizeExpected_DropsOnlyZwjNotFollowingVirama(string raw, string expected)
    {
        Assert.Equal(expected, NormalizeExpected(raw));
    }

    // A ZWJ only has meaning right after a virama (it forms the rakaransaya/yansaya/touching
    // conjunct). Lankadeepa's HTML carries stray ZWJs elsewhere (e.g. after a vowel sign in
    // ඇඟලු‍ම්) that render identically and cannot be typed, so they are source noise.
    internal static string NormalizeExpected(string sinhala)
    {
        var builder = new StringBuilder(sinhala.Length);
        for (int i = 0; i < sinhala.Length; i++)
        {
            if (sinhala[i] == ZeroWidthJoiner && (i == 0 || sinhala[i - 1] != Virama))
            {
                continue;
            }

            builder.Append(sinhala[i]);
        }

        return builder.ToString();
    }

    private static List<CorpusRow> LoadCorpus()
    {
        string directory = Path.Combine(AppContext.BaseDirectory, "Corpus");
        var rows = new List<CorpusRow>();
        foreach (string path in Directory.GetFiles(directory, "*.tsv").Order(StringComparer.Ordinal))
        {
            string file = Path.GetFileName(path);
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);

            // Line 1 is the header; line numbers are 1-based to match an editor.
            for (int index = 1; index < lines.Length; index++)
            {
                if (lines[index].Length == 0)
                {
                    continue;
                }

                string[] columns = lines[index].Split('\t');
                rows.Add(new CorpusRow(
                    file,
                    index + 1,
                    columns[0],
                    columns.Length > 1 ? columns[1] : string.Empty,
                    columns.Length > 2 ? columns[2] : string.Empty));
            }
        }

        return rows;
    }
}
