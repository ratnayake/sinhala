using SinhalaInput.Core.Transliteration;

Console.OutputEncoding = System.Text.Encoding.UTF8;

ITransliterationEngine engine = new TransliterationEngine();

Console.WriteLine("SinhalaInput CLI — type Latin ('Singlish') words, one per line. Ctrl+C to exit.");

string? line;
while ((line = Console.ReadLine()) is not null)
{
    IEnumerable<string> words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    string transliterated = string.Join(' ', words.Select(engine.Transliterate));
    Console.WriteLine(transliterated);
}
