# Sinhala Phonetic Input Tool for Windows 10/11 — Design Document

**Status:** Draft v1
**Author:** isuru.sampath@ratnayake.info
**Target platform:** Windows 10 (1809+) and Windows 11, .NET 8 (LTS)
**Goal:** A background input tool that transliterates Latin ("Singlish") keystrokes into Sinhala Unicode in real time, the same way Google Input Tools' Sinhala keyboard behaves — e.g. `mama` → `මම`, `api` → `අපි`.

---

## 1. What "Google Input Tools" actually does (behavioural spec)

Before designing the C# code, it's important to pin down the exact behaviour we are cloning, because "IME" is a loaded term that covers several very different implementations.

Google Input Tools' Sinhala keyboard is a **phonetic transliteration IME**:

1. The user keeps typing on a normal QWERTY keyboard, in Latin letters.
2. As each **word** is typed, the tool buffers the Latin characters (it does not touch the underlying application's text until a decision point is reached).
3. On a **commit trigger** — space, punctuation, Enter, Tab, or an explicit navigation key (arrow keys, click elsewhere) — the buffered Latin word is converted to Sinhala and the Latin characters already sent to the focused application are replaced with the Sinhala result.
4. While the word is still being typed, a **candidate window** (a small popup anchored under the text caret) shows the top transliteration and a numbered list of alternates (e.g. typing `karanna` might offer `කරන්න` as #1). Pressing a digit, Space, Enter, or clicking picks a candidate. Arrow keys / Page Up/Down cycle candidates.
5. Backspace edits the **Latin buffer**, not the committed Sinhala text, until the word is committed — so corrections re-run transliteration instead of just deleting Sinhala glyphs one at a time.
6. A hotkey (Google uses **Ctrl+Space** by default, configurable) toggles the tool on/off without closing it, and the language/tool is also selectable from the Windows language bar / system tray.
7. It works **per focused control**, across essentially all Win32, WPF, and most UWP/Chromium/Electron apps, because Google's implementation is a real **Text Services Framework (TSF) text service**, not a keyboard-hook hack.

That last point is the key architectural decision, covered next.

---

## 2. Architecture options

There are two legitimate ways to build this on Windows. They are not equally easy, and picking the right one for a first release matters more than any other decision in this document.

### Option A — A real TSF Text Service (what Google actually ships)

Windows' **Text Services Framework** (`ITfTextInputProcessor`, `ITfKeyEventSink`, `ITfCompositionSink`, `ITfCandidateListUIElement`, etc.) is the official extensibility point for IMEs. It is a COM-based, in-process server that:

- Is registered as a language profile and appears in the Windows language bar / input switcher (`Win+Space`) next to "English (US)" etc.
- Gets first refusal on every keystroke in the focused control, for essentially every UI framework (Win32, WPF, WinForms, UWP/WinUI, and Chromium/Electron via their TSF support).
- Can draw its own candidate UI docked to the caret using TSF's UI element interfaces, or a custom window.
- Works correctly with secure/password fields (it can detect `GUID_PROP_INPUTSCOPE` / `TF_PROFILETYPE_INPUTPROCESSOR` restrictions) and with RTL/complex text layout.

**Trade-offs:** TSF's contract is COM-first and was designed for C++/ATL. It is technically possible from C# (define the interfaces with `ComImport`/`InterfaceType`, implement them, register the assembly with `regasm /codebase` or a native COM manifest, sign it, and register profile GUIDs under `HKLM\...\CTF\TIP`), but:
- There's no first-party managed wrapper; you hand-declare a dozen COM interfaces.
- Debugging is painful — a bug in `ITfKeyEventSink.OnTestKeyDown` can freeze the calling application, not just your process.
- Deployment requires an installer that can write to `HKLM` and register per-machine COM (admin rights).
- Threading/COM apartment rules (STA) are unforgiving.

This is the *correct* long-term architecture and is documented in §11 as the v2 target, but it is a multi-week undertaking on its own even for an experienced Windows developer, and it dominates the project if attempted first.

### Option B — Global low-level keyboard hook + synthetic input (recommended v1)

A normal Windows process (no admin rights, no COM registration) that:

1. Installs a **low-level keyboard hook** (`WH_KEYBOARD_LL` via `SetWindowsHookEx`) to observe keystrokes system-wide.
2. Buffers the current word per-process based on the **focused window**.
3. On a commit trigger, synthesizes `Backspace` × N followed by the Sinhala Unicode string via `SendInput` (using `KEYEVENTF_UNICODE` scan codes), replacing what the focused app already rendered.
4. Draws its own **candidate popup** (a topmost, non-activating WPF window) positioned near the caret, obtained via `GetGUIThreadInfo`/UI Automation.

**Trade-offs:** it does not appear in the Windows language bar, cannot see into elevated (Run-as-administrator) windows unless it also runs elevated, cannot reliably distinguish password fields from normal ones (mitigated in §10), and depends on the target app accepting synthetic `SendInput` events (true for the overwhelming majority of Win32/WPF/UWP/browser/Office apps).

**This is the pragmatic choice for a first working release**: it is buildable end-to-end in pure C#, testable without a native install step, and produces the same typing experience for the user in the apps they actually use (Notepad, Word, browsers, Slack, VS Code, etc.).

### Decision

Build **Option B** first as `SinhalaInput` v1. Structure the solution (see §6) so the transliteration engine is a pure, UI/OS-agnostic library — that engine is reused unchanged if/when a TSF text service (Option A) is added later as an alternative front end.

---

## 3. Transliteration model

### 3.1 The linguistic shape of the problem

Sinhala is an abugida: a consonant letter carries an *inherent* short `a` vowel unless followed by a vowel sign (*pilla*) or a virama (*hal kirīma*, U+0DCA) that suppresses the vowel entirely (for consonant clusters). This is exactly why `mama` → `මම` (two consonants, each keeping its inherent `a`, no vowel signs needed) while `api` → `අපි` (an independent vowel `අ`, then consonant `ප` with the dependent vowel sign `ි` replacing its inherent `a`).

This means transliteration is **not** a 1:1 character map — it is a **syllable-oriented, longest-match rewrite**:

1. Scan the Latin buffer left to right.
2. At each position, try to match the *longest* known Latin pattern first (multi-letter aspirated/retroflex/vowel digraphs like `th`, `sh`, `ng`, `aa`, `ae`, `oo` must win over their shorter single-letter substrings).
3. Classify the matched token as a **consonant**, **independent vowel**, or **vowel sign continuation**, and emit the corresponding Sinhala glyph(s), correctly choosing between:
   - a bare consonant glyph (inherent `a`),
   - a consonant + dependent vowel sign,
   - a consonant + virama (when followed by another consonant with no vowel between them),
   - an independent vowel glyph (word start, or vowel following another vowel).
4. Handle a small number of **conjunct exceptions**: the *rakāraṃśaya* (`r`-conjunct, e.g. `krama` → `ක්‍රම`) and *yansaya* (`y`-conjunct, e.g. `vyaparaya` → `ව්‍යාපාරය`), which insert a virama + ZWJ (U+200D) + consonant instead of a plain virama.

This "maximal munch over a trie, then syllabify" approach is exactly how the open-source Singlish transliterators (and Google's own tool, based on its observable behaviour) work, and it is what makes the algorithm deterministic and fast (`O(length of word)`, no ML model, no network call).

### 3.2 Rule tables

> **Implementation note:** the tables below give the *glyphs*, which are unambiguous. When you write the `RuleTable` constants in code, copy the literal Sinhala characters into UTF-8 source files (`.editorconfig` → `charset = utf-8-bom` or plain `utf-8`) rather than hand-typing hex code points; cross-check anything you do hex-encode against the official Unicode "Sinhala" block chart (U+0D80–U+0DFF, virama U+0DCA "SINHALA SIGN AL-LAKUNA", ZWJ U+200D) rather than trusting memory.

**Independent vowels** (used word-initially, or after another vowel):

| Latin | Sinhala | Latin | Sinhala |
|---|---|---|---|
| `a` | අ | `e` | එ |
| `aa`, `A` | ආ | `ee`, `E` | ඒ |
| `ae` | ඇ | `ai` | ඓ |
| `aae`, `Ae` | ඈ | `o` | ඔ |
| `i` | ඉ | `oo`, `O` | ඕ |
| `ii`, `I` | ඊ | `au` | ඖ |
| `u` | උ | | |
| `uu`, `U` | ඌ | | |

**Dependent vowel signs** (*pili*, attached to a consonant, replacing its inherent `a`):

| Latin (after consonant) | Sign | Latin (after consonant) | Sign |
|---|---|---|---|
| `a` | *(none — inherent vowel)* | `e` | ෙ (wraps to the **left** of the consonant) |
| `aa`, `A` | ා | `ee`, `E` | ේ (wraps left) |
| `ae` | ැ | `ai` | ෛ (wraps left) |
| `aae`, `Ae` | ෑ | `o` | ො (wraps left+right) |
| `i` | ි | `oo`, `O` | ෝ (wraps left+right) |
| `ii`, `I` | ී | `au` | ෞ (wraps left+right) |
| `u` | ු | *(none, consonant cluster)* | ් (virama / hal kirīma) |
| `uu`, `U` | ූ | | |

> The "wraps to the left" vowel signs are a **rendering** detail handled automatically by the Sinhala OpenType shaping engine (Uniscribe/DirectWrite/HarfBuzz) once you emit the codepoints in **logical order** (consonant, then vowel sign) — do not try to reorder characters yourself; that is exactly what the shaping engine is for.

**Consonants** (each carries the inherent `a` unless followed by a vowel sign or virama):

| Latin | Sinhala | Latin | Sinhala | Latin | Sinhala |
|---|---|---|---|---|---|
| `k` | ක | `t` | ට *(retroflex)* | `p` | ප |
| `kh` | ඛ | `th` | ත *(dental)* | `ph`, `f` | ඵ |
| `g` | ග | `T` | ට *(explicit retroflex)* | `b` | බ |
| `gh` | ඝ | `d` | ද *(dental)* | `bh` | භ |
| `ng` | ඞ | `dh` | ධ *(dental aspirated)* | `m` | ම |
| `c`, `ch` | ච | `D` | ඩ *(retroflex)* | `y` | ය |
| `chh` | ඡ | `n` | න | `r` | ර |
| `j` | ජ | `N` | ණ *(retroflex)* | `l` | ල |
| `jh` | ඣ | `sh` | ශ | `v`, `w` | ව |
| `ny` | ඤ | `Sh` | ෂ *(retroflex)* | `L` | ළ |
| | | `s` | ස | `h` | හ |
| | | | | `z` | ‍ස *(no native /z/; map to ස or reject)* |

The **capitalisation convention** (`T`/`N`/`L`/`Sh` for retroflex sounds) mirrors what the community Singlish schemes (and Google's own tool) use, because English has no separate letters for dental vs. retroflex consonants. Capitalisation is the *only* way to get a retroflex consonant: the engine deliberately does **not** also accept a doubled-letter alternative (`tt`, `dd`, `nn`, `ll`, `ss`) for these, because doubling a consonant is already meaningful on its own — it is how a user spells genuine **gemination** (hal kirīma followed by a repeat of the same consonant, e.g. `malli` → මල්ලි, `anda`-style clusters generalised to a repeated letter). Registering both meanings for the same doubled spelling made every geminated retroflex-adjacent consonant ambiguous and, in practice, always lose to the retroflex reading (the longest-match rule always prefers the 2-letter key), silently corrupting common colloquial words like `malli` (→ මළි, wrong) and `enne` (→ එණෙ, wrong). A user who forgets to hold Shift for a retroflex letter gets the plain dental/alveolar consonant instead — a real but different sound, not a corrupted one — rather than a silently wrong gemination somewhere else in the word.

**Special conjuncts:**

| Latin | Result | Rule |
|---|---|---|
| `...Cra...` (consonant + `r` + vowel, mid-word) | `...C් + ZWJ + ර + vowel` | *Rakāraṃśaya*: e.g. `krama` → ක්‍රම |
| `...Cya...` (consonant + `y` + vowel, mid-word) | `...C් + ZWJ + ය + vowel` | *Yansaya*: e.g. `vyaparaya` → ව්‍යාපාරය |

**Passthrough:** digits, ASCII punctuation, whitespace, and any character not matched by a rule are passed through unchanged — the user can freely mix Sinhala words with numbers, `@handles`, URLs, etc., exactly like Google's tool.

**Anusvara:**

| Latin | Result | Rule |
|---|---|---|
| `M` | appends ං to the preceding syllable | *Anusvara*: e.g. `laMkaa` → ලංකා, `siMhala` → සිංහල |

Anusvara (ං, U+0D82 SINHALA SIGN ANUSVARAYA) is a nasal diacritic that attaches to the *end* of a syllable that already has its inherent or explicit vowel — unlike a dependent vowel sign, it does not replace one. Capital `M` is its trigger, following the same capitalisation convention used elsewhere for otherwise-ambiguous sounds (`T`/`D`/`N`/`L`/`Sh`, `A`/`I`/`U`/`E`/`O`): plain `n` before a consonant already means an ordinary consonant cluster with virama (e.g. `anda` → අන්ද), so anusvara needs its own spelling that cannot collide with that existing meaning. This rule was added after round-trip testing the engine against real Sinhala news text showed anusvara had no way to be typed at all, despite being needed for extremely common words — including `ලංකා` ("Lanka") and `සිංහල` ("Sinhala", the language's own name).

### 3.3 Algorithm (syllable-oriented, longest match)

```
function Transliterate(word: string) -> string:
    result = []
    i = 0
    pendingConsonant = null      # a consonant glyph waiting to see its vowel
    while i < word.length:
        token, kind, consumed = LongestMatch(word, i)   # trie lookup, longest key wins
        if token is null:
            if pendingConsonant: result.append(pendingConsonant); pendingConsonant = null
            result.append(word[i])   # passthrough, e.g. punctuation/digits
            i += 1
            continue

        match kind:
            case Consonant:
                if pendingConsonant:
                    result.append(pendingConsonant + VIRAMA)   # consonant cluster
                pendingConsonant = token.ConsonantGlyph
            case VowelSign:                                    # e.g. matched "aa" after a consonant
                if pendingConsonant:
                    result.append(pendingConsonant + token.SignGlyph)
                    pendingConsonant = null
                else:
                    result.append(token.IndependentGlyph)      # vowel at word/syllable start
            case ConjunctMarker:                                # r-yansa / rakaransaya
                result.append(pendingConsonant + VIRAMA + ZWJ + token.ConjunctConsonant)
                pendingConsonant = null

        i += consumed

    if pendingConsonant: result.append(pendingConsonant)        # trailing consonant, inherent 'a'
    return join(result)
```

The `LongestMatch` step is a trie (prefix tree) keyed by the Latin patterns above, checked longest-key-first (e.g. `th` before `t`, `aae` before `ae` before `a`), which is the standard technique used by every rule-based Singlish engine surveyed for this document (see §12 references) and gives `O(word length)` performance with no backtracking.

---

## 4. Word-boundary and editing semantics

| Trigger | Behaviour |
|---|---|
| Space, `.` `,` `!` `?` `;` `:` and other punctuation | Commit current buffer: run transliteration, replace the Latin text already on screen with the Sinhala result, then pass the trigger character through as-is. |
| Enter / Tab | Commit, then pass the key through. |
| Backspace | If buffer non-empty: pop last Latin char from the **buffer** and re-render the live preview (does *not* touch previously committed text). If buffer empty: pass Backspace through normally (deletes whatever precedes the caret). |
| Digit `1`–`9` while candidate popup is open | Select that candidate and commit. |
| ↑ / ↓ / Page Up / Page Down while popup open | Move candidate selection. |
| Esc | Revert the current word to the raw Latin text the user typed (undo transliteration for this word) and close the popup. |
| Ctrl+Space (configurable) | Toggle the whole tool on/off; while off, all keys pass through untouched. |
| Caret moved by mouse click, arrow keys with no popup, or focus change to a different control/window | Force-commit whatever is buffered (never leave an "in limbo" word). |

---

## 5. Candidate generation

Google's tool doesn't just apply one fixed rule set — it offers alternates because Singlish spelling is ambiguous (e.g. a word could plausibly end with `ta` → `ට` or `ත`). For v1, keep this tractable:

1. **Primary candidate**: output of the deterministic engine in §3.
2. **Alternates**: generated by re-running the engine with a small set of documented ambiguous-token substitutions (e.g. try `t → ත` in addition to the default `t → ට`) and de-duplicating.
3. **Optional (v1.1+): user dictionary** — a per-user JSON/SQLite store of `(latin word) → (chosen Sinhala spelling)` pairs, so that after the user picks candidate #2 for a word once, it becomes candidate #1 next time. This is the same "learning" behaviour Google's tool has and is a natural extension point (`ICandidateRanker`, see §6.2).

Do not attempt a statistical/ML re-ranker in v1 — it adds a training-data and model-hosting problem that is out of scope for a keystroke-latency-sensitive local tool.

---

## 6. Solution structure

```
SinhalaInput.sln
├── src/
│   ├── SinhalaInput.Core/                  # Pure .NET, no Windows/UI dependency, 100% unit-testable
│   │   ├── Transliteration/
│   │   │   ├── ITransliterationEngine.cs
│   │   │   ├── TransliterationEngine.cs
│   │   │   ├── RuleTrie.cs
│   │   │   ├── RuleTable.cs                # the tables from §3.2, as data
│   │   │   └── SyllableToken.cs            # record types for match results
│   │   ├── Candidates/
│   │   │   ├── ICandidateProvider.cs
│   │   │   ├── CandidateProvider.cs
│   │   │   └── UserDictionary.cs
│   │   └── SinhalaInput.Core.csproj
│   │
│   ├── SinhalaInput.Platform.Windows/      # All P/Invoke and Win32 concerns live here, nowhere else
│   │   ├── Hooking/
│   │   │   ├── IKeyboardHook.cs
│   │   │   └── LowLevelKeyboardHook.cs
│   │   ├── Input/
│   │   │   ├── ITextInjector.cs
│   │   │   └── SendInputTextInjector.cs
│   │   ├── Caret/
│   │   │   ├── ICaretLocator.cs
│   │   │   └── Win32CaretLocator.cs
│   │   ├── NativeMethods.cs                # centralised P/Invoke signatures ([SuppressUnmanagedCodeSecurity] avoided; LibraryImport used)
│   │   └── SinhalaInput.Platform.Windows.csproj
│   │
│   ├── SinhalaInput.App/                   # Composition root: tray app, WPF candidate window, settings
│   │   ├── TypingSessionController.cs      # wires hook -> engine -> injector -> UI (the state machine of §4)
│   │   ├── CandidateWindow.xaml(.cs)
│   │   ├── TrayApplicationContext.cs
│   │   ├── Settings/
│   │   │   ├── AppSettings.cs
│   │   │   └── SettingsWindow.xaml(.cs)
│   │   ├── App.xaml(.cs)
│   │   └── SinhalaInput.App.csproj
│   │
│   └── SinhalaInput.Cli/                   # Optional: a `stdin -> stdout` transliteration CLI for scripting/debugging
│       └── Program.cs
│
├── tests/
│   ├── SinhalaInput.Core.Tests/
│   │   ├── TransliterationEngineTests.cs   # data-driven: mama->මම, api->අපි, aayuboowan->ආයුබෝවන්, ...
│   │   └── WordList.csv                    # golden test corpus
│   └── SinhalaInput.App.Tests/
│       └── TypingSessionControllerTests.cs # drives the state machine with a fake IKeyboardHook/ITextInjector
│
├── .editorconfig
├── Directory.Build.props                   # shared <Nullable>, <LangVersion>, analyzers for every project
└── docs/
    └── SINHALA-INPUT-TOOL-DESIGN.md         # this file
```

**Why this split matters:** `SinhalaInput.Core` has zero dependency on Windows, WPF, or P/Invoke, so it is trivially unit-testable and reusable if you later add a TSF front end (Option A) or a mobile/web version — only the platform and app layers change.

---

## 7. Core engine — illustrative C# implementation

The following follows Microsoft's [.NET / C# coding conventions](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions) and [Framework Design Guidelines](https://learn.microsoft.com/dotnet/standard/design-guidelines/): `Nullable` enabled, file-scoped namespaces, `internal` by default with a deliberate public surface, immutable data via `readonly record struct`/`record`, dependency inversion via interfaces, no magic strings, and XML doc comments only on the public API.

```csharp
// SyllableToken.cs
namespace SinhalaInput.Core.Transliteration;

/// <summary>Classifies how a matched Latin token should be rendered in Sinhala.</summary>
public enum TokenKind
{
    Consonant,
    IndependentVowel,
    DependentVowelSign,
    ConjunctMarker,
}

/// <summary>An immutable rule mapping a Latin pattern to its Sinhala rendering.</summary>
public readonly record struct SyllableRule(
    string Latin,
    TokenKind Kind,
    string Glyph);
```

```csharp
// RuleTable.cs
namespace SinhalaInput.Core.Transliteration;

/// <summary>
/// The Latin-to-Sinhala phonetic rule set (see docs/SINHALA-INPUT-TOOL-DESIGN.md §3.2).
/// Longer Latin keys must be checked before their shorter prefixes; <see cref="RuleTrie"/>
/// guarantees that ordering, so the order of this list is not itself significant.
/// </summary>
public static class RuleTable
{
    public static readonly IReadOnlyList<SyllableRule> IndependentVowels =
    [
        new("a", TokenKind.IndependentVowel, "අ"),
        new("aa", TokenKind.IndependentVowel, "ආ"),
        new("A", TokenKind.IndependentVowel, "ආ"),
        new("ae", TokenKind.IndependentVowel, "ඇ"),
        new("aae", TokenKind.IndependentVowel, "ඈ"),
        new("i", TokenKind.IndependentVowel, "ඉ"),
        new("ii", TokenKind.IndependentVowel, "ඊ"),
        new("I", TokenKind.IndependentVowel, "ඊ"),
        new("u", TokenKind.IndependentVowel, "උ"),
        new("uu", TokenKind.IndependentVowel, "ඌ"),
        new("U", TokenKind.IndependentVowel, "ඌ"),
        new("e", TokenKind.IndependentVowel, "එ"),
        new("ee", TokenKind.IndependentVowel, "ඒ"),
        new("E", TokenKind.IndependentVowel, "ඒ"),
        new("ai", TokenKind.IndependentVowel, "ඓ"),
        new("o", TokenKind.IndependentVowel, "ඔ"),
        new("oo", TokenKind.IndependentVowel, "ඕ"),
        new("O", TokenKind.IndependentVowel, "ඕ"),
        new("au", TokenKind.IndependentVowel, "ඖ"),
    ];

    public static readonly IReadOnlyList<SyllableRule> DependentVowelSigns =
    [
        // "a" is intentionally absent: the inherent vowel needs no sign.
        new("aa", TokenKind.DependentVowelSign, "ා"), // ා
        new("A", TokenKind.DependentVowelSign, "ා"),
        new("ae", TokenKind.DependentVowelSign, "ැ"), // ැ
        new("aae", TokenKind.DependentVowelSign, "ෑ"), // ෑ
        new("i", TokenKind.DependentVowelSign, "ි"), // ි
        new("ii", TokenKind.DependentVowelSign, "ී"), // ී
        new("I", TokenKind.DependentVowelSign, "ී"),
        new("u", TokenKind.DependentVowelSign, "ු"), // ු
        new("uu", TokenKind.DependentVowelSign, "ූ"), // ූ
        new("U", TokenKind.DependentVowelSign, "ූ"),
        new("e", TokenKind.DependentVowelSign, "ෙ"), // ෙ
        new("ee", TokenKind.DependentVowelSign, "ේ"), // ේ
        new("E", TokenKind.DependentVowelSign, "ේ"),
        new("ai", TokenKind.DependentVowelSign, "ෛ"), // ෛ
        new("o", TokenKind.DependentVowelSign, "ො"), // ො
        new("oo", TokenKind.DependentVowelSign, "ෝ"), // ෝ
        new("O", TokenKind.DependentVowelSign, "ෝ"),
        new("au", TokenKind.DependentVowelSign, "ෞ"), // ෞ
    ];

    public static readonly IReadOnlyList<SyllableRule> Consonants =
    [
        new("k", TokenKind.Consonant, "ක"),
        new("kh", TokenKind.Consonant, "ඛ"),
        new("g", TokenKind.Consonant, "ග"),
        new("gh", TokenKind.Consonant, "ඝ"),
        new("ng", TokenKind.Consonant, "ඞ"),
        new("ch", TokenKind.Consonant, "ච"),
        new("c", TokenKind.Consonant, "ච"),
        new("chh", TokenKind.Consonant, "ඡ"),
        new("j", TokenKind.Consonant, "ජ"),
        new("jh", TokenKind.Consonant, "ඣ"),
        new("ny", TokenKind.Consonant, "ඤ"),
        new("T", TokenKind.Consonant, "ට"),
        new("tt", TokenKind.Consonant, "ට"),
        new("t", TokenKind.Consonant, "ට"),
        new("th", TokenKind.Consonant, "ත"),
        new("d", TokenKind.Consonant, "ද"),
        new("dh", TokenKind.Consonant, "ධ"),
        new("D", TokenKind.Consonant, "ඩ"),
        new("dd", TokenKind.Consonant, "ඩ"),
        new("N", TokenKind.Consonant, "ණ"),
        new("nn", TokenKind.Consonant, "ණ"),
        new("n", TokenKind.Consonant, "න"),
        new("p", TokenKind.Consonant, "ප"),
        new("ph", TokenKind.Consonant, "ඵ"),
        new("f", TokenKind.Consonant, "ෆ"),
        new("b", TokenKind.Consonant, "බ"),
        new("bh", TokenKind.Consonant, "භ"),
        new("m", TokenKind.Consonant, "ම"),
        new("y", TokenKind.Consonant, "ය"),
        new("r", TokenKind.Consonant, "ර"),
        new("l", TokenKind.Consonant, "ල"),
        new("L", TokenKind.Consonant, "ළ"),
        new("ll", TokenKind.Consonant, "ළ"),
        new("v", TokenKind.Consonant, "ව"),
        new("w", TokenKind.Consonant, "ව"),
        new("Sh", TokenKind.Consonant, "ෂ"),
        new("ss", TokenKind.Consonant, "ෂ"),
        new("sh", TokenKind.Consonant, "ශ"),
        new("s", TokenKind.Consonant, "ස"),
        new("h", TokenKind.Consonant, "හ"),
    ];

    public const string Virama = "්";           // ් — SINHALA SIGN AL-LAKUNA
    public const string ZeroWidthJoiner = "‍";
    public const string RakaransayaTail = "ර";        // conjunct 'r' glyph used after virama+ZWJ
    public const string YansayaTail = "ය";            // conjunct 'y' glyph used after virama+ZWJ

    public static IEnumerable<SyllableRule> All =>
        Consonants.Concat(IndependentVowels).Concat(DependentVowelSigns);
}
```

```csharp
// RuleTrie.cs
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
```

```csharp
// ITransliterationEngine.cs
namespace SinhalaInput.Core.Transliteration;

public interface ITransliterationEngine
{
    /// <summary>Transliterates a single Latin word into Sinhala Unicode.</summary>
    string Transliterate(string latinWord);
}
```

```csharp
// TransliterationEngine.cs
namespace SinhalaInput.Core.Transliteration;

/// <summary>
/// Deterministic, syllable-oriented Latin-to-Sinhala transliteration engine.
/// See docs/SINHALA-INPUT-TOOL-DESIGN.md §3.3 for the algorithm this implements.
/// </summary>
public sealed class TransliterationEngine : ITransliterationEngine
{
    private readonly RuleTrie _consonants;
    private readonly RuleTrie _independentVowels;
    private readonly RuleTrie _dependentVowelSigns;

    public TransliterationEngine()
        : this(RuleTable.Consonants, RuleTable.IndependentVowels, RuleTable.DependentVowelSigns)
    {
    }

    // Internal constructor seam for unit tests that need a reduced rule set.
    internal TransliterationEngine(
        IEnumerable<SyllableRule> consonants,
        IEnumerable<SyllableRule> independentVowels,
        IEnumerable<SyllableRule> dependentVowelSigns)
    {
        _consonants = new RuleTrie(consonants);
        _independentVowels = new RuleTrie(independentVowels);
        _dependentVowelSigns = new RuleTrie(dependentVowelSigns);
    }

    public string Transliterate(string latinWord)
    {
        ArgumentNullException.ThrowIfNull(latinWord);
        if (latinWord.Length == 0)
        {
            return string.Empty;
        }

        var result = new StringBuilder(latinWord.Length * 2);
        string? pendingConsonantGlyph = null;
        int i = 0;

        while (i < latinWord.Length)
        {
            SyllableRule? consonantMatch = _consonants.FindLongestMatch(latinWord, i);
            SyllableRule? vowelMatch = pendingConsonantGlyph is not null
                ? _dependentVowelSigns.FindLongestMatch(latinWord, i)
                : _independentVowels.FindLongestMatch(latinWord, i);

            SyllableRule? chosen = PickLongerMatch(consonantMatch, vowelMatch);

            if (chosen is null)
            {
                FlushPendingConsonant(result, ref pendingConsonantGlyph);
                result.Append(latinWord[i]); // passthrough: punctuation, digits, unknown chars
                i++;
                continue;
            }

            switch (chosen.Value.Kind)
            {
                case TokenKind.Consonant:
                    FlushPendingConsonant(result, ref pendingConsonantGlyph); // consonant cluster -> virama
                    pendingConsonantGlyph = chosen.Value.Glyph;
                    break;

                case TokenKind.DependentVowelSign:
                    result.Append(pendingConsonantGlyph).Append(chosen.Value.Glyph);
                    pendingConsonantGlyph = null;
                    break;

                case TokenKind.IndependentVowel:
                    result.Append(chosen.Value.Glyph);
                    break;
            }

            i += chosen.Value.Latin.Length;
        }

        FlushPendingConsonant(result, ref pendingConsonantGlyph);
        return result.ToString();
    }

    private static void FlushPendingConsonant(StringBuilder result, ref string? pendingConsonantGlyph)
    {
        if (pendingConsonantGlyph is null)
        {
            return;
        }

        // A consonant with no vowel that follows is either word-final (keeps its inherent
        // vowel, matching Google's observed behaviour) or is about to be superseded by the
        // next consonant, which appends the virama itself — see the Consonant case above.
        result.Append(pendingConsonantGlyph);
        pendingConsonantGlyph = null;
    }

    private static SyllableRule? PickLongerMatch(SyllableRule? a, SyllableRule? b)
    {
        if (a is null) return b;
        if (b is null) return a;
        return a.Value.Latin.Length >= b.Value.Latin.Length ? a : b;
    }
}
```

> This listing intentionally omits the rakāraṃśaya/yansaya conjunct pass and the consonant-cluster virama insertion refinement (a cluster like `nda` in `chandra` needs `ChandraGlyph + Virama + DaGlyph`, not two bare consonants) — implement those as a second pass or as additional `TokenKind` cases once the base engine's golden tests (§9) are green. Keeping the first implementation to the core algorithm keeps it readable; layer complexity on top of passing tests rather than up front.

---

## 8. Windows integration layer

### 8.1 Keyboard hook

```csharp
// IKeyboardHook.cs
namespace SinhalaInput.Platform.Windows.Hooking;

public sealed record KeyEvent(int VirtualKeyCode, bool IsKeyDown, bool[] ModifierStates);

public interface IKeyboardHook : IDisposable
{
    /// <summary>Raised for every key event; set <see cref="KeyEventArgs.Handled"/> to suppress it.</summary>
    event EventHandler<KeyInterceptedEventArgs>? KeyIntercepted;

    void Start();
    void Stop();
}
```

Implement `LowLevelKeyboardHook` with `SetWindowsHookEx(WH_KEYBOARD_LL, ...)`, keeping the hook callback **as fast as possible** (Windows silently removes hooks that block too long) — do all transliteration/UI work by posting to the app's dispatcher, not inline in the hook procedure. Use .NET's `[LibraryImport]` source-generated P/Invoke (not the legacy `[DllImport]`) for `user32.dll`/`SetWindowsHookEx`/`SendInput`/`GetGUIThreadInfo`, per current [.NET interop guidance](https://learn.microsoft.com/dotnet/standard/native-interop/pinvoke-source-generation), and centralise every signature in one `NativeMethods` class so unsafe/native surface area is easy to audit.

### 8.2 Text injection

```csharp
namespace SinhalaInput.Platform.Windows.Input;

public interface ITextInjector
{
    /// <summary>Sends <paramref name="backspaceCount"/> backspaces, then types <paramref name="text"/>.</summary>
    void ReplaceTypedText(int backspaceCount, string text);
}
```

`SendInputTextInjector` builds an array of `INPUT` structs: `backspaceCount` key-down/up pairs for `VK_BACK`, followed by one key-down/up pair **per UTF-16 code unit** of `text` using `KEYEVENTF_UNICODE` (this is how Windows injects arbitrary Unicode, including Sinhala, regardless of the active physical keyboard layout — no custom `.klc` keyboard layout is required). Batch the whole replacement into a single `SendInput` call so it lands atomically from the target application's point of view.

### 8.3 Caret location (for the candidate popup)

Use `GetGUIThreadInfo` on the foreground window's thread to get `rcCaret`/`hwndCaret`, falling back to `GetCaretPos` + `ClientToScreen`, and as a further fallback (apps that don't expose a Win32 caret, e.g. Chromium-based apps such as Edge/Chrome and some UWP surfaces) UI Automation's `TextPattern.GetBoundingRectangles` on the focused automation element (falling back further still to that element's own bounding rectangle if the pattern reports no rectangles). As a last resort, if none of those three locate anything, anchor near the current mouse cursor (`GetCursorPos`, which always succeeds) rather than leave the candidate popup with no anchor at all. Wrap all four behind a single `ICaretLocator.TryGetCaretScreenPosition(out Point)` so the app layer never branches on which strategy worked.

---

## 9. Testing strategy

- **`SinhalaInput.Core.Tests`** — pure unit tests, no Windows dependency, run on any CI agent (including Linux, since Core has no P/Invoke). Drive `TransliterationEngine` from a data file:

  ```csharp
  [Theory]
  [InlineData("mama", "මම")]
  [InlineData("api", "අපි")]
  [InlineData("kohomada", "කොහොමද")]
  [InlineData("machang", "මචං")]
  [InlineData("ayubowan", "අයුබෝවන්")]
  public void Transliterate_ProducesExpectedSinhala(string latin, string expected)
  {
      var engine = new TransliterationEngine();
      Assert.Equal(expected, engine.Transliterate(latin));
  }
  ```

  Grow `WordList.csv` continuously as you discover real-world words the current rule set gets wrong — treat it as a regression net, the same way a spell-checker's dictionary is grown.

- **`SinhalaInput.App.Tests`** — test `TypingSessionController` (the §4 state machine) against **fakes** of `IKeyboardHook` and `ITextInjector` (never the real Win32 hook), asserting things like "typing `m a m a SPACE` results in exactly one `ReplaceTypedText(4, "මම")` call, plus the space passed through."

- **Manual/exploratory** — because SendInput's actual effect depends on the target application, keep a short manual smoke-test checklist (Notepad, Word, Chrome address bar, Chrome text field, VS Code) and re-run it before every release; this is not something a unit test can substitute for.

---

## 10. Cross-cutting concerns

- **Password fields:** best-effort detection via the focused control's window class (`Edit` with `ES_PASSWORD`) or the UI Automation `IsPasswordProperty`; when detected, disengage the hook's buffering for that field entirely and let keystrokes pass straight through. Document the limitation (not watertight for every custom control) rather than silently pretending it's solved.
- **Privacy:** the Latin buffer for the *current, uncommitted* word is the only text ever held in memory; never persist raw keystrokes to disk or send them anywhere. The optional user dictionary (§5) stores only *already-committed words*, opt-in, local-only (no telemetry) — call this out explicitly in a privacy note in the settings UI.
- **Performance:** the hook callback must return in low single-digit milliseconds; keep `TransliterationEngine.Transliterate` allocation-light (it already is — a single `StringBuilder`) and do all UI work (popup rendering) asynchronously off the hook thread.
- **Resource cleanup:** `LowLevelKeyboardHook` must implement `IDisposable` and unhook in `Dispose`/on process exit (including `SystemEvents.SessionEnding`) — a leaked global hook survives and misbehaves after your process should have exited.
- **Elevation:** a non-elevated `SinhalaInput.App` cannot inject text into an elevated window (Windows' UIPI blocks it) — document this as a known limitation rather than auto-elevating the whole tray app (which would be a much bigger, unwarranted privilege escalation for a typing helper).

---

## 11. Packaging & deployment

- Ship as an **MSIX package** (Windows 10 1809+/11 native format): gives clean install/uninstall, a Microsoft Store-compatible path if you ever want one, without touching `HKLM`. (An MSIX startup-task extension for auto-start-at-sign-in is a natural v1.1 addition — not implemented in the current manifest, since it's a behavior change and not purely packaging.)
- **Implemented in `packaging/`** (no Visual Studio / `.wapproj` required — this environment has only the `dotnet` SDK, so the pipeline is hand-built from `dotnet publish` + a hand-authored `AppxManifest.xml` + `makeappx`/`signtool`):
  - `packaging/AppxManifest.xml` — the package manifest (`Identity`, `Properties`, `Dependencies` targeting `Windows.Desktop` MinVersion 10.0.17763.0 (1809) through current Win11, `uap:VisualElements`, and the `rescap:Capability Name="runFullTrust"` a classic desktop-bridge WPF app needs).
  - `packaging/Assets/` — placeholder tile/logo PNGs (real artwork still needed before any public release).
  - `packaging/Build-MsixPackage.ps1` — publishes `SinhalaInput.App` for `win-x64`, stages the layout, locates `makeappx`/`signtool` (a real Windows Kits install if present, otherwise the `Microsoft.Windows.SDK.BuildTools` NuGet package — this is exactly what lets packaging work from a `dotnet`-only machine/CI agent with no Windows SDK or Visual Studio installed), packs `SinhalaInput.msix`, and optionally (`-Sign`) signs it with a local self-signed certificate.
  - `packaging/README.md` — how to run it, the self-signed-cert local-trust caveat, and what changes for real distribution (purchased/EV code-signing cert, or Store submission).
- Target framework: the app builds against **.NET 10** (see `src/SinhalaInput.App/SinhalaInput.App.csproj`; this repo has moved past the .NET 8 baseline stated at the top of this document). Published **self-contained** for `win-x64` by default (bundles its own runtime, ~170MB unpacked / ~72MB packed) rather than framework-dependent (a few MB, but requires the matching `Microsoft.WindowsDesktop.App` runtime already present) — appropriate for a consumer tool that shouldn't fail to run because .NET isn't installed. `Build-MsixPackage.ps1 -SelfContained:$false` produces the smaller framework-dependent build when that tradeoff is preferred (e.g. quick internal testing on a machine that already has the SDK). See the script's parameter comments and `packaging/README.md` for the full tradeoff discussion.
- Code-sign the package (even a self-signed cert for internal use, a real cert for public distribution) — unsigned MSIX won't install by double-click on a clean machine. `Build-MsixPackage.ps1 -Sign` creates/reuses a self-signed cert and signs the package; installing it elsewhere still requires trusting that certificate first (`packaging/README.md`), and real public distribution needs a purchased/EV code-signing certificate or a Microsoft Store-issued identity instead.
- Tray icon + `NotifyIcon` (WinForms interop is fine to use *just* for `NotifyIcon` inside an otherwise-WPF app; there is no WPF-native tray icon API) with a context menu: Enable/Disable, Settings, Exit.

---

## 12. Roadmap

1. **v1** — Option B architecture, core rule set (§3.2), space/punctuation/Enter commit, backspace-edits-buffer, candidate popup with numbered alternates, tray toggle, MSIX package.
2. **v1.1** — user dictionary/learning (§5), settings UI (hotkey remap, enable/disable per-app), consonant-cluster virama refinement and rakāraṃśaya/yansaya conjuncts (§7 note).
3. **v2** — evaluate a real TSF text service (Option A) once v1's engine and rule set are battle-tested, so the risky COM/native work is isolated from the (by-then trusted) linguistic logic in `SinhalaInput.Core`.

---

## 13. References

- [Sinhala (Unicode block)](https://en.wikipedia.org/wiki/Sinhala_(Unicode_block)) — canonical code point chart; verify all hex constants against this before shipping.
- [Sinhala script — Wikipedia](https://en.wikipedia.org/wiki/Sinhala_script) — background on the abugida structure (inherent vowel, virama, conjuncts) this design relies on.
- [remeinium/singlish](https://github.com/remeinium/singlish) — a modern deterministic Singlish transliteration engine confirming the longest-match/trie approach and the capitalisation convention for retroflex consonants.
- [bhagyas/sinhala-utils](https://github.com/bhagyas/sinhala-utils) — established Java library with phonetic Singlish→Unicode support.
- Silva & Ahangama, *"Singlish to Sinhala Transliteration using Rule-based Approach"*, IEEE ICIIS 2021 — academic treatment of the same rule-based approach.
- [.NET / C# Coding Conventions — Microsoft Learn](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [.NET Framework Design Guidelines — Microsoft Learn](https://learn.microsoft.com/dotnet/standard/design-guidelines/)
- [.NET native interop / source-generated P/Invoke — Microsoft Learn](https://learn.microsoft.com/dotnet/standard/native-interop/pinvoke-source-generation)
- [Text Services Framework — Microsoft Learn](https://learn.microsoft.com/windows/win32/tsf/text-services-framework) — reference for the v2 (Option A) path.
