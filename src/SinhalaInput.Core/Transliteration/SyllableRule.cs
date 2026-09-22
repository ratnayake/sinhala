namespace SinhalaInput.Core.Transliteration;

/// <summary>An immutable rule mapping a Latin pattern to its Sinhala rendering.</summary>
public readonly record struct SyllableRule(
    string Latin,
    TokenKind Kind,
    string Glyph);
