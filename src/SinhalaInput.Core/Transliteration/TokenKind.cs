namespace SinhalaInput.Core.Transliteration;

/// <summary>Classifies how a matched Latin token should be rendered in Sinhala.</summary>
public enum TokenKind
{
    Consonant,
    IndependentVowel,
    DependentVowelSign,
    ConjunctMarker,
    Anusvara,
}
