namespace SinhalaInput.Core.Transliteration;

/// <summary>
/// A prefix trie over <see cref="RuleTable"/> entries that resolves the *longest* matching
/// Latin key at a given buffer position, so multi-letter patterns (e.g. "th", "aae") always
/// win over shorter prefixes (e.g. "t", "a").
/// </summary>
internal sealed class RuleTrie
{
    private sealed class Node
    {
        public Dictionary<char, Node> Children { get; } = new();

        public SyllableRule? Rule { get; set; }
    }

    private readonly Node _root = new();

    public RuleTrie(IEnumerable<SyllableRule> rules)
    {
        foreach (SyllableRule rule in rules)
        {
            Node current = _root;
            foreach (char c in rule.Latin)
            {
                current = current.Children.TryGetValue(c, out Node? next)
                    ? next
                    : current.Children[c] = new Node();
            }

            current.Rule = rule;
        }
    }

    /// <summary>
    /// Finds the longest rule whose Latin key matches <paramref name="text"/> starting at
    /// <paramref name="start"/>. Returns <see langword="null"/> if no rule matches.
    /// </summary>
    public SyllableRule? FindLongestMatch(ReadOnlySpan<char> text, int start)
    {
        Node current = _root;
        SyllableRule? best = null;

        for (int i = start; i < text.Length; i++)
        {
            if (!current.Children.TryGetValue(text[i], out Node? next))
            {
                break;
            }

            current = next;
            if (current.Rule is { } rule)
            {
                best = rule; // keep extending; a longer match further down wins if it exists
            }
        }

        return best;
    }
}
