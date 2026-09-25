# Lankadeepa round trip: consolidated checklist

Corpus: 10 articles from lankadeepa.lk (`business-1/2`, `feature-1/2`, `latest-1/2`, `news-1/2`,
`sport-1/2`), 1262 tokens. Each `.tsv` row pairs a published Sinhala word with its Singlish spelling.
Test: `tests/SinhalaInput.Core.Tests/CorpusRoundTripTests.cs` checks that every non-SKIP row gives
`TransliterationEngine.Transliterate(singlish) == sinhala`, codepoint for codepoint. Run it with
`dotnet test --filter FullyQualifiedName~CorpusRoundTripTests`.

| | Tested rows | Pass | Pass rate |
|---|---:|---:|---:|
| **Start** (Round-1 PRs #9–#15 merged, before Round-2 fixes) | 1231 | 1200 | **97.5%** |
| **Final** | 1232 | 1232 | **100.0%** |

SKIP rows: 30, all Latin words or bare numbers (`2019`, `Smart`, `4,570` ...) that pass through
unchanged. The tested count grows by one because `25000ක්` is now tested (see H).

At the start, the 243 rows marked `NEEDS-U1`, `NEEDS-U2` or `NEEDS-U1+U2` already passed once PR #10
(word-final hal) and PR #9 (additional letters) were merged. The 31 rows that still failed were the
28 rows marked `BUG` plus 3 rows that Round 1 had wrongly marked `PASS` (feature-1:20 and
feature-2:53 and 128, all under B). They are listed below by cause. Every row's `status` column is now `PASS`, and its `note` records what it was and
how it was fixed. The per-article checklists in the `<section>-<n>.md` files are the Round-1 triage
and are superseded by this file.

## A. ථ (U+0DAE, dental aspirate) could not be typed: 12 rows

- [x] Engine: added `thh` → ථ, mirroring `ch`/`chh` (ත්හ, the cluster it replaces, never occurs).
      Covered by `CorpusDrivenRuleTests.Transliterate_DentalAspirateTha`.
- [x] Corpus: respelled `th` → `thh` in these rows:
  - [x] business-1.tsv:130 ප්‍රථම `prathhama`
  - [x] business-2.tsv:8 ආර්ථික `aarthhika`
  - [x] business-2.tsv:9 ස්ථාවරත්වය `sthhaavarathvaya`
  - [x] business-2.tsv:104 අවස්ථා `avasthhaa`
  - [x] feature-2.tsv:125 මධ්‍යස්ථාන `madhyasthhaana`
  - [x] latest-1.tsv:88 ස්ථාපිත `sthhaapitha`
  - [x] latest-1.tsv:102 ආර්ථික `aarthhika`
  - [x] latest-2.tsv:11 ව්‍යවස්ථා `vyavasthhaa`
  - [x] news-2.tsv:101 අවස්ථාවේදී `avasthhaaveedii`
  - [x] sport-2.tsv:8 ස්ථානයට `sthhaanayaTa`
  - [x] sport-2.tsv:22 ස්ථානය `sthhaanaya`
  - [x] sport-2.tsv:59 අවස්ථාව `avasthhaava`

## B. The engine added a yansaya ZWJ after ර (ර්‍ය instead of ර්ය): 7 rows

- [x] Engine: `r` followed by `y` + vowel is now an ordinary cluster (ර + ් + ය, no ZWJ). This is the
      standard spelling, with the ර in repaya form. Every other head consonant keeps the ZWJ yansaya
      (සාමාන්‍ය, ව්‍යාපාරය). Covered by `Transliterate_RaPlusYaHasNoYansaya`. The Singlish rows
      needed no change:
  - [x] business-1.tsv:92 කාර්ය `kaarya`
  - [x] business-2.tsv:26 ආචාර්ය `aachaarya`
  - [x] feature-1.tsv:20 මහාචාර්ය `mahaacaarya`
  - [x] feature-2.tsv:53 අනිවාර්ය `anivaarya`
  - [x] feature-2.tsv:128 අනිවාර්යයෙන්ම `anivaaryayenma`
  - [x] latest-1.tsv:34 ආචාර්ය `aachaarya`
  - [x] latest-1.tsv:99 කාර්යසාධක `kaaryasaadhaka`

## C. `ng` was always read as ඞ, so n + g could not be typed: 6 rows

- [x] Engine: ඞ moved to `nG` (and ඤ to `nY`, see D). `n` + `g` is now the ordinary cluster න්ග.
      Existing unit tests were updated. Covered by
      `Transliterate_NPlusGOrYIsAnOrdinaryCluster`. The Singlish rows needed no change:
  - [x] feature-2.tsv:8 බල්ලන්ගේ `ballangee`
  - [x] feature-2.tsv:10 කොල්ලන්ගේ `kollangee`
  - [x] feature-2.tsv:147 ලෙන්ගතුම `lengathuma`
  - [x] news-1.tsv:77 තමන්ගේ `thamangee`
  - [x] news-2.tsv:75 වෙන්ගප්පුලි `vengappuli`
  - [x] news-2.tsv:77 මහත්වරුන්ගේ `mahathvarungee`

## D. `ny` was always read as ඤ, so n + y could not be typed: 1 row (plus every න්‍ය word)

- [x] Engine: ඤ moved to `nY`, so `saamaanya` → සාමාන්‍ය and `nyaaya` → න්‍යාය. `jny` → ඥ is kept.
  - [x] feature-1.tsv:49 අවසන්ය also needs E, because the word has no yansaya.

## E. The automatic yansaya fired where the word has a plain hal + ය: 3 rows

කල්යාම (hal + ය) and කල්‍යාණ (yansaya) are both real words, so no rule can choose between them
without a dictionary.

- [x] Engine: added `q` as a syllable break. It emits nothing and ends the current syllable the way a
      passthrough character does: a pending consonant takes the virama and the next letter starts
      fresh. The typing session buffers only letters, and `q` has no Sinhala reading. Covered by
      `Transliterate_SyllableBreakEndsTheSyllableAndEmitsNothing`.
- [x] Corpus: respelled with `q`:
  - [x] news-1.tsv:64 කල්යාම `kalqyaama`
  - [x] news-2.tsv:126 බවත්ය `bavathqya`
  - [x] feature-1.tsv:49 අවසන්ය `avasanqya`

## F. A vowel after an inherent `a` was joined into a diphthong sign: 1 row

- [x] `vagau...` read `au` as the ෞ sign on ග. The compound boundary is now marked with `q`:
  - [x] news-2.tsv:88 වගඋත්තරකරු `vagaquththarakaru`

## G. Stray ZWJ in the source HTML (source-data issue, not an engine bug): 2 rows

The published text has U+200D after a vowel sign (ු‍ම), where it joins nothing, renders the same
as the word without it, and cannot be typed. This is CMS or copy-paste noise on lankadeepa.lk.

- [x] Harness: `CorpusRoundTripTests.NormalizeExpected` drops any ZWJ that does not directly follow a
      virama (U+0DCA) from the expected text. Real conjunct ZWJs are kept. The engine is unchanged.
  - [x] business-1.tsv:4 ඇඟලු‍ම් `aezgalum`
  - [x] business-1.tsv:7 ඉල්ලු‍ම `illuma`

## H. Mixed digits + Sinhala token was skipped: 1 row

- [x] Digits pass through and end the syllable, so `25000k` → 25000ක්. The row is now tested.
  - [x] news-2.tsv:20 25000ක් `25000k`

## Open items

None. Every Sinhala word in the 10 articles can be typed, and no test case is skipped.
