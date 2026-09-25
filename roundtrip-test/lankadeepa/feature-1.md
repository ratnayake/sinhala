# Lankadeepa round-trip test — feature-1 (Mathawada / opinion)

**Source:** https://www.lankadeepa.lk/mathawada/%E0%B6%B4%E0%B7%83%E0%B6%9A%E0%B7%8A-%E0%B6%B4%E0%B6%BB%E0%B6%BA-%E0%B7%83%E0%B7%84-%E0%B6%8A%E0%B6%A7-%E0%B6%85%E0%B6%AF%E0%B7%85-%E0%B6%B1%E0%B7%92%E0%B6%AD%E0%B7%92%E0%B6%B8%E0%B6%BA-%E0%B6%9A%E0%B6%BB%E0%B7%92%E0%B6%BA%E0%B6%B8%E0%B7%8F%E0%B6%BB%E0%B7%8A%E0%B6%9C/317-698228
("පාස්කු ප්‍රහාරය සහ ඊට අදාළ නීතිමය ක්‍රියාමාර්ග" — "The Easter attack and the related legal proceedings", Mathawada/opinion section, an analysis piece by Prof. Prathibha Mahanamahewa)

**Fetched:** 2026-09-25, via PowerShell `Invoke-WebRequest -UseBasicParsing` decoded as UTF-8 (not WebFetch), extracting the `<div class="article-body sinhala-body">` block and HTML-decoding entities (the source HTML itself already encodes ZWJ conjuncts as literal `&zwj;` entities, e.g. `ත්&zwj;රස්ත`).

**Text used** (title + first ~204 words of the body, verbatim, as published):

> පාස්කු ප්‍රහාරය සහ ඊට අදාළ නීතිමය ක්‍රියාමාර්ග
>
> ​ත්‍රස්ත ප්‍රහාරය සම්බන්ධයෙන් මෙතෙක් විභාග වූ නඩු, ලබා දුන් තීන්දු සහ වර්තමාන නීතිමය තත්ත්වය පිළිබඳ මේ අදහස් දැක්වීම මහාචාර්ය ප්‍රතිභා මහානාමහේවා.
>
> 2019 පාස්කු ඉරුදින එල්ල වූ ම්ලේච්ඡ ත්‍රස්ත ප්‍රහාර මාලාව සම්බන්ධයෙන් මේ වන විට අපරාධ නඩු, මූලික අයිතිවාසිකම් නඩු සහ විමර්ශන මට්ටමේ පවතින ක්‍රියාමාර්ග කිහිපයක්ම ක්‍රියාත්මක වී ඇත. ඉන් ප්‍රධාන නඩු කිහිපයක තීන්දු මේ වන විටත් ප්‍රකාශයට පත් කර අවසන්ය.
>
> ​ත්‍රිපුද්ගල මහාධිකරණ විනිසුරු මඩුල්ල සහ අභියාචනා නීතිය
>
> ​පාස්කු ප්‍රහාරයට අදාළ ප්‍රධාන අපරාධ නඩු විභාග කරනු ලැබුවේ විශේෂ ත්‍රිපුද්ගල මහාධිකරණ විනිසුරු මඩුලු (Trial-at-Bar) හමුවේය. 1978 අංක 2 දරන අධිකරණ සංවිධාන පනතේ සංශෝධන අනුව, මෙවැනි ත්‍රිපුද්ගල මහාධිකරණ තීන්දුවකට එරෙහිව අභියාචනාධිකරණය මඟහැර කෙලින්ම ශ්‍රේෂ්ඨාධිකරණයට දින 28ක් ඇතුළත අභියාචනා ඉදිරිපත් කිරීමේ නීතිමය හිමිකම විත්තිකරුවන්ට හිමි වේ.
>
> ​එවැනි අභියාචනයකදී ශ්‍රේෂ්ඨාධිකරණය නැවත සාක්ෂි විභාග කිරීමක් සිදු නොකරන අතර, මුල් නඩු වාර්තාවේ අඩංගු නීතිමය හෝ කරුණුමය දෝෂ පදනම් කරගනිමින් නීතිඥ තර්ක සලකා බලා පෙර තීන්දුව තහවුරු කිරීම හෝ සංශෝධනය කිරීම සිදු කරයි.
>
> ​අපරාධමය නොසැලකිල්ල පිළිබඳ නඩුව සහ තීන්දුව
>
> ​ප්‍රහාරය පිළිබඳ පූර්ව බුද්ධි තොරතුරු ලැබී තිබියදීත් එය වළක්වා ගැනීමට ක්‍රියා නොකිරීම (අපරාධමය නොසැලකිල්ල) සම්බන්ධයෙන් හිටපු පොලිස්පති පූජිත ජයසුන්දර සහ හිටපු ආරක්ෂක ලේකම් හේමසිරි ප්‍රනාන්දු යන මහත්වරුන්ට එරෙහිව විශේෂ ත්‍රිපුද්ගල මහාධිකරණය හමුවේ නඩු විභාග විය. අනතුර දැන සිටියදීත් එය වළක්වා නොගැනීමෙන් මනුෂ්‍ය ඝාතන සිදුවීමට ඉඩහැරීම යන චෝදනා මත ඔවුන්ට මරණ දඬුවම නියම කෙරිණි.

## Method

Every distinct Sinhala word (152) plus Latin/digit tokens (7: `Trial`, `at`, `Bar`, `2019`, `1978`, `2`, `28`) was spelled in the target Singlish scheme (see `src/SinhalaInput.Core/Transliteration/RuleTable.cs`), built once, then piped through the CLI in a single run and compared codepoint-for-codepoint against the original (BOM on the first output line stripped).

## Status summary (159 rows)

> Round 2: every row now passes (see [CHECKLIST.md](CHECKLIST.md)). The counts below are the Round-1 triage.

| Status | Count |
|---|---|
| PASS | 127 |
| NEEDS-U1 | 18 |
| NEEDS-U2 | 6 |
| BUG | 1 |
| SKIP | 7 |

## BUG checklist

- [x] අවසන්ය (avasanya, "concluded") — typed as `avasanya` (a-v-a-s-a-n-y-a), which should produce ස+න්(hal)+ය (plain "n" consonant with virama, directly followed by "ය"; the source word has **no** ZWJ, so this is not the ක්‍ර/ව්‍ය conjunct case). Instead the engine's trie greedily matches the 2-letter substring "ny" as its own consonant token (ඤ) before it ever considers "n" as a standalone consonant needing a virama, so the CLI outputs අවසඤ instead of අවසන්ය. The same defect (with "ng"→ඞ instead of hal-n + ග) also breaks 3 words in *feature-2* (බල්ලන්ගේ, කොල්ලන්ගේ, ලෙන්ගතුම) — see that file for the shared diagnosis. There is no documented escape in the target scheme to type a bare "n" immediately before "y" or "g" without it collapsing into the ny/ng digraph. *(Round 2: fixed, see [CHECKLIST.md](CHECKLIST.md).)*
