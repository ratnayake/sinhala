namespace SinhalaInput.Core.Transliteration;

/// <summary>Transliterates Latin ("Singlish") text into Sinhala Unicode.</summary>
public interface ITransliterationEngine
{
    /// <summary>Transliterates a single Latin word into Sinhala Unicode.</summary>
    /// <param name="latinWord">A single word (no internal whitespace) typed in Latin script.</param>
    /// <returns>The Sinhala Unicode rendering of <paramref name="latinWord"/>.</returns>
    string Transliterate(string latinWord);
}
